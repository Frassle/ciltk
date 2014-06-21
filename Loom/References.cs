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

        static Dictionary<string, string> PrimativeTypeMap;

        static References()
        {
            PrimativeTypeMap = new Dictionary<string, string>();
            PrimativeTypeMap.Add("bool", "System.Boolean");
            PrimativeTypeMap.Add("char", "System.Char");
            PrimativeTypeMap.Add("float32", "System.Single");
            PrimativeTypeMap.Add("float64", "System.Double");
            PrimativeTypeMap.Add("int8", "System.SByte");
            PrimativeTypeMap.Add("int16", "System.Int16");
            PrimativeTypeMap.Add("int32", "System.Int32");
            PrimativeTypeMap.Add("int64", "System.Int64");
            PrimativeTypeMap.Add("native int", "System.IntPtr");
            PrimativeTypeMap.Add("native unsigned int", "System.UIntPtr");
            PrimativeTypeMap.Add("object", "System.Object");
            PrimativeTypeMap.Add("string", "System.String");
            PrimativeTypeMap.Add("typedref", "System.TypedReference");
            PrimativeTypeMap.Add("unsigned int8", "System.Byte");
            PrimativeTypeMap.Add("unsigned int16", "System.UInt16");
            PrimativeTypeMap.Add("unsigned int32", "System.UInt32");
            PrimativeTypeMap.Add("unsigned int64", "System.UInt64");
            PrimativeTypeMap.Add("void", "System.Void");
        }
        
        public static TypeReference FindType(ModuleDefinition module, MethodBody method, string name)
        {
            if (name.EndsWith("]"))
            {
                var matchArray = System.Text.RegularExpressions.Regex.Match(name, "(.*?)\\[(,*)\\]");

                // array type
                if (matchArray.Success)
                {
                    var element_type = FindType(module, method, matchArray.Groups[1].Value);
                    return new ArrayType(element_type, matchArray.Groups[2].Length + 1);
                }
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
                    if (String.Equals(generic.FullName, name, StringComparison.Ordinal))
                        return generic;
                }

                foreach (var generic in method.Method.DeclaringType.GenericParameters)
                {
                    if (String.Equals(generic.FullName, name, StringComparison.Ordinal))
                        return generic;
                }
            }

            // primative type
            string realName;
            if (PrimativeTypeMap.TryGetValue(name, out realName))
            {
                name = realName;
            }

            // normal type
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
                    foreach (var type in reference.Types)
                    {
                        var foundType = MatchType(name, type);
                        if (foundType != null)
                        {
                            return module.Import(type);
                        }
                    }
                }
            }

            throw new Exception(string.Format("Type {0} not found.", name));
        }

        private static TypeDefinition MatchType(string name, TypeDefinition type)
        {
            if (String.Equals(type.FullName, name, StringComparison.Ordinal))
            {
                return type;
            }
            else if (name.StartsWith(type.FullName, StringComparison.Ordinal)) // could be a subtype
            {
                foreach (var subtype in type.NestedTypes)
                {
                    var foundType = MatchType(name, subtype);
                    if (foundType != null)
                    {
                        return foundType;
                    }
                }
            }

            return null;
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
