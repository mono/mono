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
using System.Security;
using System.Security.Permissions;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Security.Permissions {

	public class FileIOPermissionTest : TestCase {
		
		string[] pathArrayGood;
		string[] pathArrayBad;
		FileIOPermission p;
		FileIOPermission p2;
		string[] pathsInPermission;
		string[] pathArrayGood2;
		FileIOPermission unrestricted;

		protected override void SetUp() {
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
				pathArrayBad[1] = "/usr/temp*";
				pathArrayGood2[0] = "/temp1";
				pathArrayGood2[1] = "/usr/temp2";
				pathArrayGood2[2] = "/usr/bin/something";
			}
		}

		private void SetDefaultData() {
		}
		
		public void TestConstructorPermissionState() {
			p = new FileIOPermission(PermissionState.None);
			AssertEquals("Should be Restricted", false, p.IsUnrestricted());
			p = new FileIOPermission(PermissionState.Unrestricted);
			AssertEquals("Should be Unrestricted", true, p.IsUnrestricted());
			try{
				p = new FileIOPermission((PermissionState)77);
				Fail("Should have thrown an exception on invalid PermissionState");
			}
			catch{
				// we should be here if things are working.  nothing to do
			}
		}

		public void TestConstructorString() {
			try{
				p = new FileIOPermission(FileIOPermissionAccess.Append, "this path is not rooted");
				Fail("Should have thrown an exception on path not rooted");
			}
			catch{}

			try{
				p = new FileIOPermission(FileIOPermissionAccess.Append, "<this is not a valid path>");
				Fail("Should have thrown an exception on invalid characters in path");
			}
			catch{}
			
			try{
				p = new FileIOPermission(FileIOPermissionAccess.Append, "\\\\mycomputer\\test*");
				Fail("Should have thrown an exception on wildcards not allowed in path");
			}
			catch{}

			try{
				p = new FileIOPermission((FileIOPermissionAccess)77, "c:\\temp");
				Fail("Should have thrown an exception on invalid access value");
			}
			catch{}

			string pathToAdd;
			// FIXME: Adjust to run on Mac OS's
			if (Path.VolumeSeparatorChar == ':')
				pathToAdd = "c:\\temp";
			else
				pathToAdd = "/temp";

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathToAdd);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert("Does not contain correct number of paths. Expected 1 but got: "+pathsInPermission.Length, pathsInPermission.Length == 1);
			Assert("Does not contain expected path from constructor: "+pathToAdd, pathsInPermission[0] == pathToAdd);
		}

		public void TestConstructorStringArray() {
			try{
				p = new FileIOPermission(FileIOPermissionAccess.Append, pathArrayBad);
				Fail("Should have thrown an exception on wildcards not allowed in path");
			}
			catch{}

			try{
				p = new FileIOPermission((FileIOPermissionAccess)77, pathArrayGood);
				Fail("Should have thrown an exception on invalid access value");
			}
			catch{}

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert("Does not contain correct number of paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			foreach (string s in pathsInPermission){
				Assert("Unexpected path in the Permission: " + s, Array.IndexOf(pathsInPermission, s) >=0);
			}

		}

		public void TestAddPathListStringArray() {
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert("Does not contain correct number of paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			foreach (string s in pathsInPermission){
				Assert("Unexpected path in the Permission: " + s, Array.IndexOf(pathsInPermission, s) >=0);
			}

			p.AddPathList(FileIOPermissionAccess.Append, pathArrayGood);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should still contain correct number Read paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			foreach (string s in pathsInPermission){
				Assert("Unexpected path in the Permission: " + s, Array.IndexOf(pathsInPermission, s) >=0);
			}
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Append);
			Assert("Should contain correct number of Append paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			foreach (string s in pathsInPermission){
				Assert("Unexpected path in the Permission: " + s, Array.IndexOf(pathsInPermission, s) >=0);
			}
		}

		public void TestIntersect() {
			FileIOPermission intersection;

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			p.AllFiles = FileIOPermissionAccess.Append;
			p.AllLocalFiles = FileIOPermissionAccess.Write;
			
			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			
			intersection = (FileIOPermission)p.Intersect(unrestricted);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			Assert("Should have Append bit in AllFiles.", (intersection.AllFiles & FileIOPermissionAccess.Append) != 0);
			Assert("Should have Write bit in AllLocalFiles.", (intersection.AllLocalFiles & FileIOPermissionAccess.Write) != 0);

			intersection = (FileIOPermission)unrestricted.Intersect(p);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			Assert("Should have Append bit in AllFiles.", (intersection.AllFiles & FileIOPermissionAccess.Append) != 0);
			Assert("Should have Write bit in AllLocalFiles.", (intersection.AllLocalFiles & FileIOPermissionAccess.Write) != 0);

			p2 = new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Read, pathArrayGood2);
			p2.AllFiles = FileIOPermissionAccess.Append | FileIOPermissionAccess.Write;
			p2.AllLocalFiles = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
			intersection = (FileIOPermission)p.Intersect(p2);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			AssertNotNull ("Should have some paths", pathsInPermission);
			AssertEquals ("Should contain correct number of Read paths", 2, pathsInPermission.Length);
			AssertEquals ("Should have only Append bit in AllFiles.",  FileIOPermissionAccess.Append, intersection.AllFiles);
			AssertEquals ("Should have only Write bit in AllLocalFiles.",  FileIOPermissionAccess.Write, intersection.AllLocalFiles);

			intersection = (FileIOPermission)p2.Intersect(p);
			pathsInPermission = intersection.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should contain correct number of Read paths. Expected 2 but got: "+pathsInPermission.Length, pathsInPermission.Length == 2);
			Assert("Should have only Append bit in AllFiles.", intersection.AllFiles == FileIOPermissionAccess.Append);
			Assert("Should have only Write bit in AllLocalFiles.", intersection.AllLocalFiles == FileIOPermissionAccess.Write);
		}

		public void TestIsSubsetOf() {
			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			Assert("IsSubsetOf reflective test failed", unrestricted.IsSubsetOf(unrestricted));

			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			p.AllFiles = FileIOPermissionAccess.Append;
			p.AllLocalFiles = FileIOPermissionAccess.Write;
			Assert("#1 IsSubsetOf reflective test failed", p.IsSubsetOf(p));
			Assert("#1 IsSubsetOf false test failed", !unrestricted.IsSubsetOf(p));
			Assert("#1 IsSubsetOf true test failed", p.IsSubsetOf(unrestricted));

			p2 = new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Read, pathArrayGood2);
			p2.AllFiles = FileIOPermissionAccess.Append | FileIOPermissionAccess.Write;
			p2.AllLocalFiles = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
			Assert("#2 IsSubsetOf reflective test failed", p2.IsSubsetOf(p2));
			Assert("#2 IsSubsetOf true test failed", p.IsSubsetOf(p2));
			Assert("#2 IsSubsetOf false test failed", !p2.IsSubsetOf(p));
		}

		public void TestUnion() {
			FileIOPermission union;

			unrestricted = new FileIOPermission(PermissionState.Unrestricted);
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			union = (FileIOPermission)unrestricted.Union(p);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should get an unrestricted permission", union.IsUnrestricted());
			Assert("Path list should be empty", pathsInPermission == null);

			union = (FileIOPermission)p.Union(unrestricted);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert("Should get an unrestricted permission", union.IsUnrestricted());
			Assert("Path list should be empty", pathsInPermission == null);

			p2 = new FileIOPermission(FileIOPermissionAccess.Append, pathArrayGood2);

			union = (FileIOPermission)p.Union(p2);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert("Path list should have 2 for Read", pathsInPermission.Length == pathArrayGood.Length);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Append);
			Assert("Path list should have 3 for Append", pathsInPermission.Length == pathArrayGood2.Length);

			union = (FileIOPermission)p2.Union(p);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Read);
			Assert("Path list should have 2 for Read", pathsInPermission.Length == pathArrayGood.Length);
			pathsInPermission = union.GetPathList(FileIOPermissionAccess.Append);
			Assert("Path list should have 3 for Append", pathsInPermission.Length == pathArrayGood2.Length);
		}

		public void TestFromXML() {
			p = new FileIOPermission(PermissionState.None);
			SecurityElement esd;

			esd = new SecurityElement("IPermission");
			esd.AddAttribute("class", "FileIOPermission");
			esd.AddAttribute("version", "1");
			esd.AddAttribute("Unrestricted", "true");
			p.FromXml(esd);
			Assert("Should get an unrestricted permission", p.IsUnrestricted());

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
			Assert("Path list should have 2 for Read", pathsInPermission.Length == 2);
			pathsInPermission = p.GetPathList(FileIOPermissionAccess.Write);
			Assert("Path list should have 2 for Write", pathsInPermission.Length == 3);
		}

		public void TestToXML() {
			SecurityElement esd;
			string read;
			p = new FileIOPermission(FileIOPermissionAccess.Read, pathArrayGood);
			esd = p.ToXml();
			Assert("Esd tag incorrect", esd.Tag == "IPermission");
			Assert("Esd version incorrect", (String)esd.Attributes["version"] == "1");
			read = (String)esd.Attributes["Read"];
			pathsInPermission = read.Split(';');
			Assert("Path list should have 2 for Read", pathsInPermission.Length == 2);
		}
	}
}

