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

namespace MonoTests.System.Reflection.Emit {
	[TestFixture]
	public class EnumBuilderTest : Assertion {
		private static string _assemblyName = "MonoTests.System.Reflection.Emit.EnumBuilder";
		private static string _moduleName = "EmittedModule";
		private static string _enumNamespace = "MyNameSpace";
		private static string _enumName = "MyEnum";
		private static Type _enumType = typeof(Int32);
		private static string _fieldName = "MyField";
		private static object _fieldValue = 1;

		[Test]
		public void TestEnumBuilder()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			VerifyType (enumBuilder);

			AssertNotNull (enumBuilder.TypeToken);
			AssertNotNull (enumBuilder.UnderlyingField);
			AssertNull ("type.DeclaringType of toplevel type should be null", enumBuilder.DeclaringType);
			AssertNull ("type.ReflectedType should be null", enumBuilder.ReflectedType);
			AssertEquals (_enumType, enumBuilder.UnderlyingSystemType);
			AssertEquals ("Comparing the IsSerializable field", false, enumBuilder.IsSerializable);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestHasElementTypeEnumBuilderIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			bool hasElementType = enumBuilder.HasElementType;
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestHasElementTypeEnumBuilderComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			bool hasElementType = enumBuilder.HasElementType;
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestDefineLiteralTypeComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			Type enumType = enumBuilder.CreateType ();
			// you should not be able to define literal after type 
			// has been created
			FieldBuilder fieldBuilder = enumBuilder.DefineLiteral (_fieldName, _fieldValue);
		}

		[Test]
		public void TestDefineLiteralTypeIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			FieldBuilder fieldBuilder = enumBuilder.DefineLiteral (_fieldName, _fieldValue);
			Type enumType = enumBuilder.CreateType ();

