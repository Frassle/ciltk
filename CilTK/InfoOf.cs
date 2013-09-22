using System;
using System.Collections.Generic;
using System.Text;

namespace Silk
{
    public static class InfoOf
    {
        public static System.Reflection.ParameterInfo Parameter<T>(T parameter)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.LocalVariableInfo Variable<T>(T variable)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.FieldInfo Field<T>(T field)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.PropertyInfo Property<T>(T property)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.MethodInfo Method<T>(Action<T> method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }
    }
}
