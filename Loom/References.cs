using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Loom
{
    public static class References
    {
        public static FieldReference FindField(ModuleDefinition module, MethodBody method, string name)
        {
            var matches = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*)");

            if (!matches.Success)
            {
                throw new Exception(string.Format("Field \"{0}\" is not in the correct format (return_type declaring_type::field_name).", name));
            }

            var type = FindType(module, method, matches.Groups[2].Value).Resolve();

            foreach (var field in type.Fields)
            {
                if (field.Name == matches.Groups[3].Value)
                {
                    return module.Import(field);
                }
            }

            throw new Exception(string.Format("Field {0} not found.", name));
        }
        
        public static TypeReference FindType(ModuleDefinition module, MethodBody method, string name)
        {
            var matchArray = System.Text.RegularExpressions.Regex.Match(name, "(.*?)\\[(,*)\\]");

            // array type
            if (matchArray.Success)
            {
                var element_type = FindType(module, method, matchArray.Groups[1].Value);
                return new ArrayType(element_type, matchArray.Groups[2].Length + 1);
            }

            // ref type
            if (name.EndsWith("&"))
            {
                var element_type = FindType(module, method, name.Substring(0, name.Length - 1));
                return new ByReferenceType(element_type);
            }

            // pointer type
            if (name.EndsWith("*"))
            {
                var element_type = FindType(module, method, name.Substring(0, name.Length - 1));
                return new PointerType(element_type);
            }

			// pinned type
			if (name.EndsWith(" pinned"))
			{
				var element_type = FindType(module, method, name.Substring(0, name.Length - " pinned".Length));
				return new PinnedType(element_type); 
			}

            // generic type
            if (method != null)
            {
                foreach (var generic in method.Method.GenericParameters)
                {
                    if (generic.FullName == name)
                        return generic;
                }

                foreach (var generic in method.Method.DeclaringType.GenericParameters)
                {
                    if (generic.FullName == name)
                        return generic;
                }
            }

            // c# type
            if (name == "byte")
                name = "System.Byte";
            else if (name == "ushort")
                name = "System.UInt16";
            else if (name == "uint")
                name = "System.UInt32";
            else if (name == "ulong")
                name = "System.UInt64";
            else if (name == "sbyte")
                name = "System.SByte";
            else if (name == "short")
                name = "System.Int16";
            else if (name == "int")
                name = "System.Int32";
            else if (name == "long")
                name = "System.Int64";
            else if (name == "float")
                name = "System.Single";
            else if (name == "double")
                name = "System.Double";
            else if (name == "string")
                name = "System.String";
            else if (name == "object")
                name = "System.Object";
            else if (name == "char")
                name = "System.Char";
            else if (name == "bool")
                name = "System.Boolean";
            else if (name == "void")
                name = "System.Void";

            // normal type
            var assemblies = new List<AssemblyDefinition>();

            assemblies.Add(module.Assembly);
            foreach (var reference in module.AssemblyReferences)
            {
                assemblies.Add(module.AssemblyResolver.Resolve(reference));
            }

            TypeReference maybeMatch = null;
            int maybeCount = 0;
            foreach (var assembly in assemblies)
            {
                foreach (var reference in assembly.Modules)
                {
                    foreach (var type in reference.GetTypes())
                    {
                        if (type.FullName == name)
                        {
                            return module.Import(type);
                        }
                        if (type.Name == name)
                        {
                            maybeMatch = type;
                            ++maybeCount;
                        }
                    }
                }
            }

            if (maybeCount == 1)
            {
                return module.Import(maybeMatch);
            }

            throw new Exception(string.Format("Type {0} not found.", name));
        }

        public static MethodReference FindMethod(ModuleDefinition module, MethodBody method, string name)
        {
            var matches = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*?)\\((.*?)\\)");

            if (!matches.Success)
            {
                throw new Exception(string.Format("Method \"{0}\" is not in the correct format (return_type declaring_type::method_name(method_params).", name));
            }

            var declaring_type = FindType(module, method, matches.Groups[2].Value);

            var return_type = FindType(module, method, matches.Groups[1].Value);

            var parameters = matches.Groups[4].Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(paramter_type => FindType(module, method, paramter_type));

            var mref = new MethodReference(matches.Groups[3].Value, return_type, declaring_type);
            foreach(var param in parameters)
            {
                mref.Parameters.Add(new ParameterDefinition(param));
            }

            return module.Import(mref);
        }
    }
}
