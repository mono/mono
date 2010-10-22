//
// TypeBuilderTest.cs - NUnit Test Cases for the TypeBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO:
//  - implement a mechnanism for easier testing of null argument exceptions
//  - with overloaded methods like DefineNestedType (), check the defaults
//    on the shorter versions.
//  - ToString on enums with the flags attribute set should print all
//    values which match, e.g. 0 == AutoLayou,AnsiClass,NotPublic
//

using System;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using NUnit.Framework;
using System.Runtime.CompilerServices;

using System.Collections.Generic;

namespace MonoTests.System.Reflection.Emit
{
	public interface EmptyInterface
	{
	}

	public interface OneMethodInterface
	{
		void foo ();
	}

	public class SimpleTestAttribute : Attribute
	{
	}
	public class EmptyIfaceImpl : EmptyInterface
	{
	}

	public class Gen<T> {
		public static T field = default(T);
	}

	[TestFixture]
	public class TypeBuilderTest
	{
		private interface AnInterface
		{
		}

		public interface Foo
		{
		}

		public interface Bar : Foo
		{
		}

		public interface Baz : Bar
		{
		}

		public interface IMoveable
		{
		}

		public interface IThrowable : IMoveable
		{
		}

		public interface ILiquid
		{
		}

		public interface IWater : ILiquid
		{
		}

		public interface IAir
		{
		}

		public interface IDestroyable
		{
		}

		public class Tuple <A,B> {
			A a;
			B b;
		}

		private AssemblyBuilder assembly;

		private ModuleBuilder module;

		static string ASSEMBLY_NAME = "MonoTests.System.Reflection.Emit.TypeBuilderTest";

		[SetUp]
		protected void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
		}

		static int typeIndexer = 0;

		// Return a unique type name
		private string genTypeName ()
		{
			return "t" + (typeIndexer++);
		}

		private string nullName ()
		{
			return String.Format ("{0}", (char) 0);
		}

