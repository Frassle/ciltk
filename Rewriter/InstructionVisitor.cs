using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    abstract class InstructionVisitor
    {
        protected ModuleDefinition CurrentModule
        {
            get;
            private set;
        }

        protected MethodDefinition CurrentMethod
        {
            get;
            private set;
        }

        protected TypeDefinition CurrentType
        {
            get;
            private set;
        }

        public void Visit(Mono.Cecil.AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                CurrentModule = module;
                foreach (var type in module.Types)
                {
                    CurrentType = type;
                    foreach (var method in type.Methods)
                    {
                        Visit(method);
                    }
                    foreach (var property in type.Properties)
                    {
                        Visit(property);
                    }
                    CurrentType = null;
                }
                CurrentModule = null;
            }
        }

        void Visit(Mono.Cecil.MethodDefinition method)
        {
            if (method.HasBody)
            {
                CurrentMethod = method;
                var body = method.Body;
                var il = body.GetILProcessor();
                il.Body.SimplifyMacros();

                Instruction instruction = body.Instructions[0];
                while (instruction != null)
                {
                    instruction = Visit(il, instruction);
                }

                il.Body.OptimizeMacros();
                CurrentMethod = null;
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

        protected abstract Instruction Visit(Mono.Cecil.Cil.ILProcessor ilProcessor, Mono.Cecil.Cil.Instruction instruction);
    }
}
