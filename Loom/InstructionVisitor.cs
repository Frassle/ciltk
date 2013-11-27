using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Silk.Loom
{
    public abstract class InstructionVisitor
    {
        public void Visit(Mono.Cecil.AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    foreach (var method in type.Methods)
                    {
                        Visit(method);
                    }
                    foreach (var property in type.Properties)
                    {
                        Visit(property);
                    }
                }
            }
        }

        void Visit(Mono.Cecil.MethodDefinition method)
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

        void Visit(Mono.Cecil.PropertyDefinition property)
        {
            if (property.GetMethod != null)
            {
                Visit(property.GetMethod);
            }
            if (property.SetMethod != null)
            {
                Visit(property.SetMethod);
            }
        }

        protected virtual bool ShouldVisit(Mono.Cecil.Cil.Instruction instruction)
        {
            return true;
        }

        protected abstract Instruction Visit(Mono.Cecil.Cil.ILProcessor ilProcessor, Mono.Cecil.Cil.Instruction instruction);
    }
}
