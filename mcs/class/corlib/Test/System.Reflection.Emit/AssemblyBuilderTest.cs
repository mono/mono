//
// AssemblyBuilderTest.cs - NUnit Test Cases for the AssemblyBuilder class
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
using System.Configuration.Assemblies;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class AssemblyBuilderTest : Assertion
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

	string tempDir = Path.Combine (Path.GetTempPath (), "MonoTests.System.Reflection.Emit.AssemblyBuilderTest");

	[SetUp]
	protected void SetUp () {
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
		ab.DefineDynamicModule ("def_module");
	}

	[TearDown]
	protected void TearDown () {
		if (Directory.Exists (tempDir))
			Directory.Delete (tempDir, true);
	}		

	private AssemblyName genAssemblyName () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.AssemblyBuilderTest" + (nameIndex ++);
		return assemblyName;
	}

	private AssemblyBuilder genAssembly () {
		return domain.DefineDynamicAssembly (genAssemblyName (),
											 AssemblyBuilderAccess.RunAndSave,
											 tempDir);
	}

	private MethodInfo genEntryFunction (AssemblyBuilder assembly) {
		ModuleBuilder module = assembly.DefineDynamicModule("module1");
		TypeBuilder tb = module.DefineType ("A");
		MethodBuilder mb = tb.DefineMethod ("A",
			MethodAttributes.Static, typeof (void), new Type [0]);
		mb.GetILGenerator ().Emit (OpCodes.Ret);
		return mb;
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestCodeBase () {
		string codebase = ab.CodeBase;
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestLocation () {
		string location = ab.Location;
	}

	public void TestEntryPoint () {
		AssertEquals ("EntryPoint defaults to null",
					  null, ab.EntryPoint);

		MethodInfo mi = genEntryFunction (ab);
		ab.SetEntryPoint (mi);

		AssertEquals ("EntryPoint works", mi, ab.EntryPoint);
	}

	public void TestSetEntryPoint () {
		// Check invalid arguments
		try {
			ab.SetEntryPoint (null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		// Check method from other assembly
		try {
			ab.SetEntryPoint (typeof (AssemblyBuilderTest).GetMethod ("TestSetEntryPoint"));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestIsDefined () {
		CustomAttributeBuilder cab = new CustomAttributeBuilder (typeof (FooAttribute).GetConstructor (new Type [1] {typeof (string)}), new object [1] { "A" });
		ab.SetCustomAttribute (cab);

		AssertEquals ("IsDefined works",
					  true, ab.IsDefined (typeof (FooAttribute), false));
		AssertEquals ("IsDefined works",
					  false, ab.IsDefined (typeof (AssemblyVersionAttribute), false));
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceNames () {
		ab.GetManifestResourceNames ();
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceInfo () {
		ab.GetManifestResourceInfo ("foo");
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceStream1 () {
		ab.GetManifestResourceStream ("foo");
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetManifestResourceStream2 () {
		ab.GetManifestResourceStream (typeof (int), "foo");
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFiles1 () {
		ab.GetFiles ();
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFiles2 () {
		ab.GetFiles (true);
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetFile () {
		ab.GetFile ("foo");
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestGetExportedTypes () {
		ab.GetExportedTypes ();
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestGetDynamicModule1 () {
		ab.GetDynamicModule (null);
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestGetDynamicModule2 () {
		ab.GetDynamicModule ("");
	}

	public void TestGetDynamicModule3 () {
		AssertNull (ab.GetDynamicModule ("FOO2"));

		ModuleBuilder mb = ab.DefineDynamicModule ("FOO");

		AssertEquals (mb, ab.GetDynamicModule ("FOO"));

		AssertNull (ab.GetDynamicModule ("FOO4"));
	}

#if NET_1_1
	public void TestImageRuntimeVersion () {
		string version = ab.ImageRuntimeVersion;
		Assert (version.Length > 0);
	}
#endif

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddResourceFileNullName () {
		ab.AddResourceFile (null, "foo.txt");
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddResourceFileNullFilename () {
		ab.AddResourceFile ("foo", null);
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestAddResourceFileEmptyName () {
		ab.AddResourceFile ("", "foo.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestAddResourceFileEmptyFilename () {
		ab.AddResourceFile ("foo", "");
	}

	[ExpectedException (typeof (FileNotFoundException))]
	public void TestAddResourceFileNonexistentFile () {
		ab.AddResourceFile ("foo", "not-existent.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestAddResourceFileDuplicateFileName () {
		ab.AddResourceFile ("foo", "res1.txt");
		ab.AddResourceFile ("foo2", "res1.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestAddResourceFileDuplicateName () {
		ab.AddResourceFile ("foo", "res1.txt");
		ab.AddResourceFile ("foo", "res2.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestAddResourceFileFilenameIncludesPath () {
		ab.AddResourceFile ("foo", "/tmp/res1.txt");
	}

	public void TestAddResourceFile () {
		ab.AddResourceFile ("foo", "res2.txt", ResourceAttributes.Public);

		ab.Save ("TestAddResourceFile.dll");

		// TODO: Test reading back
	}

	public void TestDefineResource () {
		ab.DefineResource ("foo", "FOO", "foo.txt", ResourceAttributes.Public);
		ab.DefineResource ("foo2", "FOO", "foo2.txt");

		ab.Save ("TestDefineResource.dll");
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestDefineDynamicModuleNullName () {
		ab.DefineDynamicModule (null, "foo.txt");
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestDefineDynamicModuleNullFilename () {
		ab.DefineDynamicModule ("foo", null);
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModuleEmptyName () {
		ab.DefineDynamicModule ("", "foo.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModuleEmptyFilename () {
		ab.DefineDynamicModule ("foo", "");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModuleDuplicateFileName () {
		ab.DefineDynamicModule ("foo", "res1.txt");
		ab.DefineDynamicModule ("foo2", "res1.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModuleDuplicateName () {
		ab.DefineDynamicModule ("foo", "res1.txt");
		ab.DefineDynamicModule ("foo", "res2.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModuleFilenameIncludesPath () {
		ab.DefineDynamicModule ("foo", "/tmp/res1.txt");
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModule5 () {
		// Filename without extension
		ab.DefineDynamicModule ("foo", "foo");
	}

	/*
	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineDynamicModule6 () {
		// Name too long
		string name = "";
		for (int i = 0; i < 259; ++i)
			name = name + "A";

		try {
			ab.DefineDynamicModule (name);
		}
		catch (Exception) {
			Fail ();
		}

		name = name + "A";
		// LAMESPEC: According to MSDN, this should throw an ArgumentException

		ab.DefineDynamicModule (name);
	}
	*/

	[ExpectedException (typeof (InvalidOperationException))]
	public void TestDefineDynamicModule7 () {
		// Called when assembly was already saved
		ab.Save ("TestDefineDynamicModule7.dll");
		ab.DefineDynamicModule ("foo", "foo.dll");
	}

	[ExpectedException (typeof (NotSupportedException))]
	public void TestDefineDynamicModule8 () {
		// Called on an assembly defined with the Run attribute
		AssemblyBuilder ab = 
			domain.DefineDynamicAssembly (genAssemblyName (),
										  AssemblyBuilderAccess.Run,
										  tempDir);
		ab.DefineDynamicModule ("foo", "foo.dll");
	}

	public void TestDefineDynamicModule () {
		ab.DefineDynamicModule ("foo", "foo.dll");
		ab.DefineDynamicModule ("foo2", true);
		ab.DefineDynamicModule ("foo3", "foo3.dll");
		ab.DefineDynamicModule ("foo4", "foo4.dll", true);
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestDefineUnmanagedResource1 () {
		// Null argument
		ab.DefineUnmanagedResource ((byte[])null);
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestDefineUnmanagedResource2 () {
		// Null argument
		ab.DefineUnmanagedResource ((string)null);
	}

	[ExpectedException (typeof (ArgumentException))]
	public void TestDefineUnmanagedResource3 () {
		// Empty filename
		ab.DefineUnmanagedResource ("");
	}

	[ExpectedException (typeof (FileNotFoundException))]
	public void TestDefineUnmanagedResource4 () {
		// Nonexistent file
		ab.DefineUnmanagedResource ("not-exists.txt");
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute1 () {
		// Null argument
		ab.SetCustomAttribute (null);
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute2 () {
		// Null constructor
		ab.SetCustomAttribute (null, new byte [0]);
	}

	[ExpectedException (typeof (ArgumentNullException))]
	public void TestSetCustomAttribute3 () {
		// Null blob
		ab.SetCustomAttribute (typeof(AssemblyCompanyAttribute).GetConstructor (new Type [] { typeof (String) }), null);
	}

	public void TestSetCustomAttribute () {
		// Test common custom attributes

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyVersionAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { "1.2.3.4"}));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyKeyFileAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { "foo"}));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { "bar"}));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }), new object [] { AssemblyHashAlgorithm.MD5 }));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).GetConstructor (new Type [] { typeof (uint) }), new object [] { (uint)0x0100 }));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));

		ab.SetCustomAttribute (typeof (FooAttribute).GetConstructor (new Type [] {}), new byte [0]);

		ab.Save ("TestSetCustomAttribute.dll");

		/* We should read back the assembly and check the attributes ... */
	}

}
}

