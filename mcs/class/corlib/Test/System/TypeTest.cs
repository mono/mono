// TypeTest.cs - NUnit Test Cases for the System.Type class
//
// Authors:
// 	Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	class Super : ICloneable {
		public virtual object Clone () {
			return null;
		}
	}
	class Duper: Super {
	}

	enum TheEnum { A, B, C };

	[TestFixture]
	public class TypeTest : Assertion
	{
		[Test]
		public void TestIsAssignableFrom () {
			// Simple tests for inheritance
			AssertEquals (typeof (Super).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals (typeof (Duper).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals (typeof (Object).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals (typeof (ICloneable).IsAssignableFrom (typeof (Duper)) , true);

			// Tests for arrays
			AssertEquals (typeof (Super[]).IsAssignableFrom (typeof (Duper[])) , true);
			AssertEquals (typeof (Duper[]).IsAssignableFrom (typeof (Super[])) , false);
			AssertEquals (typeof (Object[]).IsAssignableFrom (typeof (Duper[])) , true);
			AssertEquals (typeof (ICloneable[]).IsAssignableFrom (typeof (Duper[])) , true);

			// Tests for multiple dimensional arrays
			AssertEquals (typeof (Super[][]).IsAssignableFrom (typeof (Duper[][])) , true);
			AssertEquals (typeof (Duper[][]).IsAssignableFrom (typeof (Super[][])) , false);
			AssertEquals (typeof (Object[][]).IsAssignableFrom (typeof (Duper[][])) , true);
			AssertEquals (typeof (ICloneable[][]).IsAssignableFrom (typeof (Duper[][])) , true);

			// Test that arrays of enums can be cast to their base types
			AssertEquals (typeof (int[]).IsAssignableFrom (typeof (TypeCode[])) , true);

			// Test that arrays of valuetypes can't be cast to arrays of
			// references
			AssertEquals (typeof (object[]).IsAssignableFrom (typeof (TypeCode[])) , false);			
			AssertEquals (typeof (ValueType[]).IsAssignableFrom (typeof (TypeCode[])) , false);
			AssertEquals (typeof (Enum[]).IsAssignableFrom (typeof (TypeCode[])) , false);

			// Test that arrays of enums can't be cast to arrays of references
			AssertEquals (typeof (object[]).IsAssignableFrom (typeof (TheEnum[])) , false);
			AssertEquals (typeof (ValueType[]).IsAssignableFrom (typeof (TheEnum[])) , false);
			AssertEquals (typeof (Enum[]).IsAssignableFrom (typeof (TheEnum[])) , false);

			// Check that ValueType and Enum are recognized as reference types
			AssertEquals (typeof (object).IsAssignableFrom (typeof (ValueType)) , true);
			AssertEquals (typeof (object).IsAssignableFrom (typeof (Enum)) , true);
			AssertEquals (typeof (ValueType).IsAssignableFrom (typeof (Enum)) , true);

			AssertEquals (typeof (object[]).IsAssignableFrom (typeof (ValueType[])) , true);
			AssertEquals (typeof (ValueType[]).IsAssignableFrom (typeof (ValueType[])) , true);
			AssertEquals (typeof (Enum[]).IsAssignableFrom (typeof (ValueType[])) , false);

			AssertEquals (typeof (object[]).IsAssignableFrom (typeof (Enum[])) , true);
			AssertEquals (typeof (ValueType[]).IsAssignableFrom (typeof (Enum[])) , true);
			AssertEquals (typeof (Enum[]).IsAssignableFrom (typeof (Enum[])) , true);
		}

		[Test]
		public void TestIsSubclassOf () {
			Assert (typeof (ICloneable).IsSubclassOf (typeof (object)));
		}
	}
}

