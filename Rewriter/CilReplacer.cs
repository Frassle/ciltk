using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rewriter
{
    class CilReplacer : InstructionVisitor
    {
        Dictionary<Tuple<MethodDefinition, string>, Instruction> Labels;

        public CilReplacer()
        {
            Labels = new Dictionary<Tuple<MethodDefinition, string>, Instruction>();
        }

        protected override int Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var method = instruction.Operand as MethodReference;

                if (method != null && method.DeclaringType.FullName == "CilTK.Cil")
                {
                    if (method.Name == "Label")
                    {
                        return ReplaceLabel(ilProcessor, instruction);
                    }
                    else
                    {
                        return ReplaceInstruction(ilProcessor, instruction, method);
                    }
                }
            }

            return 0;
        }

        int ReplaceLabel(ILProcessor ilProcessor, Instruction instruction)
        {
            var label = GetOperand(instruction) as string;
            if (label == null)
            {
                Console.WriteLine("Label call must be used with a string literal.");
                Environment.Exit(1);
            }

            var nop = Instruction.Create(OpCodes.Nop);

            ilProcessor.Remove(instruction.Previous);
            ilProcessor.Replace(instruction, nop);
            Labels.Add(Tuple.Create(CurrentMethod, label), nop);
            return -1;
        }

        void ReplaceLoad(ILProcessor ilProcessor, Instruction instruction)
        {
        
        }

        void ReplaceStore(ILProcessor ilProcessor, Instruction instruction)
        {

        }

        int ReplaceInstruction(ILProcessor ilProcessor, Instruction instruction, MethodReference method)
        {
            Instruction newInstruction = null;
            int instructionChanges = 0;
            
            var opcodeField = typeof(OpCodes).GetFields().First(info => info.Name == method.Name);
            var opcode = opcodeField == null ? null : (OpCode?)opcodeField.GetValue(null);

            if (method.Parameters.Count == 0)
            {
                newInstruction = Instruction.Create(opcode.Value);
            }
            else
            {
                object operand = GetOperand(instruction);
                ilProcessor.Remove(instruction.Previous);
                instructionChanges = -1;

                if (instruction.OpCode.OperandType == OperandType.InlineVar
                    || instruction.OpCode.OperandType == OperandType.ShortInlineVar)
                {
                    var variable = ilProcessor.Body.Variables[(int)operand];

                    newInstruction = Instruction.Create(opcode.Value, variable);
                }
                else if(instruction.OpCode.OperandType == OperandType.InlineArg ||
                    instruction.OpCode.OperandType == OperandType.ShortInlineArg)
                {
                    var variable = ilProcessor.Body.Method.Parameters[(int)operand];

                    newInstruction = Instruction.Create(opcode.Value, variable);
                }
                else if (method.Name == "Ldc_I4")
                {
                    newInstruction = ShortenLdc_I4((int)operand);              
                }
                else if (method.Name == "Call")
                {
                    var calleeMethod = operand as MethodReference;
                    newInstruction = Instruction.Create(opcode.Value, calleeMethod);
                }
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
