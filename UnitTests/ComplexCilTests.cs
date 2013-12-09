using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk;
using System.Runtime.InteropServices;

namespace UnitTests
{
    [TestClass]
    public class ComplexCilTests
    {
        static void CopyTo<T>(IntPtr dst, T value)
        {
            Cil.Ldarg(0);
            Cil.Ldarga(1);
            Cil.Sizeof<T>();
            Cil.Cpblk();
        }

        [TestMethod]
        public void TestCopy()
        {
            TestStruct[] destination = new TestStruct[1];
            GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr dst = handle.AddrOfPinnedObject();

            TestStruct value = new TestStruct() 
            {
                A = 1, B = 2,
            };

            CopyTo(dst, value);

            Assert.AreEqual(value, destination[0]);
        }

        static uint Sizeof<T>()
        {
            Cil.Sizeof<T>();
            Cil.Ret();

            return 0;
        }

        [TestMethod]
        public void TestSizeof()
        {
            Assert.AreEqual(4, Sizeof<int>(), "int");
            Assert.AreEqual(8, Sizeof<long>(), "long");
            Assert.AreEqual(8, Sizeof<TestStruct>(), "TestStruct");
            Assert.AreEqual(IntPtr.Size, Sizeof<TestClass>(), "TestClass");
        }
    }
}
