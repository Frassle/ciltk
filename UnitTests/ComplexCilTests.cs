using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silk;
using System.Runtime.InteropServices;

namespace UnitTests
{
    [TestClass]
    public class ComplexCilTests
    {
        static IntPtr[] EntryPoints = new IntPtr[1];
        static IntPtr ExtensionString;

        delegate IntPtr IntPtr_IntPtr_Delegate(IntPtr i);

        static IntPtr wglGetExtensionsString(IntPtr i)
        {
            return ExtensionString;
        }
        
        static ComplexCilTests()
        {
            ExtensionString = Marshal.StringToHGlobalAnsi("extension");
            EntryPoints[0] = Marshal.GetFunctionPointerForDelegate(new IntPtr_IntPtr_Delegate(wglGetExtensionsString));
        }

        static string GetExtensionsString(IntPtr hdc)
        {
            Silk.Cil.Ldarg(0);
            Silk.Cil.Load(EntryPoints);
            Silk.Cil.Ldc_I4(0);
            Silk.Cil.Ldelem_I();
            Silk.Cil.Calli(CallingConvention.Winapi, typeof(IntPtr), typeof(IntPtr));
            return Marshal.PtrToStringAnsi(Silk.Cil.Peek<IntPtr>());
        }

        [TestMethod]
        public void TestExtensionString()
        {
            Assert.AreEqual("extension", GetExtensionsString(IntPtr.Zero));
        }

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
            Assert.AreEqual(4u, Sizeof<int>(), "int");
            Assert.AreEqual(8u, Sizeof<long>(), "long");
            Assert.AreEqual(8u, Sizeof<TestStruct>(), "TestStruct");
            Assert.AreEqual((uint)IntPtr.Size, Sizeof<TestClass>(), "TestClass");
        }
    }
}
