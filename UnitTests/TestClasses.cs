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
    }

    class TestClass
    {
        public static int Static;

        public int A;
        public int B;

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
    }
}
