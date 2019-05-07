// TypeTest.cs - NUnit Test Cases for the System.Type class
//
// Authors:
// 	Zoltan Varga (vargaz@freemail.hu)
//  Patrik Torstensson
//  Aleksey Kliger (aleksey@xamarin.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2015 Xamarin, Inc. (http://www.xamarin.com)
// 

using NUnit.Framework;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;

class NoNamespaceClass {
}

namespace MonoTests.System
{
	class Super : ICloneable
	{
		public virtual object Clone ()
		{
			return null;
		}
	}

	class Duper: Super
	{
	}

	interface IFace1
	{
		void foo ();
	}

	interface IFace2 : IFace1
	{
		void bar ();
	}

	interface IFace3 : IFace2
	{
	}

	enum TheEnum
	{
		A,
		B,
		C
	};

	abstract class Base
	{
		public int level;

		public abstract int this [byte i] {
			get;
		}

		public abstract int this [int i] {
			get;
		}

		public abstract void TestVoid ();
		public abstract void TestInt (int i);
	}

	class DeriveVTable : Base
	{
		public override int this [byte i] {
			get { return 1; }
		}

		public override int this [int i] {
			get { return 1; }
		}

		public override void TestVoid ()
		{
			level = 1;
		}

		public override void TestInt (int i)
		{
			level = 1;
		}
	}

	class NewVTable : DeriveVTable
	{
		public new int this [byte i] {
			get { return 2; }
		}

		public new int this [int i] {
			get { return 2; }
		}

		public new void TestVoid ()
		{
			level = 2;
		}

		public new void TestInt (int i)
		{
			level = 2;
		}

		public void Overload ()
		{
		}

		public void Overload (int i)
		{
		}

		public NewVTable (out int i)
		{
			i = 0;
		}

		public void byref_method (out int i)
		{
			i = 0;
		}
	}

	class Base1
	{
		public virtual int Foo {
			get { return 1; }
			set { }
		}

		public event EventHandler E;
		public void Dummy ()
		{
			E += delegate {};
		}
	}

	class Derived1 : Base1
	{
		public override int Foo {
			set { }
		}
	}

	class Derived2 : Base1
	{
		public new int Foo {
			get { return 1; }
			set { }
		}

		public new event Action E;
		public new void Dummy ()
		{
			E += delegate {};
		}
	}

	public class Foo<T>
	{
		public T Whatever;
	
		public T Test {
			get { throw new NotImplementedException (); }
		}

		public T Execute (T a)
		{
			return a;
		}
		
		public class Nested<K> {}
	}
	
	class Foo<T, U>
	{
	}

	public interface IBar<T>
	{
	}

	public class Baz<T> : IBar<T>
	{
	}

	class Gazonk {

		public static void Bang<S> () {}
	}

	public class Bug348522
	{
		public void Test (int __argument)
		{
		}
	}

	public class GenericIndexers<T, U>
	{
		// This class has two indexers that take different
		// arguments.  GetProperties on all instances of this
		// generic type should still have 2 properties, even
		// if T and U are instantiated with the same types.
		public T this[T t] { get { return t; } }
		public U this[U u] { get { return u; } }
	}

	public class FirstMethodBinder : Binder
	{
		public override MethodBase BindToMethod (BindingFlags bindingAttr, MethodBase [] match, ref object [] args,
							 ParameterModifier [] modifiers, CultureInfo culture, string [] names,
							 out object state)
		{
			state = null;
			return match [0];
		}
		
		public override object ChangeType (object value, Type type1, CultureInfo culture)
		{
			return value;
		}
		
		// The rest is just to please the compiler
		public override FieldInfo BindToField (BindingFlags a, FieldInfo[] b, object c, CultureInfo d)
		{
			return null;
		}
		
		public override void ReorderArgumentArray(ref object[] a, object b)
		{
		}
		
		public override MethodBase SelectMethod(BindingFlags a, MethodBase[] b, Type[] c, ParameterModifier[] d)
		{
		    return null;
		}
		
		public override PropertyInfo SelectProperty(BindingFlags a, PropertyInfo[] b, Type c, Type[] d, ParameterModifier[] e)
		{
			return null;
		}
	}

	[TestFixture]
	public class TypeTest
	{
#if !MONOTOUCH && !FULL_AOT_RUNTIME
		private ModuleBuilder module;
#endif
		const string ASSEMBLY_NAME = "MonoTests.System.TypeTest";
		static int typeIndexer = 0;
		static bool isMono = Type.GetType ("Mono.Runtime", false) != null;

