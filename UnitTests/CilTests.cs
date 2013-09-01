using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CilTests
    {
        public void JustReturn()
        {
            CilTK.Cil.Ret();
        }

        [TestMethod]
        public void TestReturn()
        {
            JustReturn();
        }

        public int ReturnOne()
        {
            CilTK.Cil.Ldc_I4(1);
            CilTK.Cil.Ret();

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

            CilTK.Cil.Ldloc(0);
            CilTK.Cil.Ldloc(1);
            CilTK.Cil.Add();
            CilTK.Cil.Stloc(0);

            Assert.AreEqual(3, i);
        }

        [TestMethod]
        public void TestLoadStore()
        {
            int i = 0;

            CilTK.Cil.Ldc_I4(1);
            CilTK.Cil.Stloc(0);

            Assert.AreEqual(1, i);
        }

        [TestMethod]
        public void TestLabel()
        {
            int i = 0;

            CilTK.Cil.Label("a");

            i = 1;

            CilTK.Cil.Label("b");

            Assert.AreEqual(1, i);
        }
    }
}
