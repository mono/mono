//
// ModuleBuilderTest - NUnit Test Cases for the ModuleBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//


using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class ModuleBuilderTest
	{
		static string tempDir = Path.Combine (Path.GetTempPath (), typeof (ModuleBuilderTest).FullName);
		static int nameIndex = 0;

		[SetUp]
		public void SetUp ()
		{
			Random AutoRand = new Random ();
			string basePath = tempDir;
			while (Directory.Exists (tempDir))
				tempDir = Path.Combine (basePath, AutoRand.Next ().ToString ());
			Directory.CreateDirectory (tempDir);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				// This throws an exception under MS.NET, since the directory contains loaded
				// assemblies.
				Directory.Delete (tempDir, true);
			} catch (Exception) {
			}
		}

		private AssemblyName genAssemblyName ()
		{
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = typeof (ModuleBuilderTest).FullName + (nameIndex ++);
			return assemblyName;
		}

		private AssemblyBuilder genAssembly ()
		{
			return Thread.GetDomain ().DefineDynamicAssembly (genAssemblyName (),
															  AssemblyBuilderAccess.RunAndSave,
															  tempDir);
		}

		[Test]
		public void TestIsTransient ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb1 = ab.DefineDynamicModule ("foo.dll");
			Assert.IsTrue (mb1.IsTransient (), "#1");
			ModuleBuilder mb2 = ab.DefineDynamicModule ("foo2.dll", "foo2.dll");
			Assert.IsFalse (mb2.IsTransient (), "#2");
		}

		// Some of these tests overlap with the tests for Module

		[Test]
		public void TestGlobalData ()
		{
			AssemblyBuilder ab = genAssembly ();

			string resfile = Path.Combine (tempDir, "res");
			using (StreamWriter sw = new StreamWriter (resfile)) {
				sw.WriteLine ("FOO");
			}

			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			mb.DefineInitializedData ("DATA", new byte [100], FieldAttributes.Public);
			mb.DefineInitializedData ("DATA2", new byte [100], FieldAttributes.Public);
			mb.DefineInitializedData ("DATA3", new byte [99], FieldAttributes.Public);
			mb.DefineUninitializedData ("DATA4", 101, FieldAttributes.Public);
			mb.DefineInitializedData ("DATA_PRIVATE", new byte [100], 0);
			mb.CreateGlobalFunctions ();

			ab.Save ("foo.dll");

			Assembly assembly = Assembly.LoadFrom (Path.Combine (tempDir, "foo.dll"));

			Module module = assembly.GetLoadedModules () [0];

			string [] expectedFieldNames = new string [] {
				"DATA", "DATA2", "DATA3", "DATA4" };
			ArrayList fieldNames = new ArrayList ();
			foreach (FieldInfo fi in module.GetFields ()) {
				fieldNames.Add (fi.Name);
			}
			AssertArrayEqualsSorted (expectedFieldNames, fieldNames.ToArray (typeof (string)));

			Assert.IsNotNull (module.GetField ("DATA"), "#1");
			Assert.IsNotNull (module.GetField ("DATA2"), "#2");
			Assert.IsNotNull (module.GetField ("DATA3"), "#3");
			Assert.IsNotNull (module.GetField ("DATA4"), "#4");
			Assert.IsNull (module.GetField ("DATA_PRIVATE"), "#5");
			Assert.IsNotNull (module.GetField ("DATA_PRIVATE", BindingFlags.NonPublic | BindingFlags.Static), "#6");
		}

		[Test]
		[Category("NotWorking")]
		public void TestGlobalMethods ()
		{
			AssemblyBuilder builder = genAssembly ();
			ModuleBuilder module = builder.DefineDynamicModule ("MessageModule");
			MethodBuilder method = module.DefinePInvokeMethod ("printf", "libc.so",
															  MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
															  CallingConventions.Standard, typeof (void), new Type [] { typeof (string) }, CallingConvention.Winapi,
															  CharSet.Auto);
			method.SetImplementationFlags (MethodImplAttributes.PreserveSig |
										   method.GetMethodImplementationFlags ());

			Assert.IsNull (module.GetMethod ("printf"), "#1");

			module.CreateGlobalFunctions ();

			Assert.IsNotNull (module.GetMethod ("printf"), "#2");
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_Name_Null ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);
			try {
				mb.DefineType ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("fullname", ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_Name_Empty ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);
			try {
				mb.DefineType (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("fullname", ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_Name_NullChar ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);
			try {
				mb.DefineType ("\0test");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal name
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("fullname", ex.ParamName, "#5");
			}

			mb.DefineType ("te\0st");
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_InterfaceNotAbstract ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			try {
				mb.DefineType ("ITest1", TypeAttributes.Interface);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Interface must be declared abstract
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				mb.DefineType ("ITest2", TypeAttributes.Interface, (Type) null);
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// Interface must be declared abstract
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			// fail on MS .NET 1.1
			TypeBuilder tb = mb.DefineType ("ITest2", TypeAttributes.Interface,
				typeof (object));
			Assert.AreEqual (typeof (object), tb.BaseType, "#C1");

			tb = mb.DefineType ("ITest3", TypeAttributes.Interface,
				typeof (IDisposable));
			Assert.AreEqual (typeof (IDisposable), tb.BaseType, "#D1");
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_Parent_Interface ()
		{
			TypeBuilder tb;

			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			tb = mb.DefineType ("Foo", TypeAttributes.Class,
				typeof (ICollection));
			Assert.AreEqual (typeof (ICollection), tb.BaseType, "#1");

			tb = mb.DefineType ("Bar", TypeAttributes.Interface,
				typeof (ICollection));
			Assert.AreEqual (typeof (ICollection), tb.BaseType, "#2");
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DefineType_TypeSize ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			TypeBuilder tb = mb.DefineType ("Foo", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout,
				typeof (ValueType), 1);
			Assert.AreEqual (1, tb.Size);
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		[ExpectedException (typeof (ArgumentException))]
		public void DuplicateTypeName () {
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder module = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			var itb = module.DefineType ("TBase", TypeAttributes.Public);

			itb.SetParent (typeof(ValueType));        

			var ptb = module.DefineType ("TBase", TypeAttributes.Public);

			ptb.SetParent (typeof(Enum));
		}

		[Test]
		[Category ("AndroidNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
		public void DuplicateSymbolDocument ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			// Check that it is possible to redefine a symbol document
			ISymbolDocumentWriter doc1 =
				mb.DefineDocument ("foo.il", SymDocumentType.Text,
								  SymLanguageType.ILAssembly, SymLanguageVendor.Microsoft);
			ISymbolDocumentWriter doc2 =
				mb.DefineDocument ("foo.il", SymDocumentType.Text,
								  SymLanguageType.ILAssembly, SymLanguageVendor.Microsoft);
		}

		[Test] // Test case for #80435.
		public void GetArrayMethodToStringTest ()
		{
			AssemblyBuilder assembly = genAssembly ();
			ModuleBuilder module = assembly.DefineDynamicModule ("m", "test.dll");

			Type [] myArrayClass = new Type [1];
			Type [] parameterTypes = { typeof (Array) };
			MethodInfo myMethodInfo = module.GetArrayMethod (myArrayClass.GetType (), "Sort", CallingConventions.Standard, null, parameterTypes);

			string str = myMethodInfo.ToString ();
			Assert.IsNotNull (str);
			// Don't compare string, since MS returns System.Reflection.Emit.SymbolMethod here 
			// (they do not provide an implementation of ToString).
		}

		private static void AssertArrayEqualsSorted (Array o1, Array o2)
		{
			Array s1 = (Array) o1.Clone ();
			Array s2 = (Array) o2.Clone ();

			Array.Sort (s1);
			Array.Sort (s2);

			Assert.AreEqual (s1.Length, s2.Length, "#1");
			for (int i = 0; i < s1.Length; ++i)
				Assert.AreEqual (s1.GetValue (i), s2.GetValue (i), "#2: " + i);
		}

		[Test]
		public void ResolveFieldTokenFieldBuilder ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb = mb.DefineType ("foo");
			FieldBuilder fb = tb.DefineField ("foo", typeof (int), 0);
			tb.CreateType ();

			FieldInfo fi = mb.ResolveField (0x04000001);
			Assert.IsNotNull (fi);
			Assert.AreEqual ("foo", fi.Name);
		}

		[Test]
		public void ResolveGenericFieldBuilderOnGenericTypeBuilder ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb = mb.DefineType ("Foo`1");
			var t = tb.DefineGenericParameters ("T") [0];
			FieldBuilder fb = tb.DefineField ("foo", t, 0);
			tb.CreateType ();

			FieldInfo fi = mb.ResolveField (0x04000001);
			Assert.IsNotNull (fi);
			Assert.AreEqual ("foo", fi.Name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ResolveFieldTokenInvalidToken ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			mb.ResolveField (0x4001234);
		}

		[Test]
		public void ResolveMethodTokenMethodBuilder ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder moduleb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb = moduleb.DefineType ("foo");
			MethodBuilder mb = tb.DefineMethod("Frub", MethodAttributes.Static, null, new Type[] { typeof(IntPtr) });
			int tok = mb.GetToken().Token;
			mb.SetImplementationFlags(MethodImplAttributes.NoInlining);
			ILGenerator ilgen = mb.GetILGenerator();
			ilgen.Emit(OpCodes.Ret);

			tb.CreateType ();

			MethodBase mi = moduleb.ResolveMethod (tok);
			Assert.IsNotNull (mi);
			Assert.AreEqual ("Frub", mi.Name);
		}

		[Test]
		public void GetMethodTokenCrossMethodBuilders ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder moduleb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb = moduleb.DefineType ("foo");
			MethodBuilder mb = tb.DefineMethod("Frub", MethodAttributes.Static, null, new Type[] { typeof(IntPtr) });
			int tok = mb.GetToken().Token;
			mb.SetImplementationFlags(MethodImplAttributes.NoInlining);
			ILGenerator ilgen = mb.GetILGenerator();
			ilgen.Emit(OpCodes.Ret);

			tb.CreateType ();

			var mi = (MethodInfo) moduleb.ResolveMember (tok);
			Assert.IsNotNull (mi);

			ModuleBuilder moduleb2 = ab.DefineDynamicModule ("foo2.dll", "foo2.dll");
			var tok2 = moduleb2.GetMethodToken (mi).Token;

			MethodBase mi2 = moduleb.ResolveMethod (tok2);
			Assert.IsNotNull (mi2);
			Assert.AreEqual ("Frub", mi.Name);
		}

		[Test]
		public void ResolveMemberField ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo");
			var method = type.DefineMethod ("Str", MethodAttributes.Static, typeof (string), Type.EmptyTypes);
			var il = method.GetILGenerator ();

			il.Emit (OpCodes.Ldsfld, typeof (string).GetField ("Empty"));
			il.Emit (OpCodes.Ret);

			type.CreateType ();

			var string_empty = (FieldInfo) module.ResolveMember (0x0a000001);
			Assert.IsNotNull (string_empty);
			Assert.AreEqual ("Empty", string_empty.Name);
			Assert.AreEqual (typeof (string), string_empty.DeclaringType);
		}

		[Test]
		public void ResolveMemberMethod ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo");
			var method = type.DefineMethod ("Str", MethodAttributes.Static, typeof (void), Type.EmptyTypes);
			var il = method.GetILGenerator ();

			il.Emit (OpCodes.Call, typeof (Console).GetMethod ("WriteLine", Type.EmptyTypes));
			il.Emit (OpCodes.Ret);

			type.CreateType ();

			var writeline = (MethodInfo) module.ResolveMember (0x0a000001);
			Assert.IsNotNull (writeline);
			Assert.AreEqual ("WriteLine", writeline.Name);
			Assert.AreEqual (typeof (Console), writeline.DeclaringType);
		}

		[Test]
		public void ResolveMethodDefWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			var method = type.DefineMethod ("Method", MethodAttributes.Static, typeof (void), new Type [] { t });
			method.GetILGenerator ().Emit (OpCodes.Ret);

			type.DefineDefaultConstructor (MethodAttributes.Public);

			type.CreateType ();

			var resolved_method = (MethodInfo) module.ResolveMember (0x06000001, new [] { typeof (string) }, Type.EmptyTypes);
			Assert.IsNotNull (resolved_method);
			Assert.AreEqual ("Method", resolved_method.Name);
			Assert.IsTrue (resolved_method.GetParameters () [0].ParameterType.IsGenericParameter);
		}

		[Test]
		public void ResolveFieldDefWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			var field = type.DefineField ("field", t, FieldAttributes.Public);

			type.CreateType ();

			var resolved_field = (FieldInfo) module.ResolveMember (0x04000001, new [] { typeof (string) }, Type.EmptyTypes);
			Assert.IsNotNull (resolved_field);
			Assert.AreEqual ("field", resolved_field.Name);
			Assert.IsTrue (resolved_field.FieldType.IsGenericParameter);
		}

		[Test]
		public void ResolveTypeDefWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			type.CreateType ();

			var foo = (Type) module.ResolveMember (0x02000002, new [] { typeof (string) }, Type.EmptyTypes);
			Assert.IsNotNull (foo);
			Assert.AreEqual ("Foo`1", foo.Name);
			Assert.IsTrue (foo.IsGenericTypeDefinition);
		}

		[Test]
		// The token is not guaranteed to be 0x0a000001
		[Category ("NotWorking")]
		public void ResolveFieldMemberRefWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			var field = type.DefineField ("field", t, FieldAttributes.Public);

			var method = type.DefineMethod ("Method", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
			var il = method.GetILGenerator ();

			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldfld, field); // this triggers the creation of a MemberRef on a generic TypeSpec
			il.Emit (OpCodes.Pop);
			il.Emit (OpCodes.Ret);

			type.CreateType ();

			var resolved_field = (FieldInfo) module.ResolveMember (0x0a000001, new [] { typeof (string) }, null);
			Assert.IsNotNull (resolved_field);
			Assert.AreEqual ("field", resolved_field.Name);
			Assert.AreEqual (typeof (string), resolved_field.FieldType);
		}

		[Test]
		// The token is not guaranteed to be 0x0a000002
		[Category ("NotWorking")]
		public void ResolveMethodMemberRefWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			var field = type.DefineField ("field", t, FieldAttributes.Public);

			var method = type.DefineMethod ("Method", MethodAttributes.Public, typeof (void), new Type [] { t });
			method.GetILGenerator ().Emit (OpCodes.Ret);

			var ctor = type.DefineMethod ("Caller", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
			var il = ctor.GetILGenerator ();

			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldfld, field); // this triggers the creation of a MemberRef on a generic TypeSpec
			il.Emit (OpCodes.Callvirt, method); // this too
			il.Emit (OpCodes.Ret);

			type.DefineDefaultConstructor (MethodAttributes.Public);

			type.CreateType ();

			var resolved_method = (MethodInfo) module.ResolveMember (0x0a000002, new [] { typeof (string) }, null);
			Assert.IsNotNull (resolved_method);
			Assert.AreEqual ("Method", resolved_method.Name);
			Assert.AreEqual (typeof (string), resolved_method.GetParameters () [0].ParameterType);
		}

		[Test]
		// The token is not guaranteed to be 0x2b000001
		[Category("NotWorking")]
		public void ResolveMethodSpecWithGenericArguments ()
		{
			var assembly = genAssembly ();
			var module = assembly.DefineDynamicModule ("foo.dll", "foo.dll");

			var type = module.DefineType ("Foo`1");
			var t = type.DefineGenericParameters ("T") [0];

			var field = type.DefineField ("field", t, FieldAttributes.Public);

			var method = type.DefineMethod ("Method", MethodAttributes.Public);
			var s = method.DefineGenericParameters ("S") [0];
			method.SetReturnType (typeof (void));
			method.SetParameters (t, s);
			method.GetILGenerator ().Emit (OpCodes.Ret);

			var ctor = type.DefineMethod ("Caller", MethodAttributes.Public, typeof (void), Type.EmptyTypes);
			var il = ctor.GetILGenerator ();

			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldfld, field); // this triggers the creation of a MemberRef on a generic TypeSpec
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldfld, field); // this triggers the creation of a MemberRef on a generic TypeSpec
			il.Emit (OpCodes.Callvirt, method); // this triggers the creation of a MethodSpec
			il.Emit (OpCodes.Ret);

			type.DefineDefaultConstructor (MethodAttributes.Public);

			type.CreateType ();

			var resolved_method = (MethodInfo) module.ResolveMember (0x2b000001, new [] { typeof (string) }, new [] { typeof (int) });
			Assert.IsNotNull (resolved_method);
			Assert.AreEqual ("Method", resolved_method.Name);
			Assert.AreEqual (typeof (string), resolved_method.GetParameters () [0].ParameterType);
			Assert.AreEqual (typeof (int), resolved_method.GetParameters () [1].ParameterType);
		}

		[Test]
		public void GetTypes ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb1 = mb.DefineType("Foo", TypeAttributes.Public);

			Type[] types = mb.GetTypes ();
			Assert.AreEqual (1, types.Length);
			Assert.AreEqual (tb1, types [0]);

			// After the type is created, MS seems to return the created type
			tb1.CreateType ();

			types = mb.GetTypes ();
			Assert.AreEqual (tb1.CreateType (), types [0]);
		}

		[Test] // GetTypeToken (Type)
		[Category ("NotDotNet")] // http://support.microsoft.com/kb/950986
		public void GetTypeToken2_Type_Array ()
		{
			Type type;
			TypeToken typeToken;
			Type resolved_type;

			AssemblyName aname = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder mb = ab.DefineDynamicModule ("MyModule");

			type = typeof (object []);
			typeToken = mb.GetTypeToken (type);
			Assert.IsFalse (typeToken == TypeToken.Empty, "#A1");
			resolved_type = mb.ResolveType (typeToken.Token);
			Assert.AreEqual (type, resolved_type, "#A2");

			type = typeof (object).MakeArrayType ();
			typeToken = mb.GetTypeToken (type);
			Assert.IsFalse (typeToken == TypeToken.Empty, "#B1");
			resolved_type = mb.ResolveType (typeToken.Token);
			Assert.AreEqual (type, resolved_type, "#B2");
		}

		[Test] // GetTypeToken (Type)
		public void GetTypeToken2_Type_String ()
		{
			AssemblyName aname = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder mb = ab.DefineDynamicModule ("MyModule");
			Type type = typeof (string);
			TypeToken typeToken = mb.GetTypeToken (type);
			Assert.IsFalse (typeToken == TypeToken.Empty, "#1");
			Type resolved_type = mb.ResolveType (typeToken.Token);
			Assert.AreEqual (type, resolved_type, "#2");
		}

		[Test] // bug #471302
		public void ModuleBuilder_ModuleVersionId ()
		{
			var name = new AssemblyName () { Name = "Foo" };
			var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (
				name, AssemblyBuilderAccess.Run);

			var module = assembly.DefineDynamicModule ("Foo");

			Assert.AreNotEqual (new Guid (), module.ModuleVersionId);
		}

		[Test]
		public void GetType_String_Null ()
		{
			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = ab.DefineDynamicModule ("GetTypeNullCheck");

			try {
				module.GetType (null);
				Assert.Fail ("Expected ArgumentNullException for GetType(string)");
			}
			catch (ArgumentNullException) {
			}
			try {
				module.GetType (null, true); // ignoreCase
				Assert.Fail ("Expected ArgumentNullException for GetType(string,bool)");
			}
			catch (ArgumentNullException) {
			}
			try {
				module.GetType (null, true, true); // throwOnError, ignoreCase
				Assert.Fail ("Expected ArgumentNullException for GetType(string,bool,bool)");
			}
			catch (ArgumentNullException) {
			}
		}

		[Test]
		public void GetType_String_Empty ()
		{
			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = ab.DefineDynamicModule ("GetTypeEmptyCheck");

			try {
				module.GetType (String.Empty);
				Assert.Fail ("Expected ArgumentNullException for GetType(string)");
			}
			catch (ArgumentException) {
			}
			try {
				module.GetType (String.Empty, true); // ignoreCase
				Assert.Fail ("Expected ArgumentNullException for GetType(string,bool)");
			}
			catch (ArgumentException) {
			}
			try {
				module.GetType (String.Empty, true, true); // throwOnError, ignoreCase
				Assert.Fail ("Expected ArgumentNullException for GetType(string,bool,bool)");
			}
			catch (ArgumentException) {
			}
		}

		[Test]
		public void GetType_Escaped_Chars ()
		{
			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = ab.DefineDynamicModule ("mod");

			var tb = module.DefineType ("NameSpace,+*&[]\\.Type,+*&[]\\",
						    TypeAttributes.Class | TypeAttributes.Public);

			var nestedTb = tb.DefineNestedType ("Nested,+*&[]\\",
							    TypeAttributes.Class | TypeAttributes.NestedPublic);

			var escapedOuterName =
				"NameSpace\\,\\+\\*\\&\\[\\]\\\\"
				+ "."
				+ "Type\\,\\+\\*\\&\\[\\]\\\\";

			var escapedNestedName =
				escapedOuterName
				+ "+"
				+ "Nested\\,\\+\\*\\&\\[\\]\\\\";

			Assert.AreEqual (escapedOuterName, tb.FullName);
			Assert.AreEqual (escapedNestedName, nestedTb.FullName);

			Type outerCreatedTy = tb.CreateType ();
			Type nestedCreatedTy = nestedTb.CreateType ();
			Type outerTy = module.GetType (escapedOuterName);
			Type nestedTy = module.GetType (escapedNestedName);

			Assert.IsNotNull (outerTy, "A");
			Assert.IsNotNull (nestedTy, "B");
			Assert.AreEqual (escapedNestedName, nestedTy.FullName);


			Assert.AreEqual (nestedCreatedTy, nestedTy);

		}

		[Test]
		public void GetMethodTokenNullParam ()
		{
			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = ab.DefineDynamicModule ("mod");

			var method = typeof (object).GetMethod ("GetType");

			// ArgumentNullException should not occur.
			module.GetMethodToken (method, null);
		}

		[Test]
		public void GetConstructorTokenNullParam ()
		{
			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = ab.DefineDynamicModule ("mod");

			var method = typeof (object).GetConstructor (Type.EmptyTypes);

			// ArgumentNullException should not occur.
			module.GetConstructorToken (method, null);
		}

		[Test]
		public void GetType ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder module = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);
			TypeBuilder tb = module.DefineType ("t1", TypeAttributes.Public);

			Assert.AreEqual ("t1[]", module.GetType ("t1[]").FullName);
			Assert.AreEqual ("t1*", module.GetType ("t1*").FullName);
			Assert.AreEqual ("t1&", module.GetType ("t1&").FullName);
			Assert.AreEqual ("t1[]&", module.GetType ("t1[]&").FullName);
		}
	}
}
