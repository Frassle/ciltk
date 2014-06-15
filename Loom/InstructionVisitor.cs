using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Microsoft.FSharp.Collections;

namespace Silk.Loom
{
    public abstract class InstructionVisitor : MethodVisitor
    {
        public InstructionVisitor()
            : base(true)
        {
        }

        protected Dictionary<Instruction, FSharpList<Tuple<Instruction, StackAnalyser.StackEntry>>> Analysis;

        protected override void Visit(MethodDefinition method)
        {
            if (method.HasBody)
            {
                var body = method.Body;
                var il = body.GetILProcessor();
                il.Body.SimplifyMacros();

                Analysis = null;

                Instruction instruction = body.Instructions[0];

                while (instruction != null)
                {
                    if (ShouldVisit(instruction))
                    {
                        if (Analysis == null)
                        {
                            Analysis = StackAnalyser.Analyse(method);
                        }
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
