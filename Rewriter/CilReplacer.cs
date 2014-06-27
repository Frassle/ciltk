using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Rocks;
using Silk.Loom;

namespace Weave
{
    class CilReplacer : InstructionVisitor
    {
        LabelReplacer Labels;
        System.Reflection.FieldInfo[] OpcodesFields;

        public CilReplacer(LabelReplacer labels)
        {
            Labels = labels;
            OpcodesFields = typeof(OpCodes).GetFields();
        }

        protected override bool ShouldVisit(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var calledMethod = instruction.Operand as MethodReference;

                if (calledMethod != null && calledMethod.DeclaringType.FullName == "Silk.Cil")
                {
                    return true;
                }
            }

            return false;
        }

        protected override Instruction Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            var calledMethod = instruction.Operand as MethodReference;
            var next = instruction.Next;

            if (calledMethod.Name == "KeepAlive")
            {
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
            }
            else if (calledMethod.Name == "Load" || calledMethod.Name == "Peek")
            {
                /*
                 * The compiler will have inserted the appropriate load 
                 * instructions to put the value on the operand stack in
                 * preperation to call Load<T>. Thus all we have to do is 
                 * remove the call instruction, that keeps the value on the
                 * stack instead of popping it for the call.
                 */
                StackAnalyser.RemoveInstruction(ilProcessor, instruction);
            }
            else if (calledMethod.Name == "LoadAddress")
            {
                /*
                 * The compiler will have inserted the a load instruction, we 
                 * need to change it to the appropriate load address instruction.
                 */
                return ReplaceLoadAddress(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Store")
            {
                /*
                 * The compiler will have inserted instructions to load the
                 * addr of the location we want to store to. We need to look 
                 * at these instructions and replace them with the appropriate
                 * standard store instruction. We then remove the call to Store.
                 */

                return ReplaceStore(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "LoadByName")
            {
                return ReplaceLoadByName(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "StoreByName")
            {
                return ReplaceStoreByName(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "LoadAddressByName")
            {
                return ReplaceLoadAddressByName(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "DeclareLocal")
            {
                return ReplaceDeclareLocal(ilProcessor, instruction, calledMethod);
            }
            else
            {
                return ReplaceInstruction(ilProcessor, instruction, calledMethod);
            }

            return next;
        }

        private Instruction ReplaceLoadByName(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var stack = Analysis[instruction.Previous];
            var variableName = stack.Head.Item2;

            if (!variableName.IsConstant)
            {
                throw new Exception(string.Format("Expected constant values to be passed to LoadByName used in {0}", ilProcessor.Body.Method.FullName));
            }


            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == variableName.Value);
            if (variable != null)
            {
                var ldloc = Instruction.Create(OpCodes.Ldloc, variable);
                ilProcessor.InsertAfter(instruction, ldloc);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return ldloc.Next;
            }

            var parameter = ilProcessor.Body.Method.Parameters.FirstOrDefault(p => p.Name == variableName.Value);
            if (parameter != null)
            {
                var ldarg = Instruction.Create(OpCodes.Ldarg, parameter);
                ilProcessor.InsertAfter(instruction, ldarg);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return ldarg.Next;
            }

            throw new Exception(string.Format("Variable \"{0}\", used in method {1}, not found!", variableName.Value, ilProcessor.Body.Method.FullName));
        }

        private Instruction ReplaceStoreByName(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var stack = Analysis[instruction.Previous];
            var variableName = stack.Head.Item2;

            if (!variableName.IsConstant)
            {
                throw new Exception(string.Format("Expected constant values to be passed to StoreByName used in {0}", ilProcessor.Body.Method.FullName));
            }


            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == variableName.Value);
            if (variable != null)
            {
                var stloc = Instruction.Create(OpCodes.Stloc, variable);
                ilProcessor.InsertAfter(instruction, stloc);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return stloc.Next;
            }

            var parameter = ilProcessor.Body.Method.Parameters.FirstOrDefault(p => p.Name == variableName.Value);
            if (parameter != null)
            {
                var starg = Instruction.Create(OpCodes.Starg, parameter);
                ilProcessor.InsertAfter(instruction, starg);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return starg.Next;
            }

            throw new Exception(string.Format("Variable \"{0}\", used in method {1}, not found!", variableName.Value, ilProcessor.Body.Method.FullName));
        }

        private Instruction ReplaceLoadAddressByName(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var stack = Analysis[instruction.Previous];
            var variableName = stack.Head.Item2;

            if (!variableName.IsConstant)
            {
                throw new Exception(string.Format("Expected constant values to be passed to LoadAddressByName used in {0}", ilProcessor.Body.Method.FullName));
            }

            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == variableName.Value);
            if (variable != null)
            {
                var ldloc = Instruction.Create(OpCodes.Ldloca, variable);
                ilProcessor.InsertAfter(instruction, ldloc);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return ldloc.Next;
            }

            var parameter = ilProcessor.Body.Method.Parameters.FirstOrDefault(p => p.Name == variableName.Value);
            if (parameter != null)
            {
                var ldarg = Instruction.Create(OpCodes.Ldarga, parameter);
                ilProcessor.InsertAfter(instruction, ldarg);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return ldarg.Next;
            }

            throw new Exception(string.Format("Variable \"{0}\", used in method {1}, not found!", variableName.Value, ilProcessor.Body.Method.FullName));
        }

        private Instruction ReplaceDeclareLocal(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var stack = Analysis[instruction.Previous];
            var name = stack.Head.Item2;
            var type = stack.Tail.Head.Item2;

            if (name.IsConstant && type.IsConstant)
            {
                var variableType = References.FindType(ilProcessor.Body.Method.Module, ilProcessor.Body, type.Value);

                ilProcessor.Body.Variables.Add(new VariableDefinition(name.Value, variableType));
            }
            else
            {
                throw new Exception(string.Format("Expected constant values to be passed to DeclareLocal used in method {0}", ilProcessor.Body.Method.FullName));
            }

            Instruction next = instruction.Next;
            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
            return next;
        }

        private Instruction ReplaceLoadAddress(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var generic_method = calledMethod as GenericInstanceMethod;
            var typetok = generic_method.GenericArguments[0];

            var stack = Analysis[instruction.Previous];

            var operandEntry = stack.Head;
            var addr_instruction = operandEntry.Item1;
            var next = instruction.Next;

            if (addr_instruction.OpCode == OpCodes.Ldloc)
            {
                var local = (VariableDefinition)addr_instruction.Operand;
                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldloca, local));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldarg)
            {
                var argument = (ParameterDefinition)addr_instruction.Operand;
                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldarga, argument));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldsfld)
            {
                var field = (FieldReference)addr_instruction.Operand;
                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldsflda, field));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldfld)
            {
                var field = (FieldReference)addr_instruction.Operand;
                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldflda, field));
            }
            else if (
                addr_instruction.OpCode == OpCodes.Ldelem_Any)
            {
                var type = (TypeReference)addr_instruction.Operand;
                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldelema, type));
            }
            else if (
                addr_instruction.OpCode == OpCodes.Ldelem_I ||
                addr_instruction.OpCode == OpCodes.Ldelem_I1 ||
                addr_instruction.OpCode == OpCodes.Ldelem_I2 ||
                addr_instruction.OpCode == OpCodes.Ldelem_I4 ||
                addr_instruction.OpCode == OpCodes.Ldelem_I8 ||
                addr_instruction.OpCode == OpCodes.Ldelem_U1 ||
                addr_instruction.OpCode == OpCodes.Ldelem_U2 ||
                addr_instruction.OpCode == OpCodes.Ldelem_U4 ||
                addr_instruction.OpCode == OpCodes.Ldelem_R4 ||
                addr_instruction.OpCode == OpCodes.Ldelem_R8 ||
                addr_instruction.OpCode == OpCodes.Ldelem_Ref)
            {
                var module = ilProcessor.Body.Method.Module;

                TypeReference type;
                switch (addr_instruction.OpCode.Code)
                {
                    case Code.Ldelem_I:
                        type = new TypeReference("System", "IntPtr", module, module);
                        break;
                    case Code.Ldelem_I1:
                        type = new TypeReference("System", "SByte", module, module);
                        break;
                    case Code.Ldelem_I2:
                        type = new TypeReference("System", "Int16", module, module);
                        break;
                    case Code.Ldelem_I4:
                        type = new TypeReference("System", "Int32", module, module);
                        break;
                    case Code.Ldelem_I8:
                        type = new TypeReference("System", "Int64", module, module);
                        break;
                    case Code.Ldelem_U1:
                        type = new TypeReference("System", "Byte", module, module);
                        break;
                    case Code.Ldelem_U2:
                        type = new TypeReference("System", "UInt16", module, module);
                        break;
                    case Code.Ldelem_U4:
                        type = new TypeReference("System", "UInt32", module, module);
                        break;
                    case Code.Ldelem_R4:
                        type = new TypeReference("System", "Single", module, module);
                        break;
                    case Code.Ldelem_R8:
                        type = new TypeReference("System", "Double", module, module);
                        break;
                    case Code.Ldelem_Ref:
                    default:
                        {
                            // array is the lower item in the stack
                            var array_instruction = Analysis[addr_instruction.Previous].Tail.Head.Item1;
                            // whatever load loaded this array it will have a type with it
                            type = array_instruction.Operand as TypeReference;
                        }
                        break;
                }

                StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Ldelema, type));
            }
            else
            {
                throw new Exception("ReplaceLoadAddress: How did we get here?!");
            }

            StackAnalyser.RemoveInstruction(ilProcessor, instruction);

            return next;
        }

        Instruction ReplaceStore(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var generic_method = calledMethod as GenericInstanceMethod;
            var typetok = generic_method.GenericArguments[0];

            var stack = Analysis[instruction.Previous];

            var operandEntry = stack.Head;
            var addr_instruction = operandEntry.Item1;
            var next = instruction.Next;

            if (addr_instruction.OpCode == OpCodes.Ldloca)
            {
                var local = (VariableDefinition)addr_instruction.Operand;
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Stloc, local));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldarga)
            {
                var argument = (ParameterDefinition)addr_instruction.Operand;
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Starg, argument));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldsflda)
            {
                var field = (FieldReference)addr_instruction.Operand;
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Stsfld, field));
            }
            else if (addr_instruction.OpCode == OpCodes.Ldflda)
            {
                var field = (FieldReference)addr_instruction.Operand;

                var value_temp = new VariableDefinition(field.FieldType);
                VariableDefinition addr_temp;
                if (field.DeclaringType.IsValueType)
                {
                    addr_temp = new VariableDefinition(field.DeclaringType.MakeByReferenceType());
                }
                else
                {
                    addr_temp = new VariableDefinition(field.DeclaringType);
                }

                ilProcessor.Body.Variables.Add(value_temp);
                ilProcessor.Body.Variables.Add(addr_temp);

                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Stloc, addr_temp));
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Stloc, value_temp));
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Ldloc, addr_temp));
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Ldloc, value_temp));
                ilProcessor.InsertBefore(addr_instruction, Instruction.Create(OpCodes.Stfld, field));
            }
            else
            {
                throw new Exception("ReplaceStore: How did we get here?!");
            }

            StackAnalyser.RemoveInstruction(ilProcessor, addr_instruction);
            StackAnalyser.RemoveInstruction(ilProcessor, instruction);

            return next;
        }

        Instruction ReplaceInstruction(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var nextInstruction = instruction.Next;

            var opcodeField = OpcodesFields.FirstOrDefault(info => info.Name == calledMethod.Name);
            OpCode opcode;

            if (opcodeField == null)
            {
                // Special case stelem because we don't want to call it Stelem_Any
                if (calledMethod.Name == "Stelem")
                {
                    opcode = OpCodes.Stelem_Any;
                }
                // Special case ldelem because we don't want to call it Ldelem_Any
                else if (calledMethod.Name == "Ldelem")
                {
                    opcode = OpCodes.Ldelem_Any;
                }
                else
                {
                    throw new Exception(string.Format("Unknown opcode {0}, ignoring", calledMethod.Name));
                }
            }
            else
            {
                opcode = (OpCode)opcodeField.GetValue(null);
            }

            if (calledMethod is GenericInstanceMethod)
            {
                var generic_method = calledMethod as GenericInstanceMethod;
                var typetok = generic_method.GenericArguments[0];
                StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, typetok));
            }
            else
            {
                if (calledMethod.Parameters.Count == 0)
                {
                    StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode));
                }
                if (calledMethod.Parameters.Count == 1)
                {
                    var stack = Analysis[instruction.Previous];

                    var operandEntry = stack.Head;
                    StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, operandEntry.Item1, Analysis);

                    if (!operandEntry.Item2.IsConstant)
                    {
                        Console.WriteLine("Inline ({0}) without constant argument, ignoring.", opcode);
                        StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                        return nextInstruction;
                    }

                    var operand = operandEntry.Item2.Value;

                    if (opcode.OperandType == OperandType.InlineVar)
                    {
                        var variable = ilProcessor.Body.Variables[(int)operand];
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, variable));
                    }
                    else if (opcode.OperandType == OperandType.InlineArg)
                    {
                        var variable = ilProcessor.Body.Method.Parameters[(int)operand];
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, variable));
                    }
                    else if (opcode.OperandType == OperandType.InlineBrTarget)
                    {
                        var jump = Labels.GetJumpLocation(ilProcessor.Body, (string)operand);
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, jump));
                    }
                    else if (opcode.OperandType == OperandType.ShortInlineI)
                    {
                        var integer = (byte)operand;
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.InlineI)
                    {
                        var integer = (int)operand;
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.InlineI8)
                    {
                        var integer = (long)operand;
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.ShortInlineR)
                    {
                        var real = (float)operand;
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, real));
                    }
                    else if (opcode.OperandType == OperandType.InlineR)
                    {
                        var real = (double)operand;
                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, real));
                    }
                    else if (opcode.OperandType == OperandType.InlineSwitch)
                    {
                        var target_string = (string)operand;
                        var targets = target_string.Split(';').Select(label => Labels.GetJumpLocation(ilProcessor.Body, label)).ToArray();

                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, targets));
                    }
                    else if (opcode.OperandType == OperandType.InlineField)
                    {
                        var module = ilProcessor.Body.Method.Module;
                        var field = (string)operand;
                        var fieldref = Silk.Loom.References.FindField(module, ilProcessor.Body, field);

                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, fieldref));
                    }
                    else if (opcode.OperandType == OperandType.InlineMethod)
                    {
                        var module = ilProcessor.Body.Method.Module;
                        var method = (string)operand;
                        var methodref = Silk.Loom.References.FindMethod(module, ilProcessor.Body, method);

                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, methodref));
                    }
                    else if (opcode.OperandType == OperandType.InlineType)
                    {
                        var module = ilProcessor.Body.Method.Module;
                        var type = (string)operand;
                        var typeref = Silk.Loom.References.FindType(module, ilProcessor.Body, type);

                        StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(opcode, typeref));
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Inline opcode ({0}) without argument, ignoring.", opcode));
                    }
                }
                else
                {
                    if (opcode.OperandType == OperandType.InlineSig)
                    {
                        return ReplaceCalli(ilProcessor, instruction, calledMethod);
                    }
                }
            }

            return nextInstruction;
        }

        Instruction ReplaceCalli(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            //public static unsafe void Calli(
            //System.Runtime.InteropServices.CallingConvention callingConvention, Type returnType, Type arg0, ...)
            var stack = Analysis[instruction.Previous];
            Tuple<Instruction, StackAnalyser.StackEntry> entry;

            var module = ilProcessor.Body.Method.Module;

            var args = new List<Tuple<Instruction, StackAnalyser.StackEntry>>();
            for (int i = 0; i < calledMethod.Parameters.Count - 2; ++i)
            {
                entry = stack.Head;
                stack = stack.Tail;
                args.Add(entry);
            }
            args.Reverse();

            entry = stack.Head;
            stack = stack.Tail;
            var returnType = entry;

            entry = stack.Head;
            stack = stack.Tail;
            var callingConvention = entry.Item2;

            Mono.Cecil.MethodCallingConvention methodCallingConvention;
            if (callingConvention.IsConstant)
            {
                switch ((System.Runtime.InteropServices.CallingConvention)callingConvention.Value)
                {
                    case System.Runtime.InteropServices.CallingConvention.Cdecl:
                        methodCallingConvention = MethodCallingConvention.C;
                        break;
                    case System.Runtime.InteropServices.CallingConvention.FastCall:
                        methodCallingConvention = MethodCallingConvention.FastCall;
                        break;
                    case System.Runtime.InteropServices.CallingConvention.StdCall:
                        methodCallingConvention = MethodCallingConvention.StdCall;
                        break;
                    case System.Runtime.InteropServices.CallingConvention.ThisCall:
                        methodCallingConvention = MethodCallingConvention.ThisCall;
                        break;
                    case System.Runtime.InteropServices.CallingConvention.Winapi:
                        methodCallingConvention = MethodCallingConvention.StdCall;
                        break;
                    default:
                        methodCallingConvention = MethodCallingConvention.Default;
                        break;
                }
            }
            else
            {
                Console.WriteLine("Calling convention passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return next;
            }

            TypeReference returnTypeReference = null;
            if (returnType.Item2.IsConstant)
            {
                Type retTy = returnType.Item2.Value;
                returnTypeReference = module.Import(retTy);
            }
            else if (returnType.Item1.OpCode == OpCodes.Call && returnType.Item1.Operand is MethodReference && (returnType.Item1.Operand as MethodReference).Name == "GetTypeFromHandle")
            {
                var ldtoken_stack = Analysis[returnType.Item1.Previous];
                var ldtoken = ldtoken_stack.Head;

                returnTypeReference = ldtoken.Item1.Operand as TypeReference;
            }
            
            if (returnTypeReference == null)
            {
                Console.WriteLine("Return type passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return next;
            }

            TypeReference[] parameterTypesArray = new TypeReference[args.Count];
            for (int i = 0; i < args.Count; ++i)
            {
                var arg = args[i];

                if (arg.Item2.IsConstant)
                {
                    parameterTypesArray[i] = module.Import((Type)arg.Item2.Value);
                }
                else if (arg.Item1.OpCode == OpCodes.Call && arg.Item1.Operand is MethodReference && (arg.Item1.Operand as MethodReference).Name == "GetTypeFromHandle")
                {
                    var ldtoken_stack = Analysis[arg.Item1.Previous];
                    var ldtoken = ldtoken_stack.Head;

                    parameterTypesArray[i] = ldtoken.Item1.Operand as TypeReference;
                }
            }

            if (parameterTypesArray.Any(ty => ty == null))
            {
                Console.WriteLine("Type passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return next;
            }

            var callSite = new CallSite(returnTypeReference);
            callSite.CallingConvention = methodCallingConvention;
            foreach (var parameterType in parameterTypesArray)
            {
                callSite.Parameters.Add(new ParameterDefinition(parameterType));
            }

            {
                var calli = Instruction.Create(OpCodes.Calli, callSite);
                ilProcessor.InsertAfter(instruction, calli);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                return calli.Next;
            }
        }
    }
}
