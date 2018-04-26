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
		public void DefineType_Name_Null ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);
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
		public void DefineType_Name_Empty ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);
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
		public void DefineType_Name_NullChar ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);
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
		public void DefineType_InterfaceNotAbstract ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);

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
		public void DefineType_Parent_Interface ()
		{
			TypeBuilder tb;

			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);

			tb = mb.DefineType ("Foo", TypeAttributes.Class,
				typeof (ICollection));
			Assert.AreEqual (typeof (ICollection), tb.BaseType, "#1");

			tb = mb.DefineType ("Bar", TypeAttributes.Interface,
				typeof (ICollection));
			Assert.AreEqual (typeof (ICollection), tb.BaseType, "#2");
		}

		[Test]
		public void DefineType_TypeSize ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);

			TypeBuilder tb = mb.DefineType ("Foo", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout,
				typeof (ValueType), 1);
			Assert.AreEqual (1, tb.Size);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DuplicateTypeName () {
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder module = ab.DefineDynamicModule ("foo.dll", "foo.dll", false);

			var itb = module.DefineType ("TBase", TypeAttributes.Public);

			itb.SetParent (typeof(ValueType));        

			var ptb = module.DefineType ("TBase", TypeAttributes.Public);

			ptb.SetParent (typeof(Enum));
		}

		[Test]
		[Category ("MobileNotWorking")] // Missing Mono.CompilerServices.SymbolWriter assembly
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

		[Test]
		public void GetArrayMethodMultipleCallsTest ()
		{
			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder modb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			TypeBuilder tb = modb.DefineType ("TestType");
			var int2D = typeof (int[,]);
			var mb = tb.DefineMethod ("TestMeth", MethodAttributes.Static | MethodAttributes.Public,
						  typeof(int), new Type[] { int2D });
			var ilg = mb.GetILGenerator ();

			// static int TestMeth(int[,] a)
			// {
			//   int x;
			//   x = a.Get(0,0);
			//   return a.Get(0,1) + x;
			// }
			//

			var x = ilg.DeclareLocal (typeof (int));

			ilg.Emit (OpCodes.Ldarg_0);
			ilg.Emit (OpCodes.Ldc_I4_0);
			ilg.Emit (OpCodes.Ldc_I4_0);
			var arrmi = modb.GetArrayMethod (int2D, "Get",
							 CallingConventions.Standard | CallingConventions.HasThis,
							 typeof(int),
							 new Type[] { typeof(int), typeof(int) });
			ilg.Emit (OpCodes.Call, arrmi);
			ilg.Emit (OpCodes.Stloc, x);
			var arrmi2 = modb.GetArrayMethod (int2D, "Get",
							  CallingConventions.Standard | CallingConventions.HasThis,
							  typeof(int),
							  new Type[] { typeof(int), typeof(int) });
			ilg.Emit (OpCodes.Ldarg_0);
			ilg.Emit (OpCodes.Ldc_I4_0);
			ilg.Emit (OpCodes.Ldc_I4_1);
			ilg.Emit (OpCodes.Call, arrmi2);
			ilg.Emit (OpCodes.Ldloc, x);
			ilg.Emit (OpCodes.Add);
			ilg.Emit (OpCodes.Ret);
			Assert.AreNotSame (arrmi, arrmi2); // fresh MonoArrayMethods each time
	    
			var t = tb.CreateType ();
			Assert.IsNotNull (t);
			var a = new int[,] { { 5, 7 }, { 11, 19 } };
			var mi = t.GetMethod ("TestMeth");
			Assert.IsNotNull (t);
			var o = mi.Invoke (null, new object[] { a });
			Assert.AreEqual (12, (int)o);
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

			var tc = type.CreateType ();

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
			ModuleBuilder module = ab.DefineDynamicModule ("foo.dll", "foo.dll");
			TypeBuilder tb = module.DefineType ("t1", TypeAttributes.Public);

			Assert.AreEqual ("t1[]", module.GetType ("t1[]").FullName);
			Assert.AreEqual ("t1*", module.GetType ("t1*").FullName);
			Assert.AreEqual ("t1&", module.GetType ("t1&").FullName);
			Assert.AreEqual ("t1[]&", module.GetType ("t1[]&").FullName);
		}

		[AttributeUsage(AttributeTargets.All)]
		public class MyAttribute : Attribute {
			public String Contents;
			public MyAttribute (String contents) 
			{
				this.Contents = contents;
			}
		}

		[Test]
		public void GetMethodsBeforeInstantiation ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");

			// Added to make sure fields and methods not mixed up by getters
			FieldBuilder fieldBuilder = module.DefineInitializedData ("GlobalField", new byte[4], FieldAttributes.Public);

			MethodBuilder method = module.DefinePInvokeMethod ("printf", "libc.so",
				MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
				CallingConventions.Standard, typeof (void), new Type [] { typeof (string) }, CallingConvention.Winapi,
				CharSet.Auto);
			method.SetImplementationFlags (MethodImplAttributes.PreserveSig |
										   method.GetMethodImplementationFlags ());

			module.CreateGlobalFunctions ();

			// Make sure method is defined, but field is not
			Assert.AreEqual (1, module.GetMethods (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Length);
		}

		[Test]
		public void GetFieldsBeforeInstantiation ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			FieldBuilder fieldBuilder = module.DefineInitializedData ("GlobalField", new byte[4], FieldAttributes.Public);
			module.CreateGlobalFunctions ();

			var fieldG = module.GetField (fieldBuilder.Name);
			Assert.IsNotNull (fieldG);
			Assert.AreEqual (1, module.GetFields (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Length);
		}

		[Test]
		public void GetCustomAttributesBeforeInstantiation ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			module.CreateGlobalFunctions ();

			ConstructorInfo ctor = typeof(MyAttribute).GetConstructor (new Type [] {typeof(String)});
			ctor.GetHashCode ();
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ctor, new object [] {"hi"});
			module.SetCustomAttribute (cab);

			Assert.AreEqual (1, module.GetCustomAttributes (false).Length);
			Assert.AreEqual (typeof (MyAttribute), ((MyAttribute) module.GetCustomAttributes (false)[0]).GetType ());
			Assert.AreEqual ("hi", ((MyAttribute) module.GetCustomAttributes (false)[0]).Contents);
		}

		[Test]
		public void GetCustomAttributesIgnoresArg ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			module.CreateGlobalFunctions ();

			ConstructorInfo ctor = typeof(MyAttribute).GetConstructor (new Type [] {typeof(String)});
			ctor.GetHashCode ();
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ctor, new object [] {"hi"});
			module.SetCustomAttribute (cab);

			var first = module.GetCustomAttributes (false);
			var second = module.GetCustomAttributes (true);

			Assert.AreEqual (first.Length, second.Length);

			for (int i=0; i < first.Length; i++)
				Assert.AreEqual (first [i].GetType (), second [i].GetType ());

			Assert.AreEqual ("hi", ((MyAttribute) first [0]).Contents);
			Assert.AreEqual ("hi", ((MyAttribute) second [0]).Contents);
		}

		[Test]
		public void GetCustomAttributesThrowsUnbakedAttributeType ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			TypeBuilder tb = module.DefineType ("foo");
			module.CreateGlobalFunctions ();

			ConstructorInfo ctor = typeof(MyAttribute).GetConstructor (new Type [] {typeof(String)});
			ctor.GetHashCode ();
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ctor, new object [] {"hi"});
			module.SetCustomAttribute (cab);

			try {
				module.GetCustomAttributes (tb, false);
			} 
			catch (InvalidOperationException e) {
				// Correct behavior
				return;
			}

			Assert.Fail ("Supposed to throw");
		}

		[Test]
		public void GetExternalTypeBuilderCAttr ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");

			ModuleBuilder module_two = assm.DefineDynamicModule ("ModuleTwo");
			TypeBuilder tb = module_two.DefineType ("foo");

			ConstructorInfo ctor = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ctor, Array.Empty<object> ());

			// Set the custom attribute to have a type builder from another module
			module.SetCustomAttribute (cab);

			module.CreateGlobalFunctions ();

			try {
				module.GetCustomAttributes (false);
			} 
			catch (NotSupportedException e) {
				// Correct behavior
				return;
			}
			Assert.Fail ("Supposed to throw");
		}

		[Test]
		public void GetFieldsNoGlobalType ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			FieldBuilder fieldBuilder = module.DefineInitializedData ("GlobalField", new byte[4], FieldAttributes.Public);

			try {
				module.GetFields (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			} 
			catch (InvalidOperationException e) {
				// Correct behavior
				return;
			}
			Assert.Fail ("Supposed to throw");
		}

		[Test]
		public void GetFieldNoGlobalType ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			FieldBuilder fieldBuilder = module.DefineInitializedData ("GlobalField", new byte[4], FieldAttributes.Public);

			try {
				module.GetField (fieldBuilder.Name);
			} 
			catch (InvalidOperationException e) {
				// Correct behavior
				return;
			}
			Assert.Fail ("Supposed to throw");
		}

		[Test]
		public void GetMethodsNoGlobalType ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			FieldBuilder fieldBuilder = module.DefineInitializedData ("GlobalField", new byte[4], FieldAttributes.Public);

			MethodBuilder method = module.DefinePInvokeMethod ("printf", "libc.so",
															  MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
															  CallingConventions.Standard, typeof (void), new Type [] { typeof (string) }, CallingConvention.Winapi,
															  CharSet.Auto);
			method.SetImplementationFlags (MethodImplAttributes.PreserveSig |
										   method.GetMethodImplementationFlags ());

			try {
				module.GetMethods (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			} 
			catch (InvalidOperationException e) {
				// Correct behavior
				return;
			}
			Assert.Fail ("Supposed to throw");
		}

		[Test]
		public void GetMetadataToken ()
		{
			AssemblyBuilder assm = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName ("Name"), AssemblyBuilderAccess.Run);
			ModuleBuilder module = assm.DefineDynamicModule ("Module");
			module.CreateGlobalFunctions ();
			Assert.AreEqual (0, module.MetadataToken);
		}

		[Test]
		public void SaveMemberRefGtd () {
			// Ensure that the a memberref token is emitted for a
			// field or for a method of a gtd in the same dynamic assembly.
			// Regression test for GitHub #6192

			// class T {
			//   public string F () {
			//      int i = new C<int>().Foo (42).field1;
			//      return i.ToString();
			//   }
			//   public string F2 () {
			//     int i = new C<int>().Bar (42);
			//     return i.ToString ();
			//   }
			// }
			// class C<X> {
			//    public X field1;
			//    public C<X> Foo (X x) {
			//       this.field1 = x;
			//       return this;
		        //    }
			//    public X Bar (X x) {
			//       return this.Foo (x).field1;
			//    }
			//}

			AssemblyName an = genAssemblyName ();
			AssemblyBuilder ab = Thread.GetDomain ().DefineDynamicAssembly (an, AssemblyBuilderAccess.RunAndSave, tempDir);
			ModuleBuilder modulebuilder = ab.DefineDynamicModule (an.Name, an.Name + ".dll");

			var tb = modulebuilder.DefineType ("T", TypeAttributes.Public);
			var il_gen = tb.DefineMethod ("F", MethodAttributes.Public, typeof(string), null).GetILGenerator ();
			var il_gen2 = tb.DefineMethod ("F2", MethodAttributes.Public, typeof(string), null).GetILGenerator ();

			var cbuilder = modulebuilder.DefineType ("C", TypeAttributes.Public);
			var genericParams = cbuilder.DefineGenericParameters ("X");

			var field1builder = cbuilder.DefineField ("field1", genericParams[0], FieldAttributes.Public);

			var cOfX = cbuilder.MakeGenericType(genericParams);

			var fooBuilder = cbuilder.DefineMethod ("Foo",
								MethodAttributes.Public,
								cOfX,
								new Type [] { genericParams[0] });
			var cdefaultCtor = cbuilder.DefineDefaultConstructor (MethodAttributes.Public);

			var fooIL = fooBuilder.GetILGenerator ();

			fooIL.Emit (OpCodes.Ldarg_0);
			fooIL.Emit (OpCodes.Ldarg_1);
			// Emit (Stfld, field1builder) must generate a memberref token, not fielddef.
			fooIL.Emit (OpCodes.Stfld, field1builder);
			fooIL.Emit (OpCodes.Ldarg_0);
			fooIL.Emit (OpCodes.Ret);

			var barBuilder = cbuilder.DefineMethod ("Bar",
								MethodAttributes.Public,
								genericParams [0],
								new Type [] { genericParams [0] });
			var barIL = barBuilder.GetILGenerator ();

			barIL.Emit (OpCodes.Ldarg_0);
			barIL.Emit (OpCodes.Ldarg_1);
			// Emit (Call, fooBuilder) must generate a memberref token, not a methoddef.
			barIL.Emit (OpCodes.Call, fooBuilder);
			barIL.Emit (OpCodes.Ldfld, field1builder);
			barIL.Emit (OpCodes.Ret);

			var cOfInt32 = cbuilder.MakeGenericType (new Type [] { typeof (int) });
			var fooOfInt32 = TypeBuilder.GetMethod (cOfInt32, fooBuilder);
			var cfield1OfInt32 = TypeBuilder.GetField (cOfInt32, field1builder);
			var intToString = typeof(int).GetMethod ("ToString", Type.EmptyTypes);

			var ilocal = il_gen.DeclareLocal (typeof(int));
			il_gen.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (cOfInt32, cdefaultCtor));
			il_gen.Emit (OpCodes.Ldc_I4, 42);
			il_gen.Emit (OpCodes.Call, fooOfInt32);
			il_gen.Emit (OpCodes.Ldfld, cfield1OfInt32);
			il_gen.Emit (OpCodes.Stloc, ilocal);
			il_gen.Emit (OpCodes.Ldloca, ilocal);
			il_gen.Emit (OpCodes.Call, intToString);
			il_gen.Emit (OpCodes.Ret);


			var i2local = il_gen2.DeclareLocal (typeof (int));
			var barOfInt32 = TypeBuilder.GetMethod (cOfInt32, barBuilder);
			il_gen2.Emit (OpCodes.Newobj, TypeBuilder.GetConstructor (cOfInt32, cdefaultCtor));
			il_gen2.Emit (OpCodes.Ldc_I4, 17);
			il_gen2.Emit (OpCodes.Call, barOfInt32);
			il_gen2.Emit (OpCodes.Stloc, i2local);
			il_gen2.Emit (OpCodes.Ldloca, i2local);
			il_gen2.Emit (OpCodes.Call, intToString);
			il_gen2.Emit (OpCodes.Ret);

			cbuilder.CreateType ();
			tb.CreateType ();

			ab.Save (an.Name + ".dll");

			/* Yes the test really needs to roundtrip through SRE.Save().
			 * The regression is in the token fixup code on the saving codepath.
			 */

			var assm = Assembly.LoadFrom (Path.Combine (tempDir, an.Name + ".dll"));
			
			var baked = assm.GetType ("T");

			var x = Activator.CreateInstance (baked);
			var m = baked.GetMethod ("F");

			var s = m.Invoke (x, null);

			Assert.AreEqual ("42", s);

			var m2 = baked.GetMethod ("F2");

			var s2 = m2.Invoke (x, null);

			Assert.AreEqual ("17", s2);
		}

		[Test]
		public void FieldBuilder_DistinctTokens ()
		{
			// Regression test for #33208
			// Fields of distinct classes in the same
			// module should have distinct tokens.

			AssemblyBuilder ab = genAssembly ();
			ModuleBuilder module = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			var tb1 = module.DefineType ("T1", TypeAttributes.Public);

			var tb2 = module.DefineType ("T2", TypeAttributes.Public);

			FieldBuilder fbX1 = tb1.DefineField ("X", typeof (Object), FieldAttributes.Public);

			FieldBuilder fbX2 = tb2.DefineField ("X", typeof (Object), FieldAttributes.Public);

			FieldBuilder fbY1 = tb1.DefineField ("Y", typeof (int), FieldAttributes.Public);

			Assert.AreNotEqual (fbX1.GetToken (), fbX2.GetToken (), "GetToken() T1.X != T2.X");
			Assert.AreNotEqual (fbX1.GetToken (), fbY1.GetToken (), "GetToken() T1.X != T1.Y");
			Assert.AreNotEqual (fbY1.GetToken (), fbX2.GetToken (), "GetToken() T1.Y != T2.X");

			// .NET throws NotSupportedException for
			// FieldBuilder.MetadataToken, Mono doesn't.
			// We'll check that the metadata tokens are
			// distinct, but it's also okay to take these
			// assertions out if we start following .NET
			// behavior.
			Assert.AreNotEqual (fbX1.MetadataToken, fbX2.MetadataToken, "MetadataToken T1.X != T2.X");
			Assert.AreNotEqual (fbX1.MetadataToken, fbY1.MetadataToken, "MetadataToken T1.X != T1.Y");
			Assert.AreNotEqual (fbY1.MetadataToken, fbX2.MetadataToken, "MetadataToken T1.Y != T2.X");

			var t1 = tb1.CreateType ();
			var t2 = tb2.CreateType ();

			FieldInfo fX1 = t1.GetField ("X");
			FieldInfo fX2 = t2.GetField ("X");

			FieldInfo fY1 = t1.GetField ("Y");

			Assert.AreNotEqual (fX1.MetadataToken, fX2.MetadataToken, "T1.X != T2.X");
			Assert.AreNotEqual (fX1.MetadataToken, fY1.MetadataToken, "T1.X != T1.Y");
			Assert.AreNotEqual (fY1.MetadataToken, fX2.MetadataToken, "T1.Y != T2.X");

			Assert.AreEqual (module.ResolveField (fX1.MetadataToken), fX1, "resolve T1.X");
			Assert.AreEqual (module.ResolveField (fX2.MetadataToken), fX2, "resolve T2.X");
			Assert.AreEqual (module.ResolveField (fY1.MetadataToken), fY1, "resolve T1.Y");
		}
	}
}
