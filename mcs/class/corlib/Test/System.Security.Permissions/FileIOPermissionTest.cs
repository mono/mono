//
// MonoTests.System.Security.Permissions.FileIOPermissionTest.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak II
//
// Note: Only Unix and Windows file paths are tested.  To run the tests on Mac OS's
// search for the "FIXME" notes below and adjust accordingly.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Security.Permissions {
#if !TARGET_JVM
	public class FilePathUtil {
		[DllImport("kernel32.dll")]
		private static extern uint GetLongPathName (string shortPath, 
			StringBuilder buffer, uint bufLength);

		static public string GetLongPathName (string somePath) 
		{
			StringBuilder buffer = new StringBuilder(260);
			if (0 != GetLongPathName (somePath, buffer, (uint) buffer.Capacity))
				return buffer.ToString ();
			else
				return null;
		}

		[DllImport("kernel32.dll", SetLastError=true)] 
		private static extern uint GetShortPathName ( string longPath, 
			StringBuilder buffer, uint bufLength);

		static public string GetShortPathName (string somePath) 
		{
			StringBuilder buffer = new StringBuilder(260);
			if (0 != GetShortPathName (somePath, buffer, (uint) buffer.Capacity))
				return buffer.ToString ();
			else
				return null;
		}
	}
#endif

	[TestFixture]
	public class FileIOPermissionTest {
		
		string[] pathArrayGood;
		string[] pathArrayBad;
		FileIOPermission p;
		FileIOPermission p2;
		string[] pathsInPermission;
		string[] pathArrayGood2;
		FileIOPermission unrestricted;

		private string filename;
		private bool unix;

		[SetUp]
		public void SetUp () 
		{
			Environment.CurrentDirectory = Path.GetTempPath();
			filename = Path.GetTempFileName ();

			int os = (int) Environment.OSVersion.Platform;
			unix = ((os == 4) || (os == 128) || (os == 6));

			p = null;
			pathsInPermission = null;
			pathArrayGood = new string[2];
			pathArrayBad = new string[2];
			pathArrayGood2 = new string[3];
			// FIXME: Adjust to run on Mac OS's
			if (Path.VolumeSeparatorChar == ':') {
				pathArrayGood[0] = "c:\\temp1";
				pathArrayGood[1] = "d:\\temp2";
				pathArrayBad[0] = "c:\\temp1";
				pathArrayBad[1] = "d:\\temp*";
				pathArrayGood2[0] = "c:\\temp1";
				pathArrayGood2[1] = "d:\\temp2";
				pathArrayGood2[2] = "z:\\something";
			}
			else {
				pathArrayGood[0] = "/temp1";
				pathArrayGood[1] = "/usr/temp2";
				pathArrayBad[0] = "/temp1";
				pathArrayBad[1] = "/usr/temp*"; // not really bad under Unix...
				pathArrayGood2[0] = "/temp1";
				pathArrayGood2[1] = "/usr/temp2";
				pathArrayGood2[2] = "/usr/bin/something";
			}
		}

		[TearDown]
		public void TearDown () 
		{
			if (File.Exists (filename))
				File.Delete (filename);
		}

		[Test]
		public void ConstructorPermissionState ()
		{
			p = new FileIOPermission(PermissionState.None);
			Assert.AreEqual(false, p.IsUnrestricted(), "Should be Restricted");
			p = new FileIOPermission(PermissionState.Unrestricted);
			Assert.AreEqual(true, p.IsUnrestricted(), "Should be Unrestricted");
			try{
				p = new FileIOPermission((PermissionState)77);
				Assert.Fail("Should have thrown an exception on invalid PermissionState");
			}
			catch{
				// we should be here if things are working.  nothing to do
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorString_Null () 
		{
			p = new FileIOPermission(FileIOPermissionAccess.Append, (string)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_NotRooted ()
		{
			p = new FileIOPermission(FileIOPermissionAccess.Append, "this path is not rooted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_InvalidPath () 
		{
			p = new FileIOPermission(FileIOPermissionAccess.Append, "<this is not a valid path>");
		}

		[Test]
		public void ConstructorString_Wildcard () 
		{
			try {
				// note: this is a valid path on UNIX so we must be able to protect it
				p = new FileIOPermission(FileIOPermissionAccess.Append, pathArrayBad [1]);
			}
			catch (ArgumentException) {
				if (unix)
					Assert.Fail ("Wildcard * is valid in filenames");
				// else it's normal for Windows to throw ArgumentException
			}
			catch (Exception e) {
				Assert.Fail ("Bad or wrong exception: " + e.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_InvalidAccess () 
		{
			p = new FileIOPermission((FileIOPermissionAccess)77, "c:\\temp");
		}

		[Test]
		public void ConstructorString ()
		{
			string pathToAdd;
			// FIXME: Adjust to run on Mac OS's
			if (Path.VolumeSeparatorChar == ':')
				pathToAdd = "c:\\temp";
			else
				pathToAdd = "/temp";

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathToAdd);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 1, "Does not contain correct number of paths. Expected 1 but got: "+pathsInPermission.Length);
			Assert.IsTrue(pathsInPermission[0] == pathToAdd, "Does not contain expected path from constructor: "+pathToAdd);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStringArray_Null () 
		{
			p = new FileIOPermission(FileIOPermissionAccess.Append, (string[])null);
		}

		[Test]
		public void ConstructorStringArray_Wildcard () 
		{
			try {
				// note: this is a valid path on UNIX so we must be able to protect it
				p = new FileIOPermission(FileIOPermissionAccess.Append, pathArrayBad);
			}
			catch (ArgumentException) {
				if (unix)
					Assert.Fail ("Wildcard * is valid in filenames");
				// else it's normal for Windows to throw ArgumentException
			}
			catch (Exception e) {
				Assert.Fail ("Bad or wrong exception: " + e.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorStringArray_InvalidAccess () 
		{
			p = new FileIOPermission((FileIOPermissionAccess)77, pathArrayGood);
		}

		[Test]
		public void ConstructorStringArray () 
		{
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Does not contain correct number of paths. Expected 2 but got: "+pathsInPermission.Length);
			foreach (string s in pathsInPermission){
				Assert.IsTrue (Array.IndexOf(pathsInPermission, s) >=0, "Unexpected path in the Permission: " + s);
			}
		}

		[Test]
		public void AddPathListStringArray ()
		{
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Does not contain correct number of paths. Expected 2 but got: "+pathsInPermission.Length);
			foreach (string s in pathsInPermission){
				Assert.IsTrue (Array.IndexOf(pathsInPermission, s) >=0, "Unexpected path in the Permission: " + s);
			}

			p.AddPathList(FileIOPermissionAccess.Append, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Should still contain correct number Read paths. Expected 2 but got: "+pathsInPermission.Length);
			foreach (string s in pathsInPermission){
				Assert.IsTrue (Array.IndexOf(pathsInPermission, s) >=0, "Unexpected path in the Permission: " + s);
			}
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Append);
			Assert.IsTrue (pathsInPermission.Length == 2, "Should contain correct number of Append paths. Expected 2 but got: "+pathsInPermission.Length);
			foreach (string s in pathsInPermission){
				Assert.IsTrue (Array.IndexOf(pathsInPermission, s) >=0, "Unexpected path in the Permission: " + s);
			}
		}

		[Test]
		public void Intersect ()
		{
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			p.AllFiles = FileIOPermissionAccess.Append;
			p.AllLocalFiles = FileIOPermissionAccess.Write;
			
			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			
			FileIOPermission intersection = (FileIOPermission)p.Intersect(unrestricted);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length);
			Assert.IsTrue((intersection.AllFiles & FileIOPermissionAccess.Append) != 0, "Should have Append bit in AllFiles.");
			Assert.IsTrue((intersection.AllLocalFiles & FileIOPermissionAccess.Write) != 0, "Should have Write bit in AllLocalFiles.");

			intersection = (FileIOPermission)unrestricted.Intersect(p);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length);
			Assert.IsTrue((intersection.AllFiles & FileIOPermissionAccess.Append) != 0, "Should have Append bit in AllFiles.");
			Assert.IsTrue((intersection.AllLocalFiles & FileIOPermissionAccess.Write) != 0, "Should have Write bit in AllLocalFiles.");

			p2 = new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Read, pathArrayGood2);
			p2.AllFiles = FileIOPermissionAccess.Append | FileIOPermissionAccess.Write;
			p2.AllLocalFiles = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
			intersection = (FileIOPermission)p.Intersect(p2);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsNotNull (pathsInPermission, "Should have some paths");
			Assert.AreEqual (2, pathsInPermission.Length, "Should contain correct number of Read paths");
			Assert.AreEqual ( FileIOPermissionAccess.Append, intersection.AllFiles, "Should have only Append bit in AllFiles.");
			Assert.AreEqual ( FileIOPermissionAccess.Write, intersection.AllLocalFiles, "Should have only Write bit in AllLocalFiles.");

			intersection = (FileIOPermission)p2.Intersect(p);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue (pathsInPermission.Length == 2, "Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length);
			Assert.IsTrue(intersection.AllFiles == FileIOPermissionAccess.Append, "Should have only Append bit in AllFiles.");
			Assert.IsTrue(intersection.AllLocalFiles == FileIOPermissionAccess.Write, "Should have only Write bit in AllLocalFiles.");
		}

		[Test]
		public void IsSubsetOf ()
		{
			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			Assert.IsTrue(unrestricted.IsSubsetOf(unrestricted), "IsSubsetOf reflective test failed");

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			p.AllFiles = FileIOPermissionAccess.Append;
			p.AllLocalFiles = FileIOPermissionAccess.Write;
			Assert.IsTrue(p.IsSubsetOf(p), "#1 IsSubsetOf reflective test failed");
			Assert.IsTrue(!unrestricted.IsSubsetOf(p), "#1 IsSubsetOf false test failed");
			Assert.IsTrue(p.IsSubsetOf(unrestricted), "#1 IsSubsetOf true test failed");

			p2 = new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Read, pathArrayGood2);
			p2.AllFiles = FileIOPermissionAccess.Append | FileIOPermissionAccess.Write;
			p2.AllLocalFiles = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
			Assert.IsTrue(p2.IsSubsetOf(p2), "#2 IsSubsetOf reflective test failed");
			Assert.IsTrue(p.IsSubsetOf(p2), "#2 IsSubsetOf true test failed");
			Assert.IsTrue(!p2.IsSubsetOf(p), "#2 IsSubsetOf false test failed");
		}

		[Test]
		public void Union ()
		{
			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);

			FileIOPermission union = (FileIOPermission)unrestricted.Union(p);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue(union.IsUnrestricted(), "Should get an unrestricted permission");
			Assert.IsTrue(pathsInPermission == null, "Path list should be empty");

			union = (FileIOPermission)p.Union(unrestricted);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue(union.IsUnrestricted(), "Should get an unrestricted permission");
			Assert.IsTrue(pathsInPermission == null, "Path list should be empty");

			p2 = new FileIOPermission(FileIOPermissionAccess.Append, pathArrayGood2);

			union = (FileIOPermission)p.Union(p2);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue(pathsInPermission.Length == pathArrayGood.Length, "Path list should have 2 for Read");
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Append);
			Assert.IsTrue(pathsInPermission.Length == pathArrayGood2.Length, "Path list should have 3 for Append");

			union = (FileIOPermission)p2.Union(p);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue(pathsInPermission.Length == pathArrayGood.Length, "Path list should have 2 for Read");
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Append);
			Assert.IsTrue(pathsInPermission.Length == pathArrayGood2.Length, "Path list should have 3 for Append");
		}

		[Test]
		public void Union_Bug79118 ()
		{
			string[] f1 = unix ? new string[] { "/tmp/one", "/tmp/two" } : new string[] { "c:\\temp\\one", "c:\\temp\\two" };
			string[] f2 = unix ? new string[] { "/tmp/two" } : new string[] { "c:\\temp\\two" };

			p = new FileIOPermission (FileIOPermissionAccess.Read, f1);
			p2 = new FileIOPermission (FileIOPermissionAccess.Read, f2);
			FileIOPermission union = (FileIOPermission) p.Union (p2);

			string[] paths = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.AreEqual (2, paths.Length, "Length");
			Assert.AreEqual (f1[0], paths[0], "0");
			Assert.AreEqual (f1[1], paths[1], "1");
		}

		private void Partial (string msg, string[] path1, string[] path2, int expected)
		{
			p = new FileIOPermission (FileIOPermissionAccess.Read, path1);
			p2 = new FileIOPermission (FileIOPermissionAccess.Read, path2);
			FileIOPermission union = (FileIOPermission) p.Union (p2);

			string[] paths = union.GetPathList(FileIOPermissionAccess.Read);
			Assert.AreEqual (expected, paths.Length, msg + ".Length");
			Assert.AreEqual (path1[0], paths[0], msg + "[0]");
			if (expected > 1)
				Assert.AreEqual (path2[0], paths[1], msg + "[1]");
		}

		[Test]
		public void Union_Partial ()
		{
			string[] f1 = unix ? new string[] { "/dir/part" } : new string[] { "c:\\dir\\part" };
			string[] f2 = unix ? new string[] { "/dir/partial" } : new string[] { "c:\\dir\\partial" };
			Partial ("1", f1, f2, 2);
			Partial ("2", f2, f1, 2);

			f1 = unix ? new string[] { "/dir/part/" } : new string[] { "c:\\dir\\part\\" };
			f2 = unix ? new string[] { "/dir/partial/" } : new string[] { "c:\\dir\\partial\\" };
			Partial ("3", f1, f2, 2);
			Partial ("4", f2, f1, 2);

			f1 = unix ? new string[] { "/dir/part/ial" } : new string[] { "c:\\dir\\part\\ial" };
			f2 = unix ? new string[] { "/dir/part/ial" } : new string[] { "c:\\dir\\part\\ial" };
			Partial ("5", f1, f2, 1);
			Partial ("6", f2, f1, 1);
		}

		[Test]
		public void FromXML ()
		{
			p = new FileIOPermission(PermissionState.None);
			SecurityElement esd = new SecurityElement("IPermission");
			esd.AddAttribute("class", "FileIOPermission");
			esd.AddAttribute("version", "1");
			esd.AddAttribute("Unrestricted", "true");
			p.FromXml(esd);
			Assert.IsTrue(p.IsUnrestricted(), "Should get an unrestricted permission");

			esd = new SecurityElement("IPermission");
			esd.AddAttribute("class", "FileIOPermission");
			esd.AddAttribute("version", "1");
			// FIXME: Adjust to run on Mac OS's
			if (Path.VolumeSeparatorChar == ':') {
				esd.AddAttribute("Read", "c:\\temp;d:\\temp2");
				esd.AddAttribute("Write", "c:\\temp;d:\\temp2;z:\\temp3");
			}
			else {
				esd.AddAttribute("Read", "/temp;/usr/temp2");
				esd.AddAttribute("Write", "/temp;/usr/temp2;/usr/bin/temp3");
			}
			p = new FileIOPermission(PermissionState.None);
			p.FromXml(esd);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert.IsTrue(pathsInPermission.Length == 2, "Path list should have 2 for Read");
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Write);
			Assert.IsTrue(pathsInPermission.Length == 3, "Path list should have 2 for Write");
		}

		[Test]
		public void ToXML ()
		{
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			SecurityElement esd = p.ToXml();
			Assert.IsTrue(esd.Tag == "IPermission", "Esd tag incorrect");
			Assert.IsTrue((String)esd.Attributes["version"] == "1", "Esd version incorrect");
			string read = (String)esd.Attributes["Read"];
			pathsInPermission = read.Split(';');
			Assert.IsTrue(pathsInPermission.Length == 2, "Path list should have 2 for Read");
		}
#if !TARGET_JVM
		[Test]
		[Ignore("should compatibility go that far ?")]
		public void ShortToLong () 
		{
			// on windows this returns a "short" (8.3) path and filename
			string filename = Path.GetTempFileName ();
			p = new FileIOPermission(FileIOPermissionAccess.Read, filename);
			string[] files = p.GetPathList (FileIOPermissionAccess.Read);
			Assert.AreEqual (1, files.Length, "GetPathList.Count");
			// FIXME: here GetTempFileName != GetPathList[0] for MS but == for Mono
			Assert.AreEqual (Path.GetFileName (filename), Path.GetFileName (files [0]), "Path.GetFileName(GetTempFileName)==Path.GetFileName(GetPathList[0])");
			// note: this will fail on Linux as kernel32.dll isn't available
			Assert.AreEqual (FilePathUtil.GetLongPathName (filename), files [0], "GetLongPathName(GetTempFileName)==GetPathList[0]");
		}
#endif
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FileUrl ()
		{
			// file://... isn't accepted
			string filename = Assembly.GetExecutingAssembly ().CodeBase;
			p = new FileIOPermission (FileIOPermissionAccess.Read, filename);
		}
	}
}
