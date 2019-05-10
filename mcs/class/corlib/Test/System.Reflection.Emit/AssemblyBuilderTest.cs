//
// AssemblyBuilderTest.cs - NUnit Test Cases for the AssemblyBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
// Andres G. Aragoneses (andres@7digital.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 7digital Media, Ltd. http://www.7digital.com
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

	private AssemblyBuilder genAssembly (AssemblyBuilderAccess access)
	{
		return domain.DefineDynamicAssembly (genAssemblyName (),
						     access,
						     tempDir);
	}
	private AssemblyBuilder genAssembly ()
	{
		return genAssembly (AssemblyBuilderAccess.RunAndSave);
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

#if !DISABLE_SECURITY
	[Test]
	[Category ("MobileNotWorking")]
	public void DefaultCtor ()
	{
		Assert.IsNotNull (ab.Evidence, "#1");
	}
#endif

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
		Assert.AreSame (mb1, ab.ManifestModule, "#B3");

		ab.Save ("ManifestModule.dll");

		ModuleBuilder mb2 = (ModuleBuilder) ab.ManifestModule;
		Assert.AreSame (mb2, ab.GetModules () [0], "#C1");
		Assert.IsFalse (mb2.IsResource (), "#C2");
		Assert.AreSame (mb2, ab.ManifestModule, "#C3");
		Assert.AreSame (mb1, mb2, "#C4");
	}

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

	[Test]
	public void TestImageRuntimeVersion ()
	{
		string version = ab.ImageRuntimeVersion;
		Assert.IsTrue (version.Length > 0);
	}

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
	[Category ("MobileNotWorking")] // DefineResource doesn't allow path in its fileName parameter and the test attempts to write to / in effect
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
		ab.DefineDynamicModule ("foo3", "foo3.dll");
	}

	[Category ("MobileNotWorking")] //XA doesn't ship SymbolWriter. https://bugzilla.xamarin.com/show_bug.cgi?id=53038
	public void TestDefineDynamicModuleWithSymbolWriter ()
	{
		ab.DefineDynamicModule ("foo2", true);
		ab.DefineDynamicModule ("foo4", "foo4.dll", true);
	}

	[Test] // DefineUnmanagedResource (byte [])
	[Category ("NotWorking")]
	public void TestDefineUnmanagedResource1_ResourceAlreadyDefined ()
	{
		string version_res = Path.Combine (tempDir, "version.res");
		using (FileStream fs = File.OpenWrite (version_res)) {
			fs.WriteByte (0x0a);
		}

		ab.DefineUnmanagedResource (new byte [0]);

		try {
			ab.DefineUnmanagedResource (new byte [0]);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			ab.DefineUnmanagedResource (version_res);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}

		try {
			ab.DefineVersionInfoResource ();
			Assert.Fail ("#C1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNull (ex.ParamName, "#C5");
		}

		try {
			ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");
			Assert.Fail ("#D1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.IsNull (ex.ParamName, "#D5");
		}
	}

	[Test] // DefineUnmanagedResource (byte [])
	public void TestDefineUnmanagedResource1_Resource_Null ()
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

	[Test] // DefineUnmanagedResource (String)
	public void TestDefineUnmanagedResource2_ResourceAlreadyDefined ()
	{
		string version_res = Path.Combine (tempDir, "version.res");
		using (FileStream fs = File.OpenWrite (version_res)) {
			fs.WriteByte (0x0a);
		}

		ab.DefineUnmanagedResource (version_res);

		try {
			ab.DefineUnmanagedResource (new byte [0]);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			ab.DefineUnmanagedResource (version_res);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}

		try {
			ab.DefineVersionInfoResource ();
			Assert.Fail ("#C1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNull (ex.ParamName, "#C5");
		}

		try {
			ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");
			Assert.Fail ("#D1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.IsNull (ex.ParamName, "#D5");
		}
	}

	[Test] // DefinedUnmanagedResource (String)
	[ExpectedException (typeof (FileNotFoundException))]
	public void TestDefineUnmanagedResource2_ResourceFile_DoesNotExist ()
	{
		ab.DefineUnmanagedResource ("not-exists.txt");
	}

	[Test] // DefinedUnmanagedResource (String)
	public void TestDefineUnmanagedResource2_ResourceFileName_Empty ()
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

	[Test] // DefinedUnmanagedResource (String)
	public void TestDefineUnmanagedResource2_ResourceFileName_Null ()
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

	[Test] // DefineVersionInfoResource ()
	public void TestDefineVersionInfoResource1_Culture_NotSupported ()
	{
		AssemblyName aname = new AssemblyName ();
		aname.CultureInfo = new CultureInfo ("nl-BE");
		aname.Name = "lib";
		aname.Version = new Version (3, 5, 7);

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			aname, AssemblyBuilderAccess.RunAndSave,
			tempDir);

		// AssemblyCulture
		Type attrType = typeof (AssemblyCultureAttribute);
		ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
		CustomAttributeBuilder cab = new CustomAttributeBuilder (
			ci, new object [1] { "doesnotexist" });
		ab.SetCustomAttribute (cab);

		ab.DefineVersionInfoResource ();

		try {
			ab.Save ("lib.dll");
			Assert.Fail ("#A1");
		} catch (CultureNotFoundException ex) {
		}

		ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname,
			AssemblyBuilderAccess.RunAndSave, tempDir);

		// AssemblyCulture
		attrType = typeof (AssemblyCultureAttribute);
		ci = attrType.GetConstructor (new Type [] { typeof (String) });
		cab = new CustomAttributeBuilder (ci, new object [1] { "neutral" });
		ab.SetCustomAttribute (cab);

		ab.DefineVersionInfoResource ();

		try {
			ab.Save ("lib.dll");
			Assert.Fail ("#B1");
		} catch (CultureNotFoundException ex) {
		}
	}

	[Test] // DefineVersionInfoResource ()
	public void TestDefineVersionInfoResource1_ResourceAlreadyDefined ()
	{
		string version_res = Path.Combine (tempDir, "version.res");
		using (FileStream fs = File.OpenWrite (version_res)) {
			fs.WriteByte (0x0a);
		}

		ab.DefineVersionInfoResource ();

		try {
			ab.DefineUnmanagedResource (new byte [0]);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			ab.DefineUnmanagedResource (version_res);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}

		try {
			ab.DefineVersionInfoResource ();
			Assert.Fail ("#C1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNull (ex.ParamName, "#C5");
		}

		try {
			ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");
			Assert.Fail ("#D1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.IsNull (ex.ParamName, "#D5");
		}
	}

	[Test] // DefineVersionInfoResource (String, String, String, String, String)
	public void TestDefineVersionInfoResource2_Culture_NotSupported ()
	{
		AssemblyName aname = new AssemblyName ();
		aname.CultureInfo = new CultureInfo ("nl-BE");
		aname.Name = "lib";
		aname.Version = new Version (3, 5, 7);

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			aname, AssemblyBuilderAccess.RunAndSave,
			tempDir);

		// AssemblyCulture
		Type attrType = typeof (AssemblyCultureAttribute);
		ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
		CustomAttributeBuilder cab = new CustomAttributeBuilder (
			ci, new object [1] { "doesnotexist" });
		ab.SetCustomAttribute (cab);

		ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");

		try {
			ab.Save ("lib.dll");
			Assert.Fail ("#A1");
		} catch (CultureNotFoundException ex) {
		}

		ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname,
			AssemblyBuilderAccess.RunAndSave, tempDir);

		// AssemblyCulture
		attrType = typeof (AssemblyCultureAttribute);
		ci = attrType.GetConstructor (new Type [] { typeof (String) });
		cab = new CustomAttributeBuilder (ci, new object [1] { "neutral" });
		ab.SetCustomAttribute (cab);

		ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");

		try {
			ab.Save ("lib.dll");
			Assert.Fail ("#B1");
		} catch (CultureNotFoundException ex) {
		}
	}

	[Test] // DefineVersionInfoResource (String, String, String, String, String)
	public void TestDefineVersionInfoResource2_ResourceAlreadyDefined ()
	{
		string version_res = Path.Combine (tempDir, "version.res");
		using (FileStream fs = File.OpenWrite (version_res)) {
			fs.WriteByte (0x0a);
		}

		ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");

		try {
			ab.DefineUnmanagedResource (new byte [0]);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNull (ex.ParamName, "#A5");
		}

		try {
			ab.DefineUnmanagedResource (version_res);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsNull (ex.ParamName, "#B5");
		}

		try {
			ab.DefineVersionInfoResource ();
			Assert.Fail ("#C1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsNull (ex.ParamName, "#C5");
		}

		try {
			ab.DefineVersionInfoResource ("A", "1.0", "C", "D", "E");
			Assert.Fail ("#D1");
		} catch (ArgumentException ex) {
			// Native resource has already been defined
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
			Assert.IsNull (ex.InnerException, "#D3");
			Assert.IsNotNull (ex.Message, "#D4");
			Assert.IsNull (ex.ParamName, "#D5");
		}
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

	[Test] // SetCustomAttribute (CustomAttributeBuilder)
	public void TestSetCustomAttribute1 ()
	{
		Assembly a;
		AssemblyName an;
		AssemblyName check;
		Attribute attr;
		string filename;
		
		an = new AssemblyName ();
		an.Name = "TestSetCustomAttributeA";

		ab = domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Save, tempDir);
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "1.2.3.4"}));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "bar"}));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).
			GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }),
			new object [] { AssemblyHashAlgorithm.MD5 }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).
			GetConstructor (new Type [] { typeof (uint) }), new object [] { (uint)0xff }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).
			GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (FooAttribute).
			GetConstructor (Type.EmptyTypes), new object [0]));
		ab.Save ("TestSetCustomAttributeA.dll");

		filename = Path.Combine (tempDir, "TestSetCustomAttributeA.dll");
		check = AssemblyName.GetAssemblyName (filename);
		Assert.AreEqual (CultureInfo.InvariantCulture, check.CultureInfo, "#A1");
		Assert.AreEqual (AssemblyNameFlags.None, check.Flags, "#A2");
		Assert.AreEqual ("TestSetCustomAttributeA, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", check.FullName, "#A3");
		Assert.IsNull (check.GetPublicKey (), "#A4");
		Assert.AreEqual (new byte [0], check.GetPublicKeyToken (), "#A5");
		Assert.AreEqual (AssemblyHashAlgorithm.SHA1, check.HashAlgorithm, "#A6");
		Assert.IsNull (check.KeyPair, "#A7");
		Assert.AreEqual ("TestSetCustomAttributeA", check.Name, "#A8");
		//Assert.AreEqual (ProcessorArchitecture.MSIL, check.ProcessorArchitecture, "#A9");
		Assert.AreEqual (new Version (0, 0, 0, 0), check.Version, "#A10");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, check.VersionCompatibility, "#A11");

		using (FileStream fs = File.OpenRead (filename)) {
			byte [] buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			a = Assembly.Load (buffer);
		}

		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyVersionAttribute));
		Assert.IsNotNull (attr, "#A12a");
		Assert.AreEqual ("1.2.3.4", ((AssemblyVersionAttribute) attr).Version, "#A12b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyCultureAttribute));
		Assert.IsNotNull (attr, "#A13a");
		Assert.AreEqual ("bar", ((AssemblyCultureAttribute) attr).Culture, "#A13b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyAlgorithmIdAttribute));
		Assert.IsNotNull (attr, "#A14a");
		Assert.AreEqual ((uint) AssemblyHashAlgorithm.MD5, ((AssemblyAlgorithmIdAttribute) attr).AlgorithmId, "#A14b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyFlagsAttribute));
		Assert.IsNotNull (attr, "#A15a");
		Assert.AreEqual ((uint) 0xff, ((AssemblyFlagsAttribute) attr).Flags, "#A15b");
		attr = Attribute.GetCustomAttribute (a, typeof (FooAttribute));
		Assert.IsNotNull (attr, "#A16");

		an = new AssemblyName ();
		an.CultureInfo = new CultureInfo ("nl-BE");
		an.Flags = AssemblyNameFlags.Retargetable;
		an.Name = "TestSetCustomAttributeB";
		an.ProcessorArchitecture = ProcessorArchitecture.IA64;
		an.Version = new Version (1, 3, 5, 7);
		an.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;

		ab = domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Save, tempDir);
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "1.2.3.4" }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).
			GetConstructor (new Type [] { typeof (string) }), new object [] { "en-US" }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).
			GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }),
			new object [] { AssemblyHashAlgorithm.MD5 }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).
			GetConstructor (new Type [] { typeof (uint) }), new object [] { (uint) 0x0100 }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).
			GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));
		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (FooAttribute).
			GetConstructor (Type.EmptyTypes), new object [0]));
		ab.Save ("TestSetCustomAttributeB.dll");

		filename = Path.Combine (tempDir, "TestSetCustomAttributeB.dll");
		check = AssemblyName.GetAssemblyName (filename);
		Assert.AreEqual ("nl-BE", check.CultureInfo.Name, "#B1");
		Assert.AreEqual (AssemblyNameFlags.Retargetable, check.Flags, "#B2");
		Assert.AreEqual ("TestSetCustomAttributeB, Version=1.3.5.7, Culture=nl-BE, PublicKeyToken=null, Retargetable=Yes", check.FullName, "#B3");
		Assert.IsNull (check.GetPublicKey (), "#B4");
		Assert.AreEqual (new byte [0], check.GetPublicKeyToken (), "#B5");
		Assert.AreEqual (AssemblyHashAlgorithm.SHA1, check.HashAlgorithm, "#B6");
		Assert.IsNull (check.KeyPair, "#B7");
		Assert.AreEqual ("TestSetCustomAttributeB", check.Name, "#B8");
		//Assert.AreEqual (ProcessorArchitecture.MSIL, check.ProcessorArchitecture, "#B9");
		Assert.AreEqual (new Version (1, 3, 5, 7), check.Version, "#B10");
		Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, check.VersionCompatibility, "#B11");

		using (FileStream fs = File.OpenRead (filename)) {
			byte [] buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			a = Assembly.Load (buffer);
		}

		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyVersionAttribute));
		Assert.IsNotNull (attr, "#B12a");
		Assert.AreEqual ("1.2.3.4", ((AssemblyVersionAttribute) attr).Version, "#B12b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyCultureAttribute));
		Assert.IsNotNull (attr, "#B13a");
		Assert.AreEqual ("en-US", ((AssemblyCultureAttribute) attr).Culture, "#B13b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyAlgorithmIdAttribute));
		Assert.IsNotNull (attr, "#B14a");
		Assert.AreEqual ((uint) AssemblyHashAlgorithm.MD5, ((AssemblyAlgorithmIdAttribute) attr).AlgorithmId, "#B14b");
		attr = Attribute.GetCustomAttribute (a, typeof (AssemblyFlagsAttribute));
		Assert.IsNotNull (attr, "#B15a");
		Assert.AreEqual ((uint) 0x0100, ((AssemblyFlagsAttribute) attr).Flags, "#B15b");
		attr = Attribute.GetCustomAttribute (a, typeof (FooAttribute));
		Assert.IsNotNull (attr, "#B16");
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

	static byte [] token = { 0x0e, 0xea, 0x7c, 0xe6, 0x5f, 0x35, 0xf2, 0xd8 };

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
		Assert.AreEqual (AssemblyNameFlags.None, check.Flags, "#1");
		Assert.IsNull (check.GetPublicKey (), "#2");
		Assert.IsNotNull (check.GetPublicKeyToken (), "#3a");
		Assert.AreEqual (0, check.GetPublicKeyToken ().Length, "#3b");
		Assert.IsTrue (check.FullName.IndexOf ("Version=0.0.0.0") != -1, "#4");
		Assert.IsTrue (check.FullName.IndexOf ("Culture=neutral") != -1, "#5");
		Assert.IsTrue (check.FullName.IndexOf ("PublicKeyToken=null") != -1, "#6");
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
		Assert.AreEqual (AssemblyNameFlags.None, check.Flags, "#1");
		Assert.IsNull (check.GetPublicKey (), "#2");
		Assert.IsNotNull (check.GetPublicKeyToken (), "#3a");
		Assert.AreEqual (0, check.GetPublicKeyToken ().Length, "#3b");
		Assert.IsTrue (check.FullName.IndexOf ("Version=0.0.0.0") != -1, "#4");
		Assert.IsTrue (check.FullName.IndexOf ("Culture=neutral") != -1, "#5");
		Assert.IsTrue (check.FullName.IndexOf ("PublicKeyToken=null") != -1, "#6");
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
		Assert.IsNotNull (check.GetPublicKey (), "#1a");
		Assert.IsTrue (check.GetPublicKey ().Length > 0, "#1b");
		Assert.AreEqual ("0E-EA-7C-E6-5F-35-F2-D8", BitConverter.ToString (check.GetPublicKeyToken ()), "#2");

		Assert.IsTrue (check.FullName.IndexOf ("Version=0.0.0.0") != -1, "#3");
		Assert.IsTrue (check.FullName.IndexOf ("Culture=neutral") != -1, "#4");
		Assert.IsTrue (check.FullName.IndexOf ("PublicKeyToken=0eea7ce65f35f2d8") != -1, "#5");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, check.Flags, "#6");
	}

	[Test]
	public void SaveUnfinishedTypes ()
	{
		mb.DefineType ("TestType", TypeAttributes.Class |
			TypeAttributes.Public | TypeAttributes.Sealed |
			TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
			typeof(object));
		try {
			ab.Save ("def_module");
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// Type 'TestType' was not completed
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsTrue (ex.Message.IndexOf ("TestType") != -1, "#5");
		}
	}

	[Test]
	public void GetModules ()
	{
		Module[] arr;

		arr = ab.GetModules ();
		Assert.IsNotNull (arr, "#A1");
		// FIXME: This doesn't work on mono
		//Assert.IsTrue (arr.Length >= 2, "#A2");
		foreach (Module m in arr)
			Assert.AreEqual (typeof (ModuleBuilder), m.GetType (), "#A3");

		// Test with no modules
		AssemblyBuilder ab2 = genAssembly ();
		arr = ab2.GetModules ();
		Assert.IsNotNull (arr, "#B1");
		Assert.AreEqual (0, arr.Length, "#B2");
	}

	[Test]
	[Category ("NotWorking")] // bug #351932
	public void GetReferencedAssemblies ()
	{
		AssemblyBuilder ab1;
		AssemblyBuilder ab2;
		AssemblyBuilder ab3;
		AssemblyName [] refs;
		TypeBuilder tb1;
		TypeBuilder tb2;
		TypeBuilder tb3;
		TypeBuilder tb4;
		ModuleBuilder mb1;
		ModuleBuilder mb2;
		ModuleBuilder mb3;
		AssemblyName an1 = genAssemblyName ();
		an1.Version = new Version (3, 0);
		AssemblyName an2 = genAssemblyName ();
		an2.Version = new Version ("1.2.3.4");
		an2.KeyPair = new StrongNameKeyPair (strongName);
		AssemblyName an3 = genAssemblyName ();

		ab1 = domain.DefineDynamicAssembly (an1,
			AssemblyBuilderAccess.RunAndSave,
			tempDir);
		ab2 = domain.DefineDynamicAssembly (an2,
			AssemblyBuilderAccess.RunAndSave,
			tempDir);
		ab3 = domain.DefineDynamicAssembly (an3,
			AssemblyBuilderAccess.RunAndSave,
			tempDir);

		refs = ab1.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#A1");
		refs = ab2.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#A2");
		refs = ab3.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#A3");

		mb1 = ab1.DefineDynamicModule (an1.Name + ".dll");
		tb1 = mb1.DefineType ("TestType1", TypeAttributes.Class |
			TypeAttributes.Public, typeof (Attribute));
		tb1.CreateType ();

		mb2 = ab2.DefineDynamicModule (an2.Name + ".dll");
		tb2 = mb2.DefineType ("TestType2", TypeAttributes.Class |
			TypeAttributes.Public, tb1);
		tb2.CreateType ();

		mb3 = ab3.DefineDynamicModule (an3.Name + ".dll");
		tb3 = mb3.DefineType ("TestType3", TypeAttributes.Class |
			TypeAttributes.Public, tb1);
		tb3.CreateType ();
		tb4 = mb3.DefineType ("TestType4", TypeAttributes.Class |
			TypeAttributes.Public, tb2);
		tb4.CreateType ();

		refs = ab1.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#B1");
		refs = ab2.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#B2");
		refs = ab3.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#B3");

		ab1.Save (an1.Name + ".dll");
		ab2.Save (an2.Name + ".dll");
		ab3.Save (an3.Name + ".dll");

		refs = ab1.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#C1");
		refs = ab2.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#C2");
		refs = ab3.GetReferencedAssemblies ();
		Assert.AreEqual (0, refs.Length, "#C3");

		string assemblyFile = Path.Combine (tempDir, an1.Name + ".dll");

		using (FileStream fs = File.OpenRead (assemblyFile)) {
			byte [] buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			Assembly a = Assembly.Load (buffer);
			refs = a.GetReferencedAssemblies ();
			Assert.AreEqual (1, refs.Length, "#D1");

			Assert.IsNull (refs [0].CodeBase, "#D2:CodeBase");
			Assert.IsNotNull (refs [0].CultureInfo, "#D2:CultureInfo");
			Assert.IsNull (refs [0].EscapedCodeBase, "#D2:EscapedCodeBase");
			Assert.AreEqual (AssemblyNameFlags.None, refs [0].Flags, "#D2:Flags");
			Assert.AreEqual (typeof (object).FullName, refs [0].FullName, "#D2:FullName");
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, refs [0].HashAlgorithm, "#D2:HashAlgorithm");
			Assert.IsNull (refs [0].KeyPair, "#D2:KeyPair");
			Assert.AreEqual ("mscorlib", refs [0].Name, "#D2:Name");
			Assert.AreEqual (ProcessorArchitecture.None, refs [0].ProcessorArchitecture, "#D2:PA");

			string FxVersion;
#if MOBILE
			FxVersion = "2.0.5.0;";
#else
			FxVersion = "4.0.0.0;";
#endif
			Assert.AreEqual (new Version (FxVersion), refs [0].Version, "#D2:Version");
			Assert.AreEqual (AssemblyVersionCompatibility.SameMachine,
				refs [0].VersionCompatibility, "#D2:VersionCompatibility");
			Assert.IsNull (refs [0].GetPublicKey (), "#D2:GetPublicKey");
			Assert.IsNotNull (refs [0].GetPublicKeyToken (), "#D2:GetPublicKeyToken(a)");
			Assert.AreEqual (8, refs [0].GetPublicKeyToken ().Length, "#D2:GetPublicKeyToken(b)");
			Assert.AreEqual (refs [0].FullName, refs [0].ToString (), "#D2:ToString");
		}

		assemblyFile = Path.Combine (tempDir, an2.Name + ".dll");

		using (FileStream fs = File.OpenRead (assemblyFile)) {
			byte [] buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			Assembly a = Assembly.Load (buffer);
			refs = a.GetReferencedAssemblies ();
			Assert.AreEqual (1, refs.Length, "#E1");

			Assert.IsNull (refs [0].CodeBase, "#E2:CodeBase");
			Assert.IsNotNull (refs [0].CultureInfo, "#E2:CultureInfo(a)");
			Assert.AreEqual (CultureInfo.InvariantCulture, refs [0].CultureInfo, "#E2:CultureInfo(b)");
			Assert.IsNull (refs [0].EscapedCodeBase, "#E2:EscapedCodeBase");
			Assert.AreEqual (AssemblyNameFlags.None, refs [0].Flags, "#E2:Flags");
			Assert.AreEqual (an1.Name + ", Version=3.0.0.0, Culture=neutral, PublicKeyToken=null", refs [0].FullName, "#E2:FullName");
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, refs [0].HashAlgorithm, "#E2:HashAlgorithm");
			Assert.IsNull (refs [0].KeyPair, "#E2:KeyPair");
			Assert.AreEqual (an1.Name, refs [0].Name, "#E2:Name");
			Assert.AreEqual (ProcessorArchitecture.None, refs [0].ProcessorArchitecture, "#E2:PA");
			Assert.AreEqual (new Version (3, 0, 0, 0), refs [0].Version, "#E2:Version");
			Assert.AreEqual (AssemblyVersionCompatibility.SameMachine,
				refs [0].VersionCompatibility, "#E2:VersionCompatibility");
			Assert.IsNull (refs [0].GetPublicKey (), "#E2:GetPublicKey");
			Assert.IsNotNull (refs [0].GetPublicKeyToken (), "#E2:GetPublicKeyToken(a)");
			Assert.AreEqual (0, refs [0].GetPublicKeyToken ().Length, "#E2:GetPublicKeyToken(b)");
			Assert.AreEqual (refs [0].FullName, refs [0].ToString (), "#E2:ToString");
		}

		assemblyFile = Path.Combine (tempDir, an3.Name + ".dll");

		using (FileStream fs = File.OpenRead (assemblyFile)) {
			byte [] buffer = new byte [fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			Assembly a = Assembly.Load (buffer);
			refs = a.GetReferencedAssemblies ();
			Assert.AreEqual (2, refs.Length, "#F1");

			Assert.IsNull (refs [0].CodeBase, "#F2:CodeBase");
			Assert.IsNotNull (refs [0].CultureInfo, "#F2:CultureInfo(a)");
			Assert.AreEqual (CultureInfo.InvariantCulture, refs [0].CultureInfo, "#F2:CultureInfo(b)");
			Assert.IsNull (refs [0].EscapedCodeBase, "#F2:EscapedCodeBase");
			Assert.AreEqual (AssemblyNameFlags.None, refs [0].Flags, "#F2:Flags");
			Assert.AreEqual (an1.Name + ", Version=3.0.0.0, Culture=neutral, PublicKeyToken=null", refs [0].FullName, "#F2:FullName");
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, refs [0].HashAlgorithm, "#F2:HashAlgorithm");
			Assert.IsNull (refs [0].KeyPair, "#F2:KeyPair");
			Assert.AreEqual (an1.Name, refs [0].Name, "#F2:Name");
			Assert.AreEqual (ProcessorArchitecture.None, refs [0].ProcessorArchitecture, "#F2:PA");
			Assert.AreEqual (new Version (3, 0, 0, 0), refs [0].Version, "#F2:Version");
			Assert.AreEqual (AssemblyVersionCompatibility.SameMachine,
				refs [0].VersionCompatibility, "#F2:VersionCompatibility");
			Assert.IsNull (refs [0].GetPublicKey (), "#F2:GetPublicKey");
			Assert.IsNotNull (refs [0].GetPublicKeyToken (), "#F2:GetPublicKeyToken(a)");
			Assert.AreEqual (0, refs [0].GetPublicKeyToken ().Length, "#F2:GetPublicKeyToken(b)");
			Assert.AreEqual (refs [0].FullName, refs [0].ToString (), "#F2:ToString");

			Assert.IsNull (refs [1].CodeBase, "#F3:CodeBase");
			Assert.IsNotNull (refs [1].CultureInfo, "#F3:CultureInfo(a)");
			Assert.AreEqual (CultureInfo.InvariantCulture, refs [1].CultureInfo, "#F3:CultureInfo(b)");
			Assert.IsNull (refs [1].EscapedCodeBase, "#F3:EscapedCodeBase");
			Assert.AreEqual (AssemblyNameFlags.None, refs [1].Flags, "#F3:Flags");
			Assert.AreEqual (an2.Name + ", Version=1.2.3.4, Culture=neutral, PublicKeyToken=0eea7ce65f35f2d8", refs [1].FullName, "#F3:FullName");
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, refs [1].HashAlgorithm, "#F3:HashAlgorithm");
			Assert.IsNull (refs [1].KeyPair, "#F3:KeyPair");
			Assert.AreEqual (an2.Name, refs [1].Name, "#F3:Name");
			Assert.AreEqual (ProcessorArchitecture.None, refs [1].ProcessorArchitecture, "#F3:PA");
			Assert.AreEqual (new Version (1, 2, 3, 4), refs [1].Version, "#F3:Version");
			Assert.AreEqual (AssemblyVersionCompatibility.SameMachine,
				refs [1].VersionCompatibility, "#F3:VersionCompatibility");
			Assert.IsNull (refs [1].GetPublicKey (), "#F3:GetPublicKey");
			Assert.AreEqual (token, refs [1].GetPublicKeyToken (), "#F3:GetPublicKeyToken");
			Assert.AreEqual (refs [1].FullName, refs [1].ToString (), "#F3:ToString");
		}
	}

	[Test] // bug #78724
	public void GetTypes ()
	{
		TypeBuilder tb = mb.DefineType ("sometype");
		tb.CreateType ();

		Type[] types = ab.GetTypes ();
		Assert.AreEqual (1, types.Length, "#1");
		Assert.AreEqual ("sometype", types[0].Name, "#2");
	}

	[Test]
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

		Assert.AreEqual ("AssemblyNameTest_PublicKey, Version=1.2.3.4", assemblyName.FullName, "#A1");

		const string fullName = "AssemblyNameTest_PublicKey, Version=1.2.3.4, Culture=neutral, PublicKeyToken=0eea7ce65f35f2d8";

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			assemblyName, AssemblyBuilderAccess.Save, tempDir);

		AssemblyName abName = ab.GetName ();
		Assert.AreEqual (CultureInfo.InvariantCulture, abName.CultureInfo, "#B1");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, abName.Flags, "#B2");
		Assert.IsNotNull (abName.GetPublicKey () != null, "#B3a");
		Assert.IsTrue (abName.GetPublicKey ().Length > 0, "#B3b");
		Assert.IsNotNull (abName.GetPublicKeyToken (), "#B4a");
		Assert.IsTrue (abName.GetPublicKeyToken ().Length > 0, "#B4b");
		Assert.AreEqual (fullName, abName.FullName, "#B5");

		ab.Save ("AssemblyNameTest_PublicKey.dll");
		AssemblyName bakedName = AssemblyName.GetAssemblyName (Path.Combine(
			tempDir, "AssemblyNameTest_PublicKey.dll"));

		Assert.AreEqual (CultureInfo.InvariantCulture, bakedName.CultureInfo, "#C1");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, bakedName.Flags, "#C2");
		Assert.IsNotNull (bakedName.GetPublicKeyToken (), "#C3");
		Assert.IsNotNull (bakedName.GetPublicKey (), "#C4");
		Assert.AreEqual (fullName, bakedName.FullName, "#C5");
	}

	[Test]
	public void AssemblyName_MoreCultureInfo ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest_MoreCultureInfo";
		assemblyName.Version = new Version ("1.2.3.4");
		assemblyName.KeyPair = new StrongNameKeyPair (strongName);

		Assert.IsNull (assemblyName.CultureInfo, "#A1");
		Assert.AreEqual ("AssemblyNameTest_MoreCultureInfo, Version=1.2.3.4", assemblyName.FullName, "#A2");

		const string fullName = "AssemblyNameTest_MoreCultureInfo, Version=1.2.3.4, Culture=neutral, PublicKeyToken=0eea7ce65f35f2d8";

		AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
			assemblyName, AssemblyBuilderAccess.Save, tempDir);

		AssemblyName abName = ab.GetName ();
		Assert.IsNotNull (abName.CultureInfo != null, "#B1");
		Assert.IsTrue (abName.CultureInfo != CultureInfo.InvariantCulture, "#B2a");
		Assert.AreEqual (CultureInfo.InvariantCulture.LCID, abName.CultureInfo.LCID, "#B2a");
		Assert.AreEqual (AssemblyNameFlags.PublicKey, abName.Flags, "#B3");
		Assert.AreEqual (fullName, abName.FullName, "#B4");

		ab.Save ("AssemblyNameTest_MoreCultureInfo.dll");

		AssemblyName bakedName = AssemblyName.GetAssemblyName (Path.Combine(
			tempDir, "AssemblyNameTest_MoreCultureInfo.dll"));

		Assert.IsNotNull (bakedName.CultureInfo, "#C1");

		Assert.IsTrue (abName.CultureInfo != CultureInfo.InvariantCulture, "#C2a");
		Assert.AreEqual (CultureInfo.InvariantCulture.LCID, abName.CultureInfo.LCID, "#C2b");
		Assert.AreEqual (fullName, bakedName.FullName, "#C3");
	}

	[Test]
	public void AssemblyName_NoVersion ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";

		const string fullName = "AssemblyNameTest, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
		const string abName = "AssemblyNameTest, Version=0.0.0.0";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);
	}

	[Test]
	public void AssemblyName_Version ()
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";
		assemblyName.Version = new Version (1, 2, 3, 4);

		string fullName = "AssemblyNameTest, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null";
		string abName = "AssemblyNameTest, Version=1.2.3.4";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);

		assemblyName = new AssemblyName ();
		assemblyName.Name = "AssemblyNameTest";
		assemblyName.Version = new Version (1, 2);

		fullName = "AssemblyNameTest, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null";
		abName = "AssemblyNameTest, Version=1.2.0.0";

		AssertAssemblyName (tempDir, assemblyName, abName, fullName);
	}

	[Test]
	public void GetType_IgnoreCase ()
	{
		TypeBuilder tb = mb.DefineType ("Foo.Test2", TypeAttributes.Public, typeof (object));
		tb.CreateType ();

		Type t;

		t = ab.GetType ("foo.Test2", true, true);
		Assert.AreEqual ("Test2", t.Name, "#1");

		t = ab.GetType ("foo.test2", true, true);
		Assert.AreEqual ("Test2", t.Name, "#2");

		t = ab.GetType ("Foo.test2", true, true);
		Assert.AreEqual ("Test2", t.Name, "#3");
	}


	[Test]
	public void TestGetType ()
	{
		TypeBuilder tb = mb.DefineType ("Test", TypeAttributes.Public);

		Assert.IsNull (ab.GetType ("Test", false, true), "#1");
		try {
			ab.GetType ("Test", true, true);
			Assert.Fail ("#2");
		} catch (TypeLoadException) { }

		var res = tb.CreateType ();

		Assert.AreSame (res, ab.GetType ("Test", false, true), "#3");
	}

	[Test]
	public void GetModule ()
	{
		var ab = genAssembly ();
		Assert.IsNull (ab.GetModule ("Foo"), "#1");

		var modA = ab.DefineDynamicModule ("Foo");
		var modB = ab.DefineDynamicModule ("Bar");

		Assert.AreSame (modA, ab.GetModule ("Foo"), "#2"); 
		Assert.AreSame (modB, ab.GetModule ("Bar"), "#3"); 
		Assert.IsNull (ab.GetModule ("FooBar"), "#4");
	}
	
	[Test]
	public void GetModules2 ()
	{
		//XXX this is not the v4 behavior since it returns
		//the manifest module in the place of the first one
		var ab = genAssembly ();
		var modA = ab.DefineDynamicModule ("Foo");
		var modB = ab.DefineDynamicModule ("Bar");
		Assert.AreEqual (2, ab.GetModules ().Length, "#1");
		Assert.AreSame (modA, ab.GetModules () [0], "#2");
		Assert.AreSame (modB, ab.GetModules () [1], "#3");
	}

	[Test]
	[Category ("NotDotNet")] // MS returns the real deal
	public void GetReferencedAssemblies_Trivial ()
	{
		Assert.IsNotNull (ab.GetReferencedAssemblies (), "#1");
	}
	
	[Test]
	public void GetLoadedModules ()
	{
		var res = ab.GetLoadedModules (true);
		Assert.IsNotNull (res, "#1");
		Assert.AreEqual (1, res.Length, "#2");
		Assert.AreEqual (mb, res [0], "#3");
	}

	[ExpectedException (typeof (TypeLoadException))]
	public void GetCustomAttributes_NotCreated ()
	{
		AssemblyBuilder ab = genAssembly ();
		ModuleBuilder mb = ab.DefineDynamicModule("tester", "tester.dll", false);
		TypeBuilder tb = mb.DefineType ("T");
		tb.SetParent (typeof (Attribute));
		ConstructorBuilder ctor = tb.DefineDefaultConstructor (MethodAttributes.Public);
		object [] o = new object [0];
		CustomAttributeBuilder cab = new CustomAttributeBuilder (ctor, o);
		ab.SetCustomAttribute (cab);

		ab.GetCustomAttributes (true);
	}


	[Test]
	public void GetTypesWithUnfinishedTypeBuilder ()
	{
		AssemblyBuilder ab = genAssembly ();
		ModuleBuilder mb = ab.DefineDynamicModule("tester", "tester.dll", false);
		mb.DefineType ("K").CreateType ();
		var tb = mb.DefineType ("T");

		try {
			ab.GetTypes ();
			Assert.Fail ("#1");
		} catch (ReflectionTypeLoadException ex) {
			Assert.AreEqual (1, ex.Types.Length, "#2");
			Assert.AreEqual (1, ex.LoaderExceptions.Length, "#3");
			Assert.IsNull (ex.Types [0], "#4");
			Assert.IsTrue (ex.LoaderExceptions [0] is TypeLoadException, "#5");
		}

		tb.CreateType ();
		var types = ab.GetTypes ();
		Assert.AreEqual (2, types.Length, "#5");
		foreach (var t in types)
			Assert.IsFalse (t is TypeBuilder, "#6_" + t.Name);
	}

	[Test]
	public void DynamicAssemblyGenerationInCurrentDomainShouldNotChangeTheOrderOfCurrentDomainGetAssemblies ()
	{
		var initialPosition = GetAssemblyPositionForType (GetType ());
		DefineDynamicAssembly (AppDomain.CurrentDomain);

		var currentPosition = GetAssemblyPositionForType (GetType ());
		Assert.AreEqual (initialPosition, currentPosition);
	}

	static void DefineDynamicAssembly (AppDomain domain)
	{
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "MyDynamicAssembly";

		AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);
		ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule ("MyDynamicModule");
		TypeBuilder typeBuilder = moduleBuilder.DefineType ("MyDynamicType", TypeAttributes.Public);
		ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, null);
		ILGenerator ilGenerator = constructorBuilder.GetILGenerator ();
		ilGenerator.EmitWriteLine ("MyDynamicType instantiated!");
		ilGenerator.Emit (OpCodes.Ret);
		typeBuilder.CreateType ();
	}

	static int GetAssemblyPositionForType (Type type)
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies ();
		for (int i = 0; i < assemblies.Length; i++)
			if (type.Assembly == assemblies [i])
				return i;
		return -1;
	}


	private static void AssertAssemblyName (string tempDir, AssemblyName assemblyName, string abName, string fullName)
	{
		AppDomain currentDomain = AppDomain.CurrentDomain;
		AppDomain newDomain = null;

		try {
			AssemblyBuilder ab = currentDomain.DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Save, tempDir);
			ab.Save (assemblyName.Name + ".dll");

			// on .NET 2.0, the full name of the AssemblyBuilder matches the
			// fully qualified assembly name
			Assert.AreEqual (fullName, ab.FullName, "#1");

			AssemblyName an = ab.GetName ();

			Assert.AreEqual (AssemblyNameFlags.PublicKey, an.Flags, "#2");
			Assert.IsNotNull (an.GetPublicKey (), "#3a");
			Assert.AreEqual (0, an.GetPublicKey ().Length, "#3b");
			Assert.IsNotNull (an.GetPublicKeyToken (), "#4a");
			Assert.AreEqual (0, an.GetPublicKeyToken ().Length, "#4b");

			// load assembly in separate domain, so we can clean-up after the 
			// test
			newDomain = AppDomain.CreateDomain ("test2", null, currentDomain.SetupInformation);

			Helper helper = new Helper (Path.Combine (tempDir, assemblyName.Name + ".dll"),
				fullName);
			newDomain.DoCallBack (new CrossAppDomainDelegate (helper.Test));
		} finally {
#if !MONODROID
			// RUNTIME: crash
			// AppDomain unloading crashes the runtime on Android
			if (newDomain != null) {
				AppDomain.Unload (newDomain);
			}
#endif
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


	[Test]//Bug #7126
	public void CannotCreateInstanceOfSaveOnlyAssembly ()
	{
		var asm_builder = genAssembly (AssemblyBuilderAccess.Save);
        var mod_builder = asm_builder.DefineDynamicModule("Foo", "Foo.dll");

        var type_builder = mod_builder.DefineType("Foo",
                TypeAttributes.Public | TypeAttributes.Sealed |
                TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

        var type = type_builder.CreateType();

		try {
			Activator.CreateInstance(type);
			Assert.Fail ("Cannot create instance of save only type");
		} catch (NotSupportedException e) {
		}
     }

	class AssemblyBuilderResolver {
		private Assembly mock;
		private ResolveEventHandler d;
		private string theName;

		public AssemblyBuilderResolver (string theName) {
			mock = CreateMock (theName);
			d = new ResolveEventHandler (HandleResolveEvent);
			this.theName = theName;
		}

		public void StartHandling () {
			AppDomain.CurrentDomain.AssemblyResolve += d;
		}

		public void StopHandling () {
			AppDomain.CurrentDomain.AssemblyResolve -= d;
		}

		public Assembly HandleResolveEvent (Object sender, ResolveEventArgs args) {
			if (args.Name.StartsWith (theName))
				return mock;
			else
				return null;
		}

		private static Assembly CreateMock (string name) {
			var an = new AssemblyName (name);
			var ab = AssemblyBuilder.DefineDynamicAssembly (an, AssemblyBuilderAccess.ReflectionOnly);
			var mb = ab.DefineDynamicModule (an.Name);

			// Just make some content for the assembly
			var tb = mb.DefineType ("Foo", TypeAttributes.Public);
			tb.DefineDefaultConstructor (MethodAttributes.Public);

			tb.CreateType ();

			return ab;
		}
	}

	[Test]
	public void ResolveEventHandlerReflectionOnlyError ()
	{
		// Regression test for 57850.

		// If a ResolveEventHandler returns a reflection-only
		// AssemblyBuilder, we should throw a FileNotFoundException.
		var s = "ResolveEventHandlerReflectionOnlyErrorAssembly";
		var h = new AssemblyBuilderResolver (s);
		Assert.Throws<FileNotFoundException>(() => {
				h.StartHandling ();
				var aName = new AssemblyName (s);
				try {
					AppDomain.CurrentDomain.Load (aName);
				} finally {
					h.StopHandling ();
				}
			});
	}

	[Test]
	public void GetReflectionOnly ()
	{
		// Regression test for 13028.
		// Asserts ReflectionOnly is actually implemented.
		AssemblyBuilder ab1 = genAssembly ();
		AssemblyBuilder ab2 = genAssembly (AssemblyBuilderAccess.ReflectionOnly);
		Assert.IsFalse (ab1.ReflectionOnly, "#1");
		Assert.IsTrue (ab2.ReflectionOnly, "#2");
	}


}
}
