using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Loom
{
    public static class References
    {
        private static object FindSymbol(ModuleDefinition module, string name)
        {
            var assemblies = new List<AssemblyDefinition>();

            assemblies.Add(module.Assembly);
            foreach (var reference in module.AssemblyReferences)
            {
                assemblies.Add(module.AssemblyResolver.Resolve(reference));
            }

            foreach (var assembly in assemblies)
            {
                foreach (var reference in assembly.Modules)
                {
                    foreach (var type in reference.GetTypes())
                    {
                        if (type.FullName == name)
                        {
                            if (reference != module)
                            {
                                return module.Import(type);
                            }
                            return type;
                        }

                        foreach (var method in type.Methods)
                        {
                            if (method.FullName == name)
                            {
                                if (reference != module)
                                {
                                    return module.Import(method);
                                }
                                return method;
                            }
                        }

                        foreach (var field in type.Fields)
                        {
                            if (field.FullName == name)
                            {
                                if (reference != module)
                                {
                                    return module.Import(field);
                                }
                                return field;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static FieldReference FindField(ModuleDefinition module, string name)
        {
            var field = FindSymbol(module, name);

            if (field == null)
            {
                throw new Exception(string.Format("Field {0} not found.", name));
            }

            return (FieldReference)field;
        }

        public static TypeReference FindType(ModuleDefinition module, string name)
        {
            var ty = FindSymbol(module, name);

            if (ty == null)
            {
                throw new Exception(string.Format("Type {0} not found.", name));
            }

            return (TypeReference)ty;
        }

        public static MethodReference FindMethod(ModuleDefinition module, string name)
        {
            var method = FindSymbol(module, name);

            if (method == null)
            {
                throw new Exception(string.Format("Method {0} not found.", name));
            }

            return (MethodReference)method;
        }
    }
}
