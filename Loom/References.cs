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

        public static TypeReference FindType(ModuleDefinition module, MemberReference current_scope, string name)
        {
            if (name.EndsWith("]"))
            {
                var matchArray = System.Text.RegularExpressions.Regex.Match(name, "(.*?)\\[(,*)\\]");

                // array type
                if (matchArray.Success)
                {
                    var element_type = FindType(module, current_scope, matchArray.Groups[1].Value);
                    return new ArrayType(element_type, matchArray.Groups[2].Length + 1);
                }
            }

            // ref type
            if (name.EndsWith("&"))
            {
                var element_type = FindType(module, current_scope, name.Substring(0, name.Length - 1));
                return new ByReferenceType(element_type);
            }

            // pointer type
            if (name.EndsWith("*"))
            {
                var element_type = FindType(module, current_scope, name.Substring(0, name.Length - 1));
                return new PointerType(element_type);
            }

			// pinned type
			if (name.EndsWith(" pinned"))
			{
                var element_type = FindType(module, current_scope, name.Substring(0, name.Length - " pinned".Length));
				return new PinnedType(element_type); 
			}

            // generic instance type
            if (name.EndsWith(">"))
            {
                var index = name.IndexOf('<');
                var generic_type = FindType(module, current_scope, name.Substring(0, index));

                var element_types = name.Substring(index + 1, (name.Length - index - 2)).Split(',');
                var arguments = element_types.Select(elem => FindType(module, current_scope, elem)).ToArray();

                return Mono.Cecil.Rocks.TypeReferenceRocks.MakeGenericInstanceType(generic_type, arguments);
            }

            // generic method parameter
            if (name.StartsWith("!!"))
            {
                int index = int.Parse(name.Substring(2));
                return (current_scope as MethodReference).GenericParameters[index];
            }

            // generic type parameter
            if (name.StartsWith("!"))
            {
                int index = int.Parse(name.Substring(1));
                return (current_scope as TypeReference).GenericParameters[index];
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

        public static FieldReference FindField(ModuleDefinition module, MemberReference current_scope, string name)
        {
            var matches = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*)");

            if (!matches.Success)
            {
                throw new Exception(string.Format("Field \"{0}\" is not in the correct format (return_type declaring_type::field_name).", name));
            }

            var type = FindType(module, current_scope, matches.Groups[2].Value).Resolve();

            foreach (var field in type.Fields)
            {
                if (field.Name == matches.Groups[3].Value)
                {
                    return module.Import(field);
                }
            }

            throw new Exception(string.Format("Field {0} not found.", name));
        }

        static string GenericName(MethodDefinition method, string name)
        {
            // generic method parameter
            if (name.StartsWith("!!"))
            {
                int index = int.Parse(name.Substring(2));
                return method.GenericParameters[index].FullName;
            }

            // generic type parameter
            if (name.StartsWith("!"))
            {
                int index = int.Parse(name.Substring(1));
                return method.DeclaringType.GenericParameters[index].FullName;
            }

            return name;
        }

        public static MethodReference FindMethod(ModuleDefinition module, MemberReference current_scope, string name)
        {
            var matches = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*?)\\((.*?)\\)");

            if (!matches.Success)
            {
                throw new Exception(string.Format("Method \"{0}\" is not in the correct format (return_type declaring_type::method_name(method_params).", name));
            }

            var return_type_name = matches.Groups[1].Value;
            var declaring_type_name = matches.Groups[2].Value;
            var method_name = matches.Groups[3].Value;
            var parameters = matches.Groups[4].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // generic instance method
            if(method_name.EndsWith(">"))
            {
                var index = name.IndexOf('<');
                var index_last = name.LastIndexOf('>');
                var generic_method = FindMethod(module, current_scope, name.Substring(0, index) + name.Substring(index_last + 1));

                var element_types = name.Substring(index + 1, index_last - index - 1).Split(',');
                var arguments = element_types.Select(elem => FindType(module, current_scope, elem)).ToArray();

                var gim = new GenericInstanceMethod(generic_method);
                foreach (var arg in arguments)
                {
                    gim.GenericArguments.Add(arg);
                }
                return gim;
            }
            else
            {
                var declaring_type = FindType(module, current_scope, declaring_type_name);

                // resolving loses all generic information, we have to recreate the method reference later
                // with the original declaring type.
                var declaring_type_def = declaring_type.Resolve();

                foreach (var method in declaring_type_def.Methods)
                {
                    if (method.Name == method_name && method.Parameters.Count == parameters.Length)
                    {
                        var return_type_match = method.ReturnType.FullName == GenericName(method, return_type_name);

                        var parameters_match = Enumerable.Zip(
                            method.Parameters, parameters,
                            (param, param_name) => param.ParameterType.FullName == GenericName(method, param_name))
                            .All(b => b);

                        if (return_type_match && parameters_match)
                        {
                            var method_refernce = module.Import(method);

                            // replace any generic info, safe to do even if declaring_type and declaring_type_def are the same.
                            var generic_method = new MethodReference(
                                method_refernce.Name,
                                method_refernce.ReturnType,
                                declaring_type)
                                {
                                    CallingConvention = method_refernce.CallingConvention,
                                    ExplicitThis = method_refernce.ExplicitThis,
                                    HasThis = method_refernce.HasThis
                                };

                            foreach (var param in method_refernce.Parameters)
                            {
                                generic_method.Parameters.Add(param);
                            } 
                            
                            foreach (var param in method_refernce.GenericParameters)
                            {
                                generic_method.GenericParameters.Add(param);
                            }

                            return generic_method;
                        }
                    }
                }                
            }
            throw new Exception(string.Format("Method {0} not found.", name));
        }

        public static PropertyReference FindProperty(ModuleDefinition module, MemberReference current_scope, string name)
        {
            var matches_indexer = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*?)\\((.*?)\\)");
            var matches_property = System.Text.RegularExpressions.Regex.Match(name, "(.*?) (.*?)::(.*)");

            if (!matches_indexer.Success && !matches_property.Success)
            {
                throw new Exception(string.Format("Property \"{0}\" is not in the correct format (return_type declaring_type::property_name[(property_params)].", name));
            }

            var groupA = matches_indexer.Success ? matches_indexer.Groups[1].Value : matches_property.Groups[1].Value;
            var groupB = matches_indexer.Success ? matches_indexer.Groups[2].Value : matches_property.Groups[2].Value;
            var groupC = matches_indexer.Success ? matches_indexer.Groups[3].Value : matches_property.Groups[3].Value;
            var groupD = matches_indexer.Success ? matches_indexer.Groups[4].Value : null;

            var declaring_type = FindType(module, current_scope, groupB).Resolve();
            var parameters = groupD == null ? new string[] { } : groupD.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var property in declaring_type.Properties)
            {
                if (property.Name == groupC && property.Parameters.Count == parameters.Length)
                {
                    var return_type_match = property.PropertyType.FullName == groupA;

                    var parameters_match = Enumerable.Zip(
                        property.Parameters, parameters, (param, param_name) => param.ParameterType.FullName == param_name)
                        .All(b => b);

                    if (return_type_match && parameters_match)
                    {
                        return property;
                    }
                }
            }

            throw new Exception(string.Format("Property {0} not found.", name));
        }
    }
}
