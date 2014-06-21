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

        public CilReplacer(LabelReplacer labels)
        {
            Labels = labels;
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
                StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(OpCodes.Nop));
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
                throw new Exception("Expected constant values to be passed to LoadByName");
            }


            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == variableName.Value);
            if (variable != null)
            {
                var nop = StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                var ldloc = Instruction.Create(OpCodes.Ldloc, variable);
                StackAnalyser.ReplaceInstruction(ilProcessor, nop, ldloc);
                return ldloc.Next;
            }

            var parameter = ilProcessor.Body.Method.Parameters.FirstOrDefault(p => p.Name == variableName.Value);
            if (parameter != null)
            {
                var nop = StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                var ldarg = Instruction.Create(OpCodes.Ldarg, parameter);
                StackAnalyser.ReplaceInstruction(ilProcessor, nop, ldarg);
                return ldarg.Next;
            }

            throw new Exception(string.Format("Variable \"{0}\" not found", variableName.Value));
        }

        private Instruction ReplaceStoreByName(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var stack = Analysis[instruction.Previous];
            var variableName = stack.Head.Item2;

            if (!variableName.IsConstant)
            {
                throw new Exception("Expected constant values to be passed to LoadByName");
            }


            var variable = ilProcessor.Body.Variables.FirstOrDefault(v => v.Name == variableName.Value);
            if (variable != null)
            {
                var nop = StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                var stloc = Instruction.Create(OpCodes.Stloc, variable);
                StackAnalyser.ReplaceInstruction(ilProcessor, nop, stloc);
                return stloc.Next;
            }

            var parameter = ilProcessor.Body.Method.Parameters.FirstOrDefault(p => p.Name == variableName.Value);
            if (parameter != null)
            {
                var nop = StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                var starg = Instruction.Create(OpCodes.Starg, parameter);
                StackAnalyser.ReplaceInstruction(ilProcessor, nop, starg);
                return starg.Next;
            }

            throw new Exception(string.Format("Variable \"{0}\" not found", variableName.Value));
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
                throw new Exception("Expected constant values to be passed to DeclareLocal");
            }
            
            return StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
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
                addr_instruction.OpCode == OpCodes.Ldelem_Any ||
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
                ilProcessor.Replace(addr_instruction, Instruction.Create(OpCodes.Ldelema));
            }
            else
            {
                throw new Exception("ReplaceLoadAddress: How did we get here?!");
            }

            StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(OpCodes.Nop));

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

            StackAnalyser.ReplaceInstruction(ilProcessor, addr_instruction, Instruction.Create(OpCodes.Nop));
            StackAnalyser.ReplaceInstruction(ilProcessor, instruction, Instruction.Create(OpCodes.Nop));

            return next;
        }

        Instruction ReplaceInstruction(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var nextInstruction = instruction.Next;

            var opcodeField = typeof(OpCodes).GetFields().FirstOrDefault(info => info.Name == calledMethod.Name);
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
                        methodCallingConvention = MethodCallingConvention.C; break;
                    case System.Runtime.InteropServices.CallingConvention.FastCall:
                        methodCallingConvention = MethodCallingConvention.FastCall; break;
                    case System.Runtime.InteropServices.CallingConvention.StdCall:
                        methodCallingConvention = MethodCallingConvention.StdCall; break;
                    case System.Runtime.InteropServices.CallingConvention.ThisCall:
                        methodCallingConvention = MethodCallingConvention.ThisCall; break;
                    case System.Runtime.InteropServices.CallingConvention.Winapi:
                        methodCallingConvention = MethodCallingConvention.StdCall; break;
                    default:
                        methodCallingConvention = MethodCallingConvention.Default; break;
                }
            }
            else
            {
                Console.WriteLine("Calling convention passed to Calli is not a constant expression.");
                return StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
            }

            TypeReference returnTypeReference = null;
            if (returnType.Item2.IsConstant)
            {
                Type retTy = returnType.Item2.Value;
                returnTypeReference = module.Import(retTy);
            }
            else if(returnType.Item1.OpCode == OpCodes.Call && returnType.Item1.Operand is MethodReference && (returnType.Item1.Operand as MethodReference).Name == "GetTypeFromHandle")
            {
                var ldtoken_stack = Analysis[returnType.Item1.Previous];
                var ldtoken = ldtoken_stack.Head;

                returnTypeReference = ldtoken.Item1.Operand as TypeReference;
            }
            
            if(returnTypeReference == null)
            {
                Console.WriteLine("Return type passed to Calli is not a constant expression.");
                return StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
            }

            TypeReference[] parameterTypesArray = new TypeReference[args.Count];
            for(int i = 0; i < args.Count; ++i)
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
                return StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
            }

            var callSite = new CallSite(returnTypeReference);
            callSite.CallingConvention = methodCallingConvention;
            foreach (var parameterType in parameterTypesArray)
            {
                callSite.Parameters.Add(new ParameterDefinition(parameterType));
            }

            {
                var nop = StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction, Analysis);
                var calli = Instruction.Create(OpCodes.Calli, callSite);
                StackAnalyser.ReplaceInstruction(ilProcessor, nop, calli);
                return calli.Next;
            }
        }
    }
}
