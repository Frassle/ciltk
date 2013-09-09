using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CilTests
    {
        public void JustReturn()
        {
            Silk.Cil.Ret();
        }

        [TestMethod]
        public void TestReturn()
        {
            JustReturn();
        }

        public int ReturnOne()
        {
            Silk.Cil.Ldc_I4(1);
            Silk.Cil.Ret();

            return 0;
        }

        [TestMethod]
        public void TestLdcReturn()
        {
            int i = ReturnOne();
            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestAdd()
        {
            int i = 1;
            int j = 2;

            Silk.Cil.Ldloc(0);
            Silk.Cil.Ldloc(1);
            Silk.Cil.Add();
            Silk.Cil.Stloc(0);

            Silk.Cil.KeepAlive(j);

            Assert.AreEqual(3, i);
        }

        [TestMethod]
        public void TestLoadStore()
        {
            int i = 0;

            Silk.Cil.Ldc_I4(1);
            Silk.Cil.Stloc(0);

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestLabel()
        {
            int i = 0;

            Silk.Cil.Label("a");

            i = 1;

            Silk.Cil.Label("b");

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestBranch()
        {
            int i = 0;

            Silk.Cil.Br("branch");
            i = 1;
            Silk.Cil.Label("branch");

            Assert.AreEqual(0, i);
        }

        [TestMethod]
        public void TestBranchEqual()
        {
            int i = 0;
            int j = 1;

            Silk.Cil.Ldloc(0);
            Silk.Cil.Ldloc(1);
            Silk.Cil.Beq("equal");
            i = j;
            Silk.Cil.Label("equal");

            Assert.AreEqual(i, j);
        }

        [TestMethod]
        public void TestSizeof()
        {
            int size = 0;

            Silk.Cil.Sizeof<Int32>();
            Silk.Cil.Stloc(0);

            Assert.AreEqual(4, size);
        }

        struct TestStruct
        {
            public int A;
            public int B;
        }

        public int GenericSizeof<T>()
        {
            Silk.Cil.Sizeof<T>();
            Silk.Cil.Ret();

            return 0;
        }

        [TestMethod]
        public void TestGenericSizeof()
        {
            Assert.AreEqual(8, GenericSizeof<TestStruct>());
        }

        public T ReadWrite<T>(T value)
        {
            T result = default(T);

            Silk.Cil.Ldloca(0);
            Silk.Cil.Ldarga(0);
            Silk.Cil.Sizeof<T>();
            Silk.Cil.Cpblk();

            return result;
        }

        [TestMethod]
        public void TestReadWriteGenerics()
        {
            TestStruct test = new TestStruct()
            {
                A = 1,
                B = 2
            };

            TestStruct result = ReadWrite<TestStruct>(test);

            Assert.AreEqual(test.A, result.A);
            Assert.AreEqual(test.B, result.B);
        }
    }
}
