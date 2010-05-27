//
// EnumBuiderTest - NUnit Test Cases for the EnumBuider class
//
// Keerti Narayan (keertiln@rediffmail.com)
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class EnumBuilderTest
	{
		private static string _assemblyName = "MonoTests.System.Reflection.Emit.EnumBuilder";
		private static string _moduleName = "EmittedModule";
		private static string _enumNamespace = "MyNameSpace";
		private static string _enumName = "MyEnum";
		private static Type _enumType = typeof (Int32);
		private static string _fieldName = "MyField";
		private static object _fieldValue = 1;

		[Test]
		public void TestEnumBuilder ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			VerifyType (enumBuilder);

			Assert.IsNotNull (enumBuilder.TypeToken, "#1");
			Assert.IsNotNull (enumBuilder.UnderlyingField, "#2");
			Assert.IsNull (enumBuilder.DeclaringType, "#3");
			Assert.IsNull (enumBuilder.ReflectedType, "#4");
			Assert.AreEqual (_enumType, enumBuilder.UnderlyingSystemType, "#5");
		}

		[Test]
		[Category ("ValueAdd")]
		public void TestEnumBuilder_NotInMono ()
		{
			// If we decide to fix this (I dont see why we should),
			// move to the routine above

			EnumBuilder enumBuilder = GenerateEnum ();
			Assert.IsFalse (enumBuilder.IsSerializable);
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#else
		[ExpectedException (typeof (NotSupportedException))]
#endif
		public void TestHasElementTypeEnumBuilderIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			bool hasElementType = enumBuilder.HasElementType;
#if NET_2_0
			Assert.IsFalse (hasElementType);
#else
			Assert.Fail ("Should have failed: " + hasElementType);
#endif
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("ValueAdd")] // Is this worth fixing, or is this considered, "extra value"?
#endif
		public void TestHasElementTypeEnumBuilderComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			bool hasElementType = enumBuilder.HasElementType;
#if NET_2_0
			Assert.IsFalse (hasElementType);
#else
			Assert.Fail ("Should have failed: " + hasElementType);
#endif
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDefineLiteralTypeComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			Type enumType = enumBuilder.CreateType ();
			// you should not be able to define literal after type 
			// has been created
			enumBuilder.DefineLiteral (_fieldName, _fieldValue);
		}

		[Test]
		public void TestDefineLiteralTypeIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			FieldBuilder fieldBuilder = enumBuilder.DefineLiteral (_fieldName, _fieldValue);
			Type enumType = enumBuilder.CreateType ();

			Assert.IsTrue (fieldBuilder.IsPublic, "#1");
			Assert.IsTrue (fieldBuilder.IsStatic, "#2");
			Assert.IsTrue (fieldBuilder.IsLiteral, "#3");
			Assert.AreEqual (_fieldName, fieldBuilder.Name, "#4");
#if NET_2_0
			Assert.IsFalse (enumType == fieldBuilder.DeclaringType, "#5");
			Assert.IsFalse (enumBuilder == fieldBuilder.DeclaringType, "#6");
			Assert.AreEqual (enumType.FullName, fieldBuilder.DeclaringType.FullName, "#7");
			Assert.IsFalse (enumType == fieldBuilder.FieldType, "#8");
			Assert.AreEqual (enumBuilder, fieldBuilder.FieldType, "#9");
#else
			Assert.AreEqual (enumType, fieldBuilder.DeclaringType, "#5");
			Assert.AreEqual (_enumType, fieldBuilder.FieldType, "#6");
#endif
		}

		[Test]
		public void TestEnumType ()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();

			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);

			VerifyType (enumType);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestEnumBuilderGUIDIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			Guid guid = enumBuilder.GUID;
		}

		[Test]
		[Category ("NotWorking")] // Bug:71299
		public void TestEnumBuilderGUIDComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			Assert.IsTrue (enumBuilder.GUID != Guid.Empty);
		}

		[Test]
		public void TestEnumTypeGUID ()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();
			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);

			// Tested in above test: Assert (enumType.GUID != Guid.Empty);
			Assert.IsNull (enumType.DeclaringType);
		}

		[Test]
		public void TestFieldProperties ()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();
			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			FieldBuilder fieldBuilder = GenerateField (enumBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);
			FieldInfo fi = enumType.GetField (_fieldName);
			Object o = fi.GetValue (enumType);

			Assert.IsTrue (fi.IsLiteral, "#1");
			Assert.IsTrue (fi.IsPublic, "#2");
			Assert.IsTrue (fi.IsStatic, "#3");
			Assert.AreEqual (enumBuilder, fieldBuilder.FieldType, "#4");
			Assert.IsFalse (enumType == fieldBuilder.FieldType, "#5");
