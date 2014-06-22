using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Silk.Loom
{
    public abstract class MethodVisitor
    {
        private bool definitionsOnly;
        protected StackAnalyser Analyser { get; private set; }

        public MethodVisitor(bool definitionsOnly)
        {
            this.definitionsOnly = definitionsOnly;
        }

        public void Visit(Mono.Cecil.AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
            {
                Analyser = new StackAnalyser(module);

                foreach (var type in module.GetTypes())
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.HasBody || !definitionsOnly)
                        {
                            Visit(method);
                        }
                    }
                    foreach (var property in type.Properties)
                    {
                        Visit(property);
                    }
                }
            }
        }

        void Visit(Mono.Cecil.PropertyDefinition property)
        {
            if (property.GetMethod != null)
            {
                if (property.GetMethod.HasBody || !definitionsOnly)
                {
                    Visit(property.GetMethod);
                }
            }
            if (property.SetMethod != null)
            {
                if (property.SetMethod.HasBody || !definitionsOnly)
                {
                    Visit(property.SetMethod);
                }
            }
        }

        protected abstract void Visit(Mono.Cecil.MethodDefinition method);
    }
}
