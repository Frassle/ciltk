using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Loom
{
    public static class References
    {
        struct Name
        {
            public Name(string returntype, string typename, string[] innertypes, 
                string membername, string[] parameters)
            {
                Returntype = returntype;
                Typename = typename;
                Innertypes = innertypes;
                Membername = membername;
                Paramaters = parameters;
            }

            public string Returntype;
            public string Typename;
            public string[] Innertypes;
            public string Membername;
            public string[] Paramaters;            
        }

        static string Substring(this string str, int start, string next, string before, out int index)
        {
            index = str.IndexOf(next, start);
            if (index == -1)
            {
                index = start;
                return "";
            }
            int beforeIndex = str.IndexOf(before);
            if (index < beforeIndex || beforeIndex == -1 || before == "")
            {
                int length = index - start;
                index = start + length + 1;
                return str.Substring(start, length);
            }
            else
            {
                index = start;
                return "";
            }
        }

        static string Substring(this string str, int start, string next, out int index)
        {
            return Substring(str, start, next, "", out index);
        }

        static Name SplitName(string name)
        {
            // Name = Identifer
            // Type = NAMESPACE.Name | Type/Name | Type`Integer
            // Field = Type Type::Name
            // Parameters = "" | Type | Parameters,Type
            // Method = Type Type::Name(Parameters)
            int index;
            var returntype = name.Substring(0, " ", out index);
            var typename = name.Substring(index, "/", "::", out index);
            var innertypes = new List<string>();
            if (typename == "")
            {
                typename = name.Substring(index, "::", out index);
                if (typename == "")
                {
                    typename = name.Substring(index, name.Length - index);
                    index = name.Length;
                }
                else
                {
                    index++;
                }
            }
            else
            {
                while (true)
                {
                    var innername = name.Substring(index, "/", "::", out index).Trim();
                    if (innername == "")
                    {
                        innername = name.Substring(index, "::", out index).Trim();
                        if (innername == "")
                        {
                            innername = name.Substring(index, name.Length - index).Trim();
                            index = name.Length;
                            innertypes.Add(innername);
                        }
                        else
                        {
                            innertypes.Add(innername);
                            index++;
                        }
                        break;
                    }
                    innertypes.Add(innername);
                }
            }
            var membername = name.Substring(index, "(", out index);
            var parameters = new List<string>();
            if (membername == "")
            {
                membername = name.Substring(index, name.Length - index);
            }
            else
            {
                while (true)
                {
                    var parameter = name.Substring(index, ",", out index).Trim();
                    if (parameter == "")
                    {
                        parameter = name.Substring(index, ")", out index).Trim();
                        if (parameter != "")
                        {
                            parameters.Add(parameter);
                        }
                        break;
                    }
                    parameters.Add(parameter);
                }
            }

            return new Name(returntype.Trim(), typename.Trim(), innertypes.ToArray(), membername.Trim(), parameters.ToArray());
        }


        public static FieldReference FindField(AssemblyDefinition assembly, string name)
        {
            var parts = SplitName(name);

            foreach (var module in assembly.Modules)
            {
                var type = module.Types.FirstOrDefault(t => t.FullName == parts.Typename);
                if(type != null)
                {
                    var innertype = type;
                    for (int i = 0; i < parts.Innertypes.Length; ++i)
                    {
                        innertype = innertype.NestedTypes.First(t => t.Name == parts.Innertypes[i]);
                        if (innertype == null)
                            break;
                    }
                    if (innertype != null)
                    {
                        var field = innertype.Fields.First(f => f.Name == parts.Membername);
                        if (field != null)
                        {
                            return field;
                        }
                    }
                }
            }
            
            throw new Exception(string.Format("Field {0} not found.", name));
        }
    }
}