			AssertEquals (enumType, fieldBuilder.DeclaringType);
			AssertEquals (_enumType, fieldBuilder.FieldType);
			AssertEquals (true, fieldBuilder.IsPublic);
			AssertEquals (true, fieldBuilder.IsStatic);
			AssertEquals (true, fieldBuilder.IsLiteral);
			AssertEquals (_fieldName, fieldBuilder.Name);
		}

		[Test]
		public void TestEnumType()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();

			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);

			VerifyType (enumType);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestEnumBuilderGUIDIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			Guid guid =  enumBuilder.GUID;
		}

		[Test]
		public void TestEnumBuilderGUIDComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			Assert (enumBuilder.GUID != Guid.Empty);
		}

		[Test]
		public void TestEnumTypeGUID ()
		{
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();
			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);

			Assert (enumType.GUID != Guid.Empty);
			AssertNull ("type.DeclaringType of toplevel type should be null", enumType.DeclaringType);
		}

		[Test]
		public void TestFieldProperties() {
			AssemblyBuilder assemblyBuilder = GenerateAssembly ();
			ModuleBuilder modBuilder = GenerateModule (assemblyBuilder);
			EnumBuilder enumBuilder = GenerateEnum (modBuilder);
			FieldBuilder fieldBuilder = GenerateField (enumBuilder);
			enumBuilder.CreateType ();

			Type enumType = assemblyBuilder.GetType (_enumNamespace + "." + _enumName, true);
			FieldInfo fi = enumType.GetField (_fieldName);
			Object o = fi.GetValue(enumType);

			AssertEquals ("Checking the value of the Field to be 1", _fieldValue, fi.GetValue (enumType));
			AssertEquals ("Checking if the field is a Literal", true, fi.IsLiteral);
			AssertEquals ("Checking if the field is Public", true, fi.IsPublic);
			AssertEquals ("Checking if the field is Static", true, fi.IsStatic);
		}

		[Test]
		public void TestFindInterfaces ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();

			Type[] interfaces = enumBuilder.FindInterfaces (
				new TypeFilter (MyInterfaceFilter), 
				"System.Collections.IEnumerable");
			AssertEquals (0, interfaces.Length);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestFindMembersIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			GenerateField (enumBuilder);

			MemberInfo[] members = enumBuilder.FindMembers (
				MemberTypes.All, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				_fieldName);
		}

		[Test]
		public void TestFindMembersComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			GenerateField (enumBuilder);
			enumBuilder.CreateType ();

			MemberInfo[] members = enumBuilder.FindMembers (
				MemberTypes.Field, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				_fieldName);
			AssertEquals (1, members.Length);

			members = enumBuilder.FindMembers (
				MemberTypes.Field, BindingFlags.Static |
				BindingFlags.Public, new MemberFilter (MemberNameFilter),
				"doesntmatter");
			AssertEquals (0, members.Length);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetConstructorIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.GetConstructor (BindingFlags.Public, null,
				CallingConventions.Any, Type.EmptyTypes, new ParameterModifier[0]);
		}

		[Test]
		public void TestGetConstructorComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				Type.EmptyTypes, new ParameterModifier[0]);
			AssertNull (ctor);
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void TestGetConstructorNullTypes ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				null, new ParameterModifier[0]);
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void TestGetConstructorNullElementType ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();
			ConstructorInfo ctor = enumBuilder.GetConstructor (
				BindingFlags.Public, null, CallingConventions.Any,
				new Type[] { null }, new ParameterModifier[0]);
		}

		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestGetConstructorsIncomplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();

			ConstructorInfo[] ctors = enumBuilder.GetConstructors (
				BindingFlags.Instance | BindingFlags.Public);
			AssertEquals (0, ctors.Length);
		}

		[Test]
		public void TestGetConstructorsComplete ()
		{
			EnumBuilder enumBuilder = GenerateEnum ();
			enumBuilder.CreateType ();

			ConstructorInfo[] ctors = enumBuilder.GetConstructors (
				BindingFlags.Instance | BindingFlags.Public);
			AssertEquals (0, ctors.Length);
		}

		private static void VerifyType (Type type)
		{
			AssertNotNull ("type.Assembly should not be null", type.Assembly);
			AssertNotNull ("type.AssemblyQualifiedName should not be null", type.AssemblyQualifiedName);
			AssertNotNull ("type.BaseType should not be null", type.BaseType);
			AssertNotNull ("type.FullName should not be null", type.FullName);
			AssertNotNull ("type.Module should not be null", type.Module);
			AssertNotNull ("type.Name should not be null", type.Name);
			AssertNotNull ("type.Namespace should not be null", type.Namespace);
			AssertNotNull ("type.UnderlyingSystemType should not be null", type.UnderlyingSystemType);

			AssertEquals (_moduleName, type.Module.Name);
			AssertEquals (_enumNamespace, type.Namespace);
			AssertEquals (_enumName, type.Name);
			AssertEquals (typeof(Enum), type.BaseType);
			AssertEquals (MemberTypes.TypeInfo, type.MemberType);
			AssertEquals (typeof(int), Enum.GetUnderlyingType (type));

			AssertEquals ("Comparing the IsArray field", false, type.IsArray);
			AssertEquals ("Comparing the IsAutoClass field", false, type.IsAutoClass);
			AssertEquals ("Comparing the IsAutoLayout field", true, type.IsAutoLayout);
			AssertEquals ("Comparing the IsByRef field", false, type.IsByRef);
			AssertEquals ("Comparing the IsClass field", false, type.IsClass);
			AssertEquals ("Comparing the IsComObject field", false, type.IsCOMObject);
			AssertEquals ("Comparing the IsContextful field", false, type.IsContextful);
			AssertEquals ("Comparing the IsEnum field", true, type.IsEnum);
			AssertEquals ("Comparing the IsExplicitLayout field", false, type.IsExplicitLayout);
			AssertEquals ("Comparing the IsImport field", false, type.IsImport);
			AssertEquals ("Comparing the IsInterface field", false, type.IsInterface);
			AssertEquals ("Comparing the IsLayoutSequential field", false, type.IsLayoutSequential);
			AssertEquals ("Comparing the IsMarshalByRef field", false, type.IsMarshalByRef);
			AssertEquals ("Comparing the IsNestedAssembly field", false, type.IsNestedAssembly);
			AssertEquals ("Comparing the IsNestedFamily field", false, type.IsNestedFamily);
			AssertEquals ("Comparing the IsNestedPublic field", false, type.IsNestedPublic);
			AssertEquals ("Comparing the IsNestedPrivate field", false, type.IsNestedPrivate);
			AssertEquals ("Comparing the IsNotPublic field", false, type.IsNotPublic);
			AssertEquals ("Comparing the IsPrimitive field", false, type.IsPrimitive);
			AssertEquals ("Comparing the IsPointer field", false, type.IsPointer);
			AssertEquals ("Comparing the IsPublic field", true, type.IsPublic);
			AssertEquals ("Comparing the IsSealed field", true, type.IsSealed);
			AssertEquals ("Comparing the IsUnicode field", false, type.IsUnicodeClass);
			AssertEquals ("Comparing the requires special handling field", false, type.IsSpecialName);
			AssertEquals ("Comparing the IsValueType field", true, type.IsValueType);
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
