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
	public class RuntimeHelpersTest : Assertion {
	    struct FooStruct {
			public int i;
			public string j;

			public override int GetHashCode () {
				return 5;
			}

			public override bool Equals (object o) {
				Fail ();
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
				Fail ();
				return true;
			}
		}

		public void TestOffsetToStringData () 
		{
			AssertEquals ("OffsetToStringData is not constant",
						  RuntimeHelpers.OffsetToStringData,
						  RuntimeHelpers.OffsetToStringData);
		}

		public void TestGetObjectValue ()
		{
			FooStruct s1;
			FooStruct s2;

			// Test null
			AssertEquals ("",
						  RuntimeHelpers.GetObjectValue (null),
						  null);
			
			// Test non-valuetype
			AssertEquals ("",
						  RuntimeHelpers.GetObjectValue (this),
						  this);

			// Test valuetype
			s1.i = 42;
			s1.j = "FOO";
			s2 = (FooStruct)RuntimeHelpers.GetObjectValue(s1);
			s1.i = 43;
			s1.j = "BAR";
			AssertEquals ("", s2.i, 42);
			AssertEquals ("", s2.j, "FOO");
		}

		public void TestRunClassConstructor ()
		{
			RuntimeHelpers.RunClassConstructor (typeof(FooClass).TypeHandle);
			AssertEquals ("", FooClass.counter, 1);

			// Each static constructor should only be run once
			RuntimeHelpers.RunClassConstructor (typeof(FooClass).TypeHandle);
			AssertEquals ("", FooClass.counter, 1);
		}

#if NET_1_1
		public void TestGetHashCode ()
		{
			AssertEquals ("Null has hash code 0", 0, RuntimeHelpers.GetHashCode (null));
			object o = new object ();
			AssertEquals ("", o.GetHashCode (), RuntimeHelpers.GetHashCode (o));
			Assert ("", 5 != RuntimeHelpers.GetHashCode (new FooClass ()));
		}			

		public void TestEquals ()
		{
			Assert (RuntimeHelpers.Equals (null, null));
			Assert (!RuntimeHelpers.Equals (new object (), null));
			Assert (!RuntimeHelpers.Equals (null, new object ()));

			FooStruct f1 = new FooStruct ();
			f1.i = 5;
			FooStruct f2 = new FooStruct ();
			f2.i = 5;
			object o1 = f1;
			object o2 = o1;
			object o3 = f2;
			object o4 = "AAA";
			Assert (RuntimeHelpers.Equals (o1, o2));

			// This should do a bit-by-bit comparison for valuetypes
			Assert (RuntimeHelpers.Equals (o1, o3));
			Assert (!RuntimeHelpers.Equals (o1, o4));
		}
#endif
	}
}
