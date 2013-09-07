using Mono.Cecil;
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

        protected override int Visit(ILProcessor ilProcessor, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Call)
            {
                var method = instruction.Operand as MethodReference;

                if (method != null && method.DeclaringType.FullName == "Silk.Cil" && method.Name == "Label")
                {
                    return ReplaceLabel(ilProcessor, instruction);
                }
            }

            return 0;
        }

        int ReplaceLabel(ILProcessor ilProcessor, Instruction instruction)
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

            var nop = Instruction.Create(OpCodes.Nop);

            ilProcessor.Remove(instruction.Previous);
            ilProcessor.Replace(instruction, nop);
            Labels.Add(Tuple.Create(CurrentMethod, label), nop);
            return -1;
        }
    }
}
