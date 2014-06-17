using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TStack = Microsoft.FSharp.Collections.FSharpList<System.Tuple<Mono.Cecil.Cil.Instruction, Silk.Loom.StackAnalyser.StackEntry>>;

namespace Silk.Loom
{
    public static class StackAnalyser
    {
        public struct StackEntry
        {
            public bool IsConstant { get; private set; }
            public TypeReference Type { get; private set; }

            public dynamic Value { get; private set; }

            internal StackEntry(TypeReference type) 
                : this()
            {
                Type = type;
                IsConstant = false;
                Value = null;
            }

            internal StackEntry(Mono.Cecil.ModuleDefinition module, object value)
                : this()
            {
                if (value != null)
                {
                    Type = References.FindType(module, null, value.GetType().FullName);
                    IsConstant = true;
                    Value = value;
                }
                else
                {
                    Type = References.FindType(module, null, "System.Object");
                    IsConstant = true;
                    Value = null;
                }
            }
        }

        static Dictionary<string, Type> RefiedTypes;

        private static Type Reify(TypeReference type)
        {
            if (RefiedTypes == null)
            {
                var types = new Dictionary<string, Type>();
                types.Add("System.Void", typeof(void));
                types.Add("System.SByte", typeof(sbyte));
                types.Add("System.Int16", typeof(short));
                types.Add("System.Int32", typeof(int));
                types.Add("System.Int64", typeof(long));
                types.Add("System.Byte", typeof(byte));
                types.Add("System.UInt16", typeof(ushort));
                types.Add("System.UInt32", typeof(uint));
                types.Add("System.UInt64", typeof(ulong));
                types.Add("System.Single", typeof(float));
                types.Add("System.Double", typeof(double));
                RefiedTypes = types;
            }

            Type ty = null;
            RefiedTypes.TryGetValue(type.FullName, out ty);
            return ty;

            //var assembly = type.Scope.Name;

            //try
            //{
            //    var loadedAssembly = System.Reflection.Assembly.Load(assembly);

            //    if (type.IsGenericInstance)
            //    {
            //        var genericType = type as GenericInstanceType;
            //        var ty = loadedAssembly.GetType(genericType.ElementType.FullName);
            //        var args = genericType.GenericArguments.Select(arg => Reify(arg)).ToArray();
            //        return ty.MakeGenericType(args);
            //    }
            //    else
            //    {
            //        return loadedAssembly.GetType(type.FullName);
            //    }
            //}
            //catch(Exception)
            //{
            //    return null;
            //}
        }

