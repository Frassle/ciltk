using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Loom
{
    public sealed class BasicBlock
    {
        private HashSet<BasicBlock> _in, _out;

        public BasicBlock(Instruction start, Instruction end)
        {
            Start = start;
            End = end;
            _in = new HashSet<BasicBlock>();
            _out = new HashSet<BasicBlock>();
        }

        public Instruction Start { get; private set; }
        public Instruction End { get; private set; }

        public ICollection<BasicBlock> In { get { return _in; } }
        public ICollection<BasicBlock> Out { get { return _out; } }
    }

    public sealed class ControlFlowAnalysis
    {
        public static BasicBlock Analyse(MethodDefinition method)
        {
            var basicBlocks = new HashSet<BasicBlock>();
            var starts = new List<Instruction>();
            var ends = new List<Instruction>();

            method.Body.SimplifyMacros();

            starts.Add(method.Body.Instructions.First());
            ends.Add(method.Body.Instructions.Last());

            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode.FlowControl == FlowControl.Branch)
                {
                    ends.Add(instruction);
                    starts.Add(instruction.Operand as Instruction);
                }

                if (instruction.OpCode.FlowControl == FlowControl.Return)
                {
                    ends.Add(instruction);
                }

                if (instruction.OpCode.FlowControl == FlowControl.Cond_Branch)
                {
                    ends.Add(instruction);
                    starts.Add(instruction.Next);
                    starts.Add(instruction.Operand as Instruction);
                }

                if (instruction.OpCode.FlowControl == FlowControl.Break)
                {
                }
            }

            var pairs =
                starts.Distinct().OrderBy(instruction => instruction.Offset).Zip(
                ends.Distinct().OrderBy(instruction => instruction.Offset), (a, b) => Tuple.Create(a, b));

            foreach (var pair in pairs)
            {
                basicBlocks.Add(new BasicBlock(pair.Item1, pair.Item2));
            }

            return basicBlocks.First(block => block.Start.Offset == 0);
        }
    }
}
