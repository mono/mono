// TypeTest.cs - NUnit Test Cases for the System.Type class
//
// Authors:
// 	Zoltan Varga (vargaz@freemail.hu)
//  Patrik Torstensson
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

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

	abstract class Base {
		public int level;

		public abstract int this [byte i] { get; }
		public abstract int this [int i] { get; }
		public abstract void TestVoid();
		public abstract void TestInt(int i);
	}

	class DeriveVTable : Base {
		public override int this [byte i] { get { return 1; } }
		public override int this [int i] { get { return 1; } }
		public override void TestVoid() { level = 1; }
		public override void TestInt(int i) { level = 1; }
	}

	class NewVTable : DeriveVTable {
		public new int this [byte i] { get { return 2; } }
		public new int this [int i] { get { return 2; } }
		public new void TestVoid() { level = 2; }
		public new void TestInt(int i) { level = 2; }

		public void Overload () { }
		public void Overload (int i) { }
	}

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

		[Test]
		public void TestGetMethodImpl() {
			// Test binding of new slot methods (using no types)
			AssertEquals(typeof (Base), typeof (Base).GetMethod("TestVoid").DeclaringType);
			AssertEquals(typeof (NewVTable), typeof (NewVTable).GetMethod("TestVoid").DeclaringType);

			// Test binding of new slot methods (using types)
			AssertEquals(typeof (Base), typeof (Base).GetMethod("TestInt", new Type [] { typeof(int) }).DeclaringType);
			AssertEquals(typeof (NewVTable), typeof (NewVTable).GetMethod("TestInt", new Type [] { typeof(int) }).DeclaringType);

			// Test overload resolution
			AssertEquals (0, typeof (NewVTable).GetMethod ("Overload", new Type [0]).GetParameters ().Length);
		}

		[Test]
		public void TestGetPropertyImpl() {
			// Test getting property that is exact
			AssertEquals(typeof (NewVTable), typeof (NewVTable).GetProperty("Item", new Type[1] { typeof(Int32) }).DeclaringType);

			// Test getting property that is not exact
			AssertEquals(typeof (NewVTable), typeof (NewVTable).GetProperty("Item", new Type[1] { typeof(Int16) }).DeclaringType);
		}
	}
}

