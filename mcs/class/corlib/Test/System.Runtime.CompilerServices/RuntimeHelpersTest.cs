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

	public class RuntimeHelpersTest : TestCase {
	    struct FooStruct {
			public int i;
			public string j;
		}

		class FooClass {
			public static int counter = 0;

			static FooClass () {
				counter = counter + 1;
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
	}
}
