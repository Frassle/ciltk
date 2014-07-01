using System;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture()]
    public class InfoTests
    {
        public Type GenericMethod<T>()
        {
            return Silk.Info.Type("T");
        }

        [Test()]
        public void TestType()
        {
            Assert.AreEqual(Silk.Info.Type("System.Int32").FullName, "System.Int32");
            Assert.AreEqual(Silk.Info.Type("UnitTests.TestClass/InnerClass").FullName, "UnitTests.TestClass+InnerClass");
            Assert.AreEqual(GenericMethod<int>().FullName, "System.Int32");
        }

        [Test()]
        public void TestField()
        {
            var info = Silk.Info.Field("UnitTests.TestStruct::A");
            Assert.AreEqual("A", info.Name);
            Assert.AreEqual(typeof(int), info.FieldType);
        }

        [Test()]
        public void TestExternalField()
        {
            var info = Silk.Info.Field("System.String::Empty");
            Assert.AreEqual("Empty", info.Name);
            Assert.AreEqual(typeof(string), info.FieldType);
        }

        [Test()]
        public void TestMethod()
        {
            var info = Silk.Info.Method("UnitTests.TestStruct::Add(System.Int32)");
            Assert.AreEqual("Add", info.Name);
            Assert.AreEqual(typeof(int), info.ReturnType);
        }

        [Test()]
        public void TestGenericMethod()
        {
            System.Reflection.MethodInfo info;

            info = Silk.Info.Method("UnitTests.TestClass::GenericMethod(T)");
            Assert.AreEqual("GenericMethod", info.Name);

            info = Silk.Info.Method("UnitTests.TestClass::GenericMethod<System.Int32>(T)");
            Assert.AreEqual("GenericMethod", info.Name);
            Assert.AreEqual(typeof(int), info.ReturnType);
        }

        [Test()]
        public void TestExternalMethod()
        {
            var info = Silk.Info.Method("System.Console::Write(System.String)");
            Assert.AreEqual("Write", info.Name);
            Assert.AreEqual("System.Void", info.ReturnType.FullName);
        }

        [Test()]
        public void TestProperty()
        {
            var info = Silk.Info.Property("UnitTests.TestClass::C");
            Assert.AreEqual("C", info.Name);
            Assert.AreEqual(typeof(int), info.PropertyType);
        }

        [Test()]
        public void TestPropertyIndexer()
        {
            var info = Silk.Info.Property("UnitTests.TestClass::Item(System.Int32)");
            Assert.AreEqual("Item", info.Name);
            Assert.AreEqual(typeof(int), info.PropertyType);
            Assert.AreEqual(1, info.GetIndexParameters().Length);
        }

        [Test()]
        public void TestVariable()
        {
            var info = Silk.Info.Variable("info");
            Assert.AreEqual("System.Reflection.LocalVariableInfo", info.LocalType.FullName);
            Assert.AreEqual(false, info.IsPinned);
        }

        System.Reflection.ParameterInfo GetParameterInfo(int a)
        {
            return Silk.Info.Parameter("a");
        }

        [Test()]
        public void TestParameter()
        {
            var info = GetParameterInfo(0);
            Assert.AreEqual("a", info.Name);
            Assert.AreEqual("System.Int32", info.ParameterType.FullName);
        }
    }
}