		[SetUp]
		public void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());
			module = assembly.DefineDynamicModule ("module1");
#endif
		}

		private string genTypeName ()
		{
			return "t" + (typeIndexer++);
		}

		private void ByrefMethod (ref int i, ref Derived1 j, ref Base1 k)
		{
		}

		public interface IFace {
		}

		private void GenericMethod<Q, T1> (Q q, T1 t) where T1 : IFace
		{
		}

		private void GenericMethod2<A, B, C, D> ()
			where C : Duper
			where A : B, IFace
			where B : C
			where D : Baz<object>
		{
		}

		public class Nested
		{

		}

		[Test]
		public void TestIsAssignableFrom ()
		{
			// Simple tests for inheritance
			Assert.AreEqual (typeof (Super).IsAssignableFrom (typeof (Duper)) , true, "#01");
			Assert.AreEqual (typeof (Duper).IsAssignableFrom (typeof (Duper)), true, "#02");
			Assert.AreEqual (typeof (Object).IsAssignableFrom (typeof (Duper)), true, "#03");
			Assert.AreEqual (typeof (ICloneable).IsAssignableFrom (typeof (Duper)), true, "#04");

			// Tests for arrays
			Assert.AreEqual (typeof (Super[]).IsAssignableFrom (typeof (Duper[])), true, "#05");
			Assert.AreEqual (typeof (Duper[]).IsAssignableFrom (typeof (Super[])), false, "#06");
			Assert.AreEqual (typeof (Object[]).IsAssignableFrom (typeof (Duper[])), true, "#07");
			Assert.AreEqual (typeof (ICloneable[]).IsAssignableFrom (typeof (Duper[])), true, "#08");

			// Tests for multiple dimensional arrays
			Assert.AreEqual (typeof (Super[][]).IsAssignableFrom (typeof (Duper[][])), true, "#09");
			Assert.AreEqual (typeof (Duper[][]).IsAssignableFrom (typeof (Super[][])), false, "#10");
			Assert.AreEqual (typeof (Object[][]).IsAssignableFrom (typeof (Duper[][])), true, "#11");
			Assert.AreEqual (typeof (ICloneable[][]).IsAssignableFrom (typeof (Duper[][])), true, "#12");

			// Tests for vectors<->one dimensional arrays */
			Array arr1 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {0});
			Array arr2 = Array.CreateInstance (typeof (int), new int[] {1}, new int[] {10});

			Assert.AreEqual (typeof (int[]).IsAssignableFrom (arr1.GetType ()), true, "#13");
			Assert.AreEqual (typeof (int[]).IsAssignableFrom (arr2.GetType ()), false, "#14");

			// Test that arrays of enums can be cast to their base types
			Assert.AreEqual (typeof (int[]).IsAssignableFrom (typeof (TypeCode[])), true, "#15");

			// Test that arrays of valuetypes can't be cast to arrays of
			// references
			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (TypeCode[])), false, "#16");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (TypeCode[])), false, "#17");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (TypeCode[])), false, "#18");

			// Test that arrays of enums can't be cast to arrays of references
			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (TheEnum[])), false, "#19");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (TheEnum[])), false, "#20");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (TheEnum[])), false, "#21");

			// Check that ValueType and Enum are recognized as reference types
			Assert.AreEqual (typeof (object).IsAssignableFrom (typeof (ValueType)), true, "#22");
			Assert.AreEqual (typeof (object).IsAssignableFrom (typeof (Enum)), true, "#23");
			Assert.AreEqual (typeof (ValueType).IsAssignableFrom (typeof (Enum)), true, "#24");

			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (ValueType[])), true, "#25");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (ValueType[])), true, "#26");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (ValueType[])), false, "#27");

			Assert.AreEqual (typeof (object[]).IsAssignableFrom (typeof (Enum[])), true, "#28");
			Assert.AreEqual (typeof (ValueType[]).IsAssignableFrom (typeof (Enum[])), true, "#29");
			Assert.AreEqual (typeof (Enum[]).IsAssignableFrom (typeof (Enum[])), true, "#30");

			// Tests for byref types
			MethodInfo mi = typeof (TypeTest).GetMethod ("ByrefMethod", BindingFlags.Instance|BindingFlags.NonPublic);
			Assert.IsTrue (mi.GetParameters ()[2].ParameterType.IsAssignableFrom (mi.GetParameters ()[1].ParameterType));
			Assert.IsTrue (mi.GetParameters ()[1].ParameterType.IsAssignableFrom (mi.GetParameters ()[1].ParameterType));

			// Tests for type parameters
			mi = typeof (TypeTest).GetMethod ("GenericMethod", BindingFlags.Instance|BindingFlags.NonPublic);
			Assert.IsTrue (mi.GetParameters ()[0].ParameterType.IsAssignableFrom (mi.GetParameters ()[0].ParameterType));
			Assert.IsFalse (mi.GetParameters ()[0].ParameterType.IsAssignableFrom (typeof (int)));

			// Tests for parameters with generic constraints
			mi = typeof (TypeTest).GetMethod ("GenericMethod", BindingFlags.Instance|BindingFlags.NonPublic);
			Assert.IsTrue (typeof (IFace).IsAssignableFrom (mi.GetParameters ()[1].ParameterType));

			// Transitivity of IsAssignableFrom for type parameters
			mi = typeof (TypeTest).GetMethod ("GenericMethod2", BindingFlags.Instance|BindingFlags.NonPublic);
			var gparams = mi.GetGenericArguments ();
			// B : Duper since B : C and C : Duper
			Assert.IsTrue (typeof (Duper).IsAssignableFrom (gparams[1]), "#36");
			// A : Duper since A : B and B : Duper
			Assert.IsTrue (typeof (Duper).IsAssignableFrom (gparams[0]), "#37a");
			// A : IFace since A : IFace
			Assert.IsTrue (typeof (IFace).IsAssignableFrom (gparams[0]), "#37b");
			// B : Super since B : Duper and Duper : Super
			Assert.IsTrue (typeof (Super).IsAssignableFrom (gparams[1]), "#38");
			// A : Super since A : B and B : Super
			Assert.IsTrue (typeof (Super).IsAssignableFrom (gparams[0]), "#39");
			// D : IBar<object> since D : Baz<object> and Baz<object> : IBar<object>
			Assert.IsTrue (typeof (IBar<object>).IsAssignableFrom (gparams [3]), "#40");
			// A not assignable from B since A : B
			Assert.IsFalse (gparams[0].IsAssignableFrom (gparams [1]), "#41");
			Assert.IsFalse (gparams[0].IsAssignableFrom (gparams [2]), "#42");

			// A is not assignable from Array and Delegate and vice versa
			Assert.IsFalse (gparams[0].IsAssignableFrom (typeof (Array)), "#43");
			Assert.IsFalse (gparams[0].IsAssignableFrom (typeof (Delegate)), "#44");
			Assert.IsFalse (typeof (Array).IsAssignableFrom (gparams[0]), "#45");
			Assert.IsFalse (typeof (Delegate).IsAssignableFrom (gparams[0]), "#46");

		}

		[Test]
		public void GenericParameterBaseType ()
		{
			var mi = typeof (TypeTest).GetMethod ("GenericMethod2", BindingFlags.Instance|BindingFlags.NonPublic);
			var gparams = mi.GetGenericArguments ();

			// From the .NET documentation: BaseType property of a
			// gparam is "object" if its only constraints are other
			// gparams or interfaces, otherwise if it has a class
			// constraint that class is the BaseType.

			// A : B where B is a gparam, and A : IFace which is an
			// interface, so A.BaseType is object
			Assert.AreEqual (typeof (object), gparams[0].BaseType, "#1");
			// B : C where C is a gparam, so B.BaseType is object
			Assert.AreEqual (typeof (object), gparams[1].BaseType, "#2");
			// C : Duper where Duper is a class, so A.BaseType is Duper
			Assert.AreEqual (typeof (Duper), gparams[2].BaseType, "#3");
			// D : Baz<object>
			Assert.AreEqual (typeof (Baz<object>), gparams[3].BaseType, "#4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetInterfaceMapOnInterface ()
		{
			typeof (IList).GetInterfaceMap (typeof (ICollection));
		}

		[Test]
		public void TestIsSubclassOf ()
		{
			Assert.IsTrue (typeof (ICloneable).IsSubclassOf (typeof (object)), "#01");

			// Tests for byref types
			Type paramType = typeof (TypeTest).GetMethod ("ByrefMethod", BindingFlags.Instance|BindingFlags.NonPublic).GetParameters () [0].ParameterType;
			Assert.IsFalse (paramType.IsSubclassOf (typeof(ValueType)), "#02");
			Assert.IsNull (paramType.BaseType, "#02-b");
			Assert.IsTrue (paramType.IsSubclassOf (typeof (Object)), "#03");
			Assert.IsFalse (paramType.IsSubclassOf (paramType), "#04");

			// IsSubclassOf is not reflexive
			Assert.IsFalse (typeof (string).IsSubclassOf (typeof (string)), "#05");
			Assert.IsFalse (typeof (int).IsSubclassOf (typeof (int)), "#06");
			Assert.IsFalse (typeof (object).IsSubclassOf (typeof (object)), "#07");
		}

		[Test]
		public void TestGetMethodImpl ()
		{
			// Test binding of new slot methods (using no types)
			Assert.AreEqual (typeof (Base), typeof (Base).GetMethod("TestVoid").DeclaringType, "#01");
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetMethod ("TestVoid").DeclaringType, "#02");

			// Test binding of new slot methods (using types)
			Assert.AreEqual (typeof (Base), typeof (Base).GetMethod ("TestInt", new Type[] { typeof (int) }).DeclaringType, "#03");
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetMethod ("TestInt", new Type[] { typeof (int) }).DeclaringType, "#04");

			// Test overload resolution
			Assert.AreEqual (0, typeof (NewVTable).GetMethod ("Overload", new Type[0]).GetParameters ().Length, "#05");

			// Test byref parameters
			Assert.AreEqual (null, typeof (NewVTable).GetMethod ("byref_method", new Type[] { typeof (int) }), "#06");
			Type byrefInt = typeof (NewVTable).GetMethod ("byref_method").GetParameters ()[0].ParameterType;
			Assert.IsNotNull (typeof (NewVTable).GetMethod ("byref_method", new Type[] { byrefInt }), "#07");
		}

		[Test]
		public void TestGetPropertyImpl ()
		{
			// Test getting property that is exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int32) }).DeclaringType, "#01");

			// Test getting property that is not exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int16) }).DeclaringType, "#02");

			// Test overriding of properties when only the set accessor is overriden
			Assert.AreEqual (1, typeof (Derived1).GetProperties ().Length, "#03");
		}

		[Test]
		public void GetEvents ()
		{
			// Test hide-by-name
			Assert.AreEqual (1, typeof (Derived2).GetEvents ().Length);
			Assert.AreEqual (typeof (Derived2), typeof (Derived2).GetEvents ()[0].DeclaringType);
		}

		[Test]
		public void GetProperties ()
		{
			// Test hide-by-name-and-signature
			Assert.AreEqual (1, typeof (Derived2).GetProperties ().Length, "#1");
			Assert.AreEqual (typeof (Derived2), typeof (Derived2).GetProperties ()[0].DeclaringType, "#2");

			// For generics, hide-by-name-and-signature works on the unexpanded types. The
			// GenericIndexers<T,U> class has two indexers that take different arguments.
			// GetProperties on all instances of this generic type should still have 2 properties,
			// even if T and U are instantiated with the same types.

			var ps = typeof (GenericIndexers<int,int>).GetProperties ();
			Assert.AreEqual (2, ps.Length, "#3");
			for (int i = 0; i < ps.Length; i++) {
				var p = ps[i];

				var getterResultType = p.GetGetMethod ().ReturnType;

				var msg = String.Format ("#4-{0}", i);
				Assert.AreEqual (typeof (int), getterResultType, msg);
			}

		}

		class GetProperties_Overrides_Input
		{
			public class TestClass : BaseClass<object>
			{
				public override object TestProperty { get; set; }
			}

			public abstract class BaseClass<T>
			{
				public virtual T TestProperty { get; set; }
			}

			public class TestClass_Indexer : BaseClass_Indexer<object>
			{
				public override object this[int arg] { set { } }
			}

			public abstract class BaseClass_Indexer<T>
			{
				public virtual T this[int arg] { set { } }
			}

			public interface IB : IA<object>
			{
				new object TestProperty { get; set; }
			}

			public interface IA<T>
			{
				T TestProperty { get; set; }
			}

			public class TestClass_HiddenProperty : BaseClass_HiddenProperty
			{
				public new virtual string Prop { set { } }
			}

			public class BaseClass_HiddenProperty
			{
				public virtual string Prop { set  { } }
			}
		}

		[Test]
		public void GetProperties_Overrides ()
		{
			Assert.AreEqual (1, typeof (GetProperties_Overrides_Input.IB).GetProperties().Length);

			var prop = typeof (GetProperties_Overrides_Input.TestClass).GetProperty ("TestProperty");
			Assert.AreEqual (typeof (GetProperties_Overrides_Input.TestClass), prop.DeclaringType);

			var prop_2 = typeof (GetProperties_Overrides_Input.TestClass_HiddenProperty).GetProperty ("Prop");
			Assert.AreEqual (typeof (GetProperties_Overrides_Input.TestClass_HiddenProperty), prop_2.DeclaringType);

			Assert.AreEqual (1, typeof (GetProperties_Overrides_Input.TestClass).GetProperties().Length);
			Assert.AreEqual (1, typeof (GetProperties_Overrides_Input.TestClass_Indexer).GetProperties().Length);
			Assert.AreEqual (1, typeof (GetProperties_Overrides_Input.TestClass_HiddenProperty).GetProperties().Length);
	    }

		[Test] // GetProperties (BindingFlags)
		public void GetProperties_Flags ()
		{
			PropertyInfo [] props;
			Type type = typeof (Bar);
			BindingFlags flags;

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#A1");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBase"), "#A2");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBase"), "#A3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#A4");
			Assert.IsTrue (ContainsProperty (props, "IntInstBase"), "#A5");
			Assert.IsTrue (ContainsProperty (props, "PrivInst"), "#A6");
			Assert.IsTrue (ContainsProperty (props, "ProtInst"), "#A7");
			Assert.IsTrue (ContainsProperty (props, "ProIntInst"), "#A8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#A9");
			Assert.IsTrue (ContainsProperty (props, "IntInst"), "#A10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#A11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#A12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#A13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#A14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#A15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#A16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#A17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#A18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#A19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#A20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#A21");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBlue"), "#A22");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBlue"), "#A23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#A24");
			Assert.IsTrue (ContainsProperty (props, "IntInstBlue"), "#A25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#A26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#A27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#A28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#A29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#A30");

			flags = BindingFlags.Instance | BindingFlags.Public;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#B1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#B2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#B3");
			Assert.IsTrue (ContainsProperty (props, "PubInstBase"), "#B4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#B5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#B6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#B7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#B8");
			Assert.IsTrue (ContainsProperty (props, "PubInst"), "#B9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#B10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#B11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#B12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#B13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#B14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#B15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#B16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#B17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#B18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#B19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#B20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#B21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#B22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#B23");
			Assert.IsTrue (ContainsProperty (props, "PubInstBlue"), "#B24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#B25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#B26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#B27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#B28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#B29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#B30");

			flags = BindingFlags.Static | BindingFlags.Public;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#C1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#C2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#C3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#C4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#C5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#C6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#C7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#C8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#C9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#C10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#C11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#C12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#C13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#C14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#C15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#C16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#C17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#C18");
			Assert.IsTrue (ContainsProperty (props, "PubStat"), "#C19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#C20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#C21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#C22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#C23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#C24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#C25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#C26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#C27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#C28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#C29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#C30");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#D1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#D2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#D3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#D4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#D5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#D6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#D7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#D8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#D9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#D10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#D11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#D12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#D13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#D14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#D15");
			Assert.IsTrue (ContainsProperty (props, "PrivStat"), "#D16");
			Assert.IsTrue (ContainsProperty (props, "ProtStat"), "#D17");
			Assert.IsTrue (ContainsProperty (props, "ProIntStat"), "#D18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#D19");
			Assert.IsTrue (ContainsProperty (props, "IntStat"), "#D20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#D21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#D22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#D23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#D24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#D25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#D26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#D27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#D28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#D29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#D30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#E1");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBase"), "#E2");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBase"), "#E3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#E4");
			Assert.IsTrue (ContainsProperty (props, "IntInstBase"), "#E5");
			Assert.IsTrue (ContainsProperty (props, "PrivInst"), "#E6");
			Assert.IsTrue (ContainsProperty (props, "ProtInst"), "#E7");
			Assert.IsTrue (ContainsProperty (props, "ProIntInst"), "#E8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#E9");
			Assert.IsTrue (ContainsProperty (props, "IntInst"), "#E10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#E11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#E12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#E13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#E14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#E15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#E16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#E17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#E18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#E19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#E20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#E21");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBlue"), "#E22");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBlue"), "#E23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#E24");
			Assert.IsTrue (ContainsProperty (props, "IntInstBlue"), "#E25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#E26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#E27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#E28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#E29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#E30");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#F1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#F2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#F3");
			Assert.IsTrue (ContainsProperty (props, "PubInstBase"), "#F4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#F5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#F6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#F7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#F8");
			Assert.IsTrue (ContainsProperty (props, "PubInst"), "#F9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#F10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#F11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#F12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#F13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#F14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#F15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#F16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#F17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#F18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#F19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#F20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#F21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#F22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#F23");
			Assert.IsTrue (ContainsProperty (props, "PubInstBlue"), "#F24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#F25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#F26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#F27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#F28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#F29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#F30");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#G1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#G2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#G3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#G4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#G5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#G6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#G7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#G8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#G9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#G10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#G11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#G12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#G13");
			Assert.IsTrue (ContainsProperty (props, "PubStatBase"), "#G14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#G15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#G16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#G17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#G18");
			Assert.IsTrue (ContainsProperty (props, "PubStat"), "#G19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#G20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#G21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#G22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#G23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#G24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#G25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#G26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#G27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#G28");
			Assert.IsTrue (ContainsProperty (props, "PubStatBlue"), "#G29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#G30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#H1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#H2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#H3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#H4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#H5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#H6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#H7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#H8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#H9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#H10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#H11");
			Assert.IsTrue (ContainsProperty (props, "ProtStatBase"), "#H12");
			Assert.IsTrue (ContainsProperty (props, "ProIntStatBase"), "#H13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#H14");
			Assert.IsTrue (ContainsProperty (props, "IntStatBase"), "#H15");
			Assert.IsTrue (ContainsProperty (props, "PrivStat"), "#H16");
			Assert.IsTrue (ContainsProperty (props, "ProtStat"), "#H17");
			Assert.IsTrue (ContainsProperty (props, "ProIntStat"), "#H18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#H19");
			Assert.IsTrue (ContainsProperty (props, "IntStat"), "#H20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#H21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#H22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#H23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#H24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#H25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#H26");
			Assert.IsTrue (ContainsProperty (props, "ProtStatBlue"), "#H27");
			Assert.IsTrue (ContainsProperty (props, "ProIntStatBlue"), "#H28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#H29");
			Assert.IsTrue (ContainsProperty (props, "IntStatBlue"), "#H30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#I1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#I2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#I3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#I4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#I5");
			Assert.IsTrue (ContainsProperty (props, "PrivInst"), "#I6");
			Assert.IsTrue (ContainsProperty (props, "ProtInst"), "#I7");
			Assert.IsTrue (ContainsProperty (props, "ProIntInst"), "#I8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#I9");
			Assert.IsTrue (ContainsProperty (props, "IntInst"), "#I10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#I11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#I12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#I13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#I14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#I15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#I16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#I17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#I18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#I19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#I20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#I21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#I22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#I23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#I24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#I25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#I26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#I27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#I28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#I29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#I30");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#J1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#J2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#J3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#J4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#J5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#J6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#J7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#J8");
			Assert.IsTrue (ContainsProperty (props, "PubInst"), "#J9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#J10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#J11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#J12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#J13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#J14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#J15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#J16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#J17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#J18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#J19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#J20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#J21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#J22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#J23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#J24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#J25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#J26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#J27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#J28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#J29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#J30");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#K1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#K2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#K3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#K4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#K5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#K6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#K7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#K8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#K9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#K10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#K11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#K12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#K13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#K14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#K15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#K16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#K17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#K18");
			Assert.IsTrue (ContainsProperty (props, "PubStat"), "#K19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#K20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#K21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#K22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#K23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#K24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#K25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#K26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#K27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#K28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#K29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#K30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#L1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#L2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#L3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#L4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#L5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#L6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#L7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#L8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#L9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#L10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#L11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#L12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#L13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#L14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#L15");
			Assert.IsTrue (ContainsProperty (props, "PrivStat"), "#L16");
			Assert.IsTrue (ContainsProperty (props, "ProtStat"), "#L17");
			Assert.IsTrue (ContainsProperty (props, "ProIntStat"), "#L18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#L19");
			Assert.IsTrue (ContainsProperty (props, "IntStat"), "#L20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#L21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#L22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#L23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#L24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#L25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#L26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#L27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#L28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#L29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#L30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#M1");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBase"), "#M2");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBase"), "#M3");
			Assert.IsTrue (ContainsProperty (props, "PubInstBase"), "#M4");
			Assert.IsTrue (ContainsProperty (props, "IntInstBase"), "#M5");
			Assert.IsTrue (ContainsProperty (props, "PrivInst"), "#M6");
			Assert.IsTrue (ContainsProperty (props, "ProtInst"), "#M7");
			Assert.IsTrue (ContainsProperty (props, "ProIntInst"), "#M8");
			Assert.IsTrue (ContainsProperty (props, "PubInst"), "#M9");
			Assert.IsTrue (ContainsProperty (props, "IntInst"), "#M10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#M11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#M12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#M13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#M14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#M15");
			Assert.IsFalse (ContainsProperty (props, "PrivStat"), "#M16");
			Assert.IsFalse (ContainsProperty (props, "ProtStat"), "#M17");
			Assert.IsFalse (ContainsProperty (props, "ProIntStat"), "#M18");
			Assert.IsFalse (ContainsProperty (props, "PubStat"), "#M19");
			Assert.IsFalse (ContainsProperty (props, "IntStat"), "#M20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#M21");
			Assert.IsTrue (ContainsProperty (props, "ProtInstBlue"), "#M22");
			Assert.IsTrue (ContainsProperty (props, "ProIntInstBlue"), "#M23");
			Assert.IsTrue (ContainsProperty (props, "PubInstBlue"), "#M24");
			Assert.IsTrue (ContainsProperty (props, "IntInstBlue"), "#M25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#M26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#M27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#M28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#M29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#M30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			props = type.GetProperties (flags);

			Assert.IsFalse (ContainsProperty (props, "PrivInstBase"), "#N1");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBase"), "#N2");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBase"), "#N3");
			Assert.IsFalse (ContainsProperty (props, "PubInstBase"), "#N4");
			Assert.IsFalse (ContainsProperty (props, "IntInstBase"), "#N5");
			Assert.IsFalse (ContainsProperty (props, "PrivInst"), "#N6");
			Assert.IsFalse (ContainsProperty (props, "ProtInst"), "#N7");
			Assert.IsFalse (ContainsProperty (props, "ProIntInst"), "#N8");
			Assert.IsFalse (ContainsProperty (props, "PubInst"), "#N9");
			Assert.IsFalse (ContainsProperty (props, "IntInst"), "#N10");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBase"), "#N11");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBase"), "#N12");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBase"), "#N13");
			Assert.IsFalse (ContainsProperty (props, "PubStatBase"), "#N14");
			Assert.IsFalse (ContainsProperty (props, "IntStatBase"), "#N15");
			Assert.IsTrue (ContainsProperty (props, "PrivStat"), "#N16");
			Assert.IsTrue (ContainsProperty (props, "ProtStat"), "#N17");
			Assert.IsTrue (ContainsProperty (props, "ProIntStat"), "#N18");
			Assert.IsTrue (ContainsProperty (props, "PubStat"), "#N19");
			Assert.IsTrue (ContainsProperty (props, "IntStat"), "#N20");
			Assert.IsFalse (ContainsProperty (props, "PrivInstBlue"), "#N21");
			Assert.IsFalse (ContainsProperty (props, "ProtInstBlue"), "#N22");
			Assert.IsFalse (ContainsProperty (props, "ProIntInstBlue"), "#N23");
			Assert.IsFalse (ContainsProperty (props, "PubInstBlue"), "#N24");
			Assert.IsFalse (ContainsProperty (props, "IntInstBlue"), "#N25");
			Assert.IsFalse (ContainsProperty (props, "PrivStatBlue"), "#N26");
			Assert.IsFalse (ContainsProperty (props, "ProtStatBlue"), "#N27");
			Assert.IsFalse (ContainsProperty (props, "ProIntStatBlue"), "#N28");
			Assert.IsFalse (ContainsProperty (props, "PubStatBlue"), "#N29");
			Assert.IsFalse (ContainsProperty (props, "IntStatBlue"), "#N30");
		}

		[Test] // GetProperty (String)
		public void GetProperty1_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, BindingFlags)
		public void GetProperty2 ()
		{
			Type type = typeof (Bar);
			BindingFlags flags;

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#A1");
			Assert.IsNotNull (type.GetProperty ("ProtInstBase", flags), "#A2");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBase", flags), "#A3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#A4");
			Assert.IsNotNull (type.GetProperty ("IntInstBase", flags), "#A5");
			Assert.IsNotNull (type.GetProperty ("PrivInst", flags), "#A6");
			Assert.IsNotNull (type.GetProperty ("ProtInst", flags), "#A7");
			Assert.IsNotNull (type.GetProperty ("ProIntInst", flags), "#A8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#A9");
			Assert.IsNotNull (type.GetProperty ("IntInst", flags), "#A10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#A11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#A12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#A13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#A14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#A15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#A16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#A17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#A18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#A19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#A20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#A21");
			Assert.IsNotNull (type.GetProperty ("ProtInstBlue", flags), "#A22");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBlue", flags), "#A23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#A24");
			Assert.IsNotNull (type.GetProperty ("IntInstBlue", flags), "#A25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#A26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#A27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#A28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#A29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#A30");

			flags = BindingFlags.Instance | BindingFlags.Public;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#B1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#B2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#B3");
			Assert.IsNotNull (type.GetProperty ("PubInstBase", flags), "#B4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#B5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#B6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#B7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#B8");
			Assert.IsNotNull (type.GetProperty ("PubInst", flags), "#B9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#B10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#B11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#B12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#B13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#B14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#B15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#B16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#B17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#B18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#B19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#B20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#B21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#B22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#B23");
			Assert.IsNotNull (type.GetProperty ("PubInstBlue", flags), "#B24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#B25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#B26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#B27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#B28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#B29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#B30");

			flags = BindingFlags.Static | BindingFlags.Public;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#C1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#C2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#C3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#C4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#C5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#C6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#C7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#C8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#C9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#C10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#C11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#C12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#C13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#C14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#C15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#C16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#C17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#C18");
			Assert.IsNotNull (type.GetProperty ("PubStat", flags), "#C19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#C20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#C21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#C22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#C23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#C24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#C25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#C26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#C27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#C28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#C29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#C30");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#D1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#D2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#D3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#D4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#D5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#D6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#D7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#D8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#D9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#D10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#D11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#D12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#D13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#D14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#D15");
			Assert.IsNotNull (type.GetProperty ("PrivStat", flags), "#D16");
			Assert.IsNotNull (type.GetProperty ("ProtStat", flags), "#D17");
			Assert.IsNotNull (type.GetProperty ("ProIntStat", flags), "#D18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#D19");
			Assert.IsNotNull (type.GetProperty ("IntStat", flags), "#D20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#D21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#D22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#D23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#D24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#D25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#D26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#D27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#D28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#D29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#D30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#E1");
			Assert.IsNotNull (type.GetProperty ("ProtInstBase", flags), "#E2");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBase", flags), "#E3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#E4");
			Assert.IsNotNull (type.GetProperty ("IntInstBase", flags), "#E5");
			Assert.IsNotNull (type.GetProperty ("PrivInst", flags), "#E6");
			Assert.IsNotNull (type.GetProperty ("ProtInst", flags), "#E7");
			Assert.IsNotNull (type.GetProperty ("ProIntInst", flags), "#E8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#E9");
			Assert.IsNotNull (type.GetProperty ("IntInst", flags), "#E10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#E11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#E12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#E13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#E14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#E15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#E16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#E17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#E18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#E19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#E20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#E21");
			Assert.IsNotNull (type.GetProperty ("ProtInstBlue", flags), "#E22");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBlue", flags), "#E23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#E24");
			Assert.IsNotNull (type.GetProperty ("IntInstBlue", flags), "#E25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#E26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#E27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#E28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#E29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#E30");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#F1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#F2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#F3");
			Assert.IsNotNull (type.GetProperty ("PubInstBase", flags), "#F4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#F5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#F6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#F7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#F8");
			Assert.IsNotNull (type.GetProperty ("PubInst", flags), "#F9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#F10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#F11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#F12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#F13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#F14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#F15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#F16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#F17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#F18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#F19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#F20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#F21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#F22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#F23");
			Assert.IsNotNull (type.GetProperty ("PubInstBlue", flags), "#F24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#F25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#F26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#F27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#F28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#F29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#F30");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#G1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#G2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#G3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#G4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#G5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#G6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#G7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#G8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#G9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#G10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#G11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#G12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#G13");
			Assert.IsNotNull (type.GetProperty ("PubStatBase", flags), "#G14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#G15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#G16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#G17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#G18");
			Assert.IsNotNull (type.GetProperty ("PubStat", flags), "#G19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#G20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#G21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#G22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#G23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#G24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#G25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#G26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#G27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#G28");
			Assert.IsNotNull (type.GetProperty ("PubStatBlue", flags), "#G29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#G30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#H1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#H2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#H3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#H4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#H5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#H6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#H7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#H8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#H9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#H10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#H11");
			Assert.IsNotNull (type.GetProperty ("ProtStatBase", flags), "#H12");
			Assert.IsNotNull (type.GetProperty ("ProIntStatBase", flags), "#H13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#H14");
			Assert.IsNotNull (type.GetProperty ("IntStatBase", flags), "#H15");
			Assert.IsNotNull (type.GetProperty ("PrivStat", flags), "#H16");
			Assert.IsNotNull (type.GetProperty ("ProtStat", flags), "#H17");
			Assert.IsNotNull (type.GetProperty ("ProIntStat", flags), "#H18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#H19");
			Assert.IsNotNull (type.GetProperty ("IntStat", flags), "#H20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#H21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#H22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#H23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#H24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#H25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#H26");
			Assert.IsNotNull (type.GetProperty ("ProtStatBlue", flags), "#H27");
			Assert.IsNotNull (type.GetProperty ("ProIntStatBlue", flags), "#H28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#H29");
			Assert.IsNotNull (type.GetProperty ("IntStatBlue", flags), "#H30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#I1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#I2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#I3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#I4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#I5");
			Assert.IsNotNull (type.GetProperty ("PrivInst", flags), "#I6");
			Assert.IsNotNull (type.GetProperty ("ProtInst", flags), "#I7");
			Assert.IsNotNull (type.GetProperty ("ProIntInst", flags), "#I8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#I9");
			Assert.IsNotNull (type.GetProperty ("IntInst", flags), "#I10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#I11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#I12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#I13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#I14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#I15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#I16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#I17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#I18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#I19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#I20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#I21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#I22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#I23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#I24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#I25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#I26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#I27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#I28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#I29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#I30");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#J1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#J2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#J3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#J4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#J5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#J6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#J7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#J8");
			Assert.IsNotNull (type.GetProperty ("PubInst", flags), "#J9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#J10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#J11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#J12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#J13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#J14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#J15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#J16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#J17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#J18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#J19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#J20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#J21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#J22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#J23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#J24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#J25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#J26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#J27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#J28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#J29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#J30");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#K1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#K2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#K3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#K4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#K5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#K6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#K7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#K8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#K9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#K10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#K11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#K12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#K13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#K14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#K15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#K16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#K17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#K18");
			Assert.IsNotNull (type.GetProperty ("PubStat", flags), "#K19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#K20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#K21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#K22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#K23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#K24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#K25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#K26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#K27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#K28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#K29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#K30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#L1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#L2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#L3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#L4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#L5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#L6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#L7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#L8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#L9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#L10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#L11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#L12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#L13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#L14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#L15");
			Assert.IsNotNull (type.GetProperty ("PrivStat", flags), "#L16");
			Assert.IsNotNull (type.GetProperty ("ProtStat", flags), "#L17");
			Assert.IsNotNull (type.GetProperty ("ProIntStat", flags), "#L18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#L19");
			Assert.IsNotNull (type.GetProperty ("IntStat", flags), "#L20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#L21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#L22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#L23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#L24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#L25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#L26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#L27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#L28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#L29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#L30");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#M1");
			Assert.IsNotNull (type.GetProperty ("ProtInstBase", flags), "#M2");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBase", flags), "#M3");
			Assert.IsNotNull (type.GetProperty ("PubInstBase", flags), "#M4");
			Assert.IsNotNull (type.GetProperty ("IntInstBase", flags), "#M5");
			Assert.IsNotNull (type.GetProperty ("PrivInst", flags), "#M6");
			Assert.IsNotNull (type.GetProperty ("ProtInst", flags), "#M7");
			Assert.IsNotNull (type.GetProperty ("ProIntInst", flags), "#M8");
			Assert.IsNotNull (type.GetProperty ("PubInst", flags), "#M9");
			Assert.IsNotNull (type.GetProperty ("IntInst", flags), "#M10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#M11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#M12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#M13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#M14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#M15");
			Assert.IsNull (type.GetProperty ("PrivStat", flags), "#M16");
			Assert.IsNull (type.GetProperty ("ProtStat", flags), "#M17");
			Assert.IsNull (type.GetProperty ("ProIntStat", flags), "#M18");
			Assert.IsNull (type.GetProperty ("PubStat", flags), "#M19");
			Assert.IsNull (type.GetProperty ("IntStat", flags), "#M20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#M21");
			Assert.IsNotNull (type.GetProperty ("ProtInstBlue", flags), "#M22");
			Assert.IsNotNull (type.GetProperty ("ProIntInstBlue", flags), "#M23");
			Assert.IsNotNull (type.GetProperty ("PubInstBlue", flags), "#M24");
			Assert.IsNotNull (type.GetProperty ("IntInstBlue", flags), "#M25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#M26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#M27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#M28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#M29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#M30");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (type.GetProperty ("PrivInstBase", flags), "#N1");
			Assert.IsNull (type.GetProperty ("ProtInstBase", flags), "#N2");
			Assert.IsNull (type.GetProperty ("ProIntInstBase", flags), "#N3");
			Assert.IsNull (type.GetProperty ("PubInstBase", flags), "#N4");
			Assert.IsNull (type.GetProperty ("IntInstBase", flags), "#N5");
			Assert.IsNull (type.GetProperty ("PrivInst", flags), "#N6");
			Assert.IsNull (type.GetProperty ("ProtInst", flags), "#N7");
			Assert.IsNull (type.GetProperty ("ProIntInst", flags), "#N8");
			Assert.IsNull (type.GetProperty ("PubInst", flags), "#N9");
			Assert.IsNull (type.GetProperty ("IntInst", flags), "#N10");
			Assert.IsNull (type.GetProperty ("PrivStatBase", flags), "#N11");
			Assert.IsNull (type.GetProperty ("ProtStatBase", flags), "#N12");
			Assert.IsNull (type.GetProperty ("ProIntStatBase", flags), "#N13");
			Assert.IsNull (type.GetProperty ("PubStatBase", flags), "#N14");
			Assert.IsNull (type.GetProperty ("IntStatBase", flags), "#N15");
			Assert.IsNotNull (type.GetProperty ("PrivStat", flags), "#N16");
			Assert.IsNotNull (type.GetProperty ("ProtStat", flags), "#N17");
			Assert.IsNotNull (type.GetProperty ("ProIntStat", flags), "#N18");
			Assert.IsNotNull (type.GetProperty ("PubStat", flags), "#N19");
			Assert.IsNotNull (type.GetProperty ("IntStat", flags), "#N20");
			Assert.IsNull (type.GetProperty ("PrivInstBlue", flags), "#N21");
			Assert.IsNull (type.GetProperty ("ProtInstBlue", flags), "#N22");
			Assert.IsNull (type.GetProperty ("ProIntInstBlue", flags), "#N23");
			Assert.IsNull (type.GetProperty ("PubInstBlue", flags), "#N24");
			Assert.IsNull (type.GetProperty ("IntInstBlue", flags), "#N25");
			Assert.IsNull (type.GetProperty ("PrivStatBlue", flags), "#N26");
			Assert.IsNull (type.GetProperty ("ProtStatBlue", flags), "#N27");
			Assert.IsNull (type.GetProperty ("ProIntStatBlue", flags), "#N28");
			Assert.IsNull (type.GetProperty ("PubStatBlue", flags), "#N29");
			Assert.IsNull (type.GetProperty ("IntStatBlue", flags), "#N30");
		}

		[Test] // GetProperty (String, BindingFlags)
		public void GetProperty2_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, Type)
		public void GetProperty3_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null, typeof (int));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, Type [])
		public void GetProperty4_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null, Type.EmptyTypes);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, Type, Type [])
		public void GetProperty5_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null, typeof (int),
					Type.EmptyTypes);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, Type, Type [], ParameterModifier [])
		public void GetProperty6_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null, typeof (int),
					Type.EmptyTypes, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test] // GetProperty (String, BindingFlags, Binder, Type, Type [], ParameterModifier [])
		public void GetProperty7_Name_Null ()
		{
			Type type = typeof (Bar);
			try {
				type.GetProperty ((string) null, BindingFlags.Public,
					null, typeof (int), Type.EmptyTypes, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetProperty8_PropertyType ()
		{
			Type type = typeof (Bar);
			Assert.IsNull (type.GetProperty ("PubInst", BindingFlags.Public | BindingFlags.Instance,
							 null, typeof (int), Type.EmptyTypes, null), "#1");
			Assert.IsNotNull (type.GetProperty ("PubInst", BindingFlags.Public | BindingFlags.Instance, null, 
							    typeof (long), new Type[0], null), "#2");
		}

		[Test]
		public void GetProperty9_Indexers ()
		{

			var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

			Type type1 = typeof (List<byte>);
			var p1 = type1.GetProperty ("Item", bindingFlags, null, typeof (byte), new Type[] { typeof (int) }, null);
			Assert.IsNotNull (p1, "#1");

			Type type2 = typeof (List<string>);
			var p2 = type2.GetProperty ("Item", bindingFlags, null, typeof (string), new Type[] { typeof (int) }, null);
			Assert.IsNotNull (p2, "#2");

			Type type3 = typeof (List<Type>);
			// result type not convertible, make sure we fail.
			var p3 = type3.GetProperty ("Item", bindingFlags, null, typeof (string) /*!*/,
						    new Type[] { typeof (int) }, null);
			Assert.IsNull (p3, "#3");
		}

		[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
		public class Class1
		{
		}

		[StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
		public class Class2
		{
		}

		[Test]
		public void StructLayoutAttribute ()
		{
			StructLayoutAttribute attr1 = typeof (TypeTest).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Auto, attr1.Value);

			StructLayoutAttribute attr2 = typeof (Class1).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr2.Value);
			Assert.AreEqual (4, attr2.Pack);
			Assert.AreEqual (64, attr2.Size);

			StructLayoutAttribute attr3 = typeof (Class2).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr3.Value);
			Assert.AreEqual (CharSet.Unicode, attr3.CharSet);
		}

		[Test]
		public void Namespace ()
		{
			Assert.AreEqual (null, typeof (NoNamespaceClass).Namespace);
		}

		[Test]
		public void GenericParameterNamespace ()
		{
			var t = typeof (Foo<>).GetGenericArguments () [0];

			Assert.AreEqual ("T", t.Name);
			Assert.AreEqual ("MonoTests.System", t.Namespace);

			var s = typeof (Gazonk).GetMethod ("Bang").GetGenericArguments () [0];

			Assert.AreEqual ("S", s.Name);
			Assert.AreEqual ("MonoTests.System", s.Namespace);
		}

		public static void Reflected (ref int a)
		{
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("Int32&", typeof (TypeTest).GetMethod ("Reflected").GetParameters () [0].ParameterType.Name);
			Assert.AreEqual ("String[*]", Array.CreateInstance (typeof(string), new int[] { 1 }, new int[] { 1 }).GetType ().Name);
		}

		[Test]
		public void GetInterfaces ()
		{
			Type[] t = typeof (Duper).GetInterfaces ();
			Assert.AreEqual (1, t.Length);
			Assert.AreEqual (typeof (ICloneable), t[0]);

			Type[] t2 = typeof (IFace3).GetInterfaces ();
			Assert.AreEqual (2, t2.Length);
		}

		[Test]
		public void GetInterfacesGenericVarWithConstraints ()
		{
			var a = typeof (TypeTest).GetMethod ("GenericMethod");

			var p = a.GetParameters ();
			var i = p[0].ParameterType.GetElementType ();
			i.GetInterfaces ();
		}

		public static void GenericMethod<T, T2> (T[] arr) where T: IComparable<T> {
		}

		public int AField;

		[Test]
		public void GetFieldIgnoreCase ()
		{
			Assert.IsNotNull (typeof (TypeTest).GetField ("afield", BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase));
		}

		public int Count {
			internal get {
				return 0;
			}

			set {
			}
		}

		[Test]
		public void GetPropertyAccessorModifiers ()
		{
			Assert.IsNotNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.Public));
			Assert.IsNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.NonPublic));
		}

		[Test]
		public void IsAbstract ()
		{
			Assert.IsFalse (typeof (string).IsAbstract, "#1");
			Assert.IsTrue (typeof (ICloneable).IsAbstract, "#2");
			Assert.IsTrue (typeof (ValueType).IsAbstract, "#3");
			Assert.IsTrue (typeof (Enum).IsAbstract, "#4");
			Assert.IsFalse (typeof (TimeSpan).IsAbstract, "#5");
			Assert.IsTrue (typeof (TextReader).IsAbstract, "#6");

			// LAMESPEC:
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=286308
			Type [] typeArgs = typeof (List<>).GetGenericArguments ();
			Assert.IsFalse (typeArgs [0].IsAbstract, "#7");
		}
