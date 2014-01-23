//
// ModuleTest - NUnit Test Cases for the Module class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using System.Reflection;
#if !MONOTOUCH
using System.Reflection.Emit;
#endif
using System.Runtime.Serialization;
using System.IO;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
[TestFixture]
public class ModuleTest
{
	static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.Reflection.ModuleTest");

	[SetUp]
	public void SetUp ()
	{
		while (Directory.Exists (TempFolder))
			TempFolder = Path.Combine (TempFolder, "2");
		Directory.CreateDirectory (TempFolder);
	}

	[TearDown]
	public void TearDown ()
	{
		try {
			// This throws an exception under MS.NET, since the directory contains loaded
			// assemblies.
			Directory.Delete (TempFolder, true);
		} catch (Exception) {
		}
	}

	[Test]
	public void IsDefined_AttributeType_Null ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		try {
			module.IsDefined ((Type) null, false);
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
	public void GetField_Name_Null ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		try {
			module.GetField (null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNotNull (ex.ParamName, "#A5");
			Assert.AreEqual ("name", ex.ParamName, "#A6");
		}

		try {
			module.GetField (null, 0);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNotNull (ex.ParamName, "#B5");
			Assert.AreEqual ("name", ex.ParamName, "#B6");
		}
	}

	// Some of these tests overlap with the tests for ModuleBuilder
#if !MONOTOUCH
	[Test]
	[Category("NotDotNet")] // path length can cause suprious failures
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

		Assert.IsNotNull (module.GetField ("DATA"), "#A1");
		Assert.IsNotNull (module.GetField ("DATA2"), "#A2");
		Assert.IsNotNull (module.GetField ("DATA3"), "#A3");
		Assert.IsNotNull (module.GetField ("DATA4"), "#A4");
		Assert.IsNull (module.GetField ("DATA_PRIVATE"), "#A5");
		Assert.IsNotNull (module.GetField ("DATA_PRIVATE", BindingFlags.NonPublic | BindingFlags.Static), "#A6");

		// Check that these methods work correctly on resource modules
		Module m2 = assembly.GetModule ("res");
		Assert.IsNotNull (m2, "#B1");
		Assert.AreEqual (0, m2.GetFields ().Length, "#B2");
		Assert.IsNull (m2.GetField ("DATA"), "#B3");
		Assert.IsNull (m2.GetField ("DATA", BindingFlags.Public), "#B4");
	}
#endif

#if NET_2_0
	[Test]
	public void ResolveType ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		Assert.AreEqual (t, module.ResolveType (t.MetadataToken), "#1");

		/* We currently throw ArgumentException for this one */
		try {
			module.ResolveType (1234);
			Assert.Fail ("#2");
		} catch (ArgumentException) {
		}

		try {
			module.ResolveType (t.GetMethod ("ResolveType").MetadataToken);
			Assert.Fail ("#3");
		} catch (ArgumentException) {
		}

