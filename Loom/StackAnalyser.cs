using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TStack = Microsoft.FSharp.Collections.FSharpList<System.Tuple<Mono.Cecil.Cil.Instruction, Silk.Loom.StackAnalyser.StackEntry>>;

namespace Silk.Loom
{
    public class StackAnalyser
    {
        public static Instruction ReplaceInstruction(ILProcessor ilProcessor, Instruction target, Instruction instruction)
        {
            var body = ilProcessor.Body;

            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == target) { handler.TryStart = instruction; }
                if (handler.TryEnd == target) { handler.TryEnd = instruction; }
                if (handler.HandlerStart == target) { handler.HandlerStart = instruction; }
                if (handler.HandlerEnd == target) { handler.HandlerEnd = instruction; }
                if (handler.FilterStart == target) { handler.FilterStart = instruction; }
            }

            foreach (var jmp in body.Instructions)
            {
                if (jmp.OpCode.OperandType == OperandType.InlineBrTarget)
                {
                    var jmp_target = jmp.Operand as Instruction;
                    if (jmp_target == target)
                    {
                        jmp.Operand = instruction;
                    }
                }
            }

            ilProcessor.Replace(target, instruction);

            return instruction;
        }

        public struct StackEntry
        {
            public bool IsConstant { get; private set; }

            private TypeReference _Type;
            private MethodBody _Method;
            private ModuleDefinition _Module;
            public TypeReference Type
            {
                get
                {
                    if (_Type == null)
                    {
                        if (Value == null)
                        {
                            _Type = References.FindType(_Module, _Method, "System.Object");
                        }
                        else
                        {
                            _Type = References.FindType(_Module, _Method, Value.GetType().FullName);
                        }
                    }

                    return _Type;
                }
            }

            public dynamic Value { get; private set; }

            internal StackEntry(TypeReference type) 
                : this()
            {
                _Type = type;
                _Module = null;
                _Method = null;
                IsConstant = false;
                Value = null;
            }

            internal StackEntry(Mono.Cecil.ModuleDefinition module, MethodBody method, object value)
                : this()
            {
                _Type = null;
                _Module = module;
                _Method = method;
                IsConstant = true;
                Value = value;
            }
        }

        public static Instruction RemoveInstructionChain(MethodDefinition method, Instruction instruction, Dictionary<Instruction, TStack> analysis)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var nop = Instruction.Create(OpCodes.Nop);

            if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Pop0)
            {
                ReplaceInstruction(ilProcessor, instruction, nop);
            }
            else
            {
                var stack = analysis[instruction.Previous];
                ReplaceInstruction(ilProcessor, instruction, nop);

                switch (instruction.OpCode.StackBehaviourPop)
                {
                    case StackBehaviour.Pop1:
                    case StackBehaviour.Popi:
                    case StackBehaviour.Popref:
                        RemoveInstructionChain(method, stack.Head.Item1, analysis);
                        break;
                    case StackBehaviour.Pop1_pop1:
                    case StackBehaviour.Popi_pop1:
                    case StackBehaviour.Popi_popi:
                    case StackBehaviour.Popi_popi8:
                    case StackBehaviour.Popi_popr4:
                    case StackBehaviour.Popi_popr8:
                    case StackBehaviour.Popref_pop1:
                    case StackBehaviour.Popref_popi:
                        RemoveInstructionChain(method, stack.Head.Item1, analysis);
                        RemoveInstructionChain(method, stack.Tail.Head.Item1, analysis);
                        break;
                    case StackBehaviour.Popi_popi_popi:
                    case StackBehaviour.Popref_popi_popi:
                    case StackBehaviour.Popref_popi_popi8:
                    case StackBehaviour.Popref_popi_popr4:
                    case StackBehaviour.Popref_popi_popr8:
                    case StackBehaviour.Popref_popi_popref:
                        RemoveInstructionChain(method, stack.Head.Item1, analysis);
                        RemoveInstructionChain(method, stack.Tail.Head.Item1, analysis);
                        RemoveInstructionChain(method, stack.Tail.Tail.Head.Item1, analysis);
                        break;
                    case StackBehaviour.PopAll:
                        {
                            foreach (var item in stack)
                            {
                                RemoveInstructionChain(method, item.Item1, analysis);
                            }
                        } break;
                    case StackBehaviour.Varpop:
                        {
                            if (instruction.OpCode.OperandType == OperandType.InlineMethod)
                            {
                                var m = instruction.Operand as MethodReference;
                                for (int i = 0; i < m.Parameters.Count; ++i)
                                {
                                    RemoveInstructionChain(method, stack.Head.Item1, analysis);
                                    stack = stack.Tail;
                                }
                                if (m.HasThis)
                                {
                                    RemoveInstructionChain(method, stack.Head.Item1, analysis);
                                }
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        } break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return nop;
        }

        private static Tuple<Instruction, StackEntry> Pop(ref TStack stack)
        {
            var value = stack.Head;
            stack = stack.Tail;
            return value;
        }

        private TypeReference _SystemObject;
        private TypeReference _SystemBoolean;
        private TypeReference _SystemIntPtr;
        private TypeReference _SystemRuntimeMethodHandle;
        private TypeReference _SystemRuntimeTypeHandle;
        private TypeReference _SystemRuntimeFieldHandle;

        public StackAnalyser(ModuleDefinition module)
        {
            _SystemObject = References.FindType(module, null, "System.Object");
            _SystemObject = References.FindType(module, null, "System.Boolean");
            _SystemIntPtr = References.FindType(module, null, "System.IntPtr");
            _SystemRuntimeMethodHandle = References.FindType(module, null, "System.RuntimeMethodHandle");
            _SystemRuntimeTypeHandle = References.FindType(module, null , "System.RuntimeTypeHandle");
            _SystemRuntimeFieldHandle = References.FindType(module, null , "System.RuntimeFieldHandle");
        }

        public Dictionary<Instruction, TStack> Analyse(MethodDefinition method)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var instructions = ilProcessor.Body.Instructions;
            var module = method.Module;

            var map = new Dictionary<Instruction, TStack>();
            for (int i = 0; i < instructions.Count; ++i)
            {
                var instruction = instructions[i];
                var previous_instruction = instruction.Previous;

                TStack stack = TStack.Empty;
                if (previous_instruction != null)
                {
                    stack = map[previous_instruction];
                }

                switch (instruction.OpCode.Code)
                {
                    case Code.Add:
                    case Code.Add_Ovf:
                    case Code.Add_Ovf_Un:
                    case Code.And:
                        {
                            var a = Pop(ref stack).Item2;
                            var b = Pop(ref stack).Item2;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Type)), stack);
                            break;
                        }
                    case Code.Beq:
                    case Code.Beq_S:
                    case Code.Bge:
                    case Code.Bge_S:
                    case Code.Bge_Un:
                    case Code.Bge_Un_S:
                    case Code.Bgt:
                    case Code.Bgt_S:
                    case Code.Bgt_Un:
                    case Code.Bgt_Un_S:
                    case Code.Ble:
                    case Code.Ble_S:
                    case Code.Blt:
                    case Code.Blt_S:
                    case Code.Blt_Un:
                    case Code.Blt_Un_S:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            break;
                        }
                    case Code.Box:
                        {
                            var a = Pop(ref stack).Item2;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemObject)), stack);
                            break;
                        }
                    case Code.Br:
                    case Code.Br_S:
                    case Code.Break:
                        break;
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        {
                            Pop(ref stack);
                            break;
                        }
                    case Code.Call:
                    case Code.Callvirt:
                        {
                            var m = instruction.Operand as MethodReference;
                            StackEntry[] args = new StackEntry[m.Parameters.Count];
                            for (int j = 0; j < m.Parameters.Count; ++j)
                            {
                                args[j] = Pop(ref stack).Item2;
                            }
                            Array.Reverse(args);

                            StackEntry self = default(StackEntry);
                            if(m.HasThis)
                            {
                                self = Pop(ref stack).Item2;
                            }
                            
                            stack = TStack.Cons(Tuple.Create(instruction, new StackEntry(m.ReturnType)), stack);
                            break;
                        }
                    case Code.Calli:
                    case Code.Castclass:
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.Ckfinite:
                    case Code.Clt:
                    case Code.Clt_Un:
                    case Code.Constrained:
                    case Code.Conv_I:
                    case Code.Conv_I1:
                    case Code.Conv_I2:
                    case Code.Conv_I4:
                    case Code.Conv_I8:
                    case Code.Conv_Ovf_I:
                    case Code.Conv_Ovf_I_Un:
                    case Code.Conv_Ovf_I1:
                    case Code.Conv_Ovf_I1_Un:
                    case Code.Conv_Ovf_I2:
                    case Code.Conv_Ovf_I2_Un:
                    case Code.Conv_Ovf_I4:
                    case Code.Conv_Ovf_I4_Un:
                    case Code.Conv_Ovf_I8:
                    case Code.Conv_Ovf_I8_Un:
                    case Code.Conv_Ovf_U:
                    case Code.Conv_Ovf_U_Un:
                    case Code.Conv_Ovf_U1:
                    case Code.Conv_Ovf_U1_Un:
                    case Code.Conv_Ovf_U2:
                    case Code.Conv_Ovf_U2_Un:
                    case Code.Conv_Ovf_U4:
                    case Code.Conv_Ovf_U4_Un:
                    case Code.Conv_Ovf_U8:
                    case Code.Conv_Ovf_U8_Un:
                    case Code.Conv_R_Un:
                    case Code.Conv_R4:
                    case Code.Conv_R8:
                    case Code.Conv_U:
                    case Code.Conv_U1:
                    case Code.Conv_U2:
                    case Code.Conv_U4:
                    case Code.Conv_U8:
                    case Code.Cpblk:
                    case Code.Cpobj:
                    case Code.Div:
                    case Code.Div_Un:
                    case Code.Dup:
                    case Code.Endfilter:
                    case Code.Endfinally:
                    case Code.Initblk:
                    case Code.Initobj:
                    case Code.Isinst:
                    case Code.Jmp:
                    case Code.Ldarg:
                    case Code.Ldarg_0:
                    case Code.Ldarg_1:
                    case Code.Ldarg_2:
                    case Code.Ldarg_3:
                    case Code.Ldarg_S:
                    case Code.Ldarga:
                    case Code.Ldarga_S:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Ldc_I4:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldc_I4_0:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 0)), stack);
                        break;
                    case Code.Ldc_I4_1:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 1)), stack);
                        break;
                    case Code.Ldc_I4_2:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 2)), stack);
                        break;
                    case Code.Ldc_I4_3:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 3)), stack);
                        break;
                    case Code.Ldc_I4_4:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 4)), stack);
                        break;
                    case Code.Ldc_I4_5:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 5)), stack);
                        break;
                    case Code.Ldc_I4_6:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 6)), stack);
                        break;
                    case Code.Ldc_I4_7:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 7)), stack);
                        break;
                    case Code.Ldc_I4_8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, 8)), stack);
                        break;
                    case Code.Ldc_I4_M1:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, -1)), stack);
                        break;
                    case Code.Ldc_I4_S:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldc_I8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldc_R4:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldc_R8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldelem_Any:
                    case Code.Ldelem_I:
                    case Code.Ldelem_I1:
                    case Code.Ldelem_I2:
                    case Code.Ldelem_I4:
                    case Code.Ldelem_I8:
                    case Code.Ldelem_R4:
                    case Code.Ldelem_R8:
                    case Code.Ldelem_Ref:
                    case Code.Ldelem_U1:
                    case Code.Ldelem_U2:
                    case Code.Ldelem_U4:
                    case Code.Ldelema:
                    case Code.Ldfld:
                    case Code.Ldflda:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Ldftn:
                        {
                            stack = new TStack(Tuple.Create(
                                instruction,
                                new StackEntry(_SystemIntPtr)), stack);
                            break;
                        }
                    case Code.Ldind_I:
                    case Code.Ldind_I1:
                    case Code.Ldind_I2:
                    case Code.Ldind_I4:
                    case Code.Ldind_I8:
                    case Code.Ldind_R4:
                    case Code.Ldind_R8:
                    case Code.Ldind_Ref:
                    case Code.Ldind_U1:
                    case Code.Ldind_U2:
                    case Code.Ldind_U4:
                    case Code.Ldlen:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_0:
                    case Code.Ldloc_1:
                    case Code.Ldloc_2:
                    case Code.Ldloc_3:
                    case Code.Ldloc_S:
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                        {
                            bool isAddress = instruction.OpCode.Code == Code.Ldloca || instruction.OpCode.Code == Code.Ldloca_S;
                            var loc = instruction.Operand as VariableDefinition;
                            var ty = isAddress ? Mono.Cecil.Rocks.TypeReferenceRocks.MakePointerType(loc.VariableType) : loc.VariableType;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                            break;
                        }
                    case Code.Ldnull:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, null)), stack);
                        break;
                    case Code.Ldobj:
                        {
                            var src = Pop(ref stack);
                            var type = instruction.Operand as TypeReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(type)), stack);
                        }
                        break;
                    case Code.Ldsfld:
                    case Code.Ldsflda:
                        {
                            var field = instruction.Operand as FieldReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(field.FieldType)), stack);
                            break;
                        }
                    case Code.Ldstr:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldtoken:
                        {
                            var token = instruction.Operand;

                            if (token is Mono.Cecil.MethodReference)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemRuntimeMethodHandle)), stack);
                            }
                            if (token is Mono.Cecil.TypeReference)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemRuntimeTypeHandle)), stack);
                            }
                            if (token is Mono.Cecil.FieldReference)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemRuntimeFieldHandle)), stack);
                            }
                            break;
                        }
                    case Code.Ldvirtftn:
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Localloc:
                    case Code.Mkrefany:
                    case Code.Mul:
                    case Code.Mul_Ovf:
                    case Code.Mul_Ovf_Un:
                    case Code.Neg:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Newarr:
                        {
                            var length = Pop(ref stack).Item2;
                            var type = (Mono.Cecil.TypeReference)instruction.Operand;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(Mono.Cecil.Rocks.TypeReferenceRocks.MakeArrayType(type))), stack);
                            break;
                        }
                    case Code.Newobj:
                        {
                            var ctor = instruction.Operand as MethodReference;
                            for (int j = 0; j < ctor.Parameters.Count; ++j)
                            {
                                Pop(ref stack);
                            }
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ctor.DeclaringType)), stack);
                            break;
                        }
                    case Code.No:
                        break;
                    case Code.Nop:
                        break;
                    case Code.Not:
                    case Code.Or:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Pop:
                        Pop(ref stack);
                        break;
                    case Code.Readonly:
                    case Code.Refanytype:
                    case Code.Refanyval:
                    case Code.Rem:
                    case Code.Rem_Un:
                    case Code.Ret:
                    case Code.Rethrow:
                    case Code.Shl:
                    case Code.Shr:
                    case Code.Shr_Un:
                    case Code.Sizeof:
                    case Code.Starg:
                    case Code.Starg_S:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Stelem_Any:
                    case Code.Stelem_I:
                    case Code.Stelem_I1:
                    case Code.Stelem_I2:
                    case Code.Stelem_I4:
                    case Code.Stelem_I8:
                    case Code.Stelem_R4:
                    case Code.Stelem_R8:
                    case Code.Stelem_Ref:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            Pop(ref stack);
                            break;
                        }
                    case Code.Stfld:
                    case Code.Stind_I:
                    case Code.Stind_I1:
                    case Code.Stind_I2:
                    case Code.Stind_I4:
                    case Code.Stind_I8:
                    case Code.Stind_R4:
                    case Code.Stind_R8:
                    case Code.Stind_Ref:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    case Code.Stloc:
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                    case Code.Stloc_S:
                        Pop(ref stack);
                        break;
                    case Code.Stobj:
                        Pop(ref stack);
                        Pop(ref stack);
                        break;
                    case Code.Stsfld:
                        Pop(ref stack);
                        break;
                    case Code.Sub:
                    case Code.Sub_Ovf:
                    case Code.Sub_Ovf_Un:
                    case Code.Switch:
                    case Code.Tail:
                    case Code.Throw:
                    case Code.Unaligned:
                    case Code.Unbox:
                    case Code.Unbox_Any:
                    case Code.Volatile:
                    case Code.Xor:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(null)), stack);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                map.Add(instruction, stack);
            }
            return map;
        }
    }
}
