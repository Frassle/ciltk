using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Rocks;

namespace Weave
{
    class CilReplacer : InstructionVisitor
    {
        LabelReplacer Labels;

        public CilReplacer(LabelReplacer labels)
        {
            Labels = labels;
        }

        protected override int Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var method = instruction.Operand as MethodReference;

                if (method != null && method.DeclaringType.FullName == "Silk.Cil")
                {
                    if (method.Name == "KeepAlive")
                    {
                        ilProcessor.Remove(instruction.Previous);
                        ilProcessor.Remove(instruction);
                        return -2;
                    }
                    else
                    {
                        return ReplaceInstruction(ilProcessor, instruction, method);
                    }
                }
            }

            return 0;
        }

        int ReplaceInstruction(ILProcessor ilProcessor, Instruction instruction, MethodReference method)
        {
            Instruction newInstruction = null;
            int instructionChanges = 0;
            
            var opcodeField = typeof(OpCodes).GetFields().First(info => info.Name == method.Name);
            var maybeOpcode = opcodeField == null ? null : (OpCode?)opcodeField.GetValue(null);

            if (maybeOpcode.HasValue)
            {
                var opcode = maybeOpcode.Value;

                if (method is GenericInstanceMethod)
                {
                    var generic_method = method as GenericInstanceMethod;
                    var typetok = generic_method.GenericArguments[0];
                    newInstruction = Instruction.Create(opcode, typetok);
                }
                else
                {
                    if (method.Parameters.Count == 0)
                    {
                        newInstruction = Instruction.Create(opcode);
                    }
                    else
                    {
                        object operand = GetOperand(instruction);
                        ilProcessor.Remove(instruction.Previous);
                        instructionChanges = -1;

                        if (opcode.OperandType == OperandType.InlineVar)
                        {
                            var variable = ilProcessor.Body.Variables[(int)operand];
                            newInstruction = Instruction.Create(opcode, variable);
                        }
                        else if (opcode.OperandType == OperandType.InlineArg)
                        {
                            var variable = ilProcessor.Body.Method.Parameters[(int)operand];
                            newInstruction = Instruction.Create(opcode, variable);
                        }
                        else if (opcode.OperandType == OperandType.InlineBrTarget)
                        {
                            var jump = Labels.GetJumpLocation(ilProcessor.Body.Method, (string)operand);
                            newInstruction = Instruction.Create(opcode, jump);
                        }
                        else if (opcode.OperandType == OperandType.ShortInlineI)
                        {
                            var integer = (byte)operand;
                            newInstruction = Instruction.Create(opcode, integer);
                        }
                        else if (opcode.OperandType == OperandType.InlineI)
                        {
                            var integer = (int)operand;
                            newInstruction = Instruction.Create(opcode, integer);
                        }
                        else if (opcode.OperandType == OperandType.InlineI8)
                        {
                            var integer = (long)operand;
                            newInstruction = Instruction.Create(opcode, integer);
                        }
                        else if (opcode.OperandType == OperandType.ShortInlineR)
                        {
                            var real = (float)operand;
                            newInstruction = Instruction.Create(opcode, real);
                        }
                        else if (opcode.OperandType == OperandType.InlineR)
                        {
                            var real = (double)operand;
                            newInstruction = Instruction.Create(opcode, real);
                        }
                        else if (opcode.OperandType == OperandType.InlineType)
                        {
                            Console.WriteLine("Inline type opcode ({0}) without generic argument.", opcode);   
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Unknown opcode {0}, replacing with NOP.", method.Name);
                newInstruction = Instruction.Create(OpCodes.Nop);
            }

            ilProcessor.Replace(instruction, newInstruction);
            return instructionChanges;
        }

        private object GetOperand(Instruction instruction)
        {
            var ld = instruction.Previous;

            if (ld.OpCode == OpCodes.Ldstr)
            {
                return ld.Operand as string;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4)
            {
                return (int)ld.Operand;
            }
            else if (ld.OpCode == OpCodes.Ldc_I8)
            {
                return (long)ld.Operand;
            }
            else if (ld.OpCode == OpCodes.Ldc_R4)
            {
                return (float)ld.Operand;
            }
            else if (ld.OpCode == OpCodes.Ldc_R8)
            {
                return (double)ld.Operand;
            }

            return null;
        }
    }
}