#if !MOBILE
		[Test]
		public void IsCOMObject ()
		{
			Type type = typeof (string);
			Assert.IsFalse (type.IsCOMObject, "#1");

			TypeBuilder tb = module.DefineType (genTypeName ());
			type = tb.CreateType ();
			Assert.IsFalse (type.IsCOMObject, "#2");
		}

		[Test]
		public void IsImport ()
		{
			Type type = typeof (string);
			Assert.IsFalse (type.IsImport, "#1");

			TypeBuilder tb = module.DefineType (genTypeName ());
			type = tb.CreateType ();
			Assert.IsFalse (type.IsImport, "#2");

			tb = module.DefineType (genTypeName (), TypeAttributes.Import |
				TypeAttributes.Interface | TypeAttributes.Abstract);
			type = tb.CreateType ();
			Assert.IsTrue (type.IsImport, "#3");
		}
#endif
		[Test]
		public void IsInterface ()
		{
			Assert.IsFalse (typeof (string).IsInterface, "#1");
			Assert.IsTrue (typeof (ICloneable).IsInterface, "#2");
		}

		[Test]
		public void IsPrimitive () {
			Assert.IsTrue (typeof (IntPtr).IsPrimitive, "#1");
			Assert.IsTrue (typeof (int).IsPrimitive, "#2");
			Assert.IsFalse (typeof (string).IsPrimitive, "#2");
		}

		[Test]
		public void IsValueType ()
		{
			Assert.IsTrue (typeof (int).IsValueType, "#1");
			Assert.IsFalse (typeof (Enum).IsValueType, "#2");
			Assert.IsFalse (typeof (ValueType).IsValueType, "#3");
			Assert.IsTrue (typeof (AttributeTargets).IsValueType, "#4");
			Assert.IsFalse (typeof (string).IsValueType, "#5");
			Assert.IsTrue (typeof (TimeSpan).IsValueType, "#6");
		}

		[Test]
		public void IsVisible ()
		{
			Assert.IsTrue (typeof (int).IsVisible, "#1");
			Assert.IsTrue (typeof (Nested).IsVisible, "#2");
		}

		[Test]
		public void GetTypeNonVectorArray ()
		{
			Type t = Type.GetType ("System.String[*]");
			Assert.AreEqual ("System.String[*]", t.ToString ());
		}

#if MONO_COM
		[Test]
		public void TypeFromCLSID ()
		{
			Guid CLSID_ShellDesktop = new Guid("00021400-0000-0000-c000-000000000046");
			Guid CLSID_Bogus = new Guid("1ea9d7a9-f7ab-443b-b486-30d285b21f1b");

			Type t1 = Type.GetTypeFromCLSID (CLSID_ShellDesktop);

			Type t2 = Type.GetTypeFromCLSID (CLSID_Bogus);

			Assert.AreEqual (t1.FullName, "System.__ComObject");

			if (!isMono && (Environment.OSVersion.Platform == PlatformID.Win32Windows ||
				Environment.OSVersion.Platform == PlatformID.Win32NT))
				Activator.CreateInstance(t1);

			Assert.AreEqual (t2.FullName, "System.__ComObject");

			Assert.AreNotEqual (t1, t2);
		}

		[Test]
		[Category("NotWorking")] // Mono throws TargetInvokationException
		[ExpectedException("System.Runtime.InteropServices.COMException")]
		public void TypeFromCLSIDBogus ()
		{
			Guid CLSID_Bogus = new Guid("1ea9d7a9-f7ab-443b-b486-30d285b21f1b");
			Type t = Type.GetTypeFromCLSID (CLSID_Bogus);
			if (Environment.OSVersion.Platform == PlatformID.Win32Windows ||
				Environment.OSVersion.Platform == PlatformID.Win32NT)
				Activator.CreateInstance(t);
			else
				throw new COMException ();
		}
