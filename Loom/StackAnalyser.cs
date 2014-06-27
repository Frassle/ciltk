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
        private static void FixupReferences(ILProcessor ilProcessor, Instruction target, Instruction instruction)
        {
            var body = ilProcessor.Body;

            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart == target)
                {
                    handler.TryStart = instruction;
                }
                if (handler.TryEnd == target)
                {
                    handler.TryEnd = instruction;
                }
                if (handler.HandlerStart == target)
                {
                    handler.HandlerStart = instruction;
                }
                if (handler.HandlerEnd == target)
                {
                    handler.HandlerEnd = instruction;
                }
                if (handler.FilterStart == target)
                {
                    handler.FilterStart = instruction;
                }
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
        }

        public static Instruction ReplaceInstruction(ILProcessor ilProcessor, Instruction target, Instruction instruction)
        {
            FixupReferences(ilProcessor, target, instruction);
            ilProcessor.Replace(target, instruction);
            return instruction;
        }

        public static void RemoveInstruction(ILProcessor ilProcessor, Instruction instruction)
        {
            FixupReferences(ilProcessor, instruction, instruction.Next);
            ilProcessor.Remove(instruction);
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

        public static void RemoveInstructionChain(MethodDefinition method, Instruction instruction, Dictionary<Instruction, TStack> analysis)
        {
            var ilProcessor = method.Body.GetILProcessor();

            if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Pop0)
            {
                RemoveInstruction(ilProcessor, instruction);
            }
            else
            {
                var stack = analysis[instruction.Previous];
                RemoveInstruction(ilProcessor, instruction);

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
                        }
                        break;
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
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static Tuple<Instruction, StackEntry> Pop(ref TStack stack)
        {
            var value = stack.Head;
            stack = stack.Tail;
            return value;
        }

        private TypeReference _SystemObject;
        private TypeReference _SystemBoolean;
        private TypeReference _SystemSingle;
        private TypeReference _SystemDouble;
        private TypeReference _SystemSByte;
        private TypeReference _SystemByte;
        private TypeReference _SystemInt16;
        private TypeReference _SystemUInt16;
        private TypeReference _SystemInt32;
        private TypeReference _SystemUInt32;
        private TypeReference _SystemInt64;
        private TypeReference _SystemUInt64;
        private TypeReference _SystemIntPtr;
        private TypeReference _SystemUIntPtr;
        private TypeReference _SystemRuntimeMethodHandle;
        private TypeReference _SystemRuntimeTypeHandle;
        private TypeReference _SystemRuntimeFieldHandle;
        private TypeReference _SystemTypedReference;

        public StackAnalyser(ModuleDefinition module)
        {
            _SystemObject = References.FindType(module, null, "System.Object");
            _SystemBoolean = References.FindType(module, null, "System.Boolean");
            _SystemSingle = References.FindType(module, null, "System.Single");
            _SystemDouble = References.FindType(module, null, "System.Double");
            _SystemSByte = References.FindType(module, null, "System.SByte");
            _SystemByte = References.FindType(module, null, "System.Byte");
            _SystemInt16 = References.FindType(module, null, "System.Int16");
            _SystemUInt16 = References.FindType(module, null, "System.UInt16");
            _SystemInt32 = References.FindType(module, null, "System.Int32");
            _SystemUInt32 = References.FindType(module, null, "System.UInt32");
            _SystemInt64 = References.FindType(module, null, "System.Int64");
            _SystemUInt64 = References.FindType(module, null, "System.UInt64");
            _SystemIntPtr = References.FindType(module, null, "System.IntPtr");
            _SystemUIntPtr = References.FindType(module, null, "System.UIntPtr");
            _SystemRuntimeMethodHandle = References.FindType(module, null, "System.RuntimeMethodHandle");
            _SystemRuntimeTypeHandle = References.FindType(module, null, "System.RuntimeTypeHandle");
            _SystemRuntimeFieldHandle = References.FindType(module, null, "System.RuntimeFieldHandle");
            _SystemTypedReference = References.FindType(module, null, "System.TypedReference");
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
                // Simple binary operators
                    case Code.Add:
                    case Code.Add_Ovf:
                    case Code.Add_Ovf_Un:
                    case Code.And:
                    case Code.Div:
                    case Code.Div_Un:
                    case Code.Mul:
                    case Code.Mul_Ovf:
                    case Code.Mul_Ovf_Un:
                    case Code.Or:
                    case Code.Rem:
                    case Code.Rem_Un:
                    case Code.Sub:
                    case Code.Sub_Ovf:
                    case Code.Sub_Ovf_Un:
                    case Code.Xor:
                        {
                            var a = Pop(ref stack);
                            var b = Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Item2.Type)), stack);
                        }
                        break;
                // Simple unary operators
                    case Code.Neg:
                    case Code.Not:
                        {
                            var a = Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Item2.Type)), stack);
                        }
                        break;
                // Shift operators
                    case Code.Shl:
                    case Code.Shr:
                    case Code.Shr_Un:
                        {
                            var a = Pop(ref stack);
                            var b = Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(b.Item2.Type)), stack);
                        }
                        break;
                // Compare operators
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.Ckfinite:
                    case Code.Clt:
                    case Code.Clt_Un:
                        {
                            var a = Pop(ref stack);
                            var b = Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemBoolean)), stack);
                        }
                        break;
                // No push, pop3 instructions
                    case Code.Cpblk:
                    case Code.Initblk:
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
                        }
                        break;
                // No push, pop2 instructions
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
                    case Code.Cpobj:
                    case Code.Stfld:
                    case Code.Stind_I:
                    case Code.Stind_I1:
                    case Code.Stind_I2:
                    case Code.Stind_I4:
                    case Code.Stind_I8:
                    case Code.Stind_R4:
                    case Code.Stind_R8:
                    case Code.Stind_Ref:
                    case Code.Stobj:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                        }
                        break;
                // No push, pop1 instructions
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                    case Code.Initobj:
                    case Code.Pop:
                    case Code.Starg:
                    case Code.Starg_S:
                    case Code.Stloc:
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                    case Code.Stloc_S:
                    case Code.Stsfld:
                    case Code.Switch:
                    case Code.Throw:
                        {
                            Pop(ref stack);
                        }
                        break;
                // No push, no pop instructions
                    case Code.Br:
                    case Code.Br_S:
                    case Code.Break:
                    case Code.Constrained:
                    case Code.Endfilter:
                    case Code.Endfinally:
                    case Code.Jmp:
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.No:
                    case Code.Nop:
                    case Code.Readonly:
                    case Code.Ret:
                    case Code.Rethrow:
                    case Code.Tail:
                    case Code.Unaligned:
                    case Code.Volatile:
                        break;
                //duplicate
                    case Code.Dup:
                        {
                            stack = new TStack(Tuple.Create(instruction, stack.Head.Item2), stack);
                        }
                        break;
                // box / unbox
                    case Code.Box:
                        {
                            var a = Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemObject)), stack);
                        }
                        break;
                    case Code.Unbox:
                    case Code.Unbox_Any:
                        {
                            var a = Pop(ref stack);
                            var ty = instruction.Operand as TypeReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                        }
                        break;
                // Call instructions
                    case Code.Call:
                    case Code.Callvirt:
                        {
                            var m = instruction.Operand as MethodReference;
                            for (int j = 0; j < m.Parameters.Count; ++j)
                            {
                                Pop(ref stack);
                            }

                            if (m.HasThis)
                            {
                                Pop(ref stack);
                            }
                            
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(m.ReturnType)), stack);                        
                        }    
                        break;
                    case Code.Calli:
                        {
                            var s = instruction.Operand as CallSite;
                            for (int j = 0; j < s.Parameters.Count; ++j)
                            {
                                Pop(ref stack);
                            }

                            if (s.HasThis)
                            {
                                Pop(ref stack);
                            }

                            stack = new TStack(Tuple.Create(instruction, new StackEntry(s.ReturnType)), stack);
                        }
                        break;
                // Constant loading functions
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
                    case Code.Ldstr:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, instruction.Operand)), stack);
                        break;
                    case Code.Ldnull:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, method.Body, null)), stack);
                        break;
                // Argument loading instructions
                    case Code.Ldarg:
                    case Code.Ldarg_0:
                    case Code.Ldarg_1:
                    case Code.Ldarg_2:
                    case Code.Ldarg_3:
                    case Code.Ldarg_S:
                    case Code.Ldarga:
                    case Code.Ldarga_S:
                        {
                            bool isAddress = instruction.OpCode.Code == Code.Ldarga || instruction.OpCode.Code == Code.Ldarga_S;
                            var loc = instruction.Operand as ParameterDefinition;
                            var ty = isAddress ? Mono.Cecil.Rocks.TypeReferenceRocks.MakePointerType(loc.ParameterType) : loc.ParameterType;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                        }
                        break;
                // Variable loading instructions
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
                        }
                        break;
                // Field loading instructions
                    case Code.Ldfld:
                    case Code.Ldflda:
                        {
                            Pop(ref stack);
                            var field = instruction.Operand as FieldReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(field.FieldType)), stack);
                        }
                        break;
                    case Code.Ldsfld:
                    case Code.Ldsflda:
                        {
                            var field = instruction.Operand as FieldReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(field.FieldType)), stack);
                        }
                        break;
                // Array loading instructions
                    case Code.Ldelem_I:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);
                        }
                        break;
                    case Code.Ldelem_I1:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSByte)), stack);
                        }
                        break;
                    case Code.Ldelem_I2:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt16)), stack);
                        }
                        break;
                    case Code.Ldelem_I4:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt32)), stack);
                        }
                        break;
                    case Code.Ldelem_I8:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt64)), stack);
                        }
                        break;
                    case Code.Ldelem_R4:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSingle)), stack);
                        }
                        break;
                    case Code.Ldelem_R8:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemDouble)), stack);
                        }
                        break;
                    case Code.Ldelem_U1:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemByte)), stack);
                        }
                        break;
                    case Code.Ldelem_U2:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt16)), stack);
                        }
                        break;
                    case Code.Ldelem_U4:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt64)), stack);
                        }
                        break;
                    case Code.Ldelem_Ref:
                        {
                            Pop(ref stack);
                            var array = Pop(ref stack);
                            var ty = array.Item2.Type.GetElementType();
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                        }
                        break;
                    case Code.Ldelem_Any:
                    case Code.Ldelema:
                        {
                            Pop(ref stack);
                            Pop(ref stack);
                            bool isAddress = instruction.OpCode.Code == Code.Ldelem_Any || instruction.OpCode.Code == Code.Ldelema;
                            var elem = instruction.Operand as TypeReference;
                            var ty = isAddress ? Mono.Cecil.Rocks.TypeReferenceRocks.MakePointerType(elem) : elem;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                        }
                        break;
                    case Code.Ldlen:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUIntPtr)), stack);
                        }
                        break;
                // Indirect load instructions
                    case Code.Ldind_I:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);
                        }
                        break;
                    case Code.Ldind_I1:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSByte)), stack);
                        }
                        break;
                    case Code.Ldind_I2:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt16)), stack);
                        }
                        break;
                    case Code.Ldind_I4:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt32)), stack);
                        }
                        break;
                    case Code.Ldind_I8:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt64)), stack);
                        }
                        break;
                    case Code.Ldind_R4:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSingle)), stack);
                        }
                        break;
                    case Code.Ldind_R8:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemDouble)), stack);
                        }
                        break;
                    case Code.Ldind_U1:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemByte)), stack);
                        }
                        break;
                    case Code.Ldind_U2:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt16)), stack);
                        }
                        break;
                    case Code.Ldind_U4:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt32)), stack);
                        }
                        break;
                    case Code.Ldind_Ref:
                        {
                            var ptr = Pop(ref stack);
                            var ty = ptr.Item2.Type.GetElementType();
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ty)), stack);
                        }
                        break;
                // New object/array instructions
                    case Code.Newarr:
                        {
                            Pop(ref stack);
                            var type = (Mono.Cecil.TypeReference)instruction.Operand;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(Mono.Cecil.Rocks.TypeReferenceRocks.MakeArrayType(type))), stack);

                        }
                        break;
                    case Code.Newobj:
                        {
                            var ctor = instruction.Operand as MethodReference;
                            for (int j = 0; j < ctor.Parameters.Count; ++j)
                            {
                                Pop(ref stack);
                            }
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(ctor.DeclaringType)), stack);
                        }
                        break;
                // Pop1, Push1
                    case Code.Castclass:
                    case Code.Isinst:
                    case Code.Ldobj:
                        {
                            var src = Pop(ref stack);
                            var type = instruction.Operand as TypeReference;
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(type)), stack);
                        }
                        break;
                    case Code.Localloc:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);
                        }
                        break;
                // Sizeof
                    case Code.Sizeof:
                        {
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt32)), stack);
                        }
                        break;
                // Load metadata
                    case Code.Ldvirtftn:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);                            
                        }
                        break;
                    case Code.Ldftn:
                        {
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);                            
                        }
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
                        }
                        break;
                // TypedReferences
                    case Code.Mkrefany:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemTypedReference)), stack);
                        }
                        break;
                    case Code.Refanytype:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemRuntimeTypeHandle)), stack);
                        }
                        break;
                    case Code.Refanyval:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);
                        }
                        break;
                // Casts and conversions                        
                    case Code.Conv_I:
                    case Code.Conv_Ovf_I:
                    case Code.Conv_Ovf_I_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemIntPtr)), stack);
                        }
                        break;
                    case Code.Conv_I1:
                    case Code.Conv_Ovf_I1:
                    case Code.Conv_Ovf_I1_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSByte)), stack);
                        }
                        break;
                    case Code.Conv_I2:
                    case Code.Conv_Ovf_I2:
                    case Code.Conv_Ovf_I2_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt16)), stack);
                        }
                        break;
                    case Code.Conv_I4:
                    case Code.Conv_Ovf_I4:
                    case Code.Conv_Ovf_I4_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt32)), stack);
                        }
                        break;
                    case Code.Conv_I8:
                    case Code.Conv_Ovf_I8:
                    case Code.Conv_Ovf_I8_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemInt64)), stack);
                        }
                        break;
                    case Code.Conv_U:
                    case Code.Conv_Ovf_U:
                    case Code.Conv_Ovf_U_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUIntPtr)), stack);
                        }
                        break;
                    case Code.Conv_U1:
                    case Code.Conv_Ovf_U1:
                    case Code.Conv_Ovf_U1_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemByte)), stack);
                        }
                        break;
                    case Code.Conv_U2:
                    case Code.Conv_Ovf_U2:
                    case Code.Conv_Ovf_U2_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt16)), stack);
                        }
                        break;
                    case Code.Conv_U4:
                    case Code.Conv_Ovf_U4:
                    case Code.Conv_Ovf_U4_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt32)), stack);
                        }
                        break;
                    case Code.Conv_U8:
                    case Code.Conv_Ovf_U8:
                    case Code.Conv_Ovf_U8_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemUInt64)), stack);
                        }
                        break;
                    case Code.Conv_R4:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemSingle)), stack);
                        }
                        break;
                    case Code.Conv_R8:
                    case Code.Conv_R_Un:
                        {
                            Pop(ref stack);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(_SystemDouble)), stack);
                        }
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
