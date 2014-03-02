using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class InfoTests
    {
        [TestMethod]
        public void TestField()
        {
            var info = Silk.Info.Field("System.Int32 UnitTests.TestStruct::A");
            Assert.AreEqual("A", info.Name);
            Assert.AreEqual(typeof(int), info.FieldType);
        }

        [TestMethod]
        public void TestExternalField()
        {
            var info = Silk.Info.Field("System.String System.String::Empty");
            Assert.AreEqual("Empty", info.Name);
            Assert.AreEqual(typeof(string), info.FieldType);
        }

        [TestMethod]
        public void TestMethod()
        {
            var info = Silk.Info.Method("System.Int32 UnitTests.TestStruct::Add(System.Int32)");
            Assert.AreEqual("Add", info.Name);
            Assert.AreEqual(typeof(int), info.ReturnType);
        }

        [TestMethod]
        public void TestExternalMethod()
        {
            var info = Silk.Info.Method("System.Void Silk.Cil::Label(System.String)");
            Assert.AreEqual("Label", info.Name);
            Assert.AreEqual("System.Void", info.ReturnType.FullName);
        }
    }
}
