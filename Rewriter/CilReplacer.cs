using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                        else if (method.Name == "Ldc_I4")
                        {
                            newInstruction = ShortenLdc_I4((int)operand);
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
            else if (ld.OpCode == OpCodes.Ldc_I4_S)
            {
                return (int)(sbyte)ld.Operand;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_M1)
            {
                return -1;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_0)
            {
                return 0;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_1)
            {
                return 1;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_2)
            {
                return 2;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_3)
            {
                return 3;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_4)
            {
                return 4;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_5)
            {
                return 5;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_6)
            {
                return 6;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_7)
            {
                return 7;
            }
            else if (ld.OpCode == OpCodes.Ldc_I4_8)
            {
                return 8;
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

        private Instruction ShortenLdc_I4(int value)
        {
            switch (value)
            {
                case -1:
                    return Instruction.Create(OpCodes.Ldc_I4_M1);
                case 0:
                    return Instruction.Create(OpCodes.Ldc_I4_0);
                case 1:
                    return Instruction.Create(OpCodes.Ldc_I4_1);
                case 2:
                    return Instruction.Create(OpCodes.Ldc_I4_2);
                case 3:
                    return Instruction.Create(OpCodes.Ldc_I4_3);
                case 4:
                    return Instruction.Create(OpCodes.Ldc_I4_4);
                case 5:
                    return Instruction.Create(OpCodes.Ldc_I4_5);
                case 6:
                    return Instruction.Create(OpCodes.Ldc_I4_6);
                case 7:
                    return Instruction.Create(OpCodes.Ldc_I4_7);
                case 8:
                    return Instruction.Create(OpCodes.Ldc_I4_8);
            }

            if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                return Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)value);
            }
            else
            {
                return Instruction.Create(OpCodes.Ldc_I4, value);
            }      
        }
    }
}
