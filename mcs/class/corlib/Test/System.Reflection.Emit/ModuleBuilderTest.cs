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
public class ModuleBuilderTest : Assertion
{	
	static string TempFolder = Path.Combine (Path.GetTempPath (), "MT.S.R.E.MBT");

	[SetUp]
	public void SetUp () {
		while (Directory.Exists (TempFolder))
			TempFolder = Path.Combine (TempFolder, "2");
		Directory.CreateDirectory (TempFolder);
	}		

	[TearDown]
	public void TearDown () {
		try {
			// This throws an exception under MS.NET, since the directory contains loaded
			// assemblies.
			Directory.Delete (TempFolder, true);
		}
		catch (Exception) {
		}
	}

	[Test]
	public void TestIsTransient () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "foo";

		AssemblyBuilder ab
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);		
		ModuleBuilder mb1 = ab.DefineDynamicModule ("foo.dll");
		AssertEquals (true, mb1.IsTransient ());
		ModuleBuilder mb2 = ab.DefineDynamicModule ("foo2.dll", "foo2.dll");
		AssertEquals (false, mb2.IsTransient ());
	}

	// Some of these tests overlap with the tests for Module

	[Test]
	public void TestGlobalData () {

		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "foo";

		AssemblyBuilder ab
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);

		string resfile = Path.Combine (TempFolder, "res");
		using (StreamWriter sw = new StreamWriter (resfile)) {
			sw.WriteLine ("FOO");
		}

		ModuleBuilder mb = ab.DefineDynamicModule("foo.dll", "foo.dll");

		mb.DefineInitializedData ("DATA", new byte [100], FieldAttributes.Public);
		mb.DefineInitializedData ("DATA2", new byte [100], FieldAttributes.Public);
		mb.DefineInitializedData ("DATA3", new byte [99], FieldAttributes.Public);
		mb.DefineUninitializedData ("DATA4", 101, FieldAttributes.Public);
		mb.DefineInitializedData ("DATA_PRIVATE", new byte [100], 0);
		mb.CreateGlobalFunctions ();

		ab.Save ("foo.dll");

		Assembly assembly = Assembly.LoadFrom (Path.Combine (TempFolder, "foo.dll"));

		Module module = assembly.GetLoadedModules ()[0];

		string[] expectedFieldNames = new string [] {
			"DATA", "DATA2", "DATA3", "DATA4"
		};
		ArrayList fieldNames = new ArrayList ();
		foreach (FieldInfo fi in module.GetFields ()) {
			fieldNames.Add (fi.Name);
		}
		AssertArrayEqualsSorted (expectedFieldNames, fieldNames.ToArray (typeof (string)));

		AssertEquals (module.GetField ("DATA") != null, true);
		AssertEquals (module.GetField ("DATA2") != null, true);
		AssertEquals (module.GetField ("DATA3") != null, true);
		AssertEquals (module.GetField ("DATA4") != null, true);
		AssertEquals (module.GetField ("DATA_PRIVATE"), null);
		AssertEquals (module.GetField ("DATA_PRIVATE", BindingFlags.NonPublic | BindingFlags.Static) != null, true);
	}

	[Test]
	public void TestGlobalMethods () {
		AssemblyName an = new AssemblyName();
		an.Name = "TestGlobalMethods";
		AssemblyBuilder builder = 
			AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
		ModuleBuilder module = builder.DefineDynamicModule("MessageModule");
		MethodBuilder method = module.DefinePInvokeMethod("printf", "libc.so",
														  MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public, 
														  CallingConventions.Standard, typeof(void), new Type[]{typeof(string)}, CallingConvention.Winapi, 
														  CharSet.Auto);
		method.SetImplementationFlags (MethodImplAttributes.PreserveSig | 
									   method.GetMethodImplementationFlags());
		module.CreateGlobalFunctions();

		Assert (module.GetMethod ("printf") != null);
	}

	[Test]
	public void DuplicateSymbolDocument () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "ModuleBuilderTest.DuplicateSymbolDocument";

		AssemblyBuilder ab
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);

		ModuleBuilder mb = ab.DefineDynamicModule("foo.dll", "foo.dll", true);

		// Check that it is possible to redefine a symbol document
		ISymbolDocumentWriter doc1 =
			mb.DefineDocument("foo.il", SymDocumentType.Text,
							  SymLanguageType.ILAssembly,SymLanguageVendor.Microsoft);
		ISymbolDocumentWriter doc2 =
			mb.DefineDocument("foo.il", SymDocumentType.Text,
							  SymLanguageType.ILAssembly,SymLanguageVendor.Microsoft);
	}
	
    private static void AssertArrayEqualsSorted (Array o1, Array o2) {
		Array s1 = (Array)o1.Clone ();
		Array s2 = (Array)o2.Clone ();

		Array.Sort (s1);
		Array.Sort (s2);

		AssertEquals (s1.Length, s2.Length);
		for (int i = 0; i < s1.Length; ++i)
			AssertEquals (s1.GetValue (i), s2.GetValue (i));
	}
}
}

