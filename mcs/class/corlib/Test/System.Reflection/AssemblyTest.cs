//
// System.Reflection.Assembly Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Philippe Lavoie (philippe.lavoie@cactus.ca)
//	Sebastien Pouliot (sebastien@ximian.com)
//      Aleksey Kliger (aleksey@xamarin.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2015 Xamarin, Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Linq;
using System.Resources;

// Used by GetType_TypeForwarder_Nested ()
[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof(System.Globalization.CultureInfo))]

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class AssemblyTest
	{
		static string BaseTempFolder = Path.Combine (Path.GetTempPath (),
			"MonoTests.System.Reflection.AssemblyTest");
		static string TempFolder;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				// Try to cleanup from any previous NUnit run.
				Directory.Delete (BaseTempFolder, true);
			} catch (Exception) {
			}
		}

		[SetUp]
		public void SetUp ()
		{
			int i = 0;
			do {
				TempFolder = Path.Combine (BaseTempFolder, (++i).ToString());
			} while (Directory.Exists (TempFolder));
			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				// This throws an exception under MS.NET and Mono on Windows,
				// since the directory contains loaded assemblies.
				Directory.Delete (TempFolder, true);
			} catch (Exception) {
			}
		}

		[Test] 
		public void CreateInstance() 
		{
			Type type = typeof (AssemblyTest);
			Object obj = type.Assembly.CreateInstance ("MonoTests.System.Reflection.AssemblyTest");
			Assert.IsNotNull (obj, "#01");
			Assert.AreEqual (GetType (), obj.GetType (), "#02");
		}

		[Test] 
		public void CreateInvalidInstance() 
		{
			Type type = typeof (AssemblyTest);
			Object obj = type.Assembly.CreateInstance("NunitTests.ThisTypeDoesNotExist");
			Assert.IsNull (obj, "#03");
		}

		[Test] // bug #49114
		[ExpectedException (typeof (ArgumentException))]
		public void GetType_TypeName_Invalid () 
		{
			typeof (int).Assembly.GetType ("&blabla", true, true);
		}

		[Test] // bug #17571
		[ExpectedException (typeof (ArgumentException))]
		public void GetType_Invalid_RefPtr () {
			typeof (int).Assembly.GetType ("System.Int32&*", true, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetType_Invalid_RefArray () {
			typeof (int).Assembly.GetType ("System.Int32&[]", true, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetType_Invalid_RefGeneric () {
			typeof (int).Assembly.GetType ("System.Tuple`1&[System.Int32]", true, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetType_Invalid_PtrGeneric () {
			typeof (int).Assembly.GetType ("System.Tuple`1*[System.Int32]", true, true);
		}

		[Test]
		public void GetType_ComposeModifiers () {
			var a = typeof(int).Assembly;

			var e1 = typeof (Int32).MakePointerType().MakeByRefType();
			var t1 = a.GetType ("System.Int32*&", true, true);
			Assert.AreEqual (e1, t1, "#1");

			var e2 = typeof (Int32).MakeArrayType(2).MakeByRefType();
			var t2 = a.GetType ("System.Int32[,]&", true, true);
			Assert.AreEqual (e2, t2, "#2");

			var e3 = typeof (Int32).MakePointerType().MakeArrayType();
			var t3 = a.GetType ("System.Int32*[]", true, true);
			Assert.AreEqual (e3, t3, "#3");

			var e4 = typeof (Int32).MakeArrayType().MakePointerType().MakePointerType().MakeArrayType().MakePointerType().MakeByRefType();
			var t4 = a.GetType ("System.Int32[]**[]*&", true, true);
			Assert.AreEqual (e4, t4, "#4");
				
		}
		
		[Test] // bug #334203
		public void GetType_TypeName_AssemblyName ()
		{
			Assembly a = typeof (int).Assembly;
			string typeName = typeof (string).AssemblyQualifiedName;
			try {
				a.GetType (typeName, true, false);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			Type type = a.GetType (typeName, false);
			Assert.IsNull (type, "#B1");
			type = a.GetType (typeName, false, true);
			Assert.IsNull (type, "#B2");
		}

		[Test]
		public void GetType_TypeForwarder_Nested () {
			// System.Globalization is a PCL assembly
			Type t = typeof (AssemblyTest).Assembly.GetType ("System.Globalization.CultureInfo/Data");
			Assert.IsNotNull (t);
			Assert.AreEqual ("System.Globalization.CultureInfo+Data", t.FullName);
		}

		[Test]
		public void GetEntryAssembly ()
		{
			// note: only available in default appdomain
			// http://weblogs.asp.net/asanto/archive/2003/09/08/26710.aspx
			// Not sure we should emulate this behavior.
#if MONOTOUCH_WATCH || WASM
			Assert.IsNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
			Assert.IsTrue (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
#elif !MONODROID
			string fname = AppDomain.CurrentDomain.FriendlyName;
			if (fname.EndsWith (".dll")) { // nunit-console
				Assert.IsNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
				Assert.IsFalse (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
			} else { // gnunit
				Assert.IsNotNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
				Assert.IsTrue (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
			}
#else
			Assert.IsNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
			Assert.IsTrue (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
#endif
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME // Reflection.Emit is not supported.
		[Test]
		public void GetModules_MissingFile ()
		{
			AssemblyName newName = new AssemblyName ();
			newName.Name = "AssemblyTest";

			AssemblyBuilder ab = Thread.GetDomain().DefineDynamicAssembly (newName, AssemblyBuilderAccess.RunAndSave, TempFolder);

			ModuleBuilder mb = ab.DefineDynamicModule ("myDynamicModule1", "myDynamicModule.dll", false);

			ab.Save ("test_assembly.dll");

			File.Delete (Path.Combine (TempFolder, "myDynamicModule.dll"));

			Assembly ass = Assembly.LoadFrom (Path.Combine (TempFolder, "test_assembly.dll"));
			try {
				ass.GetModules ();
				Assert.Fail ();
			} catch (FileNotFoundException ex) {
				Assert.AreEqual ("myDynamicModule.dll", ex.FileName);
			}
		}
#endif

		[Category ("NotWorking")]
		[Test]
		public void Corlib () 
		{
			Assembly corlib = typeof (int).Assembly;
			Assert.IsTrue (corlib.CodeBase.EndsWith ("mscorlib.dll"), "CodeBase");
			Assert.IsNull (corlib.EntryPoint, "EntryPoint");
			Assert.IsTrue (corlib.EscapedCodeBase.EndsWith ("mscorlib.dll"), "EscapedCodeBase");
			Assert.IsNotNull (corlib.Evidence, "Evidence");
			Assert.IsTrue (corlib.Location.EndsWith ("mscorlib.dll"), "Location");

			// corlib doesn't reference anything
			Assert.AreEqual (0, corlib.GetReferencedAssemblies ().Length, "GetReferencedAssemblies");
			Assert.AreEqual ("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", corlib.FullName, "FullName");
			// not really "true" but it's even more trusted so...
			Assert.IsTrue (corlib.GlobalAssemblyCache, "GlobalAssemblyCache");
			Assert.AreEqual (0, corlib.HostContext, "HostContext");
			Assert.AreEqual ("v2.0.50727", corlib.ImageRuntimeVersion, "ImageRuntimeVersion");
			Assert.IsFalse (corlib.ReflectionOnly, "ReflectionOnly");
			Assert.AreEqual (0x1, corlib.ManifestModule.MetadataToken);
		}

		[Test]
		public void Corlib_test ()
		{
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
#if MOBILE
			Assert.IsNull (corlib_test.Evidence, "Evidence");
#else
			Assert.IsNotNull (corlib_test.Evidence, "Evidence");
#endif
			Assert.IsFalse (corlib_test.GlobalAssemblyCache, "GlobalAssemblyCache");

			Assert.IsTrue (corlib_test.GetReferencedAssemblies ().Length > 0, "GetReferencedAssemblies");
			Assert.AreEqual (0, corlib_test.HostContext, "HostContext");
			Assert.AreEqual ("v4.0.30319", corlib_test.ImageRuntimeVersion, "ImageRuntimeVersion");

			Assert.IsNotNull (corlib_test.ManifestModule, "ManifestModule");
			Assert.IsFalse (corlib_test.ReflectionOnly, "ReflectionOnly");
		}

		[Test]
		public void GetAssembly ()
		{
			Assert.IsTrue (Assembly.GetAssembly (typeof (int)).FullName.StartsWith ("mscorlib"), "GetAssembly(int)");
			Assert.AreEqual (this.GetType ().Assembly.FullName, Assembly.GetAssembly (this.GetType ()).FullName, "GetAssembly(this)");
		}

		[Test]
		public void GetFile_Null ()
		{
			try {
				Assembly.GetExecutingAssembly ().GetFile (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFile_Empty ()
		{
			try {
				Assembly.GetExecutingAssembly ().GetFile (
					String.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("AndroidNotWorking")] // Assemblies in Xamarin.Android cannot be accessed as FileStream
		[Category ("StaticLinkedAotNotWorking")] // Can't find .dll files when bundled in .exe
		[Category ("NotWasm")]
		public void GetFiles_False ()
		{
			Assembly corlib = typeof (int).Assembly;
			FileStream[] fss = corlib.GetFiles ();
			Assert.AreEqual (fss.Length, corlib.GetFiles (false).Length, "corlib.GetFiles (false)");

			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			fss = corlib_test.GetFiles ();
			Assert.AreEqual (fss.Length, corlib_test.GetFiles (false).Length, "test.GetFiles (false)");
		}

		[Test]
		[Category ("AndroidNotWorking")] // Assemblies in Xamarin.Android cannot be accessed as FileStream
		[Category ("StaticLinkedAotNotWorking")] // Can't find .dll files when bundled in .exe
		[Category ("NotWasm")]
		public void GetFiles_True ()
		{
			Assembly corlib = typeof (int).Assembly;
			FileStream[] fss = corlib.GetFiles ();
			Assert.IsTrue (fss.Length <= corlib.GetFiles (true).Length, "corlib.GetFiles (true)");

			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			fss = corlib_test.GetFiles ();
			Assert.IsTrue (fss.Length <= corlib_test.GetFiles (true).Length, "test.GetFiles (true)");
		}

		[Test]
		public void GetManifestResourceStream_Name_Empty ()
		{
			Assembly corlib = typeof (int).Assembly;

			try {
				corlib.GetManifestResourceStream (string.Empty);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// String cannot have zero length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			corlib.GetManifestResourceStream (typeof (int), string.Empty);

			try {
				corlib.GetManifestResourceStream ((Type) null, string.Empty);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// String cannot have zero length
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void GetManifestResourceStream_Name_Null ()
		{
			Assembly corlib = typeof (int).Assembly;

			try {
				corlib.GetManifestResourceStream ((string) null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			corlib.GetManifestResourceStream (typeof (int), (string) null);

			try {
				corlib.GetManifestResourceStream ((Type) null, (string) null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("type", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			Assembly corlib = typeof (int).Assembly;

			try {
				corlib.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test] // bug #78517
		public void LoadFrom_Empty_Assembly ()
		{
			string tempFile = Path.Combine (TempFolder, Path.GetRandomFileName ());
			File.CreateText (tempFile).Close ();

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (BadImageFormatException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			}
		}

		[Test] // bug #78517
		public void LoadFrom_Invalid_Assembly ()
		{
			string tempFile = Path.Combine (TempFolder, Path.GetRandomFileName ());
			using (StreamWriter sw = File.CreateText (tempFile)) {
				sw.WriteLine ("foo");
				sw.Close ();
			}

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (BadImageFormatException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			}
		}

		[Test]
		public void LoadFrom_NonExisting_Assembly ()
		{
			string tempFile = Path.Combine (TempFolder, Path.GetRandomFileName ());

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void LoadWithPartialName ()
		{
// FIXME?
// So the problem here is that if we load an assembly
// in a fully aot mode and then cannot load the
// dylib, we will assert. This test is incompatible
// with the semantics of aot'ed assembly loading, as
// aot may assert when loading. This assumes that it's
// safe to greedly load everything.
			var names = new string[] { Assembly.GetCallingAssembly ().GetName ().Name };

			foreach (string s in names)
				if (Assembly.LoadWithPartialName (s) != null)
					return;
			Assert.Fail ("Was not able to load any corlib test");
		}

		[Test]
		public void GetObjectData_Info_Null ()
		{
			Assembly corlib = typeof (int).Assembly;
			try {
				corlib.GetObjectData (null, new StreamingContext (
					StreamingContextStates.All));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("info", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetReferencedAssemblies ()
		{
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			AssemblyName an = corlib_test.GetReferencedAssemblies ().First (l => l.Name == "mscorlib");
			Assert.IsNull (an.CodeBase, "CodeBase");
			Assert.IsNotNull (an.CultureInfo, "CultureInfo");
			Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
			Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
			Assert.IsNotNull (an.FullName, "FullName");
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, an.HashAlgorithm, "HashAlgorithm");
			Assert.IsNull (an.KeyPair, "KeyPair");
			Assert.IsNotNull (an.Name, "Name");
			Assert.IsNotNull (an.Version, "Version");
			Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, an.VersionCompatibility, "VersionCompatibility");
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME // Reflection.Emit is not supported.
		[Test]
		public void Location_Empty() {
			string assemblyFileName = Path.Combine (
				TempFolder, "AssemblyLocation.dll");

			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "AssemblyLocation";

			AssemblyBuilder ab = AppDomain.CurrentDomain
				.DefineDynamicAssembly (assemblyName,
				AssemblyBuilderAccess.Save,
				TempFolder);
			ab.Save (Path.GetFileName (assemblyFileName));

			using (FileStream fs = File.OpenRead (assemblyFileName)) {
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer, 0, buffer.Length);
				Assembly assembly = Assembly.Load (buffer);
				Assert.AreEqual (string.Empty, assembly.Location);
				fs.Close ();
			}
		}

		[Test]
		public void SateliteAssemblyForInMemoryAssembly ()
		{
			string assemblyFileName = Path.Combine (
				TempFolder, "AssemblyLocation1.dll");

			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "AssemblyLocation1";

			AssemblyBuilder ab = AppDomain.CurrentDomain
				.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					TempFolder);

			ModuleBuilder moduleBuilder = ab.DefineDynamicModule (assemblyName.Name, assemblyName.Name + ".dll");
			TypeBuilder typeBuilder = moduleBuilder.DefineType ("Program", TypeAttributes.Public);

			MethodBuilder methodBuilder = typeBuilder.DefineMethod ("TestCall", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
			ILGenerator gen = methodBuilder.GetILGenerator ();

			//
			// 	var resourceManager = new ResourceManager (typeof (Program));
			//	resourceManager.GetString ("test");
			//
			gen.Emit (OpCodes.Ldtoken, typeBuilder);
			gen.Emit (OpCodes.Call, typeof(Type).GetMethod ("GetTypeFromHandle"));
			gen.Emit (OpCodes.Newobj, typeof(ResourceManager).GetConstructor (new Type[] { typeof(Type) }));
			gen.Emit (OpCodes.Ldstr, "test");
			gen.Emit (OpCodes.Callvirt, typeof(ResourceManager).GetMethod ("GetString", new Type[] { typeof(string) }));
			gen.Emit (OpCodes.Pop);
			gen.Emit (OpCodes.Ret);

			typeBuilder.CreateType ();

			ab.Save (Path.GetFileName (assemblyFileName));

			using (FileStream fs = File.OpenRead (assemblyFileName)) {
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer, 0, buffer.Length);
				Assembly assembly = Assembly.Load (buffer);

				var mm = assembly.GetType ("Program").GetMethod ("TestCall");
				try {
					mm.Invoke (null, null);
					Assert.Fail ();
				} catch (TargetInvocationException e) {
					Assert.IsTrue (e.InnerException is MissingManifestResourceException);
				}

				fs.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void bug78464 ()
		{
			string assemblyFileName = Path.Combine (
				TempFolder, "bug78464.dll");

			// execute test in separate appdomain to allow assembly to be unloaded
			AppDomain testDomain = CreateTestDomain (AppDomain.CurrentDomain.BaseDirectory, false);
			CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
			try {
				crossDomainTester.bug78464 (assemblyFileName);
			} finally {
				AppDomain.Unload (testDomain);
			}
		}

		[Test]
		[Category("MobileNotWorking")]
		public void bug78465 ()
		{
			string assemblyFileName = Path.Combine (
				TempFolder, "bug78465.dll");

			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "bug78465";

			AssemblyBuilder ab = AppDomain.CurrentDomain
				.DefineDynamicAssembly (assemblyName,
				AssemblyBuilderAccess.Save,
				Path.GetDirectoryName (assemblyFileName));
			ab.Save (Path.GetFileName (assemblyFileName));

			using (FileStream fs = File.OpenRead (assemblyFileName)) {
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer, 0, buffer.Length);
				Assembly assembly = Assembly.Load (buffer);
				Assert.AreEqual (string.Empty, assembly.Location, "#1");
				fs.Close ();
			}

			AppDomain testDomain = CreateTestDomain (AppDomain.CurrentDomain.BaseDirectory, false);
			CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
			try {
				crossDomainTester.bug78465 (assemblyFileName);
			} finally {
				AppDomain.Unload (testDomain);
			}
		}

		[Test]
		[Category("MobileNotWorking")]
		public void bug78468 ()
		{
			string assemblyFileNameA = Path.Combine (TempFolder,
				"bug78468a.dll");
			string resourceFileName = Path.Combine (TempFolder,
				"readme.txt");

			using (StreamWriter sw = File.CreateText (resourceFileName)) {
				sw.WriteLine ("FOO");
				sw.Close ();
			}

			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "bug78468a";

			AssemblyBuilder ab = AppDomain.CurrentDomain
				.DefineDynamicAssembly (assemblyName,
				AssemblyBuilderAccess.Save,
				TempFolder);
			ab.AddResourceFile ("read", "readme.txt");
			ab.Save (Path.GetFileName (assemblyFileNameA));

			Assembly assembly;

			using (FileStream fs = File.OpenRead (assemblyFileNameA)) {
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer, 0, buffer.Length);
				assembly = Assembly.Load (buffer);
				fs.Close ();
			}

			Assert.AreEqual (string.Empty, assembly.Location, "#A1");
			string[] resNames = assembly.GetManifestResourceNames ();
			Assert.IsNotNull (resNames, "#A2");
			Assert.AreEqual (1, resNames.Length, "#A3");
			Assert.AreEqual ("read", resNames[0], "#A4");
			ManifestResourceInfo resInfo = assembly.GetManifestResourceInfo ("read");
			Assert.IsNotNull (resInfo, "#A5");
			Assert.AreEqual ("readme.txt", resInfo.FileName, "#A6");
			Assert.IsNull (resInfo.ReferencedAssembly, "#A7");
			Assert.AreEqual ((ResourceLocation) 0, resInfo.ResourceLocation, "#A8");
			try {
				assembly.GetManifestResourceStream ("read");
				Assert.Fail ("#A9");
			} catch (FileNotFoundException) {
			}
			try {
				assembly.GetFile ("readme.txt");
				Assert.Fail ("#A10");
			} catch (FileNotFoundException) {
			}

			string assemblyFileNameB = Path.Combine (TempFolder,
				"bug78468b.dll");

			AppDomain testDomain = CreateTestDomain (AppDomain.CurrentDomain.BaseDirectory, false);
			CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
			try {
				crossDomainTester.bug78468 (assemblyFileNameB);
			} finally {
				AppDomain.Unload (testDomain);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ReflectionOnlyLoad ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (AssemblyTest).Assembly.FullName);
			
			Assert.IsNotNull (assembly);
			Assert.IsTrue (assembly.ReflectionOnly);
		}

		[Test]
		[Category ("AndroidNotWorking")] // Xamarin.Android assemblies are bundled so they don't exist in the file system.
		public void ReflectionOnlyLoadFrom ()
		{
			string loc = typeof (AssemblyTest).Assembly.Location;
			string filename = Path.GetFileName (loc);
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom (loc);

			Assert.IsNotNull (assembly);
			Assert.IsTrue (assembly.ReflectionOnly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateInstanceOnRefOnly ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (AssemblyTest).Assembly.FullName);
			assembly.CreateInstance ("MonoTests.System.Reflection.AssemblyTest");
		}

		[Test]
		[Category ("NotWorking")] // patch for bug #79720 must be committed first
		public void Load_Culture ()
		{
			string tempDir = TempFolder;
			string cultureTempDir = Path.Combine (tempDir, "nl-BE");
			if (!Directory.Exists (cultureTempDir))
				Directory.CreateDirectory (cultureTempDir);
			cultureTempDir = Path.Combine (tempDir, "en-US");
			if (!Directory.Exists (cultureTempDir))
				Directory.CreateDirectory (cultureTempDir);


			AppDomain ad = CreateTestDomain (tempDir, true);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);

				// PART A

				AssemblyName aname = new AssemblyName ();
				aname.Name = "culturea";
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "culturea.dll"));

				aname = new AssemblyName ();
				aname.Name = "culturea";
				Assert.IsTrue (cdt.AssertLoad(aname), "#A1");

				aname = new AssemblyName ();
				aname.Name = "culturea";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#A2");

				aname = new AssemblyName ();
				aname.Name = "culturea";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertLoad(aname), "#A3");

				// PART B

				aname = new AssemblyName ();
				aname.Name = "cultureb";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "cultureb.dll"));

				aname = new AssemblyName ();
				aname.Name = "cultureb";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#B1");

				aname = new AssemblyName ();
				aname.Name = "cultureb";
				Assert.IsTrue (cdt.AssertLoad (aname), "#B2");

				aname = new AssemblyName ();
				aname.Name = "cultureb";
				aname.CultureInfo = new CultureInfo ("en-US");
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#B3");

				// PART C

				aname = new AssemblyName ();
				aname.Name = "culturec";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "nl-BE/culturec.dll"));

				aname = new AssemblyName ();
				aname.Name = "culturec";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertLoad (aname), "#C1");

				aname = new AssemblyName ();
				aname.Name = "culturec";
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#C2");

				aname = new AssemblyName ();
				aname.Name = "culturec";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#C3");

				// PART D

				aname = new AssemblyName ();
				aname.Name = "cultured";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "en-US/cultured.dll"));

				aname = new AssemblyName ();
				aname.Name = "cultured";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#D1");

				aname = new AssemblyName ();
				aname.Name = "cultured";
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#D2");

				aname = new AssemblyName ();
				aname.Name = "cultured";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#D3");
			} finally {
				AppDomain.Unload (ad);
			}
		}

		[Test] // bug #79712
		[Category ("NotWorking")] // in non-default domain, MS throws FileNotFoundException
		public void Load_Culture_Mismatch ()
		{
			string tempDir = TempFolder;
			string cultureTempDir = Path.Combine (tempDir, "en-US");
			if (!Directory.Exists (cultureTempDir))
				Directory.CreateDirectory (cultureTempDir);

			AppDomain ad = CreateTestDomain (tempDir, true);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);

				// PART A

				AssemblyName aname = new AssemblyName ();
				aname.Name = "bug79712a";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "bug79712a.dll"));

				aname = new AssemblyName ();
				aname.Name = "bug79712a";
				aname.CultureInfo = CultureInfo.InvariantCulture;
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#A1");

				// PART B

				aname = new AssemblyName ();
				aname.Name = "bug79712b";
				aname.CultureInfo = new CultureInfo ("nl-BE");
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "en-US/bug79712b.dll"));

				aname = new AssemblyName ();
				aname.Name = "bug79712b";
				aname.CultureInfo = new CultureInfo ("en-US");
				Assert.IsTrue (cdt.AssertFileNotFoundException (aname), "#B1");
			} finally {
				AppDomain.Unload (ad);
			}
		}


		[Test] // bug #79715
		[Category("MobileNotWorking")]
		public void Load_PartialVersion ()
		{
			string tempDir = TempFolder;

			AppDomain ad = CreateTestDomain (tempDir, true);
			try {
				CrossDomainTester cdt = CreateCrossDomainTester (ad);

				AssemblyName aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3, 4);
				cdt.GenerateAssembly (aname, Path.Combine (tempDir, "bug79715.dll"));

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2);
				Assert.IsTrue (cdt.AssertLoad (aname), "#A1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#A2");

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3);
				Assert.IsTrue (cdt.AssertLoad (aname), "#B1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#B2");

				aname = new AssemblyName ();
				aname.Name = "bug79715";
				aname.Version = new Version (1, 2, 3, 4);
				Assert.IsTrue (cdt.AssertLoad (aname), "#C1");
				Assert.IsTrue (cdt.AssertLoad (aname.FullName), "#C2");
			} finally {
				AppDomain.Unload (ad);
			}
		}

		private static AppDomain CreateTestDomain (string baseDirectory, bool assemblyResolver)
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = baseDirectory;
			setup.ApplicationName = "testdomain";

			AppDomain ad = AppDomain.CreateDomain ("testdomain", null, setup);

			if (assemblyResolver) {
				Assembly ea = Assembly.GetExecutingAssembly ();
				ad.CreateInstanceFrom (ea.CodeBase,
					typeof (AssemblyResolveHandler).FullName,
					false,
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new object [] { ea.Location, ea.FullName },
					CultureInfo.InvariantCulture,
					null,
					null);
			}

			return ad;
		}

		private static CrossDomainTester CreateCrossDomainTester (AppDomain domain)
		{
			Type testerType = typeof (CrossDomainTester);
			return (CrossDomainTester) domain.CreateInstanceAndUnwrap (
				testerType.Assembly.FullName, testerType.FullName, false,
				BindingFlags.Public | BindingFlags.Instance, null, new object[0],
				CultureInfo.InvariantCulture, new object[0], null);
		}

		private class CrossDomainTester : MarshalByRefObject
		{
			public void GenerateAssembly (AssemblyName aname, string path)
			{
				AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
					aname, AssemblyBuilderAccess.Save, Path.GetDirectoryName (path));
				ab.Save (Path.GetFileName (path));
			}

			public void Load (AssemblyName assemblyRef)
			{
				Assembly.Load (assemblyRef);
			}

			public void LoadFrom (string assemblyFile)
			{
				Assembly.LoadFrom (assemblyFile);
			}

			public bool AssertLoad (AssemblyName assemblyRef)
			{
				try {
					Assembly.Load (assemblyRef);
					return true;
				} catch {
					return false;
				}
			}

			public bool AssertLoad (string assemblyString)
			{
				try {
					Assembly.Load (assemblyString);
					return true;
				} catch {
					return false;
				}
			}

			public bool AssertFileLoadException (AssemblyName assemblyRef)
			{
				try {
					Assembly.Load (assemblyRef);
					return false;
				} catch (FileLoadException) {
					return true;
				}
			}

			public bool AssertFileNotFoundException (AssemblyName assemblyRef)
			{
				try {
					Assembly.Load (assemblyRef);
					return false;
				} catch (FileNotFoundException) {
					return true;
				}
			}

			public void bug78464 (string assemblyFileName)
			{
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78464";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetDirectoryName (assemblyFileName));
				ab.Save (Path.GetFileName (assemblyFileName));

				Assembly assembly;

				using (FileStream fs = File.OpenRead (assemblyFileName)) {
					byte[] buffer = new byte[fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					assembly = Assembly.Load (buffer);
					fs.Close ();
				}

				Assert.AreEqual (string.Empty, assembly.Location, "#1");

				assembly = Assembly.LoadFrom (assemblyFileName);
				Assert.IsFalse (assembly.Location == string.Empty, "#2");
				Assert.AreEqual (Path.GetFileName (assemblyFileName), Path.GetFileName(assembly.Location), "#3");
				// note: we cannot check if directory names match, as MS.NET seems to 
				// convert directory part of assembly location to lowercase
				Assert.IsFalse (Path.GetDirectoryName(assembly.Location) == string.Empty, "#4");
			}

			public void bug78465 (string assemblyFileName)
			{
				Assembly assembly = Assembly.LoadFrom (assemblyFileName);
				Assert.IsFalse (assembly.Location == string.Empty, "#2");
				Assert.AreEqual (Path.GetFileName (assemblyFileName), Path.GetFileName (assembly.Location), "#3");
				// note: we cannot check if directory names match, as MS.NET seems to 
				// convert directory part of assembly location to lowercase
				Assert.IsFalse (Path.GetDirectoryName (assembly.Location) == string.Empty, "#4");
			}

			public void bug78468 (string assemblyFileName)
			{
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78468b";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetDirectoryName (assemblyFileName));
				ab.AddResourceFile ("read", "readme.txt");
				ab.Save (Path.GetFileName (assemblyFileName));

				Assembly assembly = Assembly.LoadFrom (assemblyFileName);
				Assert.IsTrue (assembly.Location != string.Empty, "#B1");
				string[] resNames = assembly.GetManifestResourceNames ();
				Assert.IsNotNull (resNames, "#B2");
				Assert.AreEqual (1, resNames.Length, "#B3");
				Assert.AreEqual ("read", resNames[0], "#B4");
				ManifestResourceInfo resInfo = assembly.GetManifestResourceInfo ("read");
				Assert.IsNotNull (resInfo, "#B5");
				Assert.AreEqual ("readme.txt", resInfo.FileName, "#B6");
				Assert.IsNull (resInfo.ReferencedAssembly, "#B7");
				Assert.AreEqual ((ResourceLocation) 0, resInfo.ResourceLocation, "#B8");
				Stream s = assembly.GetManifestResourceStream ("read");
				Assert.IsNotNull (s, "#B9");
				s.Close ();
				s = assembly.GetFile ("readme.txt");
				Assert.IsNotNull (s, "#B10");
				s.Close ();
			}
		}

		[Test]
		public void bug79872 ()
		{
			string outdir = TempFolder;

			AssemblyName an = new AssemblyName ();
			an.Name = "bug79872";
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Save, outdir);
			string dllname = "bug79872.dll";
			ModuleBuilder mb1 = ab.DefineDynamicModule ("bug79872", dllname);
			string netmodule = "bug79872.netmodule";
			ModuleBuilder mb2 = ab.DefineDynamicModule (netmodule, netmodule);
			TypeBuilder a1 = mb2.DefineType ("A");
			a1.CreateType ();
			ab.Save (dllname);

			bool ok = true;
			try {
				Assembly.LoadFrom (Path.Combine (outdir, dllname));
			} catch {
				ok = false;
			}
			Assert.IsTrue (ok, "Should load a .NET metadata file with an assembly manifest");

			ok = false;
			try {
				Assembly.LoadFrom (Path.Combine (outdir, netmodule));
			} catch (BadImageFormatException) {
				ok = true; // mono and .net 2.0 throw this
			} catch (FileLoadException) {
				ok = true; // .net 1.1 throws this
			} catch {
				// swallow the rest
			}
			Assert.IsTrue (ok, "Complain on loading a .NET metadata file without an assembly manifest");
		}
#endif

		[Test]
		public void ManifestModule ()
		{
			Assembly assembly = typeof (int).Assembly;
			Module module = assembly.ManifestModule;
			Assert.IsNotNull (module, "#1");

			Assert.AreEqual ("RuntimeModule", module.GetType ().Name, "#2");

#if !MONOTOUCH && !FULL_AOT_RUNTIME
			Assert.AreEqual ("mscorlib.dll", module.Name, "#3");
#endif
			Assert.IsFalse (module.IsResource (), "#4");
			Assert.IsTrue (assembly.GetModules ().Length > 0, "#5");
			Assert.AreSame (module, assembly.GetModules () [0], "#6");
			Assert.AreSame (module, assembly.ManifestModule, "#7");
		}


		[Serializable ()]
		private class AssemblyResolveHandler
		{
			public AssemblyResolveHandler (string assemblyFile, string assemblyName)
			{
				_assemblyFile = assemblyFile;
				_assemblyName = assemblyName;

				AppDomain.CurrentDomain.AssemblyResolve +=
					new ResolveEventHandler (ResolveAssembly);
			}

			private Assembly ResolveAssembly (Object sender, ResolveEventArgs args)
			{
				if (args.Name == _assemblyName)
					return Assembly.LoadFrom (_assemblyFile);

				return null;
			}

			private readonly string _assemblyFile;
			private readonly string _assemblyName;
		}

		protected internal class Bug328812_NestedFamORAssem { };

		[Test]
		public void bug328812 ()
		{
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			Assert.IsNull (corlib_test.GetType ("Bug328812_NestedFamORAssem"));
			// Just a sanity check, in case the above passed for some other reason
			Assert.IsNotNull (corlib_test.GetType ("MonoTests.System.Reflection.AssemblyTest+Bug328812_NestedFamORAssem"));
		}
		
		[Test]
		public void GetCustomAttributes_AttributeType_Null ()
		{
			Assembly a = typeof (int).Assembly;
			try {
				a.GetCustomAttributes (null, false);
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
		public void GetTypeWithEmptyStringShouldThrow ()
		{
			try {
				typeof (string).Assembly.GetType ("");
				Assert.Fail ("#1");
			} catch (ArgumentException) {}
		}

		class GetCallingAssemblyCallee {
			static int _dummy;

			static void sideEffect () {
				_dummy++;
			}

			// GetCallingAssembly may see an unpredictable
			// view of the stack if it's called in tail
			// position, or if its caller or the caller's
			// caller is inlined.  So we put in a side
			// effect to get out of tail position, and we
			// tag the methods NoInlining to discourage
			// the inliner.
			[MethodImplAttribute (MethodImplOptions.NoInlining)]
			public static Assembly Leaf () {
				var a = Assembly.GetCallingAssembly ();
				sideEffect();
				return a;
			}

			[MethodImplAttribute (MethodImplOptions.NoInlining)]
			public static Assembly DirectCall () {
				var a = Leaf();
				sideEffect();
				return a;
			}

			[MethodImplAttribute (MethodImplOptions.NoInlining)]
			public static Assembly InvokeCall () {
				var ty = typeof (GetCallingAssemblyCallee);
				var mi = ty.GetMethod("Leaf");
				var o = mi.Invoke(null, null);
				sideEffect();
				return (Assembly)o;
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void GetCallingAssembly_Direct() {
			var a = GetCallingAssemblyCallee.DirectCall ();
			Assert.IsNotNull (a);

			Assert.AreEqual (GetType().Assembly, a);
		}

		[Test]
		[Category ("StackWalks")]
		public void GetCallingAssembly_SkipsReflection () {
			// check that the calling assembly is this
			// one, not mscorlib (aka, the reflection
			// API).
			var a = GetCallingAssemblyCallee.InvokeCall ();
			Assert.IsNotNull (a);

			var invokeAssembly =
				typeof (MethodInfo).Assembly;
			Assert.AreNotEqual (invokeAssembly, a);

			Assert.AreEqual (GetType().Assembly, a);
		}

		[Test]
		public void DefinedTypes_Equality ()
		{
			var x1 = Assembly.GetExecutingAssembly ().DefinedTypes.Where (l => l.FullName == "MonoTests.System.Reflection.TestDefinedTypes").Single ();
			var x2 = Assembly.GetExecutingAssembly ().GetTypes ().Where (l => l.FullName == "MonoTests.System.Reflection.TestDefinedTypes").Single ();

			Assert.AreSame (x1, x2, "#1");
		}

		class MyAssembly : Assembly { }

		[Test]
		public void CustomAssemblyImplThrows ()
		{
			var ma = new MyAssembly();
			try {
				ma.GetName ();
				Assert.Fail ("must throw");
			} catch (NotImplementedException){
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LoadFileRelativeThrows ()
		{
			Assembly.LoadFile (Path.Combine ("non-existent", "relative", "path.dll"));
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void LoadFileAbsoluteNotFoundThrows ()
		{
			// have to use GetFullPath so we get C:\... on Windows
			var abspath = Path.GetFullPath (Path.Combine ("non-existent", "absolute", "path.dll"));
			Assembly.LoadFile (abspath);
		}
	}

	public class TestDefinedTypes
	{
	}
}
