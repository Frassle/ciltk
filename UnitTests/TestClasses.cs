using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests
{
    struct TestStruct
    {
        public static int Static;

        public int A;
        public int B;

        public int Add(int c)
        {
            return A + B + c;
        }
    }

    class TestClass
    {
        public class InnerClass
        {
            public static int Static;

            public int A;
            public int B;

            public InnerClass()
            {
            }

            public InnerClass(int a, int b)
            {
                A = a;
                B = b;
            }

            public override bool Equals(object obj)
            {
                var other = obj as InnerClass;
                if (other == null)
                {
                    return false;
                }

                return A == other.A && B == other.B;
            }
        }


        public static int Static;

        public int A;
        public int B;
        public int C { get; set; }
        public int this[int index]
        {
            get { return A; }
            set { A = value; }
        }

        public TestClass()
        {
        }

        public TestClass(int a, int b)
        {
            A = a;
            B = b;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestClass;
            if (other == null)
            {
                return false;
            }

            return A == other.A && B == other.B;
        }

        public static int Increment(int i)
        {
            return i + 1;
        }
    }

    class TestDerivedClass : TestClass
    {
        public int C;

        public TestDerivedClass()
        {
        }

        public TestDerivedClass(int a, int b, int c)
            : base(a, b)
        {
            C = c;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TestDerivedClass;
            if (other == null)
            {
                return false;
            }

            return base.Equals(other) && C == other.C;
        }
    }

    class GenericClass<TA, TB>
    {
        public static int Static;

        public TA A;
        public TB B;

        public GenericClass()
        {
        }

        public GenericClass(TA a, TB b)
        {
            A = a;
            B = b;
        }

        public static T GenericMethod<T>(T t)
        {
            return t;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GenericClass<TA, TB>;
            if (other == null)
            {
                return false;
            }

            return A.Equals(other.A) && B.Equals(other.B);
        }
    }
}
