using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class LoomTest
    {
        [TestMethod]
        public void TestSplit()
        {
            var ty = new PrivateType(typeof(Silk.Loom.References));
            PrivateObject obj;

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Test"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.TestGeneric`1"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.TestGeneric`1", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test/Inner"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test/Inner1/Inner2"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner1", "Inner2" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test`1/Inner1`2/Inner2`3"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test`1", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner1`2", "Inner2`3" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test::Field"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Field", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "Namespace.Test/Inner::Field"));
            Assert.AreEqual("", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Field", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "void Namespace.Test/Inner::Method()"));
            Assert.AreEqual("void", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Method", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[0], (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "void Namespace.Test/Inner::Method(int)"));
            Assert.AreEqual("void", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Method", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[] { "int" }, (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "void Namespace.Test/Inner::Method(int, bar)"));
            Assert.AreEqual("void", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Method", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[] { "int", "bar" }, (string[])obj.GetField("Paramaters"));

            obj = new PrivateObject(ty.InvokeStatic("SplitName", "void Namespace.Test/Inner::Method(int, Namespace.Test/Inner)"));
            Assert.AreEqual("void", obj.GetField("Returntype"));
            Assert.AreEqual("Namespace.Test", obj.GetField("Typename"));
            CollectionAssert.AreEqual(new string[] { "Inner" }, (string[])obj.GetField("Innertypes"));
            Assert.AreEqual("Method", obj.GetField("Membername"));
            CollectionAssert.AreEqual(new string[] { "int", "Namespace.Test/Inner" }, (string[])obj.GetField("Paramaters"));  
        }
    }
}