#if NET_2_0
			Assert.AreEqual (enumType.FullName, fieldBuilder.FieldType.FullName, "#6");
			Assert.IsFalse (_enumType == fieldBuilder.FieldType, "#7");
#else
			Assert.IsFalse (enumType.FullName == fieldBuilder.FieldType.FullName, "#6");
			Assert.AreEqual (_enumType, fieldBuilder.FieldType, "#7");
#endif

			object fieldValue = fi.GetValue (enumType);
#if NET_2_0
			Assert.IsFalse (_fieldValue == fieldValue, "#8");
			Assert.IsTrue (fieldValue.GetType ().IsEnum, "#9");
			Assert.AreEqual (enumType, fieldValue.GetType (), "#10");
#else
			Assert.AreEqual (_fieldValue, fieldValue, "#8");
			Assert.IsFalse (fieldValue.GetType ().IsEnum, "#9");
			Assert.AreEqual (_enumType, fieldValue.GetType (), "#10");
#endif
			Assert.AreEqual (_fieldValue, (int) fieldValue, "#11");
		}

		[Test]
		public void TestFindInterfaces ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();

			Type [] interfaces = enumBuilder.FindInterfaces (
				new TypeFilter (MyInterfaceFilter),
				"System.Collections.IEnumerable");
			Assert.AreEqual (0, interfaces.Length);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("ValueAdd")]
		public void TestFindMembersIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			GenerateField (enumBuilder);

			MemberInfo [] members = enumBuilder.FindMembers (
				MemberTypes.All, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				_fieldName);
		}

#if NET_4_0
		[Test]
		public void GetEnumUnderlyingType ()
		{
			var @enum = GenerateEnum ();

			Assert.AreEqual (_enumType, @enum.GetEnumUnderlyingType ());
		}
