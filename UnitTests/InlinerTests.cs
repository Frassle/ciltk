using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class InlinerTests
    {
        [CilTK.Inline]
        public static int ReturnOne()
        {
            return 1;
        }

        [TestMethod]
        public void TestReturnConstant()
        {
            int i = ReturnOne();
            Assert.AreEqual(1, i);
        }
    }
}
