
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
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using NUnit.Framework;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace MonoTests.System.Reflection.Emit
{
	public interface EmptyInterface {

	}

	public interface OneMethodInterface {
		void foo ();
	}

	[TestFixture]
	public class TypeBuilderTest
	{
		private interface AnInterface
		{
		}

		interface Foo
		{
		}

		interface Bar : Foo
		{
		}

		interface Baz : Bar
		{
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGUIDIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Guid g = tb.GUID;
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
#if NET_2_0
			Assert.IsFalse (tb.HasElementType);
#else
			try {
				bool b = tb.HasElementType;
				Assert.Fail ("#1: " + b);
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
#endif
		}

		[Test]
#if ONLY_1_1
		[Category ("NotWorking")]
#endif
		public void TestHasElementType_Complete ()
		{
			// According to the MSDN docs, this member works, but in reality, it
			// returns a NotSupportedException
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
#if NET_2_0
			Assert.IsFalse (tb.HasElementType);
#else
			try {
				bool b = tb.HasElementType;
				Assert.Fail ("#1: " + b);
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
#endif
		}

		[Test] //bug 82018
		public void TestEnumWithoutValueFieldThrowsException ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (),
				TypeAttributes.Sealed | TypeAttributes.Serializable,
				typeof (Enum));

			try {
				tb.CreateType ();
				Assert.Fail ("#1: must throw TypeLoadException");
			} catch (TypeLoadException) {
			}

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
		}

		[Test] // bug 82018
#if ONLY_1_1
		[Category ("NotWorking")] // we do not throw IOE when repeatedly invoking CreateType
#endif
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

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#B1");
			Assert.IsNull (tb.CreateType (), "#B2");
			//Assert.IsTrue (tb.IsCreated (), "#B3");
#else
			try {
				tb.CreateType ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#endif
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

#if NET_2_0
			Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
				//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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

#if NET_2_0
			//Assert.IsTrue (tb.IsCreated (), "#2");
#endif
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
			AssertEquals ("",
						  "E", tb4.Name);
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
			AssertEquals ("",
						  "E", tb4.Name);
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
		public void TestSetParentNull ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Class,
				typeof (Attribute));
#if NET_2_0
			tb.SetParent (null);
			Assert.AreEqual (typeof (object), tb.BaseType, "#A1");
#else
			try {
				tb.SetParent (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("parent", ex.ParamName, "#A6");
			}
#endif

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract);
#if NET_2_0
			tb.SetParent (null);
			Assert.IsNull (tb.BaseType, "#B1");
#else
			try {
				tb.SetParent (null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("parent", ex.ParamName, "#B6");
			}
#endif

#if NET_2_0
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
#endif
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

#if NET_2_0
			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract, typeof (IDisposable));
			tb.SetParent (typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType, "#4");

			tb = module.DefineType (genTypeName (), TypeAttributes.Interface |
				TypeAttributes.Abstract, typeof (IDisposable));
			tb.SetParent (typeof (ICloneable));
			Assert.AreEqual (typeof (ICloneable), tb.BaseType, "#5");
#endif
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestSetParentComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			tb.SetParent (typeof (Attribute));
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestTypeHandle ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			RuntimeTypeHandle handle = tb.TypeHandle;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestTypeInitializerIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorInfo cb = tb.TypeInitializer;
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
		[Category ("NotWorking")]
		public void TestUnderlyingSystemType ()
		{
			{
				TypeBuilder tb = module.DefineType (genTypeName ());
				Assert.AreEqual (tb, tb.UnderlyingSystemType, "#1");
			}
			{
				TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
				Assert.AreEqual (tb, tb.UnderlyingSystemType, "#2");
			}
			{
				TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (ValueType));
				Assert.AreEqual (tb, tb.UnderlyingSystemType, "#3");
			}

			{
				TypeBuilder tb = module.DefineType (genTypeName (), 0, typeof (Enum));
				try {
					Type t = tb.UnderlyingSystemType;
					Assert.Fail ("#4");
				} catch (InvalidOperationException) {
				}

				tb.DefineField ("val", typeof (int), 0);
				Assert.AreEqual (typeof (int), tb.UnderlyingSystemType, "#5");
			}
		}

		[Test]
		public void TestAddInterfaceImplementation ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			try {
				tb.AddInterfaceImplementation (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

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
#if ONLY_1_1
		[Category ("NotWorking")] // we allow CreateType to be invoked multiple times
#endif
		public void TestCreateType_Created ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
#if NET_2_0
			Assert.IsFalse (tb.IsCreated (), "#A1");
#endif
			Type emittedType1 = tb.CreateType ();
#if NET_2_0
			Assert.IsTrue (tb.IsCreated (), "#A2");
#endif
			Assert.IsNotNull (emittedType1, "#A3");

#if NET_2_0
			Type emittedType2 = tb.CreateType ();
			Assert.IsNotNull (emittedType2, "#B1");
			Assert.IsTrue (tb.IsCreated (), "#B2");
			Assert.AreSame (emittedType1, emittedType2, "#B3");
#else
			try {
				tb.CreateType ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#endif
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
				Assert.Fail ();
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestDefineDefaultConstructor ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineDefaultConstructor (0);
			tb.CreateType ();

			// Can not be called on a created type, altough the MSDN docs does not mention this
			try {
				tb.DefineDefaultConstructor (0);
				Assert.Fail ();
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestDefineDefaultConstructorParent ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.DefineConstructor (MethodAttributes.Public,
				CallingConventions.Standard,
				new Type [] { typeof (string) });
			Type type = tb.CreateType ();

			// create TypeBuilder for type that derived from the 
			// previously created type (which has no default ctor)
			tb = module.DefineType (genTypeName (), TypeAttributes.Class
				| TypeAttributes.Public, type);

			// you cannot create a type with a default ctor that
			// derives from a type without a default ctor
			tb.CreateType ();
		}

		[Test]
		public void TestDefineEvent ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Test invalid arguments
			try {
				tb.DefineEvent (null, 0, typeof (int));
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineEvent ("FOO", 0, null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineEvent ("", 0, typeof (int));
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			tb.CreateType ();
			// Can not be called on a created type
			try {
				tb.DefineEvent ("BAR", 0, typeof (int));
				Assert.Fail ("#4");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestDefineField ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineField (null, typeof (int), 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineField ("", typeof (int), 0);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				// Strangely, 'A<NULL>' is accepted...
				string name = String.Format ("{0}", (char) 0);
				tb.DefineField (name, typeof (int), 0);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				tb.DefineField ("A", typeof (void), 0);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			tb.CreateType ();
			// Can not be called on a created type
			try {
				tb.DefineField ("B", typeof (int), 0);
				Assert.Fail ("#5");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void TestDefineInitializedData ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineInitializedData (null, new byte [1], 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineInitializedData ("FOO", null, 0);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineInitializedData ("", new byte [1], 0);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			// The size of the data is less than or equal to zero ???
			try {
				tb.DefineInitializedData ("BAR", new byte [0], 0);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				string name = String.Format ("{0}", (char) 0);
				tb.DefineInitializedData (name, new byte [1], 0);
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			tb.CreateType ();

			// Can not be called on a created type, altough the MSDN docs does not mention this
			try {
				tb.DefineInitializedData ("BAR2", new byte [1], 0);
				Assert.Fail ("#6");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		public void DefineUninitializedDataInvalidArgs ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			try {
				tb.DefineUninitializedData (null, 1, 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineUninitializedData ("", 1, 0);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			// The size of the data is less than or equal to zero ???
			try {
				tb.DefineUninitializedData ("BAR", 0, 0);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				string name = String.Format ("{0}", (char) 0);
				tb.DefineUninitializedData (name, 1, 0);
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DefineUninitializedDataAlreadyCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			tb.DefineUninitializedData ("BAR2", 1, 0);
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
		public void TestDefineMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineMethod (null, 0, null, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineMethod ("", 0, null, null);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			// Check non-virtual methods on an interface
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);
			try {
				tb2.DefineMethod ("FOO", MethodAttributes.Abstract, null, null);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			// Check static methods on an interface
			tb2.DefineMethod ("BAR", MethodAttributes.Public | MethodAttributes.Static,
							  typeof (void),
							  Type.EmptyTypes);

			tb.CreateType ();
			// Can not be called on a created type
			try {
				tb.DefineMethod ("bar", 0, null, null);
				Assert.Fail ("#4");
			} catch (InvalidOperationException) {
			}
		}

		// TODO: DefineMethodOverride

		[Test]
		public void TestDefineNestedType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check invalid arguments
			try {
				tb.DefineNestedType (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				tb.DefineNestedType ("");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				tb.DefineNestedType (nullName ());
				Assert.Fail ("#3");
			} catch (ArgumentException) {
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
				Assert.Fail ("#5");
			} catch (ArgumentException) {
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

				Assert.AreEqual ("N1", nested.Name, "#6");
				Assert.AreEqual (typeof (object), nested.BaseType, "#7");
				Assert.AreEqual (TypeAttributes.NestedPrivate, nested.Attributes, "#8");
				Assert.AreEqual (0, nested.GetInterfaces ().Length, "#9");
			}

			// TODO:
		}

		[Test]
		public void TestDefinePInvokeMethod ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			tb.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);

			// Try invalid parameters
			try {
				tb.DefinePInvokeMethod (null, "B", "C", 0, 0, null, null, 0, 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
			// etc...

			// Try invalid attributes
			try {
				tb.DefinePInvokeMethod ("A2", "B", "C", MethodAttributes.Abstract, 0, null, null, 0, 0);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			// Try an interface parent
			TypeBuilder tb2 = module.DefineType (genTypeName (), TypeAttributes.Interface | TypeAttributes.Abstract);

			try {
				tb2.DefinePInvokeMethod ("A", "B", "C", 0, 0, null, null, 0, 0);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TestDefineProperty ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			// Check null parameter types
			try {
				tb.DefineProperty ("A", 0, null, new Type [1]);
				Assert.Fail ();
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NotWorking")]
		public void TestIsDefinedIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.IsDefined (typeof (int), true);
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
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetCustomAttributesIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetCustomAttributes (false);
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetCustomAttributesOfTypeIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetCustomAttributes (typeof (ObsoleteAttribute), false);
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetCustomAttributesOfNullTypeComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();
			tb.GetCustomAttributes (null, false);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetEventsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetEvents ();
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetEventsFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetEvents (BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetEventIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetEvent ("FOO");
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetEventFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetEvent ("FOO", BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetFieldsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetFields ();
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

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetFieldsFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetFields (BindingFlags.Instance | BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetFieldIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetField ("test");
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetFieldFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetField ("test", BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetPropertiesIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetProperties ();
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
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetPropertiesFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetProperties (BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetPropertyIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetProperty ("test");
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
				Assert.Fail ();
			} catch (NotSupportedException) { }
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetPropertyFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetProperty ("test", BindingFlags.Public);
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
				Assert.Fail ();
			} catch (NotSupportedException) { }
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetMethodsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetMethods ();
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGetMethodsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			MethodBuilder helloMethod = tb.DefineMethod ("HelloMethod",
				MethodAttributes.Public, typeof (string), new Type [0]);
			ILGenerator helloMethodIL = helloMethod.GetILGenerator ();
			helloMethodIL.Emit (OpCodes.Ldstr, "Hi! ");
			helloMethodIL.Emit (OpCodes.Ldarg_1);
			MethodInfo infoMethod = typeof (string).GetMethod ("Concat",
				new Type [] { typeof (string), typeof (string) });
			helloMethodIL.Emit (OpCodes.Call, infoMethod);
			helloMethodIL.Emit (OpCodes.Ret);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (typeof (object).GetMethods (BindingFlags.Public | BindingFlags.Instance).Length + 1,
				tb.GetMethods ().Length);
			Assert.AreEqual (tb.GetMethods ().Length, emittedType.GetMethods ().Length);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Ignore ("mcs depends on this")]
		public void TestGetMethodsFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetMethods (BindingFlags.Public);
		}

		[Test]
		public void TestGetMethodsFlagsComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			MethodBuilder helloMethod = tb.DefineMethod ("HelloMethod",
				MethodAttributes.Public, typeof (string), new Type [0]);
			ILGenerator helloMethodIL = helloMethod.GetILGenerator ();
			helloMethodIL.Emit (OpCodes.Ldstr, "Hi! ");
			helloMethodIL.Emit (OpCodes.Ldarg_1);
			MethodInfo infoMethod = typeof (string).GetMethod ("Concat",
				new Type [] { typeof (string), typeof (string) });
			helloMethodIL.Emit (OpCodes.Call, infoMethod);
			helloMethodIL.Emit (OpCodes.Ret);

			Type emittedType = tb.CreateType ();

			Assert.AreEqual (1, tb.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Length);
			Assert.AreEqual (tb.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length,
				emittedType.GetMethods (BindingFlags.Instance | BindingFlags.Public).Length);
			Assert.AreEqual (0, tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Length);
			Assert.AreEqual (tb.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length,
				emittedType.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic).Length);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetMemberIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetMember ("FOO", MemberTypes.All, BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetMembersIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetMembers ();
		}

		[Test]
		public void TestGetMembersComplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			Type emittedType = tb.CreateType ();

			Assert.AreEqual (tb.GetMembers ().Length, emittedType.GetMembers ().Length);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetMembersFlagsIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetMembers (BindingFlags.Public);
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
		[ExpectedException (typeof (NotSupportedException))]
		public void TestGetInterfaceIncomplete ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.GetInterface ("FOO", true);
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
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddDeclarativeSecurityAlreadyCreated ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			tb.CreateType ();

			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddDeclarativeSecurityNullPermissionSet ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			tb.AddDeclarativeSecurity (SecurityAction.Demand, null);
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
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddDeclarativeSecurityDuplicateAction ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());

			PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
			tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			tb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		}

		[Test]
		public void TestEnums ()
		{
			TypeAttributes typeAttrs = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
			TypeBuilder enumToCreate = module.DefineType (genTypeName (), typeAttrs,
														 typeof (Enum));
			enumToCreate.SetCustomAttribute (new CustomAttributeBuilder (typeof (FlagsAttribute).GetConstructors () [0], new Type [0]));
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
			}
#if NET_2_0
			Assert.IsTrue (typeBuilder.IsCreated (), "#2");
			Assert.IsNull (typeBuilder.CreateType (), "#3");
#else
			try {
				typeBuilder.CreateType ();
			} catch (InvalidOperationException ex) {
				// Unable to change after type has been created
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
#endif
		}

		[Test]
		public void SetCustomAttribute_SuppressUnmanagedCodeSecurity ()
		{
			TypeBuilder tb = module.DefineType (genTypeName ());
			ConstructorInfo attrCtor = typeof (SuppressUnmanagedCodeSecurityAttribute).
				GetConstructor (new Type [0]);
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

		private void DefineStringProperty (TypeBuilder tb, string propertyName, string fieldName, MethodAttributes methodAttribs)
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
		public void TestIsAssignableTo ()
		{
			Type icomparable = typeof (IComparable);

			TypeBuilder tb = module.DefineType (genTypeName (),
												TypeAttributes.Public, null, new Type [] { icomparable, typeof (Bar) });

			Assert.IsTrue (icomparable.IsAssignableFrom (tb), "#1");
			Assert.IsFalse (tb.IsAssignableFrom (icomparable), "#2");

			Assert.IsTrue (typeof (Bar).IsAssignableFrom (tb), "#3");
			Assert.IsFalse (typeof (Baz).IsAssignableFrom (tb), "#4");

			Assert.IsTrue (tb.IsAssignableFrom (tb), "#5");

			Assert.IsFalse (tb.IsAssignableFrom (typeof (IDisposable)), "#6");
			tb.AddInterfaceImplementation (typeof (IDisposable));

			// Fails under .net, so we don't support it either
			//Assert.IsTrue (tb.IsAssignableFrom (typeof (IDisposable)), "#7");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestIsAssignableTo_NotDotNet ()
		{
			Type icomparable = typeof (IComparable);

			TypeBuilder tb = module.DefineType (genTypeName (),
												TypeAttributes.Public, null, new Type [] { icomparable, typeof (Bar) });

			Assert.IsTrue (typeof (Foo).IsAssignableFrom (tb), "#1");

			tb.AddInterfaceImplementation (typeof (IDisposable));

			// bug #73469
			Assert.IsTrue (typeof (Bar []).IsAssignableFrom (module.GetType (tb.FullName + "[]")), "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EmptyMethodBody ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			tb.DefineMethod ("foo", MethodAttributes.Public, typeof (void), new Type [] { });
			tb.CreateType ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EmptyCtorBody ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);

			tb.DefineConstructor (0, CallingConventions.Standard, null);
			tb.CreateType ();
		}

		[Test]
		public void ParentNull ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public, null);
			Type t = tb.CreateType ();

			Assert.AreEqual (typeof (object), t.BaseType);
		}

#if NET_2_0
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
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.DefineGenericParameters ("T");

			Type t1 = tb.MakeGenericType (typeof (int));
			Assert.IsTrue (t1.IsGenericType, "#A1");
			Assert.IsFalse (t1.IsGenericTypeDefinition, "#A2");
			Assert.IsFalse (t1.ContainsGenericParameters, "#A3");
			Assert.IsFalse (t1.IsGenericParameter, "#A4");

			Type t2 = tb.MakeGenericType (typeof (List<>).GetGenericArguments ());
			Assert.IsTrue (t2.IsGenericType, "#B1");
			Assert.IsFalse (t2.IsGenericTypeDefinition, "#B2");
			Assert.IsTrue (t2.ContainsGenericParameters, "#B3");
			Assert.IsFalse (t2.IsGenericParameter, "#B4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Fail_MakeGenericType ()
		{
			TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
			tb.MakeGenericType (typeof (int));
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
#endif
	}
}