		try {
			module.ResolveType (t.MetadataToken + 10000);
			Assert.Fail ("#4");
		} catch (ArgumentOutOfRangeException) {
		}
	}

	[Test]
	public void ResolveMethod ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		Assert.AreEqual (t.GetMethod ("ResolveMethod"), module.ResolveMethod (t.GetMethod ("ResolveMethod").MetadataToken));

		try {
			module.ResolveMethod (1234);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveMethod (t.MetadataToken);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveMethod (t.GetMethod ("ResolveMethod").MetadataToken + 10000);
			Assert.Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
	}

	public int aField;

	[Test]
	public void ResolveField ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		Assert.AreEqual (t.GetField ("aField"), module.ResolveField (t.GetField ("aField").MetadataToken));

		try {
			module.ResolveField (1234);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveField (t.MetadataToken);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveField (t.GetField ("aField").MetadataToken + 10000);
			Assert.Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
	}

	[Ignore ("it breaks nunit-console.exe execution under .NET 2.0")]
	[Test]
	public void ResolveString ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		for (int i = 1; i < 10000; ++i) {
			try {
				module.ResolveString (0x70000000 + i);
			} catch (Exception) {
			}
		}

		try {
			module.ResolveString (1234);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveString (t.MetadataToken);
			Assert.Fail ();
		} catch (ArgumentException) {
		}

		try {
			module.ResolveString (0x70000000 | 10000);
			Assert.Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
	}


	[Test]
	public void ResolveMember ()
	{
		Type t = typeof (ModuleTest);
		Module module = t.Module;

		Assert.AreEqual (t, module.ResolveMember (t.MetadataToken), "#1");
		Assert.AreEqual (t.GetField ("aField"), module.ResolveMember (t.GetField ("aField").MetadataToken), "#2");
		Assert.AreEqual (t.GetMethod ("ResolveMember"), module.ResolveMember (t.GetMethod ("ResolveMember").MetadataToken), "#3");

		try {
			module.ResolveMember (module.MetadataToken);
			Assert.Fail ("#4");
		} catch (ArgumentException) {
		}
	}

	public class Foo<T>  {
		public void Bar(T t) {}
	}

	[Test]
	public void ResolveMethodOfGenericClass ()
	{
		Type type = typeof (Foo<>);
		Module mod = type.Module;
		MethodInfo method = type.GetMethod ("Bar");
		MethodBase res = mod.ResolveMethod (method.MetadataToken);
		Assert.AreEqual (method, res, "#1");
	}
#endif

	[Test]
	public void FindTypes ()
	{
		Module m = typeof (ModuleTest).Module;

		Type[] t;

		t = m.FindTypes (Module.FilterTypeName, "FindTypesTest*");
		Assert.AreEqual (2, t.Length, "#A1");
		Assert.AreEqual ("FindTypesTestFirstClass", t [0].Name, "#A2");
		Assert.AreEqual ("FindTypesTestSecondClass", t [1].Name, "#A3");
		t = m.FindTypes (Module.FilterTypeNameIgnoreCase, "findtypestest*");
		Assert.AreEqual (2, t.Length, "#B1");
		Assert.AreEqual ("FindTypesTestFirstClass", t [0].Name, "#B2");
		Assert.AreEqual ("FindTypesTestSecondClass", t [1].Name, "#B3");
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void GetObjectData_Null ()
	{
		Module m = typeof (ModuleTest).Module;
		m.GetObjectData (null, new StreamingContext (StreamingContextStates.All));
	}
#if !MONOTOUCH
	[Test]
	public void GetTypes ()
	{
		AssemblyName newName = new AssemblyName ();
		newName.Name = "ModuleTest";

		AssemblyBuilder ab = Thread.GetDomain().DefineDynamicAssembly (newName, AssemblyBuilderAccess.RunAndSave, TempFolder);

		ModuleBuilder mb = ab.DefineDynamicModule ("myDynamicModule1", "myDynamicModule" + ".dll", true);

		TypeBuilder tb = mb.DefineType ("Foo", TypeAttributes.Public);
		tb.CreateType ();

		ab.Save ("test_assembly.dll");

		Assembly ass = Assembly.LoadFrom (Path.Combine (TempFolder, "test_assembly.dll"));
		ArrayList types = new ArrayList ();
		// The order of the modules is different between MS.NET and mono
		foreach (Module m in ass.GetModules ()) {
			Type[] t = m.GetTypes ();
			types.AddRange (t);
		}
		Assert.AreEqual (1, types.Count);
		Assert.AreEqual ("Foo", ((Type)(types [0])).Name);
	}
#endif
	class FindTypesTestFirstClass {
	}

	class FindTypesTestSecondClass {
	}

	private static void AssertArrayEqualsSorted (Array o1, Array o2) {
		Array s1 = (Array)o1.Clone ();
		Array s2 = (Array)o2.Clone ();

		Array.Sort (s1);
		Array.Sort (s2);

		Assert.AreEqual (s1.Length, s2.Length);
		for (int i = 0; i < s1.Length; ++i)
			Assert.AreEqual (s1.GetValue (i), s2.GetValue (i));
	}
}
}

