//
// System.IO.Path Test Cases
//
// Authors:
// 	Marcin Szczepanski (marcins@zipworld.com.au)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ben Maurer (bmaurer@users.sf.net)
//	Gilles Freart (gfr@skynet.be)
//
// (c) Marcin Szczepanski 
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Ben Maurer
// (c) 2003 Gilles Freart
//

#define NUNIT // Comment out this one if you wanna play with the test without using NUnit

#if NUNIT
using NUnit.Framework;
#else
using System.Reflection;
#endif

using System.IO;
using System;
using System.Text;

namespace MonoTests.System.IO
{

	enum OsType {
		Windows,
		Unix,
		Mac
	}

#if NUNIT
	public class PathTest : TestCase
	{
#else
	public class PathTest
	{
#endif
		static string path1;
		static string path2;
		static string path3;
		static OsType OS;
		static char DSC = Path.DirectorySeparatorChar;
	     
#if NUNIT
		protected override void SetUp ()
		{
#else
		static PathTest ()
		{
#endif
			if ('/' == DSC) {
				OS = OsType.Unix;
				path1 = "/foo/test.txt";
				path2 = "/etc";
				path3 = "init.d";
			} else if ('\\' == DSC) {
				OS = OsType.Windows;
				path1 = "c:\\foo\\test.txt";
				path2 = Environment.GetEnvironmentVariable ("SYSTEMROOT");
				path3 = "system32";
			} else {
				OS = OsType.Mac;
				//FIXME: For Mac. figure this out when we need it
				path1 = "foo:test.txt";
				path2 = "foo";
				path3 = "bar";
			}
		}

		bool Windows
		{
			get {
				return OS == OsType.Windows;
			}
		}

		bool Unix
		{
			get {
				return OS == OsType.Unix;
			}
		}

		bool Mac
		{
			get {
				return OS == OsType.Mac;
			}
		}

		public void TestChangeExtension ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/foo/test.doc";
			files [(int) OsType.Windows] = "c:\\foo\\test.doc";
			files [(int) OsType.Mac] = "foo:test.doc";

			string testPath = Path.ChangeExtension (path1, "doc");
			AssertEquals ("ChangeExtension #01", files [(int) OS], testPath);

			testPath = Path.ChangeExtension ("", ".extension");
			AssertEquals ("ChangeExtension #02", String.Empty, testPath);

			testPath = Path.ChangeExtension (null, ".extension");
			AssertEquals ("ChangeExtension #03", null, testPath);

			testPath = Path.ChangeExtension ("path", null);
			AssertEquals ("ChangeExtension #04", "path", testPath);

			testPath = Path.ChangeExtension ("path.ext", "doc");
			AssertEquals ("ChangeExtension #05", "path.doc", testPath);

			testPath = Path.ChangeExtension ("path.ext1.ext2", "doc");
			AssertEquals ("ChangeExtension #06", "path.ext1.doc", testPath);

			if (Windows) {
				try {
					testPath = Path.ChangeExtension ("<", ".extension");
					Fail ("ChangeException Fail #01");
				} catch (Exception e) {
					AssertEquals ("ChangeExtension Exc. #01", typeof (ArgumentException), e.GetType ());
				}
			}
		}

		public void TestCombine ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/etc/init.d";
			files [(int) OsType.Windows] = Environment.GetEnvironmentVariable ("SYSTEMROOT") + @"\system32";
			files [(int) OsType.Mac] = "foo:bar";

			string testPath = Path.Combine (path2, path3);
			AssertEquals ("Combine #01", files [(int) OS], testPath);

			testPath = Path.Combine ("one", "");
			AssertEquals ("Combine #02", "one", testPath);

			testPath = Path.Combine ("", "one");
			AssertEquals ("Combine #03", "one", testPath);

			string current = Directory.GetCurrentDirectory ();
			testPath = Path.Combine (current, "one");

			string expected = current + DSC + "one";
			AssertEquals ("Combine #04", expected, testPath);

			testPath = Path.Combine ("one", current);
			// LAMESPEC noted in Path.cs
			AssertEquals ("Combine #05", current, testPath);

