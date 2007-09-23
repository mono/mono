//
// AssemblyBuilderTest.cs - NUnit Test Cases for the AssemblyBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//


using System;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Configuration.Assemblies;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class AssemblyBuilderTest
{	
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class FooAttribute : Attribute
	{
		public FooAttribute (string arg)
		{
		}

		public FooAttribute ()
		{
		}
	}

	static int nameIndex = 0;
	static AppDomain domain;
	static AssemblyBuilder ab;
	static ModuleBuilder mb;
	string tempDir = Path.Combine (Path.GetTempPath (), typeof (AssemblyBuilderTest).FullName);

	[SetUp]
	protected void SetUp ()
	{
		if (Directory.Exists (tempDir))
			Directory.Delete (tempDir, true);

		Directory.CreateDirectory (tempDir);

		for (int i = 1; i < 3; ++i) {
			string resFile = Path.Combine (tempDir, "res" + i + ".txt");
			using (StreamWriter sw = new StreamWriter (resFile)) {
				sw.WriteLine ("FOO");
			}
		}

		domain = Thread.GetDomain ();
		ab = genAssembly ();
		mb = ab.DefineDynamicModule ("def_module");
	}

	[TearDown]
	protected void TearDown ()
	{
		if (Directory.Exists (tempDir))
			Directory.Delete (tempDir, true);
	}

	private AssemblyName genAssemblyName ()
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = typeof (AssemblyBuilderTest).FullName + (nameIndex ++);
		return assemblyName;
	}

	private AssemblyBuilder genAssembly ()
	{
		return domain.DefineDynamicAssembly (genAssemblyName (),
											 AssemblyBuilderAccess.RunAndSave,
											 tempDir);
	}

	private MethodInfo genEntryFunction (AssemblyBuilder assembly)
	{
		ModuleBuilder module = assembly.DefineDynamicModule("module1");
		TypeBuilder tb = module.DefineType ("A");
		MethodBuilder mb = tb.DefineMethod ("A",
			MethodAttributes.Static, typeof (void), new Type [0]);
		mb.GetILGenerator ().Emit (OpCodes.Ret);
		return mb;
	}

#if NET_2_0
	[Test]
	[Category ("NotWorking")]
	public void ManifestModule ()
	{
		AssemblyName aname = new AssemblyName ("ManifestModule1");
		ab = domain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.RunAndSave,
			tempDir);
		Assert.IsNotNull (ab.ManifestModule, "#A1");
		Assert.AreEqual (1, ab.GetModules ().Length, "#A2");
		Assert.AreEqual (typeof (ModuleBuilder), ab.ManifestModule.GetType (), "#A3");

		ModuleBuilder mb1 = (ModuleBuilder) ab.ManifestModule;
		Assert.AreSame (mb1, ab.GetModules () [0], "#B1");
		Assert.IsFalse (mb1.IsResource (), "#B2");
		Assert.AreSame (ab, mb1.Assembly, "#B3");
		Assert.AreSame (mb1, ab.ManifestModule, "#B4");

		ab.Save ("ManifestModule.dll");

		ModuleBuilder mb2 = (ModuleBuilder) ab.ManifestModule;
		Assert.AreSame (mb2, ab.GetModules () [0], "#C1");
		Assert.IsFalse (mb2.IsResource (), "#C2");
		Assert.AreSame (ab, mb2.Assembly, "#C3");
		Assert.AreSame (mb2, ab.ManifestModule, "#C4");
		Assert.AreSame (mb1, mb2, "#C5");
	}
