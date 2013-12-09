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
                ilProcessor.Remove(instruction.Previous);
                ilProcessor.Remove(instruction);
            }
            else if (calledMethod.Name.StartsWith("Declare"))
            {
                AddVariable(ilProcessor, instruction, calledMethod);
            }
            else if (calledMethod.Name == "Load")
            {
                /*
                * The compiler will have inserted the appropriate load instructions to put the value on the 
                * operand stack in preperation to call Load<T>. Thus all we have to do is remove the call instruction,
                * that keeps the value on the stack instead of popping it for the call.
                */
                ilProcessor.Remove(instruction);
            }
            else if (calledMethod.Name == "Store")
            {
                /*
                * The compiler will have inserted instructions to load the addr of the location
                * we want to store to. We need to look at these instructions and replace them
                * with the appropriate standard store instruction. We then remove the call to Store.
                */

                return ReplaceStore(ilProcessor, instruction, calledMethod);
            }
            else
            {
                return ReplaceInstruction(ilProcessor, instruction, calledMethod);
            }

            return next;
        }

        private void AddVariable(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            string name = instruction.Previous.Operand as string;
            ilProcessor.Remove(instruction.Previous);

            var generic_method = calledMethod as GenericInstanceMethod;
            var typetok = generic_method.GenericArguments[0];

            VariableDefinition variable;
            if (calledMethod.Name == "DeclarePinnedVariable")
            {
                variable = new VariableDefinition(name, new PinnedType(typetok));
            }
            else
            {
                variable = new VariableDefinition(name, typetok);
            }

            ilProcessor.Body.Variables.Add(variable);
        }

        Instruction ReplaceStore(ILProcessor ilProcessor, Instruction instruction, MethodReference calledMethod)
        {
            var generic_method = calledMethod as GenericInstanceMethod;
            var typetok = generic_method.GenericArguments[0];

            var addr_instruction = instruction.Previous;
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

            ilProcessor.Remove(addr_instruction);
            ilProcessor.Remove(instruction);

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
                else
                {
                    object operand = instruction.Previous.Operand;
                    ilProcessor.Remove(instruction.Previous);

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
                    else if (opcode.OperandType == OperandType.InlineType)
                    {
                        Console.WriteLine("Inline type opcode ({0}) without generic argument, ignoring.", opcode);
                        ilProcessor.Remove(instruction);
                    }
                }
            }

            return nextInstruction;
        }
    }
}
