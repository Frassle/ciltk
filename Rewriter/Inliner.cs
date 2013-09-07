using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    class Inliner : InstructionVisitor
    {
        protected override int Visit(Mono.Cecil.Cil.ILProcessor ilProcessor, Mono.Cecil.Cil.Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var method = instruction.Operand as MethodReference;
                var definition = method.Resolve();

                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName == "Silk.InlineAttribute"))
                {
                    var insertAfter = instruction.Previous;
                    ilProcessor.Remove(instruction);
                    return InlineMethod(ilProcessor, insertAfter, definition) - 1;
                }
            }

            return 0;
        }

        int InlineMethod(ILProcessor ilProcessor, Instruction insertAfter, MethodDefinition methodToInline)
        {
            var callingMethod = ilProcessor.Body.Method;

            foreach (var local in methodToInline.Body.Variables)
            {
                callingMethod.Body.Variables.Add(new VariableDefinition(methodToInline.Name + "@" + local.Name, local.VariableType));
            }

            foreach (var instruction in methodToInline.Body.Instructions)
            {
                Instruction nextInstruction;
                if (instruction.Operand == null)
                {
                    nextInstruction = Instruction.Create(instruction.OpCode);
                }
                else
                {
                    if (instruction.OpCode.OperandType == OperandType.InlineField)
                    {
                        nextInstruction = Instruction.Create(
                            instruction.OpCode, instruction.Operand as FieldReference);
                    }
                    if (instruction.OpCode.OperandType == OperandType.InlineMethod)
                    {
                        nextInstruction = Instruction.Create(
                            instruction.OpCode,  instruction.Operand as MethodReference);
                    }
                    if (instruction.OpCode.OperandType == OperandType.InlineType)
                    {
                        nextInstruction = Instruction.Create(
                            instruction.OpCode, instruction.Operand as TypeReference);
                    }
                    if (instruction.OpCode.OperandType == OperandType.InlineVar)
                    {
                        var variable = instruction.Operand as VariableReference;

                        var newVariable = callingMethod.Body.Variables.First(
                            var => var.Name == methodToInline.Name + "@" + variable.Name);

                        nextInstruction = Instruction.Create(instruction.OpCode, newVariable);
                    }

                    nextInstruction = null;
                }

                ilProcessor.InsertAfter(insertAfter, nextInstruction);
                insertAfter = nextInstruction;
            }

            return methodToInline.Body.Instructions.Count;
        }
    }
}