#endif

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestCodeBase ()
	{
		string codebase = ab.CodeBase;
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestLocation ()
	{
		string location = ab.Location;
	}

	[Test]
	public void TestEntryPoint ()
	{
		Assert.AreEqual (null, ab.EntryPoint, "EntryPoint defaults to null");

		MethodInfo mi = genEntryFunction (ab);
		ab.SetEntryPoint (mi);

		Assert.AreEqual (mi, ab.EntryPoint, "EntryPoint works");
	}

	[Test]
	public void TestSetEntryPoint ()
	{
		// Check invalid arguments
		try {
			ab.SetEntryPoint (null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNotNull (ex.ParamName, "#A5");
			Assert.AreEqual ("entryMethod", ex.ParamName, "#A6");
		}

		// Check method from other assembly
		try {
			ab.SetEntryPoint (typeof (AssemblyBuilderTest).GetMethod ("TestSetEntryPoint"));
			Assert.Fail ("#B");
		} catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestIsDefined ()
	{
		CustomAttributeBuilder cab = new CustomAttributeBuilder (typeof (FooAttribute).GetConstructor (new Type [1] {typeof (string)}), new object [1] { "A" });
		ab.SetCustomAttribute (cab);

		Assert.IsTrue (ab.IsDefined (typeof (FooAttribute), false),
			"IsDefined(FooAttribute) works");
		Assert.IsFalse (ab.IsDefined (typeof (AssemblyVersionAttribute), false),
			"!IsDefined(AssemblyVersionAttribute) works");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceNames ()
	{
		ab.GetManifestResourceNames ();
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceInfo ()
	{
		ab.GetManifestResourceInfo ("foo");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceStream1 ()
	{
		ab.GetManifestResourceStream ("foo");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceStream2 ()
	{
		ab.GetManifestResourceStream (typeof (int), "foo");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFiles1 ()
	{
		ab.GetFiles ();
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFiles2 ()
	{
		ab.GetFiles (true);
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFile ()
	{
		ab.GetFile ("foo");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetExportedTypes ()
	{
		ab.GetExportedTypes ();
	}

	[Test]
	public void TestGetDynamicModule_Name_Null ()
	{
		try {
			ab.GetDynamicModule (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestGetDynamicModule_Name_Empty ()
	{
		try {
			ab.GetDynamicModule (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestGetDynamicModule3 ()
	{
		Assert.IsNull (ab.GetDynamicModule ("FOO2"));
		ModuleBuilder mb = ab.DefineDynamicModule ("FOO");
		Assert.AreEqual (mb, ab.GetDynamicModule ("FOO"));
		Assert.IsNull (ab.GetDynamicModule ("FOO4"));
	}

#if NET_1_1
	[Test]
	public void TestImageRuntimeVersion ()
	{
		string version = ab.ImageRuntimeVersion;
		Assert.IsTrue (version.Length > 0);
	}
#endif

	[Test]
	public void TestAddResourceFile_Name_Null ()
	{
		try {
			ab.AddResourceFile (null, "foo.txt");
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestAddResourceFile_Filename_Null ()
	{
		try {
			ab.AddResourceFile ("foo", null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("fileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestAddResourceFile_Name_Empty ()
	{
		try {
			ab.AddResourceFile (string.Empty, "foo.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	public void TestAddResourceFile_Filename_Empty ()
	{
		try {
			ab.AddResourceFile ("foo", string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty file name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	[ExpectedException (typeof (FileNotFoundException))]
	public void TestAddResourceFile_FileName_DoesNotExist ()
	{
		ab.AddResourceFile ("foo", "not-existent.txt");
	}

	[Test]
	public void TestAddResourceFile_FileName_Duplicate ()
	{
		ab.AddResourceFile ("foo", "res1.txt");
		try {
			ab.AddResourceFile ("foo2", "res1.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Duplicate file names
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestAddResourceFile_Name_Duplicate ()
	{
		ab.AddResourceFile ("foo", "res1.txt");
		try {
			ab.AddResourceFile ("foo", "res2.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Duplicate resource name within an assembly
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestAddResourceFile_Filename_IncludesPath ()
	{
		try {
			ab.AddResourceFile ("foo", "/tmp/res1.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The filename must not include a path specification
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("fileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestAddResourceFile ()
	{
		ab.AddResourceFile ("foo", "res2.txt", ResourceAttributes.Public);
		ab.Save ("TestAddResourceFile.dll");

		// TODO: Test reading back
	}

	[Test]
	public void TestDefineResource ()
	{
		ab.DefineResource ("foo", "FOO", "foo.txt", ResourceAttributes.Public);
		ab.DefineResource ("foo2", "FOO", "foo2.txt");
		ab.Save ("TestDefineResource.dll");
	}

	[Test]
	public void TestDefineDynamicModule_Name_Null ()
	{
		try {
			ab.DefineDynamicModule (null, "foo.txt");
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineDynamicModule_FileName_Null ()
	{
		try {
			ab.DefineDynamicModule ("foo", null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("fileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineDynamicModule_Name_Empty ()
	{
		try {
			ab.DefineDynamicModule (string.Empty, "foo.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("name", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineDynamicModule_Filename_Empty ()
	{
		try {
			ab.DefineDynamicModule ("foo", string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Empty file name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("fileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineDynamicModule_FileName_Duplicate ()
	{
		ab.DefineDynamicModule ("foo", "res1.txt");
		try {
			ab.DefineDynamicModule ("foo2", "res1.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Duplicate file names
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestDefineDynamicModule_Name_Duplicate ()
	{
		ab.DefineDynamicModule ("foo", "res1.txt");
		try {
			ab.DefineDynamicModule ("foo", "res2.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Duplicate dynamic module name within an assembly
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	public void TestDefineDynamicModule_Filename_IncludesPath ()
	{
		try {
			ab.DefineDynamicModule ("foo", "/tmp/res1.txt");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The filename must not include a path specification
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("fileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineDynamicModule5_FileName_NoExtension ()
	{
		try {
			ab.DefineDynamicModule ("foo", "bar");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Module file name 'bar' must have file extension
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("bar") != -1, "#5");
			Assert.IsNull (ex.ParamName, "#6");
		}
	}

	[Test]
	[Category ("NotWorking")]
	public void TestDefineDynamicModule_Name_MaxLength () {
		string name = string.Empty;
		for (int i = 0; i < 259; ++i)
			name = name + "A";
		ab.DefineDynamicModule (name);

		name = name + "A";
		try {
			ab.DefineDynamicModule (name);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Value does not fall within expected range
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestDefineDynamicModule_Assembly_Saved ()
	{
		// Called when assembly was already saved
		ab.Save ("TestDefineDynamicModule7.dll");
		ab.DefineDynamicModule ("foo", "foo.dll");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestDefineDynamicModule_Access_Run ()
	{
		// Called on an assembly defined with the Run attribute
		AssemblyBuilder ab = 
			domain.DefineDynamicAssembly (genAssemblyName (),
										  AssemblyBuilderAccess.Run,
										  tempDir);
		ab.DefineDynamicModule ("foo", "foo.dll");
	}

	[Test]
	public void TestDefineDynamicModule ()
	{
		ab.DefineDynamicModule ("foo", "foo.dll");
		ab.DefineDynamicModule ("foo2", true);
		ab.DefineDynamicModule ("foo3", "foo3.dll");
		ab.DefineDynamicModule ("foo4", "foo4.dll", true);
	}

	[Test]
	public void TestDefineUnmanagedResource_Resource_Null ()
	{
		try {
			ab.DefineUnmanagedResource ((byte []) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("resource", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineUnmanagedResource_ResourceFileName_Null ()
	{
		try {
			ab.DefineUnmanagedResource ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("resourceFileName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestDefineUnmanagedResource_ResourceFileName_Empty ()
	{
		try {
			ab.DefineUnmanagedResource (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The path is not of a legal form
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
	}

	[Test]
	[ExpectedException (typeof (FileNotFoundException))]
	public void TestDefineUnmanagedResource_ResourceFile_DoesNotExist ()
	{
		ab.DefineUnmanagedResource ("not-exists.txt");
	}

	[Test]
	public void TestSetCustomAttribute1_CustomBuilder_Null ()
	{
		try {
			ab.SetCustomAttribute (null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("customBuilder", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestSetCustomAttribute2_ConstructorInfo_Null ()
	{
		try {
			ab.SetCustomAttribute (null, new byte [0]);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("con", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestSetCustomAttribute2_BinaryAttribute_Null ()
	{
		try {
			ab.SetCustomAttribute (typeof (AssemblyCompanyAttribute).GetConstructor (
				new Type [] { typeof (String) }), null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("binaryAttribute", ex.ParamName, "#6");
		}
	}

	[Test]
	public void TestSetCustomAttribute ()
	{
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "1.2.3.4"}));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "bar"}));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).
			GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }),
			new object [] { AssemblyHashAlgorithm.MD5 }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).
			GetConstructor (new Type [] { typeof (uint) }), new object [] { (uint)0x0100 }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).
			GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));
		ab.SetCustomAttribute (typeof (FooAttribute).GetConstructor (new Type [] {}), new byte [0]);
		ab.Save ("TestSetCustomAttribute.dll");

		/* We should read back the assembly and check the attributes ... */
	}

	// strongname generated using "sn -k unit.snk"
	static byte[] strongName = { 
		0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 
		0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x7F, 0x7C, 0xEA, 0x4A, 
		0x28, 0x33, 0xD8, 0x3C, 0x86, 0x90, 0x86, 0x91, 0x11, 0xBB, 0x30, 0x0D, 
		0x3D, 0x69, 0x04, 0x4C, 0x48, 0xF5, 0x4F, 0xE7, 0x64, 0xA5, 0x82, 0x72, 
		0x5A, 0x92, 0xC4, 0x3D, 0xC5, 0x90, 0x93, 0x41, 0xC9, 0x1D, 0x34, 0x16, 
		0x72, 0x2B, 0x85, 0xC1, 0xF3, 0x99, 0x62, 0x07, 0x32, 0x98, 0xB7, 0xE4, 
		0xFA, 0x75, 0x81, 0x8D, 0x08, 0xB9, 0xFD, 0xDB, 0x00, 0x25, 0x30, 0xC4, 
		0x89, 0x13, 0xB6, 0x43, 0xE8, 0xCC, 0xBE, 0x03, 0x2E, 0x1A, 0x6A, 0x4D, 
		0x36, 0xB1, 0xEB, 0x49, 0x26, 0x6C, 0xAB, 0xC4, 0x29, 0xD7, 0x8F, 0x25, 
		0x11, 0xA4, 0x7C, 0x81, 0x61, 0x97, 0xCB, 0x44, 0x2D, 0x80, 0x49, 0x93, 
		0x48, 0xA7, 0xC9, 0xAB, 0xDB, 0xCF, 0xA3, 0x34, 0xCB, 0x6B, 0x86, 0xE0, 
		0x4D, 0x27, 0xFC, 0xA7, 0x4F, 0x36, 0xCA, 0x13, 0x42, 0xD3, 0x83, 0xC4, 
		0x06, 0x6E, 0x12, 0xE0, 0xA1, 0x3D, 0x9F, 0xA9, 0xEC, 0xD1, 0xC6, 0x08, 
		0x1B, 0x3D, 0xF5, 0xDB, 0x4C, 0xD4, 0xF0, 0x2C, 0xAA, 0xFC, 0xBA, 0x18, 
		0x6F, 0x48, 0x7E, 0xB9, 0x47, 0x68, 0x2E, 0xF6, 0x1E, 0x67, 0x1C, 0x7E, 
		0x0A, 0xCE, 0x10, 0x07, 0xC0, 0x0C, 0xAD, 0x5E, 0xC1, 0x53, 0x70, 0xD5, 
		0xE7, 0x25, 0xCA, 0x37, 0x5E, 0x49, 0x59, 0xD0, 0x67, 0x2A, 0xBE, 0x92, 
		0x36, 0x86, 0x8A, 0xBF, 0x3E, 0x17, 0x04, 0xFB, 0x1F, 0x46, 0xC8, 0x10, 
		0x5C, 0x93, 0x02, 0x43, 0x14, 0x96, 0x6A, 0xD9, 0x87, 0x17, 0x62, 0x7D, 
		0x3A, 0x45, 0xBE, 0x35, 0xDE, 0x75, 0x0B, 0x2A, 0xCE, 0x7D, 0xF3, 0x19, 
		0x85, 0x4B, 0x0D, 0x6F, 0x8D, 0x15, 0xA3, 0x60, 0x61, 0x28, 0x55, 0x46, 
		0xCE, 0x78, 0x31, 0x04, 0x18, 0x3C, 0x56, 0x4A, 0x3F, 0xA4, 0xC9, 0xB1, 
		0x41, 0xED, 0x22, 0x80, 0xA1, 0xB3, 0xE2, 0xC7, 0x1B, 0x62, 0x85, 0xE4, 
		0x81, 0x39, 0xCB, 0x1F, 0x95, 0xCC, 0x61, 0x61, 0xDF, 0xDE, 0xF3, 0x05, 
		0x68, 0xB9, 0x7D, 0x4F, 0xFF, 0xF3, 0xC0, 0x0A, 0x25, 0x62, 0xD9, 0x8A, 
		0x8A, 0x9E, 0x99, 0x0B, 0xFB, 0x85, 0x27, 0x8D, 0xF6, 0xD4, 0xE1, 0xB9, 
		0xDE, 0xB4, 0x16, 0xBD, 0xDF, 0x6A, 0x25, 0x9C, 0xAC, 0xCD, 0x91, 0xF7, 
		0xCB, 0xC1, 0x81, 0x22, 0x0D, 0xF4, 0x7E, 0xEC, 0x0C, 0x84, 0x13, 0x5A, 
		0x74, 0x59, 0x3F, 0x3E, 0x61, 0x00, 0xD6, 0xB5, 0x4A, 0xA1, 0x04, 0xB5, 
		0xA7, 0x1C, 0x29, 0xD0, 0xE1, 0x11, 0x19, 0xD7, 0x80, 0x5C, 0xEE, 0x08, 
		0x15, 0xEB, 0xC9, 0xA8, 0x98, 0xF5, 0xA0, 0xF0, 0x92, 0x2A, 0xB0, 0xD3, 
		0xC7, 0x8C, 0x8D, 0xBB, 0x88, 0x96, 0x4F, 0x18, 0xF0, 0x8A, 0xF9, 0x31, 
		0x9E, 0x44, 0x94, 0x75, 0x6F, 0x78, 0x04, 0x10, 0xEC, 0xF3, 0xB0, 0xCE, 
		0xA0, 0xBE, 0x7B, 0x25, 0xE1, 0xF7, 0x8A, 0xA8, 0xD4, 0x63, 0xC2, 0x65, 
		0x47, 0xCC, 0x5C, 0xED, 0x7D, 0x8B, 0x07, 0x4D, 0x76, 0x29, 0x53, 0xAC, 
		0x27, 0x8F, 0x5D, 0x78, 0x56, 0xFA, 0x99, 0x45, 0xA2, 0xCC, 0x65, 0xC4, 
		0x54, 0x13, 0x9F, 0x38, 0x41, 0x7A, 0x61, 0x0E, 0x0D, 0x34, 0xBC, 0x11, 
		0xAF, 0xE2, 0xF1, 0x8B, 0xFA, 0x2B, 0x54, 0x6C, 0xA3, 0x6C, 0x09, 0x1F, 
		0x0B, 0x43, 0x9B, 0x07, 0x95, 0x83, 0x3F, 0x97, 0x99, 0x89, 0xF5, 0x51, 
		0x41, 0xF6, 0x8E, 0x5D, 0xEF, 0x6D, 0x24, 0x71, 0x41, 0x7A, 0xAF, 0xBE, 
		0x81, 0x71, 0xAB, 0x76, 0x2F, 0x1A, 0x5A, 0xBA, 0xF3, 0xA6, 0x65, 0x7A, 
		0x80, 0x50, 0xCE, 0x23, 0xC3, 0xC7, 0x53, 0xB0, 0x7C, 0x97, 0x77, 0x27, 
		0x70, 0x98, 0xAE, 0xB5, 0x24, 0x66, 0xE1, 0x60, 0x39, 0x41, 0xDA, 0x54, 
		0x01, 0x64, 0xFB, 0x10, 0x33, 0xCE, 0x8B, 0xBE, 0x27, 0xD4, 0x21, 0x57, 
		0xCC, 0x0F, 0x1A, 0xC1, 0x3D, 0xF3, 0xCC, 0x39, 0xF0, 0x2F, 0xAE, 0xF1, 
		0xC0, 0xCD, 0x3B, 0x23, 0x87, 0x49, 0x7E, 0x40, 0x32, 0x6A, 0xD3, 0x96, 
		0x4A, 0xE5, 0x5E, 0x6E, 0x26, 0xFD, 0x8A, 0xCF, 0x7E, 0xFC, 0x37, 0xDE, 
		0x39, 0x0C, 0x53, 0x81, 0x75, 0x08, 0xAF, 0x6B, 0x39, 0x6C, 0xFB, 0xC9, 
		0x79, 0xC0, 0x9B, 0x5F, 0x34, 0x86, 0xB2, 0xDE, 0xC4, 0x19, 0x84, 0x5F, 
		0x0E, 0xED, 0x9B, 0xB8, 0xD3, 0x17, 0xDA, 0x78 };

	[Test]
	public void StrongName_MissingKeyFile_NoDelay ()
	{
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyKeyFileAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { "missing.snk" }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).GetConstructor (new Type [] { typeof (bool) }), new object [] { false }));
		ab.Save ("StrongName_MissingKeyFile_NoDelay.dll");

		string filename = Path.Combine (tempDir, "StrongName_MissingKeyFile_NoDelay.dll");
		AssemblyName check = AssemblyName.GetAssemblyName (filename);
		// no exception is thrown (file not found)
		// because it's not AssemblyBuilder.Save job to do the signing :-/
		Assert.IsNull (check.GetPublicKeyToken (), "Token");
	}

	[Test]
	public void StrongName_KeyFile_Delay ()
	{
		string strongfile = Path.Combine (tempDir, "strongname.snk");
		using (FileStream fs = File.OpenWrite (strongfile)) {
			fs.Write (strongName, 0, strongName.Length);
			fs.Close ();
		}
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyKeyFileAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { strongfile }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));
		ab.Save ("StrongName_KeyFile_Delay.dll");

		string filename = Path.Combine (tempDir, "StrongName_KeyFile_Delay.dll");
		AssemblyName check = AssemblyName.GetAssemblyName (filename);
		// no public key is inserted into the assembly
		// because it's not AssemblyBuilder.Save job to do the signing :-/
		Assert.IsNull (check.GetPublicKeyToken (), "Token");
	}

	[Test]
	public void StrongName_WithoutAttributes ()
	{
		// this demonstrate that AssemblyKeyFileAttribute (or AssemblyKeyNameAttribute)
		// aren't required to sign an assembly.
		AssemblyName an = genAssemblyName ();
		an.KeyPair = new StrongNameKeyPair (strongName);
		AssemblyBuilder ab = domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.RunAndSave, tempDir);
		ab.Save ("StrongName_WithoutAttributes.dll");

		string filename = Path.Combine (tempDir, "StrongName_WithoutAttributes.dll");
		AssemblyName check = AssemblyName.GetAssemblyName (filename);
		Assert.AreEqual ("0E-EA-7C-E6-5F-35-F2-D8", BitConverter.ToString (check.GetPublicKeyToken ()), "Token");
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void SaveUnfinishedTypes ()
	{
		TypeBuilder typeBuilder = mb.DefineType ("TestType",
			TypeAttributes.Class | TypeAttributes.Public |
			TypeAttributes.Sealed | TypeAttributes.AnsiClass |
			TypeAttributes.AutoClass, typeof(object));
		ab.Save ("def_module");
	}

	[Test]
	public void GetModules ()
	{
		Module[] m;

		m = ab.GetModules ();
		Assert.IsTrue (m.Length >= 2);

		// Test with no modules
		AssemblyBuilder ab2 = genAssembly ();
		m = ab2.GetModules ();
	}

	[Test] // bug #78724
	public void GetTypes ()
	{
		TypeBuilder tb = mb.DefineType ("sometype");
		tb.CreateType ();

		Type[] types = ab.GetTypes ();
		Assert.AreEqual (1, types.Length);
		Assert.AreEqual ("sometype", types[0].Name);
	}

	[Test]
	[Category ("NotWorking")]
	public void AssemblyName_Culture ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";
		assemblyName.Version = new Version ("1.0.0.0");
		assemblyName.CultureInfo = new CultureInfo ("en-US");

		const string fullName = "AssemblyNameTest, Version=1.0.0.0, Culture=en-US, PublicKeyToken=null";
		const string abName = "AssemblyNameTest, Version=1.0.0.0, Culture=en-US";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);
	}

	[Test]
	public void AssemblyName_PublicKey ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest_PublicKey";
		assemblyName.Version = new Version ("1.2.3.4");
		assemblyName.KeyPair = new StrongNameKeyPair (strongName);

		Assert.AreEqual ("AssemblyNameTest_PublicKey, Version=1.2.3.4", assemblyName.FullName, "#1");

		const string fullName = "AssemblyNameTest_PublicKey, Version=1.2.3.4, Culture=neutral, PublicKeyToken=0eea7ce65f35f2d8";

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			assemblyName, AssemblyBuilderAccess.Save, tempDir);

		AssemblyName abName = ab.GetName ();
		Assert.IsNotNull (abName.GetPublicKeyToken (), "#2");
		Assert.IsTrue (abName.GetPublicKeyToken ().Length > 0, "#3");
		Assert.IsNotNull (abName.GetPublicKey () != null, "#4");
		Assert.IsTrue (abName.GetPublicKey ().Length > 0, "#5");

		ab.Save ("AssemblyNameTest_PublicKey.dll");
		AssemblyName bakedName = AssemblyName.GetAssemblyName (Path.Combine(
			tempDir, "AssemblyNameTest_PublicKey.dll"));

		Assert.IsNotNull (bakedName.GetPublicKeyToken (), "#6");
		Assert.IsNotNull (bakedName.GetPublicKey (), "#7");
		Assert.AreEqual (fullName, bakedName.FullName, "#8");
	}

	[Test]
	[Category ("NotWorking")] 
	public void AssemblyName_MoreCultureInfo ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest_MoreCultureInfo";
		assemblyName.Version = new Version ("1.2.3.4");
		assemblyName.KeyPair = new StrongNameKeyPair (strongName);

		Assert.IsNull (assemblyName.CultureInfo, "#1");
		Assert.AreEqual ("AssemblyNameTest_MoreCultureInfo, Version=1.2.3.4", assemblyName.FullName, "#2");

		const string fullName = "AssemblyNameTest_MoreCultureInfo, Version=1.2.3.4, Culture=neutral, PublicKeyToken=0eea7ce65f35f2d8";

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			assemblyName, AssemblyBuilderAccess.Save, tempDir);

		AssemblyName abName = ab.GetName ();
		Assert.IsNotNull (abName.CultureInfo != null, "#3");
#if NET_2_0
		Assert.IsTrue (abName.CultureInfo != CultureInfo.InvariantCulture, "#4");
		Assert.AreEqual (CultureInfo.InvariantCulture.LCID, abName.CultureInfo.LCID, "#5");
		Assert.AreEqual (fullName, abName.FullName, "#6");
#else
		Assert.AreEqual (CultureInfo.InvariantCulture, abName.CultureInfo, "#7");
		Assert.AreEqual ("AssemblyNameTest_MoreCultureInfo, Version=1.2.3.4, PublicKeyToken=0eea7ce65f35f2d8", abName.FullName, "#8");
#endif

		ab.Save ("AssemblyNameTest_MoreCultureInfo.dll");

		AssemblyName bakedName = AssemblyName.GetAssemblyName (Path.Combine(
			tempDir, "AssemblyNameTest_MoreCultureInfo.dll"));

		Assert.IsNotNull (bakedName.CultureInfo, "#9");

#if NET_2_0
		Assert.IsTrue (abName.CultureInfo != CultureInfo.InvariantCulture, "#10");
		Assert.AreEqual (CultureInfo.InvariantCulture.LCID, abName.CultureInfo.LCID, "#11");
#else
		Assert.AreEqual (CultureInfo.InvariantCulture, bakedName.CultureInfo, "#12");
#endif
		Assert.AreEqual (fullName, bakedName.FullName, "#13");
	}

	[Test]
	[Category ("NotWorking")]
	public void AssemblyName_NoVersion ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";

		const string fullName = "AssemblyNameTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
		const string abName = "AssemblyNameTest, Version=0.0.0.0";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);
	}

	[Test]
	[Category ("NotWorking")]
	public void AssemblyName_Version ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";
		assemblyName.Version = new Version (1, 2, 3, 4);

		const string fullName = "AssemblyNameTest, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null";
		const string abName = "AssemblyNameTest, Version=1.2.3.4";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);
	}

	[Test]
	[Category ("NotDotNet")]
	public void GetType_IgnoreCase ()
	{
		TypeBuilder tb = mb.DefineType ("Foo.Test2", TypeAttributes.Public, typeof (object));
		// the previous line throws a TypeLoadException under MS 1.1 SP1

		Type t;

		t = ab.GetType ("foo.Test2", true, true);
		Assert.AreEqual ("Test2", t.Name);

		t = ab.GetType ("foo.test2", true, true);
		Assert.AreEqual ("Test2", t.Name);

		t = ab.GetType ("Foo.test2", true, true);
		Assert.AreEqual ("Test2", t.Name);
	}

	private static void AssertAssemblyName (string tempDir, AssemblyName assemblyName, string abName, string fullName)
	{
		AppDomain currentDomain = AppDomain.CurrentDomain;
		AppDomain newDomain = null;

		try {
			AssemblyBuilder ab = currentDomain.DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Save, tempDir);
			ab.Save (assemblyName.Name + ".dll");

#if NET_2_0
			// on .NET 2.0, the full name of the AssemblyBuilder matches the 
			// fully qualified assembly name
			Assert.AreEqual (fullName, ab.FullName);
#else
			Assert.AreEqual (abName, ab.FullName);
#endif

			// load assembly in separate domain, so we can clean-up after the 
			// test
			newDomain = AppDomain.CreateDomain ("test2", currentDomain.Evidence,
				currentDomain.SetupInformation);

			Helper helper = new Helper (Path.Combine (tempDir, assemblyName.Name + ".dll"),
				fullName);
			newDomain.DoCallBack (new CrossAppDomainDelegate (helper.Test));
		} finally {
			if (newDomain != null) {
				AppDomain.Unload (newDomain);
			}
		}
	}

	[Serializable ()]
	private class Helper
	{
		private readonly string _assemblyPath;
		private readonly string _assemblyName;

		public Helper (string assemblyPath, string assemblyName)
		{
			_assemblyPath = assemblyPath;
			_assemblyName = assemblyName;
		}
		public void Test ()
		{
			AssemblyName assemblyName = AssemblyName.GetAssemblyName (_assemblyPath);
			Assert.AreEqual (_assemblyName, assemblyName.ToString ());
		}
	}
}
}
