//
// RuntimeHelpersTest.cs - NUnit Test Cases for the System.Runtime.CompilerServices.RuntimeHelpers class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

using System;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace MonoTests.System.Runtime.CompilerServices {

	[TestFixture]
	public class RuntimeHelpersTest {
	    struct FooStruct {
			public int i;
			public string j;

			public override int GetHashCode () {
				return 5;
			}

			public override bool Equals (object o) {
				Assert.Fail ();
				return false;
			}
		}

		class FooClass {
			public static int counter = 0;

			static FooClass () {
				counter = counter + 1;
			}

			public override int GetHashCode () {
				return 5;
			}

			public override bool Equals (object o) {
				Assert.Fail ();
				return true;
			}
		}

		public void TestOffsetToStringData () 
		{
			Assert.AreEqual (
						  RuntimeHelpers.OffsetToStringData,
						  RuntimeHelpers.OffsetToStringData, "OffsetToStringData is not constant");
		}

		public void TestGetObjectValue ()
		{
			FooStruct s1;
			FooStruct s2;

			// Test null
			Assert.AreEqual (RuntimeHelpers.GetObjectValue (null),
						  null);
			
			// Test non-valuetype
			Assert.AreEqual (RuntimeHelpers.GetObjectValue (this),
						  this);

			// Test valuetype
			s1.i = 42;
			s1.j = "FOO";
			s2 = (FooStruct)RuntimeHelpers.GetObjectValue(s1);
			s1.i = 43;
			s1.j = "BAR";
			Assert.AreEqual (s2.i, 42);
			Assert.AreEqual (s2.j, "FOO");
		}

		public void TestRunClassConstructor ()
		{
			RuntimeHelpers.RunClassConstructor (typeof(FooClass).TypeHandle);
			Assert.AreEqual (FooClass.counter, 1);

			// Each static constructor should only be run once
			RuntimeHelpers.RunClassConstructor (typeof(FooClass).TypeHandle);
			Assert.AreEqual (FooClass.counter, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RunClassConstructor_Default ()
		{
			RuntimeTypeHandle rth = new RuntimeTypeHandle ();
			Assert.AreEqual (IntPtr.Zero, rth.Value, "Value");
			RuntimeHelpers.RunClassConstructor (rth);
		}

		static RuntimeTypeHandle handle;

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RunClassConstructor_Uninitialized ()
		{
			RuntimeHelpers.RunClassConstructor (handle);
		}

		class Thrower {
			static Thrower ()
			{
				throw new NotFiniteNumberException ();
			}
		}

		[Test]
		[ExpectedException (typeof (TypeInitializationException))]
		public void RunClassConstructor_Throw ()
		{
			RuntimeHelpers.RunClassConstructor (typeof (Thrower).TypeHandle);
		}

		class Fielder {
			public byte [] array = new byte [1];
		}

		static RuntimeFieldHandle rfh = typeof (Fielder).GetField ("array").FieldHandle;
		static RuntimeFieldHandle static_rfh;

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InitializeArray_Null ()
		{
			RuntimeHelpers.InitializeArray (null, rfh);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InitializeArray_Default ()
		{
			RuntimeFieldHandle h = new RuntimeFieldHandle ();
			RuntimeHelpers.InitializeArray (new Fielder ().array, h);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InitializeArray_Uninitialized ()
		{
			RuntimeHelpers.InitializeArray (new Fielder ().array, static_rfh);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InitializeArray ()
		{
			RuntimeHelpers.InitializeArray (new Fielder ().array, rfh);
		}

		public void TestGetHashCode ()
		{
			Assert.AreEqual (0, RuntimeHelpers.GetHashCode (null));
			object o = new object ();
			Assert.AreEqual (o.GetHashCode (), RuntimeHelpers.GetHashCode (o));
			Assert.IsTrue (5 != RuntimeHelpers.GetHashCode (new FooClass ()));
		}			

		public void TestEquals ()
		{
			Assert.IsTrue (RuntimeHelpers.Equals (null, null));
			Assert.IsTrue (!RuntimeHelpers.Equals (new object (), null));
			Assert.IsTrue (!RuntimeHelpers.Equals (null, new object ()));

			FooStruct f1 = new FooStruct ();
			f1.i = 5;
			FooStruct f2 = new FooStruct ();
			f2.i = 5;
			object o1 = f1;
			object o2 = o1;
			object o3 = f2;
			object o4 = "AAA";
			Assert.IsTrue (RuntimeHelpers.Equals (o1, o2));

			// This should do a bit-by-bit comparison for valuetypes
			Assert.IsTrue (RuntimeHelpers.Equals (o1, o3));
			Assert.IsTrue (!RuntimeHelpers.Equals (o1, o4));
		}
	}
}
