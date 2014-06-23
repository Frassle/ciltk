using System;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture()]
    public class InfoTests
    {
        [Test()]
        public void TestField()
        {
            var info = Silk.Info.Field("System.Int32 UnitTests.TestStruct::A");
            Assert.AreEqual("A", info.Name);
            Assert.AreEqual(typeof(int), info.FieldType);
        }

        [Test()]
        public void TestExternalField()
        {
            var info = Silk.Info.Field("System.String System.String::Empty");
            Assert.AreEqual("Empty", info.Name);
            Assert.AreEqual(typeof(string), info.FieldType);
        }

        [Test()]
        public void TestMethod()
        {
            var info = Silk.Info.Method("System.Int32 UnitTests.TestStruct::Add(System.Int32)");
            Assert.AreEqual("Add", info.Name);
            Assert.AreEqual(typeof(int), info.ReturnType);
        }

        [Test()]
        public void TestExternalMethod()
        {
            var info = Silk.Info.Method("System.Void System.Console::Write(System.String)");
            Assert.AreEqual("Write", info.Name);
            Assert.AreEqual("System.Void", info.ReturnType.FullName);
        }
    }
}
