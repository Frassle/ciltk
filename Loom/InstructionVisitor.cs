using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Silk.Loom
{
    public abstract class InstructionVisitor : MethodVisitor
    {
        public InstructionVisitor()
            : base(true)
        {
        }

        protected override void Visit(MethodDefinition method)
        {
            if (method.HasBody)
            {
                var body = method.Body;
                var il = body.GetILProcessor();
                il.Body.SimplifyMacros();

                Instruction instruction = body.Instructions[0];

                while (instruction != null)
                {
                    if (ShouldVisit(instruction))
                    {
                        instruction = Visit(il, instruction);
                    }
                    else
                    {
                        instruction = instruction.Next;
                    }
                }

                il.Body.OptimizeMacros();
            }
        }

        protected virtual bool ShouldVisit(Mono.Cecil.Cil.Instruction instruction)
        {
            return true;
        }

        protected abstract Instruction Visit(Mono.Cecil.Cil.ILProcessor ilProcessor, Mono.Cecil.Cil.Instruction instruction);
    }
}
