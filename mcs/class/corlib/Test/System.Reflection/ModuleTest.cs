//
// ModuleTest - NUnit Test Cases for the Module class
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

using NUnit.Framework;

namespace MonoTests.System.Reflection
{

[TestFixture]
public class ModuleTest : Assertion
{	
	static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.Reflection.ModuleTest");

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

	// Some of these tests overlap with the tests for ModuleBuilder

	[Test]
	public void TestGlobalData () {

		string name = "moduletest-assembly";
		string fileName = name + ".dll";

		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = name;

		AssemblyBuilder ab
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);

		string resfile = Path.Combine (TempFolder, "res");
		using (StreamWriter sw = new StreamWriter (resfile)) {
			sw.WriteLine ("FOO");
		}

		ab.AddResourceFile ("res", "res");

		ModuleBuilder mb = ab.DefineDynamicModule(fileName, fileName);

		mb.DefineInitializedData ("DATA", new byte [100], FieldAttributes.Public);
		mb.DefineInitializedData ("DATA2", new byte [100], FieldAttributes.Public);
		mb.DefineInitializedData ("DATA3", new byte [99], FieldAttributes.Public);
		mb.DefineUninitializedData ("DATA4", 101, FieldAttributes.Public);
		mb.DefineInitializedData ("DATA_PRIVATE", new byte [100], 0);
		mb.CreateGlobalFunctions ();

		ab.Save (fileName);

		Assembly assembly = Assembly.LoadFrom (Path.Combine (TempFolder, fileName));

		Module module = assembly.GetLoadedModules ()[0];

		string[] expectedFieldNames = new string [] {
			"DATA", "DATA2", "DATA3", "DATA4"
		};
		ArrayList fieldNames = new ArrayList ();
		foreach (FieldInfo fi in module.GetFields ()) {
			fieldNames.Add (fi.Name);
		}
		AssertArrayEqualsSorted (expectedFieldNames, fieldNames.ToArray (typeof (string)));

		try {
			module.GetField (null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		try {
			module.GetField (null, 0);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		AssertEquals (module.GetField ("DATA") != null, true);
		AssertEquals (module.GetField ("DATA2") != null, true);
		AssertEquals (module.GetField ("DATA3") != null, true);
		AssertEquals (module.GetField ("DATA4") != null, true);
		AssertEquals (module.GetField ("DATA_PRIVATE"), null);
		AssertEquals (module.GetField ("DATA_PRIVATE", BindingFlags.NonPublic | BindingFlags.Static) != null, true);

		// Check that these methods work correctly on resource modules
		Module m2 = assembly.GetModule ("res");
		AssertEquals (m2 != null, true);
		AssertEquals (m2.GetFields ().Length, 0);
		AssertEquals (m2.GetField ("DATA"), null);
		AssertEquals (m2.GetField ("DATA", BindingFlags.Public), null);
	}

#if NET_2_0

	[Test]
	public void ResolveType () {
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		AssertEquals (t, module.ResolveType (t.MetadataToken));

		/* We currently throw ArgumentException for this one */
		try {
			module.ResolveType (1234);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveType (t.GetMethod ("ResolveType").MetadataToken);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveType (t.MetadataToken + 10000);
			Fail ();
		}
		catch (ArgumentOutOfRangeException) {
		}
	}

	[Test]
	public void ResolveMethod () {
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		AssertEquals (t.GetMethod ("ResolveMethod"), module.ResolveMethod (t.GetMethod ("ResolveMethod").MetadataToken));

		try {
			module.ResolveMethod (1234);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveMethod (t.MetadataToken);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveMethod (t.GetMethod ("ResolveMethod").MetadataToken + 10000);
			Fail ();
		}
		catch (ArgumentOutOfRangeException) {
		}
	}

	public int aField;

	[Test]
	public void ResolveField () {
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		AssertEquals (t.GetField ("aField"), module.ResolveField (t.GetField ("aField").MetadataToken));

		try {
			module.ResolveField (1234);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveField (t.MetadataToken);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveField (t.GetField ("aField").MetadataToken + 10000);
			Fail ();
		}
		catch (ArgumentOutOfRangeException) {
		}
	}

	[Test]
	public void ResolveString () {
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		for (int i = 1; i < 10000; ++i) {
			try {
				module.ResolveString (0x70000000 + i);
			}
			catch (Exception) {
			}
		}

		try {
			module.ResolveString (1234);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveString (t.MetadataToken);
			Fail ();
		}
		catch (ArgumentException) {
		}

		try {
			module.ResolveString (0x70000000 | 10000);
			Fail ();
		}
		catch (ArgumentOutOfRangeException) {
		}
	}


	[Test]
	public void ResolveMember () {
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		AssertEquals (t, module.ResolveMember (t.MetadataToken));
		AssertEquals (t.GetField ("aField"), module.ResolveMember (t.GetField ("aField").MetadataToken));
		AssertEquals (t.GetMethod ("ResolveMember"), module.ResolveMember (t.GetMethod ("ResolveMember").MetadataToken));

		try {
			module.ResolveMember (module.MetadataToken);
		}
		catch (ArgumentException) {
		}
	}
#endif

	[Test]
	public void FindTypes () {
		Module m = typeof (ModuleTest).Module;

		Type[] t;

		t = m.FindTypes (Module.FilterTypeName, "FindTypesTest*");
		AssertEquals (2, t.Length);
		AssertEquals ("FindTypesTestFirstClass", t [0].Name);
		AssertEquals ("FindTypesTestSecondClass", t [1].Name);
		t = m.FindTypes (Module.FilterTypeNameIgnoreCase, "findtypestest*");
		AssertEquals (2, t.Length);
		AssertEquals ("FindTypesTestFirstClass", t [0].Name);
		AssertEquals ("FindTypesTestSecondClass", t [1].Name);
	}

	class FindTypesTestFirstClass { 
	}

	class FindTypesTestSecondClass {
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

