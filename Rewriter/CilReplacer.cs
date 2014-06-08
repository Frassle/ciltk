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
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction);
            }
            else if (calledMethod.Name == "Load")
            {
                /*
                 * The compiler will have inserted the appropriate load 
                 * instructions to put the value on the operand stack in
                 * preperation to call Load<T>. Thus all we have to do is 
                 * remove the call instruction, that keeps the value on the
                 * stack instead of popping it for the call.
                 */
                ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Nop));
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
            else
            {
                return ReplaceInstruction(ilProcessor, instruction, calledMethod);
            }

            return next;
        }

        Instruction ReplaceStore(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var generic_method = calledMethod as GenericInstanceMethod;
            var typetok = generic_method.GenericArguments[0];

            var stack = StackAnalyser.Analyse(ilProcessor.Body.Method)[instruction.Previous];

            var operandEntry = stack.Pop();
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

            ilProcessor.Replace(addr_instruction, Instruction.Create(OpCodes.Nop));
            ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Nop));

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
                // Special case ldelem because we don't want to call it Stelem_Any
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
                ilProcessor.Replace(instruction, Instruction.Create(opcode, typetok));
            }
            else
            {
                if (calledMethod.Parameters.Count == 0)
                {
                    ilProcessor.Replace(instruction, Instruction.Create(opcode));
                }
                if (calledMethod.Parameters.Count == 1)
                {
                    var stack = StackAnalyser.Analyse(ilProcessor.Body.Method)[instruction.Previous];

                    var operandEntry = stack.Pop();
                    StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, operandEntry.Item1);

                    if (!operandEntry.Item2.IsConstant)
                    {
                        Console.WriteLine("Inline ({0}) without constant argument, ignoring.", opcode);
                        StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction);
                        return nextInstruction;
                    }

                    var operand = operandEntry.Item2.Value;

                    if (opcode.OperandType == OperandType.InlineVar)
                    {
                        var variable = ilProcessor.Body.Variables[(int)operand];
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, variable));
                    }
                    else if (opcode.OperandType == OperandType.InlineArg)
                    {
                        var variable = ilProcessor.Body.Method.Parameters[(int)operand];
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, variable));
                    }
                    else if (opcode.OperandType == OperandType.InlineBrTarget)
                    {
                        var jump = Labels.GetJumpLocation(ilProcessor.Body, (string)operand);
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, jump));
                    }
                    else if (opcode.OperandType == OperandType.ShortInlineI)
                    {
                        var integer = (byte)operand;
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.InlineI)
                    {
                        var integer = (int)operand;
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.InlineI8)
                    {
                        var integer = (long)operand;
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, integer));
                    }
                    else if (opcode.OperandType == OperandType.ShortInlineR)
                    {
                        var real = (float)operand;
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, real));
                    }
                    else if (opcode.OperandType == OperandType.InlineR)
                    {
                        var real = (double)operand;
                        ilProcessor.Replace(instruction, Instruction.Create(opcode, real));
                    }
                    else if (opcode.OperandType == OperandType.InlineSwitch)
                    {
                        var target_string = (string)operand;
                        var targets = target_string.Split(';').Select(label => Labels.GetJumpLocation(ilProcessor.Body, label)).ToArray();

                        ilProcessor.Replace(instruction, Instruction.Create(opcode, targets));
                    }
                    else if (opcode.OperandType == OperandType.InlineField)
                    {
                        var module = ilProcessor.Body.Method.Module;
                        var field = (string)operand;
                        var fieldref = Silk.Loom.References.FindField(module, field);

                        ilProcessor.Replace(instruction, Instruction.Create(opcode, fieldref));
                    }
                    else if (opcode.OperandType == OperandType.InlineMethod)
                    {
                        var module = ilProcessor.Body.Method.Module;
                        var method = (string)operand;
                        var methodref = Silk.Loom.References.FindMethod(module, method);

                        ilProcessor.Replace(instruction, Instruction.Create(opcode, methodref));
                    }
                    else if (opcode.OperandType == OperandType.InlineType)
                    {
                        Console.WriteLine("Inline type opcode ({0}) without generic argument, ignoring.", opcode);
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
            var stack = StackAnalyser.Analyse(ilProcessor.Body.Method)[instruction.Previous];
            Tuple<Instruction, StackAnalyser.StackEntry> entry;

            var module = ilProcessor.Body.Method.Module;

            var args = new List<StackAnalyser.StackEntry>();
            for (int i = 0; i < calledMethod.Parameters.Count - 2; ++i)
            {
                entry = stack.Pop();
                args.Add(entry.Item2);
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, entry.Item1);
            }
            args.Reverse();

            entry = stack.Pop();
            var returnType = entry.Item2;
            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, entry.Item1);

            entry = stack.Pop();
            var callingConvention = entry.Item2;
            StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, entry.Item1);

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
                        methodCallingConvention = MethodCallingConvention.Generic; break;
                    default:
                        methodCallingConvention = MethodCallingConvention.Default; break;
                }
            }
            else
            {
                Console.WriteLine("Calling convention passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction);
                return next;
            }

            TypeReference returnTypeReference;
            if (returnType.IsConstant)
            {
                Type retTy = returnType.Value;
                returnTypeReference = module.Import(retTy);
            }
            else
            {
                Console.WriteLine("Return type passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction);
                return next;
            }

            TypeReference[] parameterTypesArray;
            if (args.All(arg => arg.IsConstant))
            {
                parameterTypesArray = args.Select(arg => module.Import((Type)arg.Value)).ToArray();
            }
            else
            {
                Console.WriteLine("Type passed to Calli is not a constant expression.");
                var next = instruction.Next;
                StackAnalyser.RemoveInstructionChain(ilProcessor.Body.Method, instruction);
                return next;
            }

            var callSite = new CallSite(returnTypeReference);
            callSite.CallingConvention = methodCallingConvention;
            foreach (var parameterType in parameterTypesArray)
            {
                callSite.Parameters.Add(new ParameterDefinition(parameterType));
            }

            {
                var next = instruction.Next;
                ilProcessor.Replace(instruction, Instruction.Create(OpCodes.Calli, callSite));
                return next;
            }
        }
    }
}