			testPath = Path.Combine (current, expected);
			AssertEquals ("Combine #06", expected, testPath);

			testPath = DSC + "one";
			testPath = Path.Combine (testPath, "two" + DSC);
			expected = DSC + "one" + DSC + "two" + DSC;
			AssertEquals ("Combine #06", expected, testPath);

			testPath = "one" + DSC;
			testPath = Path.Combine (testPath, DSC + "two");
			expected = DSC + "two";
			AssertEquals ("Combine #06", expected, testPath);

			testPath = "one" + DSC;
			testPath = Path.Combine (testPath, "two" + DSC);
			expected = "one" + DSC + "two" + DSC;
			AssertEquals ("Combine #07", expected, testPath);

			//TODO: Tests for UNC names
			try {
				testPath = Path.Combine ("one", null);
				Fail ("Combine Fail #01");
			} catch (Exception e) {
				AssertEquals ("Combine Exc. #01", typeof (ArgumentNullException), e.GetType ());
			}

			try {
				testPath = Path.Combine (null, "one");
				Fail ("Combine Fail #02");
			} catch (Exception e) {
				AssertEquals ("Combine Exc. #02", typeof (ArgumentNullException), e.GetType ());
			}

			if (Windows) {
				try {
					testPath = Path.Combine ("a>", "one");
					Fail ("Combine Fail #03");
				} catch (Exception e) {
					AssertEquals ("Combine Exc. #03", typeof (ArgumentException), e.GetType ());
				}

				try {
					testPath = Path.Combine ("one", "aaa<");
					Fail ("Combine Fail #04");
				} catch (Exception e) {
					AssertEquals ("Combine Exc. #04", typeof (ArgumentException), e.GetType ());
				}
			}
		}

		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void EmptyDirectoryName ()
		{
			string testDirName = Path.GetDirectoryName ("");
		}

		public void TestDirectoryName ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/foo";
			files [(int) OsType.Windows] = "c:\\foo";
			files [(int) OsType.Mac] = "foo";

			AssertEquals ("GetDirectoryName #01", null, Path.GetDirectoryName (null));
			string testDirName = Path.GetDirectoryName (path1);
			AssertEquals ("GetDirectoryName #02", files [(int) OS], testDirName);
			testDirName = Path.GetDirectoryName (files [(int) OS] + DSC);
			AssertEquals ("GetDirectoryName #03", files [(int) OS], testDirName);

			if (Windows) {
				try {
					testDirName = Path.GetDirectoryName ("aaa>");
					Fail ("GetDirectoryName Fail #02");
				} catch (Exception e) {
					AssertEquals ("GetDirectoryName Exc. #02", typeof (ArgumentException), e.GetType ());
				}
			}

			try {
				testDirName = Path.GetDirectoryName ("   ");
				Fail ("GetDirectoryName Fail #03");
			} catch (Exception e) {
				AssertEquals ("GetDirectoryName Exc. #03", typeof (ArgumentException), e.GetType ());
			}
		}

		public void TestGetExtension ()
		{
			string testExtn = Path.GetExtension (path1);

			AssertEquals ("GetExtension #01",  ".txt", testExtn);

			testExtn = Path.GetExtension (path2);
			AssertEquals ("GetExtension #02", String.Empty, testExtn);

			testExtn = Path.GetExtension (String.Empty);
			AssertEquals ("GetExtension #03", String.Empty, testExtn);

			testExtn = Path.GetExtension (null);
			AssertEquals ("GetExtension #04", null, testExtn);

			testExtn = Path.GetExtension (" ");
			AssertEquals ("GetExtension #05", String.Empty, testExtn);

			testExtn = Path.GetExtension (path1 + ".doc");
			AssertEquals ("GetExtension #06", ".doc", testExtn);

			testExtn = Path.GetExtension (path1 + ".doc" + DSC + "a.txt");
			AssertEquals ("GetExtension #07", ".txt", testExtn);

			if (Windows) {
				try {
					testExtn = Path.GetExtension ("hi<there.txt");
					Fail ("GetExtension Fail #01");
				} catch (Exception e) {
					AssertEquals ("GetExtension Exc. #01", typeof (ArgumentException), e.GetType ());
				}
			}
		}

