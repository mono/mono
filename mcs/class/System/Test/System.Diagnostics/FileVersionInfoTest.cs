//
// FileVersionInfoTest.cs - NUnit Test Cases for System.Diagnostics.FileVersionInfo
//
// Authors:
//   Gert Driesen <drieseng@users.sourceforge.net>
//
// (c) 2008 Gert Driesen
// 

using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
#if !MONOTOUCH
using System.Reflection.Emit;
#endif
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class FileVersionInfoTest
	{
		private string tempDir;

		[SetUp]
		public void SetUp ()
		{
			tempDir = Path.Combine (Path.GetTempPath (), Environment.UserName);
			tempDir = Path.Combine (tempDir, "MonoTests.System.Diagnostics.AppDomainTest");
			if (!Directory.Exists (tempDir))
				Directory.CreateDirectory (tempDir);
		}

		[TearDown]
		public void TearDown ()
		{
			Directory.Delete (tempDir, true);
		}

		[Test]
		public void GetVersionInfo_FileName_DoesNotExist ()
		{
			try {
				FileVersionInfo.GetVersionInfo ("shouldnoteverexist.tmp");
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.AreEqual (typeof (FileNotFoundException), ex.GetType (), "#2");
				Assert.IsNull (ex.FileName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.AreEqual ("shouldnoteverexist.tmp", ex.Message, "#5");
			}
		}

		[Test]
		public void GetVersionInfo_FileName_Null ()
		{
			try {
				FileVersionInfo.GetVersionInfo ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetVersionInfo_TextFile ()
		{
			string file = Path.Combine (tempDir, "lib.dll");

			using (StreamWriter sw = new StreamWriter (file, false, Encoding.UTF8)) {
				sw.WriteLine ("WHATEVER");
			}

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (file);
#if NET_2_0
			Assert.IsNull (fvi.Comments, "#1");
			Assert.IsNull (fvi.CompanyName, "#2");
#else
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
#endif
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
#if NET_2_0
			Assert.IsNull (fvi.FileDescription, "#4");
#else
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
#endif
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (file, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
#if NET_2_0
			Assert.IsNull (fvi.FileVersion, "#9");
			Assert.IsNull (fvi.InternalName, "#10");
#else
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
#endif
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
#if NET_2_0
			Assert.IsNull (fvi.Language, "#16");
			Assert.IsNull (fvi.LegalCopyright, "#17");
			Assert.IsNull (fvi.LegalTrademarks, "#18");
			Assert.IsNull (fvi.OriginalFilename, "#19");
			Assert.IsNull (fvi.PrivateBuild, "#20");
#else
			Assert.AreEqual (string.Empty, fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
#endif
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
#if NET_2_0
			Assert.IsNull (fvi.ProductName, "#24");
#else
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
#endif
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
#if NET_2_0
			Assert.IsNull (fvi.ProductVersion, "#26");
			Assert.IsNull (fvi.SpecialBuild, "#27");
#else
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
#endif
		}

#if !MONOTOUCH
		[Test]
		public void GetVersionInfo_NoNativeResources ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.Save ("lib.dll");

			string assemblyFile = Path.Combine (tempDir, "lib.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
#if NET_2_0
			Assert.IsNull (fvi.Comments, "#1");
			Assert.IsNull (fvi.CompanyName, "#2");
#else
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
#endif
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
#if NET_2_0
			Assert.IsNull (fvi.FileDescription, "#4");
#else
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
#endif
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
#if NET_2_0
			Assert.IsNull (fvi.FileVersion, "#9");
			Assert.IsNull (fvi.InternalName, "#10");
#else
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
#endif
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
#if NET_2_0
			Assert.IsNull (fvi.Language, "#16");
			Assert.IsNull (fvi.LegalCopyright, "#17");
			Assert.IsNull (fvi.LegalTrademarks, "#18");
			Assert.IsNull (fvi.OriginalFilename, "#19");
			Assert.IsNull (fvi.PrivateBuild, "#20");
#else
			Assert.AreEqual (string.Empty, fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
#endif
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
#if NET_2_0
			Assert.IsNull (fvi.ProductName, "#24");
#else
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
#endif
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
#if NET_2_0
			Assert.IsNull (fvi.ProductVersion, "#26");
			Assert.IsNull (fvi.SpecialBuild, "#27");
#else
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
#endif
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1a ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res1, 0, version_res1.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3a";
			aname.Version = new Version (8, 5, 4, 2);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3a.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3a.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("N Comment", fvi.Comments, "#1");
			Assert.AreEqual ("N Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual ("N File Description", fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("N 1.2.3.4", fvi.FileVersion, "#9");
			Assert.AreEqual ("N lib3", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsTrue (fvi.IsPrivateBuild, "#14");
			Assert.IsTrue (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual ("N Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("N Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("N lib3.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual ("N PRIV", fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("N Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("N 4,2,1,7", fvi.ProductVersion, "#26");
			Assert.AreEqual ("N SPEC", fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1b ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res1, 0, version_res1.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3b";
			aname.Version = new Version (9, 0, 3, 0);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3b.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3b.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("N Comment", fvi.Comments, "#1");
			Assert.AreEqual ("N Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual ("N File Description", fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("N 1.2.3.4", fvi.FileVersion, "#9");
			Assert.AreEqual ("N lib3", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsTrue (fvi.IsPrivateBuild, "#14");
			Assert.IsTrue (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual ("N Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("N Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("N lib3.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual ("N PRIV", fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("N Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("N 4,2,1,7", fvi.ProductVersion, "#26");
			Assert.AreEqual ("N SPEC", fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1c ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res2, 0, version_res2.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3c";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3c.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3c.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1d ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res2, 0, version_res2.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3d";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3d.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3d.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1e ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res3, 0, version_res3.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3e";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3e.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3e.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United States)", fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1f ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res3, 0, version_res3.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3f";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3f.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3f.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
			Assert.AreEqual (1, fvi.FileBuildPart, "#3");
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
			Assert.AreEqual (6, fvi.FileMajorPart, "#5");
			Assert.AreEqual (9, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (3, fvi.FilePrivatePart, "#8");
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United States)", fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (9, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (8, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
			Assert.AreEqual (6, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1g ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res4, 0, version_res4.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3g";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3g.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3g.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
#if NET_2_0
			Assert.IsNull (fvi.Comments, "#1");
			Assert.IsNull (fvi.CompanyName, "#2");
#else
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
#endif
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
#if NET_2_0
			Assert.IsNull (fvi.FileDescription, "#4");
#else
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
#endif
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
#if NET_2_0
			Assert.IsNull (fvi.FileVersion, "#9");
			Assert.IsNull (fvi.InternalName, "#10");
#else
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
#endif
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
#if NET_2_0
			Assert.IsNull (fvi.Language, "#16");
			Assert.IsNull (fvi.LegalCopyright, "#17");
			Assert.IsNull (fvi.LegalTrademarks, "#18");
			Assert.IsNull (fvi.OriginalFilename, "#19");
			Assert.IsNull (fvi.PrivateBuild, "#20");
#else
			Assert.AreEqual (string.Empty, fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
#endif
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
#if NET_2_0
			Assert.IsNull (fvi.ProductName, "#24");
#else
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
#endif
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
#if NET_2_0
			Assert.IsNull (fvi.ProductVersion, "#26");
			Assert.IsNull (fvi.SpecialBuild, "#27");
#else
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
#endif
		}

		[Test] // DefineUnmanagedResource (String)
		public void DefineUnmanagedResource1h ()
		{
			string resFile = Path.Combine (tempDir, "version.res");

			using (FileStream fs = File.OpenWrite (resFile)) {
				fs.Write (version_res4, 0, version_res4.Length);
			}

			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib3h";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave, tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineUnmanagedResource (resFile);
			ab.Save ("lib3h.dll");

			string assemblyFile = Path.Combine (tempDir, "lib3h.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
#if NET_2_0
			Assert.IsNull (fvi.Comments, "#1");
			Assert.IsNull (fvi.CompanyName, "#2");
#else
			Assert.AreEqual (string.Empty, fvi.Comments, "#1");
			Assert.AreEqual (string.Empty, fvi.CompanyName, "#2");
#endif
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
#if NET_2_0
			Assert.IsNull (fvi.FileDescription, "#4");
#else
			Assert.AreEqual (string.Empty, fvi.FileDescription, "#4");
#endif
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
#if NET_2_0
			Assert.IsNull (fvi.FileVersion, "#9");
			Assert.IsNull (fvi.InternalName, "#10");
#else
			Assert.AreEqual (string.Empty, fvi.FileVersion, "#9");
			Assert.AreEqual (string.Empty, fvi.InternalName, "#10");
#endif
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
#if NET_2_0
			Assert.IsNull (fvi.Language, "#16");
			Assert.IsNull (fvi.LegalCopyright, "#17");
			Assert.IsNull (fvi.LegalTrademarks, "#18");
			Assert.IsNull (fvi.OriginalFilename, "#19");
			Assert.IsNull (fvi.PrivateBuild, "#20");
#else
			Assert.AreEqual (string.Empty, fvi.Language, "#16");
			Assert.AreEqual (string.Empty, fvi.LegalCopyright, "#17");
			Assert.AreEqual (string.Empty, fvi.LegalTrademarks, "#18");
			Assert.AreEqual (string.Empty, fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
#endif
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
#if NET_2_0
			Assert.IsNull (fvi.ProductName, "#24");
#else
			Assert.AreEqual (string.Empty, fvi.ProductName, "#24");
#endif
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
#if NET_2_0
			Assert.IsNull (fvi.ProductVersion, "#26");
			Assert.IsNull (fvi.SpecialBuild, "#27");
#else
			Assert.AreEqual (string.Empty, fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
#endif
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1a ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1a";

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource ("BBB", "1.3.2.4", "CCC", "DDD", "EEE");
			ab.Save ("lib1a.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1a.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual ("CCC", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1a", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual ("DDD", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("EEE", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1a.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (2, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (1, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (3, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("BBB", fvi.ProductName, "#24");
			Assert.AreEqual (4, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("1.3.2.4", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1b ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1b";

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource (null, null, null, null, null);
			ab.Save ("lib1b.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1b.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1b", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1b.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1c ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1c";

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ("AAA", "3.9.2", "BBB", "CCC", "DDD");
			ab.Save ("lib1c.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1c.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("BBB", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1c", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("CCC", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("DDD", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1c.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (2, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (3, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (9, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("AAA", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("3.9.2", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1d ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1d";

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource (null, null, null, null, null);
			ab.Save ("lib1d.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1d.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1d", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1d.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1e ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib1e";
			aname.Version = new Version (5, 4, 7, 8);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource ("BBB", "1.3.2.4", "CCC", "DDD", "EEE");
			ab.Save ("lib1e.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1e.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual ("CCC", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1e", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Belgium)", fvi.Language, "#16");
			Assert.AreEqual ("DDD", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("EEE", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1e.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (2, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (1, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (3, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("BBB", fvi.ProductName, "#24");
			Assert.AreEqual (4, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("1.3.2.4", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1f ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl");
			aname.Name = "lib1f";
			aname.Version = new Version (5, 4, 7, 8);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource (null, null, null, null, null);
			ab.Save ("lib1f.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1f.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1f", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Netherlands)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1f.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1g ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib1g";
			aname.Version = new Version (5, 4, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ("AAA", "3.9.2", "BBB", "CCC", "DDD");
			ab.Save ("lib1g.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1g.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("BBB", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
#if NET_2_0
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.0", fvi.FileVersion, "#9");
#else
			Assert.AreEqual (65535, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.65535", fvi.FileVersion, "#9");
#endif
			Assert.AreEqual ("lib1g", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("CCC", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("DDD", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1g.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (2, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (3, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (9, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("AAA", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("3.9.2", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1h ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib1h";
			aname.Version = new Version (5, 4);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource (null, null, null, null, null);
			ab.Save ("lib1h.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1h.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
#if NET_2_0
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
#else
			Assert.AreEqual (65535, fvi.FileBuildPart, "#3");
#endif
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
#if NET_2_0
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.0.0", fvi.FileVersion, "#9");
#else
			Assert.AreEqual (65535, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.65535.65535", fvi.FileVersion, "#9");
#endif
			Assert.AreEqual ("lib1h", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1h.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1i ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1i";
			aname.Version = new Version (5, 4, 8, 2);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ("AAA", string.Empty,
				"BBB", "CCC", "DDD");
			ab.Save ("lib1i.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1i.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("BBB", fvi.CompanyName, "#2");
			Assert.AreEqual (8, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (2, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.8.2", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1i", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual ("CCC", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("DDD", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1i.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("AAA", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1j ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib1j";
			aname.Version = new Version (5, 4, 8, 2);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource (string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
			ab.Save ("lib1j.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1j.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (8, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (2, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.8.2", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1j", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1j.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1k ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl");
			aname.Name = "lib1k";
			aname.Version = new Version (5, 4, 7, 8);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// AssemblyCulture
			Type attrType = typeof (AssemblyCultureAttribute);
			ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
			CustomAttributeBuilder cab = new CustomAttributeBuilder (
				ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource (null, null, null, null, null);
			ab.Save ("lib1k.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1k.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1k", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1k.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1l ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib1l";
			aname.Version = new Version (5, 4, 7, 8);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource ("AAA", "3.9.2", "BBB", "CCC", "DDD");

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.Save ("lib1l.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1l.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("BBB", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1l", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("CCC", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("DDD", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1l.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (2, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (3, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (9, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("AAA", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("3.9.2", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource (String, String, String, String, String)
		public void DefineVersionInfoResource1m ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib1m";
			aname.Version = new Version (5, 4, 7, 8);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource (string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.Save ("lib1m.dll");

			string assemblyFile = Path.Combine (tempDir, "lib1m.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (5, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("5.4.7.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib1m", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib1m.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2a ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib2a";

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource ();
			ab.Save ("lib2a.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2a.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2a", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2a.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2b ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib2b";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6.8" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7.1" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2b.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2b.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (6, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (2, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (8, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("2.4.6.8", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2b", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2b.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (6, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (4, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (1, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("6.4.7.1", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2c ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib2c";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// AssemblyVersion
			Type attrType = typeof (AssemblyVersionAttribute);
			ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2c.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2c.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (3, fvi.FileMajorPart, "#5");
			Assert.AreEqual (5, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (9, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("3.5.7.9", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2c", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2c.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2d ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib2d";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// AssemblyVersion
			Type attrType = typeof (AssemblyVersionAttribute);
			ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2d.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2d.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (6, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (2, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("2.4.6", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2d", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2d.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2e ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.Name = "lib2e";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// AssemblyVersion
			Type attrType = typeof (AssemblyVersionAttribute);
			ConstructorInfo ci = attrType.GetConstructor (new Type [] { typeof (String) });
			CustomAttributeBuilder cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "0.0.0.0" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2e.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2e.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("0.0.0.0", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2e", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2e.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2f ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2f";
			aname.Version = new Version (3, 5, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2f.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2f.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (7, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (3, fvi.FileMajorPart, "#5");
			Assert.AreEqual (5, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
#if NET_2_0
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("3.5.7.0", fvi.FileVersion, "#9");
#else
			Assert.AreEqual (65535, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("3.5.7.65535", fvi.FileVersion, "#9");
#endif
			Assert.AreEqual ("lib2f", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Belgium)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2f.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2g ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2g";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2b.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2b.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (6, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (2, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("2.4.6", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2b", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2b.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (6, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (4, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("6.4.7", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2h ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2h";
			aname.Version = new Version (3, 5, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2h.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2h.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (6, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (2, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("2.4.6", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2h", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Belgium)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2h.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (6, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (4, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("6.4.7", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2i ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2i";
			aname.Version = new Version (3, 5, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { string.Empty });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2i.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2i.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual (" ", fvi.Comments, "#1");
			Assert.AreEqual (" ", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual (" ", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2i", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			Assert.AreEqual ("Invariant Language (Invariant Country)", fvi.Language, "#16");
			Assert.AreEqual (" ", fvi.LegalCopyright, "#17");
			Assert.AreEqual (" ", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2i.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual (" ", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual (" ", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2j ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2j";
			aname.Version = new Version (3, 5, 7, 9);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);
			ab.DefineVersionInfoResource ();

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "2.4.6" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "6.4.7" });
			ab.SetCustomAttribute (cab);

			// AssemblyCulture
			attrType = typeof (AssemblyCultureAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "en-GB" });
			ab.SetCustomAttribute (cab);

			ab.Save ("lib2j.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2j.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (6, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (2, fvi.FileMajorPart, "#5");
			Assert.AreEqual (4, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("2.4.6", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2j", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("English (United Kingdom)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2j.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (7, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (6, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (4, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("6.4.7", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2k ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2k";
			aname.Version = new Version (3, 5, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "abc" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "def" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2k.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2k.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (0, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("abc", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2k", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Belgium)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2k.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("def", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		[Test] // DefineVersionInfoResource ()
		public void DefineVersionInfoResource2l ()
		{
			AssemblyName aname = new AssemblyName ();
			aname.CultureInfo = new CultureInfo ("nl-BE");
			aname.Name = "lib2l";
			aname.Version = new Version (3, 5, 7);

			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (
				aname, AssemblyBuilderAccess.RunAndSave,
				tempDir);

			// CompanyName
			Type attrType = typeof (AssemblyCompanyAttribute);
			ConstructorInfo ci = attrType.GetConstructor (
				new Type [] { typeof (String) });
			CustomAttributeBuilder cab =
				new CustomAttributeBuilder (ci, new object [1] { "Mono Team" });
			ab.SetCustomAttribute (cab);

			// Comments
			attrType = typeof (AssemblyDescriptionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "System Test" });
			ab.SetCustomAttribute (cab);

			// ProductName
			attrType = typeof (AssemblyProductAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Mono Runtime" });
			ab.SetCustomAttribute (cab);

			// LegalCopyright
			attrType = typeof (AssemblyCopyrightAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Copyright 2007 Mono Hackers" });
			ab.SetCustomAttribute (cab);

			// LegalTrademarks
			attrType = typeof (AssemblyTrademarkAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "Registered to All" });
			ab.SetCustomAttribute (cab);

			// AssemblyVersion
			attrType = typeof (AssemblyVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.2.3.4" });
			ab.SetCustomAttribute (cab);

			// AssemblyFileVersion
			attrType = typeof (AssemblyFileVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "1.b.3.c" });
			ab.SetCustomAttribute (cab);

			// AssemblyInformationalVersion
			attrType = typeof (AssemblyInformationalVersionAttribute);
			ci = attrType.GetConstructor (new Type [] { typeof (String) });
			cab = new CustomAttributeBuilder (ci, new object [1] { "b.3.6.c" });
			ab.SetCustomAttribute (cab);

			ab.DefineVersionInfoResource ();
			ab.Save ("lib2l.dll");

			string assemblyFile = Path.Combine (tempDir, "lib2l.dll");

			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assemblyFile);
			Assert.AreEqual ("System Test", fvi.Comments, "#1");
			Assert.AreEqual ("Mono Team", fvi.CompanyName, "#2");
			Assert.AreEqual (0, fvi.FileBuildPart, "#3");
			Assert.AreEqual (" ", fvi.FileDescription, "#4");
			Assert.AreEqual (1, fvi.FileMajorPart, "#5");
			Assert.AreEqual (0, fvi.FileMinorPart, "#6");
			Assert.AreEqual (assemblyFile, fvi.FileName, "#7");
			Assert.AreEqual (0, fvi.FilePrivatePart, "#8");
			Assert.AreEqual ("1.b.3.c", fvi.FileVersion, "#9");
			Assert.AreEqual ("lib2l", fvi.InternalName, "#10");
			Assert.IsFalse (fvi.IsDebug, "#11");
			Assert.IsFalse (fvi.IsPatched, "#12");
			Assert.IsFalse (fvi.IsPreRelease, "#13");
			Assert.IsFalse (fvi.IsPrivateBuild, "#14");
			Assert.IsFalse (fvi.IsSpecialBuild, "#15");
			//Assert.AreEqual ("Dutch (Belgium)", fvi.Language, "#16");
			Assert.AreEqual ("Copyright 2007 Mono Hackers", fvi.LegalCopyright, "#17");
			Assert.AreEqual ("Registered to All", fvi.LegalTrademarks, "#18");
			Assert.AreEqual ("lib2l.dll", fvi.OriginalFilename, "#19");
			Assert.AreEqual (string.Empty, fvi.PrivateBuild, "#20");
			Assert.AreEqual (0, fvi.ProductBuildPart, "#21");
			Assert.AreEqual (0, fvi.ProductMajorPart, "#22");
			Assert.AreEqual (0, fvi.ProductMinorPart, "#23");
			Assert.AreEqual ("Mono Runtime", fvi.ProductName, "#24");
			Assert.AreEqual (0, fvi.ProductPrivatePart, "#25");
			Assert.AreEqual ("b.3.6.c", fvi.ProductVersion, "#26");
			Assert.AreEqual (string.Empty, fvi.SpecialBuild, "#27");
		}

		private static byte [] version_res1 = {
			0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00,
			0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xbc,
			0x03, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x10, 0x00,
			0xff, 0xff, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xbc, 0x03,
			0x34, 0x00, 0x00, 0x00, 0x56, 0x00, 0x53, 0x00, 0x5f, 0x00, 0x56,
			0x00, 0x45, 0x00, 0x52, 0x00, 0x53, 0x00, 0x49, 0x00, 0x4f, 0x00,
			0x4e, 0x00, 0x5f, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x46, 0x00, 0x4f,
			0x00, 0x00, 0x00, 0x00, 0x00, 0xbd, 0x04, 0xef, 0xfe, 0x00, 0x00,
			0x01, 0x00, 0x09, 0x00, 0x06, 0x00, 0x03, 0x00, 0x01, 0x00, 0x08,
			0x00, 0x09, 0x00, 0x06, 0x00, 0x07, 0x00, 0x3f, 0x00, 0x00, 0x00,
			0x28, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x1c, 0x03, 0x00, 0x00, 0x01, 0x00, 0x53, 0x00, 0x74,
			0x00, 0x72, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x67, 0x00, 0x46, 0x00,
			0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x49, 0x00, 0x6e, 0x00, 0x66,
			0x00, 0x6f, 0x00, 0x00, 0x00, 0xf8, 0x02, 0x00, 0x00, 0x01, 0x00,
			0x30, 0x00, 0x30, 0x00, 0x37, 0x00, 0x66, 0x00, 0x30, 0x00, 0x34,
			0x00, 0x62, 0x00, 0x30, 0x00, 0x00, 0x00, 0x2c, 0x00, 0x0a, 0x00,
			0x01, 0x00, 0x43, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x6d, 0x00, 0x65,
			0x00, 0x6e, 0x00, 0x74, 0x00, 0x73, 0x00, 0x00, 0x00, 0x4e, 0x00,
			0x20, 0x00, 0x43, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x6d, 0x00, 0x65,
			0x00, 0x6e, 0x00, 0x74, 0x00, 0x00, 0x00, 0x38, 0x00, 0x0c, 0x00,
			0x01, 0x00, 0x43, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x70, 0x00, 0x61,
			0x00, 0x6e, 0x00, 0x79, 0x00, 0x4e, 0x00, 0x61, 0x00, 0x6d, 0x00,
			0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x4d,
			0x00, 0x6f, 0x00, 0x6e, 0x00, 0x6f, 0x00, 0x20, 0x00, 0x54, 0x00,
			0x65, 0x00, 0x61, 0x00, 0x6d, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x13,
			0x00, 0x01, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00,
			0x44, 0x00, 0x65, 0x00, 0x73, 0x00, 0x63, 0x00, 0x72, 0x00, 0x69,
			0x00, 0x70, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x46, 0x00, 0x69,
			0x00, 0x6c, 0x00, 0x65, 0x00, 0x20, 0x00, 0x44, 0x00, 0x65, 0x00,
			0x73, 0x00, 0x63, 0x00, 0x72, 0x00, 0x69, 0x00, 0x70, 0x00, 0x74,
			0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x34, 0x00, 0x0a, 0x00, 0x01, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c,
			0x00, 0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00,
			0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4e,
			0x00, 0x20, 0x00, 0x31, 0x00, 0x2e, 0x00, 0x32, 0x00, 0x2e, 0x00,
			0x33, 0x00, 0x2e, 0x00, 0x34, 0x00, 0x00, 0x00, 0x2e, 0x00, 0x07,
			0x00, 0x01, 0x00, 0x49, 0x00, 0x6e, 0x00, 0x74, 0x00, 0x65, 0x00,
			0x72, 0x00, 0x6e, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x4e, 0x00, 0x61,
			0x00, 0x6d, 0x00, 0x65, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00,
			0x6c, 0x00, 0x69, 0x00, 0x62, 0x00, 0x33, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x60, 0x00, 0x1e, 0x00, 0x01, 0x00, 0x4c, 0x00, 0x65, 0x00,
			0x67, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x43, 0x00, 0x6f, 0x00, 0x70,
			0x00, 0x79, 0x00, 0x72, 0x00, 0x69, 0x00, 0x67, 0x00, 0x68, 0x00,
			0x74, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x43, 0x00, 0x6f,
			0x00, 0x70, 0x00, 0x79, 0x00, 0x72, 0x00, 0x69, 0x00, 0x67, 0x00,
			0x68, 0x00, 0x74, 0x00, 0x20, 0x00, 0x32, 0x00, 0x30, 0x00, 0x30,
			0x00, 0x37, 0x00, 0x20, 0x00, 0x4d, 0x00, 0x6f, 0x00, 0x6e, 0x00,
			0x6f, 0x00, 0x20, 0x00, 0x48, 0x00, 0x61, 0x00, 0x63, 0x00, 0x6b,
			0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x00, 0x00, 0x50, 0x00,
			0x14, 0x00, 0x01, 0x00, 0x4c, 0x00, 0x65, 0x00, 0x67, 0x00, 0x61,
			0x00, 0x6c, 0x00, 0x54, 0x00, 0x72, 0x00, 0x61, 0x00, 0x64, 0x00,
			0x65, 0x00, 0x6d, 0x00, 0x61, 0x00, 0x72, 0x00, 0x6b, 0x00, 0x73,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x52, 0x00,
			0x65, 0x00, 0x67, 0x00, 0x69, 0x00, 0x73, 0x00, 0x74, 0x00, 0x65,
			0x00, 0x72, 0x00, 0x65, 0x00, 0x64, 0x00, 0x20, 0x00, 0x74, 0x00,
			0x6f, 0x00, 0x20, 0x00, 0x41, 0x00, 0x6c, 0x00, 0x6c, 0x00, 0x00,
			0x00, 0x3e, 0x00, 0x0b, 0x00, 0x01, 0x00, 0x4f, 0x00, 0x72, 0x00,
			0x69, 0x00, 0x67, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x61, 0x00, 0x6c,
			0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x6e, 0x00,
			0x61, 0x00, 0x6d, 0x00, 0x65, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20,
			0x00, 0x6c, 0x00, 0x69, 0x00, 0x62, 0x00, 0x33, 0x00, 0x2e, 0x00,
			0x64, 0x00, 0x6c, 0x00, 0x6c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2e,
			0x00, 0x07, 0x00, 0x01, 0x00, 0x50, 0x00, 0x72, 0x00, 0x69, 0x00,
			0x76, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65, 0x00, 0x42, 0x00, 0x75,
			0x00, 0x69, 0x00, 0x6c, 0x00, 0x64, 0x00, 0x00, 0x00, 0x4e, 0x00,
			0x20, 0x00, 0x50, 0x00, 0x52, 0x00, 0x49, 0x00, 0x56, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x3e, 0x00, 0x0f, 0x00, 0x01, 0x00, 0x50, 0x00,
			0x72, 0x00, 0x6f, 0x00, 0x64, 0x00, 0x75, 0x00, 0x63, 0x00, 0x74,
			0x00, 0x4e, 0x00, 0x61, 0x00, 0x6d, 0x00, 0x65, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x4d, 0x00, 0x6f, 0x00, 0x6e,
			0x00, 0x6f, 0x00, 0x20, 0x00, 0x52, 0x00, 0x75, 0x00, 0x6e, 0x00,
			0x74, 0x00, 0x69, 0x00, 0x6d, 0x00, 0x65, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x38, 0x00, 0x0a, 0x00, 0x01, 0x00, 0x50, 0x00, 0x72, 0x00,
			0x6f, 0x00, 0x64, 0x00, 0x75, 0x00, 0x63, 0x00, 0x74, 0x00, 0x56,
			0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00,
			0x6e, 0x00, 0x00, 0x00, 0x4e, 0x00, 0x20, 0x00, 0x34, 0x00, 0x2c,
			0x00, 0x32, 0x00, 0x2c, 0x00, 0x31, 0x00, 0x2c, 0x00, 0x37, 0x00,
			0x00, 0x00, 0x2e, 0x00, 0x07, 0x00, 0x01, 0x00, 0x53, 0x00, 0x70,
			0x00, 0x65, 0x00, 0x63, 0x00, 0x69, 0x00, 0x61, 0x00, 0x6c, 0x00,
			0x42, 0x00, 0x75, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x64, 0x00, 0x00,
			0x00, 0x4e, 0x00, 0x20, 0x00, 0x53, 0x00, 0x50, 0x00, 0x45, 0x00,
			0x43, 0x00, 0x00, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x01,
			0x00, 0x56, 0x00, 0x61, 0x00, 0x72, 0x00, 0x46, 0x00, 0x69, 0x00,
			0x6c, 0x00, 0x65, 0x00, 0x49, 0x00, 0x6e, 0x00, 0x66, 0x00, 0x6f,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x04, 0x00, 0x00, 0x00,
			0x54, 0x00, 0x72, 0x00, 0x61, 0x00, 0x6e, 0x00, 0x73, 0x00, 0x6c,
			0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x7f, 0x00, 0xb0, 0x04 };

		private static byte [] version_res2 = {
			0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00,
			0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xec,
			0x01, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x10, 0x00,
			0xff, 0xff, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xec, 0x01,
			0x34, 0x00, 0x00, 0x00, 0x56, 0x00, 0x53, 0x00, 0x5f, 0x00, 0x56,
			0x00, 0x45, 0x00, 0x52, 0x00, 0x53, 0x00, 0x49, 0x00, 0x4f, 0x00,
			0x4e, 0x00, 0x5f, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x46, 0x00, 0x4f,
			0x00, 0x00, 0x00, 0x00, 0x00, 0xbd, 0x04, 0xef, 0xfe, 0x00, 0x00,
			0x01, 0x00, 0x09, 0x00, 0x06, 0x00, 0x03, 0x00, 0x01, 0x00, 0x08,
			0x00, 0x09, 0x00, 0x06, 0x00, 0x07, 0x00, 0x17, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x4c, 0x01, 0x00, 0x00, 0x01, 0x00, 0x53, 0x00, 0x74,
			0x00, 0x72, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x67, 0x00, 0x46, 0x00,
			0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x49, 0x00, 0x6e, 0x00, 0x66,
			0x00, 0x6f, 0x00, 0x00, 0x00, 0x28, 0x01, 0x00, 0x00, 0x01, 0x00,
			0x30, 0x00, 0x30, 0x00, 0x37, 0x00, 0x66, 0x00, 0x30, 0x00, 0x34,
			0x00, 0x62, 0x00, 0x30, 0x00, 0x00, 0x00, 0x22, 0x00, 0x01, 0x00,
			0x01, 0x00, 0x43, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x70, 0x00, 0x61,
			0x00, 0x6e, 0x00, 0x79, 0x00, 0x4e, 0x00, 0x61, 0x00, 0x6d, 0x00,
			0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2a,
			0x00, 0x01, 0x00, 0x01, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00,
			0x65, 0x00, 0x44, 0x00, 0x65, 0x00, 0x73, 0x00, 0x63, 0x00, 0x72,
			0x00, 0x69, 0x00, 0x70, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00,
			0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22,
			0x00, 0x01, 0x00, 0x01, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00,
			0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69,
			0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x22, 0x00, 0x01, 0x00, 0x01, 0x00, 0x49, 0x00, 0x6e,
			0x00, 0x74, 0x00, 0x65, 0x00, 0x72, 0x00, 0x6e, 0x00, 0x61, 0x00,
			0x6c, 0x00, 0x4e, 0x00, 0x61, 0x00, 0x6d, 0x00, 0x65, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x2a, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x4f, 0x00, 0x72, 0x00, 0x69, 0x00, 0x67, 0x00, 0x69, 0x00, 0x6e,
			0x00, 0x61, 0x00, 0x6c, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00,
			0x65, 0x00, 0x6e, 0x00, 0x61, 0x00, 0x6d, 0x00, 0x65, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x22, 0x00, 0x01, 0x00, 0x01, 0x00,
			0x50, 0x00, 0x72, 0x00, 0x6f, 0x00, 0x64, 0x00, 0x75, 0x00, 0x63,
			0x00, 0x74, 0x00, 0x4e, 0x00, 0x61, 0x00, 0x6d, 0x00, 0x65, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x26, 0x00, 0x01,
			0x00, 0x01, 0x00, 0x50, 0x00, 0x72, 0x00, 0x6f, 0x00, 0x64, 0x00,
			0x75, 0x00, 0x63, 0x00, 0x74, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72,
			0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x01, 0x00, 0x56,
			0x00, 0x61, 0x00, 0x72, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6c, 0x00,
			0x65, 0x00, 0x49, 0x00, 0x6e, 0x00, 0x66, 0x00, 0x6f, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x24, 0x00, 0x04, 0x00, 0x00, 0x00, 0x54, 0x00,
			0x72, 0x00, 0x61, 0x00, 0x6e, 0x00, 0x73, 0x00, 0x6c, 0x00, 0x61,
			0x00, 0x74, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x7f, 0x00, 0xb0, 0x04 };

		private static byte [] version_res3 = {
			0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00,
			0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5c,
			0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x10, 0x00,
			0xff, 0xff, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5c, 0x00,
			0x34, 0x00, 0x00, 0x00, 0x56, 0x00, 0x53, 0x00, 0x5f, 0x00, 0x56,
			0x00, 0x45, 0x00, 0x52, 0x00, 0x53, 0x00, 0x49, 0x00, 0x4f, 0x00,
			0x4e, 0x00, 0x5f, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x46, 0x00, 0x4f,
			0x00, 0x00, 0x00, 0x00, 0x00, 0xbd, 0x04, 0xef, 0xfe, 0x00, 0x00,
			0x01, 0x00, 0x09, 0x00, 0x06, 0x00, 0x03, 0x00, 0x01, 0x00, 0x08,
			0x00, 0x09, 0x00, 0x06, 0x00, 0x07, 0x00, 0x17, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00 };

		private static byte [] version_res4 = {
			0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00,
			0x00, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};
#endif
	}
}
