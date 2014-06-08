using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TStack = System.Collections.Generic.Stack<System.Tuple<Mono.Cecil.Cil.Instruction, Silk.Loom.StackAnalyser.StackEntry>>;

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
                    Type = module.Import(value.GetType());
                    IsConstant = true;
                    Value = value;
                }
                else
                {
                    Type = module.Import(typeof(object));
                    IsConstant = true;
                    Value = null;
                }
            }
        }

        private static Type Reify(TypeReference type)
        {
            var assembly = type.Scope.Name;

            try
            {
                var loadedAssembly = System.Reflection.Assembly.Load(assembly);

                if (type.IsGenericInstance)
                {
                    var genericType = type as GenericInstanceType;
                    var ty = loadedAssembly.GetType(genericType.ElementType.FullName);
                    var args = genericType.GenericArguments.Select(arg => Reify(arg)).ToArray();
                    return ty.MakeGenericType(args);
                }
                else
                {
                    return loadedAssembly.GetType(type.FullName);
                }
            }
            catch(Exception)
            {
                return null;
            }
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

        public static void RemoveInstructionChain(MethodDefinition method, Instruction instruction)
        {
            var stack = StackAnalyser.Analyse(method)[instruction.Previous];

            var ilProcessor = method.Body.GetILProcessor();
            ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Nop));

            switch (instruction.OpCode.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    break;
                case StackBehaviour.Pop1:
                    RemoveInstructionChain(method, stack.Pop().Item1);
                    break;
                case StackBehaviour.Varpop:
                    {
                        if (instruction.OpCode.OperandType == OperandType.InlineMethod)
                        {
                            var m = instruction.Operand as MethodReference;
                            for (int i = 0; i < m.Parameters.Count; ++i)
                            {
                                RemoveInstructionChain(method, stack.Pop().Item1);
                            }
                            if (m.HasThis)
                            {
                                RemoveInstructionChain(method, stack.Pop().Item1);
                            }
                            break;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                default:
                    throw new NotImplementedException();
            }
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

                TStack stack = new TStack();
                TStack previous_stack;
                if (previous_instruction != null && map.TryGetValue(previous_instruction, out previous_stack))
                {
                    foreach (var item in previous_stack.Reverse())
                    {
                        stack.Push(item);
                    }
                }

                switch (instruction.OpCode.Code)
                {
                    case Code.Add:
                    case Code.Add_Ovf:
                    case Code.Add_Ovf_Un:
                        {
                            var a = stack.Pop().Item2;
                            var b = stack.Pop().Item2;
                            if (a.IsConstant & b.IsConstant)
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, a.Value + b.Value)));
                            }
                            else
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(a.Type)));
                            }
                            break;
                        }
                    case Code.And:
                        {
                            var a = stack.Pop().Item2;
                            var b = stack.Pop().Item2;
                            if (a.IsConstant & b.IsConstant)
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, a.Value & b.Value)));
                            }
                            else
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(a.Type)));
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
                            stack.Pop();
                            stack.Pop();
                            break;
                        }
                    case Code.Box:
                        {
                            var a = stack.Pop().Item2;
                            if (a.IsConstant)
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, (object)a.Value)));
                            }
                            else
                            {
                                stack.Push(Tuple.Create(instruction, new StackEntry(a.Type)));
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
                            stack.Pop();
                            break;
                        }
                    case Code.Call:
                    case Code.Callvirt:
                        {
                            var m = instruction.Operand as MethodReference;
                            StackEntry[] args = new StackEntry[m.Parameters.Count];
                            for (int j = 0; j < m.Parameters.Count; ++j)
                            {
                                args[j] = stack.Pop().Item2;
                            }
                            Array.Reverse(args);

                            StackEntry self = default(StackEntry);
                            if(m.HasThis)
                            {
                                self = stack.Pop().Item2;
                            }

                            if (args.All(arg => arg.IsConstant) && (!m.HasThis || self.IsConstant))
                            {
                                var meth = Reify(m);
                                if (meth != null)
                                {
                                    var obj = (object)self.Value;
                                    var parameters = args.Select(arg => (object)arg.Value).ToArray();
                                    try
                                    {
                                        var result = meth.Invoke(obj, parameters);
                                        stack.Push(Tuple.Create(instruction, new StackEntry(module, result)));
                                        break;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }                                
                            }
                            
                            stack.Push(Tuple.Create(instruction, new StackEntry(m.ReturnType)));
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
                        break;
                    case Code.Ldc_I4:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, (int)instruction.Operand)));
                        break;
                    case Code.Ldc_I4_0:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 0)));
                        break;
                    case Code.Ldc_I4_1:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 1)));
                        break;
                    case Code.Ldc_I4_2:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 2)));
                        break;
                    case Code.Ldc_I4_3:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 3)));
                        break;
                    case Code.Ldc_I4_4:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 4)));
                        break;
                    case Code.Ldc_I4_5:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 5)));
                        break;
                    case Code.Ldc_I4_6:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 6)));
                        break;
                    case Code.Ldc_I4_7:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 7)));
                        break;
                    case Code.Ldc_I4_8:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, 8)));
                        break;
                    case Code.Ldc_I4_M1:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, -1)));
                        break;
                    case Code.Ldc_I4_S:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, (int)instruction.Operand)));
                        break;
                    case Code.Ldc_I8:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, (long)instruction.Operand)));
                        break;
                    case Code.Ldc_R4:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, (float)instruction.Operand)));
                        break;
                    case Code.Ldc_R8:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, (double)instruction.Operand)));
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
                        break;
                    case Code.Ldftn:
                        {
                            var mref = instruction.Operand as MethodReference;
                            stack.Push(Tuple.Create(instruction, new StackEntry(module, mref)));
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
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
                            stack.Push(Tuple.Create(instruction, new StackEntry(ty)));
                            break;
                        }
                    case Code.Ldnull:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, null)));
                        break;
                    case Code.Ldobj:
                        {
                            var src = stack.Pop();
                            var type = instruction.Operand as TypeReference;
                            Type ty = Reify(type);
                            stack.Push(Tuple.Create(instruction, new StackEntry(module, ty)));
                        }
                        break;
                    case Code.Ldsfld:
                    case Code.Ldsflda:
                        {
                            var field = instruction.Operand as FieldReference;
                            stack.Push(Tuple.Create(instruction, new StackEntry(field.FieldType)));
                            break;
                        }
                    case Code.Ldstr:
                        stack.Push(Tuple.Create(instruction, new StackEntry(module, instruction.Operand as string)));
                        break;
                    case Code.Ldtoken:
                        {
                            var token = instruction.Operand;

                            if (token is Mono.Cecil.MethodReference)
                            {
                                var m = Reify((Mono.Cecil.MethodReference)token).MethodHandle;
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, m)));
                            }
                            if (token is Mono.Cecil.TypeReference)
                            {
                                var t = Reify((Mono.Cecil.TypeReference)token).TypeHandle;
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, t)));
                            }
                            if (token is Mono.Cecil.FieldReference)
                            {
                                var f = Reify((Mono.Cecil.FieldReference)token).FieldHandle;
                                stack.Push(Tuple.Create(instruction, new StackEntry(module, f)));
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
                        break;
                    case Code.Newarr:
                        {
                            var length = stack.Pop().Item2;
                            var type = (Mono.Cecil.TypeReference)instruction.Operand;
                            if (length.IsConstant)
                            {
                                Type ty = Reify(type);
                                if (ty != null)
                                {
                                    var arr = Array.CreateInstance(ty, (int)length.Value);
                                    stack.Push(Tuple.Create(instruction, new StackEntry(module, arr)));
                                    break;
                                }
                            }
                            
                            stack.Push(Tuple.Create(instruction, new StackEntry(Mono.Cecil.Rocks.TypeReferenceRocks.MakeArrayType(type))));
                            break;
                        }
                    case Code.Newobj:
                        {
                            var ctor = instruction.Operand as MethodReference;
                            for (int j = 0; j < ctor.Parameters.Count; ++j)
                            {
                                stack.Pop();
                            }
                            stack.Push(Tuple.Create(instruction, new StackEntry(ctor.DeclaringType)));
                            break;
                        }
                    case Code.No:
                        break;
                    case Code.Nop:
                        break;
                    case Code.Not:
                    case Code.Or:
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
                        break;
                    case Code.Pop:
                        stack.Pop();
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
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
                            stack.Pop();
                            stack.Pop();
                            stack.Pop();
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
                        break;
                    case Code.Stloc:
                    case Code.Stloc_0:
                    case Code.Stloc_1:
                    case Code.Stloc_2:
                    case Code.Stloc_3:
                    case Code.Stloc_S:
                        stack.Pop();
                        break;
                    case Code.Stobj:
                        stack.Pop();
                        stack.Pop();
                        break;
                    case Code.Stsfld:
                        stack.Pop();
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
                        stack.Push(Tuple.Create(instruction, new StackEntry(null)));
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