		public void TestGetFileName ()
		{
			string testFileName = Path.GetFileName (path1);

			AssertEquals ("GetFileName #01", "test.txt", testFileName);
			testFileName = Path.GetFileName (null);
			AssertEquals ("GetFileName #02", null, testFileName);
			testFileName = Path.GetFileName (String.Empty);
			AssertEquals ("GetFileName #03", String.Empty, testFileName);
			testFileName = Path.GetFileName (" ");
			AssertEquals ("GetFileName #04", " ", testFileName);

			if (Windows) {
				try {
					testFileName = Path.GetFileName ("hi<");
					Fail ("GetFileName Fail #01");
				} catch (Exception e) {
					AssertEquals ("GetFileName Exc. #01", typeof (ArgumentException), e.GetType ());
				}
			}
		}

		public void TestGetFileNameWithoutExtension ()
		{
			string testFileName = Path.GetFileNameWithoutExtension (path1);

			AssertEquals ("GetFileNameWithoutExtension #01", "test", testFileName);

			testFileName = Path.GetFileNameWithoutExtension (null);
			AssertEquals ("GetFileNameWithoutExtension #02", null, testFileName);

			testFileName = Path.GetFileNameWithoutExtension (String.Empty);
			AssertEquals ("GetFileNameWithoutExtension #03", String.Empty, testFileName);
		}

		public void TestGetFullPath ()
		{
			string current = Directory.GetCurrentDirectory ();

			string testFullPath = Path.GetFullPath ("foo.txt");
			string expected = current + DSC + "foo.txt";
			AssertEquals ("GetFullPath #01", expected, testFullPath);

			testFullPath = Path.GetFullPath ("a//./.././foo.txt");
			AssertEquals ("GetFullPath #02", expected, testFullPath);
			string root = Windows ? "C:\\" : "/";
			string [,] test = new string [,] {		
				{"root////././././././../root/././../root", "root"},
				{"root/", "root/"},
				{"root/./", "root/"},
				{"root/./", "root/"},
				{"root/../", ""},
				{"root/../", ""},
				{"root/../..", ""},
				{"root/.hiddenfile", "root/.hiddenfile"},
				{"root/. /", "root/. /"},
				{"root/.. /", "root/.. /"},
				{"root/..weirdname", "root/..weirdname"},
				{"root/..", ""},
				{"root/../a/b/../../..", ""},
				{"root/./..", ""},
				{"..", ""},
				{".", ""},
				{"root//dir", "root/dir"},
				{"root/.              /", "root/.              /"},
				{"root/..             /", "root/..             /"},
				{"root/      .              /", "root/      .              /"},
				{"root/      ..             /", "root/      ..             /"},
				{"root/./", "root/"},
				{"root/..                      /", "root/..                   /"},
				{".//", ""}
			};
			for (int i = 0; i < test.GetUpperBound (1); i++) {
				AssertEquals (String.Format ("GetFullPath #{0}", i), root + test [i, 1], Path.GetFullPath (root + test [i, 0]));
			}
			
			if (Windows) {
				string uncroot = @"\\server\share\";
				string [,] testunc = new string [,] {		
					{"root////././././././../root/././../root", "root"},
					{"root/", "root/"},
					{"root/./", "root/"},
					{"root/./", "root/"},
					{"root/../", ""},
					{"root/../", ""},
					{"root/../..", ""},
					{"root/.hiddenfile", "root/.hiddenfile"},
					{"root/. /", "root/. /"},
					{"root/.. /", "root/.. /"},
					{"root/..weirdname", "root/..weirdname"},
					{"root/..", ""},
					{"root/../a/b/../../..", ""},
					{"root/./..", ""},
					{"..", ""},
					{".", ""},
					{"root//dir", "root/dir"},
					{"root/.              /", "root/.              /"},
					{"root/..             /", "root/..             /"},
					{"root/      .              /", "root/      .              /"},
					{"root/      ..             /", "root/      ..             /"},
					{"root/./", "root/"},
					{"root/..                      /", "root/..                   /"},
					{".//", ""}
				};
				for (int i = 0; i < test.GetUpperBound (1); i++) {
					AssertEquals (String.Format ("GetFullPath UNC #{0}", i), uncroot + test [i, 1], Path.GetFullPath (uncroot + test [i, 0]));
				}	
			}
			
			try {
				testFullPath = Path.GetFullPath (null);
				Fail ("GetFullPath Fail #01");
			} catch (Exception e) {
				AssertEquals ("GetFullPath Exc. #01", typeof (ArgumentNullException), e.GetType ());
			}

			try {
				testFullPath = Path.GetFullPath (String.Empty);
				Fail ("GetFullPath Fail #02");
			} catch (Exception e) {
				AssertEquals ("GetFullPath Exc. #02", typeof (ArgumentException), e.GetType ());
			}
		}
		
