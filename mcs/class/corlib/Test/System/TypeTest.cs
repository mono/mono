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
using System.Runtime.InteropServices;

class NoNamespaceClass {
}

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

	class Base1 {
		public virtual int Foo {
			get {
				return 1;
			}
			set {
			}
		}
	}

	class Derived1 : Base1 {
		public override int Foo {
			set {
			}
		}
	}

	[TestFixture]
	public class TypeTest : Assertion
	{
		[Test]
		public void TestIsAssignableFrom () {
			// Simple tests for inheritance
			AssertEquals ("#01", typeof (Super).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals ("#02", typeof (Duper).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals ("#03", typeof (Object).IsAssignableFrom (typeof (Duper)) , true);
			AssertEquals ("#04", typeof (ICloneable).IsAssignableFrom (typeof (Duper)) , true);

			// Tests for arrays
			AssertEquals ("#05", typeof (Super[]).IsAssignableFrom (typeof (Duper[])) , true);
			AssertEquals ("#06", typeof (Duper[]).IsAssignableFrom (typeof (Super[])) , false);
			AssertEquals ("#07", typeof (Object[]).IsAssignableFrom (typeof (Duper[])) , true);
			AssertEquals ("#08", typeof (ICloneable[]).IsAssignableFrom (typeof (Duper[])) , true);

			// Tests for multiple dimensional arrays
			AssertEquals ("#09", typeof (Super[][]).IsAssignableFrom (typeof (Duper[][])) , true);
			AssertEquals ("#10", typeof (Duper[][]).IsAssignableFrom (typeof (Super[][])) , false);
			AssertEquals ("#11", typeof (Object[][]).IsAssignableFrom (typeof (Duper[][])) , true);
			AssertEquals ("#12", typeof (ICloneable[][]).IsAssignableFrom (typeof (Duper[][])) , true);

			// Tests for vectors<->one dimensional arrays */
			Array arr1 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {0});
			Array arr2 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {10});

			AssertEquals ("#13", typeof (int[]).IsAssignableFrom (arr1.GetType ()), true);
			AssertEquals ("#14", typeof (int[]).IsAssignableFrom (arr2.GetType ()), false);

			// Test that arrays of enums can be cast to their base types
			AssertEquals ("#15", typeof (int[]).IsAssignableFrom (typeof (TypeCode[])) , true);

			// Test that arrays of valuetypes can't be cast to arrays of
			// references
			AssertEquals ("#16", typeof (object[]).IsAssignableFrom (typeof (TypeCode[])) , false);			
			AssertEquals ("#17", typeof (ValueType[]).IsAssignableFrom (typeof (TypeCode[])) , false);
			AssertEquals ("#18", typeof (Enum[]).IsAssignableFrom (typeof (TypeCode[])) , false);

			// Test that arrays of enums can't be cast to arrays of references
			AssertEquals ("#19", typeof (object[]).IsAssignableFrom (typeof (TheEnum[])) , false);
			AssertEquals ("#20", typeof (ValueType[]).IsAssignableFrom (typeof (TheEnum[])) , false);
			AssertEquals ("#21", typeof (Enum[]).IsAssignableFrom (typeof (TheEnum[])) , false);

			// Check that ValueType and Enum are recognized as reference types
			AssertEquals ("#22", typeof (object).IsAssignableFrom (typeof (ValueType)) , true);
			AssertEquals ("#23", typeof (object).IsAssignableFrom (typeof (Enum)) , true);
			AssertEquals ("#24", typeof (ValueType).IsAssignableFrom (typeof (Enum)) , true);

			AssertEquals ("#25", typeof (object[]).IsAssignableFrom (typeof (ValueType[])) , true);
			AssertEquals ("#26", typeof (ValueType[]).IsAssignableFrom (typeof (ValueType[])) , true);
			AssertEquals ("#27", typeof (Enum[]).IsAssignableFrom (typeof (ValueType[])) , false);

			AssertEquals ("#28", typeof (object[]).IsAssignableFrom (typeof (Enum[])) , true);
			AssertEquals ("#29", typeof (ValueType[]).IsAssignableFrom (typeof (Enum[])) , true);
			AssertEquals ("#30", typeof (Enum[]).IsAssignableFrom (typeof (Enum[])) , true);
		}

		[Test]
		public void TestIsSubclassOf () {
			Assert ("#01", typeof (ICloneable).IsSubclassOf (typeof (object)));
		}

		[Test]
		public void TestGetMethodImpl() {
			// Test binding of new slot methods (using no types)
			AssertEquals("#01", typeof (Base), typeof (Base).GetMethod("TestVoid").DeclaringType);
			AssertEquals("#02", typeof (NewVTable), typeof (NewVTable).GetMethod("TestVoid").DeclaringType);

			// Test binding of new slot methods (using types)
			AssertEquals("#03", typeof (Base), typeof (Base).GetMethod("TestInt", new Type [] { typeof(int) }).DeclaringType);
			AssertEquals("#04", typeof (NewVTable), typeof (NewVTable).GetMethod("TestInt", new Type [] { typeof(int) }).DeclaringType);

			// Test overload resolution
			AssertEquals ("#05", 0, typeof (NewVTable).GetMethod ("Overload", new Type [0]).GetParameters ().Length);
		}

		[Test]
		public void TestGetPropertyImpl() {
			// Test getting property that is exact
			AssertEquals("#01", typeof (NewVTable), typeof (NewVTable).GetProperty("Item", new Type[1] { typeof(Int32) }).DeclaringType);

			// Test getting property that is not exact
			AssertEquals("#02", typeof (NewVTable), typeof (NewVTable).GetProperty("Item", new Type[1] { typeof(Int16) }).DeclaringType);

			// Test overriding of properties when only the set accessor is overriden
			AssertEquals ("#03", 1, typeof (Derived1).GetProperties ().Length);
		}

		[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
		public class Class1 {
		}

		[StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
		public class Class2 {
		}

#if NET_2_0
		[Test]
		public void StructLayoutAttribute () {
			StructLayoutAttribute attr1 = typeof (TypeTest).StructLayoutAttribute;
			AssertEquals (LayoutKind.Auto, attr1.Value);

			StructLayoutAttribute attr2 = typeof (Class1).StructLayoutAttribute;
			AssertEquals (LayoutKind.Explicit, attr2.Value);
			AssertEquals (4, attr2.Pack);
			AssertEquals (64, attr2.Size);

			StructLayoutAttribute attr3 = typeof (Class2).StructLayoutAttribute;
			AssertEquals (LayoutKind.Explicit, attr3.Value);
			AssertEquals (CharSet.Unicode, attr3.CharSet);
		}
#endif

		[Test]
		public void Namespace () {
			AssertEquals (null, typeof (NoNamespaceClass).Namespace);
		}
	}
}