		[Test]
		public void TestAssembly ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			Assert.AreEqual (assembly, tb.Assembly);
		}

		[Test]
		public void TestAssemblyQualifiedName ()
		{
			TypeBuilder tb = module.DefineType ("A.B.C.D", TypeAttributes.Public);
			Assert.AreEqual ("A.B.C.D, " + assembly.GetName ().FullName,
				tb.AssemblyQualifiedName);
		}

		[Test]
		public void TestAttributes ()
		{
			TypeAttributes attrs = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
			TypeBuilder tb = module.DefineType (genTypeName (), attrs);
			Assert.AreEqual (attrs, tb.Attributes);
		}

		[Test]
		public void TestBaseTypeClass ()
		{
			TypeAttributes attrs = TypeAttributes.Public;
			TypeBuilder tb = module.DefineType (genTypeName (), attrs);
			Assert.AreEqual (typeof (object), tb.BaseType, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), attrs, tb);
			Assert.AreEqual (tb, tb2.BaseType, "#2");
		}

		[Test] // bug #71301
		public void TestBaseTypeInterface ()
		{
			TypeBuilder tb3 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			Assert.IsNull (tb3.BaseType);
		}

		[Test]
		public void TestDeclaringType ()
		{
			TypeAttributes attrs = 0;
			TypeBuilder tb = module.DefineType (genTypeName (), attrs);
			Assert.IsNull (tb.DeclaringType, "#1");

			attrs = TypeAttributes.NestedPublic;
			TypeBuilder tb2 = tb.DefineNestedType (genTypeName (), attrs);
			TypeBuilder tb3 = tb2.DefineNestedType (genTypeName (), attrs);
			Assert.AreEqual (tb3.DeclaringType.DeclaringType, tb, "#2");
		}

		[Test]
		public void TestFullName ()
		{
			string name = genTypeName ();
			TypeAttributes attrs = 0;
			TypeBuilder tb = module.DefineType (name, attrs);
			Assert.AreEqual (name, tb.FullName, "#1");

			string name2 = genTypeName ();
			attrs = TypeAttributes.NestedPublic;
			TypeBuilder tb2 = tb.DefineNestedType (name2, attrs);

			string name3 = genTypeName ();
			attrs = TypeAttributes.NestedPublic;
			TypeBuilder tb3 = tb2.DefineNestedType (name3, attrs);

			Assert.AreEqual (name + "+" + name2 + "+" + name3, tb3.FullName, "#2");
		}

		[Test]
		public void DefineCtorUsingDefineMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public | TypeAttributes.Class);
			MethodBuilder mb = tb.DefineMethod(
				".ctor", MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName,
				null, null);
			ILGenerator ilgen = mb.GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Call,
					   typeof(object).GetConstructor(Type.EmptyTypes));
			ilgen.Emit(OpCodes.Ret);
			Type t = tb.CreateType();

			Assert.AreEqual (1, t.GetConstructors ().Length);
		}

		[Test]
		public void TestGUIDIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				Guid g = tb.GUID;
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // bug #71302
		[Category ("NotWorking")]
		public void TestGUIDComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			Assert.IsTrue (tb.GUID != Guid.Empty);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestFixedGUIDComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			Guid guid = Guid.NewGuid ();

			ConstructorInfo guidCtor = typeof (GuidAttribute).GetConstructor (
				new Type [] { typeof (string) });

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
				new object [] { guid.ToString ("D") }, new FieldInfo [0], new object [0]);

			tb.SetCustomAttribute (caBuilder);
			tb.CreateType ();
			Assert.AreEqual (guid, tb.GUID);
		}

		[Test]
		public void TestHasElementType_Incomplete ()
		{
			// According to the MSDN docs, this member works, but in reality, it
			// returns a NotSupportedException
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.HasElementType);
		}

		[Test]
		public void TestHasElementType_Complete ()
		{
			// According to the MSDN docs, this member works, but in reality, it
			// returns a NotSupportedException
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			Assert.IsFalse (tb.HasElementType);
		}

		[Test] // bug #324692
		public void CreateType_Enum_NoInstanceField ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));

			try {
				tb.CreateType ();
				Assert.Fail ("#1: must throw TypeLoadException");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test] // bug #324692
		public void TestCreateTypeReturnsNullOnSecondCallForBadType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));

			try {
				tb.CreateType ();
				Assert.Fail ("#A1");
			} catch (TypeLoadException ex) {
				Assert.AreEqual (typeof (TypeLoadException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsTrue (tb.IsCreated (), "#B1");
			Assert.IsNull (tb.CreateType (), "#B2");
			Assert.IsTrue (tb.IsCreated (), "#B3");
		}

		[Test]
		public void TestEnumWithEmptyInterfaceBuildsOk ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			tb.AddInterfaceImplementation (typeof (EmptyInterface));

			try {
				tb.CreateType ();
			} catch (TypeLoadException) {
				Assert.Fail ("#1: must build enum type ok");
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestEnumWithNonEmptyInterfaceBuildsFails ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			tb.AddInterfaceImplementation (typeof (OneMethodInterface));

			try {
				tb.CreateType ();
				Assert.Fail ("#1: type doesn't have all interface methods");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestTypeDontImplementInterfaceMethodBuildsFails ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (object));

			tb.AddInterfaceImplementation (typeof (OneMethodInterface));

			try {
				tb.CreateType ();
				Assert.Fail ("#1: type doesn't have all interface methods");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		public void TestEnumWithSequentialLayoutBuildsFails ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable |
				TypeAttributes.SequentialLayout, typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			try {
				tb.CreateType ();
				Assert.Fail ("#1: type doesn't have all interface methods");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		public void TestEnumWithExplicitLayoutBuildsFails ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable |
				TypeAttributes.ExplicitLayout, typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			try {
				tb.CreateType ();
				Assert.Fail ("#1: type doesn't have all interface methods");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		public void TestEnumWithMethodsBuildFails ()
		{
			TypeBuilder tb = module.DefineType ("FooEnum7",
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			MethodBuilder methodBuilder = tb.DefineMethod("mmm",
				MethodAttributes.Public | MethodAttributes.Virtual,
				null,
				new Type[] { });

			methodBuilder.GetILGenerator().Emit(OpCodes.Ret);
			try {
				tb.CreateType ();
				Assert.Fail ("#1: enum has method");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		public void TestEnumWithBadTypeValueFieldBuildFails ()
		{
			Type[] badTypes = {
				typeof (object),
				typeof (string),
				typeof (DateTime)
			};

			foreach (Type type in badTypes) {
				TypeBuilder tb = module.DefineType (genTypeName (),
					TypeAttributes.Sealed | TypeAttributes.Serializable,
					typeof (Enum));
				tb.DefineField ("value__", type, FieldAttributes.SpecialName |
					FieldAttributes.Private | FieldAttributes.RTSpecialName);

				try {
					tb.CreateType ();
					Assert.Fail ("#1: enum using bad type: " + type);
				} catch (TypeLoadException) {
				}

				Assert.IsTrue (tb.IsCreated (), "#2");
			}
		}

		[Test]
		public void TestEnumWithGoodTypeValueFieldBuildOk ()
		{
			Type[] goodTypes = {
				typeof (byte),typeof (sbyte),typeof (bool),
				typeof (ushort),typeof (short),typeof (char),
				typeof (uint),typeof (int),
				typeof (ulong),typeof (long),
				typeof (UIntPtr),typeof (IntPtr),
			};

			foreach (Type type in goodTypes) {
				TypeBuilder tb = module.DefineType (genTypeName (),
					TypeAttributes.Sealed | TypeAttributes.Serializable,
					typeof (Enum));
				tb.DefineField ("value__", type, FieldAttributes.SpecialName |
					FieldAttributes.Private | FieldAttributes.RTSpecialName);

				try {
					tb.CreateType ();
				} catch (TypeLoadException) {
					Assert.Fail ("#1: enum using good type: " + type);
				}
			}
		}

		[Test]
		public void TestEnumWithMultipleValueFieldsBuildFals ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);
			tb.DefineField ("value2__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			try {
				tb.CreateType ();
				Assert.Fail ("#1: invalid enum type");
			} catch (TypeLoadException) {
			}

			Assert.IsTrue (tb.IsCreated (), "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestEnumWithEmptyInterfaceCanBeCasted ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);
			tb.AddInterfaceImplementation (typeof (EmptyInterface));

			try {
				tb.CreateType ();
			} catch (TypeLoadException) {
				Assert.Fail ("#1: must build enum type ok");
			}

			try {
				EmptyInterface obj = (EmptyInterface) Activator.CreateInstance (tb);
				Assert.IsNotNull (obj, "#2");
			} catch (TypeLoadException) {
				Assert.Fail ("#3: must cast enum to interface");
			}
		}

		[Test]
		public void TestEnumWithValueFieldBuildOk ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);

			try {
				tb.CreateType ();
			} catch (TypeLoadException) {
				Assert.Fail ("#1: must build enum type ok");
			}
		}

		[Test]
		public void TestIsAbstract ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsAbstract, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Abstract);
			Assert.IsTrue (tb2.IsAbstract, "#2");
		}

		[Test]
		public void TestIsAnsiClass ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsTrue (tb.IsAnsiClass, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
			Assert.IsFalse (tb2.IsAnsiClass, "#2");
		}

		[Test]
		public void TestIsArray ()
		{
			// How can a TypeBuilder be an array ?
			string name = genTypeName ();
			TypeBuilder tb = module.DefineType (name);
			Assert.IsFalse (tb.IsArray);
		}

		[Test]
		public void TestIsAutoClass ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsAutoClass, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.AutoClass);
			Assert.IsTrue (tb2.IsAutoClass, "#2");
		}

		[Test]
		public void TestIsAutoLayout ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsTrue (tb.IsAutoLayout, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
			Assert.IsFalse (tb2.IsAutoLayout, "#2");
		}

		[Test]
		public void TestIsByRef ()
		{
			// How can a TypeBuilder be ByRef ?
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsByRef);
		}

		[Test]
		public void TestIsClass ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsTrue (tb.IsClass, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			Assert.IsFalse (tb2.IsClass, "#2");

			TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
			Assert.IsFalse (tb3.IsClass, "#3");

			TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
			Assert.IsFalse (tb4.IsClass, "#4");
		}

		[Test] // bug #71304
		public void TestIsCOMObject ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsCOMObject, "#1");

			tb = module.DefineType (genTypeName (), TypeAttributes.Import);
			Assert.IsTrue (tb.IsCOMObject, "#2");
		}

		[Test]
		public void TestIsContextful ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsContextful, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
			Assert.IsTrue (tb2.IsContextful, "#2");
		}

		[Test]
		public void TestIsEnum ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsEnum, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (ValueType));
			Assert.IsFalse (tb2.IsEnum, "#2");

			TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (Enum));
			Assert.IsTrue (tb3.IsEnum, "#3");
		}

		[Test]
		public void TestIsExplicitLayout ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsExplicitLayout, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.ExplicitLayout);
			Assert.IsTrue (tb2.IsExplicitLayout, "#2");
		}

		[Test]
		public void TestIsImport ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsImport, "#1");

			tb = module.DefineType (genTypeName (), TypeAttributes.Import);
			Assert.IsTrue (tb.IsImport, "#2");
		}

		[Test]
		public void TestIsInterface ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsInterface, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			Assert.IsTrue (tb2.IsInterface, "#2");

			TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
			Assert.IsFalse (tb3.IsInterface, "#3");
		}

		[Test]
		public void TestIsLayoutSequential ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsLayoutSequential, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SequentialLayout);
			Assert.IsTrue (tb2.IsLayoutSequential, "#2");
		}

		[Test]
		public void TestIsMarshalByRef ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsMarshalByRef, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (MarshalByRefObject));
			Assert.IsTrue (tb2.IsMarshalByRef, "#2");

			TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ContextBoundObject));
			Assert.IsTrue (tb3.IsMarshalByRef, "#3");
		}

		// TODO: Visibility properties

		[Test]
		public void TestIsPointer ()
		{
			// How can this be true?
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsPointer);
		}

		[Test]
		public void TestIsPrimitive ()
		{
			TypeBuilder tb = module.DefineType ("int");
			Assert.IsFalse (tb.IsPrimitive);
		}

		[Test]
		public void IsSealed ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsSealed, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
			Assert.IsTrue (tb2.IsSealed, "#2");
		}

		static string CreateTempAssembly ()
		{
			FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = Path.Combine (Path.GetTempPath (), "tmp" + num.ToString ("x") + ".dll");

				try {
					f = new FileStream (path, FileMode.CreateNew);
				} catch { }
			} while (f == null);

			f.Close ();


			return "tmp" + num.ToString ("x") + ".dll";
		}

		[Test]
		public void IsSerializable ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsSerializable, "#1");

			ConstructorInfo [] ctors = typeof (SerializableAttribute).GetConstructors (BindingFlags.Instance | BindingFlags.Public);
			Assert.IsTrue (ctors.Length > 0, "#2");

			tb.SetCustomAttribute (new CustomAttributeBuilder (ctors [0], new object [0]));
			Type createdType = tb.CreateType ();

			string an = CreateTempAssembly ();
			assembly.Save (an);
			Assert.IsTrue (createdType.IsSerializable, "#3");
			File.Delete (Path.Combine (Path.GetTempPath (), an));
		}

		[Test]
		public void TestIsSpecialName ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsSpecialName, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.SpecialName);
			Assert.IsTrue (tb2.IsSpecialName, "#2");
		}

		[Test]
		public void TestIsUnicodeClass ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsUnicodeClass, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.UnicodeClass);
			Assert.IsTrue (tb2.IsUnicodeClass, "#2");
		}

		[Test]
		public void TestIsValueType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsValueType, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			Assert.IsFalse (tb2.IsValueType, "#2");

			TypeBuilder tb3 = module.DefineType (genTypeName (), 0, typeof (ValueType));
			Assert.IsTrue (tb3.IsValueType, "#3");

			TypeBuilder tb4 = module.DefineType (genTypeName (), 0, typeof (Enum));
			Assert.IsTrue (tb4.IsValueType, "#4");
		}

		[Test]
		public void TestMemberType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.AreEqual (MemberTypes.TypeInfo, tb.MemberType);
		}

		[Test]
		public void TestModule ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.AreEqual (module, tb.Module);
		}

		[Test]
		public void TestName ()
		{
			TypeBuilder tb = module.DefineType ("A");
			Assert.AreEqual ("A", tb.Name, "#1");

			TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
			Assert.AreEqual ("E", tb2.Name, "#2");

			TypeBuilder tb3 = tb2.DefineNestedType ("A");
			Assert.AreEqual ("A", tb3.Name, "#3");

			/* Is .E a valid name ?
			TypeBuilder tb4 = module.DefineType (".E");
			Assert.AreEqual ("E", tb4.Name);
			*/
		}

		[Test]
		public void TestNamespace ()
		{
			TypeBuilder tb = module.DefineType ("A");
			Assert.AreEqual (string.Empty, tb.Namespace, "#1");

			TypeBuilder tb2 = module.DefineType ("A.B.C.D.E");
			Assert.AreEqual ("A.B.C.D", tb2.Namespace, "#2");

			TypeBuilder tb3 = tb2.DefineNestedType ("A");
			Assert.AreEqual (string.Empty, tb3.Namespace, "#3");

			/* Is .E a valid name ?
			TypeBuilder tb4 = module.DefineType (".E");
			Assert.AreEqual ("E", tb4.Name);
			*/
		}

		[Test]
		public void TestPackingSize ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.AreEqual (PackingSize.Unspecified, tb.PackingSize, "#1");

			TypeBuilder tb2 = module.DefineType (genTypeName (), 0, typeof (object),
				PackingSize.Size16, 16);
			Assert.AreEqual (PackingSize.Size16, tb2.PackingSize, "#2");
		}

		[Test]
		public void TestReflectedType ()
		{
			// It is the same as DeclaringType, but why?
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsNull (tb.ReflectedType, "#1");

			TypeBuilder tb2 = tb.DefineNestedType (genTypeName ());
			Assert.AreEqual (tb, tb2.ReflectedType, "#2");
		}

		[Test]
		public void SetParent_Parent_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Class,
				typeof (Attribute));
			tb.SetParent (null);
			Assert.AreEqual (typeof (object), tb.BaseType, "#A1");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract);
			tb.SetParent (null);
			Assert.IsNull (tb.BaseType, "#B1");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract, typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType, "#C1");
			tb.SetParent (null);
			Assert.IsNull (tb.BaseType, "#C2");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface,
				typeof (IDisposable));
			try {
				tb.SetParent (null);
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Interface must be declared abstract
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}
		}

		[Test]
		public void SetParent_Parent_Interface ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Class);
			tb.SetParent (typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType);
		}

		[Test]
		public void TestSetParentIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.SetParent (typeof (Attribute));
			Assert.AreEqual (typeof (Attribute), tb.BaseType, "#1");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract);
			tb.SetParent (typeof (IDisposable));
			Assert.AreEqual (typeof (IDisposable), tb.BaseType, "#2");

			tb = module.DefineType (genTypeName ());
			tb.SetParent (typeof (IDisposable));
			Assert.AreEqual (typeof (IDisposable), tb.BaseType, "#3");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract, typeof (IDisposable));
			tb.SetParent (typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType, "#4");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract, typeof (IDisposable));
			tb.SetParent (typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType, "#5");
		}

		[Test]
		public void TestSetParentComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			try {
				tb.SetParent (typeof (Attribute));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestSize ()
		{
			{
				TypeBuilder tb = module.DefineType (genTypeName ());
				Assert.AreEqual (0, tb.Size, "#1");
				tb.CreateType ();
				Assert.AreEqual (0, tb.Size, "#2");
			}

			{
				TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (object),
					PackingSize.Size16, 32);
				Assert.AreEqual (32, tb.Size, "#3");
			}
		}

		[Test]
		public void TestTypeHandle ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				RuntimeTypeHandle handle = tb.TypeHandle;
				Assert.Fail ("#1:" + handle);
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestTypeInitializerIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				ConstructorInfo cb = tb.TypeInitializer;
				Assert.Fail ("#1:" + (cb != null));
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestTypeInitializerComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			ConstructorInfo cb = tb.TypeInitializer;
		}

		[Test]
		public void TestTypeToken ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			TypeToken token = tb.TypeToken;
		}

		[Test]
		public void UnderlyingSystemType ()
		{
			TypeBuilder tb;
			Type emitted_type;

			tb = module.DefineType (genTypeName ());
			Assert.AreSame (tb, tb.UnderlyingSystemType, "#A1");
			emitted_type = tb.CreateType ();
			Assert.AreSame (emitted_type, tb.UnderlyingSystemType, "#A2");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			Assert.AreSame (tb, tb.UnderlyingSystemType, "#B1");
			emitted_type = tb.CreateType ();
			Assert.AreSame (emitted_type, tb.UnderlyingSystemType, "#B2");

			tb = module.DefineType (genTypeName (), 0, typeof (ValueType));
			Assert.AreSame (tb, tb.UnderlyingSystemType, "#C1");
			emitted_type = tb.CreateType ();
			Assert.AreSame (emitted_type, tb.UnderlyingSystemType, "#C2");

			tb = module.DefineType (genTypeName (), 0, typeof (Enum));
			try {
				Type t = tb.UnderlyingSystemType;
				Assert.Fail ("#D1:" + t);
			} catch (InvalidOperationException ex) {
				// Underlying type information on enumeration
				// is not specified
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}
			tb.DefineField ("val", typeof (int), FieldAttributes.Private);
			Assert.AreEqual (typeof (int), tb.UnderlyingSystemType, "#D5");
			emitted_type = tb.CreateType ();
			Assert.AreSame (emitted_type, tb.UnderlyingSystemType, "#D6");

			tb = module.DefineType (genTypeName (), 0, typeof (Enum));
			tb.DefineField ("val", typeof (int), FieldAttributes.Static);
			try {
				Type t = tb.UnderlyingSystemType;
				Assert.Fail ("#E1:" + t);
			} catch (InvalidOperationException ex) {
				// Underlying type information on enumeration
				// is not specified
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}
			tb.DefineField ("foo", typeof (long), FieldAttributes.Private);
			Assert.AreEqual (typeof (long), tb.UnderlyingSystemType, "#E5");
			tb.DefineField ("bar", typeof (short), FieldAttributes.Private);
			Assert.AreEqual (typeof (long), tb.UnderlyingSystemType, "#E6");
			tb.DefineField ("boo", typeof (int), FieldAttributes.Static);
			Assert.AreEqual (typeof (long), tb.UnderlyingSystemType, "#E7");
		}

		[Test]
		public void AddInterfaceImplementation_InterfaceType_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.AddInterfaceImplementation (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("interfaceType", ex.ParamName, "#5");
			}
		}

		[Test]
		public void TestAddInterfaceImplementation ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.AddInterfaceImplementation (typeof (AnInterface));
			tb.AddInterfaceImplementation (typeof (AnInterface));

			Type t = tb.CreateType ();
			Assert.AreEqual (1, tb.GetInterfaces ().Length, "#2");

			// Can not be called on a created type
			try {
				tb.AddInterfaceImplementation (typeof (AnInterface));
				Assert.Fail ("#3");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestCreateType_Created ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsFalse (tb.IsCreated (), "#A1");

			Type emittedType1 = tb.CreateType ();
			Assert.IsTrue (tb.IsCreated (), "#A2");
			Assert.IsNotNull (emittedType1, "#A3");

			Type emittedType2 = tb.CreateType ();
			Assert.IsNotNull (emittedType2, "#B1");
			Assert.IsTrue (tb.IsCreated (), "#B2");
			Assert.AreSame (emittedType1, emittedType2, "#B3");
		}

		[Test]
		public void TestDefineConstructor ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			ConstructorBuilder cb = tb.DefineConstructor (0, 0, null);
			cb.GetILGenerator ().Emit (OpCodes.Ret);
			tb.CreateType ();

			// Can not be called on a created type
			try {
				tb.DefineConstructor (0, 0, null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineDefaultConstructor ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineDefaultConstructor (0);
			tb.CreateType ();

			// Can not be called on a created type, altough the MSDN docs does not mention this
			try {
				tb.DefineDefaultConstructor (0);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineDefaultConstructor_Parent_DefaultCtorInaccessible ()
		{
			TypeBuilder tb;
			
			tb = module.DefineType (genTypeName ());
			tb.DefineDefaultConstructor (MethodAttributes.Private);
			Type parent_type = tb.CreateType ();

			tb = module.DefineType (genTypeName (), TypeAttributes.Class,
				parent_type);
			tb.DefineDefaultConstructor (MethodAttributes.Public);
			Type emitted_type = tb.CreateType ();
			try {
				Activator.CreateInstance (emitted_type);
				Assert.Fail ("#1");
			} catch (TargetInvocationException ex) {
				Assert.AreEqual (typeof (TargetInvocationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");

				MethodAccessException mae = ex.InnerException as MethodAccessException;
				Assert.IsNotNull (mae, "#5");
				Assert.AreEqual (typeof (MethodAccessException), mae.GetType (), "#6");
				Assert.IsNull (mae.InnerException, "#7");
				Assert.IsNotNull (mae.Message, "#8");
				Assert.IsTrue (mae.Message.IndexOf (parent_type.FullName) != -1, "#9:" + mae.Message);
				Assert.IsTrue (mae.Message.IndexOf (".ctor") != -1, "#10:" + mae.Message);
			}
		}

		[Test]
		public void DefineDefaultConstructor_Parent_DefaultCtorMissing ()
		{
			TypeBuilder tb;

			tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				new Type [] { typeof (string) });
			cb.GetILGenerator ().Emit (OpCodes.Ret);
			Type parent_type = tb.CreateType ();

			tb = module.DefineType (genTypeName (), TypeAttributes.Class,
				parent_type);
			try {
				tb.DefineDefaultConstructor (MethodAttributes.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Parent does not have a default constructor.
				// The default constructor must be explicitly defined
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineEvent_Name_NullChar ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineEvent ("\0test", EventAttributes.None,
					typeof (int));
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			EventBuilder eb = tb.DefineEvent ("te\0st", EventAttributes.None,
				typeof (int));
			Assert.IsNotNull (eb, "#B1");
		}

		[Test]
		public void TestDefineEvent ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Test invalid arguments
			try {
				tb.DefineEvent (null, 0, typeof (int));
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			try {
				tb.DefineEvent ("FOO", 0, null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("type", ex.ParamName, "#B5");
			}

			try {
				tb.DefineEvent (string.Empty, 0, typeof (int));
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("name", ex.ParamName, "#C5");
			}

			tb.CreateType ();

			// Can not be called on a created type
			try {
				tb.DefineEvent ("BAR", 0, typeof (int));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}
		}

		[Test] // DefineField (String, Type, FieldAttributes)
		public void DefineField1 ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineField (null, typeof (int), 0);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#A5");
			}

			try {
				tb.DefineField (string.Empty, typeof (int), 0);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#B5");
			}

			try {
				// Strangely, 'A<NULL>' is accepted...
				string name = String.Format ("{0}", (char) 0);
				tb.DefineField (name, typeof (int), 0);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#C5");
			}

			try {
				tb.DefineField ("A", typeof (void), 0);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Bad field type in defining field
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
			}

			tb.CreateType ();

			// Can not be called on a created type
			try {
				tb.DefineField ("B", typeof (int), 0);
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
			}
		}

		[Test] // DefineField (String, Type, FieldAttributes)
		public void DefineField1_Name_NullChar ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineField ("\0test", typeof (int),
					FieldAttributes.Private);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#A5");
			}

			FieldBuilder fb = tb.DefineField ("te\0st", typeof (int),
				FieldAttributes.Private);
			Assert.IsNotNull (fb, "#B1");
			Assert.AreEqual ("te\0st", fb.Name, "#B2");
		}

		[Test] // DefineField (String, Type, FieldAttributes)
		public void DefineField1_Type_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineField ("test", (Type) null,
					FieldAttributes.Private);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("type", ex.ParamName, "#5");
			}
		}

		[Test] // DefineField (String, Type, Type [], Type [], FieldAttributes)
		public void DefineField2_Type_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineField ("test", (Type) null, Type.EmptyTypes,
					Type.EmptyTypes, FieldAttributes.Private);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("type", ex.ParamName, "#5");
			}
		}

		[Test]
		public void TestDefineInitializedData ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineInitializedData (null, new byte [1], 0);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			try {
				tb.DefineInitializedData ("FOO", null, 0);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("data", ex.ParamName, "#B5");
			}

			try {
				tb.DefineInitializedData (string.Empty, new byte [1], 0);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("name", ex.ParamName, "#C5");
			}

			// The size of the data is less than or equal to zero ???
			try {
				tb.DefineInitializedData ("BAR", new byte [0], 0);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Data size must be > 0 and < 0x3f0000
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
			}

			try {
				string name = String.Format ("{0}", (char) 0);
				tb.DefineInitializedData (name, new byte [1], 0);
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#E5");
			}

			tb.CreateType ();

			// Can not be called on a created type, altough the MSDN docs does not mention this
			try {
				tb.DefineInitializedData ("BAR2", new byte [1], 0);
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
			}
		}

		[Test]
		public void DefineUninitializedDataInvalidArgs ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineUninitializedData (null, 1, 0);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			try {
				tb.DefineUninitializedData (string.Empty, 1, 0);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("name", ex.ParamName, "#B5");
			}

			// The size of the data is less than or equal to zero ???
			try {
				tb.DefineUninitializedData ("BAR", 0, 0);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Data size must be > 0 and < 0x3f0000
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}

			try {
				string name = String.Format ("{0}", (char) 0);
				tb.DefineUninitializedData (name, 1, 0);
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.AreEqual ("fieldName", ex.ParamName, "#D5");
			}
		}

		[Test]
		public void DefineUninitializedDataAlreadyCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			try {
				tb.DefineUninitializedData ("BAR2", 1, 0);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineUninitializedData ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			tb.DefineUninitializedData ("foo", 4, FieldAttributes.Public);

			Type t = tb.CreateType ();

			object o = Activator.CreateInstance (t);

			FieldInfo fi = t.GetField ("foo");

			object fieldVal = fi.GetValue (o);

			IntPtr ptr = Marshal.AllocHGlobal (4);
			Marshal.StructureToPtr (fieldVal, ptr, true);
			Marshal.FreeHGlobal (ptr);
		}

		[Test]
		public void DefineMethod_Name_NullChar ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.DefineMethod ("\0test", MethodAttributes.Private,
					typeof (string), Type.EmptyTypes);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			MethodBuilder mb = tb.DefineMethod ("te\0st", MethodAttributes.Private,
				typeof (string), Type.EmptyTypes);
			Assert.IsNotNull (mb, "#B1");
			Assert.AreEqual ("te\0st", mb.Name, "#B2");
		}

		[Test]
		public void TestDefineMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineMethod (null, 0, null, null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			try {
				tb.DefineMethod (string.Empty, 0, null, null);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("name", ex.ParamName, "#B5");
			}

			// Check non-virtual methods on an interface
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			try {
				tb2.DefineMethod ("FOO", MethodAttributes.Abstract, null, null);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Interface method must be abstract and virtual
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}

			// Check static methods on an interface
			tb2.DefineMethod ("BAR", MethodAttributes.Public | MethodAttributes.Static,
							  typeof (void),
							  Type.EmptyTypes);

			tb.CreateType ();
			// Can not be called on a created type
			try {
				tb.DefineMethod ("bar", 0, null, null);
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}
		}

		[Test] // bug #327484
		[Category ("NotWorking")]
		public void TestDefineMethod_Abstract ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineMethod ("Run", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			try {
				tb.CreateType ();
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Type must be declared abstract if any of its
				// methods are abstract
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			tb = module.DefineType (genTypeName (), TypeAttributes.Abstract);
			tb.DefineMethod ("Run", MethodAttributes.Public |
				MethodAttributes.Abstract, typeof (void),
				Type.EmptyTypes);

			try {
				tb.CreateType ();
				Assert.Fail ("#B1");
			} catch (TypeLoadException ex) {
				// Non-virtual abstract method
				Assert.AreEqual (typeof (TypeLoadException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			tb = module.DefineType (genTypeName (), TypeAttributes.Abstract |
				TypeAttributes.Public);
			tb.DefineMethod ("Run", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);
			Type emittedType = tb.CreateType ();

			MethodInfo mi1 = emittedType.GetMethod ("Run");
			Assert.IsNotNull (mi1, "#C1");
			Assert.IsTrue (mi1.IsAbstract, "#C2");

			MethodInfo mi2 = tb.GetMethod ("Run");
			Assert.IsNotNull (mi2, "#D1");
			Assert.IsTrue (mi2.IsAbstract, "#D2");
		}

		// TODO: DefineMethodOverride

		[Test]
		public void TestDefineNestedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineNestedType (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("fullname", ex.ParamName, "#A5");
			}

			try {
				tb.DefineNestedType (string.Empty);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("fullname", ex.ParamName, "#B5");
			}

			try {
				tb.DefineNestedType (nullName ());
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("fullname", ex.ParamName, "#C5");
			}

			// If I fix the code so this works then mcs breaks -> how can mcs
			// works under MS .NET in the first place ???
			/*
			try {
				tb.DefineNestedType ("AA", TypeAttributes.Public, null, null);
				Fail ("Nested visibility must be specified.");
			}
			catch (ArgumentException) {
			}
			*/

			try {
				tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
									 new Type [1]);
				Assert.Fail ("#D1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.AreEqual ("interfaces", ex.ParamName, "#D5");
			}

			// I think this should reject non-interfaces, but it does not
			tb.DefineNestedType ("BB", TypeAttributes.NestedPublic, null,
								 new Type [1] { typeof (object) });

			// Normal invocation
			tb.DefineNestedType ("Nest");

			tb.CreateType ();

			// According to the MSDN docs, this cannnot be called after the type
			// is created, but it works.
			tb.DefineNestedType ("Nest2");

			// According to the MSDN docs, a Sealed class can't contain nested 
			// types, but this is not true
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Sealed);
			tb2.DefineNestedType ("AA");

			// According to the MSDN docs, interfaces can only contain interfaces,
			// but this is not true
			TypeBuilder tb3 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

			tb3.DefineNestedType ("AA");

			// Check shorter versions
			{
				TypeBuilder nested = tb.DefineNestedType ("N1");

				Assert.AreEqual ("N1", nested.Name, "#E1");
				Assert.AreEqual (typeof (object), nested.BaseType, "#E2");
				Assert.AreEqual (TypeAttributes.NestedPrivate, nested.Attributes, "#E3");
				Assert.AreEqual (0, nested.GetInterfaces ().Length, "#E4");
			}

			// TODO:
		}

		[Test]
		public void DefinePInvokeMethod_Name_NullChar ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.DefinePInvokeMethod ("\0test", "B", "C",
					MethodAttributes.Private, CallingConventions.Standard,
					typeof (string),Type.EmptyTypes, CallingConvention.Cdecl,
					CharSet.Unicode);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			MethodBuilder mb = tb.DefinePInvokeMethod ("te\0st", "B", "C",
				MethodAttributes.Private, CallingConventions.Standard,
				typeof (string), Type.EmptyTypes, CallingConvention.Cdecl,
				CharSet.Unicode);
			Assert.IsNotNull (mb, "#B1");
			Assert.AreEqual ("te\0st", mb.Name, "#B2");
		}

		[Test]
		public void TestDefinePInvokeMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			tb.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);

			// Try invalid parameters
			try {
				tb.DefinePInvokeMethod (null, "B", "C", 0, 0, null, null, 0, 0);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}
			// etc...

			// Try invalid attributes
			try {
				tb.DefinePInvokeMethod ("A2", "B", "C", MethodAttributes.Abstract, 0, null, null, 0, 0);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// PInvoke methods must be static and native and
				// cannot be abstract
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			// Try an interface parent
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

			try {
				tb2.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// PInvoke methods cannot exist on interfaces
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
		}

		[Test]
		public void DefineProperty_Name_NullChar ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineProperty ("\0test", 0, typeof (string), Type.EmptyTypes);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("name", ex.ParamName, "#A5");
			}

			PropertyBuilder pb = tb.DefineProperty ("te\0st", 0,
				typeof (string), Type.EmptyTypes); 
			Assert.IsNotNull (pb, "#B1");
			Assert.AreEqual ("te\0st", pb.Name, "#B2");
		}

		[Test]
		public void DefineProperty_ParameterTypes_ItemNull ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineProperty ("A", 0, typeof (string), new Type [1]);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineProperty_ReturnType_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineProperty ("A", 0, null, Type.EmptyTypes);
		}

		[Test]
		public void GetMethod_WorksWithTypeBuilderParameter () {
			TypeBuilder tb = module.DefineType (genTypeName ());
			var garg = tb.DefineGenericParameters ("T") [0];
			MethodBuilder mb = tb.DefineMethod ("create", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
		
			var mi = TypeBuilder.GetMethod (tb, mb);
			var decl = mi.DeclaringType;

			Assert.IsTrue (decl.IsGenericType, "#1");
			Assert.IsFalse (decl.IsGenericTypeDefinition, "#2");
			Assert.AreEqual (tb, decl.GetGenericTypeDefinition (), "#3");
			Assert.AreEqual (garg, decl.GetGenericArguments () [0], "#4");
		}

		[Test]
		public void GetConstructor_FailWithTypeBuilderParameter () {
			TypeBuilder tb = module.DefineType (genTypeName ());
			var garg = tb.DefineGenericParameters ("T") [0];
			var cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);

			try {
				TypeBuilder.GetConstructor (tb, cb);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual ("type", ex.ParamName, "#2");
			}
		}

		[Test]
		public void GetField_FailWithTypeBuilderParameter () {
			TypeBuilder tb = module.DefineType (genTypeName ());
			var garg = tb.DefineGenericParameters ("T") [0];
			var fb = tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);

			try {
				TypeBuilder.GetField (tb, fb);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual ("type", ex.ParamName, "#2");
			}
		}

		[Test]
		public void GetMethod_RejectMethodFromInflatedTypeBuilder () {
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters ("T");
			MethodBuilder mb = tb.DefineMethod ("create", MethodAttributes.Public, typeof (void), Type.EmptyTypes);

			Type ginst = tb.MakeGenericType (typeof (int));
			
			MethodInfo mi = TypeBuilder.GetMethod (ginst, mb);
			try {
				TypeBuilder.GetMethod (ginst, mi);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual ("method", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetMethod_WorkWithInstancesOfCreatedTypeBuilder () {
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters ("T");
			MethodBuilder mb = tb.DefineMethod ("create", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
			ILGenerator ig = mb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);
			
			tb.CreateType ();
			
			MethodInfo mi = TypeBuilder.GetMethod (tb.MakeGenericType (typeof (int)), mb);
			Assert.IsNotNull (mi);
		}

		[Test]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void GetMethod_AcceptMethodFromInflatedTypeBuilder_UnderCompilerContext () {
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = ASSEMBLY_NAME;

			assembly =
				Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave | (AssemblyBuilderAccess)0x800, Path.GetTempPath ());

			module = assembly.DefineDynamicModule ("module1");
			
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters ("T");
			MethodBuilder mb = tb.DefineMethod ("create", MethodAttributes.Public, typeof (void), Type.EmptyTypes);

			Type ginst = tb.MakeGenericType (typeof (int));
			
			MethodInfo mi = TypeBuilder.GetMethod (ginst, mb);

			try {
				TypeBuilder.GetMethod (ginst, mi);
			} catch (ArgumentException ex) {
				Assert.Fail ("#1");
			}
		}


		[Test]
		// Test that changes made to the method builder after a call to GetMethod ()
		// are visible
		public void TestGetMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

			ConstructorBuilder cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ig;
			ig = cb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);

			Type fooOfT = tb.MakeGenericType (typeParams [0]);

			// Create a method builder but do not emit IL yet
			MethodBuilder mb1 = tb.DefineMethod ("create", MethodAttributes.Public|MethodAttributes.Static, fooOfT, Type.EmptyTypes);

			Type t = tb.MakeGenericType (typeof (int));

			MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public|MethodAttributes.Static, t, Type.EmptyTypes);

			ig = mb.GetILGenerator ();
			ig.Emit (OpCodes.Call, TypeBuilder.GetMethod (t, mb1));
			ig.Emit (OpCodes.Ret);

			// Finish the method
			ig = mb1.GetILGenerator ();
			ig.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (fooOfT, cb));
			ig.Emit (OpCodes.Ret);

			Type t2 = tb.CreateType ();

			Assert.AreEqual (tb.Name + "[System.Int32]", t2.MakeGenericType (typeof (int)).GetMethod ("foo").Invoke (null, null).GetType ().ToString ());
		}

		[Test]
		public void TestGetConstructor ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

			ConstructorBuilder cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ig;

			Type t = tb.MakeGenericType (typeof (int));

			MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public|MethodAttributes.Static, t, Type.EmptyTypes);

			ig = mb.GetILGenerator ();

			ConstructorInfo ci = TypeBuilder.GetConstructor (t, cb);

			ig.Emit (OpCodes.Newobj, ci);
			ig.Emit (OpCodes.Ret);

			// Finish the ctorbuilder
			ig = cb.GetILGenerator ();
			ig.Emit (OpCodes.Ret);

			Type t2 = tb.CreateType ();

			Assert.AreEqual (tb.Name + "[System.Int32]", t2.MakeGenericType (typeof (int)).GetMethod ("foo").Invoke (null, null).GetType ().ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Static_GetConstructor_TypeNull ()
		{
			ConstructorInfo ci = typeof (object).GetConstructor (Type.EmptyTypes);
			// null is non-generic (from exception message)
			TypeBuilder.GetConstructor (null, ci);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Static_GetConstructor_TypeGeneric ()
		{
			Type t = typeof (List<>).MakeGenericType (typeof (int));
			ConstructorInfo ci = typeof (object).GetConstructor (Type.EmptyTypes);
			// type is not 'TypeBuilder' (from exception message)
			TypeBuilder.GetConstructor (t, ci);
		}

		[Test]
		public void Static_GetConstructor_TypeBuilderGeneric_ConstructorInfoNull ()
		{
			TypeBuilder tb = module.DefineType ("XXX");
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");
			Type fooOfT = tb.MakeGenericType (typeParams [0]);
			try {
				TypeBuilder.GetConstructor (fooOfT, null);
				Assert.Fail ("Expected NullReferenceException");
			}
			catch (NullReferenceException) {
			}
		}

		[Test] //#536243
		public void CreateTypeThrowsForMethodsWithBadLabels ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			MethodBuilder mb = tb.DefineMethod("F", MethodAttributes.Public, typeof(string), null);
			ILGenerator il_gen = mb.GetILGenerator ();
			il_gen.DefineLabel ();
			il_gen.Emit (OpCodes.Leave, new Label ());
			try {
				tb.CreateType ();
				Assert.Fail ();
			} catch (ArgumentException) {}
		}

		[Test]
		[Category ("NotWorking")]
		public void TestIsDefinedIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.IsDefined (typeof (int), true);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestIsDefinedComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			ConstructorInfo obsoleteCtor = typeof (ObsoleteAttribute).GetConstructor (
				new Type [] { typeof (string) });

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (obsoleteCtor,
				new object [] { "obsolete message" }, new FieldInfo [0], new object [0]);

			tb.SetCustomAttribute (caBuilder);
			tb.CreateType ();
			Assert.IsTrue (tb.IsDefined (typeof (ObsoleteAttribute), false));
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=293659
		public void IsDefined_AttributeType_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();

			try {
				tb.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("attributeType", ex.ParamName, "#5");
			}
		}

		[Test] // GetConstructor (Type [])
		public void GetConstructor1_Incomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			try {
				tb.GetConstructor (Type.EmptyTypes);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // GetConstructor (BindingFlags, Binder, Type [], ParameterModifier [])
		public void GetConstructor2_Complete ()
		{
			BindingFlags flags;
			ConstructorInfo ctor;

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (redType, "Red", true);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);
			ConstructorBuilder cb = greenType.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#A8a");
			Assert.IsTrue (ctor.IsPrivate, "#A8b");
			Assert.IsFalse (ctor.IsStatic, "#A8c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#A8d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#A8e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#A9a");
			Assert.IsTrue (ctor.IsFamily, "#A9b");
			Assert.IsFalse (ctor.IsStatic, "#A9c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#A9d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#A9e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#A10a");
			Assert.IsTrue (ctor.IsFamilyAndAssembly, "#A10b");
			Assert.IsFalse (ctor.IsStatic, "#A10c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#A10d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#A10e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#A11a");
			Assert.IsTrue (ctor.IsFamilyOrAssembly, "#A11b");
			Assert.IsFalse (ctor.IsStatic, "#A11c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#A11d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#A11e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#A13a");
			Assert.IsTrue (ctor.IsAssembly, "#A13b");
			Assert.IsFalse (ctor.IsStatic, "#A13c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#A13d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#A13e");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#A14");

			flags = BindingFlags.Instance | BindingFlags.Public;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#B7a");
			Assert.IsTrue (ctor.IsPublic, "#B7b");
			Assert.IsFalse (ctor.IsStatic, "#B7c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#B7d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#B7e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#B12a");
			Assert.IsTrue (ctor.IsPublic, "#B12b");
			Assert.IsFalse (ctor.IsStatic, "#B12c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#B12d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#B12e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#B14");

			flags = BindingFlags.Static | BindingFlags.Public;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C11a");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#C14");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#D13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#D14a");
			Assert.IsTrue (ctor.IsPrivate, "#D14b");
			Assert.IsTrue (ctor.IsStatic, "#B14c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#B14d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#B14e");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#E8a");
			Assert.IsTrue (ctor.IsPrivate, "#E8b");
			Assert.IsFalse (ctor.IsStatic, "#E8c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#E8d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#E8e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#E9a");
			Assert.IsTrue (ctor.IsFamily, "#E9b");
			Assert.IsFalse (ctor.IsStatic, "#E9c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#E9d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#E9e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#E10a");
			Assert.IsTrue (ctor.IsFamilyAndAssembly, "#E10b");
			Assert.IsFalse (ctor.IsStatic, "#E10c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#E10d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#E10e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#E11a");
			Assert.IsTrue (ctor.IsFamilyOrAssembly, "#E11b");
			Assert.IsFalse (ctor.IsStatic, "#E11c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#E11d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#E11e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#E13a");
			Assert.IsTrue (ctor.IsAssembly, "#E13b");
			Assert.IsFalse (ctor.IsStatic, "#E13c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#E13d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#E13e");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#E14");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#F7a");
			Assert.IsTrue (ctor.IsPublic, "#F7b");
			Assert.IsFalse (ctor.IsStatic, "#F7c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#F7d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#F7e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#F12a");
			Assert.IsTrue (ctor.IsPublic, "#F12b");
			Assert.IsFalse (ctor.IsStatic, "#F12c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#F12d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#F12e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#F14");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#G14");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#H13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#H14");
			Assert.IsTrue (ctor.IsPrivate, "#H14b");
			Assert.IsTrue (ctor.IsStatic, "#H14c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#H14d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#H14e");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#I8a");
			Assert.IsTrue (ctor.IsPrivate, "#I8b");
			Assert.IsFalse (ctor.IsStatic, "#I8c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#I8d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#I8e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#I9a");
			Assert.IsTrue (ctor.IsFamily, "#I9b");
			Assert.IsFalse (ctor.IsStatic, "#I9c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#I9d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#I9e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#I10a");
			Assert.IsTrue (ctor.IsFamilyAndAssembly, "#I10b");
			Assert.IsFalse (ctor.IsStatic, "#I10c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#I10d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#I10e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#I11a");
			Assert.IsTrue (ctor.IsFamilyOrAssembly, "#I11b");
			Assert.IsFalse (ctor.IsStatic, "#I11c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#I11d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#I11e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#I13a");
			Assert.IsTrue (ctor.IsAssembly, "#I13b");
			Assert.IsFalse (ctor.IsStatic, "#I13c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#I13d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#I13e");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#I14");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#J7a");
			Assert.IsTrue (ctor.IsPublic, "#J7b");
			Assert.IsFalse (ctor.IsStatic, "#J7c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#J7d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#J7e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#J12a");
			Assert.IsTrue (ctor.IsPublic, "#J12b");
			Assert.IsFalse (ctor.IsStatic, "#J12c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#J12d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#J12e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#J14");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#K14");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#L13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#L14a");
			Assert.IsTrue (ctor.IsPrivate, "#L14b");
			Assert.IsTrue (ctor.IsStatic, "#L14c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#L14d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#L14e");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M7a");
			Assert.IsTrue (ctor.IsPublic, "#M7b");
			Assert.IsFalse (ctor.IsStatic, "#M7c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#M7d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M7e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M8a");
			Assert.IsTrue (ctor.IsPrivate, "#M8b");
			Assert.IsFalse (ctor.IsStatic, "#M8c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#M8d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M8e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M9a");
			Assert.IsTrue (ctor.IsFamily, "#M9b");
			Assert.IsFalse (ctor.IsStatic, "#M9c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#M9d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M9e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M10a");
			Assert.IsTrue (ctor.IsFamilyAndAssembly, "#M10b");
			Assert.IsFalse (ctor.IsStatic, "#M10c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#M10d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M10e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M11a");
			Assert.IsTrue (ctor.IsFamilyOrAssembly, "#M11b");
			Assert.IsFalse (ctor.IsStatic, "#M11c");
			Assert.AreEqual (1, ctor.GetParameters ().Length, "#M11d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M11e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M12a");
			Assert.IsTrue (ctor.IsPublic, "#M12b");
			Assert.IsFalse (ctor.IsStatic, "#M12c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#M12d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M12e");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#M13a");
			Assert.IsTrue (ctor.IsAssembly, "#M13b");
			Assert.IsFalse (ctor.IsStatic, "#M13c");
			Assert.AreEqual (2, ctor.GetParameters ().Length, "#M13d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#M13e");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#M14");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N1");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N2");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N3");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N4");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N5");

			ctor = greenType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N6");

			ctor = greenType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N7");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N8");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N9");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (string) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N10");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N11");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (int), typeof (bool) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N12");

			ctor = redType.GetConstructor (flags, null,
				new Type [] { typeof (string), typeof (int) },
				new ParameterModifier [0]);
			Assert.IsNull (ctor, "#N13");

			ctor = redType.GetConstructor (flags, null,
				Type.EmptyTypes,
				new ParameterModifier [0]);
			Assert.IsNotNull (ctor, "#N14a");
			Assert.IsTrue (ctor.IsPrivate, "#N14b");
			Assert.IsTrue (ctor.IsStatic, "#N14c");
			Assert.AreEqual (0, ctor.GetParameters ().Length, "#N14d");
			Assert.IsFalse (ctor is ConstructorBuilder, "#N14e");
		}

		[Test] // GetConstructor (BindingFlags, Binder, Type [], ParameterModifier [])
		public void GetConstructor2_Incomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			try {
				tb.GetConstructor (BindingFlags.Public | BindingFlags.Instance,
					null, Type.EmptyTypes, new ParameterModifier [0]);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // GetConstructor (BindingFlags, Binder, CallingConventions, Type [], ParameterModifier [])
		public void GetConstructor3_Incomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			try {
				tb.GetConstructor (BindingFlags.Public | BindingFlags.Instance,
					null, CallingConventions.Standard, Type.EmptyTypes,
					new ParameterModifier [0]);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // GetConstructors ()
		[Category ("NotWorking")] // mcs depends on this
		public void GetConstructors1_Incomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			try {
				tb.GetConstructors ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // GetConstructors (BindingFlags)
		public void GetConstructors2_Complete ()
		{
			BindingFlags flags;
			ConstructorInfo [] ctors;

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (redType, "Red", true);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);
			ConstructorBuilder cb = greenType.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#A1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (5, ctors.Length, "#A2");
			Assert.IsTrue (ctors [0].IsPrivate, "#A3a");
			Assert.IsFalse (ctors [0].IsStatic, "#A3b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#A3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#A3d");
			Assert.IsTrue (ctors [1].IsFamily, "#A4a");
			Assert.IsFalse (ctors [1].IsStatic, "#A4b");
			Assert.AreEqual (1, ctors [1].GetParameters ().Length, "#A4c");
			Assert.IsFalse (ctors [1] is ConstructorBuilder, "#A4d");
			Assert.IsTrue (ctors [2].IsFamilyAndAssembly, "#A5a");
			Assert.IsFalse (ctors [2].IsStatic, "#A5b");
			Assert.AreEqual (2, ctors [2].GetParameters ().Length, "#A5c");
			Assert.IsFalse (ctors [2] is ConstructorBuilder, "#A5d");
			Assert.IsTrue (ctors [3].IsFamilyOrAssembly, "#A6a");
			Assert.IsFalse (ctors [3].IsStatic, "#A6b");
			Assert.AreEqual (1, ctors [3].GetParameters ().Length, "#A6c");
			Assert.IsFalse (ctors [3] is ConstructorBuilder, "#A6d");
			Assert.IsTrue (ctors [4].IsAssembly, "#A7a");
			Assert.IsFalse (ctors [4].IsStatic, "#A7b");
			Assert.AreEqual (2, ctors [4].GetParameters ().Length, "#A7c");
			Assert.IsFalse (ctors [4] is ConstructorBuilder, "#A7d");

			flags = BindingFlags.Instance | BindingFlags.Public;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#B1");
			Assert.IsTrue (ctors [0].IsPublic, "#B2a");
			Assert.IsFalse (ctors [0].IsStatic, "#B2b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#B2c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#B2d");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#B3");
			Assert.IsTrue (ctors [0].IsPublic, "#B4a");
			Assert.IsFalse (ctors [0].IsStatic, "#B4b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#B4c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#B4d");

			flags = BindingFlags.Static | BindingFlags.Public;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#C1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#C2");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#D1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#D2");
			Assert.IsTrue (ctors [0].IsPrivate, "#D3a");
			Assert.IsTrue (ctors [0].IsStatic, "#D3b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#D3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#D3d");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#E1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (5, ctors.Length, "#E2");
			Assert.IsTrue (ctors [0].IsPrivate, "#E3a");
			Assert.IsFalse (ctors [0].IsStatic, "#E3b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#E3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#E3d");
			Assert.IsTrue (ctors [1].IsFamily, "#E4a");
			Assert.IsFalse (ctors [1].IsStatic, "#E4b");
			Assert.AreEqual (1, ctors [1].GetParameters ().Length, "#E4c");
			Assert.IsFalse (ctors [1] is ConstructorBuilder, "#E4d");
			Assert.IsTrue (ctors [2].IsFamilyAndAssembly, "#E5a");
			Assert.IsFalse (ctors [2].IsStatic, "#A5b");
			Assert.AreEqual (2, ctors [2].GetParameters ().Length, "#E5c");
			Assert.IsFalse (ctors [2] is ConstructorBuilder, "#E5d");
			Assert.IsTrue (ctors [3].IsFamilyOrAssembly, "#E6a");
			Assert.IsFalse (ctors [3].IsStatic, "#E6b");
			Assert.AreEqual (1, ctors [3].GetParameters ().Length, "#E6c");
			Assert.IsFalse (ctors [3] is ConstructorBuilder, "#E6d");
			Assert.IsTrue (ctors [4].IsAssembly, "#E7a");
			Assert.IsFalse (ctors [4].IsStatic, "#E7b");
			Assert.AreEqual (2, ctors [4].GetParameters ().Length, "#E7c");
			Assert.IsFalse (ctors [4] is ConstructorBuilder, "#E7d");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#F1");
			Assert.IsTrue (ctors [0].IsPublic, "#F2a");
			Assert.IsFalse (ctors [0].IsStatic, "#F2b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#F2c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#F2d");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#F3");
			Assert.IsTrue (ctors [0].IsPublic, "#F4a");
			Assert.IsFalse (ctors [0].IsStatic, "#F4b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#F4c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#F4d");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#G1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#G2");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#H1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#H2");
			Assert.IsTrue (ctors [0].IsPrivate, "#H3a");
			Assert.IsTrue (ctors [0].IsStatic, "#H3b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#H3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#H3d");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#I1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (5, ctors.Length, "#I2");
			Assert.IsTrue (ctors [0].IsPrivate, "#I3a");
			Assert.IsFalse (ctors [0].IsStatic, "#I3b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#I3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#I3d");
			Assert.IsTrue (ctors [1].IsFamily, "#I4a");
			Assert.IsFalse (ctors [1].IsStatic, "#I4b");
			Assert.AreEqual (1, ctors [1].GetParameters ().Length, "#I4c");
			Assert.IsFalse (ctors [1] is ConstructorBuilder, "#I4d");
			Assert.IsTrue (ctors [2].IsFamilyAndAssembly, "#I5a");
			Assert.IsFalse (ctors [2].IsStatic, "#I5b");
			Assert.AreEqual (2, ctors [2].GetParameters ().Length, "#I5c");
			Assert.IsFalse (ctors [2] is ConstructorBuilder, "#I5d");
			Assert.IsTrue (ctors [3].IsFamilyOrAssembly, "#I6a");
			Assert.IsFalse (ctors [3].IsStatic, "#I6b");
			Assert.AreEqual (1, ctors [3].GetParameters ().Length, "#I6c");
			Assert.IsFalse (ctors [3] is ConstructorBuilder, "#I6d");
			Assert.IsTrue (ctors [4].IsAssembly, "#I7a");
			Assert.IsFalse (ctors [4].IsStatic, "#I7b");
			Assert.AreEqual (2, ctors [4].GetParameters ().Length, "#I7c");
			Assert.IsFalse (ctors [4] is ConstructorBuilder, "#I7d");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#J1");
			Assert.IsTrue (ctors [0].IsPublic, "#J2a");
			Assert.IsFalse (ctors [0].IsStatic, "#J2b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#J2c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#J2d");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#J3");
			Assert.IsTrue (ctors [0].IsPublic, "#J4a");
			Assert.IsFalse (ctors [0].IsStatic, "#J4b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#J4c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#J4d");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#K1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#K2");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#L1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#L2");
			Assert.IsTrue (ctors [0].IsPrivate, "#L3a");
			Assert.IsTrue (ctors [0].IsStatic, "#L3b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#L3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#L3d");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#M1");
			Assert.IsTrue (ctors [0].IsPublic, "#M2a");
			Assert.IsFalse (ctors [0].IsStatic, "#M2b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#M2c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#M2d");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (6, ctors.Length, "#M3");
			Assert.IsTrue (ctors [0].IsPrivate, "#M4a");
			Assert.IsFalse (ctors [0].IsStatic, "#M4b");
			Assert.AreEqual (2, ctors [0].GetParameters ().Length, "#M4c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#M4d");
			Assert.IsTrue (ctors [1].IsFamily, "#M5a");
			Assert.IsFalse (ctors [1].IsStatic, "#M5b");
			Assert.AreEqual (1, ctors [1].GetParameters ().Length, "#M5c");
			Assert.IsFalse (ctors [1] is ConstructorBuilder, "#M5d");
			Assert.IsTrue (ctors [2].IsFamilyAndAssembly, "#M6a");
			Assert.IsFalse (ctors [2].IsStatic, "#M6b");
			Assert.AreEqual (2, ctors [2].GetParameters ().Length, "#M6c");
			Assert.IsFalse (ctors [2] is ConstructorBuilder, "#M6d");
			Assert.IsTrue (ctors [3].IsFamilyOrAssembly, "#M7a");
			Assert.IsFalse (ctors [3].IsStatic, "#M7b");
			Assert.AreEqual (1, ctors [3].GetParameters ().Length, "#M7c");
			Assert.IsFalse (ctors [3] is ConstructorBuilder, "#M7d");
			Assert.IsTrue (ctors [4].IsPublic, "#M8a");
			Assert.IsFalse (ctors [4].IsStatic, "#M8b");
			Assert.AreEqual (2, ctors [4].GetParameters ().Length, "#M8c");
			Assert.IsFalse (ctors [4] is ConstructorBuilder, "#M8d");
			Assert.IsTrue (ctors [5].IsAssembly, "#M9a");
			Assert.IsFalse (ctors [5].IsStatic, "#M9b");
			Assert.AreEqual (2, ctors [5].GetParameters ().Length, "#M9c");
			Assert.IsFalse (ctors [5] is ConstructorBuilder, "#M9d");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			ctors = greenType.GetConstructors (flags);
			Assert.AreEqual (0, ctors.Length, "#N1");

			ctors = redType.GetConstructors (flags);
			Assert.AreEqual (1, ctors.Length, "#N2");
			Assert.IsTrue (ctors [0].IsPrivate, "#N3a");
			Assert.IsTrue (ctors [0].IsStatic, "#N3b");
			Assert.AreEqual (0, ctors [0].GetParameters ().Length, "#N3c");
			Assert.IsFalse (ctors [0] is ConstructorBuilder, "#N3d");
		}

		[Test] // GetConstructors (BindingFlags)
		[Category ("NotWorking")] // mcs depends on this
		public void GetConstructors2_Incomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				Type.EmptyTypes);
			cb.GetILGenerator ().Emit (OpCodes.Ret);

			try {
				tb.GetConstructors (BindingFlags.Public |
					BindingFlags.Instance);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetCustomAttributesIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetCustomAttributes (false);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetCustomAttributesComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			ConstructorInfo guidCtor = typeof (GuidAttribute).GetConstructor (
				new Type [] { typeof (string) });

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
				new object [] { Guid.NewGuid ().ToString ("D") }, new FieldInfo [0], new object [0]);

			tb.SetCustomAttribute (caBuilder);
			tb.CreateType ();

			Assert.AreEqual (1, tb.GetCustomAttributes (false).Length);
		}

		[Test]
		public void TestGetCustomAttributesOfTypeIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetCustomAttributes (typeof (ObsoleteAttribute), false);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetCustomAttributesOfTypeComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			ConstructorInfo guidCtor = typeof (GuidAttribute).GetConstructor (
				new Type [] { typeof (string) });

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
				new object [] { Guid.NewGuid ().ToString ("D") }, new FieldInfo [0], new object [0]);

			tb.SetCustomAttribute (caBuilder);
			tb.CreateType ();

			Assert.AreEqual (1, tb.GetCustomAttributes (typeof (GuidAttribute), false).Length, "#1");
			Assert.AreEqual (0, tb.GetCustomAttributes (typeof (ObsoleteAttribute), false).Length, "#2");
		}

		[Test]
		public void TestGetCustomAttributesOfNullTypeComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			try {
				tb.GetCustomAttributes (null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("attributeType", ex.ParamName, "#5");
			}
		}

		[Test]
		[Ignore ("mcs depends on this")]
		public void TestGetEventsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetEvents ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				throw;
			}
		}

		[Test]
		public void TestGetEventsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
				typeof (void), new Type [] { typeof (Object) });
			onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

			// create public event
			EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
				typeof (ResolveEventHandler));
			eventbuilder.SetRaiseMethod (onclickMethod);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetEvents ().Length, "#1");
			Assert.AreEqual (tb.GetEvents ().Length, emittedType.GetEvents ().Length, "#2");
		}


		[Test]
		[Ignore ("mcs depends on this")]
		public void TestGetEventsFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetEvents (BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				throw;
			}
		}

		[Test]
		public void TestGetEventsFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			MethodBuilder onchangeMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
				typeof (void), new Type [] { typeof (Object) });
			onchangeMethod.GetILGenerator ().Emit (OpCodes.Ret);

			// create public event
			EventBuilder changeEvent = tb.DefineEvent ("Change", EventAttributes.None,
				typeof (ResolveEventHandler));
			changeEvent.SetRaiseMethod (onchangeMethod);

			// create non-public event
			EventBuilder redoChangeEvent = tb.DefineEvent ("RedoChange", EventAttributes.None,
				typeof (ResolveEventHandler));

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (1, tb.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length);
			Assert.AreEqual (2, tb.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length);
			Assert.AreEqual (tb.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetEvents (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (tb.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetEvents (BindingFlags.Instance | BindingFlags.NonPublic).Length);
			Assert.AreEqual (tb.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length,
				emittedType.GetEvents (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length);
		}

		[Test]
		public void TestGetEventsFlagsComplete_Inheritance ()
		{
			EventInfo [] events;
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (13, events.Length, "#A1");
			Assert.AreEqual ("OnPrivateInstanceGreen", events [0].Name, "#A2");
			Assert.AreEqual ("OnFamilyInstanceGreen", events [1].Name, "#A3");
			Assert.AreEqual ("OnFamANDAssemInstanceGreen", events [2].Name, "#A4");
			Assert.AreEqual ("OnFamORAssemInstanceGreen", events [3].Name, "#A5");
			Assert.AreEqual ("OnAssemblyInstanceGreen", events [4].Name, "#A6");
			Assert.AreEqual ("OnFamilyInstanceRed", events [5].Name, "#A7");
			Assert.AreEqual ("OnFamANDAssemInstanceRed", events [6].Name, "#A8");
			Assert.AreEqual ("OnFamORAssemInstanceRed", events [7].Name, "#A9");
			Assert.AreEqual ("OnAssemblyInstanceRed", events [8].Name, "#A10");
			Assert.AreEqual ("OnFamilyInstanceBlue", events [9].Name, "#A11");
			Assert.AreEqual ("OnFamANDAssemInstanceBlue", events [10].Name, "#A12");
			Assert.AreEqual ("OnFamORAssemInstanceBlue", events [11].Name, "#A13");
			Assert.AreEqual ("OnAssemblyInstanceBlue", events [12].Name, "#A14");

			flags = BindingFlags.Instance | BindingFlags.Public;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (3, events.Length, "#B1");
			Assert.AreEqual ("OnPublicInstanceGreen", events [0].Name, "#B2");
			Assert.AreEqual ("OnPublicInstanceRed", events [1].Name, "#B3");
			Assert.AreEqual ("OnPublicInstanceBlue", events [2].Name, "#B4");

			flags = BindingFlags.Static | BindingFlags.Public;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (1, events.Length, "#C1");
			Assert.AreEqual ("OnPublicStaticGreen", events [0].Name, "#C2");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (5, events.Length, "#D1");
			Assert.AreEqual ("OnPrivateStaticGreen", events [0].Name, "#D2");
			Assert.AreEqual ("OnFamilyStaticGreen", events [1].Name, "#D3");
			Assert.AreEqual ("OnFamANDAssemStaticGreen", events [2].Name, "#D4");
			Assert.AreEqual ("OnFamORAssemStaticGreen", events [3].Name, "#D5");
			Assert.AreEqual ("OnAssemblyStaticGreen", events [4].Name, "#D6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (13, events.Length, "#E1");
			Assert.AreEqual ("OnPrivateInstanceGreen", events [0].Name, "#E2");
			Assert.AreEqual ("OnFamilyInstanceGreen", events [1].Name, "#E3");
			Assert.AreEqual ("OnFamANDAssemInstanceGreen", events [2].Name, "#E4");
			Assert.AreEqual ("OnFamORAssemInstanceGreen", events [3].Name, "#E5");
			Assert.AreEqual ("OnAssemblyInstanceGreen", events [4].Name, "#E6");
			Assert.AreEqual ("OnFamilyInstanceRed", events [5].Name, "#E7");
			Assert.AreEqual ("OnFamANDAssemInstanceRed", events [6].Name, "#E8");
			Assert.AreEqual ("OnFamORAssemInstanceRed", events [7].Name, "#E9");
			Assert.AreEqual ("OnAssemblyInstanceRed", events [8].Name, "#E10");
			Assert.AreEqual ("OnFamilyInstanceBlue", events [9].Name, "#E11");
			Assert.AreEqual ("OnFamANDAssemInstanceBlue", events [10].Name, "#E12");
			Assert.AreEqual ("OnFamORAssemInstanceBlue", events [11].Name, "#E13");
			Assert.AreEqual ("OnAssemblyInstanceBlue", events [12].Name, "#E14");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (3, events.Length, "#F1");
			Assert.AreEqual ("OnPublicInstanceGreen", events [0].Name, "#F2");
			Assert.AreEqual ("OnPublicInstanceRed", events [1].Name, "#F3");
			Assert.AreEqual ("OnPublicInstanceBlue", events [2].Name, "#F4");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (3, events.Length, "#G1");
			Assert.AreEqual ("OnPublicStaticGreen", events [0].Name, "#G2");
			Assert.AreEqual ("OnPublicStaticRed", events [1].Name, "#G3");
			Assert.AreEqual ("OnPublicStaticBlue", events [2].Name, "#G4");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (13, events.Length, "#H1");
			Assert.AreEqual ("OnPrivateStaticGreen", events [0].Name, "#H2");
			Assert.AreEqual ("OnFamilyStaticGreen", events [1].Name, "#H3");
			Assert.AreEqual ("OnFamANDAssemStaticGreen", events [2].Name, "#H4");
			Assert.AreEqual ("OnFamORAssemStaticGreen", events [3].Name, "#H5");
			Assert.AreEqual ("OnAssemblyStaticGreen", events [4].Name, "#H6");
			Assert.AreEqual ("OnFamilyStaticRed", events [5].Name, "#H7");
			Assert.AreEqual ("OnFamANDAssemStaticRed", events [6].Name, "#H8");
			Assert.AreEqual ("OnFamORAssemStaticRed", events [7].Name, "#H9");
			Assert.AreEqual ("OnAssemblyStaticRed", events [8].Name, "#H10");
			Assert.AreEqual ("OnFamilyStaticBlue", events [9].Name, "#H11");
			Assert.AreEqual ("OnFamANDAssemStaticBlue", events [10].Name, "#H12");
			Assert.AreEqual ("OnFamORAssemStaticBlue", events [11].Name, "#H13");
			Assert.AreEqual ("OnAssemblyStaticBlue", events [12].Name, "#H14");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (5, events.Length, "#I1");
			Assert.AreEqual ("OnPrivateInstanceGreen", events [0].Name, "#I2");
			Assert.AreEqual ("OnFamilyInstanceGreen", events [1].Name, "#I3");
			Assert.AreEqual ("OnFamANDAssemInstanceGreen", events [2].Name, "#I4");
			Assert.AreEqual ("OnFamORAssemInstanceGreen", events [3].Name, "#I5");
			Assert.AreEqual ("OnAssemblyInstanceGreen", events [4].Name, "#I6");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (1, events.Length, "#J1");
			Assert.AreEqual ("OnPublicInstanceGreen", events [0].Name, "#J2");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (1, events.Length, "#K1");
			Assert.AreEqual ("OnPublicStaticGreen", events [0].Name, "#K2");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (5, events.Length, "#L1");
			Assert.AreEqual ("OnPrivateStaticGreen", events [0].Name, "#L2");
			Assert.AreEqual ("OnFamilyStaticGreen", events [1].Name, "#L3");
			Assert.AreEqual ("OnFamANDAssemStaticGreen", events [2].Name, "#L4");
			Assert.AreEqual ("OnFamORAssemStaticGreen", events [3].Name, "#L5");
			Assert.AreEqual ("OnAssemblyStaticGreen", events [4].Name, "#L6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (16, events.Length, "#M1");
			Assert.AreEqual ("OnPrivateInstanceGreen", events [0].Name, "#M2");
			Assert.AreEqual ("OnFamilyInstanceGreen", events [1].Name, "#M3");
			Assert.AreEqual ("OnFamANDAssemInstanceGreen", events [2].Name, "#M4");
			Assert.AreEqual ("OnFamORAssemInstanceGreen", events [3].Name, "#M5");
			Assert.AreEqual ("OnPublicInstanceGreen", events [4].Name, "#M6");
			Assert.AreEqual ("OnAssemblyInstanceGreen", events [5].Name, "#M7");
			Assert.AreEqual ("OnFamilyInstanceRed", events [6].Name, "#M8");
			Assert.AreEqual ("OnFamANDAssemInstanceRed", events [7].Name, "#M9");
			Assert.AreEqual ("OnFamORAssemInstanceRed", events [8].Name, "#M10");
			Assert.AreEqual ("OnPublicInstanceRed", events [9].Name, "#M11");
			Assert.AreEqual ("OnAssemblyInstanceRed", events [10].Name, "#M12");
			Assert.AreEqual ("OnFamilyInstanceBlue", events [11].Name, "#M13");
			Assert.AreEqual ("OnFamANDAssemInstanceBlue", events [12].Name, "#M14");
			Assert.AreEqual ("OnFamORAssemInstanceBlue", events [13].Name, "#M15");
			Assert.AreEqual ("OnPublicInstanceBlue", events [14].Name, "#M16");
			Assert.AreEqual ("OnAssemblyInstanceBlue", events [15].Name, "#M17");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			events = greenType.GetEvents (flags);

			Assert.AreEqual (6, events.Length, "#N1");
			Assert.AreEqual ("OnPrivateStaticGreen", events [0].Name, "#N2");
			Assert.AreEqual ("OnFamilyStaticGreen", events [1].Name, "#N3");
			Assert.AreEqual ("OnFamANDAssemStaticGreen", events [2].Name, "#N4");
			Assert.AreEqual ("OnFamORAssemStaticGreen", events [3].Name, "#N5");
			Assert.AreEqual ("OnPublicStaticGreen", events [4].Name, "#N6");
			Assert.AreEqual ("OnAssemblyStaticGreen", events [5].Name, "#N7");
		}

		[Test]
		[Ignore ("mcs depends on this")]
		public void TestGetEventIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetEvent ("FOO");
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				throw;
			}
		}

		[Test]
		public void TestGetEventComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
				typeof (void), new Type [] { typeof (Object) });
			onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

			EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
				typeof (ResolveEventHandler));
			eventbuilder.SetRaiseMethod (onclickMethod);

			Type emittedType = tb.CreateType ();

			Assert.IsNotNull (tb.GetEvent ("Change"));
			Assert.AreEqual (tb.GetEvent ("Change"), emittedType.GetEvent ("Change"));
			Assert.IsNull (tb.GetEvent ("NotChange"));
			Assert.AreEqual (tb.GetEvent ("NotChange"), emittedType.GetEvent ("NotChange"));
		}

		[Test]
		[Ignore ("mcs depends on this")]
		public void TestGetEventFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetEvent ("FOO", BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				throw;
			}
		}

		[Test]
		public void TestGetEventFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			MethodBuilder onclickMethod = tb.DefineMethod ("OnChange", MethodAttributes.Public,
				typeof (void), new Type [] { typeof (Object) });
			onclickMethod.GetILGenerator ().Emit (OpCodes.Ret);

			EventBuilder eventbuilder = tb.DefineEvent ("Change", EventAttributes.None,
				typeof (ResolveEventHandler));
			eventbuilder.SetRaiseMethod (onclickMethod);

			Type emittedType = tb.CreateType ();

			Assert.IsNotNull (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public));
			Assert.AreEqual (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public),
				emittedType.GetEvent ("Change", BindingFlags.Instance | BindingFlags.Public));
			Assert.IsNull (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic));
			Assert.AreEqual (tb.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic),
				emittedType.GetEvent ("Change", BindingFlags.Instance | BindingFlags.NonPublic));
		}

		[Test]
		public void TestGetEventFlagsComplete_Inheritance ()
		{
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#A1");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#A2");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#A3");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#A4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#A5");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#A6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#A7");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#A8");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#A9");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#A10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#A11");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#A12");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#A13");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#A14");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#A15");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#A16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#A17");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#A18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#A19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#A20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#A21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#A22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#A23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#A24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#A25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#A26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#A27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#A28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#A29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#A30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#A31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#A32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#A33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#A34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#A35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#A36");

			flags = BindingFlags.Instance | BindingFlags.Public;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#B1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#B2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#B3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#B4");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#B5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#B6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#B7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#B8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#B9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#B10");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#B11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#B12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#B13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#B14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#B15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#B16");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#B17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#B18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#B19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#B20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#B21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#B22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#B23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#B24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#B25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#B26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#B27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#B28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#B29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#B30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#B31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#B32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#B33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#B34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#B35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#B36");

			flags = BindingFlags.Static | BindingFlags.Public;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#C1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#C2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#C3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#C4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#C5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#C6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#C7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#C8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#C9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#C10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#C11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#C12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#C13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#C14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#C15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#C16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#C17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#C18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#C19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#C20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#C21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#C22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#C23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#C24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#C25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#C26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#C27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#C28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#C29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#C30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#C31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#C32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#C33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#C34");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#C35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#C36");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#D1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#D2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#D3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#D4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#D5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#D6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#D7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#D8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#D9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#D10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#D11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#D12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#D13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#D14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#D15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#D16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#D17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#D18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#D19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#D20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#D21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#D22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#D23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#D24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#D25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#D26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#D27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#D28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#D29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#D30");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#D31");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#D32");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#D33");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#D34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#D35");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#D36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#E1");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#E2");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#E3");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#E4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#E5");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#E6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#E7");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#E8");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#E9");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#E10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#E11");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#E12");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#E13");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#E14");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#E15");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#E16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#E17");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#E18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#E19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#E20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#E21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#E22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#E23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#E24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#E25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#E26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#E27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#E28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#E29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#E30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#E31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#E32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#E33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#E34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#E35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#E36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#F1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#F2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#F3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#F4");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#F5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#F6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#F7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#F8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#F9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#F10");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#F11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#F12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#F13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#F14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#F15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#F16");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#F17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#F18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#F19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#F20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#F21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#F22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#F23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#F24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#F25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#F26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#F27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#F28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#F29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#F30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#F31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#F32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#F33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#F34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#F35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#F36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#G1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#G2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#G3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#G4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#G5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#G6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#G7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#G8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#G9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#G10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#G11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#G12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#G13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#G14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#G15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#G16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#G17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#G18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#G19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#G20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#G21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#G22");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#G23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#G24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#G25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#G26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#G27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#G28");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#G29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#G30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#G31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#G32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#G33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#G34");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#G35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#G36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#H1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#H2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#H3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#H4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#H5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#H6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#H7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#H8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#H9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#H10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#H11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#H12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#H13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#H14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#H15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#H16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#H17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#H18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#H19");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#H20");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#H21");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#H22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#H23");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#H24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#H25");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#H26");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#H27");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#H28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#H29");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#H30");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#H31");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#H32");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#H33");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#H34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#H35");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#H36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#I1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#I2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#I3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#I4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#I5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#I6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#I7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#I8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#I9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#I10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#I11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#I12");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#I13");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#I14");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#I15");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#I16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#I17");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#I18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#I19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#I20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#I21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#I22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#I23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#I24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#I25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#I26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#I27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#I28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#I29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#I30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#I31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#I32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#I33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#I34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#I35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#I36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#J1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#J2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#J3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#J4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#J5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#J6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#J7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#J8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#J9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#J10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#J11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#J12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#J13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#J14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#J15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#J16");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#J17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#J18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#J19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#J20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#J21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#J22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#J23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#J24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#J25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#J26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#J27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#J28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#J29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#J30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#J31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#J32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#J33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#J34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#J35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#J36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#K1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#K2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#K3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#K4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#K5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#K6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#K7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#K8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#K9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#K10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#K11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#K12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#K13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#K14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#K15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#K16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#K17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#K18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#K19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#K20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#K21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#K22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#K23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#K24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#K25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#K26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#K27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#K28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#K29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#K30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#K31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#K32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#K33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#K34");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#K35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#K36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#L1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#L2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#L3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#L4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#L5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#L6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#L7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#L8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#L9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#L10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#L11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#L12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#L13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#L14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#L15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#L16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#L17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#L18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#L19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#L20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#L21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#L22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#L23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#L24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#L25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#L26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#L27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#L28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#L29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#L30");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#L31");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#L32");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#L33");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#L34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#L35");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#L36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#M1");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#M2");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#M3");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#M4");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#M5");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#M6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#M7");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#M8");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#M9");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#M10");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#M11");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#M12");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#M13");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#M14");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#M15");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#M16");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#M17");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#M18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#M19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#M20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#M21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#M22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#M23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#M24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#M25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#M26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#M27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#M28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#M29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#M30");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#M31");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#M32");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#M33");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#M34");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#M35");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#M36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceBlue", flags), "#N1");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceBlue", flags), "#N2");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceBlue", flags), "#N3");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceBlue", flags), "#N4");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceBlue", flags), "#N5");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceBlue", flags), "#N6");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceRed", flags), "#N7");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceRed", flags), "#N8");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceRed", flags), "#N9");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceRed", flags), "#N10");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceRed", flags), "#N11");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceRed", flags), "#N12");
			Assert.IsNull (greenType.GetEvent ("OnPrivateInstanceGreen", flags), "#N13");
			Assert.IsNull (greenType.GetEvent ("OnFamilyInstanceGreen", flags), "#N14");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemInstanceGreen", flags), "#N15");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemInstanceGreen", flags), "#N16");
			Assert.IsNull (greenType.GetEvent ("OnPublicInstanceGreen", flags), "#N17");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyInstanceGreen", flags), "#N18");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticBlue", flags), "#N19");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticBlue", flags), "#N20");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticBlue", flags), "#N21");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticBlue", flags), "#N22");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticBlue", flags), "#N23");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticBlue", flags), "#N24");
			Assert.IsNull (greenType.GetEvent ("OnPrivateStaticRed", flags), "#N25");
			Assert.IsNull (greenType.GetEvent ("OnFamilyStaticRed", flags), "#N26");
			Assert.IsNull (greenType.GetEvent ("OnFamANDAssemStaticRed", flags), "#N27");
			Assert.IsNull (greenType.GetEvent ("OnFamORAssemStaticRed", flags), "#N28");
			Assert.IsNull (greenType.GetEvent ("OnPublicStaticRed", flags), "#N29");
			Assert.IsNull (greenType.GetEvent ("OnAssemblyStaticRed", flags), "#N30");
			Assert.IsNotNull (greenType.GetEvent ("OnPrivateStaticGreen", flags), "#N31");
			Assert.IsNotNull (greenType.GetEvent ("OnFamilyStaticGreen", flags), "#N32");
			Assert.IsNotNull (greenType.GetEvent ("OnFamANDAssemStaticGreen", flags), "#N33");
			Assert.IsNotNull (greenType.GetEvent ("OnFamORAssemStaticGreen", flags), "#N34");
			Assert.IsNotNull (greenType.GetEvent ("OnPublicStaticGreen", flags), "#N35");
			Assert.IsNotNull (greenType.GetEvent ("OnAssemblyStaticGreen", flags), "#N36");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetFieldsIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			try {
				tb.GetFields ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetFieldsIncomplete_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("name", typeof (string), FieldAttributes.Private);
			tb.DefineField ("Sex", typeof (int), FieldAttributes.Public);
			tb.DefineField ("MALE", typeof (int), FieldAttributes.Public | FieldAttributes.Static);
			tb.DefineField ("FEMALE", typeof (int), FieldAttributes.Private | FieldAttributes.Static);

			FieldInfo [] fields = tb.GetFields ();
			Assert.AreEqual (2, fields.Length, "#A1");
			Assert.AreEqual ("Sex", fields [0].Name, "#A2");
			Assert.AreEqual ("MALE", fields [1].Name, "#A3");

			tb = module.DefineType (genTypeName ());
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("K", "V");
			tb.DefineField ("First", typeParams [0], FieldAttributes.Public);
			tb.DefineField ("Second", typeParams [1], FieldAttributes.Public);
			tb.DefineField ("Sex", typeof (int), FieldAttributes.Public);
			tb.DefineField ("MALE", typeof (int), FieldAttributes.Public | FieldAttributes.Static);
			tb.DefineField ("FEMALE", typeof (int), FieldAttributes.Private | FieldAttributes.Static);

			fields = tb.GetFields ();
			Assert.AreEqual (4, fields.Length, "#B1");
			Assert.AreEqual ("First", fields [0].Name, "#B2");
			Assert.AreEqual ("Second", fields [1].Name, "#B3");
			Assert.AreEqual ("Sex", fields [2].Name, "#B4");
			Assert.AreEqual ("MALE", fields [3].Name, "#B5");
		}

		[Test]
		public void TestGetFieldsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);

			Type emittedType = tb.CreateType ();
			FieldInfo [] dynamicFields = tb.GetFields ();
			FieldInfo [] emittedFields = emittedType.GetFields ();

			Assert.AreEqual (1, dynamicFields.Length, "#A1");
			Assert.AreEqual (dynamicFields.Length, emittedFields.Length, "#A2");
			Assert.IsFalse ((dynamicFields [0]) is FieldBuilder, "#A3");
			Assert.IsFalse ((emittedFields [0]) is FieldBuilder, "#A4");

			// bug #81638
			object value = Activator.CreateInstance (emittedType);
			emittedFields [0].SetValue (value, 5);
			Assert.AreEqual (5, emittedFields [0].GetValue (value), "#B1");
			Assert.AreEqual (5, dynamicFields [0].GetValue (value), "#B2");
			dynamicFields [0].SetValue (value, 4);
			Assert.AreEqual (4, emittedFields [0].GetValue (value), "#B3");
			Assert.AreEqual (4, dynamicFields [0].GetValue (value), "#B4");
		}

		[Test] // bug #82625 / 325292
		public void TestGetFieldsComplete_Generic ()
		{
			// FIXME: merge this with TestGetFieldsComplete when
			// bug #82625 is fixed

			TypeBuilder tb;
			Type emittedType;
			FieldInfo [] dynamicFields;
			FieldInfo [] emittedFields;

			tb = module.DefineType (genTypeName ());
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("K", "V");
			tb.DefineField ("First", typeParams [0], FieldAttributes.Public);
			tb.DefineField ("Second", typeParams [1], FieldAttributes.Public);
			tb.DefineField ("Sex", typeof (int), FieldAttributes.Public);
			tb.DefineField ("MALE", typeof (int), FieldAttributes.Public | FieldAttributes.Static);
			tb.DefineField ("FEMALE", typeof (int), FieldAttributes.Private | FieldAttributes.Static);

			emittedType = tb.CreateType ();
			dynamicFields = tb.GetFields ();
			emittedFields = emittedType.GetFields ();

			Assert.AreEqual (4, dynamicFields.Length, "#C1");
			Assert.IsFalse ((dynamicFields [0]) is FieldBuilder, "#C2");
			Assert.IsFalse ((dynamicFields [1]) is FieldBuilder, "#C3");
			Assert.IsFalse ((dynamicFields [2]) is FieldBuilder, "#C4");
			Assert.IsFalse ((dynamicFields [3]) is FieldBuilder, "#C5");
			Assert.AreEqual ("First", dynamicFields [0].Name, "#C6");
			Assert.AreEqual ("Second", dynamicFields [1].Name, "#C7");
			Assert.AreEqual ("Sex", dynamicFields [2].Name, "#C8");
			Assert.AreEqual ("MALE", dynamicFields [3].Name, "#C9");

			Assert.AreEqual (4, emittedFields.Length, "#D1");
			Assert.IsFalse ((emittedFields [0]) is FieldBuilder, "#D2");
			Assert.IsFalse ((emittedFields [1]) is FieldBuilder, "#D3");
			Assert.IsFalse ((emittedFields [2]) is FieldBuilder, "#D4");
			Assert.IsFalse ((emittedFields [3]) is FieldBuilder, "#D5");
			Assert.AreEqual ("First", emittedFields [0].Name, "#D6");
			Assert.AreEqual ("Second", emittedFields [1].Name, "#D7");
			Assert.AreEqual ("Sex", emittedFields [2].Name, "#D8");
			Assert.AreEqual ("MALE", emittedFields [3].Name, "#D9");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetFieldsFlagsIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			try {
				tb.GetFields (BindingFlags.Instance | BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetFieldsFlagsIncomplete_Mono ()
		{
			FieldInfo [] fields;

			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("name", typeof (string), FieldAttributes.Private);
			tb.DefineField ("Sex", typeof (int), FieldAttributes.Public);
			tb.DefineField ("MALE", typeof (int), FieldAttributes.Public | FieldAttributes.Static);
			tb.DefineField ("FEMALE", typeof (int), FieldAttributes.Private | FieldAttributes.Static);

			fields = tb.GetFields (BindingFlags.Public |
				BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreEqual (2, fields.Length, "#A1");
			Assert.AreEqual ("name", fields [0].Name, "#A2");
			Assert.AreEqual ("Sex", fields [1].Name, "#A3");

			fields = tb.GetFields (BindingFlags.Public |
				BindingFlags.Instance | BindingFlags.Static);
			Assert.AreEqual (2, fields.Length, "#B1");
			Assert.AreEqual ("Sex", fields [0].Name, "#B2");
			Assert.AreEqual ("MALE", fields [1].Name, "#B3");

			fields = tb.GetFields (BindingFlags.Public |
				BindingFlags.NonPublic | BindingFlags.Instance |
				BindingFlags.Static);
			Assert.AreEqual (4, fields.Length, "#C1");
			Assert.AreEqual ("name", fields [0].Name, "#C2");
			Assert.AreEqual ("Sex", fields [1].Name, "#C3");
			Assert.AreEqual ("MALE", fields [2].Name, "#C4");
			Assert.AreEqual ("FEMALE", fields [3].Name, "#C5");
		}

		[Test]
		public void TestGetFieldsFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetFields (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (tb.GetFields (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetFields (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (0, tb.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length);
			Assert.AreEqual (tb.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetFields (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		}

		[Test]
		public void TestGetFieldsFlagsComplete_Inheritance ()
		{
			FieldInfo [] fields;
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (13, fields.Length, "#A1");
			Assert.AreEqual ("privateInstanceGreen", fields [0].Name, "#A2");
			Assert.AreEqual ("familyInstanceGreen", fields [1].Name, "#A3");
			Assert.AreEqual ("famANDAssemInstanceGreen", fields [2].Name, "#A4");
			Assert.AreEqual ("famORAssemInstanceGreen", fields [3].Name, "#A5");
			Assert.AreEqual ("assemblyInstanceGreen", fields [4].Name, "#A6");
			Assert.AreEqual ("familyInstanceRed", fields [5].Name, "#A7");
			Assert.AreEqual ("famANDAssemInstanceRed", fields [6].Name, "#A8");
			Assert.AreEqual ("famORAssemInstanceRed", fields [7].Name, "#A9");
			Assert.AreEqual ("assemblyInstanceRed", fields [8].Name, "#A10");
			Assert.AreEqual ("familyInstanceBlue", fields [9].Name, "#A11");
			Assert.AreEqual ("famANDAssemInstanceBlue", fields [10].Name, "#A12");
			Assert.AreEqual ("famORAssemInstanceBlue", fields [11].Name, "#A13");
			Assert.AreEqual ("assemblyInstanceBlue", fields [12].Name, "#A14");

			flags = BindingFlags.Instance | BindingFlags.Public;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (3, fields.Length, "#B1");
			Assert.AreEqual ("publicInstanceGreen", fields [0].Name, "#B2");
			Assert.AreEqual ("publicInstanceRed", fields [1].Name, "#B3");
			Assert.AreEqual ("publicInstanceBlue", fields [2].Name, "#B4");

			flags = BindingFlags.Static | BindingFlags.Public;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (1, fields.Length, "#C1");
			Assert.AreEqual ("publicStaticGreen", fields [0].Name, "#C2");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (5, fields.Length, "#D1");
			Assert.AreEqual ("privateStaticGreen", fields [0].Name, "#D2");
			Assert.AreEqual ("familyStaticGreen", fields [1].Name, "#D3");
			Assert.AreEqual ("famANDAssemStaticGreen", fields [2].Name, "#D4");
			Assert.AreEqual ("famORAssemStaticGreen", fields [3].Name, "#D5");
			Assert.AreEqual ("assemblyStaticGreen", fields [4].Name, "#D6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (13, fields.Length, "#E1");
			Assert.AreEqual ("privateInstanceGreen", fields [0].Name, "#E2");
			Assert.AreEqual ("familyInstanceGreen", fields [1].Name, "#E3");
			Assert.AreEqual ("famANDAssemInstanceGreen", fields [2].Name, "#E4");
			Assert.AreEqual ("famORAssemInstanceGreen", fields [3].Name, "#E5");
			Assert.AreEqual ("assemblyInstanceGreen", fields [4].Name, "#E6");
			Assert.AreEqual ("familyInstanceRed", fields [5].Name, "#E7");
			Assert.AreEqual ("famANDAssemInstanceRed", fields [6].Name, "#E8");
			Assert.AreEqual ("famORAssemInstanceRed", fields [7].Name, "#E9");
			Assert.AreEqual ("assemblyInstanceRed", fields [8].Name, "#E10");
			Assert.AreEqual ("familyInstanceBlue", fields [9].Name, "#E11");
			Assert.AreEqual ("famANDAssemInstanceBlue", fields [10].Name, "#E12");
			Assert.AreEqual ("famORAssemInstanceBlue", fields [11].Name, "#E13");
			Assert.AreEqual ("assemblyInstanceBlue", fields [12].Name, "#E14");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (3, fields.Length, "#F1");
			Assert.AreEqual ("publicInstanceGreen", fields [0].Name, "#F2");
			Assert.AreEqual ("publicInstanceRed", fields [1].Name, "#F3");
			Assert.AreEqual ("publicInstanceBlue", fields [2].Name, "#F4");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (3, fields.Length, "#G1");
			Assert.AreEqual ("publicStaticGreen", fields [0].Name, "#G2");
			Assert.AreEqual ("publicStaticRed", fields [1].Name, "#G3");
			Assert.AreEqual ("publicStaticBlue", fields [2].Name, "#G4");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (13, fields.Length, "#H1");
			Assert.AreEqual ("privateStaticGreen", fields [0].Name, "#H2");
			Assert.AreEqual ("familyStaticGreen", fields [1].Name, "#H3");
			Assert.AreEqual ("famANDAssemStaticGreen", fields [2].Name, "#H4");
			Assert.AreEqual ("famORAssemStaticGreen", fields [3].Name, "#H5");
			Assert.AreEqual ("assemblyStaticGreen", fields [4].Name, "#H6");
			Assert.AreEqual ("familyStaticRed", fields [5].Name, "#H7");
			Assert.AreEqual ("famANDAssemStaticRed", fields [6].Name, "#H8");
			Assert.AreEqual ("famORAssemStaticRed", fields [7].Name, "#H9");
			Assert.AreEqual ("assemblyStaticRed", fields [8].Name, "#H10");
			Assert.AreEqual ("familyStaticBlue", fields [9].Name, "#H11");
			Assert.AreEqual ("famANDAssemStaticBlue", fields [10].Name, "#H12");
			Assert.AreEqual ("famORAssemStaticBlue", fields [11].Name, "#H13");
			Assert.AreEqual ("assemblyStaticBlue", fields [12].Name, "#H14");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (5, fields.Length, "#I1");
			Assert.AreEqual ("privateInstanceGreen", fields [0].Name, "#I2");
			Assert.AreEqual ("familyInstanceGreen", fields [1].Name, "#I3");
			Assert.AreEqual ("famANDAssemInstanceGreen", fields [2].Name, "#I4");
			Assert.AreEqual ("famORAssemInstanceGreen", fields [3].Name, "#I5");
			Assert.AreEqual ("assemblyInstanceGreen", fields [4].Name, "#I6");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (1, fields.Length, "#J1");
			Assert.AreEqual ("publicInstanceGreen", fields [0].Name, "#J2");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (1, fields.Length, "#K1");
			Assert.AreEqual ("publicStaticGreen", fields [0].Name, "#K2");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (5, fields.Length, "#L1");
			Assert.AreEqual ("privateStaticGreen", fields [0].Name, "#L2");
			Assert.AreEqual ("familyStaticGreen", fields [1].Name, "#L3");
			Assert.AreEqual ("famANDAssemStaticGreen", fields [2].Name, "#L4");
			Assert.AreEqual ("famORAssemStaticGreen", fields [3].Name, "#L5");
			Assert.AreEqual ("assemblyStaticGreen", fields [4].Name, "#L6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (16, fields.Length, "#M1");
			Assert.AreEqual ("privateInstanceGreen", fields [0].Name, "#M2");
			Assert.AreEqual ("familyInstanceGreen", fields [1].Name, "#M3");
			Assert.AreEqual ("famANDAssemInstanceGreen", fields [2].Name, "#M4");
			Assert.AreEqual ("famORAssemInstanceGreen", fields [3].Name, "#M5");
			Assert.AreEqual ("publicInstanceGreen", fields [4].Name, "#M6");
			Assert.AreEqual ("assemblyInstanceGreen", fields [5].Name, "#M7");
			Assert.AreEqual ("familyInstanceRed", fields [6].Name, "#M8");
			Assert.AreEqual ("famANDAssemInstanceRed", fields [7].Name, "#M9");
			Assert.AreEqual ("famORAssemInstanceRed", fields [8].Name, "#M10");
			Assert.AreEqual ("publicInstanceRed", fields [9].Name, "#M11");
			Assert.AreEqual ("assemblyInstanceRed", fields [10].Name, "#M12");
			Assert.AreEqual ("familyInstanceBlue", fields [11].Name, "#M13");
			Assert.AreEqual ("famANDAssemInstanceBlue", fields [12].Name, "#M14");
			Assert.AreEqual ("famORAssemInstanceBlue", fields [13].Name, "#M15");
			Assert.AreEqual ("publicInstanceBlue", fields [14].Name, "#M16");
			Assert.AreEqual ("assemblyInstanceBlue", fields [15].Name, "#M17");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			fields = greenType.GetFields (flags);

			Assert.AreEqual (6, fields.Length, "#N1");
			Assert.AreEqual ("privateStaticGreen", fields [0].Name, "#N2");
			Assert.AreEqual ("familyStaticGreen", fields [1].Name, "#N3");
			Assert.AreEqual ("famANDAssemStaticGreen", fields [2].Name, "#N4");
			Assert.AreEqual ("famORAssemStaticGreen", fields [3].Name, "#N5");
			Assert.AreEqual ("publicStaticGreen", fields [4].Name, "#N6");
			Assert.AreEqual ("assemblyStaticGreen", fields [5].Name, "#N7");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetFieldIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("test", typeof (int), FieldAttributes.Public);
			try {
				tb.GetField ("test");
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetFieldIncomplete_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			tb.DefineField ("OtherField", typeof (int), FieldAttributes.Private);

			FieldInfo field = tb.GetField ("TestField");
			Assert.IsNotNull (field, "#A1");
			Assert.AreEqual ("TestField", field.Name, "#A2");
			Assert.IsTrue (field is FieldBuilder, "#A3");

			Assert.IsNull (tb.GetField ("OtherField"), "#B1");
			Assert.IsNull (tb.GetField ("TestOtherField"), "#B2");
		}

		[Test]
		public void TestGetFieldComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);

			Type emittedType = tb.CreateType ();

			FieldInfo dynamicField = tb.GetField ("TestField");
			FieldInfo emittedField = emittedType.GetField ("TestField");
			Assert.IsNotNull (dynamicField, "#A1");
			Assert.AreEqual (dynamicField.Name, emittedField.Name, "#A2");
			Assert.IsNull (tb.GetField ("TestOtherField"), "#A3");
			Assert.IsFalse (emittedField is FieldBuilder, "#A4");
			Assert.IsFalse (dynamicField is FieldBuilder, "#A5");

			// bug #81638
			object value = Activator.CreateInstance (emittedType);
			emittedField.SetValue (value, 5);
			Assert.AreEqual (5, emittedField.GetValue (value), "#B1");
			Assert.AreEqual (5, dynamicField.GetValue (value), "#B2");
			dynamicField.SetValue (value, 4);
			Assert.AreEqual (4, emittedField.GetValue (value), "#B3");
			Assert.AreEqual (4, dynamicField.GetValue (value), "#B4");
		}

		[Test] // bug #81640
		public void TestGetFieldComplete_Type ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			Type emittedType = tb.CreateType ();
			FieldInfo dynamicField = tb.GetField ("TestField");
			Assert.IsFalse (dynamicField is FieldBuilder, "#1");

			object value = Activator.CreateInstance (emittedType);
			Assert.AreEqual (0, dynamicField.GetValue (value), "#2");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetFieldFlagsIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			tb.DefineField ("OtherField", typeof (int), FieldAttributes.Private);
			try {
				tb.GetField ("test", BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetFieldFlagsIncomplete_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);
			tb.DefineField ("OtherField", typeof (int), FieldAttributes.Private);

			FieldInfo field = tb.GetField ("TestField", BindingFlags.Public
				| BindingFlags.Instance);
			Assert.IsNotNull (field, "#A1");
			Assert.AreEqual ("TestField", field.Name, "#A2");
			Assert.IsTrue (field is FieldBuilder, "#A3");

			field = tb.GetField ("OtherField", BindingFlags.NonPublic |
				BindingFlags.Instance);
			Assert.IsNotNull (field, "#B1");
			Assert.AreEqual ("OtherField", field.Name, "#B2");
			Assert.IsTrue (field is FieldBuilder, "#B3");

			Assert.IsNull (tb.GetField ("TestField", BindingFlags.NonPublic |
				BindingFlags.Instance), "#C1");
			Assert.IsNull (tb.GetField ("TestField", BindingFlags.Public |
				BindingFlags.Static), "#C2");
			Assert.IsNull (tb.GetField ("OtherField", BindingFlags.Public |
				BindingFlags.Instance), "#C3");
			Assert.IsNull (tb.GetField ("OtherField", BindingFlags.Public |
				BindingFlags.Static), "#C4");
			Assert.IsNull (tb.GetField ("NotExist", BindingFlags.NonPublic |
				BindingFlags.Instance), "#C5");
			Assert.IsNull (tb.GetField ("NotExist", BindingFlags.Public |
				BindingFlags.Instance), "#C6");
		}

		[Test]
		public void TestGetFieldFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("TestField", typeof (int), FieldAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.IsNotNull (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public));
			Assert.AreEqual (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public).Name,
				emittedType.GetField ("TestField", BindingFlags.Instance | BindingFlags.Public).Name);
			Assert.IsNull (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic));
			Assert.AreEqual (tb.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic),
				emittedType.GetField ("TestField", BindingFlags.Instance | BindingFlags.NonPublic));
		}

		[Test]
		public void TestGetFieldFlagsComplete_Inheritance ()
		{
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#A1");
			Assert.IsNotNull (greenType.GetField ("familyInstanceBlue", flags), "#A2");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#A3");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#A4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#A5");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceBlue", flags), "#A6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#A7");
			Assert.IsNotNull (greenType.GetField ("familyInstanceRed", flags), "#A8");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#A9");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceRed", flags), "#A10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#A11");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceRed", flags), "#A12");
			Assert.IsNotNull (greenType.GetField ("privateInstanceGreen", flags), "#A13");
			Assert.IsNotNull (greenType.GetField ("familyInstanceGreen", flags), "#A14");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#A15");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#A16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#A17");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceGreen", flags), "#A18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#A19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#A20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#A21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#A22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#A23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#A24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#A25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#A26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#A27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#A28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#A29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#A30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#A31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#A32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#A33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#A34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#A35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#A36");

			flags = BindingFlags.Instance | BindingFlags.Public;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#B1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#B2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#B3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#B4");
			Assert.IsNotNull (greenType.GetField ("publicInstanceBlue", flags), "#B5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#B6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#B7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#B8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#B9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#B10");
			Assert.IsNotNull (greenType.GetField ("publicInstanceRed", flags), "#B11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#B12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#B13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#B14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#B15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#B16");
			Assert.IsNotNull (greenType.GetField ("publicInstanceGreen", flags), "#B17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#B18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#B19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#B20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#B21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#B22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#B23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#B24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#B25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#B26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#B27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#B28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#B29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#B30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#B31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#B32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#B33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#B34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#B35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#B36");

			flags = BindingFlags.Static | BindingFlags.Public;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#C1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#C2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#C3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#C4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#C5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#C6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#C7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#C8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#C9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#C10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#C11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#C12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#C13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#C14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#C15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#C16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#C17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#C18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#C19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#C20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#C21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#C22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#C23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#C24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#C25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#C26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#C27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#C28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#C29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#C30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#C31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#C32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#C33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#C34");
			Assert.IsNotNull (greenType.GetField ("publicStaticGreen", flags), "#C35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#C36");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#D1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#D2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#D3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#D4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#D5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#D6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#D7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#D8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#D9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#D10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#D11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#D12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#D13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#D14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#D15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#D16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#D17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#D18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#D19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#D20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#D21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#D22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#D23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#D24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#D25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#D26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#D27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#D28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#D29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#D30");
			Assert.IsNotNull (greenType.GetField ("privateStaticGreen", flags), "#D31");
			Assert.IsNotNull (greenType.GetField ("familyStaticGreen", flags), "#D32");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#D33");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticGreen", flags), "#D34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#D35");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticGreen", flags), "#D36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#E1");
			Assert.IsNotNull (greenType.GetField ("familyInstanceBlue", flags), "#E2");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#E3");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#E4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#E5");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceBlue", flags), "#E6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#E7");
			Assert.IsNotNull (greenType.GetField ("familyInstanceRed", flags), "#E8");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#E9");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceRed", flags), "#E10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#E11");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceRed", flags), "#E12");
			Assert.IsNotNull (greenType.GetField ("privateInstanceGreen", flags), "#E13");
			Assert.IsNotNull (greenType.GetField ("familyInstanceGreen", flags), "#E14");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#E15");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#E16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#E17");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceGreen", flags), "#E18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#E19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#E20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#E21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#E22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#E23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#E24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#E25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#E26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#E27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#E28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#E29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#E30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#E31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#E32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#E33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#E34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#E35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#E36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#F1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#F2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#F3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#F4");
			Assert.IsNotNull (greenType.GetField ("publicInstanceBlue", flags), "#F5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#F6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#F7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#F8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#F9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#F10");
			Assert.IsNotNull (greenType.GetField ("publicInstanceRed", flags), "#F11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#F12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#F13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#F14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#F15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#F16");
			Assert.IsNotNull (greenType.GetField ("publicInstanceGreen", flags), "#F17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#F18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#F19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#F20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#F21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#F22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#F23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#F24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#F25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#F26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#F27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#F28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#F29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#F30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#F31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#F32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#F33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#F34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#F35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#F36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#G1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#G2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#G3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#G4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#G5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#G6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#G7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#G8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#G9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#G10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#G11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#G12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#G13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#G14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#G15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#G16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#G17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#G18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#G19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#G20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#G21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#G22");
			Assert.IsNotNull (greenType.GetField ("publicStaticBlue", flags), "#G23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#G24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#G25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#G26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#G27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#G28");
			Assert.IsNotNull (greenType.GetField ("publicStaticRed", flags), "#G29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#G30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#G31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#G32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#G33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#G34");
			Assert.IsNotNull (greenType.GetField ("publicStaticGreen", flags), "#G35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#G36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#H1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#H2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#H3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#H4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#H5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#H6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#H7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#H8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#H9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#H10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#H11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#H12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#H13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#H14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#H15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#H16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#H17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#H18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#H19");
			Assert.IsNotNull (greenType.GetField ("familyStaticBlue", flags), "#H20");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#H21");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticBlue", flags), "#H22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#H23");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticBlue", flags), "#H24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#H25");
			Assert.IsNotNull (greenType.GetField ("familyStaticRed", flags), "#H26");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticRed", flags), "#H27");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticRed", flags), "#H28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#H29");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticRed", flags), "#H30");
			Assert.IsNotNull (greenType.GetField ("privateStaticGreen", flags), "#H31");
			Assert.IsNotNull (greenType.GetField ("familyStaticGreen", flags), "#H32");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#H33");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticGreen", flags), "#H34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#H35");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticGreen", flags), "#H36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#I1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#I2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#I3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#I4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#I5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#I6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#I7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#I8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#I9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#I10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#I11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#I12");
			Assert.IsNotNull (greenType.GetField ("privateInstanceGreen", flags), "#I13");
			Assert.IsNotNull (greenType.GetField ("familyInstanceGreen", flags), "#I14");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#I15");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#I16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#I17");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceGreen", flags), "#I18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#I19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#I20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#I21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#I22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#I23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#I24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#I25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#I26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#I27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#I28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#I29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#I30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#I31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#I32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#I33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#I34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#I35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#I36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#J1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#J2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#J3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#J4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#J5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#J6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#J7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#J8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#J9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#J10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#J11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#J12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#J13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#J14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#J15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#J16");
			Assert.IsNotNull (greenType.GetField ("publicInstanceGreen", flags), "#J17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#J18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#J19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#J20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#J21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#J22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#J23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#J24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#J25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#J26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#J27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#J28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#J29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#J30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#J31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#J32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#J33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#J34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#J35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#J36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#K1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#K2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#K3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#K4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#K5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#K6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#K7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#K8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#K9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#K10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#K11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#K12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#K13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#K14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#K15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#K16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#K17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#K18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#K19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#K20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#K21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#K22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#K23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#K24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#K25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#K26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#K27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#K28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#K29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#K30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#K31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#K32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#K33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#K34");
			Assert.IsNotNull (greenType.GetField ("publicStaticGreen", flags), "#K35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#K36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#L1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#L2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#L3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#L4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#L5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#L6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#L7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#L8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#L9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#L10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#L11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#L12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#L13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#L14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#L15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#L16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#L17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#L18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#L19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#L20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#L21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#L22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#L23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#L24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#L25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#L26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#L27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#L28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#L29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#L30");
			Assert.IsNotNull (greenType.GetField ("privateStaticGreen", flags), "#L31");
			Assert.IsNotNull (greenType.GetField ("familyStaticGreen", flags), "#L32");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#L33");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticGreen", flags), "#L34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#L35");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticGreen", flags), "#L36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#M1");
			Assert.IsNotNull (greenType.GetField ("familyInstanceBlue", flags), "#M2");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#M3");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#M4");
			Assert.IsNotNull (greenType.GetField ("publicInstanceBlue", flags), "#M5");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceBlue", flags), "#M6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#M7");
			Assert.IsNotNull (greenType.GetField ("familyInstanceRed", flags), "#M8");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#M9");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceRed", flags), "#M10");
			Assert.IsNotNull (greenType.GetField ("publicInstanceRed", flags), "#M11");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceRed", flags), "#M12");
			Assert.IsNotNull (greenType.GetField ("privateInstanceGreen", flags), "#M13");
			Assert.IsNotNull (greenType.GetField ("familyInstanceGreen", flags), "#M14");
			Assert.IsNotNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#M15");
			Assert.IsNotNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#M16");
			Assert.IsNotNull (greenType.GetField ("publicInstanceGreen", flags), "#M17");
			Assert.IsNotNull (greenType.GetField ("assemblyInstanceGreen", flags), "#M18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#M19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#M20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#M21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#M22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#M23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#M24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#M25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#M26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#M27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#M28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#M29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#M30");
			Assert.IsNull (greenType.GetField ("privateStaticGreen", flags), "#M31");
			Assert.IsNull (greenType.GetField ("familyStaticGreen", flags), "#M32");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#M33");
			Assert.IsNull (greenType.GetField ("famORAssemStaticGreen", flags), "#M34");
			Assert.IsNull (greenType.GetField ("publicStaticGreen", flags), "#M35");
			Assert.IsNull (greenType.GetField ("assemblyStaticGreen", flags), "#M36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetField ("privateInstanceBlue", flags), "#N1");
			Assert.IsNull (greenType.GetField ("familyInstanceBlue", flags), "#N2");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceBlue", flags), "#N3");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceBlue", flags), "#N4");
			Assert.IsNull (greenType.GetField ("publicInstanceBlue", flags), "#N5");
			Assert.IsNull (greenType.GetField ("assemblyInstanceBlue", flags), "#N6");
			Assert.IsNull (greenType.GetField ("privateInstanceRed", flags), "#N7");
			Assert.IsNull (greenType.GetField ("familyInstanceRed", flags), "#N8");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceRed", flags), "#N9");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceRed", flags), "#N10");
			Assert.IsNull (greenType.GetField ("publicInstanceRed", flags), "#N11");
			Assert.IsNull (greenType.GetField ("assemblyInstanceRed", flags), "#N12");
			Assert.IsNull (greenType.GetField ("privateInstanceGreen", flags), "#N13");
			Assert.IsNull (greenType.GetField ("familyInstanceGreen", flags), "#N14");
			Assert.IsNull (greenType.GetField ("famANDAssemInstanceGreen", flags), "#N15");
			Assert.IsNull (greenType.GetField ("famORAssemInstanceGreen", flags), "#N16");
			Assert.IsNull (greenType.GetField ("publicInstanceGreen", flags), "#N17");
			Assert.IsNull (greenType.GetField ("assemblyInstanceGreen", flags), "#N18");
			Assert.IsNull (greenType.GetField ("privateStaticBlue", flags), "#N19");
			Assert.IsNull (greenType.GetField ("familyStaticBlue", flags), "#N20");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticBlue", flags), "#N21");
			Assert.IsNull (greenType.GetField ("famORAssemStaticBlue", flags), "#N22");
			Assert.IsNull (greenType.GetField ("publicStaticBlue", flags), "#N23");
			Assert.IsNull (greenType.GetField ("assemblyStaticBlue", flags), "#N24");
			Assert.IsNull (greenType.GetField ("privateStaticRed", flags), "#N25");
			Assert.IsNull (greenType.GetField ("familyStaticRed", flags), "#N26");
			Assert.IsNull (greenType.GetField ("famANDAssemStaticRed", flags), "#N27");
			Assert.IsNull (greenType.GetField ("famORAssemStaticRed", flags), "#N28");
			Assert.IsNull (greenType.GetField ("publicStaticRed", flags), "#N29");
			Assert.IsNull (greenType.GetField ("assemblyStaticRed", flags), "#N30");
			Assert.IsNotNull (greenType.GetField ("privateStaticGreen", flags), "#N31");
			Assert.IsNotNull (greenType.GetField ("familyStaticGreen", flags), "#N32");
			Assert.IsNotNull (greenType.GetField ("famANDAssemStaticGreen", flags), "#N33");
			Assert.IsNotNull (greenType.GetField ("famORAssemStaticGreen", flags), "#N34");
			Assert.IsNotNull (greenType.GetField ("publicStaticGreen", flags), "#N35");
			Assert.IsNotNull (greenType.GetField ("assemblyStaticGreen", flags), "#N36");
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetPropertiesIncomplete_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "Name", "name", MethodAttributes.Public);
			DefineStringProperty (tb, "Income", "income", MethodAttributes.Private);
			DefineStringProperty (tb, "FirstName", "firstName", MethodAttributes.Public);

			PropertyInfo [] properties = tb.GetProperties ();
			Assert.AreEqual (2, properties.Length, "#1");
			Assert.AreEqual ("Name", properties [0].Name, "#2");
			Assert.AreEqual ("FirstName", properties [1].Name, "#3");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetPropertiesIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "Name", "name", MethodAttributes.Public);
			DefineStringProperty (tb, "FirstName", "firstName", MethodAttributes.Public);
			DefineStringProperty (tb, "Income", "income", MethodAttributes.Private);

			try {
				tb.GetProperties ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetPropertiesComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetProperties ().Length);
			Assert.AreEqual (tb.GetProperties ().Length, emittedType.GetProperties ().Length);
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetPropertiesFlagsIncomplete_Mono ()
		{
			PropertyInfo [] properties;

			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "Name", "name", MethodAttributes.Public);
			DefineStringProperty (tb, "Income", "income", MethodAttributes.Private);
			DefineStringProperty (tb, "FirstName", "firstName", MethodAttributes.Public);

			properties = tb.GetProperties (BindingFlags.Public | 
				BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreEqual (3, properties.Length, "#A1");
			Assert.AreEqual ("Name", properties [0].Name, "#A2");
			Assert.AreEqual ("Income", properties [1].Name, "#A3");
			Assert.AreEqual ("FirstName", properties [2].Name, "#A4");

			properties = tb.GetProperties (BindingFlags.Public |
				BindingFlags.Instance);
			Assert.AreEqual (2, properties.Length, "#B1");
			Assert.AreEqual ("Name", properties [0].Name, "#B2");
			Assert.AreEqual ("FirstName", properties [1].Name, "#B3");

			properties = tb.GetProperties (BindingFlags.NonPublic |
				BindingFlags.Instance);
			Assert.AreEqual (1, properties.Length, "#C1");
			Assert.AreEqual ("Income", properties [0].Name, "#C2");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetPropertiesFlagsIncomplete_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "Name", "name", MethodAttributes.Public);
			DefineStringProperty (tb, "Income", "income", MethodAttributes.Private);
			DefineStringProperty (tb, "FirstName", "firstName", MethodAttributes.Public);

			try {
				tb.GetProperties (BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetPropertiesFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (tb.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetProperties (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (0, tb.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length);
			Assert.AreEqual (tb.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		}

		[Test]
		public void TestGetPropertiesFlagsComplete_Inheritance ()
		{
			PropertyInfo [] props;
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (13, props.Length, "#A1");
			Assert.AreEqual ("PrivateInstanceGreen", props [0].Name, "#A2");
			Assert.AreEqual ("FamilyInstanceGreen", props [1].Name, "#A3");
			Assert.AreEqual ("FamANDAssemInstanceGreen", props [2].Name, "#A4");
			Assert.AreEqual ("FamORAssemInstanceGreen", props [3].Name, "#A5");
			Assert.AreEqual ("AssemblyInstanceGreen", props [4].Name, "#A6");
			Assert.AreEqual ("FamilyInstanceRed", props [5].Name, "#A7");
			Assert.AreEqual ("FamANDAssemInstanceRed", props [6].Name, "#A8");
			Assert.AreEqual ("FamORAssemInstanceRed", props [7].Name, "#A9");
			Assert.AreEqual ("AssemblyInstanceRed", props [8].Name, "#A10");
			Assert.AreEqual ("FamilyInstanceBlue", props [9].Name, "#A11");
			Assert.AreEqual ("FamANDAssemInstanceBlue", props [10].Name, "#A12");
			Assert.AreEqual ("FamORAssemInstanceBlue", props [11].Name, "#A13");
			Assert.AreEqual ("AssemblyInstanceBlue", props [12].Name, "#A15");

			flags = BindingFlags.Instance | BindingFlags.Public;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (3, props.Length, "#B1");
			Assert.AreEqual ("PublicInstanceGreen", props [0].Name, "#B2");
			Assert.AreEqual ("PublicInstanceRed", props [1].Name, "#B3");
			Assert.AreEqual ("PublicInstanceBlue", props [2].Name, "#B4");

			flags = BindingFlags.Static | BindingFlags.Public;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (1, props.Length, "#C1");
			Assert.AreEqual ("PublicStaticGreen", props [0].Name, "#C2");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (5, props.Length, "#D1");
			Assert.AreEqual ("PrivateStaticGreen", props [0].Name, "#D2");
			Assert.AreEqual ("FamilyStaticGreen", props [1].Name, "#D3");
			Assert.AreEqual ("FamANDAssemStaticGreen", props [2].Name, "#D4");
			Assert.AreEqual ("FamORAssemStaticGreen", props [3].Name, "#D5");
			Assert.AreEqual ("AssemblyStaticGreen", props [4].Name, "#D6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (13, props.Length, "#E1");
			Assert.AreEqual ("PrivateInstanceGreen", props [0].Name, "#E2");
			Assert.AreEqual ("FamilyInstanceGreen", props [1].Name, "#E3");
			Assert.AreEqual ("FamANDAssemInstanceGreen", props [2].Name, "#E4");
			Assert.AreEqual ("FamORAssemInstanceGreen", props [3].Name, "#E5");
			Assert.AreEqual ("AssemblyInstanceGreen", props [4].Name, "#E6");
			Assert.AreEqual ("FamilyInstanceRed", props [5].Name, "#E7");
			Assert.AreEqual ("FamANDAssemInstanceRed", props [6].Name, "#E8");
			Assert.AreEqual ("FamORAssemInstanceRed", props [7].Name, "#E9");
			Assert.AreEqual ("AssemblyInstanceRed", props [8].Name, "#E10");
			Assert.AreEqual ("FamilyInstanceBlue", props [9].Name, "#E11");
			Assert.AreEqual ("FamANDAssemInstanceBlue", props [10].Name, "#E12");
			Assert.AreEqual ("FamORAssemInstanceBlue", props [11].Name, "#E13");
			Assert.AreEqual ("AssemblyInstanceBlue", props [12].Name, "#E14");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (3, props.Length, "#F1");
			Assert.AreEqual ("PublicInstanceGreen", props [0].Name, "#F2");
			Assert.AreEqual ("PublicInstanceRed", props [1].Name, "#F3");
			Assert.AreEqual ("PublicInstanceBlue", props [2].Name, "#F4");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (3, props.Length, "#G1");
			Assert.AreEqual ("PublicStaticGreen", props [0].Name, "#G2");
			Assert.AreEqual ("PublicStaticRed", props [1].Name, "#G3");
			Assert.AreEqual ("PublicStaticBlue", props [2].Name, "#G4");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (13, props.Length, "#H1");
			Assert.AreEqual ("PrivateStaticGreen", props [0].Name, "#H2");
			Assert.AreEqual ("FamilyStaticGreen", props [1].Name, "#H3");
			Assert.AreEqual ("FamANDAssemStaticGreen", props [2].Name, "#H4");
			Assert.AreEqual ("FamORAssemStaticGreen", props [3].Name, "#H5");
			Assert.AreEqual ("AssemblyStaticGreen", props [4].Name, "#H6");
			Assert.AreEqual ("FamilyStaticRed", props [5].Name, "#H7");
			Assert.AreEqual ("FamANDAssemStaticRed", props [6].Name, "#H8");
			Assert.AreEqual ("FamORAssemStaticRed", props [7].Name, "#H9");
			Assert.AreEqual ("AssemblyStaticRed", props [8].Name, "#H10");
			Assert.AreEqual ("FamilyStaticBlue", props [9].Name, "#H11");
			Assert.AreEqual ("FamANDAssemStaticBlue", props [10].Name, "#H12");
			Assert.AreEqual ("FamORAssemStaticBlue", props [11].Name, "#H13");
			Assert.AreEqual ("AssemblyStaticBlue", props [12].Name, "#H14");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (5, props.Length, "#I1");
			Assert.AreEqual ("PrivateInstanceGreen", props [0].Name, "#I2");
			Assert.AreEqual ("FamilyInstanceGreen", props [1].Name, "#I3");
			Assert.AreEqual ("FamANDAssemInstanceGreen", props [2].Name, "#I4");
			Assert.AreEqual ("FamORAssemInstanceGreen", props [3].Name, "#I5");
			Assert.AreEqual ("AssemblyInstanceGreen", props [4].Name, "#I6");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (1, props.Length, "#J1");
			Assert.AreEqual ("PublicInstanceGreen", props [0].Name, "#J2");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (1, props.Length, "#K1");
			Assert.AreEqual ("PublicStaticGreen", props [0].Name, "#K2");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (5, props.Length, "#L1");
			Assert.AreEqual ("PrivateStaticGreen", props [0].Name, "#L2");
			Assert.AreEqual ("FamilyStaticGreen", props [1].Name, "#L3");
			Assert.AreEqual ("FamANDAssemStaticGreen", props [2].Name, "#L4");
			Assert.AreEqual ("FamORAssemStaticGreen", props [3].Name, "#L5");
			Assert.AreEqual ("AssemblyStaticGreen", props [4].Name, "#L6");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (16, props.Length, "#M1");
			Assert.AreEqual ("PrivateInstanceGreen", props [0].Name, "#M2");
			Assert.AreEqual ("FamilyInstanceGreen", props [1].Name, "#M3");
			Assert.AreEqual ("FamANDAssemInstanceGreen", props [2].Name, "#M4");
			Assert.AreEqual ("FamORAssemInstanceGreen", props [3].Name, "#M5");
			Assert.AreEqual ("PublicInstanceGreen", props [4].Name, "#M6");
			Assert.AreEqual ("AssemblyInstanceGreen", props [5].Name, "#M7");
			Assert.AreEqual ("FamilyInstanceRed", props [6].Name, "#M8");
			Assert.AreEqual ("FamANDAssemInstanceRed", props [7].Name, "#M9");
			Assert.AreEqual ("FamORAssemInstanceRed", props [8].Name, "#M10");
			Assert.AreEqual ("PublicInstanceRed", props [9].Name, "#M11");
			Assert.AreEqual ("AssemblyInstanceRed", props [10].Name, "#M12");
			Assert.AreEqual ("FamilyInstanceBlue", props [11].Name, "#M13");
			Assert.AreEqual ("FamANDAssemInstanceBlue", props [12].Name, "#M14");
			Assert.AreEqual ("FamORAssemInstanceBlue", props [13].Name, "#M15");
			Assert.AreEqual ("PublicInstanceBlue", props [14].Name, "#M16");
			Assert.AreEqual ("AssemblyInstanceBlue", props [15].Name, "#M17");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			props = greenType.GetProperties (flags);

			Assert.AreEqual (6, props.Length, "#N1");
			Assert.AreEqual ("PrivateStaticGreen", props [0].Name, "#N2");
			Assert.AreEqual ("FamilyStaticGreen", props [1].Name, "#N3");
			Assert.AreEqual ("FamANDAssemStaticGreen", props [2].Name, "#N4");
			Assert.AreEqual ("FamORAssemStaticGreen", props [3].Name, "#N5");
			Assert.AreEqual ("PublicStaticGreen", props [4].Name, "#N6");
			Assert.AreEqual ("AssemblyStaticGreen", props [5].Name, "#N7");
		}

		[Test]
		public void TestGetPropertyIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetProperty ("test");
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetPropertyComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.IsNotNull (emittedType.GetProperty ("CustomerName"));
			Assert.IsNull (emittedType.GetProperty ("OtherCustomerName"));

			try {
				tb.GetProperty ("CustomerName");
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetPropertyFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetProperty ("test", BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetPropertyFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			DefineStringProperty (tb, "CustomerName", "customerName", MethodAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.IsNotNull (emittedType.GetProperty ("CustomerName", BindingFlags.Instance |
				BindingFlags.Public));
			Assert.IsNull (emittedType.GetProperty ("CustomerName", BindingFlags.Instance |
				BindingFlags.NonPublic));

			try {
				tb.GetProperty ("CustomerName", BindingFlags.Instance | BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMethodFlagsComplete ()
		{
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#A1");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#A2");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#A3");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#A4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#A5");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#A6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#A7");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#A8");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#A9");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#A10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#A11");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#A12");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#A13");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#A14");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#A15");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#A16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#A17");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#A18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#A19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#A20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#A21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#A22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#A23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#A24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#A25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#A26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#A27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#A28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#A29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#A30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#A31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#A32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#A33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#A34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#A35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#A36");

			flags = BindingFlags.Instance | BindingFlags.Public;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#B1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#B2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#B3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#B4");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#B5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#B6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#B7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#B8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#B9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#B10");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#B11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#B12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#B13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#B14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#B15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#B16");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#B17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#B18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#B19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#B20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#B21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#B22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#B23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#B24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#B25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#B26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#B27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#B28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#B29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#B30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#B31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#B32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#B33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#B34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#B35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#B36");

			flags = BindingFlags.Static | BindingFlags.Public;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#C1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#C2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#C3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#C4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#C5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#C6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#C7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#C8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#C9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#C10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#C11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#C12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#C13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#C14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#C15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#C16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#C17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#C18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#C19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#C20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#C21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#C22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#C23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#C24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#C25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#C26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#C27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#C28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#C29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#C30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#C31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#C32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#C33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#C34");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#C35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#C36");

			flags = BindingFlags.Static | BindingFlags.NonPublic;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#D1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#D2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#D3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#D4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#D5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#D6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#D7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#D8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#D9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#D10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#D11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#D12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#D13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#D14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#D15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#D16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#D17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#D18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#D19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#D20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#D21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#D22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#D23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#D24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#D25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#D26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#D27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#D28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#D29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#D30");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#D31");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#D32");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#D33");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#D34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#D35");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#D36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#E1");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#E2");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#E3");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#E4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#E5");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#E6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#E7");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#E8");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#E9");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#E10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#E11");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#E12");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#E13");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#E14");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#E15");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#E16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#E17");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#E18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#E19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#E20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#E21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#E22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#E23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#E24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#E25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#E26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#E27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#E28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#E29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#E30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#E31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#E32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#E33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#E34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#E35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#E36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#F1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#F2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#F3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#F4");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#F5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#F6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#F7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#F8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#F9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#F10");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#F11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#F12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#F13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#F14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#F15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#F16");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#F17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#F18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#F19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#F20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#F21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#F22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#F23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#F24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#F25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#F26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#F27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#F28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#F29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#F30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#F31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#F32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#F33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#F34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#F35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#F36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#G1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#G2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#G3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#G4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#G5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#G6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#G7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#G8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#G9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#G10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#G11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#G12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#G13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#G14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#G15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#G16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#G17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#G18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#G19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#G20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#G21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#G22");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#G23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#G24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#G25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#G26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#G27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#G28");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#G29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#G30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#G31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#G32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#G33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#G34");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#G35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#G36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#H1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#H2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#H3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#H4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#H5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#H6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#H7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#H8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#H9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#H10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#H11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#H12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#H13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#H14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#H15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#H16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#H17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#H18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#H19");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#H20");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#H21");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#H22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#H23");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#H24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#H25");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#H26");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#H27");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#H28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#H29");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#H30");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#H31");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#H32");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#H33");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#H34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#H35");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#H36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#I1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#I2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#I3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#I4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#I5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#I6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#I7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#I8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#I9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#I10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#I11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#I12");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#I13");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#I14");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#I15");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#I16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#I17");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#I18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#I19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#I20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#I21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#I22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#I23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#I24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#I25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#I26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#I27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#I28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#I29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#I30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#I31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#I32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#I33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#I34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#I35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#I36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#J1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#J2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#J3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#J4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#J5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#J6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#J7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#J8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#J9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#J10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#J11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#J12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#J13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#J14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#J15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#J16");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#J17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#J18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#J19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#J20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#J21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#J22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#J23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#J24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#J25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#J26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#J27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#J28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#J29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#J30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#J31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#J32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#J33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#J34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#J35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#J36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#K1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#K2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#K3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#K4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#K5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#K6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#K7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#K8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#K9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#K10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#K11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#K12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#K13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#K14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#K15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#K16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#K17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#K18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#K19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#K20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#K21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#K22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#K23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#K24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#K25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#K26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#K27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#K28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#K29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#K30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#K31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#K32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#K33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#K34");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#K35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#K36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#L1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#L2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#L3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#L4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#L5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#L6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#L7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#L8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#L9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#L10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#L11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#L12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#L13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#L14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#L15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#L16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#L17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#L18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#L19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#L20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#L21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#L22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#L23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#L24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#L25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#L26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#L27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#L28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#L29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#L30");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#L31");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#L32");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#L33");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#L34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#L35");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#L36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#M1");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#M2");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#M3");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#M4");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#M5");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#M6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#M7");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#M8");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#M9");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#M10");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#M11");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#M12");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#M13");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#M14");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#M15");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#M16");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#M17");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#M18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#M19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#M20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#M21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#M22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#M23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#M24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#M25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#M26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#M27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#M28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#M29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#M30");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#M31");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#M32");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#M33");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#M34");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#M35");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#M36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;

			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceBlue", flags), "#N1");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceBlue", flags), "#N2");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceBlue", flags), "#N3");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceBlue", flags), "#N4");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceBlue", flags), "#N5");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceBlue", flags), "#N6");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceRed", flags), "#N7");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceRed", flags), "#N8");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceRed", flags), "#N9");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceRed", flags), "#N10");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceRed", flags), "#N11");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceRed", flags), "#N12");
			Assert.IsNull (greenType.GetMethod ("GetPrivateInstanceGreen", flags), "#N13");
			Assert.IsNull (greenType.GetMethod ("GetFamilyInstanceGreen", flags), "#N14");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemInstanceGreen", flags), "#N15");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemInstanceGreen", flags), "#N16");
			Assert.IsNull (greenType.GetMethod ("GetPublicInstanceGreen", flags), "#N17");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyInstanceGreen", flags), "#N18");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticBlue", flags), "#N19");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticBlue", flags), "#N20");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticBlue", flags), "#N21");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticBlue", flags), "#N22");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticBlue", flags), "#N23");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticBlue", flags), "#N24");
			Assert.IsNull (greenType.GetMethod ("GetPrivateStaticRed", flags), "#N25");
			Assert.IsNull (greenType.GetMethod ("GetFamilyStaticRed", flags), "#N26");
			Assert.IsNull (greenType.GetMethod ("GetFamANDAssemStaticRed", flags), "#N27");
			Assert.IsNull (greenType.GetMethod ("GetFamORAssemStaticRed", flags), "#N28");
			Assert.IsNull (greenType.GetMethod ("GetPublicStaticRed", flags), "#N29");
			Assert.IsNull (greenType.GetMethod ("GetAssemblyStaticRed", flags), "#N30");
			Assert.IsNotNull (greenType.GetMethod ("GetPrivateStaticGreen", flags), "#N31");
			Assert.IsNotNull (greenType.GetMethod ("GetFamilyStaticGreen", flags), "#N32");
			Assert.IsNotNull (greenType.GetMethod ("GetFamANDAssemStaticGreen", flags), "#N33");
			Assert.IsNotNull (greenType.GetMethod ("GetFamORAssemStaticGreen", flags), "#N34");
			Assert.IsNotNull (greenType.GetMethod ("GetPublicStaticGreen", flags), "#N35");
			Assert.IsNotNull (greenType.GetMethod ("GetAssemblyStaticGreen", flags), "#N36");
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetMethodsIncomplete_Mono ()
		{
			MethodBuilder mb;
			ILGenerator ilgen;

			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Abstract);
			mb = tb.DefineMethod ("Hello", MethodAttributes.Public,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Run", MethodAttributes.Private,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Execute", MethodAttributes.Public |
				MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Init", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			MethodInfo [] methods = tb.GetMethods ();
			Assert.AreEqual (7, methods.Length, "#A");

			Assert.AreEqual ("Equals", methods [0].Name, "#B1");
			Assert.IsFalse (methods [0].IsStatic, "#B2");
			Assert.IsFalse (methods [0].IsAbstract, "#B3");

			Assert.AreEqual ("GetHashCode", methods [1].Name, "#C1");
			Assert.IsFalse (methods [1].IsStatic, "#C2");
			Assert.IsFalse (methods [1].IsAbstract, "#C3");

			Assert.AreEqual ("GetType", methods [2].Name, "#D1");
			Assert.IsFalse (methods [2].IsStatic, "#D2");
			Assert.IsFalse (methods [2].IsAbstract, "#D3");

			Assert.AreEqual ("ToString", methods [3].Name, "#E1");
			Assert.IsFalse (methods [3].IsStatic, "#E2");
			Assert.IsFalse (methods [3].IsAbstract, "#E3");

			Assert.AreEqual ("Hello", methods [4].Name, "#F1");
			Assert.IsFalse (methods [4].IsStatic, "#F2");
			Assert.IsFalse (methods [4].IsAbstract, "#F3");

			Assert.AreEqual ("Execute", methods [5].Name, "#G1");
			Assert.IsTrue (methods [5].IsStatic, "#G2");
			Assert.IsFalse (methods [5].IsAbstract, "#G3");

			Assert.AreEqual ("Init", methods [6].Name, "#H1");
			Assert.IsFalse (methods [6].IsStatic, "#H2");
			Assert.IsTrue (methods [6].IsAbstract, "#H3");
		}

		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetMethodsIncomplete_MS ()
		{
			MethodBuilder mb;
			ILGenerator ilgen;

			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Abstract);
			mb = tb.DefineMethod ("Hello", MethodAttributes.Public,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Run", MethodAttributes.Private,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Execute", MethodAttributes.Public |
				MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Init", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			try {
				tb.GetMethods ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMethodsComplete ()
		{
			MethodBuilder mb;
			ILGenerator ilgen;
			MethodInfo mi;

			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Abstract);
			mb = tb.DefineMethod ("Hello", MethodAttributes.Public,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldstr, "Hi! ");
			ilgen.Emit (OpCodes.Ldarg_1);
			MethodInfo infoMethod = typeof (string).GetMethod ("Concat",
				new Type [] { typeof (string), typeof (string) });
			ilgen.Emit (OpCodes.Call, infoMethod);
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Run", MethodAttributes.Private,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Execute", MethodAttributes.Public |
				MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Init", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			Type emittedType = tb.CreateType ();

			MethodInfo [] methods = emittedType.GetMethods ();
			Assert.AreEqual (7, methods.Length, "#A1");
			Assert.AreEqual (7, tb.GetMethods ().Length, "#A2");

			mi = GetMethodByName (methods, "Hello");
			Assert.IsNotNull (mi, "#B1");
			Assert.IsFalse (mi.IsStatic, "#B2");
			Assert.IsFalse (mi.IsAbstract, "#B3");

			mi = GetMethodByName (methods, "Execute");
			Assert.IsNotNull (mi, "#C1");
			Assert.IsTrue (mi.IsStatic, "#C2");
			Assert.IsFalse (mi.IsAbstract, "#C3");

			mi = GetMethodByName (methods, "Init");
			Assert.IsNotNull (mi, "#D1");
			Assert.IsFalse (mi.IsStatic, "#D2");
			Assert.IsTrue (mi.IsAbstract, "#D3");

			mi = GetMethodByName (methods, "GetType");
			Assert.IsNotNull (mi, "#E1");
			Assert.IsFalse (methods [3].IsStatic, "#E2");
			Assert.IsFalse (methods [3].IsAbstract, "#E3");

			mi = GetMethodByName (methods, "ToString");
			Assert.IsNotNull (mi, "#F1");
			Assert.IsFalse (mi.IsStatic, "#F2");
			Assert.IsFalse (mi.IsAbstract, "#F3");

			mi = GetMethodByName (methods, "Equals");
			Assert.IsNotNull (mi, "#G1");
			Assert.IsFalse (mi.IsStatic, "#G2");
			Assert.IsFalse (mi.IsAbstract, "#G3");

			mi = GetMethodByName (methods, "GetHashCode");
			Assert.IsNotNull (mi, "#H1");
			Assert.IsFalse (mi.IsStatic, "#H2");
			Assert.IsFalse (mi.IsAbstract, "#H3");
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetMethodsFlagsIncomplete_Inheritance ()
		{
			MethodInfo [] methods;
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#A1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#A2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#A3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#A4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#A5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#A6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#A7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#A8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#A9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#A10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#A11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#A12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#A13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#A14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#A15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#A16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#A17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#A18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#A19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#A20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#A21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#A22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#A23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#A24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#A25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#A26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#A27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#A28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#A29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#A30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#A31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#A32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#A33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#A34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#A35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#A36");

			flags = BindingFlags.Instance | BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#B1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#B2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#B3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#B4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#B5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#B6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#B7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#B8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#B9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#B10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#B11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#B12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#B13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#B14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#B15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#B16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#B17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#B18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#B19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#B20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#B21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#B22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#B23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#B24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#B25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#B26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#B27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#B28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#B29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#B30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#B31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#B32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#B33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#B34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#B35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#B36");

			flags = BindingFlags.Static | BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#C1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#C2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#C3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#C4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#C5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#C6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#C7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#C8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#C9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#C10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#C11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#C12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#C13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#C14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#C15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#C16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#C17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#C18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#C19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#C20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#C21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#C22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#C23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#C24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#C25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#C26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#C27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#C28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#C29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#C30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#C31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#C32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#C33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#C34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#C35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#C36");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#D1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#D2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#D3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#D4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#D5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#D6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#D7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#D8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#D9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#D10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#D11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#D12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#D13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#D14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#D15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#D16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#D17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#D18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#D19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#D20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#D21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#D22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#D23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#D24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#D25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#D26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#D27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#D28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#D29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#D30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#D31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#D32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#D33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#D34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#D35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#D36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#E1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#E2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#E3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#E4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#E5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#E6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#E7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#E8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#E9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#E10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#E11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#E12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#E13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#E14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#E15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#E16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#E17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#E18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#E19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#E20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#E21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#E22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#E23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#E24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#E25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#E26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#E27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#E28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#E29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#E30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#E31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#E32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#E33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#E34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#E35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#E36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#F1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#F2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#F3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#F4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#F5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#F6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#F7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#F8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#F9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#F10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#F11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#F12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#F13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#F14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#F15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#F16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#F17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#F18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#F19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#F20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#F21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#F22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#F23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#F24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#F25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#F26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#F27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#F28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#F29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#F30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#F31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#F32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#F33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#F34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#F35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#F36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#G1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#G2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#G3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#G4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#G5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#G6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#G7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#G8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#G9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#G10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#G11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#G12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#G13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#G14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#G15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#G16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#G17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#G18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#G19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#G20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#G21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#G22");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#G23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#G24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#G25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#G26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#G27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#G28");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticRed"), "#G29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#G30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#G31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#G32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#G33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#G34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#G35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#G36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#H1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#H2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#H3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#H4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#H5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#H6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#H7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#H8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#H9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#H10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#H11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#H12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#H13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#H14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#H15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#H16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#H17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#H18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#H19");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#H20");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#H21");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#H22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#H23");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#H24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#H25");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#H26");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#H27");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#H28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#H29");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#H30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#H31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#H32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#H33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#H34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#H35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#H36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#I1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#I2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#I3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#I4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#I5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#I6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#I7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#I8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#I9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#I10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#I11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#I12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#I13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#I14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#I15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#I16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#I17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#I18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#I19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#I20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#I21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#I22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#I23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#I24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#I25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#I26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#I27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#I28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#I29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#I30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#I31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#I32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#I33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#I34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#I35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#I36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#J1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#J2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#J3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#J4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#J5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#J6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#J7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#J8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#J9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#J10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#J11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#J12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#J13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#J14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#J15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#J16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#J17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#J18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#J19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#J20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#J21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#J22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#J23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#J24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#J25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#J26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#J27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#J28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#J29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#J30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#J31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#J32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#J33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#J34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#J35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#J36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#K1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#K2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#K3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#K4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#K5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#K6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#K7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#K8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#K9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#K10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#K11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#K12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#K13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#K14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#K15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#K16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#K17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#K18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#K19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#K20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#K21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#K22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#K23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#K24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#K25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#K26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#K27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#K28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#K29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#K30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#K31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#K32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#K33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#K34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#K35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#K36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#L1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#L2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#L3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#L4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#L5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#L6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#L7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#L8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#L9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#L10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#L11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#L12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#L13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#L14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#L15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#L16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#L17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#L18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#L19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#L20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#L21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#L22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#L23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#L24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#L25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#L26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#L27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#L28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#L29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#L30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#L31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#L32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#L33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#L34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#L35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#L36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#M1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#M2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#M3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#M4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#M5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#M6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#M7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#M8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#M9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#M10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#M11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#M12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#M13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#M14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#M15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#M16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#M17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#M18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#M19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#M20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#M21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#M22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#M23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#M24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#M25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#M26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#M27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#M28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#M29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#M30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#M31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#M32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#M33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#M34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#M35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#M36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#N1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#N2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#N3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#N4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#N5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#N6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#N7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#N8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#N9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#N10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#N11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#N12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#N13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#N14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#N15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#N16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#N17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#N18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#N19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#N20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#N21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#N22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#N23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#N24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#N25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#N26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#N27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#N28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#N29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#N30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#N31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#N32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#N33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#N34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#N35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#N36");
		}

		[Test]
		[Category ("NotDotNet")] // mcs depends on this
		public void TestGetMethodsFlagsIncomplete_Mono ()
		{
			MethodBuilder mb;
			ILGenerator ilgen;
			MethodInfo [] methods;

			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Abstract);
			mb = tb.DefineMethod ("Hello", MethodAttributes.Public,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Run", MethodAttributes.Private,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Execute", MethodAttributes.Public |
				MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Init", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			methods = tb.GetMethods (BindingFlags.Public |
				BindingFlags.Instance);
			Assert.AreEqual (6, methods.Length, "#A1");
			Assert.IsNotNull (GetMethodByName (methods, "Hello"), "#A2");
			Assert.IsNotNull (GetMethodByName (methods, "Init"), "#A3");
			Assert.IsNotNull (GetMethodByName (methods, "ToString"), "#A4");
			Assert.IsNotNull (GetMethodByName (methods, "Equals"), "#A5");
			Assert.IsNotNull (GetMethodByName (methods, "GetHashCode"), "#A6");

			methods = tb.GetMethods (BindingFlags.Public |
				BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert.AreEqual (2, methods.Length, "#B1");
			Assert.IsNotNull (GetMethodByName (methods, "Hello"), "#B2");
			Assert.IsNotNull (GetMethodByName (methods, "Init"), "#B3");

			methods = tb.GetMethods (BindingFlags.Public |
				BindingFlags.Instance | BindingFlags.Static);
			Assert.AreEqual (7, methods.Length, "#C1");
			Assert.IsNotNull (GetMethodByName (methods, "Hello"), "#C2");
			Assert.IsNotNull (GetMethodByName (methods, "Init"), "#C3");
			Assert.IsNotNull (GetMethodByName (methods, "Execute"), "#C4");
			Assert.IsNotNull (GetMethodByName (methods, "ToString"), "#C5");
			Assert.IsNotNull (GetMethodByName (methods, "Equals"), "#C6");
			Assert.IsNotNull (GetMethodByName (methods, "GetHashCode"), "#C7");

			methods = tb.GetMethods (BindingFlags.NonPublic |
				BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert.AreEqual (1, methods.Length, "#D1");
			Assert.IsNotNull (GetMethodByName (methods, "Run"), "#D2");
		}


		[Test]
		[Category ("NotWorking")] // mcs depends on this
		public void TestGetMethodsFlagsIncomplete_MS ()
		{
			MethodBuilder mb;
			ILGenerator ilgen;

			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Abstract);
			mb = tb.DefineMethod ("Hello", MethodAttributes.Public,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Run", MethodAttributes.Private,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Execute", MethodAttributes.Public |
				MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("Init", MethodAttributes.Public |
				MethodAttributes.Abstract | MethodAttributes.Virtual,
				typeof (void), Type.EmptyTypes);

			try {
				tb.GetMethods (BindingFlags.Public | BindingFlags.Instance);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMethodsFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			MethodBuilder helloMethod = tb.DefineMethod ("HelloMethod",
				MethodAttributes.Public, typeof (string), Type.EmptyTypes);
			ILGenerator helloMethodIL = helloMethod.GetILGenerator ();
			helloMethodIL.Emit (OpCodes.Ldstr, "Hi! ");
			helloMethodIL.Emit (OpCodes.Ldarg_1);
			MethodInfo infoMethod = typeof (string).GetMethod ("Concat",
				new Type [] { typeof (string), typeof (string) });
			helloMethodIL.Emit (OpCodes.Call, infoMethod);
			helloMethodIL.Emit (OpCodes.Ret);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Length, "#1");
			Assert.AreEqual (tb.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length, "#2");
			Assert.AreEqual (0, tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length, "#3");
			Assert.AreEqual (tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length, "#4");
		}

		[Test]
		public void TestGetMethodsFlagsComplete_Inheritance ()
		{
			MethodInfo [] methods;
			BindingFlags flags;

			TypeBuilder blueType = module.DefineType (genTypeName (),
				TypeAttributes.Public);
			CreateMembers (blueType, "Blue", false);

			TypeBuilder redType = module.DefineType (genTypeName (),
				TypeAttributes.Public, blueType);
			CreateMembers (redType, "Red", false);

			TypeBuilder greenType = module.DefineType (genTypeName (),
				TypeAttributes.Public, redType);
			CreateMembers (greenType, "Green", false);

			blueType.CreateType ();
			redType.CreateType ();
			greenType.CreateType ();

			flags = BindingFlags.Instance | BindingFlags.NonPublic;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#A1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#A2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#A3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#A4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#A5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#A6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#A7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#A8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#A9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#A10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#A11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#A12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#A13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#A14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#A15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#A16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#A17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#A18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#A19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#A20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#A21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#A22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#A23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#A24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#A25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#A26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#A27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#A28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#A29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#A30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#A31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#A32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#A33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#A34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#A35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#A36");

			flags = BindingFlags.Instance | BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#B1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#B2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#B3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#B4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#B5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#B6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#B7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#B8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#B9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#B10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#B11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#B12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#B13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#B14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#B15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#B16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#B17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#B18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#B19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#B20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#B21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#B22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#B23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#B24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#B25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#B26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#B27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#B28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#B29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#B30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#B31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#B32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#B33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#B34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#B35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#B36");

			flags = BindingFlags.Static | BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#C1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#C2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#C3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#C4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#C5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#C6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#C7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#C8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#C9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#C10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#C11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#C12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#C13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#C14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#C15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#C16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#C17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#C18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#C19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#C20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#C21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#C22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#C23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#C24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#C25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#C26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#C27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#C28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#C29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#C30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#C31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#C32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#C33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#C34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#C35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#C36");

			flags = BindingFlags.Static | BindingFlags.NonPublic;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#D1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#D2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#D3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#D4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#D5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#D6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#D7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#D8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#D9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#D10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#D11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#D12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#D13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#D14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#D15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#D16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#D17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#D18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#D19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#D20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#D21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#D22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#D23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#D24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#D25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#D26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#D27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#D28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#D29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#D30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#D31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#D32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#D33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#D34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#D35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#D36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#E1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#E2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#E3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#E4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#E5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#E6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#E7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#E8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#E9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#E10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#E11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#E12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#E13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#E14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#E15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#E16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#E17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#E18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#E19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#E20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#E21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#E22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#E23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#E24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#E25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#E26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#E27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#E28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#E29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#E30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#E31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#E32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#E33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#E34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#E35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#E36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#F1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#F2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#F3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#F4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#F5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#F6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#F7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#F8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#F9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#F10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#F11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#F12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#F13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#F14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#F15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#F16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#F17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#F18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#F19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#F20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#F21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#F22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#F23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#F24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#F25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#F26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#F27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#F28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#F29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#F30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#F31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#F32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#F33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#F34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#F35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#F36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#G1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#G2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#G3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#G4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#G5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#G6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#G7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#G8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#G9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#G10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#G11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#G12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#G13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#G14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#G15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#G16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#G17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#G18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#G19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#G20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#G21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#G22");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#G23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#G24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#G25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#G26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#G27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#G28");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticRed"), "#G29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#G30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#G31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#G32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#G33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#G34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#G35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#G36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.FlattenHierarchy;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#H1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#H2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#H3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#H4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#H5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#H6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#H7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#H8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#H9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#H10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#H11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#H12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#H13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#H14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#H15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#H16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#H17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#H18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#H19");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#H20");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#H21");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#H22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#H23");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#H24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#H25");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#H26");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#H27");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#H28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#H29");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#H30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#H31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#H32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#H33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#H34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#H35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#H36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#I1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#I2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#I3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#I4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#I5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#I6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#I7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#I8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#I9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#I10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#I11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#I12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#I13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#I14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#I15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#I16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#I17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#I18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#I19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#I20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#I21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#I22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#I23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#I24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#I25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#I26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#I27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#I28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#I29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#I30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#I31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#I32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#I33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#I34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#I35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#I36");

			flags = BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#J1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#J2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#J3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#J4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#J5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#J6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#J7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#J8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#J9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#J10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#J11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#J12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#J13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#J14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#J15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#J16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#J17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#J18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#J19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#J20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#J21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#J22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#J23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#J24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#J25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#J26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#J27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#J28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#J29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#J30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#J31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#J32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#J33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#J34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#J35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#J36");

			flags = BindingFlags.Static | BindingFlags.Public |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#K1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#K2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#K3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#K4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#K5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#K6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#K7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#K8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#K9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#K10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#K11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#K12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#K13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#K14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#K15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#K16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#K17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#K18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#K19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#K20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#K21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#K22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#K23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#K24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#K25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#K26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#K27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#K28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#K29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#K30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#K31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#K32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#K33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#K34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#K35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#K36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#L1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#L2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#L3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#L4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#L5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#L6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#L7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#L8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#L9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#L10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#L11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#L12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#L13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#L14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#L15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#L16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#L17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#L18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#L19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#L20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#L21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#L22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#L23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#L24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#L25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#L26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#L27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#L28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#L29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#L30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#L31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#L32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#L33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#L34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#L35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#L36");

			flags = BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#M1");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#M2");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#M3");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#M4");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#M5");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#M6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#M7");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#M8");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#M9");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#M10");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#M11");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#M12");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#M13");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#M14");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#M15");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#M16");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#M17");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#M18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#M19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#M20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#M21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#M22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#M23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#M24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#M25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#M26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#M27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#M28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#M29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#M30");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#M31");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#M32");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#M33");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#M34");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#M35");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#M36");

			flags = BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public;
			methods = greenType.GetMethods (flags);

			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceBlue"), "#N1");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceBlue"), "#N2");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceBlue"), "#N3");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceBlue"), "#N4");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceBlue"), "#N5");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceBlue"), "#N6");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceRed"), "#N7");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceRed"), "#N8");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceRed"), "#N9");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceRed"), "#N10");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceRed"), "#N11");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceRed"), "#N12");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateInstanceGreen"), "#N13");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyInstanceGreen"), "#N14");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemInstanceGreen"), "#N15");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemInstanceGreen"), "#N16");
			Assert.IsNull (GetMethodByName (methods, "GetPublicInstanceGreen"), "#N17");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyInstanceGreen"), "#N18");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticBlue"), "#N19");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticBlue"), "#N20");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticBlue"), "#N21");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticBlue"), "#N22");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticBlue"), "#N23");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticBlue"), "#N24");
			Assert.IsNull (GetMethodByName (methods, "GetPrivateStaticRed"), "#N25");
			Assert.IsNull (GetMethodByName (methods, "GetFamilyStaticRed"), "#N26");
			Assert.IsNull (GetMethodByName (methods, "GetFamANDAssemStaticRed"), "#N27");
			Assert.IsNull (GetMethodByName (methods, "GetFamORAssemStaticRed"), "#N28");
			Assert.IsNull (GetMethodByName (methods, "GetPublicStaticRed"), "#N29");
			Assert.IsNull (GetMethodByName (methods, "GetAssemblyStaticRed"), "#N30");
			Assert.IsNotNull (GetMethodByName (methods, "GetPrivateStaticGreen"), "#N31");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamilyStaticGreen"), "#N32");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamANDAssemStaticGreen"), "#N33");
			Assert.IsNotNull (GetMethodByName (methods, "GetFamORAssemStaticGreen"), "#N34");
			Assert.IsNotNull (GetMethodByName (methods, "GetPublicStaticGreen"), "#N35");
			Assert.IsNotNull (GetMethodByName (methods, "GetAssemblyStaticGreen"), "#N36");
		}

		[Test]
		public void TestGetMemberIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetMember ("FOO", MemberTypes.All, BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMemberComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("FOO", typeof (int), FieldAttributes.Private);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetMember ("FOO", MemberTypes.Field, BindingFlags.Instance | BindingFlags.NonPublic).Length);
			Assert.AreEqual (0, tb.GetMember ("FOO", MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public).Length);
		}

		[Test]
		public void TestGetMembersIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetMembers ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMembersComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Type emittedType = tb.CreateType ();

			Assert.AreEqual (tb.GetMembers ().Length, emittedType.GetMembers ().Length);
		}

		[Test]
		public void TestGetMembersFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetMembers (BindingFlags.Public);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetMembersFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineField ("FOO", typeof (int), FieldAttributes.Public);

			Type emittedType = tb.CreateType ();

			Assert.IsTrue (tb.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length != 0);
			Assert.AreEqual (tb.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetMembers (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (tb.GetMembers (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetMembers (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		}

		[Test]
		public void TestGetInterfaceIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetInterface ("FOO", true);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// The invoked member is not supported in a
				// dynamic module
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestGetInterfaces ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Type [] interfaces = tb.GetInterfaces ();
			Assert.AreEqual (0, interfaces.Length);

			TypeBuilder tbInterface = module.DefineType (genTypeName (), TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
			Type emittedInterface = tbInterface.CreateType ();

			tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object), new Type [] { emittedInterface });
			interfaces = tb.GetInterfaces ();
			Assert.AreEqual (1, interfaces.Length);
		}

		[Test]
		public void TestAddDeclarativeSecurityAlreadyCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();

			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			try {
				tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestAddDeclarativeSecurityNullPermissionSet ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.AddDeclarativeSecurity (SecurityAction.Demand, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("pset", ex.ParamName, "#5");
			}

		}

		[Test]
		public void TestAddDeclarativeSecurityInvalidAction ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			SecurityAction [] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

			foreach (SecurityAction action in actions) {
				try {
					tb.AddDeclarativeSecurity (action, set);
					Assert.Fail ();
				} catch (ArgumentOutOfRangeException) {
				}
			}
		}

		[Test]
		public void TestAddDeclarativeSecurityDuplicateAction ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			try {
				tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Multiple permission sets specified with the
				// same SecurityAction
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void TestEnums ()
		{
			TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
			TypeBuilder enumToCreate = module.DefineType (genTypeName (), typeAttrs,
														 typeof (Enum));
			enumToCreate.SetCustomAttribute (new CustomAttributeBuilder (typeof (FlagsAttribute).GetConstructors () [0], Type.EmptyTypes));
			// add value__ field, see DefineEnum method of ModuleBuilder
			enumToCreate.DefineField ("value__", typeof (Int32),
				FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName);

			// add enum entries
			FieldBuilder fb = enumToCreate.DefineField ("A", enumToCreate,
				FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			fb.SetConstant ((Int32) 0);

			fb = enumToCreate.DefineField ("B", enumToCreate,
				FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			fb.SetConstant ((Int32) 1);

			fb = enumToCreate.DefineField ("C", enumToCreate,
				FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal);
			fb.SetConstant ((Int32) 2);

			Type enumType = enumToCreate.CreateType ();

			object enumVal = Enum.ToObject (enumType, (Int32) 3);

			Assert.AreEqual ("B, C", enumVal.ToString ());
			Assert.AreEqual (3, (Int32) enumVal);
		}

		[Test]
		public void DefineEnum ()
		{
			TypeBuilder typeBuilder = module.DefineType (genTypeName (),
														 TypeAttributes.Public);
			EnumBuilder enumBuilder = module.DefineEnum (genTypeName (),
														 TypeAttributes.Public, typeof (int));
			typeBuilder.DefineField ("myField", enumBuilder, FieldAttributes.Private);
			enumBuilder.CreateType ();
			typeBuilder.CreateType ();
		}

		[Test]
		[Category ("NotWorking")]
		public void DefineEnumThrowIfTypeBuilderCalledBeforeEnumBuilder ()
		{
			TypeBuilder typeBuilder = module.DefineType (genTypeName (),
														 TypeAttributes.Public);
			EnumBuilder enumBuilder = module.DefineEnum (genTypeName (),
														 TypeAttributes.Public, typeof (int));
			typeBuilder.DefineField ("myField", enumBuilder, FieldAttributes.Private);
			try {
				typeBuilder.CreateType ();
				Assert.Fail ("#1");
			} catch (TypeLoadException) {
				// Could not load type '...' from assembly
				// 'MonoTests.System.Reflection.Emit.TypeBuilderTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
			}
			Assert.IsTrue (typeBuilder.IsCreated (), "#2");
			Assert.IsNull (typeBuilder.CreateType (), "#3");
		}

		[Test]
		public void SetCustomAttribute_SuppressUnmanagedCodeSecurity ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorInfo attrCtor = typeof (SuppressUnmanagedCodeSecurityAttribute).
				GetConstructor (Type.EmptyTypes);
			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				attrCtor, new object [0]);
			Assert.IsTrue ((tb.Attributes & TypeAttributes.HasSecurity) == 0, "#1");
			tb.SetCustomAttribute (caBuilder);
			//Assert.IsTrue ((tb.Attributes & TypeAttributes.HasSecurity) == 0, "#2");
			Type emittedType = tb.CreateType ();
			Assert.AreEqual (TypeAttributes.HasSecurity, emittedType.Attributes & TypeAttributes.HasSecurity, "#3");
			//Assert.IsTrue ((tb.Attributes & TypeAttributes.HasSecurity) == 0, "#4");
			object [] emittedAttrs = emittedType.GetCustomAttributes (typeof (SuppressUnmanagedCodeSecurityAttribute), true);
			Assert.AreEqual (1, emittedAttrs.Length, "#5");
		}

		private PropertyBuilder DefineStringProperty (TypeBuilder tb, string propertyName, string fieldName, MethodAttributes methodAttribs)
		{
			// define the field holding the property value
			FieldBuilder fieldBuilder = tb.DefineField (fieldName,
				typeof (string), FieldAttributes.Private);

			PropertyBuilder propertyBuilder = tb.DefineProperty (
				propertyName, PropertyAttributes.HasDefault, typeof (string),
				new Type [] { typeof (string) });

			// First, we'll define the behavior of the "get" property for CustomerName as a method.
			MethodBuilder getMethodBuilder = tb.DefineMethod ("Get" + propertyName,
									methodAttribs,
									typeof (string),
									new Type [] { });

			ILGenerator getIL = getMethodBuilder.GetILGenerator ();

			getIL.Emit (OpCodes.Ldarg_0);
			getIL.Emit (OpCodes.Ldfld, fieldBuilder);
			getIL.Emit (OpCodes.Ret);

			// Now, we'll define the behavior of the "set" property for CustomerName.
			MethodBuilder setMethodBuilder = tb.DefineMethod ("Set" + propertyName,
									methodAttribs,
									null,
									new Type [] { typeof (string) });

			ILGenerator setIL = setMethodBuilder.GetILGenerator ();

			setIL.Emit (OpCodes.Ldarg_0);
			setIL.Emit (OpCodes.Ldarg_1);
			setIL.Emit (OpCodes.Stfld, fieldBuilder);
			setIL.Emit (OpCodes.Ret);

			// Last, we must map the two methods created above to our PropertyBuilder to 
			// their corresponding behaviors, "get" and "set" respectively. 
			propertyBuilder.SetGetMethod (getMethodBuilder);
			propertyBuilder.SetSetMethod (setMethodBuilder);
			return propertyBuilder;
		}

		static int handler_called = 0;

		[Test]
		public void TestTypeResolve ()
		{
			string typeName = genTypeName ();

			ResolveEventHandler handler = new ResolveEventHandler (TypeResolve);
			AppDomain.CurrentDomain.TypeResolve += handler;
			handler_called = 0;
			Type t = Type.GetType (typeName);
			Assert.AreEqual (typeName, t.Name);
			Assert.AreEqual (1, handler_called);
			AppDomain.CurrentDomain.TypeResolve -= handler;
		}

		Assembly TypeResolve (object sender, ResolveEventArgs args)
		{
			TypeBuilder tb = module.DefineType (args.Name, TypeAttributes.Public);
			tb.CreateType ();
			handler_called++;
			return tb.Assembly;
		}

		[Test]
		public void IsAssignableFrom_Created ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (MemoryStream),
				new Type [] { typeof (IThrowable), typeof (Bar) });
			tb.AddInterfaceImplementation (typeof (IDestroyable));
			Type emitted_type = tb.CreateType ();

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb), "#A1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IThrowable)), "#A2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (tb), "#A3");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IMoveable)), "#A4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (tb), "#A5");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Foo)), "#A6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (tb), "#A7");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Bar)), "#A8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb), "#A9");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Baz)), "#A10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (tb), "#A11");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDestroyable)), "#A12");
			Assert.IsFalse (typeof (IAir).IsAssignableFrom (tb), "#A13");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IAir)), "#A14");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (tb), "#A15");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IWater)), "#A16");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (tb), "#A17");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (ILiquid)), "#A18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb), "#B1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (MemoryStream)), "#B2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb), "#B3");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Stream)), "#B4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb), "#B5");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (FileStream)), "#B6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb), "#B7");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (object)), "#B8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (tb), "#B9");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDisposable)), "#B10");

			Assert.IsTrue (tb.IsAssignableFrom (tb), "#C1");
			Assert.IsFalse (tb.IsAssignableFrom ((Type) null), "#C2");
			Assert.IsTrue (tb.IsAssignableFrom (emitted_type), "#C3");
			Assert.IsTrue (emitted_type.IsAssignableFrom (tb), "#C4");
			Assert.IsFalse (emitted_type.IsAssignableFrom ((Type) null), "#C5");

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (emitted_type), "#D1");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IThrowable)), "#D2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (emitted_type), "#D3");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IMoveable)), "#D4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (emitted_type), "#D5");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (Foo)), "#D6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (emitted_type), "#D7");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (Bar)), "#D8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (emitted_type), "#D9");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (Baz)), "#D10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (emitted_type), "#D11");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IDestroyable)), "#D12");
			Assert.IsFalse (typeof (IAir).IsAssignableFrom (emitted_type), "#D13");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IAir)), "#D14");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (emitted_type), "#D15");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IWater)), "#D16");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (emitted_type), "#D17");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (ILiquid)), "#D18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (emitted_type), "#E1");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (MemoryStream)), "#E2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (emitted_type), "#E3");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (Stream)), "#E4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (emitted_type), "#E5");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (FileStream)), "#E6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (emitted_type), "#E7");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (object)), "#E8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (emitted_type), "#E9");
			Assert.IsFalse (emitted_type.IsAssignableFrom (typeof (IDisposable)), "#E10");

			Assert.IsTrue (typeof (Foo []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#F1");
			Assert.IsTrue (typeof (Bar []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#F2");
			Assert.IsFalse (typeof (Baz []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#F3");

			TypeBuilder tb2 = module.DefineType (genTypeName (),
				TypeAttributes.Public, tb,
				new Type [] { typeof (IAir) });
			Type emitted_type2 = tb2.CreateType ();

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb2), "#G1");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IThrowable)), "#G2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (tb2), "#G3");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IMoveable)), "#G4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (tb2), "#G5");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Foo)), "#G6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (tb2), "#G7");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Bar)), "#G8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb2), "#G9");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Baz)), "#G10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (tb2), "#G11");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IDestroyable)), "#G12");
			Assert.IsTrue (typeof (IAir).IsAssignableFrom (tb2), "#G13");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IAir)), "#G14");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (tb2), "#G15");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IWater)), "#G16");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (tb2), "#G17");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (ILiquid)), "#G18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb2), "#H1");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (MemoryStream)), "#H2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb2), "#H3");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Stream)), "#H4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb2), "#H5");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (FileStream)), "#H6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb2), "#H7");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (object)), "#H8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (tb2), "#H9");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IDisposable)), "#H10");

			Assert.IsTrue (tb2.IsAssignableFrom (tb2), "#I1");
			Assert.IsFalse (tb2.IsAssignableFrom (tb), "#I2");
			Assert.IsTrue (tb2.IsAssignableFrom (emitted_type2), "#I3");
			Assert.IsFalse (tb2.IsAssignableFrom (emitted_type), "#I4");
			Assert.IsFalse (tb2.IsAssignableFrom ((Type) null), "#I5");
			Assert.IsTrue (emitted_type2.IsAssignableFrom (emitted_type2), "#I6");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (emitted_type), "#I7");
			Assert.IsTrue (emitted_type2.IsAssignableFrom (tb2), "#I8");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (tb), "#I9");
			Assert.IsFalse (emitted_type2.IsAssignableFrom ((Type) null), "#I10");
			Assert.IsTrue (tb.IsAssignableFrom (tb2), "#I11");
			Assert.IsTrue (tb.IsAssignableFrom (emitted_type2), "#I12");
			Assert.IsTrue (emitted_type.IsAssignableFrom (tb2), "#I13");
			Assert.IsTrue (emitted_type.IsAssignableFrom (emitted_type2), "#I14");

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (emitted_type2), "#J1");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IThrowable)), "#J2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (emitted_type2), "#J3");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IMoveable)), "#J4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (emitted_type2), "#J5");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (Foo)), "#J6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (emitted_type2), "#J7");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (Bar)), "#J8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (emitted_type2), "#J9");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (Baz)), "#J10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (tb2), "#J11");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IDestroyable)), "#J12");
			Assert.IsTrue (typeof (IAir).IsAssignableFrom (emitted_type2), "#J13");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IAir)), "#J14");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (emitted_type2), "#J15");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IWater)), "#J16");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (emitted_type2), "#J17");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (ILiquid)), "#J18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (emitted_type2), "#K1");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (MemoryStream)), "#K2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (emitted_type2), "#K3");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (Stream)), "#K4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (emitted_type2), "#K5");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (FileStream)), "#K6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (emitted_type2), "#K7");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (object)), "#K8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (emitted_type2), "#K9");
			Assert.IsFalse (emitted_type2.IsAssignableFrom (typeof (IDisposable)), "#K10");

			Assert.IsTrue (typeof (Foo []).IsAssignableFrom (module.GetType (
				tb2.FullName + "[]")), "#L1");
			Assert.IsTrue (typeof (Bar []).IsAssignableFrom (module.GetType (
				tb2.FullName + "[]")), "#L2");
			Assert.IsFalse (typeof (Baz []).IsAssignableFrom (module.GetType (
				tb2.FullName + "[]")), "#L3");

			TypeBuilder tb3 = module.DefineType (genTypeName (),
				TypeAttributes.Public, tb2,
				new Type [] { typeof (IWater) });
			Type emitted_type3 = tb3.CreateType ();

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb3), "#M1");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IThrowable)), "#M2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (tb3), "#M3");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IMoveable)), "#M4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (tb3), "#M5");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Foo)), "#M6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (tb3), "#M7");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Bar)), "#M8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb3), "#M9");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Baz)), "#M10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (tb3), "#M11");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IDestroyable)), "#M12");
			Assert.IsTrue (typeof (IAir).IsAssignableFrom (tb3), "#M13");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IAir)), "#M14");
			Assert.IsTrue (typeof (IWater).IsAssignableFrom (tb3), "#M15");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IWater)), "#M16");
			Assert.IsTrue (typeof (ILiquid).IsAssignableFrom (tb3), "#M17");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (ILiquid)), "#M18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb3), "#N1");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (MemoryStream)), "#N2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb3), "#N3");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Stream)), "#N4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb3), "#N5");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (FileStream)), "#N6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb3), "#N7");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (object)), "#N8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (tb3), "#N9");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IDisposable)), "#N10");

			Assert.IsTrue (tb3.IsAssignableFrom (tb3), "#O1");
			Assert.IsFalse (tb3.IsAssignableFrom (tb2), "#O2");
			Assert.IsFalse (tb3.IsAssignableFrom (tb), "#O3");
			Assert.IsTrue (tb3.IsAssignableFrom (emitted_type3), "#O4");
			Assert.IsFalse (tb3.IsAssignableFrom (emitted_type2), "#O5");
			Assert.IsFalse (tb3.IsAssignableFrom (emitted_type), "#O6");
			Assert.IsFalse (tb3.IsAssignableFrom ((Type) null), "#O7");
			Assert.IsTrue (emitted_type3.IsAssignableFrom (emitted_type3), "#O8");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (emitted_type2), "#O9");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (emitted_type), "#O10");
			Assert.IsTrue (emitted_type3.IsAssignableFrom (tb3), "#O11");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (tb2), "#O12");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (tb), "#O13");
			Assert.IsFalse (emitted_type3.IsAssignableFrom ((Type) null), "#O14");
			Assert.IsTrue (tb2.IsAssignableFrom (tb3), "#O15");
			Assert.IsTrue (tb2.IsAssignableFrom (emitted_type3), "#O16");
			Assert.IsTrue (emitted_type2.IsAssignableFrom (emitted_type3), "#O17");
			Assert.IsTrue (emitted_type2.IsAssignableFrom (tb3), "#O18");
			Assert.IsTrue (tb.IsAssignableFrom (tb3), "#O19");
			Assert.IsTrue (tb.IsAssignableFrom (emitted_type3), "#O20");
			Assert.IsTrue (emitted_type.IsAssignableFrom (tb3), "#021");
			Assert.IsTrue (emitted_type.IsAssignableFrom (emitted_type3), "#O22");

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (emitted_type3), "#P1");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IThrowable)), "#P2");
			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (emitted_type3), "#P3");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IMoveable)), "#P4");
			Assert.IsTrue (typeof (Foo).IsAssignableFrom (emitted_type3), "#P5");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (Foo)), "#P6");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (emitted_type3), "#P7");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (Bar)), "#P8");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (emitted_type3), "#P9");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (Baz)), "#P10");
			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (emitted_type3), "#P11");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IDestroyable)), "#P12");
			Assert.IsTrue (typeof (IAir).IsAssignableFrom (emitted_type3), "#P13");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IAir)), "#P14");
			Assert.IsTrue (typeof (IWater).IsAssignableFrom (emitted_type3), "#P15");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IWater)), "#P16");
			Assert.IsTrue (typeof (ILiquid).IsAssignableFrom (emitted_type3), "#P17");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (ILiquid)), "#P18");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (emitted_type3), "#Q1");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (MemoryStream)), "#Q2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (emitted_type3), "#Q3");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (Stream)), "#Q4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (emitted_type3), "#Q5");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (FileStream)), "#Q6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (emitted_type3), "#Q7");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (object)), "#Q8");
			Assert.IsTrue (typeof (IDisposable).IsAssignableFrom (emitted_type3), "#Q9");
			Assert.IsFalse (emitted_type3.IsAssignableFrom (typeof (IDisposable)), "#Q10");

			Assert.IsTrue (typeof (Foo []).IsAssignableFrom (module.GetType (
				tb3.FullName + "[]")), "#R1");
			Assert.IsTrue (typeof (Bar []).IsAssignableFrom (module.GetType (
				tb3.FullName + "[]")), "#R2");
			Assert.IsFalse (typeof (Baz []).IsAssignableFrom (module.GetType (
				tb3.FullName + "[]")), "#R3");

			TypeBuilder tb4 = module.DefineType (genTypeName (),
				TypeAttributes.Public, null,
				new Type [] { typeof (IWater) });
			tb4.DefineGenericParameters ("T");

			Type inst = tb4.MakeGenericType (typeof (int));
			Type emitted_type4 = tb4.CreateType ();
			Assert.IsFalse (typeof (IComparable).IsAssignableFrom (inst));
			// This returns True if CreateType () is called _before_ MakeGenericType...
			//Assert.IsFalse (typeof (IWater).IsAssignableFrom (inst));
		}

		[Test]
		public void IsAssignableFrom_NotCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (MemoryStream),
				new Type [] {
					typeof (IThrowable), typeof (Bar),
					typeof (IComparable)
					});

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb), "#A1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IThrowable)), "#A2");
			//Assert.IsFalse (typeof (IMoveable).IsAssignableFrom (tb), "#A3");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IMoveable)), "#A4");
			Assert.IsTrue (typeof (IComparable).IsAssignableFrom (tb), "#A5");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IComparable)), "#A6");
			Assert.IsFalse (typeof (IAir).IsAssignableFrom (tb), "#A7");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IAir)), "#A8");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (tb), "#A9");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IWater)), "#A10");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (tb), "#A11");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (ILiquid)), "#A12");

			//Assert.IsFalse (typeof (Foo).IsAssignableFrom (tb), "#B1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Foo)), "#B2");
			Assert.IsTrue (typeof (Bar).IsAssignableFrom (tb), "#B3");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Bar)), "#B4");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb), "#B5");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Baz)), "#B6");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb), "#C1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (MemoryStream)), "#C2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb), "#C3");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (Stream)), "#C4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb), "#C5");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (FileStream)), "#C6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb), "#C7");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (object)), "#C8");
			Assert.IsFalse (typeof (IDisposable).IsAssignableFrom (tb), "#C9");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDisposable)), "#C10");

			Assert.IsTrue (tb.IsAssignableFrom (tb), "#D1");
			Assert.IsFalse (tb.IsAssignableFrom ((Type) null), "#D2");

			TypeBuilder tb2 = module.DefineType (genTypeName (),
				TypeAttributes.Public, tb,
				new Type [] { typeof (IAir) });

			Assert.IsFalse (typeof (IThrowable).IsAssignableFrom (tb2), "#E1");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IThrowable)), "#E2");
			Assert.IsFalse (typeof (IMoveable).IsAssignableFrom (tb2), "#E3");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IMoveable)), "#E4");
			Assert.IsFalse (typeof (IComparable).IsAssignableFrom (tb2), "#E5");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IComparable)), "#E6");
			Assert.IsTrue (typeof (IAir).IsAssignableFrom (tb2), "#E7");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IAir)), "#E8");
			Assert.IsFalse (typeof (IWater).IsAssignableFrom (tb2), "#E9");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IWater)), "#E10");
			Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (tb2), "#E11");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (ILiquid)), "#E12");

			Assert.IsFalse (typeof (Foo).IsAssignableFrom (tb2), "#F1");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Foo)), "#F2");
			Assert.IsFalse (typeof (Bar).IsAssignableFrom (tb2), "#F3");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Bar)), "#F4");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb2), "#F5");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Baz)), "#F6");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb2), "#G1");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (MemoryStream)), "#G2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb2), "#G3");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (Stream)), "#G4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb2), "#G5");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (FileStream)), "#G6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb2), "#G7");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (object)), "#G8");
			Assert.IsFalse (typeof (IDisposable).IsAssignableFrom (tb2), "#G9");
			Assert.IsFalse (tb2.IsAssignableFrom (typeof (IDisposable)), "#G10");

			Assert.IsTrue (tb2.IsAssignableFrom (tb2), "#H1");
			Assert.IsFalse (tb2.IsAssignableFrom (tb), "#H2");
			Assert.IsTrue (tb.IsAssignableFrom (tb2), "#H3");

			TypeBuilder tb3 = module.DefineType (genTypeName (),
				TypeAttributes.Public, tb2,
				new Type [] { typeof (IWater) });

			Assert.IsFalse (typeof (IThrowable).IsAssignableFrom (tb3), "#I1");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IThrowable)), "#I2");
			Assert.IsFalse (typeof (IMoveable).IsAssignableFrom (tb3), "#I3");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IMoveable)), "#I4");
			Assert.IsFalse (typeof (IComparable).IsAssignableFrom (tb3), "#I5");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IComparable)), "#I6");
			Assert.IsFalse (typeof (IAir).IsAssignableFrom (tb3), "#I7");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IAir)), "#I8");
			Assert.IsTrue (typeof (IWater).IsAssignableFrom (tb3), "#I9");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IWater)), "#I10");
			//Assert.IsFalse (typeof (ILiquid).IsAssignableFrom (tb3), "#I11");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (ILiquid)), "#I12");

			Assert.IsFalse (typeof (Foo).IsAssignableFrom (tb3), "#J1");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Foo)), "#J2");
			Assert.IsFalse (typeof (Bar).IsAssignableFrom (tb3), "#J3");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Bar)), "#J4");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb3), "#J5");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Baz)), "#J6");

			Assert.IsTrue (typeof (MemoryStream).IsAssignableFrom (tb3), "#K1");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (MemoryStream)), "#K2");
			Assert.IsTrue (typeof (Stream).IsAssignableFrom (tb3), "#K3");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (Stream)), "#K4");
			Assert.IsFalse (typeof (FileStream).IsAssignableFrom (tb3), "#K5");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (FileStream)), "#K6");
			Assert.IsTrue (typeof (object).IsAssignableFrom (tb3), "#K7");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (object)), "#K8");
			Assert.IsFalse (typeof (IDisposable).IsAssignableFrom (tb3), "#K9");
			Assert.IsFalse (tb3.IsAssignableFrom (typeof (IDisposable)), "#K10");

			Assert.IsTrue (tb3.IsAssignableFrom (tb3), "#L1");
			Assert.IsFalse (tb3.IsAssignableFrom (tb2), "#L2");
			Assert.IsFalse (tb3.IsAssignableFrom (tb), "#L3");
			Assert.IsTrue (tb2.IsAssignableFrom (tb3), "#L4");
			Assert.IsTrue (tb.IsAssignableFrom (tb3), "#L5");
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344549
		public void IsAssignableFrom_NotCreated_AddInterfaceImplementation_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (FormatException),
				new Type [] { typeof (IThrowable) });
			tb.AddInterfaceImplementation (typeof (IDestroyable));

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb), "#A1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IThrowable)), "#A2");

			Assert.IsTrue (typeof (IDestroyable).IsAssignableFrom (tb), "#B1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDestroyable)), "#B2");
		}

		[Test]
		[Category ("NotWorking")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344549
		public void IsAssignableFrom_NotCreated_AddInterfaceImplementation_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (FormatException),
				new Type [] { typeof (IThrowable) });
			tb.AddInterfaceImplementation (typeof (IDestroyable));

			Assert.IsTrue (typeof (IThrowable).IsAssignableFrom (tb), "#A1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IThrowable)), "#A2");

			Assert.IsFalse (typeof (IDestroyable).IsAssignableFrom (tb), "#B1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDestroyable)), "#B2");
		}


		[Test]
		[Category ("NotDotNet")]
		public void IsAssignableFrom_NotCreated_Array ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (FormatException),
				new Type [] {
					typeof (IThrowable), typeof (Bar),
					typeof (IComparable)
					});

			Assert.IsTrue (typeof (Foo []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#1");
			Assert.IsTrue (typeof (Bar []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#2");
			Assert.IsFalse (typeof (Baz []).IsAssignableFrom (module.GetType (
				tb.FullName + "[]")), "#3");
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344353
		public void IsAssignableFrom_NotCreated_BaseInterface_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (FormatException),
				new Type [] {
					typeof (IThrowable), typeof (Bar),
					typeof (IComparable)
					});

			Assert.IsTrue (typeof (IMoveable).IsAssignableFrom (tb), "#1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IMoveable)), "#2");
		}

		[Test]
		[Category ("NotWorking")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=344353
		public void IsAssignableFrom_NotCreated_BaseInterface_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Public, typeof (FormatException),
				new Type [] {
					typeof (IThrowable), typeof (Bar),
					typeof (IComparable)
					});

			Assert.IsFalse (typeof (IMoveable).IsAssignableFrom (tb), "#1");
			Assert.IsFalse (tb.IsAssignableFrom (typeof (IMoveable)), "#2");
		}

		[Test]
		public void CreateType_EmptyMethodBody ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			tb.DefineMethod ("foo", MethodAttributes.Public, typeof (void), new Type [] { });
			try {
				tb.CreateType ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void CreateType_EmptyCtorBody ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			tb.DefineConstructor (0, CallingConventions.Standard, null);
			try {
				tb.CreateType ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void CreateType_Interface_ParentInvalid ()
		{
			TypeBuilder tb;

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface,
				typeof (Exception));
			Assert.AreEqual (typeof (Exception), tb.BaseType, "#A1");
			try {
				tb.CreateType ();
				Assert.Fail ("#A2");
			} catch (TypeLoadException ex) {
				// Could not load interface 't5' from assembly '...'
				// because it must extend from Object
				Assert.AreEqual (typeof (TypeLoadException), ex.GetType (), "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (tb.Name) != -1, "#A6");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#A7");
			}

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface,
				typeof (object));
			Assert.AreEqual (typeof (object), tb.BaseType, "#B1");
			try {
				tb.CreateType ();
				Assert.Fail ("#B2");
			} catch (TypeLoadException ex) {
				// Failure has occurred while loading a type
				Assert.AreEqual (typeof (TypeLoadException), ex.GetType (), "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.IsNotNull (ex.Message, "#B5");
			}

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface,
				typeof (EmptyInterface));
			Assert.AreEqual (typeof (EmptyInterface), tb.BaseType, "#C1");
			try {
				tb.CreateType ();
				Assert.Fail ("#C2");
			} catch (TypeLoadException ex) {
				// Could not load interface 't5' from assembly '...'
				// because the parent type is an interface
				Assert.AreEqual (typeof (TypeLoadException), ex.GetType (), "#C3");
				Assert.IsNull (ex.InnerException, "#C4");
				Assert.IsNotNull (ex.Message, "#C5");
				Assert.IsTrue (ex.Message.IndexOf (tb.Name) != -1, "#C6");
				Assert.IsTrue (ex.Message.IndexOf (assembly.FullName) != -1, "#C7");
			}
		}

		[Test]
		public void CreateType_Parent_DefaultCtorMissing ()
		{
			TypeBuilder tb;

			tb = module.DefineType (genTypeName ());
			ConstructorBuilder cb = tb.DefineConstructor (
				MethodAttributes.Public,
				CallingConventions.Standard,
				new Type [] { typeof (string) });
			cb.GetILGenerator ().Emit (OpCodes.Ret);
			Type parent_type = tb.CreateType ();

			tb = module.DefineType (genTypeName (), TypeAttributes.Class,
				parent_type);
			try {
				tb.CreateType ();
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// Parent does not have a default constructor.
				// The default constructor must be explicitly defined
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void CreateType_Parent_Null ()
		{
			TypeBuilder tb;
			Type emitted_type;
			
			tb = module.DefineType (genTypeName (), TypeAttributes.Public, null);
			Assert.AreEqual (typeof (object), tb.BaseType, "#A1");
			emitted_type = tb.CreateType ();
			Assert.AreEqual (typeof (object), emitted_type.BaseType, "#A2");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract, null);
			Assert.IsNull (tb.BaseType, "#B1");
			emitted_type = tb.CreateType ();
			Assert.IsNull (emitted_type.BaseType, "#B2");
		}

		[Test]
		[Category ("NotWorking")]
		public void DefineGenericParameters_AlreadyDefined ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("K");
			try {
				tb.DefineGenericParameters ("V");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void DefineGenericParameters_Names_Empty ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			try {
				tb.DefineGenericParameters (new string [0]);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Value does not fall within the expected range
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void DefineGenericParameters_Names_Null ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			try {
				tb.DefineGenericParameters ((string []) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("names", ex.ParamName, "#A5");
			}

			try {
				tb.DefineGenericParameters ("K", null, "V");
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("names", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void GenericType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("T");

			Assert.IsTrue (tb.IsGenericType, "#A1");
			Assert.IsTrue (tb.IsGenericTypeDefinition, "#A2");
			Assert.IsTrue (tb.ContainsGenericParameters, "#A3");
			Assert.IsFalse (tb.IsGenericParameter, "#A4");

			Type[] args = tb.GetGenericArguments ();
			Assert.IsFalse (args [0].IsGenericType, "#B1");
			Assert.IsFalse (args [0].IsGenericTypeDefinition, "#B2");
			Assert.IsTrue (args [0].ContainsGenericParameters, "#B3");
			Assert.IsTrue (args [0].IsGenericParameter, "#B4");
		}

		[Test]
		public void MakeGenericType ()
		{
			TypeBuilder tb;
			Type generic_type;
		
			tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("T");

			generic_type = tb.MakeGenericType (typeof (int));
			Assert.IsTrue (generic_type.IsGenericType, "#A1");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#A2");
			Assert.IsFalse (generic_type.ContainsGenericParameters, "#A3");
			Assert.IsFalse (generic_type.IsGenericParameter, "#A4");

			generic_type = tb.MakeGenericType (typeof (List<>).GetGenericArguments ());
			Assert.IsTrue (generic_type.IsGenericType, "#B1");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#B2");
			Assert.IsTrue (generic_type.ContainsGenericParameters, "#B3");
			Assert.IsFalse (generic_type.IsGenericParameter, "#B4");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface
				| TypeAttributes.Abstract | TypeAttributes.Public);
			tb.DefineGenericParameters ("T");

			generic_type = tb.MakeGenericType (typeof (int));
			Assert.IsTrue (generic_type.IsGenericType, "#C1");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#C2");
			Assert.IsFalse (generic_type.ContainsGenericParameters, "#C3");
			Assert.IsFalse (generic_type.IsGenericParameter, "#C4");

			generic_type = tb.MakeGenericType (typeof (List<>).GetGenericArguments ());
			Assert.IsTrue (generic_type.IsGenericType, "#D1");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#D2");
			Assert.IsTrue (generic_type.ContainsGenericParameters, "#D3");
			Assert.IsFalse (generic_type.IsGenericParameter, "#D4");
		}

		[Test]
		public void MakeGenericType_NoGenericTypeDefinition ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			try {
				tb.MakeGenericType (typeof (int));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Operation is not valid due to the current
				// state of the object
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=351169
		public void MakeGenericType_TypeArguments_Null_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("K", "V");

			try {
				tb.MakeGenericType ((Type []) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("typeArguments", ex.ParamName, "#A5");
			}

			try {
				tb.MakeGenericType (typeof (string), (Type) null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("typeArguments", ex.ParamName, "#B5");
			}
		}

		[Test]
		[Category ("NotWorking")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=351169
		public void MakeGenericType_TypeArguments_Null_MS ()
		{
			Type generic_type;
			Type [] type_args;

			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("K", "V");

			generic_type = tb.MakeGenericType ((Type []) null);
			Assert.IsNotNull (generic_type, "#A1");
			Assert.IsTrue (generic_type.IsGenericType, "#A2");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#A3");
			type_args = generic_type.GetGenericArguments ();
			Assert.IsNull (type_args, "#A4");

			generic_type  = tb.MakeGenericType (typeof (string), (Type) null);
			Assert.IsNotNull (generic_type, "#B1");
			Assert.IsTrue (generic_type.IsGenericType, "#B2");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#B3");
			type_args = generic_type.GetGenericArguments ();
			Assert.IsNotNull (type_args, "#B4");
			Assert.AreEqual (2, type_args.Length, "#B5");
			Assert.AreEqual (typeof (string), type_args [0], "#B6");
			Assert.IsNull (type_args [1], "#B7");
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=351143
		public void MakeGenericType_TypeArguments_Mismatch_Mono ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("K", "V");
			try {
				tb.MakeGenericType (typeof (int));
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The type or method has 2 generic prarameter(s)
				// but 1 generic argument(s) were provided. A
				// generic argument must be provided for each
				// generic parameter
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("typeArguments", ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("NotWorking")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=351143
		public void MakeGenericType_TypeArguments_Mismatch_MS ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("K", "V");
			
			Type generic_type = tb.MakeGenericType (typeof (int));
			Assert.IsTrue (generic_type.IsGenericType, "#1");
			Assert.IsFalse (generic_type.IsGenericTypeDefinition, "#2");
			Type [] type_args = generic_type.GetGenericArguments ();
			Assert.IsNotNull (type_args, "#3");
			Assert.AreEqual (1, type_args.Length, "#4");
			Assert.AreEqual (typeof (int), type_args [0], "#5");
		}

		[Test]
		public void MakeArrayType_Complete ()
		{
			// reference type
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (ContextBoundObject));
			Type emittedType = tb.CreateType ();
			Type arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#A1");
			Assert.IsTrue (arrayType.HasElementType, "#A2");
			Assert.AreEqual (tb, arrayType.GetElementType (), "#A3");
			Assert.IsFalse (tb.HasElementType, "#A4");
			Assert.IsTrue (tb.IsCreated (), "#A5");

			// value type
			tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (ValueType));
			emittedType = tb.CreateType ();
			arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#B1");
			Assert.IsTrue (arrayType.HasElementType, "#B2");
			Assert.AreEqual (tb, arrayType.GetElementType (), "#B3");
			Assert.IsFalse (tb.HasElementType, "#B4");
			Assert.IsTrue (tb.IsCreated (), "#B5");

			tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			tb.DefineField ("value__", typeof (int), FieldAttributes.SpecialName |
				FieldAttributes.Private | FieldAttributes.RTSpecialName);
			emittedType = tb.CreateType ();
			arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#C1");
			Assert.IsTrue (arrayType.HasElementType, "#C2");
			Assert.AreEqual (tb, arrayType.GetElementType (), "#C3");
			Assert.IsFalse (tb.HasElementType, "#C4");
			Assert.IsTrue (tb.IsCreated (), "#C5");
		}

		[Test] // bug #82015
		public void MakeArrayType_Incomplete ()
		{
			// reference type
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (ContextBoundObject));
			Type arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#A1");
			Assert.IsTrue (arrayType.HasElementType, "#A2");
			Assert.AreEqual (tb, arrayType.GetElementType (), "#A3");
			Assert.IsFalse (tb.HasElementType, "#A4");
			Assert.IsFalse (tb.IsCreated (), "#A5");

			// value type
			tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (ValueType));
			arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#B1");
			Assert.IsTrue (arrayType.HasElementType, "#B2");
			Assert.AreEqual (tb, arrayType.GetElementType (), "#B3");
			Assert.IsFalse (tb.HasElementType, "#B4");
			Assert.IsFalse (tb.IsCreated (), "#B5");

			// enum
			tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));
			arrayType = tb.MakeArrayType ();
			Assert.IsTrue (arrayType.IsArray, "#C1");
			Assert.IsTrue (arrayType.HasElementType, "#C2");
			Assert.IsFalse (tb.HasElementType, "#C3");
			Assert.IsFalse (tb.IsCreated (), "#C4");
		}

		[Test]
		public void GetCustomAttributes_InflatedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters (new string[] { "FOO" });

			ConstructorInfo guidCtor = typeof (GuidAttribute).GetConstructor (
				new Type [] { typeof (string) });

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (guidCtor,
				new object [] { Guid.NewGuid ().ToString ("D") }, new FieldInfo [0], new object [0]);

			tb.SetCustomAttribute (caBuilder);
			Type t = tb.CreateType ();

			Type inflated = t.MakeGenericType (new Type [] { typeof (int) });

			Assert.AreEqual (1, inflated.GetCustomAttributes (false).Length);
		}

		[Test]
		public void GetField ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

			ConstructorBuilder cb = tb.DefineDefaultConstructor (MethodAttributes.Public);

			FieldBuilder fb1 = tb.DefineField ("field1", typeParams [0], FieldAttributes.Public);

			Type t = tb.MakeGenericType (typeof (int));

			// Chect that calling MakeArrayType () does not initialize the class
			// (bug #351172)
			t.MakeArrayType ();

			// Check that the instantiation of a type builder contains live data
			TypeBuilder.GetField (t, fb1);
			FieldBuilder fb2 = tb.DefineField ("field2", typeParams [0], FieldAttributes.Public);
			FieldInfo fi2 = TypeBuilder.GetField (t, fb1);

			MethodBuilder mb = tb.DefineMethod ("get_int", MethodAttributes.Public|MethodAttributes.Static, typeof (int), Type.EmptyTypes);
			ILGenerator ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (t, cb));
			ilgen.Emit (OpCodes.Dup);
			ilgen.Emit (OpCodes.Ldc_I4, 42);
			ilgen.Emit (OpCodes.Stfld, fi2);
			ilgen.Emit (OpCodes.Ldfld, fi2);
			ilgen.Emit (OpCodes.Ret);

			// Check GetField on a type instantiated with type parameters
			Type t3 = tb.MakeGenericType (typeParams [0]);
			FieldBuilder fb3 = tb.DefineField ("field3", typeParams [0], FieldAttributes.Public);
			FieldInfo fi3 = TypeBuilder.GetField (t3, fb3);

			MethodBuilder mb3 = tb.DefineMethod ("get_T", MethodAttributes.Public|MethodAttributes.Static, typeParams [0], Type.EmptyTypes);
			ILGenerator ilgen3 = mb3.GetILGenerator ();
			ilgen3.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (t3, cb));
			ilgen3.Emit (OpCodes.Ldfld, fi3);
			ilgen3.Emit (OpCodes.Ret);

			Type created = tb.CreateType ();

			Type inst = created.MakeGenericType (typeof (object));

			Assert.AreEqual (42, inst.GetMethod ("get_int").Invoke (null, null));

			Assert.AreEqual (null, inst.GetMethod ("get_T").Invoke (null, null));
		}
		
		[Test] //bug #354047
		public void CreatedTypeInstantiationOverTypeBuilderArgsIsNotAGenericTypeDefinition ()
		{
			TypeBuilder tb = module.DefineType ("TheType", TypeAttributes.Public, typeof (object), new Type [] {typeof (IDelegateFactory)});
			GenericTypeParameterBuilder[] typeParams = tb.DefineGenericParameters (new String[] { "T" });
			Type t = tb.CreateType ();

			Type inst = tb.MakeGenericType (typeParams [0]);
			Assert.IsFalse (inst.IsGenericTypeDefinition, "#1 create type instance is not a generic type definition");
		}

		[Test] //bug #354047
		public void CreatedTypeAndTypeBuilderOwnTheirGenericArguments ()
		{
			TypeBuilder tb = module.DefineType ("TheType", TypeAttributes.Public, typeof (object), new Type [] {typeof (IDelegateFactory)});
			GenericTypeParameterBuilder[] typeParams = tb.DefineGenericParameters (new String[] { "T" });
			Type t = tb.CreateType ();

			Assert.IsTrue (tb.GetGenericArguments()[0].DeclaringType == tb, "#1 TypeBuilder owns its arguments");
			Assert.IsTrue (t.GetGenericArguments()[0].DeclaringType == t, "#1 create type owns its arguments");
		}

		[Test] //bug #354047
		public void CreatedTypeAndTypeBuilderDontShareGenericArguments ()
		{
			TypeBuilder tb = module.DefineType ("TheType", TypeAttributes.Public, typeof (object), new Type [] {typeof (IDelegateFactory)});
			GenericTypeParameterBuilder[] typeParams = tb.DefineGenericParameters (new String[] { "T" });
			Type t = tb.CreateType ();

			Assert.IsTrue (tb.GetGenericArguments()[0] != t.GetGenericArguments()[0], "#1 TypeBuilder and create type arguments are diferent");
		}

		[Test] //bug #399047
		public void FieldOnTypeBuilderInstDontInflateWhenEncoded () {
				assembly = Thread.GetDomain ().DefineDynamicAssembly (new AssemblyName (ASSEMBLY_NAME), AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

				module = assembly.DefineDynamicModule ("Instance.exe");
  
                TypeBuilder G = module.DefineType ("G", TypeAttributes.Public);
                Type T = G.DefineGenericParameters ("T") [0];
				ConstructorInfo ctor = G.DefineDefaultConstructor (MethodAttributes.Public);
                Type GObj = G.MakeGenericType (new Type [] { T });

                FieldBuilder F = G.DefineField ("F", T, FieldAttributes.Public);

                TypeBuilder P = module.DefineType ("P", TypeAttributes.Public);

                MethodBuilder Test = P.DefineMethod ("Test", MethodAttributes.Public);
                Type TATest = Test.DefineGenericParameters ("TA") [0];
                {
                        Type TestGObj = G.MakeGenericType (new Type [] { TATest });

                        ILGenerator il = Test.GetILGenerator ();

                        il.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (TestGObj, ctor));
                        il.Emit (OpCodes.Ldfld, TypeBuilder.GetField (TestGObj, F));
                        il.Emit (OpCodes.Pop);

                        il.Emit (OpCodes.Ret);
                }

 				MethodBuilder main = P.DefineMethod ("Main", MethodAttributes.Public | MethodAttributes.Static);
				{
					ILGenerator il = main.GetILGenerator ();
					il.Emit(OpCodes.Newobj, P.DefineDefaultConstructor (MethodAttributes.Public));
					il.Emit(OpCodes.Call, Test.MakeGenericMethod (typeof (int)));
					il.Emit (OpCodes.Ret);
				}

				assembly.SetEntryPoint (main);
                G.CreateType ();
                P.CreateType ();

                assembly.Save ("Instance.exe");
				Thread.GetDomain ().ExecuteAssembly(Path.GetTempPath () + Path.DirectorySeparatorChar + "Instance.exe");
		}

		[Test]
		public void FieldWithInitializedDataWorksWithCompilerRuntimeHelpers ()
		{
			TypeBuilder tb = module.DefineType ("Type1", TypeAttributes.Public);
			FieldBuilder fb = tb.DefineInitializedData ("Foo", new byte [] {1,2,3,4}, FieldAttributes.Static|FieldAttributes.Public);
			tb.CreateType ();

			assembly = Thread.GetDomain ().DefineDynamicAssembly (new AssemblyName (ASSEMBLY_NAME+"2"), AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());
			module = assembly.DefineDynamicModule ("Instance.exe");

			TypeBuilder tb2 = module.DefineType ("Type2", TypeAttributes.Public);
			MethodBuilder mb = tb2.DefineMethod ("Test", MethodAttributes.Public | MethodAttributes.Static, typeof (object), new Type [0]);
			ILGenerator il = mb.GetILGenerator ();

			il.Emit (OpCodes.Ldc_I4_1);
			il.Emit (OpCodes.Newarr, typeof (int));
			il.Emit (OpCodes.Dup);
			il.Emit (OpCodes.Ldtoken, fb);
			il.Emit (OpCodes.Call, typeof (RuntimeHelpers).GetMethod ("InitializeArray"));
			il.Emit (OpCodes.Ret);

			Type t = tb2.CreateType ();
			int[] res = (int[])t.GetMethod ("Test").Invoke (null, new object[0]);
			//Console.WriteLine (res[0]);
		}

		public interface IDelegateFactory
		{
			Delegate Create (Delegate del);
		}

		[Test]
		public void CreateType_Ctor_NoBody ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineConstructor (MethodAttributes.Public,
				CallingConventions.Standard,
				new Type [] { typeof (string) });
			try {
				tb.CreateType ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Method '.ctor' does not have a method body
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (".ctor") != -1, "#5");
			}
		}

		[Test] //bug #361689
		public void CreateTypeFailsWithInvalidMethodOverride ()
		{
			TypeBuilder tb = module.DefineType ("TheType", TypeAttributes.Public, typeof (object), new Type [] {typeof (IDelegateFactory)});

			MethodBuilder mc = tb.DefineMethod ("Create", MethodAttributes.Public, typeof (Delegate), new Type[] {typeof (Delegate)});
			ILGenerator gen = mc.GetILGenerator ();
			gen.Emit (OpCodes.Ldarg_0);
			gen.Emit (OpCodes.Ret);
			tb.DefineMethodOverride (mc, typeof (IDelegateFactory).GetMethod ("Create"));
			try {
				tb.CreateType ();
				Assert.Fail ("#1 create type did not throw TypeLoadException");
			} catch (TypeLoadException) {
			
			}
		}

		[Test] //bug #349194
		public void IsAssignableToWorksWithInterfacesOnParent ()
		{
            TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (EmptyIfaceImpl));
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Public, tb);

			Assert.IsFalse (typeof (EmptyInterface).IsAssignableFrom (tb));
			Type t = tb.CreateType ();
			Assert.IsTrue (typeof (EmptyInterface).IsAssignableFrom (tb));
			Assert.IsTrue (typeof (EmptyInterface).IsAssignableFrom (t));
			
			
			Assert.IsFalse (typeof (EmptyInterface).IsAssignableFrom (tb2));
			Type t2 = tb2.CreateType ();
			Assert.IsTrue (typeof (EmptyInterface).IsAssignableFrom (tb2));
			Assert.IsTrue (typeof (EmptyInterface).IsAssignableFrom (t2));
		}


		[Test] //bug #430508
		public void MakeGenericTypeRespectBaseType ()
		{
            TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (EmptyIfaceImpl));
			EnumBuilder eb = module.DefineEnum (genTypeName (), TypeAttributes.Public, typeof (int));

			MethodBuilder mb = tb.DefineMethod ("Test",
												MethodAttributes.Public,
												typeof (Tuple<,>).MakeGenericType (typeof (int), eb),
												new Type [0]);
			ILGenerator il = mb.GetILGenerator();
			il.Emit (OpCodes.Ldnull);
			il.Emit (OpCodes.Ret);
	
			Type e = eb.CreateType ();
			Type c = tb.CreateType ();
		
			Assert.AreEqual (c.GetMethod ("Test").ReturnType.GetGenericArguments ()[1], e, "#1");
		}

		[Test]
		public void GetCustomAttrOnFieldOfInflatedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("T");

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				typeof (SimpleTestAttribute).GetConstructors ()[0],
				new object [0]);

			FieldBuilder field = tb.DefineField ("OI", typeof (int), 0);
			field.SetCustomAttribute (caBuilder);

			Type t = tb.CreateType ();

			FieldInfo fi = t.GetFields (BindingFlags.NonPublic | BindingFlags.Instance)[0];
			object[] cattrs = fi.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);

			fi = t.MakeGenericType (typeof (int)).GetFields (BindingFlags.NonPublic | BindingFlags.Instance)[0];
			cattrs = fi.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);
		}

		[Test]
		public void GetCustomAttrOnPropertyOfInflatedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters ("T");

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				typeof (SimpleTestAttribute).GetConstructors ()[0],
				new object [0]);

			PropertyBuilder property = DefineStringProperty (tb, "Name", "name", MethodAttributes.Public);
			property.SetCustomAttribute (caBuilder);

			Type t = tb.CreateType ();

			PropertyInfo pi = t.GetProperties ()[0];
			object[] cattrs = pi.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);

			pi = t.MakeGenericType (typeof (int)).GetProperties ()[0];
			cattrs = pi.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);
		}

		[Test]
		public void GetCustomAttrOnEventOfInflatedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineGenericParameters ("T");

			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				typeof (SimpleTestAttribute).GetConstructors ()[0],
				new object [0]);

			EventBuilder evt = tb.DefineEvent ("OI", 0, typeof (int));
			evt.SetCustomAttribute (caBuilder);

			Type t = tb.CreateType ();

			EventInfo ei = t.GetEvents (BindingFlags.NonPublic | BindingFlags.Instance)[0];
			object[] cattrs = ei.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);

			ei = t.MakeGenericType (typeof (int)).GetEvents (BindingFlags.NonPublic | BindingFlags.Instance)[0];
			cattrs = ei.GetCustomAttributes (false);
			Assert.AreEqual (1, cattrs.Length);
		}

		public void TestDoubleInitializationOfMonoGenericClass () //bug #400643
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("T");
 
			CustomAttributeBuilder caBuilder = new CustomAttributeBuilder (
				typeof (SimpleTestAttribute).GetConstructors ()[0],
				new object [0]);

			FieldBuilder field = tb.DefineField ("OI", typeof (int), 0);
			field.SetCustomAttribute (caBuilder);


			tb.MakeGenericType (typeof (int)).GetMethods ();
			tb.MakeGenericType (typeof (double)).GetMethods ();
			
			Type t = tb.CreateType ();
			
			t.MakeGenericType (typeof (int)).GetMethods ();
			t.MakeGenericType (typeof (double)).GetMethods ();
		}

		[Test]
		public void TestGenericFieldAccess () // bug #467415
		{
			AssemblyName asmName = new AssemblyName("DemoMethodBuilder1");
			AppDomain domain = AppDomain.CurrentDomain;
			AssemblyBuilder demoAssembly =
				domain.DefineDynamicAssembly(asmName,
						AssemblyBuilderAccess.RunAndSave);

			// Define the module that contains the code. For an
			// assembly with one module, the module name is the
			// assembly name plus a file extension.
			ModuleBuilder demoModule =
				demoAssembly.DefineDynamicModule(asmName.Name,
						asmName.Name+".dll");

			TypeBuilder demoType =
				demoModule.DefineType("DemoType", TypeAttributes.Public);

			MethodBuilder factory =
				demoType.DefineMethod("Factory",
						MethodAttributes.Public | MethodAttributes.Static);

			string[] typeParameterNames = {"T"};
			GenericTypeParameterBuilder[] typeParameters =
				factory.DefineGenericParameters(typeParameterNames);

			GenericTypeParameterBuilder T = typeParameters[0];

			Type[] parms = {};
			factory.SetParameters(parms);

			factory.SetReturnType(T);

			ILGenerator ilgen = factory.GetILGenerator();

			Type G = typeof(Gen<>);
			Type GT = G.MakeGenericType (T);
			FieldInfo GF = G.GetField("field");
			FieldInfo GTF = TypeBuilder.GetField(GT, GF);

			ilgen.Emit(OpCodes.Ldsfld, GTF);
			ilgen.Emit(OpCodes.Ret);

			// Complete the type.
			Type dt = demoType.CreateType();
			// Save the assembly, so it can be examined with Ildasm.exe.
			//demoAssembly.Save(asmName.Name+".dll");

			MethodInfo m = dt.GetMethod("Factory");
			MethodInfo bound = m.MakeGenericMethod(typeof(int));

			// Display a string representing the bound method.
			//Console.WriteLine(bound);

			object[] parameters = {};
			int result = (int)(bound.Invoke(null, parameters));

			Assert.AreEqual (0, result, "#1");
		}

		static MethodInfo GetMethodByName (MethodInfo [] methods, string name)
		{
			foreach (MethodInfo mi in methods)
				if (mi.Name == name)
					return mi;
			return null;
		}

		void CreateMembers (TypeBuilder tb, string suffix, bool defineCtors)
		{
			ConstructorBuilder cb;
			MethodBuilder mb;
			PropertyBuilder pb;
			EventBuilder eb;
			ILGenerator ilgen;

			if (defineCtors) {
				//
				// instance constructors
				//
				cb = tb.DefineConstructor (MethodAttributes.Private,
					CallingConventions.Standard,
					new Type [] { typeof (int), typeof (int) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				cb = tb.DefineConstructor (MethodAttributes.Family,
					CallingConventions.Standard,
					new Type [] { typeof (string) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				cb = tb.DefineConstructor (MethodAttributes.FamANDAssem,
					CallingConventions.Standard,
					new Type [] { typeof (string), typeof (string) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				cb = tb.DefineConstructor (MethodAttributes.FamORAssem,
					CallingConventions.Standard,
					new Type [] { typeof (int) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				cb = tb.DefineConstructor (MethodAttributes.Public,
					CallingConventions.Standard,
					new Type [] { typeof (int), typeof (bool) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				cb = tb.DefineConstructor (MethodAttributes.Assembly,
					CallingConventions.Standard,
					new Type [] { typeof (string), typeof (int) });
				cb.GetILGenerator ().Emit (OpCodes.Ret);

				//
				// static constructors
				//

				cb = tb.DefineConstructor (MethodAttributes.Private |
					MethodAttributes.Static,
					CallingConventions.Standard,
					Type.EmptyTypes);
				cb.GetILGenerator ().Emit (OpCodes.Ret);
			}

			//
			// instance methods
			//

			mb = tb.DefineMethod ("GetPrivateInstance" + suffix,
				MethodAttributes.Private, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamilyInstance" + suffix,
				MethodAttributes.Family, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamANDAssemInstance" + suffix,
				MethodAttributes.FamANDAssem, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamORAssemInstance" + suffix,
				MethodAttributes.FamORAssem, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetPublicInstance" + suffix,
				MethodAttributes.Public, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetAssemblyInstance" + suffix,
				MethodAttributes.Assembly, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			//
			// static methods
			//

			mb = tb.DefineMethod ("GetPrivateStatic" + suffix,
				MethodAttributes.Private | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamilyStatic" + suffix,
				MethodAttributes.Family | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamANDAssemStatic" + suffix,
				MethodAttributes.FamANDAssem | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetFamORAssemStatic" + suffix,
				MethodAttributes.FamORAssem | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetPublicStatic" + suffix,
				MethodAttributes.Public | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			mb = tb.DefineMethod ("GetAssemblyStatic" + suffix,
				MethodAttributes.Assembly | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);

			//
			// instance fields
			//

			tb.DefineField ("privateInstance" + suffix,
				typeof (string), FieldAttributes.Private);
			tb.DefineField ("familyInstance" + suffix,
				typeof (string), FieldAttributes.Family);
			tb.DefineField ("famANDAssemInstance" + suffix,
				typeof (string), FieldAttributes.FamANDAssem);
			tb.DefineField ("famORAssemInstance" + suffix,
				typeof (string), FieldAttributes.FamORAssem);
			tb.DefineField ("publicInstance" + suffix,
				typeof (string), FieldAttributes.Public);
			tb.DefineField ("assemblyInstance" + suffix,
				typeof (string), FieldAttributes.Assembly);

			//
			// static fields
			//

			tb.DefineField ("privateStatic" + suffix,
				typeof (string), FieldAttributes.Private |
				FieldAttributes.Static);
			tb.DefineField ("familyStatic" + suffix,
				typeof (string), FieldAttributes.Family |
				FieldAttributes.Static);
			tb.DefineField ("famANDAssemStatic" + suffix,
				typeof (string), FieldAttributes.FamANDAssem |
				FieldAttributes.Static);
			tb.DefineField ("famORAssemStatic" + suffix,
				typeof (string), FieldAttributes.FamORAssem |
				FieldAttributes.Static);
			tb.DefineField ("publicStatic" + suffix,
				typeof (string), FieldAttributes.Public |
				FieldAttributes.Static);
			tb.DefineField ("assemblyStatic" + suffix,
				typeof (string), FieldAttributes.Assembly |
				FieldAttributes.Static);

			//
			// instance properties
			//

			pb = tb.DefineProperty ("PrivateInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_PrivateInstance" + suffix,
				MethodAttributes.Private, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamilyInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamilyInstance" + suffix,
				MethodAttributes.Family, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamANDAssemInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamANDAssemInstance" + suffix,
				MethodAttributes.FamANDAssem, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamORAssemInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamORAssemInstance" + suffix,
				MethodAttributes.FamORAssem, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("PublicInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_PublicInstance" + suffix,
				MethodAttributes.Public, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("AssemblyInstance" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_AssemblyInstance" + suffix,
				MethodAttributes.Assembly, typeof (string),
				Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			//
			// static properties
			//

			pb = tb.DefineProperty ("PrivateStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_PrivateStatic" + suffix,
				MethodAttributes.Private | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamilyStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamilyStatic" + suffix,
				MethodAttributes.Family | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamANDAssemStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamANDAssemStatic" + suffix,
				MethodAttributes.FamANDAssem | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("FamORAssemStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_FamORAssemStatic" + suffix,
				MethodAttributes.FamORAssem | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("PublicStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_PublicStatic" + suffix,
				MethodAttributes.Public | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			pb = tb.DefineProperty ("AssemblyStatic" + suffix,
				PropertyAttributes.None, typeof (string),
				Type.EmptyTypes);
			mb = tb.DefineMethod ("get_AssemblyStatic" + suffix,
				MethodAttributes.Assembly | MethodAttributes.Static,
				typeof (string), Type.EmptyTypes);
			ilgen = mb.GetILGenerator ();
			ilgen.Emit (OpCodes.Ldnull);
			ilgen.Emit (OpCodes.Ret);
			pb.SetGetMethod (mb);

			//
			// instance events
			//

			eb = tb.DefineEvent ("OnPrivateInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnPrivateInstance" + suffix,
				MethodAttributes.Private, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamilyInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamilyInstance" + suffix,
				MethodAttributes.Family, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamANDAssemInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamANDAssemInstance" + suffix,
				MethodAttributes.FamANDAssem, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamORAssemInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamORAssemInstance" + suffix,
				MethodAttributes.FamORAssem, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnPublicInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnPublicInstance" + suffix,
				MethodAttributes.Public, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnAssemblyInstance" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnAssemblyInstance" + suffix,
				MethodAttributes.Family, typeof (void),
				Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			//
			// static events
			//

			eb = tb.DefineEvent ("OnPrivateStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnPrivateStatic" + suffix,
				MethodAttributes.Private | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamilyStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamilyStatic" + suffix,
				MethodAttributes.Family | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamANDAssemStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamANDAssemStatic" + suffix,
				MethodAttributes.FamANDAssem | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnFamORAssemStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnFamORAssemStatic" + suffix,
				MethodAttributes.FamORAssem | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnPublicStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnPublicStatic" + suffix,
				MethodAttributes.Public | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);

			eb = tb.DefineEvent ("OnAssemblyStatic" + suffix,
				EventAttributes.None, typeof (EventHandler));
			mb = tb.DefineMethod ("add_OnAssemblyStatic" + suffix,
				MethodAttributes.Family | MethodAttributes.Static,
				typeof (void), Type.EmptyTypes);
			mb.GetILGenerator ().Emit (OpCodes.Ret);
			eb.SetAddOnMethod (mb);
		}

		static TypeBuilder Resolve1_Tb;
		static bool Resolve1_Called;

		public class Lookup<T>
		{
			public static Type t = typeof(T);
		}

		Assembly Resolve1 (object sender, ResolveEventArgs args) {
			Resolve1_Called = true;
			Resolve1_Tb.CreateType ();
			return Resolve1_Tb.Assembly;
		}

		[Test]
		public void TypeResolveGenericInstances () {
			// Test that TypeResolve is called for generic instances (#483852)
			TypeBuilder tb1 = null;

			AppDomain.CurrentDomain.TypeResolve += Resolve1;

			tb1 = module.DefineType("Foo");
			Resolve1_Tb = tb1;
			FieldInfo field = TypeBuilder.GetField(typeof(Lookup<>).MakeGenericType(tb1), typeof(Lookup<>).GetField("t"));
			TypeBuilder tb2 = module.DefineType("Bar");
			ConstructorBuilder cb = tb2.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			ILGenerator ilgen = cb.GetILGenerator();
			ilgen.Emit(OpCodes.Ldsfld, field);
			ilgen.Emit(OpCodes.Pop);
			ilgen.Emit(OpCodes.Ret);
			Activator.CreateInstance(tb2.CreateType());

			Assert.IsTrue (Resolve1_Called);
		}

		[Test]
		public void CreateConcreteTypeWithAbstractMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineMethod("method", MethodAttributes.Abstract | MethodAttributes.Public, typeof (void), Type.EmptyTypes);
			try {
				tb.CreateType ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void ConcreteTypeDontLeakGenericParamFromItSelf ()
		{
            var tb = module.DefineType (genTypeName ());
			var gps = tb.DefineGenericParameters ("T");
            var mb = tb.DefineMethod ("m", MethodAttributes.Public | MethodAttributes.Static);
            mb.SetParameters (gps);
            mb.GetILGenerator ().Emit (OpCodes.Ret);

            var ti = tb.CreateType ();
            var mi = ti.GetMethod ("m");
			var arg0 = mi.GetParameters () [0].ParameterType;

			Assert.AreNotSame (arg0, gps [0], "#1");
		}

		[Test]
		public void ConcreteTypeDontLeakGenericParamFromMethods ()
		{
            var tb = module.DefineType (genTypeName ());
            var mb = tb.DefineMethod("m", MethodAttributes.Public | MethodAttributes.Static);
            var gps = mb.DefineGenericParameters ("T");
            mb.SetParameters (gps);
            mb.GetILGenerator ().Emit (OpCodes.Ret);

            var ti = tb.CreateType ();
            var mi = ti.GetMethod ("m");
 			var arg0 = mi.GetParameters () [0].ParameterType;

			Assert.AreNotSame (arg0, gps [0], "#1");
		}

		[Test]
		public void DeclaringMethodReturnsNull ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsNull (tb.DeclaringMethod, null, "#1");
		}

		[Test]
		public void GenericParameterPositionReturns0 ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.AreEqual (0, tb.GenericParameterPosition, "#1");
		}

		[Test]
		public void GetGenericTypeDefinitionBehavior ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetGenericTypeDefinition ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {}

			tb.DefineGenericParameters ("T");
			Assert.AreEqual (tb, tb.GetGenericTypeDefinition (), "#2");

			tb.CreateType ();
			Assert.AreEqual (tb, tb.GetGenericTypeDefinition (), "#3");
		}

		[Test]
		public void GetElementTypeNotSupported ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetElementType ();
				Assert.Fail ("#1");
			} catch (NotSupportedException) {}
		}

		[Test]
		public void GenericParameterAttributesReturnsNone ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.AreEqual (GenericParameterAttributes.None, tb.GenericParameterAttributes, "#1");

			tb.DefineGenericParameters ("T");
			Assert.AreEqual (GenericParameterAttributes.None, tb.GenericParameterAttributes, "#2");

			tb.CreateType ();
			Assert.AreEqual (GenericParameterAttributes.None, tb.GenericParameterAttributes, "#3");
		}

		[Test]
		public void GetGenericArgumentsReturnsNullForNonGenericTypeBuilder ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Assert.IsNull (tb.GetGenericArguments (), "#1");
		}

		public interface IFaceA {}
		public interface IFaceB : IFaceA {}
		[Test]
		public void GetInterfacesAfterCreate ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object), new Type[] { typeof (IFaceB) });

			Type[] ifaces = tb.GetInterfaces ();
			Assert.AreEqual (1, ifaces.Length, "#1");
			Assert.AreEqual (typeof (IFaceB), ifaces [0], "#2");

			tb.CreateType ();
			ifaces = tb.GetInterfaces ();
			Assert.AreEqual (2, ifaces.Length, "#3");
			//Interfaces can come in any particular order
			if (ifaces [0] == typeof (IFaceB)) {
				Assert.AreEqual (typeof (IFaceB), ifaces [0], "#4");
				Assert.AreEqual (typeof (IFaceA), ifaces [1], "#5");
			} else {
				Assert.AreEqual (typeof (IFaceB), ifaces [1], "#4");
				Assert.AreEqual (typeof (IFaceA), ifaces [0], "#5");
			}
		}

		public interface MB_Iface
		{
		    int Test ();
		}

		public class MB_Impl : MB_Iface
		{
		    public virtual int Test () { return 1; }
		}
		[Test]
		public void MethodOverrideBodyMustBelongToTypeBuilder ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			MethodInfo md = typeof (MB_Iface).GetMethod("Test");
            MethodInfo md2 = typeof (MB_Impl).GetMethod("Test");
			try {
            	tb.DefineMethodOverride (md, md2);
            	Assert.Fail ("#1");
			} catch (ArgumentException) {}
		}

		[Test]
		public void GetConstructorsThrowWhenIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetConstructors (BindingFlags.Instance);
				Assert.Fail ("#1");
			} catch (NotSupportedException) { }

			tb.CreateType ();
			Assert.IsNotNull (tb.GetConstructors (BindingFlags.Instance), "#2");
		}

		[Test]
		public void GetEventsThrowWhenIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.GetEvents (BindingFlags.Instance);
				Assert.Fail ("#1");
			} catch (NotSupportedException) { }

			tb.CreateType ();
			Assert.IsNotNull (tb.GetEvents (BindingFlags.Instance), "#2");
		}

		[Test]
		public void GetNestedTypeCreatedAfterTypeIsCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			TypeBuilder nested = tb.DefineNestedType ("Bar", TypeAttributes.Class | TypeAttributes.NestedPrivate);
			tb.CreateType ();
			Assert.IsNull (tb.GetNestedType ("Bar", BindingFlags.NonPublic), "#1");
			Type res = nested.CreateType ();
			Assert.AreEqual (res, tb.GetNestedType ("Bar", BindingFlags.NonPublic), "#2");

			TypeBuilder nested2 = tb.DefineNestedType ("Bar2", TypeAttributes.Class | TypeAttributes.NestedPrivate);
			Assert.IsNull (tb.GetNestedType ("Bar2", BindingFlags.NonPublic), "#3");
			res = nested2.CreateType ();
			Assert.AreEqual (res, tb.GetNestedType ("Bar2", BindingFlags.NonPublic), "#4");
		}


		[Test]
		public void IsDefinedThrowWhenIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.IsDefined (typeof (string), true);
				Assert.Fail ("#1");
			} catch (NotSupportedException) { }

			tb.CreateType ();
			Assert.IsNotNull (tb.IsDefined (typeof (string), true), "#2");
		}

		[Test] //Bug #594728
		public void IsSubclassOfWorksIfSetParentIsCalledOnParent ()
		{
			var tb_a = module.DefineType ("A", TypeAttributes.Public);
			var tb_b = module.DefineType ("B", TypeAttributes.Public);
	
			tb_b.SetParent (tb_a);
			tb_a.SetParent (typeof (Attribute));
	
			Assert.IsTrue (tb_b.IsSubclassOf (tb_a), "#1");
			Assert.IsTrue (tb_b.IsSubclassOf (typeof (Attribute)), "#2");
			Assert.IsFalse (tb_a.IsSubclassOf (tb_b), "#3");
	
	
			var a = tb_a.CreateType ();
			var b = tb_b.CreateType ();
	
			Assert.IsTrue (b.IsSubclassOf (a), "#4");
			Assert.IsTrue (b.IsSubclassOf (typeof (Attribute)), "#5");
			Assert.IsFalse (a.IsSubclassOf (b), "#6");
		}

		[Test]
		public void DefinedDefaultConstructorWorksWithGenericBaseType ()
		{
			AssemblyName assemblyName = new AssemblyName ("a");
			AssemblyBuilder ass = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.RunAndSave);
			var mb = ass.DefineDynamicModule ("a.dll");

			var tb = mb.DefineType ("Base");
			tb.DefineGenericParameters ("F");

			var inst = tb.MakeGenericType (typeof (int));
			var tb2 = mb.DefineType ("Child", TypeAttributes.Public, inst);

			tb.CreateType ();
			var res = tb2.CreateType ();

			Assert.IsNotNull (res, "#1");
			Assert.AreEqual (1, res.GetConstructors ().Length, "#2");
		}

		/* 
		 * Tests for passing user types to Ref.Emit. Currently these only test
		 * whenever the runtime code can handle them without crashing, since we
		 * don't support user types yet.
		 * These tests are disabled for windows since the MS runtime trips on them.
		 */
		[Test]
		[Category ("NotDotNet")] //Proper UT handling is a mono extension to SRE bugginess
		public void UserTypes () {
			TypeDelegator t = new TypeDelegator (typeof (int));

			try {
				/* Parent */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, t);
			} catch {
			}

			try {
				/* Interfaces */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object), new Type [] { t });
				tb.CreateType ();
			} catch {
			}

			try {
				/* Fields */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				tb.DefineField ("Foo", t, FieldAttributes.Public);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Custom modifiers on fields */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				tb.DefineField ("Foo", typeof (int), new Type [] { t }, new Type [] { t }, FieldAttributes.Public);
				tb.CreateType ();
			} catch {
			}

			try {
				/* This is mono only */
				UnmanagedMarshal m = UnmanagedMarshal.DefineCustom (t, "foo", "bar", Guid.Empty);
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				FieldBuilder fb = tb.DefineField ("Foo", typeof (int), FieldAttributes.Public);
				fb.SetMarshal (m);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Properties */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				tb.DefineProperty ("Foo", PropertyAttributes.None, t, null);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Custom modifiers on properties */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				// FIXME: These seems to be ignored
				tb.DefineProperty ("Foo", PropertyAttributes.None, typeof (int), new Type [] { t }, new Type [] { t }, null, null, null);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Method return types */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, t, null);
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Custom modifiers on method return types */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { t }, new Type [] { t }, null, null, null);
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Method parameters */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, typeof (int), new Type [] { t });
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Custom modifiers on method parameters */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), null, null, new Type [] { typeof (int) }, new Type [][] { new Type [] { t }}, new Type[][] { new Type [] { t }});
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* Ctor parameters */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				ConstructorBuilder mb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, new Type [] { t });
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}
			
			try {
				/* Custom modifiers on ctor parameters */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				ConstructorBuilder mb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, new Type [] { typeof (int) }, new Type [][] { new Type [] { t }}, new Type[][] { new Type [] { t }});
				mb.GetILGenerator ().Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* SignatureHelper arguments */
				SignatureHelper sighelper = SignatureHelper.GetFieldSigHelper (module);
				sighelper.AddArgument (t, false);
				byte[] arr = sighelper.GetSignature ();
			} catch {
			}

			try {
				SignatureHelper sighelper = SignatureHelper.GetLocalVarSigHelper (module);
				sighelper.AddArgument (t, false);
				byte[] arr = sighelper.GetSignature ();
			} catch {
			}

			try {
				/* Custom modifiers on SignatureHelper arguments */
				SignatureHelper sighelper = SignatureHelper.GetFieldSigHelper (module);
				sighelper.AddArgument (typeof (int), new Type [] { t }, new Type [] { t });
				byte[] arr = sighelper.GetSignature ();
			} catch {
			}

			try {
				/* Custom modifiers on SignatureHelper arguments */
				SignatureHelper sighelper = SignatureHelper.GetLocalVarSigHelper (module);
				sighelper.AddArgument (typeof (int), new Type [] { t }, new Type [] { t });
				byte[] arr = sighelper.GetSignature ();
			} catch {
			}

			/* Arguments to ILGenerator methods */
			try {
				/* Emit () */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
				ILGenerator ig = mb.GetILGenerator ();
				ig.Emit (OpCodes.Ldnull);
				ig.Emit (OpCodes.Castclass, t);
				ig.Emit (OpCodes.Pop);
				ig.Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* DeclareLocal () */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
				ILGenerator ig = mb.GetILGenerator ();
				ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* BeginExceptionCatchBlock () */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
				ILGenerator ig = mb.GetILGenerator ();
				ig.BeginExceptionBlock ();
				ig.BeginCatchBlock (t);
				ig.Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}

			try {
				/* EmitCalli () */
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, typeof (object));
				MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, CallingConventions.Standard, typeof (int), new Type [] { });
				ILGenerator ig = mb.GetILGenerator ();
				ig.EmitCalli (OpCodes.Call, CallingConventions.Standard, typeof (void), new Type [] { t }, null);
				ig.Emit (OpCodes.Ret);
				tb.CreateType ();
			} catch {
			}
		}

		//Test for #572660
        [Test]
        public void CircularArrayType()
        {
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Test"), AssemblyBuilderAccess.RunAndSave);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("Test", "Test.dll", true);
			var typeBuilder = moduleBuilder.DefineType("Foo", TypeAttributes.Public);
			var fieldBuilder = typeBuilder.DefineField("Foos", typeBuilder.MakeArrayType(), FieldAttributes.Public);

			var fooType = typeBuilder.CreateType();
			Assert.AreSame(fooType.MakeArrayType(), fooType.GetField("Foos").FieldType);
        }


		[Test] //Test for #422113
		[ExpectedException (typeof (TypeLoadException))]
		public void CreateInstanceOfIncompleteType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Class, null, new Type[] { typeof (IComparable) });
			Type proxyType = tb.CreateType();
			Activator.CreateInstance(proxyType);
		}

		[Test] //Test for #640780
		public void StaticMethodNotUsedInIfaceVtable ()
		{
			TypeBuilder tb1 = module.DefineType("Interface", TypeAttributes.Interface | TypeAttributes.Abstract);
			tb1.DefineTypeInitializer().GetILGenerator().Emit(OpCodes.Ret);
			tb1.DefineMethod("m", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract);
			tb1.CreateType();
			
			TypeBuilder tb2 = module.DefineType("Class", TypeAttributes.Sealed);
			tb2.AddInterfaceImplementation(tb1);
			tb2.DefineMethod("m", MethodAttributes.Public | MethodAttributes.Virtual)
			    .GetILGenerator().Emit(OpCodes.Ret);
			tb2.DefineDefaultConstructor(MethodAttributes.Public);
			
			Activator.CreateInstance(tb2.CreateType());
		}

		[Test] //Test for #648391
		public void GetConstructorCheckCtorDeclaringType ()
		{
			TypeBuilder myType = module.DefineType ("Sample", TypeAttributes.Public);
			string[] typeParamNames = { "TFirst" };
			GenericTypeParameterBuilder[] typeParams = myType.DefineGenericParameters (typeParamNames);
			var ctor = myType.DefineDefaultConstructor (MethodAttributes.Public);
			var ctori = TypeBuilder.GetConstructor (myType.MakeGenericType (typeof (int)), ctor);
			try {
				TypeBuilder.GetConstructor (myType.MakeGenericType (typeof (bool)), ctori);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
				//OK
			}
		}
	}
}