		public void TestGetPathRoot ()
		{
			string current;
			string expected;
			if (!Windows){
				current = Directory.GetCurrentDirectory ();
				expected = current [0].ToString ();
			} else {
				current = @"J:\Some\Strange Directory\Name";
				expected = "J:\\";
			}

			string pathRoot = Path.GetPathRoot (current);
			AssertEquals ("GetPathRoot #01", expected, pathRoot);

			pathRoot = Path.GetPathRoot ("hola");
			AssertEquals ("GetPathRoot #02", String.Empty, pathRoot);

			pathRoot = Path.GetPathRoot (null);
			AssertEquals ("GetPathRoot #03", null, pathRoot);
		}

		public void TestGetTempPath ()
		{
			string getTempPath = Path.GetTempPath ();
			Assert ("GetTempPath #01",  getTempPath != String.Empty);
			Assert ("GetTempPath #02",  Path.IsPathRooted (getTempPath));
		}

		public void TestGetTempFileName ()
		{
			string getTempFileName = null;
			try {
				getTempFileName = Path.GetTempFileName ();
				Assert ("GetTempFileName #01", getTempFileName != String.Empty);
				Assert ("GetTempFileName #02", File.Exists (getTempFileName));
			} finally {
				if (getTempFileName != null && getTempFileName != String.Empty){
					File.Delete (getTempFileName);
				}
			}
		}

		public void TestHasExtension ()
		{
			AssertEquals ("HasExtension #01",  true, Path.HasExtension ("foo.txt"));
			AssertEquals ("HasExtension #02",  false, Path.HasExtension ("foo"));
			AssertEquals ("HasExtension #03",  true, Path.HasExtension (path1));
			AssertEquals ("HasExtension #04",  false, Path.HasExtension (path2));
		}

		public void TestRooted ()
		{
			Assert ("IsPathRooted #01", Path.IsPathRooted (path2));
			Assert ("IsPathRooted #02", !Path.IsPathRooted (path3));
			Assert ("IsPathRooted #03", !Path.IsPathRooted (null));
		}

		public void TestCanonicalizeDots ()
		{
			string current = Path.GetFullPath (".");
			Assert ("TestCanonicalizeDotst #01", !current.EndsWith ("."));
			string parent = Path.GetFullPath ("..");
			Assert ("TestCanonicalizeDotst #02", !current.EndsWith (".."));
		}
#if !NUNIT
		void Assert (string msg, bool result)
		{
			if (!result)
				Console.WriteLine (msg);
		}

		void AssertEquals (string msg, object expected, object real)
		{
			if (expected == null && real == null)
				return;

			if (expected != null && expected.Equals (real))
				return;

			Console.WriteLine ("{0}: expected: '{1}', got: '{2}'", msg, expected, real);
		}

		void Fail (string msg)
		{
			Console.WriteLine ("Failed: {0}", msg);
		}

		static void Main ()
		{
			PathTest p = new PathTest ();
			Type t = p.GetType ();
			MethodInfo [] methods = t.GetMethods ();
			foreach (MethodInfo m in methods) {
				if (m.Name.Substring (0, 4) == "Test") {
					m.Invoke (p, null);
				}
			}
		}
#endif
	}
}