#endif

		[Test]
		public void TestFindMembersComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			GenerateField (enumBuilder);
			enumBuilder.CreateType ();

			MemberInfo [] members = enumBuilder.FindMembers (
				MemberTypes.Field, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				_fieldName);
			Assert.AreEqual (1, members.Length, "#1");

			members = enumBuilder.FindMembers (
				MemberTypes.Field, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				"doesntmatter");
			Assert.AreEqual (0, members.Length, "#2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("ValueAdd")]
		public void TestGetConstructorIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.GetConstructor (BindingFlags.Public, null,
				CallingConventions.Any, Type.EmptyTypes, new ParameterModifier [0]);
		}

		[Test]
		public void TestGetConstructorComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				Type.EmptyTypes, new ParameterModifier [0]);
			Assert.IsNull (ctor);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetConstructorNullTypes ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				null, new ParameterModifier [0]);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetConstructorNullElementType ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				new Type [] { null }, new ParameterModifier [0]);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotWorking")]
		public void TestGetConstructorsIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();

			ConstructorInfo [] ctors = enumBuilder.GetConstructors (
				BindingFlags.Instance | BindingFlags.Public);
			Assert.AreEqual (0, ctors.Length);
		}

		[Test]
		public void TestGetConstructorsComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();

			ConstructorInfo [] ctors = enumBuilder.GetConstructors (
				BindingFlags.Instance | BindingFlags.Public);
			Assert.AreEqual (0, ctors.Length);
		}

		[Test]
		public void TestIsValue__SpecialName ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			Type enumType = enumBuilder.CreateType ();
			FieldInfo value = enumType.GetField ("value__", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.AreEqual (FieldAttributes.RTSpecialName, value.Attributes & FieldAttributes.RTSpecialName);
		}

		private static void VerifyType (Type type)
		{
			Assert.IsNotNull (type.Assembly, "#V1");
			Assert.IsNotNull (type.AssemblyQualifiedName, "#V2");
			Assert.IsNotNull (type.BaseType, "#V3");
			Assert.IsNotNull (type.FullName, "#V4");
			Assert.IsNotNull (type.Module, "#V5");
			Assert.IsNotNull (type.Name, "#V6");
			Assert.IsNotNull (type.Namespace, "#V7");
			Assert.IsNotNull (type.UnderlyingSystemType, "#V8");

#if ONLY_1_1
			// on .NET 2.0, module is name is fixed to 
			Assert.AreEqual (_moduleName, type.Module.Name, "#V9");
#endif
			Assert.AreEqual (_enumNamespace, type.Namespace, "#V10");
			Assert.AreEqual (_enumName, type.Name, "#V11");
			Assert.AreEqual (typeof (Enum), type.BaseType, "#V12");
			Assert.AreEqual (MemberTypes.TypeInfo, type.MemberType, "#V13");
			Assert.AreEqual (typeof (int), Enum.GetUnderlyingType (type), "#V14");

			Assert.IsFalse (type.IsArray, "#V15");
			Assert.IsFalse (type.IsAutoClass, "#V16");
			Assert.IsTrue (type.IsAutoLayout, "#V17");
			Assert.IsFalse (type.IsByRef, "#V18");
			Assert.IsFalse (type.IsClass, "#V19");
			Assert.IsFalse (type.IsCOMObject, "#V20");
			Assert.IsFalse (type.IsContextful, "#V21");
			Assert.IsTrue (type.IsEnum, "#V22");
			Assert.IsFalse (type.IsExplicitLayout, "#V23");
			Assert.IsFalse (type.IsImport, "#V24");
			Assert.IsFalse (type.IsInterface, "#V25");
			Assert.IsFalse (type.IsLayoutSequential, "#V26");
			Assert.IsFalse (type.IsMarshalByRef, "#V27");
			Assert.IsFalse (type.IsNestedAssembly, "#V28");
			Assert.IsFalse (type.IsNestedFamily, "#V29");
			Assert.IsFalse (type.IsNestedPublic, "#V30");
			Assert.IsFalse (type.IsNestedPrivate, "#V31");
			Assert.IsFalse (type.IsNotPublic, "#V32");
			Assert.IsFalse (type.IsPrimitive, "#V33");
			Assert.IsFalse (type.IsPointer, "#V34");
			Assert.IsTrue (type.IsPublic, "#V35");
			Assert.IsTrue (type.IsSealed, "#V36");
			Assert.IsFalse (type.IsUnicodeClass, "#V37");
			Assert.IsFalse (type.IsSpecialName, "#V38");
			Assert.IsTrue (type.IsValueType, "#V39");
		}

		public static bool MyInterfaceFilter (Type t, object filterCriteria)
		{
			if (t.ToString () == filterCriteria.ToString ())
				return true;
			else
				return false;
		}

		public static bool MemberNameFilter (MemberInfo m, object filterCriteria)
		{
			if (m.Name == filterCriteria.ToString ())
				return true;
			else
				return false;
		}

		private static AssemblyName GetAssemblyName ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = _assemblyName;
			return assemblyName;
		}

		private static AssemblyBuilder GenerateAssembly ()
		{
			return AppDomain.CurrentDomain.DefineDynamicAssembly (
				GetAssemblyName (), AssemblyBuilderAccess.RunAndSave);
		}

		private static ModuleBuilder GenerateModule ()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();
			return assemblyBuilder.DefineDynamicModule (_moduleName);
		}

		private static ModuleBuilder GenerateModule (AssemblyBuilder assemblyBuilder)
		{
			return assemblyBuilder.DefineDynamicModule (_moduleName);
		}

		private static EnumBuilder GenerateEnum ()
		{
			ModuleBuilder modBuilder = GenerateModule ();
			return modBuilder.DefineEnum (_enumNamespace + "."
				+ _enumName, TypeAttributes.Public, _enumType);
		}

		private static EnumBuilder GenerateEnum (ModuleBuilder modBuilder)
		{
			return modBuilder.DefineEnum (_enumNamespace + "."
				+ _enumName, TypeAttributes.Public, _enumType);
		}

		private static FieldBuilder GenerateField ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			return enumBuilder.DefineLiteral (_fieldName, _fieldValue);
		}

		private static FieldBuilder GenerateField (EnumBuilder enumBuilder)
		{
			return enumBuilder.DefineLiteral (_fieldName, _fieldValue);
		}
	}
}
