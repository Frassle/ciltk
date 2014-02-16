using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class InfoTests
    {
        [TestMethod]
        public void TestVariable()
        {
            int a = 0;
            var ainfo = Silk.Info.Variable("a");
            Assert.AreEqual(0, ainfo.LocalIndex);
            Assert.AreEqual(typeof(int), ainfo.LocalType);
            Silk.Cil.KeepAlive(a);
        }

        [TestMethod]
        public void TestField()
        {
            var info = Silk.Info.Field("UnitTests.TestStruct::A");
            Assert.AreEqual("A", info.Name);
            Assert.AreEqual(typeof(int), info.FieldType);
        }
    }
}