#endif
		[Test]
		public void ExerciseFilterName ()
		{
			MemberInfo[] mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "*");
			Assert.AreEqual (4, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "Test*");
			Assert.AreEqual (2, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "TestVoid");
			Assert.AreEqual (1, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterName, "NonExistingMethod");
			Assert.AreEqual (0, mi.Length);
		}
		
		[Test]
		public void ExerciseFilterNameIgnoreCase ()
		{
			MemberInfo[] mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "*");
			Assert.AreEqual (4, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "test*");
			Assert.AreEqual (2, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "TESTVOID");
			Assert.AreEqual (1, mi.Length);
			mi = typeof(Base).FindMembers(
				MemberTypes.Method, 
				BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly,
				Type.FilterNameIgnoreCase, "NonExistingMethod");
			Assert.AreEqual (0, mi.Length);
		}

		[Test]
		[ExpectedException (typeof (InvalidFilterCriteriaException))]
		public void FilterAttribute_Invalid ()
		{
			Type.FilterAttribute (MethodBase.GetCurrentMethod (), (byte) 1);
		}

		[Test]
		public void GenericParameterMemberType ()
		{
			var t = typeof (Foo<>).GetGenericArguments () [0];
			Assert.IsNotNull (t);

			Assert.AreEqual (MemberTypes.TypeInfo, t.MemberType);
		}

		public class ByRef0
		{
			public int field;
			public int property {
				get { return 0; }
			}
			public ByRef0 (int i) {}
			public void f (int i) {}
		}

		[Test]
		public void ByrefTypes ()
		{
			Type t = Type.GetType ("MonoTests.System.TypeTest+ByRef0&");
			Assert.IsNotNull (t);
			Assert.IsTrue (t.IsByRef);
			Assert.AreEqual (0, t.GetMethods (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetConstructors (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetEvents (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetProperties (BindingFlags.Public | BindingFlags.Instance).Length);

			Assert.IsNull (t.GetMethod ("f"));
			Assert.IsNull (t.GetField ("field"));
			Assert.IsNull (t.GetProperty ("property"));
		}
		
		[Test]
		public void TestAssemblyQualifiedName ()
		{
			Type t = Type.GetType ("System.Byte[]&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte[]&"));
			
			t = Type.GetType ("System.Byte*&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte*&"));
			
			t = Type.GetType ("System.Byte&");
			Assert.IsTrue (t.AssemblyQualifiedName.StartsWith ("System.Byte&"));
		}

		struct B
		{
			#pragma warning disable 169
			int value;
			#pragma warning restore 169
		}

		[Test]
		public void CreateValueTypeNoCtor ()
		{
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateValueTypeNoCtorArgs ()
		{
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, new object [] { 1 });
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void InvokeGetPropertyMissing ()
		{
			typeof(B).InvokeMember ("", BindingFlags.GetProperty, null, null, new object [] { 1 });
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void InvokeSetPropertyMissing ()
		{
			typeof(B).InvokeMember ("", BindingFlags.SetProperty, null, null, new object [] { 1 });
		}

		internal static string bug336841 (string param1, params string [] param2)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("#A:");
			sb.Append (param1);
			sb.Append ("|");
			for (int i = 0; i < param2.Length; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (param2 [i]);
			}
			return sb.ToString ();
		}

		internal static string bug336841 (string param1)
		{
			return "#B:" + param1;
		}

		internal static string bug336841 (params string [] param1)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("#C:");
			for (int i = 0; i < param1.Length; i++) {
				if (i > 0)
					sb.Append (";");
				sb.Append (param1 [i]);
			}
			return sb.ToString ();
		}

		[Test]
		public void InvokeMember_GetSetField ()
		{
			typeof (X).InvokeMember ("Value", BindingFlags.Public |
				BindingFlags.Static | BindingFlags.FlattenHierarchy |
				BindingFlags.SetField, null, null, new object [] { 5 });

			Assert.AreEqual (5, X.Value, "#A1");
			Assert.AreEqual (5, typeof (X).InvokeMember ("Value",
				BindingFlags.Public | BindingFlags.Static |
				BindingFlags.FlattenHierarchy | BindingFlags.GetField,
				null, null, new object [0]), "#A2");
			Assert.AreEqual (5, Y.Value, "#A3");
			Assert.AreEqual (5, typeof (Y).InvokeMember ("Value",
				BindingFlags.Public | BindingFlags.Static |
				BindingFlags.FlattenHierarchy | BindingFlags.GetField,
				null, null, new object [0]), "#A4");

			try {
				typeof (X).InvokeMember ("Value", BindingFlags.Public |
					BindingFlags.Static | BindingFlags.FlattenHierarchy |
					BindingFlags.GetField | BindingFlags.SetField,
					null, null, new object [] { 5 });
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot specify both Get and Set on a field
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void InvokeMember_GetSetProperty ()
		{
			try {
				typeof (ArrayList).InvokeMember ("Item",
					BindingFlags.GetProperty | BindingFlags.SetProperty |
					BindingFlags.Instance | BindingFlags.Public,
					null, new ArrayList (), new object [] { 0, "bar" });
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Cannot specify both Get and Set on a property
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#6");
			}
		}


		[Test]
		public void InvokeMember_InvokeMethod_Set ()
		{
			try {
				typeof (ArrayList).InvokeMember ("ToString",
					BindingFlags.InvokeMethod | BindingFlags.SetField |
					BindingFlags.Instance | BindingFlags.Public,
					null, new ArrayList (), new object [0]);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot specify Set on a field and Invoke on a method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#A6");
			}

			try {
				typeof (ArrayList).InvokeMember ("ToString",
					BindingFlags.InvokeMethod | BindingFlags.SetProperty |
					BindingFlags.Instance | BindingFlags.Public,
					null, new ArrayList (), new object [0]);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot specify Set on a property and Invoke on a method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void InvokeMember_MatchPrimitiveTypeWithInterface ()
		{
			object [] invokeargs = { 1 };
			typeof (Z).InvokeMember ("", BindingFlags.DeclaredOnly |
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.CreateInstance,
				null, null, invokeargs);
		}

		[Test]
		public void InvokeMember_Name_Null ()
		{
			try {
				typeof (X).InvokeMember ((string) null,
					BindingFlags.Public | BindingFlags.Static |
					BindingFlags.FlattenHierarchy | BindingFlags.SetField,
					null, null, new object [] { 5 });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}
		}

		[Test]
		public void InvokeMember_NoOperation ()
		{
			try {
				typeof (TypeTest).InvokeMember ("Run", BindingFlags.Public |
					BindingFlags.Static, null, null, new object [0]);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Must specify binding flags describing the
				// invoke operation required
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#6");
			}
		}

		[Test] // bug #321735
		public void InvokeMember_SetFieldProperty ()
		{
			ArrayList list = new ArrayList ();
			list.Add ("foo");
			list.GetType ().InvokeMember ("Item",
				BindingFlags.SetField | BindingFlags.SetProperty |
				BindingFlags.Instance | BindingFlags.Public,
				null, list, new object [] { 0, "bar" });
			Assert.AreEqual ("bar", list [0]);
		}

		[Test]
		public void InvokeMember_SetField_ProvidedArgs ()
		{
			try {
				typeof (X).InvokeMember ("Value", BindingFlags.Public |
					BindingFlags.Static | BindingFlags.SetField,
					null, null, new object [0]);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Only the field value can be specified to set
				// a field value
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("bindingFlags", ex.ParamName, "#6");
			}

			try {
				typeof (X).InvokeMember ("Value", BindingFlags.Public |
					BindingFlags.Static | BindingFlags.SetField,
					null, null, null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("providedArgs", ex.ParamName, "#B6");
			}
		}

		[Test] // bug #336841
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=306797
		public void InvokeMember_VarArgs ()
		{
			BindingFlags flags = BindingFlags.InvokeMethod | BindingFlags.Public |
				BindingFlags.NonPublic | BindingFlags.OptionalParamBinding |
				BindingFlags.Static | BindingFlags.FlattenHierarchy |
				BindingFlags.Instance;

			Type type = typeof (TypeTest);
			string result = (string) type.InvokeMember ("bug336841",
				flags, null, null, new object [] { "1" });
			Assert.IsNotNull (result, "#A1");
			Assert.AreEqual ("#B:1", result, "#A2");

			result = (string) type.InvokeMember ("bug336841", flags,
				null, null, new object [] { "1", "2", "3", "4" });
			Assert.IsNotNull (result, "#B1");
			Assert.AreEqual ("#A:1|2,3,4", result, "#B2");
		}

	
		[Test] // bug #348522
		public void InvokeMember_WithoutDefaultValue ()
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
			try {
				typeof (Bug348522).InvokeMember ("Test", flags, new FirstMethodBinder (), new Bug348522(),
					new object [] {Missing.Value}, null, null, null);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Missing parameter does not have a default value
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("parameters", ex.ParamName, "#6");
			}
		}

	    [Test]
		public void TestMissing () {
			Assert.AreEqual (Type.Missing, Missing.Value);
		}

		[Test]
		public void GetGenericMethodDefinitionOverInflatedMethodOnGTD () {
			var s = new List<int> () { 1, 2, 3 }.ConvertAll ( i => i.ToString () );
			Assert.AreEqual (3, s.Count);
			var l = typeof (List<>);
			var m = l.GetMethod ("ConvertAll");
			var infl = m.MakeGenericMethod (typeof (int));
			var res = m.GetGenericMethodDefinition ();
			Assert.AreEqual (m, res, "#1");
			Assert.AreEqual (1, infl.GetGenericArguments().Length, "#2");
		}

		[Test]
		public void InvokeMember_OutParam ()
		{
			object[] args = new object[] { new string [0] };
			typeof (TypeTest).InvokeMember ("OutTest", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, args);
			Assert.IsTrue (args [0] is string[]);
			Assert.AreEqual (10, ((string[])args[0]).Length);
		}

		public static void OutTest (out string[] a1)
		{
			a1 = new string [10];
		}

		public class X
		{
			public static int Value;
		}

		class Y : X
		{
		}

		class Z
		{
			public Z (IComparable value)
			{
			}
		}
	
		public static void Run ()
		{
		}

		class TakesInt
		{
			private int i;

			public TakesInt (int x)
			{
				i = x;
			}

			public int Integer {
				get { return i; }
			}
		}

		class TakesObject
		{
			public TakesObject (object x) {}
		}

		[Test] // bug #75241
		public void GetConstructorNullInTypes ()
		{
			// This ends up calling type.GetConstructor ()
			Activator.CreateInstance (typeof (TakesInt), new object [] { null });
			Activator.CreateInstance (typeof (TakesObject), new object [] { null });
		}

		[Test]
		public void GetConstructor_TakeInt_Object ()
		{
			Assert.IsNull (typeof (TakesInt).GetConstructor (new Type[1] { typeof (object) }));
		}

		[Test]
		public void GetCustomAttributes_All ()
		{
			object [] attrs = typeof (A).GetCustomAttributes (false);
			Assert.AreEqual (2, attrs.Length, "#A1");
			Assert.IsTrue (HasAttribute (attrs, typeof (FooAttribute)), "#A2");
			Assert.IsTrue (HasAttribute (attrs, typeof (VolatileModifier)), "#A3");

			attrs = typeof (BA).GetCustomAttributes (false);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#B2");

			attrs = typeof (BA).GetCustomAttributes (true);
			Assert.AreEqual (2, attrs.Length, "#C1");
			Assert.IsTrue (HasAttribute (attrs, typeof (BarAttribute)), "#C2");
			Assert.IsTrue (HasAttribute (attrs, typeof (VolatileModifier)), "#C3");

			attrs = typeof (CA).GetCustomAttributes (false);
			Assert.AreEqual (0, attrs.Length, "#D");

			attrs = typeof (CA).GetCustomAttributes (true);
			Assert.AreEqual (1, attrs.Length, "#E1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#E2");
		}

		static bool HasAttribute (object [] attrs, Type attributeType)
		{
			foreach (object attr in attrs)
				if (attr.GetType () == attributeType)
					return true;
			return false;
		}

		[Test]
		public void GetCustomAttributes_Type ()
		{
			object [] attrs = null;

			attrs = typeof (A).GetCustomAttributes (
				typeof (VolatileModifier), false);
			Assert.AreEqual (1, attrs.Length, "#A1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#A2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (VolatileModifier), true);
			Assert.AreEqual (1, attrs.Length, "#A3");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#A4");

			attrs = typeof (A).GetCustomAttributes (
				typeof (NemerleAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#B2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (NemerleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#B3");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#B4");

			attrs = typeof (A).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C1");
			Assert.AreEqual (typeof (FooAttribute), attrs [0].GetType (), "#C2");
			attrs = typeof (A).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#C3");
			Assert.AreEqual (typeof (FooAttribute), attrs [0].GetType (), "#C4");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (VolatileModifier), false);
			Assert.AreEqual (0, attrs.Length, "#D1");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (VolatileModifier), true);
			Assert.AreEqual (1, attrs.Length, "#D2");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#D3");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (NemerleAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#E1");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (NemerleAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#E2");
			Assert.AreEqual (typeof (VolatileModifier), attrs [0].GetType (), "#E3");

			attrs = typeof (BA).GetCustomAttributes (
				typeof (FooAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#F1");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#F2");
			attrs = typeof (BA).GetCustomAttributes (
				typeof (FooAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#F3");
			Assert.AreEqual (typeof (BarAttribute), attrs [0].GetType (), "#F4");

			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#G1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#G2");
			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#G3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#G4");

			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#H1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#H2");
			attrs = typeof (bug82431A1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#H3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#H4");

			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#I1");
			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#I2");

			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#J1");
			attrs = typeof (bug82431A2).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#J2");

			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (2, attrs.Length, "#K1");
			Assert.IsTrue (HasAttribute (attrs, typeof (InheritAttribute)), "#K2");
			Assert.IsTrue (HasAttribute (attrs, typeof (NotInheritAttribute)), "#K3");
			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (2, attrs.Length, "#K4");
			Assert.IsTrue (HasAttribute (attrs, typeof (InheritAttribute)), "#K5");
			Assert.IsTrue (HasAttribute (attrs, typeof (NotInheritAttribute)), "#K6");

			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#L1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#L2");
			attrs = typeof (bug82431A3).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#L3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#L4");

			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#M1");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#M2");
			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#M3");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#M4");

			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#N1");
			attrs = typeof (bug82431B1).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#N2");

			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#O1");
			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#O2");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#O3");

			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#P1");
			attrs = typeof (bug82431B2).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#P2");

			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#Q1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#Q2");
			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (2, attrs.Length, "#Q3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#Q4");
			Assert.AreEqual (typeof (InheritAttribute), attrs [1].GetType (), "#Q5");

			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (1, attrs.Length, "#R1");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#R2");
			attrs = typeof (bug82431B3).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#R3");
			Assert.AreEqual (typeof (NotInheritAttribute), attrs [0].GetType (), "#R4");

			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (InheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#S1");
			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (InheritAttribute), true);
			Assert.AreEqual (1, attrs.Length, "#S2");
			Assert.AreEqual (typeof (InheritAttribute), attrs [0].GetType (), "#S3");

			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (NotInheritAttribute), false);
			Assert.AreEqual (0, attrs.Length, "#T1");
			attrs = typeof (bug82431B4).GetCustomAttributes (
				typeof (NotInheritAttribute), true);
			Assert.AreEqual (0, attrs.Length, "#T2");

			attrs = typeof (A).GetCustomAttributes (
				typeof (string), false);
			Assert.AreEqual (0, attrs.Length, "#U1");
			attrs = typeof (A).GetCustomAttributes (
				typeof (string), true);
			Assert.AreEqual (0, attrs.Length, "#U2");
		}

		[Test] // bug #76150
		public void IsDefined ()
		{
			Assert.IsTrue (typeof (A).IsDefined (typeof (NemerleAttribute), false), "#A1");
			Assert.IsTrue (typeof (A).IsDefined (typeof (VolatileModifier), false), "#A2");
			Assert.IsTrue (typeof (A).IsDefined (typeof (FooAttribute), false), "#A3");
			Assert.IsFalse (typeof (A).IsDefined (typeof (BarAttribute), false), "#A4");

			Assert.IsFalse (typeof (BA).IsDefined (typeof (NemerleAttribute), false), "#B1");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (VolatileModifier), false), "#B2");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (FooAttribute), false), "#B3");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (BarAttribute), false), "#B4");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (string), false), "#B5");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (int), false), "#B6");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (NemerleAttribute), true), "#B7");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (VolatileModifier), true), "#B8");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (FooAttribute), true), "#B9");
			Assert.IsTrue (typeof (BA).IsDefined (typeof (BarAttribute), true), "#B10");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (string), true), "#B11");
			Assert.IsFalse (typeof (BA).IsDefined (typeof (int), true), "#B12");
		}

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			try {
				typeof (BA).IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test] // bug #82431
		[Category ("NotWorking")]
		public void IsDefined_Inherited ()
		{
			Assert.IsFalse (typeof (CA).IsDefined (typeof (NemerleAttribute), false), "#C1");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (VolatileModifier), false), "#C2");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (FooAttribute), false), "#C3");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (BarAttribute), false), "#C4");
			Assert.IsTrue (typeof (CA).IsDefined (typeof (NemerleAttribute), true), "#C5");
			Assert.IsTrue (typeof (CA).IsDefined (typeof (VolatileModifier), true), "#C6");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (FooAttribute), true), "#C7");
			Assert.IsFalse (typeof (CA).IsDefined (typeof (BarAttribute), true), "#C8");

			Assert.IsFalse (typeof (BBA).IsDefined (typeof (NemerleAttribute), false), "#D1");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (VolatileModifier), false), "#D2");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (FooAttribute), false), "#D3");
			Assert.IsFalse (typeof (BBA).IsDefined (typeof (BarAttribute), false), "#D4");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (NemerleAttribute), true), "#D5");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (VolatileModifier), true), "#D6");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (FooAttribute), true), "#D7");
			Assert.IsTrue (typeof (BBA).IsDefined (typeof (BarAttribute), true), "#D8");

			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (InheritAttribute), false), "#E1");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (NotInheritAttribute), false), "#E2");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (InheritAttribute), true), "#E3");
			Assert.IsTrue (typeof (bug82431A1).IsDefined (typeof (NotInheritAttribute), true), "#E4");

			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (InheritAttribute), false), "#F1");
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (NotInheritAttribute), false), "#F2");
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (InheritAttribute), true), "#F3");
			Assert.IsFalse (typeof (bug82431A2).IsDefined (typeof (NotInheritAttribute), true), "#F4");

			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (InheritAttribute), false), "#G1");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (NotInheritAttribute), false), "#G2");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (InheritAttribute), true), "#G3");
			Assert.IsTrue (typeof (bug82431A3).IsDefined (typeof (NotInheritAttribute), true), "#G4");

			Assert.IsTrue (typeof (bug82431B1).IsDefined (typeof (InheritAttribute), false), "#H1");
			Assert.IsFalse (typeof (bug82431B1).IsDefined (typeof (NotInheritAttribute), false), "#H2");
			Assert.IsTrue (typeof (bug82431B1).IsDefined (typeof (InheritAttribute), true), "#H3");
			Assert.IsFalse (typeof (bug82431B1).IsDefined (typeof (NotInheritAttribute), true), "#H4");

			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (InheritAttribute), false), "#I1");
			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (NotInheritAttribute), false), "#I2");
			Assert.IsTrue (typeof (bug82431B2).IsDefined (typeof (InheritAttribute), true), "#I3");
			Assert.IsFalse (typeof (bug82431B2).IsDefined (typeof (NotInheritAttribute), true), "#I4");

			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (InheritAttribute), false), "#J1");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (NotInheritAttribute), false), "#J2");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (InheritAttribute), true), "#J3");
			Assert.IsTrue (typeof (bug82431B3).IsDefined (typeof (NotInheritAttribute), true), "#J4");

			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (InheritAttribute), false), "#K2");
			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (NotInheritAttribute), false), "#K2");
			Assert.IsTrue (typeof (bug82431B4).IsDefined (typeof (InheritAttribute), true), "#K3");
			Assert.IsFalse (typeof (bug82431B4).IsDefined (typeof (NotInheritAttribute), true), "#K4");
		}

		class Bug13767Attribute : Attribute
		{
			public object[] field;

			public Bug13767Attribute (params object[] args)
			{
				field = args;
			}
		}

		public enum Bug13767Enum
		{
			Value0,
			Value1,
		}

		[Bug13767("Demo", new[] { Bug13767Enum.Value1, Bug13767Enum.Value0 })]
		public void Bug13767Method(string attributeName, Bug13767Enum[]options)
		{

		}

		[Test] //Bug 13767
		public void CustomAttributeWithNestedArrayOfEnum ()
		{
			var m = GetType ().GetMethod ("Bug13767Method");

			var attr = m.GetCustomAttributes (false);
			Assert.AreEqual (1, attr.Length, "#1");

			var tc = (Bug13767Attribute)attr[0];
			Assert.AreEqual (2, tc.field.Length, "#2");
			Assert.AreEqual ("Demo", tc.field[0], "#3");
			Assert.IsNotNull (tc.field[1], "#4");

			var arr = (Bug13767Enum[])tc.field [1];
			Assert.AreEqual (2, arr.Length, "#5");
			Assert.AreEqual (Bug13767Enum.Value1, arr [0], "#6");
			Assert.AreEqual (Bug13767Enum.Value0, arr [1], "#7");
		}

		[Test] // GetType (String)
		public void GetType1_TypeName_Null ()
		{
			try {
				Type.GetType ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("TypeName", ex.ParamName, "#5");
			}
		}

		[Test] // GetType (String, Boolean)
		public void GetType2_TypeName_Null ()
		{
			try {
				Type.GetType ((string) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("TypeName", ex.ParamName, "#5");
			}
		}

		[Test] // GetType (String, Boolean, Boolean)
		public void GetType3_TypeName_Null ()
		{
			try {
				Type.GetType ((string) null, false, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("TypeName", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetType1_TypeName_Empty_nothrow ()
		{
			var t = Type.GetType ("");
			Assert.IsNull (t);
		}

		[Test]
		[ExpectedException (typeof (TypeLoadException))]
		public void GetType2_TypeName_Empty ()
		{
			Type.GetType ("", true);
		}

		[Test]
		public void GetType2_TypeName_Empty_nothrow ()
		{
			var t = Type.GetType ("", false);
			Assert.IsNull (t);
		}

		[Test]
		[ExpectedException (typeof (TypeLoadException))]
		public void GetType3_TypeName_Empty ()
		{
			Type.GetType ("", true, false);
		}

		[Test]
		public void GetType3_TypeName_Empty_nothrow ()
		{
			var t = Type.GetType ("", false, false);
			Assert.IsNull (t);
		}

		[Test]
		public void GetTypeArray_Args_Null ()
		{
			try {
				Type.GetTypeArray ((object []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("args", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetTypeCode ()
		{
			Assert.AreEqual (TypeCode.Boolean, Type.GetTypeCode (typeof (bool)), "#1");
			Assert.AreEqual (TypeCode.Byte, Type.GetTypeCode (typeof (byte)), "#2");
			Assert.AreEqual (TypeCode.Char, Type.GetTypeCode (typeof (char)), "#3");
			Assert.AreEqual (TypeCode.DateTime, Type.GetTypeCode (typeof (DateTime)), "#4");
			Assert.AreEqual (TypeCode.DBNull, Type.GetTypeCode (typeof (DBNull)), "#5");
			Assert.AreEqual (TypeCode.Decimal, Type.GetTypeCode (typeof (decimal)), "#6");
			Assert.AreEqual (TypeCode.Double, Type.GetTypeCode (typeof (double)), "#7");
			Assert.AreEqual (TypeCode.Empty, Type.GetTypeCode (null), "#8");
			Assert.AreEqual (TypeCode.Int16, Type.GetTypeCode (typeof (short)), "#9");
			Assert.AreEqual (TypeCode.Int32, Type.GetTypeCode (typeof (int)), "#10");
			Assert.AreEqual (TypeCode.Int64, Type.GetTypeCode (typeof (long)), "#11");
			Assert.AreEqual (TypeCode.Object, Type.GetTypeCode (typeof (TakesInt)), "#12");
			Assert.AreEqual (TypeCode.SByte, Type.GetTypeCode (typeof (sbyte)), "#13");
			Assert.AreEqual (TypeCode.Single, Type.GetTypeCode (typeof (float)), "#14");
			Assert.AreEqual (TypeCode.String, Type.GetTypeCode (typeof (string)), "#15");
			Assert.AreEqual (TypeCode.UInt16, Type.GetTypeCode (typeof (ushort)), "#16");
			Assert.AreEqual (TypeCode.UInt32, Type.GetTypeCode (typeof (uint)), "#17");
			Assert.AreEqual (TypeCode.UInt64, Type.GetTypeCode (typeof (ulong)), "#18");
		}

		[Test]
		public void GetTypeFromHandle_Handle_Zero ()
		{
			RuntimeTypeHandle handle = new RuntimeTypeHandle ();

			Assert.IsNull (Type.GetTypeFromHandle (handle));
		}

		[Test]
		public void GetTypeHandle_O_Null ()
		{
			try {
				Type.GetTypeHandle (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // GetConstructor (Type [])
		public void GetConstructor1 ()
		{
			Type type;
			ConstructorInfo ctor;

			type = typeof (CtorsA);
			ctor = type.GetConstructor (Type.EmptyTypes);
			Assert.IsNotNull (ctor, "#A1");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#A2");
			Assert.IsFalse (ctor.IsStatic, "#A3");
			Assert.IsTrue (ctor.IsPublic, "#A4");
			Assert.AreEqual (".ctor", ctor.Name, "#A5");

			type = typeof (CtorsB);
			ctor = type.GetConstructor (Type.EmptyTypes);
			Assert.IsNotNull (ctor, "#B1");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#B2");
			Assert.IsFalse (ctor.IsStatic, "#B3");
			Assert.IsTrue (ctor.IsPublic, "#B4");
			Assert.AreEqual (".ctor", ctor.Name, "#B5");

			type = typeof (CtorsC);
			ctor = type.GetConstructor (Type.EmptyTypes);
			Assert.IsNull (ctor, "#C");

			type = typeof (CtorsC);
			ctor = type.GetConstructor (new Type [] { typeof (int) });
			Assert.IsNotNull (ctor, "#D1");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#D2");
			Assert.IsFalse (ctor.IsStatic, "#D3");
			Assert.IsTrue (ctor.IsPublic, "#D4");
			Assert.AreEqual (".ctor", ctor.Name, "#D5");
		}

		[Test] // GetConstructor (Type [])
		public void GetConstructor1_Types_Null ()
		{
			try {
				typeof (BindingFlags).GetConstructor (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("types", ex.ParamName, "#6");
			}
		}

		[Test] // GetConstructor (Type [])
		public void GetConstructor1_Types_ItemNull ()
		{
			Type type = typeof (BindingFlags);
			try {
				type.GetConstructor (new Type[1] { null });
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("types", ex.ParamName, "#A6");
			}

			type = typeof (TakesInt);
			try {
				type.GetConstructor (new Type [1] { null });
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("types", ex.ParamName, "#B6");
			}
		}

		[Test] // GetConstructor (BindingFlags, Binder, Type [], ParameterModifier [])
		public void GetConstructor2_Types_ItemNull ()
		{
			Type type = typeof (BindingFlags);
			try {
				type.GetConstructor (BindingFlags.Default, null,
					new Type[1] { null }, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("types", ex.ParamName, "#6");
			}
		}

		[Test] // GetConstructor (BindingFlags, Binder, CallingConventions, Type [], ParameterModifier [])
		public void GetConstructor3_Types_ItemNull ()
		{
			Type type = typeof (BindingFlags);
			try {
				type.GetConstructor (BindingFlags.Default,
					null, CallingConventions.Any,
					new Type[1] { null }, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("types", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetMethod_Bug77367 ()
		{
			MethodInfo i = typeof (Bug77367).GetMethod ("Run", Type.EmptyTypes);
			Assert.IsNull (i);
		}

#if !MOBILE
		[Test]
		public void EqualsUnderlyingType ()
		{
			AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
			TypeAttributes attribs = TypeAttributes.Public;

			AssemblyName name = new AssemblyName ();
			name.Name = "enumtest";
			AssemblyBuilder assembly = 
				AppDomain.CurrentDomain.DefineDynamicAssembly (
					name, access);

			ModuleBuilder module = assembly.DefineDynamicModule 
				("m", "enumtest.dll");
			EnumBuilder e = module.DefineEnum ("E", attribs, typeof (int));

			Assert.IsTrue (typeof (int).Equals (e));
		}
#endif

		[Test]
		public void Equals_Type_Null ()
		{
			Assert.IsFalse (typeof (int).Equals ((Type) null), "#1");
			Assert.IsFalse (typeof (int).Equals ((object) null), "#2");
		}

		[Test]
		public void GetElementType_Bug63841 ()
		{
			Assert.IsNull (typeof (TheEnum).GetElementType (), "#1");
		}

		[Test]
		public void FullNameGenerics ()
		{
			Type fooType = typeof (Foo<>);
			FieldInfo [] fields = fooType.GetFields ();

			Assert.AreEqual (1, fields.Length, "#0");

			Assert.IsNotNull (fooType.FullName, "#1");
			Assert.IsNotNull (fooType.AssemblyQualifiedName, "#1a");

			FieldInfo field = fooType.GetField ("Whatever");
			Assert.IsNotNull (field, "#2");
			Assert.AreEqual (field, fields [0], "#2a");
			Assert.IsNull (field.FieldType.FullName, "#3");
			Assert.IsNull (field.FieldType.AssemblyQualifiedName, "#3a");
			Assert.IsNotNull (field.FieldType.ToString (), "#4");

			PropertyInfo prop = fooType.GetProperty ("Test");
			Assert.IsNotNull (prop, "#5");
			Assert.IsNull (prop.PropertyType.FullName, "#6");
			Assert.IsNull (prop.PropertyType.AssemblyQualifiedName, "#6a");
			Assert.IsNotNull (prop.PropertyType.ToString (), "#7");

			MethodInfo method = fooType.GetMethod("Execute");
			Assert.IsNotNull (method, "#8");
			Assert.IsNull (method.ReturnType.FullName, "#9");
			Assert.IsNull (method.ReturnType.AssemblyQualifiedName, "#9a");
			Assert.IsNotNull (method.ReturnType.ToString (), "#10");

			ParameterInfo[] parameters = method.GetParameters();
			Assert.AreEqual (1, parameters.Length, "#11");
			Assert.IsNull (parameters[0].ParameterType.FullName, "#12");
			Assert.IsNull (parameters[0].ParameterType.AssemblyQualifiedName, "#12a");
			Assert.IsNotNull (parameters[0].ParameterType.ToString (), "#13");
		}

		[Test]
		public void TypeParameterIsNotGeneric ()
		{
			Type fooType = typeof (Foo<>);
			Type type_param = fooType.GetGenericArguments () [0];
			Assert.IsTrue (type_param.IsGenericParameter);
			Assert.IsFalse (type_param.IsGenericType);
			Assert.IsFalse (type_param.IsGenericTypeDefinition);

			// LAMESPEC: MSDN claims that this should be false, but .NET v2.0.50727 says it's true
			// http://msdn2.microsoft.com/en-us/library/system.type.isgenerictype.aspx
			Assert.IsTrue (type_param.ContainsGenericParameters);
		}

		[Test]
		public void IsAssignable ()
		{
			Type foo_type = typeof (Foo<>);
			Type foo_int_type = typeof (Foo<int>);
			Assert.IsFalse (foo_type.IsAssignableFrom (foo_int_type), "Foo<int> -!-> Foo<>");
			Assert.IsFalse (foo_int_type.IsAssignableFrom (foo_type), "Foo<> -!-> Foo<int>");

			Type ibar_short_type = typeof (IBar<short>);
			Type ibar_int_type = typeof (IBar<int>);
			Type baz_short_type = typeof (Baz<short>);
			Type baz_int_type = typeof (Baz<int>);

			Assert.IsTrue (ibar_int_type.IsAssignableFrom (baz_int_type), "Baz<int> -> IBar<int>");
			Assert.IsTrue (ibar_short_type.IsAssignableFrom (baz_short_type), "Baz<short> -> IBar<short>");

			Assert.IsFalse (ibar_int_type.IsAssignableFrom (baz_short_type), "Baz<short> -!-> IBar<int>");
			Assert.IsFalse (ibar_short_type.IsAssignableFrom (baz_int_type), "Baz<int> -!-> IBar<short>");

			// Nullable tests
			Assert.IsTrue (typeof (Nullable<int>).IsAssignableFrom (typeof (int)));
			Assert.IsFalse (typeof (int).IsAssignableFrom (typeof (Nullable<int>)));
			Assert.IsTrue (typeof (Nullable<FooStruct>).IsAssignableFrom (typeof (FooStruct)));
		}

		[Test]
		public void IsInstanceOf ()
		{
			Assert.IsTrue (typeof (Nullable<int>).IsInstanceOfType (5));
		}

		[Test]
		public void IsInstanceOfArrayOfNullable ()
		{
			Assert.IsTrue (typeof (Nullable<int>[]).IsInstanceOfType (new Nullable<int> [0]));
		}

		[Test]
		public void IsInstanceOfType_Null ()
		{
			Assert.IsFalse (typeof (int).IsInstanceOfType (null), "int");
			Assert.IsFalse (typeof (object).IsInstanceOfType (null), "object");
			Assert.IsFalse (typeof (int?).IsInstanceOfType (null), "int?");
		}

		[Test]
		public void ByrefType ()
		{
			Type foo_type = typeof (Foo<>);
			Type type_param = foo_type.GetGenericArguments () [0];
			Type byref_type_param = type_param.MakeByRefType ();
			Assert.IsFalse (byref_type_param.IsGenericParameter);
			Assert.IsNull (byref_type_param.DeclaringType);
		}

		[Test]
		[ExpectedException (typeof (TypeLoadException))]
		public void MakeByRefByRef ()
		{
			typeof (int).MakeByRefType ().MakeByRefType ();
		}

		[Test]
		public void MakeArrayTypeTest ()
		{
			// This should not crash:
			Type t = typeof (void).MakeArrayType ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProgramException))]
		public void MakeArrayTypedReferenceInstanceTest ()
		{
			object o = Array.CreateInstance (typeof (global::System.TypedReference), 1);
		}

		[Test]
		public void MakeArrayTypeLargeRank ()
		{
			Assert.Throws<TypeLoadException> (delegate () {
					typeof (int).MakeArrayType (33);
				});
		}

		[ComVisible (true)]
		public class ComFoo<T> {
		}

		[Test]
		public void GetCustomAttributesGenericInstance ()
		{
			Assert.AreEqual (1, typeof (ComFoo<int>).GetCustomAttributes (typeof (ComVisibleAttribute), true).Length);
		}

		interface ByRef1<T> { void f (ref T t); }
		interface ByRef2 { void f<T> (ref T t); }

		interface ByRef3<T> where T:struct { void f (ref T? t); }
		interface ByRef4 { void f<T> (ref T? t) where T:struct; }

		void CheckGenericByRef (Type t)
		{
			string name = t.Name;
			t = t.GetMethod ("f").GetParameters () [0].ParameterType;

			Assert.IsFalse (t.IsGenericType, name);
			Assert.IsFalse (t.IsGenericTypeDefinition, name);
			Assert.IsFalse (t.IsGenericParameter, name);
		}

		[Test]
		public void GenericByRef ()
		{
			CheckGenericByRef (typeof (ByRef1<>));
			CheckGenericByRef (typeof (ByRef2));
			CheckGenericByRef (typeof (ByRef3<>));
			CheckGenericByRef (typeof (ByRef4));
		}

		public class Bug80242<T> {
			public interface IFoo { }
			public class Bar : IFoo { }
			public class Baz : Bar { }
		}

		[Test]
		public void TestNestedTypes ()
		{
			Type t = typeof (Bug80242<object>);
			Assert.IsFalse (t.IsGenericTypeDefinition);
			foreach (Type u in t.GetNestedTypes ()) {
				Assert.IsTrue (u.IsGenericTypeDefinition, "{0} isn't a generic definition", u);
				Assert.AreEqual (u, u.GetGenericArguments () [0].DeclaringType);
			}
		}

		[Test] // bug #82211
		public void GetMembers_GenericArgument ()
		{
			Type argType = typeof (ComFoo<>).GetGenericArguments () [0];
			MemberInfo [] members = argType.GetMembers ();
			Assert.IsNotNull (members, "#1");
			Assert.AreEqual (4, members.Length, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReflectionOnlyGetTypeNullTypeName ()
		{
			Type.ReflectionOnlyGetType (null, false, false);
		}

		[Test]
		public void ReflectionOnlyGetTypeDoNotThrow ()
		{
			Assert.IsNull (Type.ReflectionOnlyGetType ("a, nonexistent.dll", false, false));
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ReflectionOnlyGetTypeThrow ()
		{
			Type.ReflectionOnlyGetType ("a, nonexistent.dll", true, false);
		}

		[Test]
		public void ReflectionOnlyGetType ()
		{
			Type t = Type.ReflectionOnlyGetType (typeof (int).AssemblyQualifiedName.ToString (), true, true);
			Assert.AreEqual ("System.Int32", t.FullName);
		}

/*
		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void MakeGenericType_UserDefinedType ()
		{
			Type ut = new UserType (typeof (int));
			Type t = typeof (Foo<>).MakeGenericType (ut);
			Assert.IsTrue (t.IsGenericType, "#A1");
			Assert.AreEqual (1, t.GetGenericArguments ().Length, "#A2");

			Type arg = t.GetGenericArguments () [0];
			Assert.IsNotNull (arg, "#B1");
			Assert.IsFalse (arg.IsGenericType, "#B2");
			Assert.AreEqual (ut, arg, "#B3");
		}

		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void MakeGenericType_NestedUserDefinedType ()
		{
			Type ut = new UserType (new UserType (typeof (int)));
			Type t = typeof (Foo<>).MakeGenericType (ut);
			Assert.IsTrue (t.IsGenericType, "#A1");
			Assert.AreEqual (1, t.GetGenericArguments ().Length, "#A2");

			Type arg = t.GetGenericArguments () [0];
			Assert.IsNotNull (arg, "#B1");
			Assert.IsFalse (arg.IsGenericType, "#B2");
			Assert.AreEqual (ut, arg, "#B3");
		}
		
		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void TestMakeGenericType_UserDefinedType_DotNet20SP1 () 
		{
			Type ut = new UserType(typeof(int));
			Type t = typeof(Foo<>).MakeGenericType(ut);
			Assert.IsTrue (t.IsGenericType, "#1");

			Assert.AreEqual (ut, t.GetGenericArguments()[0], "#2");
		}
		
		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void MakeGenericType_BadUserType ()
		{
			Type ut = new UserType (null);
			Type t = typeof (Foo<>).MakeGenericType (ut);
			var g0 = t.GetGenericArguments () [0];
			Assert.AreSame (g0, ut, "#1");
		}
*/

		[Test]
		public void MakeGenericType_WrongNumOfArguments ()
		{
			try {
				Type t = typeof (Foo<,>).MakeGenericType (new Type [] { typeof (int) });
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[AttributeUsage (AttributeTargets.All)]
		public class DocAttribute : Attribute {
			public DocAttribute (string docs) {}
		}
		
		class GenericClassWithAttributes<[Doc ("T")] T, [Doc ("B")] B> 
			where T : class, new ()
			where B : Attribute
		{
			public T Bar { get{return null;}}

			public void M<[Doc ("X")] X> (X x)
			{
			}
		}
	
		[Test] //bug #377596
		public void GetGenericArguments_ArgumentsHaveAttributes ()
		{
			Type type = typeof(GenericClassWithAttributes<,>);
			Type[] tArgs = type.GetGenericArguments ();
			MethodInfo m = type.GetMethod ("M");
			Type[] mArgs = m.GetGenericArguments ();
			Assert.AreEqual(1, tArgs[0].GetCustomAttributes (typeof (DocAttribute), true).Length, "#1");
			Assert.AreEqual(1, tArgs[1].GetCustomAttributes (typeof (DocAttribute), true).Length, "#1");
			Assert.AreEqual(1, mArgs[0].GetCustomAttributes (typeof (DocAttribute), true).Length, "#1");
		}

		[Test] //bug #471255
		public void GetTypeCalledUsingReflection ()
		{
			Type expectedType = Type.GetType ("NoNamespaceClass");
			Assert.IsNotNull (expectedType, "#1");
			MethodInfo m = typeof (Type).GetMethod ("GetType",  BindingFlags.Public | BindingFlags.Static, null, new Type [] { typeof (string) },  null);
			object r = m.Invoke (null, BindingFlags.Default, null, new object [] { "NoNamespaceClass" }, CultureInfo.InvariantCulture);
			Assert.AreSame (expectedType, r, "#2");
		}

		public class BConstrained<Y> where Y : BConstrained<Y> {
		}

		public class AConstrained<X> : BConstrained<AConstrained<X>> {
		}

		[Test] // Bug https://bugzilla.xamarin.com/show_bug.cgi?id=54485
		public void MakeGenericType_GTD_Constraint ()
		{
			// This is pretty weird, but match .NET behavior (note
			// that typeof(BConstrained<AConstrained<>>) is a
			// compile-time error with roslyn, but it's apparently
			// an ok thing to make with reflection.
			var tb = typeof (BConstrained<>);
			var ta = typeof (AConstrained<>);
			var result = tb.MakeGenericType (ta);
			Assert.IsNotNull (result, "#1");
			// lock down the answer to match what .NET makes
			Assert.IsTrue (result.IsGenericType, "#2");
			Assert.AreEqual (tb, result.GetGenericTypeDefinition (), "#3");
			var bargs = result.GetGenericArguments ();
			Assert.AreEqual (1, bargs.Length, "#4");
			var arg = bargs [0];
			Assert.IsTrue (arg.IsGenericType, "#5");
			// N.B. evidently AConstrained`1 and AConstrained`1<!0> are the same type
			Assert.IsTrue (arg.IsGenericTypeDefinition, "#6");
			Assert.AreEqual (ta, arg.GetGenericTypeDefinition (), "#7");
			var aargs = arg.GetGenericArguments ();
			Assert.AreEqual (1, aargs.Length, "#8");
			Assert.AreEqual (ta.GetGenericArguments () [0], aargs [0], "#9");
		}

	[Test]
	public void EqualsUserType () {
		UserType2 t1 = new UserType2(null);
		UserType2 t2 = new UserType2(t1);
		Assert.IsTrue (t1.Equals(t2));
	}

	[Test]
	public void GetHashCodeUserType () {
		UserType2 t1 = new UserType2(null);
		UserType2 t2 = new UserType2(t1);
		Assert.AreEqual (42, t2.GetHashCode());
	}
	
	[Test]
	public void IsGenericTypeDefinitionUserType () {
		Assert.IsFalse (new UserType(null).IsGenericTypeDefinition);
	}
	
	[Test]
	public void IsGenericTypeUserType () {
		Assert.IsFalse (new UserType(null).IsGenericType);
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void GetGenericTypeDefinitionUserType () {
		new UserType(null).GetGenericTypeDefinition ();
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void GetGenericArgumentsUserType () {
		new UserType(null).GetGenericArguments ();
	}
	
	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void GenericParameterPositionUserType () {
		Assert.IsTrue (new UserType(null).GenericParameterPosition == 0);
	}

		[Test]
		public void TypeGetMemberReturnTypeTest ()
		{
			object obj;
			MemberTypes memtype;
			Type testtype;
			object [] flagsandtypes = new object [] {
				MemberTypes.All, typeof (MemberInfo []),
				MemberTypes.Constructor, typeof (ConstructorInfo []),
				MemberTypes.Custom, typeof (MemberInfo []),
				MemberTypes.Event, typeof (EventInfo []),
				MemberTypes.Field, typeof (FieldInfo []),
				MemberTypes.Method, typeof (MethodInfo []),
				MemberTypes.NestedType, typeof (Type []),
				MemberTypes.Property, typeof (PropertyInfo []),
				MemberTypes.TypeInfo, typeof (Type [])};

			for (int i=0; i < flagsandtypes.Length; i+=2) {
				memtype = (MemberTypes)flagsandtypes [i];
				testtype = (Type)flagsandtypes [i+1];
				obj = GetType ().GetMember ("DummyMember", memtype,
						BindingFlags.Public | BindingFlags.Instance);
				Assert.AreEqual (testtype.GetHashCode (), obj.GetType ().GetHashCode (),
						"Expected #" + i + " " + testtype.FullName);
			}

		}
 
 		[Test]
 		public void TypeNameStartsWithSpace ()
		{
			Type t1 = Type.GetType ("System.Type, mscorlib");
			Type t2 = Type.GetType (" System.Type, mscorlib");
			Assert.AreEqual (t1, t2);
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		public void SpaceAfterComma () {
			string strType = "System.Collections.Generic.Dictionary`2[[System.Int32,mscorlib], [System.String,mscorlib]],mscorlib";
			Assert.IsTrue (Type.GetType (strType) != null);
		}
#endif

#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		public void Bug506757 ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "customMod";
			assemblyName.Version = new Version (1, 2, 3, 4);
	
			AssemblyBuilder assembly 
				= Thread.GetDomain().DefineDynamicAssembly(
					  assemblyName, AssemblyBuilderAccess.RunAndSave);
	
			ModuleBuilder module = assembly.DefineDynamicModule("res.exe", "res.exe");
	
			TypeBuilder type0 = module.DefineType ("Base", TypeAttributes.Public, typeof (object));
			TypeBuilder type1 = module.DefineType ("Middle", TypeAttributes.Public, type0);
			TypeBuilder type2 = module.DefineType ("End", TypeAttributes.Public, type1);
	
			MethodAttributes attrs0 = MethodAttributes.Virtual | MethodAttributes.HideBySig |
						  MethodAttributes.NewSlot | MethodAttributes.FamORAssem;
	
			MethodAttributes attrs1 = MethodAttributes.Virtual | MethodAttributes.HideBySig |
						  MethodAttributes.FamORAssem;
	
			MethodAttributes attrs2 = MethodAttributes.Virtual | MethodAttributes.HideBySig |
						  MethodAttributes.Public;
	
	
			MethodBuilder m0 = type0.DefineMethod ("Tst", attrs0, typeof (void), null);
			m0.GetILGenerator ().Emit (OpCodes.Ret);
	
			MethodBuilder m1 = type1.DefineMethod ("Tst", attrs1, typeof (void), null);
			m1.GetILGenerator ().Emit (OpCodes.Ret);
	
			MethodBuilder m2 = type2.DefineMethod ("Tst", attrs2, typeof (void), null);
			m2.GetILGenerator ().Emit (OpCodes.Ret);
	
	
			type0.CreateType ();
			type1.CreateType ();
			Type t2 = type2.CreateType ();
	
			foreach (var m in t2.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic))
				Assert.IsTrue (m.DeclaringType == typeof (object), String.Format ("{0}::{1}", m.DeclaringType, m.Name));
		}
#endif
		[Test]
		public void MakeArrayTypeOfOneDimension ()
		{
			Type vector = typeof (int).MakeArrayType ();
			Type szarray = typeof (int).MakeArrayType (1);

			Assert.AreNotEqual (vector, szarray, "#1");
			Assert.AreEqual ("Int32[]", vector.Name, "#2");
			Assert.AreEqual ("Int32[*]", szarray.Name, "#3");
		}

		public class DeclaringMethodFoo {
			public void Test<T> (T t) {}
			public void Test2<T> (ref T t) {}
		}

		public class DeclaringMethodBar<T> {
			public void Test2 (ref T t) {}
		}

		[Test]
		public void DeclaringMethodOnlyWorksWithGenericArgs ()
		{
	        MethodInfo testMethod = typeof (DeclaringMethodFoo).GetMethod ("Test");
	        MethodBase otherMethod = testMethod.GetParameters ()[0].ParameterType.DeclaringMethod;

			Assert.AreEqual (testMethod, otherMethod,"#1");

			Assert.IsNull (typeof (DeclaringMethodBar<>).GetGenericArguments ()[0].DeclaringMethod, "#2");

			try {
				var x = typeof (int).DeclaringMethod;
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {}

			try {
				var x = typeof (DeclaringMethodFoo).GetMethod ("Test2").GetParameters () [0].ParameterType.DeclaringMethod;
				Assert.Fail ("#4");
			} catch (InvalidOperationException) {}

			try {
				var x = typeof (DeclaringMethodBar<>).GetMethod ("Test2").GetParameters () [0].ParameterType.DeclaringMethod;
				Assert.Fail ("#5");
			} catch (InvalidOperationException) {}

		}

		[Test]
		public void GetArrayRankThrowsForNonArrayType ()
		{
			Assert.AreEqual (1, typeof (int[]).GetArrayRank (), "#1");
			Assert.AreEqual (2, typeof (int[,]).GetArrayRank (), "#2");
			try {
				typeof (int).GetArrayRank ();
				Assert.Fail ("#3");
			} catch (ArgumentException) {}
		}

		[Test] //Bug #564379
		public void GetMethodsReturnPublicMethodsInInterfaces ()
		{
			Type t = typeof (NonClosingStream);
			MethodInfo[] methods = t.GetMethods (BindingFlags.Public | BindingFlags.Instance);

			Assert.AreEqual (5, methods.Length, "#1");
			int id = 2;

			foreach (var m in methods) {
				if (m.Name.Equals ("ToString"))
					Assert.IsTrue (m.DeclaringType == typeof (NonClosingStream), "#" + id);
				else if (m.Name.Equals ("Dispose") && m.GetParameters ().Length == 0)
					Assert.IsTrue (m.DeclaringType == typeof (Stream), "#" + id);
				else if (m.Name.Equals ("Equals") || m.Name.Equals ("GetHashCode") || m.Name.Equals ("GetType"))
					Assert.IsTrue (m.DeclaringType == typeof (object), "#" + id);
				else
					Assert.Fail ("invalid method " + m);
				++id;
			}
		}

		[Test] // Bug #574696
		public void GetMember_DoesntReturnPrivatePropOfParent ()
		{
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			Assert.AreEqual (1, typeof (Bar).GetMember ("PrivInst", flags).Length);
			Assert.AreEqual (0, typeof (Bar).GetMember ("PrivInstBase", flags).Length);
			Assert.AreEqual (1, typeof (Foo).GetMember ("PrivInstBase", flags).Length);
		}

		[Test] // Bug #484246
		public void GetInterfaceCompareAgainstGTDNames ()
		{
			var t = typeof (Dictionary<string,string>);
			var iface = typeof (IDictionary<string,string>);

			Assert.AreSame (iface, t.GetInterface ("System.Collections.Generic.IDictionary`2"), "#1");

			string name = "System.Collections.Generic.IDictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

			Assert.IsNull (t.GetInterface (name), "#2");
		} 

		[Test]
		public void RuntimeCorrectlyNormalizeGenericTypes ()
		{
			Type lst = typeof (MList<>);
			Type arg = lst.GetGenericArguments ()[0];

			Type sup = lst.BaseType;
			Type sa0 = sup.GetGenericArguments ()[0];
			Type sa1 = sup.GetGenericArguments ()[1];

			Assert.IsTrue (sa1 == lst, "#1");
			Assert.IsTrue (sa0 == arg, "#2");

			Type inst = typeof (Cons<,>).MakeGenericType (arg, lst.MakeGenericType (arg));
			Assert.IsTrue (inst == sup, "#3");
		}

		class Cons<T,U>
		{

		}

		class MList<A> : Cons<A, MList<A>>
		{

		}

		[Test] // Bug #331126
		public void IsAssignableFromWorksCorrectlyWithByRefs ()
		{
			Type int_byref = typeof (int).MakeByRefType ();
			Type obj_byref = typeof (object).MakeByRefType ();
			Type long_byref = typeof (long).MakeByRefType ();
			Type enum1_byref = typeof (AttributeTargets).MakeByRefType ();
			Type enum2_byref = typeof (PlatformID).MakeByRefType ();
			Type uint_byref = typeof (uint).MakeByRefType ();
			Type string_byref = typeof (object).MakeByRefType ();
			Type struct0_byref = typeof (Size4).MakeByRefType ();
			Type struct1_byref = typeof (Size4b).MakeByRefType ();
			Type mvar0_byref = typeof (TypeTest).GetMethod ("Bug331126").GetGenericArguments ()[0].MakeByRefType ();
			Type mvar1_byref = typeof (TypeTest).GetMethod ("Bug331126").GetGenericArguments ()[1].MakeByRefType ();

			Assert.IsFalse (typeof (int).IsAssignableFrom (int_byref), "#1");
			Assert.IsFalse (int_byref.IsAssignableFrom (typeof (int)), "#2");
			Assert.IsFalse (obj_byref.IsAssignableFrom (long_byref), "#3");
			Assert.IsFalse (long_byref.IsAssignableFrom (obj_byref), "#4");
			Assert.IsTrue (enum1_byref.IsAssignableFrom (enum2_byref), "#5");
			Assert.IsTrue (enum2_byref.IsAssignableFrom (enum1_byref), "#6");
			Assert.IsTrue (int_byref.IsAssignableFrom (enum2_byref), "#7");
			Assert.IsTrue (enum2_byref.IsAssignableFrom (int_byref), "#8");
			Assert.IsTrue (enum2_byref.IsAssignableFrom (uint_byref), "#9");
			Assert.IsTrue (uint_byref.IsAssignableFrom (enum2_byref), "#10");
			Assert.IsTrue (int_byref.IsAssignableFrom (uint_byref), "#11");
			Assert.IsTrue (uint_byref.IsAssignableFrom (int_byref), "#12");

			Assert.IsTrue (typeof (object).IsAssignableFrom (typeof (long)), "#13");

			Assert.IsTrue (obj_byref.IsAssignableFrom (string_byref), "#14");
			Assert.IsTrue (string_byref.IsAssignableFrom (obj_byref), "#15");

			Assert.IsFalse (uint_byref.IsAssignableFrom (struct0_byref), "#16");
			Assert.IsFalse (struct0_byref.IsAssignableFrom (int_byref), "#17");
			Assert.IsFalse (struct0_byref.IsAssignableFrom (struct1_byref), "#18");

			Assert.IsFalse (obj_byref.IsAssignableFrom (mvar0_byref), "#19");
			Assert.IsFalse (mvar0_byref.IsAssignableFrom (mvar1_byref), "#20");
			Assert.IsTrue (mvar0_byref.IsAssignableFrom (mvar0_byref), "#21");
			Assert.IsFalse (mvar0_byref.IsAssignableFrom (obj_byref), "#22");
		}

		public void Bug331126<T,K> () {}

		public struct Size4 {
			public int field;
		}

		public struct Size4b {
			public int field;
		}

		[Test]
		public void IsAssignableFromGenericArgumentsWithConstraints ()
		{
			// Regression test for #58809

			// Generic Parameters of a gtd should have their
			// constraints respected even when those constraints
			// are other generic parameters themselves.

			var ps = typeof (GenericWithParamConstraints<,,>).GetGenericArguments ();

			var a = ps[0];
			var b = ps[1];
			var c = ps[2];

			// Foo<C>
			var fooOfC = typeof (Foo<>).MakeGenericType (c);

			// constraint B : Foo <C>
			Assert.IsTrue (fooOfC.IsAssignableFrom (b), "#1");

			// constraint A : B
			Assert.IsTrue (b.IsAssignableFrom (a), "#2");

			// A : Foo<C> since A : B and B : Foo<C>
			Assert.IsTrue (fooOfC.IsAssignableFrom (a), "#3");
		}

		class GenericWithParamConstraints<A, B, C> where B : Foo<C> where A : B
		{
		}

		[Test]
		public void IsAssignableFromArraySpecialInterfaceGtd ()
		{
			// Regression test for https://github.com/mono/mono/issues/7095
			// An "array special interface" is a Mono name for some
			// interfaces that are implemented by arrays.
			// Check that an array special interface GTD (ie, IList<> not IList<Foo>) work
			// correctly with IsAssignableFrom.
			var il = typeof (IList<>);
			var ie = typeof (IEnumerable<>);
			var ilparam = il.GetTypeInfo ().GenericTypeParameters [0];
			var ilparr = ilparam.MakeArrayType ();

			Assert.IsTrue (ie.IsAssignableFrom (ie), "IList<> ---> IEnumerable<>");
			Assert.IsTrue (il.IsAssignableFrom (ilparr), "!0[] ---> IList<>");

			var ilparrarr = ilparr.MakeArrayType ();

			Assert.IsFalse (il.IsAssignableFrom (ilparrarr), "!0[][] -!-> IList<>");

			Assert.IsFalse (il.IsAssignableFrom (typeof (Array)), "System.Array -!-> IList<>");
		}

		[Test]
		public void IsAssignableFromArrayEnumerator ()
		{
			// Regression test for https://github.com/mono/mono/issues/7093
			// An array does not implement IEnumerator`1

			var arrStr = typeof (string[]);
			var ieStr = typeof (IEnumerator<string>);
			var ieEqStr = typeof (IEnumerator<IEquatable<string>>);
			Assert.IsFalse (ieStr.IsAssignableFrom (arrStr), "string[] -!-> IEnumerator<string>");
			Assert.IsFalse (ieEqStr.IsAssignableFrom (arrStr), "string[] -!-> IEnumerator<IEquatable<string>>");

			var arrInt = typeof (int[]);
			var ieInt = typeof (IEnumerator<int>);
			var ieEqInt = typeof (IEnumerator<IEquatable<int>>);
			Assert.IsFalse (ieInt.IsAssignableFrom (arrInt), "int[] -!-> IEnumerator<int>");
			Assert.IsFalse (ieEqInt.IsAssignableFrom (arrInt), "int[] -!-> IEnumerator<IEquatable<int>>");
		}

		[Test] // Bug #612780
		public void CannotMakeDerivedTypesFromTypedByRef ()
		{
		try {
	        typeof (global::System.TypedReference).MakeArrayType ();
	        Assert.Fail ("#1");
		} catch (TypeLoadException) { }

		try {
	        typeof (global::System.TypedReference).MakeByRefType ();
	        Assert.Fail ("#2");
		} catch (TypeLoadException) { }

		try {
	        typeof (global::System.TypedReference).MakePointerType ();
	        Assert.Fail ("#3");
		} catch (TypeLoadException) { }

		}
		
		[Test] //Bug643890
		public void DeclaringTypeOfGenericNestedTypeInstanceIsOpen ()
		{
			var type = typeof (Foo<int>.Nested<string>);
			Assert.AreSame (typeof (Foo<>), type.DeclaringType, "#1");
		}

		interface IGetInterfaceMap<in T>
		{
		    string Bar (T t);
		}

		class GetInterfaceMap : IGetInterfaceMap<object>
		{
		    public string Bar (object t)
		    {
		        return t.GetType ().FullName;
		    }
		}

		[Test]
		public void GetInterfaceMapWorksWithVariantIfaces ()
		{
			InterfaceMapping res = typeof (GetInterfaceMap).GetInterfaceMap (typeof (IGetInterfaceMap <object>));
			Assert.AreEqual (typeof (IGetInterfaceMap <object>), res.InterfaceType);
			Assert.AreEqual (typeof (object), res.InterfaceMethods [0].GetParameters () [0].ParameterType);

			res = typeof (GetInterfaceMap).GetInterfaceMap (typeof (IGetInterfaceMap <string>));
			Assert.AreEqual (typeof (IGetInterfaceMap <string>), res.InterfaceType);
			Assert.AreEqual (typeof (string), res.InterfaceMethods [0].GetParameters () [0].ParameterType);
		}


		public class MyType : TypeDelegator {
			public int eq, ust;

			public override bool Equals (Type t) {
				++eq;
				return base.Equals (t);
			}

			public override Type UnderlyingSystemType  {
				get { 
					++ust;
					return typeof (int);
				}
			}
		}

		[Test]
		public void NewV4EqualsBehavior ()
		{
			var ta = new MyType ();
			var tb = new MyType ();
			object a = ta, b = tb;

			a.Equals (a);
			Assert.AreEqual (1, ta.eq, "#1");
			Assert.AreEqual (2, ta.ust, "#2");
			a.Equals (b);
			Assert.AreEqual (2, ta.eq, "#3");
			Assert.AreEqual (3, ta.ust, "#4");
			Assert.AreEqual (0, tb.eq, "#5");
			Assert.AreEqual (1, tb.ust, "#6");
		}

		public enum MyRealEnum : short {
			A,B,C
		}


		public enum MyRealEnum2 : byte {
			A,B,C
		}

		public enum MyRealEnum3 : short {
			A,B,C
		}

		public class MyEnum : TypeDelegator {
			public bool is_enum { get; set; }
			public int fields { get; set; }

			public override bool IsSubclassOf (Type c) {
				return c == typeof (Enum) && is_enum;
			}

			public override FieldInfo[] GetFields (BindingFlags bindingAttr) {
				if (fields == 0)
					return null;
				FieldInfo[] res = new FieldInfo [fields];
				for (int i = 0; i < fields; ++i) {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						res [i] = typeof (MyRealEnum).GetField ("value__");
					else
						res [i] = typeof (MyRealEnum).GetField ("A");
				}
				return res;
			}
		}

		[Test]
		public void GetEnumUnderlyingType () {

			try {
				new MyEnum () { is_enum = false }.GetEnumUnderlyingType ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {}

			try {
				new MyEnum () { is_enum = true, fields = 0 }.GetEnumUnderlyingType ();
				Assert.Fail ("#2");
			} catch (ArgumentException) {}

			try {
				new MyEnum () { is_enum = true, fields = 2 }.GetEnumUnderlyingType ();
				Assert.Fail ("#3");
			} catch (ArgumentException) {}

			Assert.AreSame (typeof (short), new MyEnum () { is_enum = true, fields = 1 }.GetEnumUnderlyingType ());
		}

		[Test]
		public void GetEnumNames () {
			try {
				new MyEnum () { is_enum = false }.GetEnumNames ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {}

			var res = new MyEnum () { is_enum = true, fields = 1 }.GetEnumNames ();
			Assert.AreEqual (1, res.Length, "#2");
			Assert.AreEqual ("A", res [0], "#3");

			res = typeof (MyRealEnum).GetEnumNames ();
			Assert.AreEqual (3, res.Length, "#4");
			Assert.AreEqual ("A", res [0], "#5");
			Assert.AreEqual ("B", res [1], "#6");
			Assert.AreEqual ("C", res [2], "#7");
		}

		public enum OutOfOrderEnum : sbyte
		{
			D = -1, C = 2, B = 1, A = 0
		}
				
		[Test]
		public void GetEnumNamesSortsByUnsignedValue ()
		{
			string[] names = typeof (OutOfOrderEnum).GetEnumNames ();
			Assert.AreEqual (4, names.Length);
			Assert.AreEqual ("A", names [0]);
			Assert.AreEqual ("B", names [1]);
			Assert.AreEqual ("C", names [2]);
			Assert.AreEqual ("D", names [3]);
		}
		
		[Test]
		public void GetEnumValues () {
			try {
				new MyEnum () { is_enum = false }.GetEnumValues ();
				Assert.Fail ("#1");
			} catch (ArgumentException) {}

			try {
				new MyEnum () { is_enum = true }.GetEnumValues ();
				Assert.Fail ("#2");
			} catch (NotImplementedException) {}

			var array = typeof (MyRealEnum).GetEnumValues ();
			Assert.AreEqual (typeof (MyRealEnum[]), array.GetType (), "#3");
			MyRealEnum[] res = (MyRealEnum[])array;

			Assert.AreEqual (3, res.Length, "#4");
			Assert.AreEqual (MyRealEnum.A, res [0], "#5");
			Assert.AreEqual (MyRealEnum.B, res [1], "#6");
			Assert.AreEqual (MyRealEnum.C, res [2], "#7");
		}

		[Test]
		public void GetEnumValue () {
			try {
				typeof (MyRealEnum).GetEnumName (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) { }

			try {
				new MyEnum () { is_enum = false }.GetEnumName (99);
				Assert.Fail ("#2");
			} catch (ArgumentException) { }


			Assert.IsNull (new MyEnum () { fields = 1, is_enum = true }.GetEnumName (77), "#3");
			Assert.AreEqual ("A", new MyEnum () { fields = 1, is_enum = true }.GetEnumName (0), "#4");
			Assert.AreEqual ("A", new MyEnum () { fields = 1, is_enum = true }.GetEnumName (MyRealEnum.A), "#5");
			Assert.AreEqual ("A", new MyEnum () { fields = 1, is_enum = true }.GetEnumName (MyRealEnum2.A), "#6");

			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName (MyRealEnum.A), "#7");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((short)0), "#8");
			Assert.AreEqual ("C", typeof (MyRealEnum).GetEnumName (2), "#9");
			Assert.IsNull (typeof (MyRealEnum).GetEnumName (9), "#10");

			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((byte)0), "#11");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((sbyte)0), "#12");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName (false), "#13");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((short)0), "#14");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((ushort)0), "#15");
			Assert.IsNull (typeof (MyRealEnum).GetEnumName ('c'), "#16");

			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((int)0), "#17");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((uint)0), "#18");

			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((long)0), "#19");
			Assert.AreEqual ("A", typeof (MyRealEnum).GetEnumName ((ulong)0), "#20");

			try {
				typeof (MyRealEnum).GetEnumName ((float)0);
				Assert.Fail ("#21");
			} catch (ArgumentException) { }
			try {
				typeof (MyRealEnum).GetEnumName ((double)0);
				Assert.Fail ("#22");
			} catch (ArgumentException) { }


			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((byte)0), "#23");
			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((sbyte)0), "#24");
			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName (false), "#25");

			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((short)0), "#26");
			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((ushort)0), "#27");

			Assert.IsNull (typeof (MyRealEnum2).GetEnumName ('c'), "#28");

			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((int)0), "#29");
			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((uint)0), "#30");

			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((long)0), "#31");
			Assert.AreEqual ("A", typeof (MyRealEnum2).GetEnumName ((ulong)0), "#32");

			try {
				typeof (MyRealEnum2).GetEnumName ((float)0);
				Assert.Fail ("#33");
			} catch (ArgumentException) { }
			try {
				typeof (MyRealEnum2).GetEnumName ((double)0);
				Assert.Fail ("#34");
			} catch (ArgumentException) { }

			Assert.IsNull (typeof (MyRealEnum2).GetEnumName (12345), "#35");
		}

		[Test]
		public void IsEnumDefined () {
			try {
				typeof (MyRealEnum).IsEnumDefined (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) { }

			try {
				new MyEnum () { is_enum = false }.IsEnumDefined (99);
				Assert.Fail ("#2");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined (0);
				Assert.Fail ("#3");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined ((ushort)0);
				Assert.Fail ("#4");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined (MyRealEnum3.A);
				Assert.Fail ("#5");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined (true);
				Assert.Fail ("#6");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined (MyRealEnum2.A);
				Assert.Fail ("#7");
			} catch (ArgumentException) { }

			try {
				typeof (MyRealEnum).IsEnumDefined (typeof (MyRealEnum));
				Assert.Fail ("#8");
			} catch (InvalidOperationException) { }

			Assert.IsTrue (typeof (MyRealEnum).IsEnumDefined ((short)0), "#9");
			Assert.IsFalse (typeof (MyRealEnum).IsEnumDefined ((short)88), "#10");
			Assert.IsTrue (typeof (MyRealEnum).IsEnumDefined (MyRealEnum.A), "#11");
			Assert.IsFalse (typeof (MyRealEnum).IsEnumDefined ("d"), "#12");
			Assert.IsTrue  (typeof (MyRealEnum).IsEnumDefined ("A"), "#13");
			Assert.IsFalse  (new MyEnum () { is_enum = true, fields = 1 }.IsEnumDefined ((short)99), "#14");
		}



		public class Outer {
			public class Inner {}
		}


		public class Outer<T> {
			public class Inner {}
		}

		[Test]
		public void GetTypeWithDelegates () {
			var tname = typeof (MyRealEnum).AssemblyQualifiedName;
			var res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum), res, "#1");


			tname = typeof (Dictionary<int, string>).AssemblyQualifiedName;
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Dictionary<int, string>), res, "#2");


			tname = typeof (Foo<int>).AssemblyQualifiedName;
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Foo<int>), res, "#3");


			tname = typeof (Outer.Inner).AssemblyQualifiedName;
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Outer.Inner), res, "#4");


			tname = typeof (Outer<double>.Inner).AssemblyQualifiedName;
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Outer<double>.Inner), res, "#5");


			tname = "System.Collections.Generic.List`1[System.Int32]";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (List<int>), res, "#6");


			tname = typeof (Foo<>).FullName + "[,][]";
			res = Type.GetType (tname, name => {
					Console.WriteLine ("resolve-asm name {0}", name);
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Foo<>).MakeArrayType (2).MakeArrayType (), res, "#7");

			tname = string.Format("{0}[{1}][]*&", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName);
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (Foo<MyRealEnum>[]).MakePointerType ().MakeByRefType (), res, "#8");


			tname = typeof (MyRealEnum).FullName + "[][]";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum[][]), res, "#9");


			tname = typeof (MyRealEnum).FullName + "[*]";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakeArrayType (1), res, "#10");


			tname = typeof (MyRealEnum).FullName + "&";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakeByRefType (), res, "#11");


			tname = typeof (MyRealEnum).FullName + "*";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakePointerType (), res, "#12");

			tname = typeof (MyRealEnum).FullName + "*&";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakePointerType ().MakeByRefType(),
					 res, "#13");

			tname = typeof (MyRealEnum).FullName + "[,]&";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakeArrayType (2).MakeByRefType (),
					 res, "#14");

			tname = typeof (MyRealEnum).FullName + "*[]";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakePointerType().MakeArrayType(),
					 res, "#15");

			// not a very useful type, but ought to be parsed correctly
			tname = typeof (MyRealEnum).FullName + "[]**[]*&";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual (typeof (MyRealEnum).MakeArrayType().MakePointerType().MakePointerType().MakeArrayType().MakePointerType().MakeByRefType(),
					 res, "#16");

			// assembly resolve without type resolve
			res = Type.GetType ("System.String,mscorlib", delegate (AssemblyName aname) { return typeof (int).Assembly; }, null);
			Assert.AreEqual (typeof (string), res);
		}


		public class CustomGetType : TypeDelegator {
			string name;

			public CustomGetType (string name) { this.name = name; }

			public override Type MakeGenericType (Type[] args) {
				return new CustomGetType ("GINST");
			}

		 	public override Type GetNestedType(String name, BindingFlags bidingAttr) {
				return new CustomGetType ("NESTED");
			}

			public override string ToString () { return "UT_" + name; }

			public override string Name {
				get { return  "UT_" + name; }
			}
		}

		[Test]
		public void GetTypeWithDelegatesAndUserTypes ()
		{
			var tname = "Magic[System.Int32]";
			var res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					if (name == "Magic") return new CustomGetType ("MAGIC");
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual ("UT_GINST", res.Name, "#1");


			tname = "Magic+MyRealEnum";
			res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					if (name == "Magic") return new CustomGetType ("MAGIC");
					return asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, false, false);
			Assert.AreEqual ("UT_NESTED", res.Name, "#2");
		}

		void MustTLE (string tname) {
			try {
				var res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return (object)asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, true, false);
				Assert.Fail (tname);
			} catch (TypeLoadException) {}
		}

		void MustANE (string tname) {
			try {
				var res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return (object)asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, true, false);
				Assert.Fail (tname);
			} catch (ArgumentNullException) {}
		}

		void MustAE_general (string tname, Func<string,Type> getType) {
			try {
				var res = getType (tname);
				Assert.Fail (tname);
			} catch (ArgumentException) {}
		}

		void MustAE (string typename) {
			MustAE_general (typename, tname => {
					return Type.GetType (tname, name => {
							return Assembly.Load (name);
						},(asm,name,ignore) => {
							return (object)asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
						}, true, false);
				});
		}

		void MustAEnn (string typename) {
			MustAE_general (typename, tname => Type.GetType (tname, null, null));
		}

		void MustFNFE (string tname) {
			try {
				var res = Type.GetType (tname, name => {
					return Assembly.Load (name);
				},(asm,name,ignore) => {
					return (object)asm == null ? Type.GetType (name, false, ignore) : asm.GetType (name, false, ignore);
				}, true, false);
				Assert.Fail (tname);
			} catch (FileNotFoundException) {}
		}

		[Test]
		public void NewGetTypeErrors () {
			MustANE (null);
			MustTLE ("");
			MustAE ("!@#$%^&*");
			MustAE (string.Format ("{0}[{1}&]", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[{1}*]", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}&&", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}&*", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}&[{1}]", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[,", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[*", typeof (MyRealEnum).FullName));

			MustAE (string.Format ("{0}[[{1},", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[[{1}]", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[[{1}],", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[[{1}]_", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));

			MustAE (string.Format ("{0}[{1}", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[{1},", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[{1},,", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[{1} (", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[", typeof (Foo<>).FullName));

			MustAE (string.Format ("{0}[**]", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[*,*]", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[*,]", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[,*]", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[,-]", typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[,{0}]", typeof (MyRealEnum).FullName));

			MustAE (string.Format ("{0}[{1}]]", typeof (Foo<>).FullName, typeof (MyRealEnum).FullName));
			MustAE (string.Format ("{0}[,]]", typeof (MyRealEnum).FullName));


			string aqn = typeof (MyRealEnum).Assembly.FullName;
			MustFNFE (string.Format ("{0}, ZZZ{1}", typeof (MyRealEnum).FullName, aqn));
			MustTLE (string.Format ("{0}ZZZZ", typeof (MyRealEnum).FullName));
			MustTLE (string.Format ("{0}ZZZZ,{1}", typeof (MyRealEnum).FullName, aqn));
		}

		[Test]
		public void GetTypeExceptionMsg () {
			string typeName = "system.int32, foo";
			try {
				Type.GetType(typeName, true, false);
			} catch (TypeLoadException ex) {
				Assert.IsTrue (ex.Message.Contains ("system.int32"));
				Assert.IsTrue (ex.Message.Contains ("foo"));
			}
		}

	   	delegate void MyAction<in T>(T ag);

		[Test] //bug #668506
		public void IsAssignableFromWithVariantDelegate () {
			Assert.IsFalse (typeof(MyAction<string>).IsAssignableFrom(typeof(MyAction<>)), "#1");
		}

		[Test] //bug #124
		public void IsAssignableFromWithNullable () {
            Console.WriteLine(typeof(IEnumerable<int?>).IsAssignableFrom(typeof(IEnumerable<int>)));
		}

		[Test]
		public void GetTypeParseGenericCorrectly () { //Bug #15124
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1"), typeof (Foo<>), "#1");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32]"), typeof (Foo<int>), "#2");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[[System.Int32]]"), typeof (Foo<int>), "#3");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32][]"), typeof (Foo<int>[]), "#4");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[][System.Int32]"), null, "#5");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32][,]"), typeof (Foo<int>[,]), "#6");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[]"), typeof (Foo<>).MakeArrayType(), "#7");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[,]"), typeof (Foo<>).MakeArrayType (2), "#8");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[][]"), typeof (Foo<>).MakeArrayType ().MakeArrayType (), "#9");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1["), null, "#10");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[["), null, "#11");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[[]"), null, "#12");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[,"), null, "#13");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[*"), null, "#14");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32"), null, "#15");
		}

		[Test]
		public void GetTypeNullDelegatesParseGenericCorrectly () {
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1", null, null), typeof (Foo<>), "#1");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32]", null, null), typeof (Foo<int>), "#2");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[[System.Int32]]", null, null), typeof (Foo<int>), "#3");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32][]", null, null), typeof (Foo<int>[]), "#4");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[System.Int32][,]", null, null), typeof (Foo<int>[,]), "#5");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[]", null, null), typeof (Foo<>).MakeArrayType(), "#6");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[,]", null, null), typeof (Foo<>).MakeArrayType (2), "#7");
			Assert.AreEqual (Type.GetType ("MonoTests.System.Foo`1[][]", null, null), typeof (Foo<>).MakeArrayType ().MakeArrayType (), "#8");

			MustAEnn ("MonoTests.System.Foo`1[][System.Int32]");
			MustAEnn ("MonoTests.System.Foo`1[");
			MustAEnn ("MonoTests.System.Foo`1[[");
			MustAEnn ("MonoTests.System.Foo`1[[]");
			MustAEnn ("MonoTests.System.Foo`1[,");
			MustAEnn ("MonoTests.System.Foo`1[*");
			MustAEnn ("MonoTests.System.Foo`1[System.Int32");
		}

		Dictionary<int, T> MakeDictHelper<T> (T[] arr) {
			return new Dictionary<int, T>();
		}

		[Test]
		public void GetTypeAnonymousParseCorrectly () {
			var x = new { X = 1 };
			var a = new [] { x };
			var d = MakeDictHelper (a);

			var x_type = x.GetType ();
			var a_type = a.GetType ();
			var d_type = d.GetType ();

			Assert.AreEqual (Type.GetType (x_type.ToString ()), x_type, "#1");
			Assert.AreEqual (Type.GetType (x_type.ToString (), null, null), x_type, "#2");
			Assert.AreEqual (Type.GetType (a_type.ToString ()), a_type, "#3");
			Assert.AreEqual (Type.GetType (a_type.ToString (), null, null), a_type, "#4");
			Assert.AreEqual (Type.GetType (d_type.ToString ()), d_type, "#5");
			Assert.AreEqual (Type.GetType (d_type.ToString (), null, null), d_type, "#6");

			Assert.AreEqual (Type.GetType (x_type.FullName), x_type, "#7");
			Assert.AreEqual (Type.GetType (x_type.FullName, null, null), x_type, "#8");
			Assert.AreEqual (Type.GetType (a_type.FullName), a_type, "#9");
			Assert.AreEqual (Type.GetType (a_type.FullName, null, null), a_type, "#10");
			Assert.AreEqual (Type.GetType (d_type.FullName), d_type, "#11");
			Assert.AreEqual (Type.GetType (d_type.FullName, null, null), d_type, "#12");

		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME && !MONOMAC
		[Test]
		[Category ("AndroidNotWorking")] // requires symbol writer
		public void FullNameGetTypeParseEscapeRoundtrip () // bug #26384
		{
			var nm = new AssemblyName ("asm");
			var ab = AssemblyBuilder.DefineDynamicAssembly (nm,
									AssemblyBuilderAccess.Run);
			var mb = ab.DefineDynamicModule("m", false);
			var tb = mb.DefineType ("NameSpace,+*&[]\\.Type,+*&[]\\",
						TypeAttributes.Class | TypeAttributes.Public);

			var nestedTb = tb.DefineNestedType("Nested,+*&[]\\",
							  TypeAttributes.Class | TypeAttributes.NestedPublic);

			var ty = tb.CreateType();

			var nestedTy = nestedTb.CreateType();

			var escapedNestedName =
				"NameSpace\\,\\+\\*\\&\\[\\]\\\\"
				+ "."
				+ "Type\\,\\+\\*\\&\\[\\]\\\\"
				+ "+"
				+ "Nested\\,\\+\\*\\&\\[\\]\\\\";

			Assert.AreEqual(escapedNestedName, nestedTy.FullName);

			var lookupNestedTy =
				Type.GetType(escapedNestedName + "," + nm.FullName,
					     asmName => {
						     if (asmName.FullName.Equals(nm.FullName)) return ab;
						     else return Assembly.Load (asmName);
					     },
					     (asm,name,ignore) => {
						     if (asm == null)
							     return Type.GetType(name, true, ignore);
						     else return asm.GetType(name, true, ignore);
					     },
					     true,
					     false);
			Assert.AreEqual(nestedTy, lookupNestedTy);

		}
#endif


		[Test]
		public void GetTypeBadArity()
		{
			// Regression test for #46250
			try {
				Type.GetType ("System.Collections.Generic.Dictionary`2[System.String]", true);
				Assert.Fail ("Did not throw an exception (#1)");
			} catch (ArgumentException) {
			}

			try {
				Type.GetType ("System.Collections.Generic.Dictionary`2[System.String,System.Int32,System.Int64]", true);
				Assert.Fail ("Did not throw an exception (#2)");
			} catch (ArgumentException) {
			}
		}

		public abstract class Stream : IDisposable
		{
			public void Dispose ()
			{
				Console.WriteLine ("stream::dispose");
			}

			protected virtual void Dispose (bool disposing)
			{
			}
		}

		public class NonClosingStream 
			: Stream, IDisposable
		{
			void  IDisposable.Dispose()
			{
				Console.WriteLine ("ncs::dispose");
			}

			public override string ToString () { return ""; }
		}

		static bool ContainsProperty (PropertyInfo [] props, string name)
		{
			foreach (PropertyInfo p in props)
				if (p.Name == name)
					return true;
			return false;
		}

		public class NemerleAttribute : Attribute
		{
		}

		public class VolatileModifier : NemerleAttribute
		{
		}

		[VolatileModifier]
		[FooAttribute]
		class A
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=false)]
		public class FooAttribute : Attribute
		{
		}

		public class BarAttribute : FooAttribute
		{
		}

		[BarAttribute]
		class BA : A
		{
		}

		class BBA : BA
		{
		}

		class CA : A
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=true)]
		public class InheritAttribute : Attribute
		{
		}

		[AttributeUsage (AttributeTargets.Class, Inherited=false)]
		public class NotInheritAttribute : InheritAttribute
		{
		}

		[NotInheritAttribute]
		public class bug82431A1
		{
		}

		public class bug82431A2 : bug82431A1
		{
		}

		[NotInheritAttribute]
		[InheritAttribute]
		public class bug82431A3 : bug82431A1
		{
		}

		[InheritAttribute]
		public class bug82431B1
		{
		}

		public class bug82431B2 : bug82431B1
		{
		}

		[NotInheritAttribute]
		public class bug82431B3 : bug82431B2
		{
		}

		public class bug82431B4 : bug82431B3
		{
		}

		struct FooStruct
		{
		}

		public class Bug77367
		{
			public void Run (bool b)
			{
			}
		}

		public class Blue
		{
			private string PrivInstBlue
			{
				get { return null; }
			}

			protected string ProtInstBlue
			{
				get { return null; }
			}

			protected internal string ProIntInstBlue
			{
				get { return null; }
			}

			public long PubInstBlue
			{
				get
				{
					if (PrivInstBlue == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal int IntInstBlue
			{
				get { return 0; }
			}

			private static string PrivStatBlue
			{
				get { return null; }
			}

			protected static string ProtStatBlue
			{
				get { return null; }
			}

			protected static internal string ProIntStatBlue
			{
				get { return null; }
			}

			public static long PubStatBlue
			{
				get
				{
					if (PrivStatBlue == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal static int IntStatBlue
			{
				get { return 0; }
			}
		}

		public class Foo : Blue
		{
			private string PrivInstBase
			{
				get { return null; }
			}

			protected string ProtInstBase
			{
				get { return null; }
			}

			protected internal string ProIntInstBase
			{
				get { return null; }
			}

			public long PubInstBase
			{
				get
				{
					if (PrivInstBase == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal int IntInstBase
			{
				get { return 0; }
			}

			private static string PrivStatBase
			{
				get { return null; }
			}

			protected static string ProtStatBase
			{
				get { return null; }
			}

			protected static internal string ProIntStatBase
			{
				get { return null; }
			}

			public static long PubStatBase
			{
				get
				{
					if (PrivStatBase == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal static int IntStatBase
			{
				get { return 0; }
			}
		}

		public class Bar : Foo
		{
			private string PrivInst
			{
				get { return null; }
			}

			protected string ProtInst
			{
				get { return null; }
			}

			protected internal string ProIntInst
			{
				get { return null; }
			}

			public long PubInst
			{
				get
				{
					if (PrivInst == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal int IntInst
			{
				get { return 0; }
			}

			private static string PrivStat
			{
				get { return null; }
			}

			protected static string ProtStat
			{
				get { return null; }
			}

			protected static internal string ProIntStat
			{
				get { return null; }
			}

			public static long PubStat
			{
				get
				{
					if (PrivStat == null)
						return 0;
					return long.MaxValue;
				}
			}

			internal static int IntStat
			{
				get { return 0; }
			}
		}

		class CtorsA
		{
			static CtorsA ()
			{
			}
		}

		class CtorsB
		{
			public CtorsB ()
			{
			}
		}

		class CtorsC
		{
			static CtorsC ()
			{
			}

			public CtorsC (int x)
			{
			}
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=57938
		[Test]
		public void NullFullNameForSpecificGenericTypes()
		{
			var expected = new [] {
				(
					typeof(Bug59738Class<>).GetFields()[0].FieldType,
					"Bug59738Interface`1", (string)null, 
					"MonoTests.System.TypeTest+Bug59738Interface`1[U]"
				),
				(
					typeof(Bug59738Derived<>).BaseType,
					"Bug59738Class`1", (string)null, 
					"MonoTests.System.TypeTest+Bug59738Class`1[U]"
				),
				(
					typeof(Bug59738Class<int>),
					"Bug59738Class`1", 
					$"MonoTests.System.TypeTest+Bug59738Class`1[[System.Int32, {typeof (int).Assembly.FullName}]]",
					"MonoTests.System.TypeTest+Bug59738Class`1[System.Int32]"
				)
			};

			for (var i = 0; i < expected.Length; i++) {
				var (t, name, fullname, tostring) = expected[i];
				Assert.AreEqual(name, t.Name, $"{i}.Name");
				Assert.AreEqual(fullname, t.FullName, $"{i}.FullName");
				Assert.AreEqual(tostring, t.ToString(), $"{i}.ToString()");
			}
		}

		// https://github.com/mono/mono/issues/6579
		[Test]
		public void GetInterfaceCaseInsensitiveTest()
		{
			var type = typeof(Dictionary<string, object>);

			Assert.NotNull (
				type.GetInterface ("System.Collections.IDictionary", false),
				"strict named interface must be found (ignoreCase = false)"
			);
			Assert.NotNull (
				type.GetInterface ("System.Collections.IDictionary", true),
				"strict named interface must be found (ignoreCase = true)"
			);
			Assert.Null (
				type.GetInterface ("System.Collections.Idictionary", false),
				"interface, named in mixed case, must not be found (ignoreCase = false)"
			);
			Assert.NotNull (
				type.GetInterface ("System.Collections.Idictionary", true),
				"interface, named in mixed case, must be found (ignoreCase = true)"
			);
		}

		interface Bug59738Interface<T> {
		}

		class Bug59738Class<U> {
			public Bug59738Interface<U> Iface;
		}

		class Bug59738Derived<U> : Bug59738Class<U> {
		}
	}

	class UserType : Type
	{
		protected Type type;
	
		public UserType(Type type) {
			this.type = type;
		}
	
		public override Type UnderlyingSystemType { get { return this.type; } }
	
		public override Assembly Assembly { get { return this.type == null ? null : this.type.Assembly; } }
	
		public override string AssemblyQualifiedName { get { return null; } }
	
		public override Type BaseType { get { return null; } }
	
		public override Module Module { get { return this.type.Module; } }
	
		public override string Namespace { get { return null; } }
	
		public override bool IsGenericParameter { get { return true; } }
	 
		public override RuntimeTypeHandle TypeHandle { get { throw new NotSupportedException(); } }
	
		public override bool ContainsGenericParameters { get { return true; } }
	
		public override string FullName { get { return this.type.Name; } }
	
		public override Guid GUID { get { throw new NotSupportedException(); } }
	
	
		protected override bool IsArrayImpl() {
			return false;
		}
	
		protected override bool IsByRefImpl()
		{
			return false;
		}
	
		protected override bool IsCOMObjectImpl()
		{
			return false;
		}
	
		protected override bool IsPointerImpl()
		{
			return false;
		}
	
		protected override bool IsPrimitiveImpl()
		{
			return false;
		}
	
	
		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			return 0;
		}
	
		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder,
									   CallingConventions callConvention, Type[] types,
									   ParameterModifier[] modifiers)
		{
			return null;
		}
	
		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			return null;
		}
	
		public override Type GetElementType()
		{
			return null;
		}
	
		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			return null;
		}
	
	
		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			return null;
		}
	
	
		public override Type GetInterface(string name, bool ignoreCase)
		{
			return null;
		}
	
		public override Type[] GetInterfaces()
		{
			return null;
		}
	
		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			return null;
		}
	
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return null;
		}
	
		public override object[] GetCustomAttributes(bool inherit)
		{
			return null;
		}
	
		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return false;
		}
	
		public override string Name { get { return this.type.Name; } }
	
		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}
	
		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}
	
		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder,
								 CallingConventions callConvention, Type[] types,
								 ParameterModifier[] modifiers)
		{
			return null;
		}
	
		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			return null;
		}
	
		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			return null;
		}
	
		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			return null;
		}
	
		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			return null;
		}
	
		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
								 Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return null;
		}
	
		protected override bool HasElementTypeImpl()
		{
			return false;
		}
	
		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
							 object[] args, ParameterModifier[] modifiers, CultureInfo culture,
							 string[] namedParameters)
		{
			throw new NotSupportedException();
		}
	}

    class UserType2 : UserType {
		public UserType2 (Type type) : base (type) {
		}

		public override Type UnderlyingSystemType { get { return this.type ?? this; } }

		public override int GetHashCode()
		{
			if (type == null)
				return 42;
			return type.GetHashCode();
		}
	}
}
