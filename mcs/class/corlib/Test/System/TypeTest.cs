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

	interface IFace1 {
		void foo ();
	}

	interface IFace2 : IFace1 {
		void bar ();
	}

	interface IFace3 : IFace2 {
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

		public NewVTable (out int i) {
			i = 0;
		}

		public void byref_method (out int i) {
			i = 0;
		}

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

#if NET_2_0
	public class Foo<T> {
		public T Whatever;
	
		public T Test {
			get { throw new NotImplementedException (); }
		}

		public T Execute(T a) {
			return a;
		}
	}

	public interface IBar<T> { }
	public class Baz<T> : IBar<T> { }
#endif

	[TestFixture]
	public class TypeTest
	{
		private void ByrefMethod (ref int i, ref Derived1 j, ref Base1 k) {
		}

		[Test]
		public void TestIsAssignableFrom () {
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
		}

		[Test]
		public void TestIsSubclassOf () {
			Assert.IsTrue (typeof (ICloneable).IsSubclassOf (typeof (object)), "#01");

			// Tests for byref types
			Type paramType = typeof (TypeTest).GetMethod ("ByrefMethod", BindingFlags.Instance|BindingFlags.NonPublic).GetParameters () [0].ParameterType;
			Assert.IsTrue (!paramType.IsSubclassOf (typeof (ValueType)), "#02");
			//Assert.IsTrue (paramType.IsSubclassOf (typeof (Object)), "#03");
			Assert.IsTrue (!paramType.IsSubclassOf (paramType), "#04");
		}

		[Test]
		public void TestGetMethodImpl() {
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
		public void TestGetPropertyImpl() {
			// Test getting property that is exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int32) }).DeclaringType, "#01");

			// Test getting property that is not exact
			Assert.AreEqual (typeof (NewVTable), typeof (NewVTable).GetProperty ("Item", new Type[1] { typeof (Int16) }).DeclaringType, "#02");

			// Test overriding of properties when only the set accessor is overriden
			Assert.AreEqual (1, typeof (Derived1).GetProperties ().Length, "#03");
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
			Assert.AreEqual (LayoutKind.Auto, attr1.Value);

			StructLayoutAttribute attr2 = typeof (Class1).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr2.Value);
			Assert.AreEqual (4, attr2.Pack);
			Assert.AreEqual (64, attr2.Size);

			StructLayoutAttribute attr3 = typeof (Class2).StructLayoutAttribute;
			Assert.AreEqual (LayoutKind.Explicit, attr3.Value);
			Assert.AreEqual (CharSet.Unicode, attr3.CharSet);
		}
#endif

		[Test]
		public void Namespace () {
			Assert.AreEqual (null, typeof (NoNamespaceClass).Namespace);
		}

		[Test]
		public void GetInterfaces () {
			Type[] t = typeof (Duper).GetInterfaces ();
			Assert.AreEqual (1, t.Length);
			Assert.AreEqual (typeof (ICloneable), t[0]);

			Type[] t2 = typeof (IFace3).GetInterfaces ();
			Assert.AreEqual (2, t2.Length);
		}

		public int AField;

		[Test]
		public void GetFieldIgnoreCase () {
			Assert.IsNotNull (typeof (TypeTest).GetField ("afield", BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase));
		}

#if NET_2_0
		public int Count {
			internal get {
				return 0;
			}

			set {
			}
		}

		[Test]
		public void GetPropertyAccessorModifiers () {
			Assert.IsNotNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.Public));
			Assert.IsNull (typeof (TypeTest).GetProperty ("Count", BindingFlags.Instance | BindingFlags.NonPublic));
		}
#endif

		[Test]
		public void IsPrimitive () {
			Assert.IsTrue (typeof (IntPtr).IsPrimitive);
		}

		[Test]
		[Category("NotDotNet")]
		// Depends on the GAC working, which it doesn't durring make distcheck.
		[Category ("NotWorking")]
		public void GetTypeWithWhitespace () {
			Assert.IsNotNull (Type.GetType
						   (@"System.Configuration.NameValueSectionHandler,
			System,
Version=1.0.5000.0,
Culture=neutral
,
PublicKeyToken=b77a5c561934e089"));
		}
		
		[Test]
		public void ExerciseFilterName() {
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
		public void ExerciseFilterNameIgnoreCase() {
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

		public int byref_field;

		public int byref_property {
			get {
				return 0;
			}
		}

		[Test]
		public void ByrefTypes ()
		{
			Type t = Type.GetType ("MonoTests.System.TypeTest&");
			Assert.AreEqual (0, t.GetMethods (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetConstructors (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetEvents (BindingFlags.Public | BindingFlags.Instance).Length);
			Assert.AreEqual (0, t.GetProperties (BindingFlags.Public | BindingFlags.Instance).Length);

			Assert.IsNull (t.GetMethod ("ByrefTypes"));
			Assert.IsNull (t.GetField ("byref_field"));
			Assert.IsNull (t.GetProperty ("byref_property"));
		}

		struct B
		{
			int value;
		}

		[Test]
		public void CreateValueTypeNoCtor () {
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateValueTypeNoCtorArgs () {
			typeof(B).InvokeMember ("", BindingFlags.CreateInstance, null, null, new object [] { 1 });
		}

		class X
		{
			public static int Value;
		}

		class Y  : X
		{
		}

		[Test]
		public void InvokeMemberGetSetField () {
			typeof (X).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.SetField, null, null, new object [] { 5 });

			Assert.AreEqual (5, X.Value);
			Assert.AreEqual (5, typeof (X).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.GetField, null, null, new object [0]));
			Assert.AreEqual (5, Y.Value);
			Assert.AreEqual (5, typeof (Y).InvokeMember ("Value", BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy|BindingFlags.GetField, null, null, new object [0]));
		}			

		class TakesInt {
			public TakesInt (int x) {}
		}

		class TakesObject {
			public TakesObject (object x) {}
		}

		// Filed as bug #75241
		[Test]
		public void GetConstructoNullInTypes ()
		{
			// This ends up calling type.GetConstructor ()
			Activator.CreateInstance (typeof (TakesInt), new object [] { null });
			Activator.CreateInstance (typeof (TakesObject), new object [] { null });
		}

		// bug #76150
		[Test]
		public void IsDefined ()
		{
			Assert.IsTrue (typeof (A).IsDefined (typeof (NemerleAttribute), false), "#1");
			Assert.IsTrue (typeof (A).IsDefined (typeof (VolatileModifier), false), "#2");
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

#if NET_2_0
		[Test]
		public void FullNameGenerics ()
		{
			Type fooType = typeof (Foo<>);

			Assert.IsNotNull (fooType.FullName, "#1");
			Assert.IsNotNull (fooType.AssemblyQualifiedName, "#1a");

			FieldInfo field = fooType.GetField ("Whatever");
			Assert.IsNotNull (field, "#2");
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
			Assert.IsTrue (ibar_short_type.IsAssignableFrom (baz_short_type), "Baz<short> -> IBaz<short>");

			Assert.IsFalse (ibar_int_type.IsAssignableFrom (baz_short_type), "Baz<int> -!-> IBaz<short>");
			Assert.IsFalse (ibar_short_type.IsAssignableFrom (baz_int_type), "Baz<short> -!-> IBaz<int>");
		}
#endif

		public class NemerleAttribute : Attribute
		{ }

		public class VolatileModifier : NemerleAttribute
		{ }

		[VolatileModifier]
		class A { }
	}
}
