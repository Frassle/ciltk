using System;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture()]
	public class CilTests
	{

		public void JustReturn()
		{
			Silk.Cil.Ret();
		}

		[Test()]
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

		[Test()]
		public void TestLdcReturn()
		{
			int i = ReturnOne();
			Assert.AreEqual(1, i);
		}

		[Test()]
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

		[Test()]
		public void TestLoadStore()
		{
			int i = 0;

			Silk.Cil.Ldc_I4(1);
			Silk.Cil.Stloc(0);

			Assert.AreEqual(1, i);
		}

		[Test()]
		public void TestLabel()
		{
			int i = 0;

			Silk.Cil.Label("a");

			i = 1;

			Silk.Cil.Label("b");

			Assert.AreEqual(1, i);
		}

		[Test()]
		public void TestBranch()
		{
			int i = 0;

			Silk.Cil.Br("branch");
			i = 1;
			Silk.Cil.Label("branch");

			Assert.AreEqual(0, i);
		}

		[Test()]
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

		[Test()]
		public void TestCall()
		{
			int result;
			Silk.Cil.Ldc_I4(1);
			Silk.Cil.Call("System.Int32 UnitTests.TestClass::Increment(System.Int32)");
			Silk.Cil.Store(out result);
			Assert.AreEqual(2, result);
		}

		delegate int TestFunction(int a);

		[Test()]
		public void TestCalli()
		{
			TestFunction test_function = (x => x + 1);
			var function = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(test_function);
			int result;

			Silk.Cil.Ldc_I4(1);
			Silk.Cil.Load(function);
			Silk.Cil.Calli(System.Runtime.InteropServices.CallingConvention.StdCall, typeof(int), typeof(int));
			Silk.Cil.Store(out result);

			Assert.AreEqual(2, result);
		}

		[Test()]
		public void TestSizeof()
		{
			int size = 0;

			Silk.Cil.Sizeof<Int32>();
			Silk.Cil.Stloc(0);

			Assert.AreEqual(4, size);
		}

		public int GenericSizeof<T>()
		{
			Silk.Cil.Sizeof<T>();
			Silk.Cil.Ret();

			return 0;
		}

		[Test()]
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

		[Test()]
		public void TestReadWriteGenerics()
		{
			TestStruct test = new TestStruct() {
				A = 1,
				B = 2
			};

			TestStruct result = ReadWrite<TestStruct>(test);

			Assert.AreEqual(test.A, result.A);
			Assert.AreEqual(test.B, result.B);
		}

		[Test()]
		public void TestLdStobj()
		{
			TestStruct test1 = new TestStruct() {
				A = 1,
				B = 2,
			};

			TestStruct test2 = new TestStruct();

			Silk.Cil.LoadAddress(test2);
			Silk.Cil.LoadAddress(test1);
			Silk.Cil.Ldobj<TestStruct>();
			Silk.Cil.Stobj<TestStruct>();

			Assert.AreEqual(test1.A, test2.A);
			Assert.AreEqual(test1.B, test2.B);
		}

		[Test()]
		public void TestCpobj()
		{
			TestStruct test1 = new TestStruct() {
				A = 1,
				B = 2,
			};

			TestStruct test2 = new TestStruct();

			Silk.Cil.LoadAddress(test2);
			Silk.Cil.LoadAddress(test1);
			Silk.Cil.Cpobj<TestStruct>();

			Assert.AreEqual(test1.A, test2.A);
			Assert.AreEqual(test1.B, test2.B);
		}

		[Test()]
		public void TestInitobj()
		{
			TestStruct test = new TestStruct() {
				A = 1,
				B = 2,
			};

			Silk.Cil.Ldloca(0);
			Silk.Cil.Initobj<TestStruct>();

			Assert.AreEqual(test.A, 0);
			Assert.AreEqual(test.B, 0);
		}

		[Test()]
		public void TestLoadLocal()
		{
			int a = 4;
			int b = 4;
			int c = 8;

			Silk.Cil.Load(a);
			Silk.Cil.Load(b);
			Silk.Cil.Add();
			Silk.Cil.Stloc(0);

			Assert.AreEqual(a, c);
		}

		[Test()]
		public void TestLoadStructField()
		{
			var a = new TestStruct() { A = 1, B = 2 };
			int b = 1;

			Silk.Cil.Load(a.A);
			Silk.Cil.Load(a.B);
			Silk.Cil.Add();
			Silk.Cil.Store(out b);

			Assert.AreEqual(b, 3);
		}

		[Test()]
		public void TestLoadClassField()
		{
			var a = new TestClass() { A = 1, B = 2 };
			int b = 1;

			Silk.Cil.Load(a.A);
			Silk.Cil.Load(a.B);
			Silk.Cil.Add();
			Silk.Cil.Store(out b);

			Assert.AreEqual(b, 3);
		}

		[Test()]
		public void TestStoreInt()
		{
			int a = 4;
			int b;

			Silk.Cil.Load(a);
			Silk.Cil.Store(out b);

			Assert.AreEqual(a, b);
		}

		[Test()]
		public void TestStoreStruct()
		{
			TestStruct a = new TestStruct() { A = 1, B = 2 };
			TestStruct b;

			Silk.Cil.Load(a);
			Silk.Cil.Store(out b);

			Assert.AreEqual(a, b);
		}

		[Test()]
		public void TestStoreClass()
		{
			TestClass a = new TestClass() { A = 1, B = 2 };
			TestClass b;

			Silk.Cil.Load(a);
			Silk.Cil.Store(out b);

			Assert.AreEqual(a, b);
		}

		[Test()]
		public void TestStoreStructField()
		{
			TestStruct a = new TestStruct() { A = 1, B = 2 };
			TestStruct b = new TestStruct() { A = 1, B = 3 };

			Silk.Cil.Load(a.B);
			Silk.Cil.Store(out b.B);

			Assert.AreEqual(a, b);
		}

		[Test()]
		public void TestStoreClassField()
		{
			TestClass a = new TestClass() { A = 1, B = 2 };
			TestClass b = new TestClass() { A = 1, B = 3 };

			Silk.Cil.Load(a.B);
			Silk.Cil.Store(out b.B);

			Assert.AreEqual(a, b);
		}

		[Test()]
		public void TestStoreStaticStructField()
		{
			int a = 1;

			Silk.Cil.Load(a);
			Silk.Cil.Store(out TestStruct.Static);

			Assert.AreEqual(a, TestStruct.Static);
		}

		[Test()]
		public void TestStoreStaticClassField()
		{
			int a = 1;

			Silk.Cil.Load(a);
			Silk.Cil.Store(out TestClass.Static);

			Assert.AreEqual(a, TestClass.Static);
		}

		[Test()]
		public void TestNewarr()
		{
			int[] a;

			Silk.Cil.Ldc_I4(5);
			Silk.Cil.Newarr<int>();
			Silk.Cil.Store(out a);

			Assert.IsNotNull(a);
			Assert.AreEqual(a.Length, 5);
		}

		[Test()]
		public void TestLdelem()
		{
			sbyte[] a = new sbyte[1] { 0x0F };
			short[] b = new short[1] { 0x1F };
			int[] c = new int[1] { 0x2F };
			long[] d = new long[1] { 0x3F };
			float[] e = new float[1] { 0x4F };
			double[] f = new double[1] { 0x5F };

			sbyte va;
			short vb;
			int vc;
			long vd;
			float ve;
			double vf;

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_I1();
			Silk.Cil.Store(out va);
			Assert.AreEqual(va, 0x0F, "Ldelem_I1 failed");

			Silk.Cil.Load(b);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_I2();
			Silk.Cil.Store(out vb);
			Assert.AreEqual(vb, 0x1F, "Ldelem_I2 failed");

			Silk.Cil.Load(c);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_I4();
			Silk.Cil.Store(out vc);
			Assert.AreEqual(vc, 0x2F, "Ldelem_I4 failed");

			Silk.Cil.Load(d);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_I8();
			Silk.Cil.Store(out vd);
			Assert.AreEqual(vd, 0x3F, "Ldelem_I8 failed");

			Silk.Cil.Load(e);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_R4();
			Silk.Cil.Store(out ve);
			Assert.AreEqual(ve, 0x4F, "Ldelem_R4 failed");

			Silk.Cil.Load(f);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_R8();
			Silk.Cil.Store(out vf);
			Assert.AreEqual(vf, 0x5F, "Ldelem_R8 failed");
		}

		[Test()]
		public void TestLdelemT()
		{
			TestStruct[] a = new TestStruct[1];
			a[0] = new TestStruct() { A = 1, B = 2 };
			TestStruct b;

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem<TestStruct>();
			Silk.Cil.Store(out b);

			Assert.AreEqual(a[0], b);
		}

		[Test()]
		public void TestLdelemRef()
		{
			TestDerivedClass[] a = new TestDerivedClass[1];
			a[0] = new TestDerivedClass(1, 2, 3);
			TestClass b;

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelem_Ref();
			Silk.Cil.Store(out b);

			Assert.AreEqual(b.A, 1);
			Assert.AreEqual(b.B, 2);
		}

		[Test()]
		public void TestLdelema()
		{
			TestStruct[] a = new TestStruct[1];
			TestStruct b = new TestStruct() { A = 1, B = 2 };

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Ldelema<TestStruct>();
			Silk.Cil.Load(b);
			Silk.Cil.Stobj<TestStruct>();

			Assert.AreEqual(a[0], b);
		}

		[Test()]
		public void TestStelem()
		{
			byte[] a = new byte[1];
			short[] b = new short[1];
			int[] c = new int[1];
			long[] d = new long[1];
			float[] e = new float[1];
			double[] f = new double[1];

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_I1();
			Silk.Cil.Stelem_I1();
			Assert.AreEqual(a[0], 0xFF, "Stelem_I1 failed");

			Silk.Cil.Load(b);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_I2();
			Silk.Cil.Stelem_I2();
			Assert.AreEqual(b[0], 0xFF, "Stelem_I2 failed");

			Silk.Cil.Load(c);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_I4();
			Silk.Cil.Stelem_I4();
			Assert.AreEqual(c[0], 0xFF, "Stelem_I4 failed");

			Silk.Cil.Load(d);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_I8();
			Silk.Cil.Stelem_I8();
			Assert.AreEqual(d[0], 0xFF, "Stelem_I8 failed");

			Silk.Cil.Load(e);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_R4();
			Silk.Cil.Stelem_R4();
			Assert.AreEqual(e[0], 0xFF, "Stelem_R4 failed");

			Silk.Cil.Load(f);
			Silk.Cil.Load(0);
			Silk.Cil.Load(0xFF);
			Silk.Cil.Conv_R8();
			Silk.Cil.Stelem_R8();
			Assert.AreEqual(f[0], 0xFF, "Stelem_R8 failed");
		}

		[Test()]
		public void TestStelemT()
		{
			TestStruct[] a = new TestStruct[1];
			TestStruct b = new TestStruct() { A = 1, B = 2 };

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Load(b);
			Silk.Cil.Stelem<TestStruct>();

			Assert.AreEqual(a[0], b);
		}

		[Test()]
		public void TestStelemRef()
		{
			TestDerivedClass[] a = new TestDerivedClass[1];
			TestClass b = new TestDerivedClass(1, 2, 3);            

			Silk.Cil.Load(a);
			Silk.Cil.Load(0);
			Silk.Cil.Load(b);
			Silk.Cil.Stelem_Ref();

			Assert.AreEqual(a[0].A, 1);
			Assert.AreEqual(a[0].B, 2);
		}

		[Test()]
		public void TestBox()
		{
			TestStruct a = new TestStruct() { A = 1, B = 2 };
			object b;
			TestStruct c;

			Silk.Cil.Load(a);
			Silk.Cil.Box<TestStruct>();
			Silk.Cil.Store(out b);

			c = (TestStruct)b;

			Assert.AreEqual(a, c);
		}

		[Test()]
		public void TestUnbox()
		{
			TestStruct a = new TestStruct() { A = 1, B = 2 };
			object b = a;
			TestStruct c;

			Silk.Cil.Load(b);
			Silk.Cil.Unbox<TestStruct>();
			Silk.Cil.Ldobj<TestStruct>();
			Silk.Cil.Store(out c);

			Assert.AreEqual(a, c);
		}

		[Test()]
		public void TestUnboxAnyStruct()
		{
			TestStruct a = new TestStruct() { A = 1, B = 2 };
			object b = a;
			TestStruct c;

			Silk.Cil.Load(b);
			Silk.Cil.Unbox_Any<TestStruct>();
			Silk.Cil.Store(out c);

			Assert.AreEqual(a, c);
		}

		[Test()]
		public void TestUnboxAnyClass()
		{
			TestClass a = new TestClass(1, 2);
			object b = a;
			TestClass c;

			Silk.Cil.Load(b);
			Silk.Cil.Unbox_Any<TestClass>();
			Silk.Cil.Store(out c);

			Assert.AreSame(a, c);
		}

		[Test()]
		public void TestSwitch()
		{
			for (int i = 0; i <= 5; ++i) {
				Silk.Cil.Load(i);
				Silk.Cil.Switch("0;1;2;3;4");

				Assert.AreEqual(5, i);
				Silk.Cil.Br("end");

				Silk.Cil.Label("0");
				Assert.AreEqual(0, i);
				Silk.Cil.Br("end");

				Silk.Cil.Label("1");
				Assert.AreEqual(1, i);
				Silk.Cil.Br("end");

				Silk.Cil.Label("2");
				Assert.AreEqual(2, i);
				Silk.Cil.Br("end");

				Silk.Cil.Label("3");
				Assert.AreEqual(3, i);
				Silk.Cil.Br("end");

				Silk.Cil.Label("4");
				Assert.AreEqual(4, i);

				Silk.Cil.Label("end");
			}
		}

		[Test()]
		public void TestLoadAddress()
		{
			int a = 1;

			Silk.Cil.LoadAddress(a);
			Silk.Cil.Ldc_I4(2);
			Silk.Cil.Stind_I4();

			Assert.AreEqual(2, a);
		}

		[Test()]
		public void TestPinned()
		{
			Silk.Cil.DeclareLocal("System.Int32 pinned", "i");
			int j = 0;

			Silk.Cil.Ldc_I4(1);
			Silk.Cil.StoreByName("i");
			Silk.Cil.LoadByName("i");
			Silk.Cil.Store(out j);

			Assert.AreEqual(1, j);
		}

		[Test()]
		public void TestManagedPointer()
		{
			Silk.Cil.DeclareLocal("System.Int32&", "i");
			int j = 0;

			Silk.Cil.LoadAddress(j);
			Silk.Cil.StoreByName("i");
			Silk.Cil.LoadByName("i");
			Silk.Cil.Load(1);
			Silk.Cil.Stind_I4();

			Assert.AreEqual(1, j);
		}

		[Test()]
		public void TestPinnedPointerCopy()
		{
			Silk.Cil.DeclareLocal("System.Byte& pinned", "bytes");
			Silk.Cil.Load(new System.Byte[] { 1, 2, 3, 4, 5 });
			Silk.Cil.Ldc_I4(0);
			Silk.Cil.Ldelema<System.Byte>();
			Silk.Cil.StoreByName("bytes");

			IntPtr localloc;

			Silk.Cil.Ldc_I4(5);
			Silk.Cil.Localloc();
			Silk.Cil.Store(out localloc);

			Silk.Cil.Load(localloc);
			Silk.Cil.LoadByName("bytes");
			Silk.Cil.Ldc_I4(5);
			Silk.Cil.Cpblk();

			Assert.AreEqual(1, System.Runtime.InteropServices.Marshal.ReadByte(localloc, 0));
			Assert.AreEqual(2, System.Runtime.InteropServices.Marshal.ReadByte(localloc, 1));
			Assert.AreEqual(3, System.Runtime.InteropServices.Marshal.ReadByte(localloc, 2));
			Assert.AreEqual(4, System.Runtime.InteropServices.Marshal.ReadByte(localloc, 3));
			Assert.AreEqual(5, System.Runtime.InteropServices.Marshal.ReadByte(localloc, 4));
		}

		[Test()]
		public void TestPeek()
		{
			int a = 1;

			Silk.Cil.Ldc_I4(1);
			Silk.Cil.Ldc_I4(2);
			Silk.Cil.Add();
			a = Silk.Cil.Peek<int>();

			Assert.AreEqual(3, a);
		}
	}
}
