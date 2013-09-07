using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weave
{
    abstract class InstructionVisitor
    {
        protected MethodDefinition CurrentMethod
        {
            get;
            private set;
        }

        public void Visit(Mono.Cecil.AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
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
                CurrentMethod = method;
                var body = method.Body;
                var il = body.GetILProcessor();

                for (int i = 0; i < body.Instructions.Count; ++i)
                {
                    i += Visit(il, body.Instructions[i]);
                }
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

        protected abstract int Visit(Mono.Cecil.Cil.ILProcessor ilProcessor, Mono.Cecil.Cil.Instruction instruction);
    }
}
