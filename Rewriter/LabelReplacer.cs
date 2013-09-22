﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    class LabelReplacer : InstructionVisitor
    {
        Dictionary<Tuple<MethodDefinition, string>, Instruction> Labels;

        public LabelReplacer()
        {
            Labels = new Dictionary<Tuple<MethodDefinition, string>, Instruction>();
        }

        public Instruction GetJumpLocation(MethodDefinition method, string label)
        {
            Instruction instruction;
            if (!Labels.TryGetValue(Tuple.Create(method, label), out instruction))
            {
                Console.WriteLine("Label {0} in method {1} not found.", label, method.FullName);
                Environment.Exit(1);
            }
            return instruction;
        }

        protected override Instruction Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var method = instruction.Operand as MethodReference;

                if (method != null && method.DeclaringType.FullName == "Silk.Cil" && method.Name == "Label")
                {
                    return ReplaceLabel(ilProcessor, instruction);
                }
            }

            return instruction.Next;
        }

        Instruction ReplaceLabel(ILProcessor ilProcessor, Instruction instruction)
        {
            var ld = instruction.Previous;

            if (ld.OpCode != OpCodes.Ldstr)
            {
                Console.WriteLine("Label call must be used with a string literal.");
                Environment.Exit(1);
            }

            var label = ld.Operand as string;
            if (label == null)
            {
                Console.WriteLine("Label call must be used with a string literal.");
                Environment.Exit(1);
            }

            ilProcessor.Remove(instruction.Previous);

            var jump_location = Instruction.Create(OpCodes.Nop);
            ilProcessor.Replace(instruction, jump_location);
            Labels.Add(Tuple.Create(CurrentMethod, label), jump_location);

            return jump_location.Next;
        }
    }
}
