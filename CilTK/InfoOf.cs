using System;
using System.Collections.Generic;
using System.Text;

namespace Silk
{
    public delegate void Action();
    public delegate void Action<T1>(T1 item1);
    public delegate void Action<T1, T2>(T1 item1, T2 item2);
    public delegate void Action<T1, T2, T3>(T1 item1, T2 item2, T3 item3);
    public delegate void Action<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4);
    public delegate void Action<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9);

    public delegate TRet Function<TRet>();
    public delegate TRet Function<T1, TRet>(T1 item1);
    public delegate TRet Function<T1, T2, TRet>(T1 item1, T2 item2);
    public delegate TRet Function<T1, T2, T3, TRet>(T1 item1, T2 item2, T3 item3);
    public delegate TRet Function<T1, T2, T3, T4, TRet>(T1 item1, T2 item2, T3 item3, T4 item4);
    public delegate TRet Function<T1, T2, T3, T4, T5, TRet>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5);
    public delegate TRet Function<T1, T2, T3, T4, T5, T6, TRet>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6);
    public delegate TRet Function<T1, T2, T3, T4, T5, T6, T7, TRet>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7);
    public delegate TRet Function<T1, T2, T3, T4, T5, T6, T7, T8, TRet>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8);
    public delegate TRet Function<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8, T9 item9);

    public static class InfoOf
    {
        public static System.Reflection.ParameterInfo ParameterByName(string parameter)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.LocalVariableInfo VariableByName(string variable)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.FieldInfo FieldByName(string field)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.PropertyInfo PropertyByName(string property)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.MethodInfo MethodByName(string method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

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

        public static System.Reflection.MethodInfo Method(Action method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.MethodInfo Method<T1>(Action<T1> method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.MethodInfo Method<T1, T2>(Action<T1, T2> method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }

        public static System.Reflection.MethodInfo Method<TRet>(Function<TRet> method)
        {
            throw new Exception("CilTK Rewriter not run.");
        }
    }
}
