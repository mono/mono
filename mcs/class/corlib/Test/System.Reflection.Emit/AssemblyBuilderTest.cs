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

	static ModuleBuilder mb;

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
		mb = ab.DefineDynamicModule ("def_module");
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

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyCultureAttribute).GetConstructor (new Type [] { typeof (string) }), new object [] { "bar"}));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyAlgorithmIdAttribute).GetConstructor (new Type [] { typeof (AssemblyHashAlgorithm) }), new object [] { AssemblyHashAlgorithm.MD5 }));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyFlagsAttribute).GetConstructor (new Type [] { typeof (uint) }), new object [] { (uint)0x0100 }));

		ab.SetCustomAttribute (new CustomAttributeBuilder (typeof (AssemblyDelaySignAttribute).GetConstructor (new Type [] { typeof (bool) }), new object [] { true }));

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
		AssertNull ("Token", check.GetPublicKeyToken ());
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
		AssertNull ("Token", check.GetPublicKeyToken ());
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
		AssertEquals ("Token", "0E-EA-7C-E6-5F-35-F2-D8", BitConverter.ToString (check.GetPublicKeyToken ()));
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void SaveUnfinishedTypes ()
	{
		TypeBuilder typeBuilder = mb.DefineType ("TestType", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(object));

		ab.Save ("def_module");
	}

}
}