        private static System.Reflection.MethodInfo Reify(MethodReference method)
        {
            var ty = Reify(method.DeclaringType);
            if (ty != null)
            {
                var methods = ty.GetMethods();
                foreach (var m in methods)
                {
                    if (m.Name == method.Name)
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length == method.Parameters.Count)
                        {
                            if (parameters.Zip(method.Parameters, (param, paramRef) => param.ParameterType.FullName == paramRef.ParameterType.FullName).All(b => b))
                            {
                                return m;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static Instruction RemoveInstructionChain(MethodDefinition method, Instruction instruction, Dictionary<Instruction, TStack> analysis)
        {
            var ilProcessor = method.Body.GetILProcessor();
            var nop = Instruction.Create(OpCodes.Nop);

            if (instruction.OpCode.StackBehaviourPop == StackBehaviour.Pop0)
            {
                ilProcessor.Replace(instruction, nop);
            }
            else
            {
                var stack = analysis[instruction.Previous];
                ilProcessor.Replace(instruction, nop);

                switch (instruction.OpCode.StackBehaviourPop)
                {
                    case StackBehaviour.Pop1:
                        RemoveInstructionChain(method, stack.Head.Item1, analysis);
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
                                break;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                    case StackBehaviour.Popref_popi:
                        RemoveInstructionChain(method, stack.Head.Item1, analysis);
                        RemoveInstructionChain(method, stack.Tail.Head.Item1, analysis);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return nop;
        }

        private static System.Reflection.FieldInfo Reify(FieldReference field)
        {
            var assembly = field.Module.Assembly.FullName;
            var type = field.DeclaringType.FullName;
            var name = field.Name;

            try
            {
                var loadedAssembly = System.Reflection.Assembly.Load(assembly);
                var ty = loadedAssembly.GetType(type);
                return ty.GetField(name);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Tuple<Instruction, StackEntry> Pop(ref TStack stack)
        {
            var value = stack.Head;
            stack = stack.Tail;
            return value;
        }

        public static Dictionary<Instruction, TStack> Analyse(MethodDefinition method)
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
                        {
                            var a = Pop(ref stack).Item2;
                            var b = Pop(ref stack).Item2;
                            if (a.IsConstant & b.IsConstant)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(module, a.Value + b.Value)), stack);
                            }
                            else
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Type)), stack);
                            }
                            break;
                        }
                    case Code.And:
                        {
                            var a = Pop(ref stack).Item2;
                            var b = Pop(ref stack).Item2;
                            if (a.IsConstant & b.IsConstant)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(module, a.Value & b.Value)), stack);
                            }
                            else
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Type)), stack);
                            }
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
                            if (a.IsConstant)
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (object)a.Value)), stack);
                            }
                            else
                            {
                                stack = new TStack(Tuple.Create(instruction, new StackEntry(a.Type)), stack);
                            }
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

                            if (args.All(arg => arg.IsConstant) && (!m.HasThis || self.IsConstant) && (m.DeclaringType.FullName != "Silk.Cil")) // We know the Silk methods will just throw
                            {
                                var meth = Reify(m);
                                if (meth != null)
                                {
                                    var obj = (object)self.Value;
                                    var parameters = args.Select(arg => (object)arg.Value).ToArray();
                                    try
                                    {
                                        var result = meth.Invoke(obj, parameters);
                                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, result)), stack);
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }                                
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
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (int)instruction.Operand)), stack);
                        break;
                    case Code.Ldc_I4_0:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 0)), stack);
                        break;
                    case Code.Ldc_I4_1:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 1)), stack);
                        break;
                    case Code.Ldc_I4_2:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 2)), stack);
                        break;
                    case Code.Ldc_I4_3:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 3)), stack);
                        break;
                    case Code.Ldc_I4_4:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 4)), stack);
                        break;
                    case Code.Ldc_I4_5:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 5)), stack);
                        break;
                    case Code.Ldc_I4_6:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 6)), stack);
                        break;
                    case Code.Ldc_I4_7:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 7)), stack);
                        break;
                    case Code.Ldc_I4_8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, 8)), stack);
                        break;
                    case Code.Ldc_I4_M1:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, -1)), stack);
                        break;
                    case Code.Ldc_I4_S:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (int)instruction.Operand)), stack);
                        break;
                    case Code.Ldc_I8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (long)instruction.Operand)), stack);
                        break;
                    case Code.Ldc_R4:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (float)instruction.Operand)), stack);
                        break;
                    case Code.Ldc_R8:
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, (double)instruction.Operand)), stack);
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
								new StackEntry(References.FindType(module, method.Body, "System.IntPtr"))), stack);
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
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, null)), stack);
                        break;
                    case Code.Ldobj:
                        {
                            var src = Pop(ref stack);
                            var type = instruction.Operand as TypeReference;
                            Type ty = Reify(type);
                            stack = new TStack(Tuple.Create(instruction, new StackEntry(module, ty)), stack);
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
                        stack = new TStack(Tuple.Create(instruction, new StackEntry(module, instruction.Operand as string)), stack);
                        break;
                    case Code.Ldtoken:
                        {
                            var token = instruction.Operand;

                            if (token is Mono.Cecil.MethodReference)
                            {
                                var m = Reify((Mono.Cecil.MethodReference)token);
                                if (m == null)
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(References.FindType(module, null, "System.RuntimeMethodHandle"))), stack);
                                }
                                else
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(module, m.MethodHandle)), stack);
                                }
                            }
                            if (token is Mono.Cecil.TypeReference)
                            {
                                var t = Reify((Mono.Cecil.TypeReference)token); 
                                if (t == null)
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(References.FindType(module, null, "System.RuntimeTypeHandle"))), stack);
                                }
                                else
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(module, t.TypeHandle)), stack);
                                }
                            }
                            if (token is Mono.Cecil.FieldReference)
                            {
                                var f = Reify((Mono.Cecil.FieldReference)token); 
                                if (f == null)
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(References.FindType(module, null, "System.RuntimeFieldHandle"))), stack);
                                }
                                else
                                {
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(module, f.FieldHandle)), stack);
                                }
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
                            if (length.IsConstant)
                            {
                                Type ty = Reify(type);
                                if (ty != null)
                                {
                                    var arr = Array.CreateInstance(ty, (int)length.Value);
                                    stack = new TStack(Tuple.Create(instruction, new StackEntry(module, arr)), stack);
                                    break;
                                }
                            }

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
