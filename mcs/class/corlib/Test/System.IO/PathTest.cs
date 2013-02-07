//
// System.IO.Path Test Cases
//
// Authors:
// 	Marcin Szczepanski (marcins@zipworld.com.au)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Ben Maurer (bmaurer@users.sf.net)
//	Gilles Freart (gfr@skynet.be)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) Marcin Szczepanski 
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Ben Maurer
// (c) 2003 Gilles Freart
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
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

	[TestFixture]
	public class PathTest
	{
		static string path1;
		static string path2;
		static string path3;
		static OsType OS;
		static char DSC = Path.DirectorySeparatorChar;

		[SetUp]
		public void SetUp ()
		{
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

		[Test]
		public void ChangeExtension ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/foo/test.doc";
			files [(int) OsType.Windows] = "c:\\foo\\test.doc";
			files [(int) OsType.Mac] = "foo:test.doc";

			string testPath = Path.ChangeExtension (path1, "doc");
			Assert.AreEqual (files [(int) OS], testPath, "ChangeExtension #01");

			testPath = Path.ChangeExtension (String.Empty, ".extension");
			Assert.AreEqual (String.Empty, testPath, "ChangeExtension #02");

			testPath = Path.ChangeExtension (null, ".extension");
			Assert.AreEqual (null, testPath, "ChangeExtension #03");

			testPath = Path.ChangeExtension ("path", null);
			Assert.AreEqual ("path", testPath, "ChangeExtension #04");

			testPath = Path.ChangeExtension ("path.ext", "doc");
			Assert.AreEqual ("path.doc", testPath, "ChangeExtension #05");

			testPath = Path.ChangeExtension ("path.ext1.ext2", "doc");
			Assert.AreEqual ("path.ext1.doc", testPath, "ChangeExtension #06");

			testPath = Path.ChangeExtension ("hogehoge.xml", ".xsl");
			Assert.AreEqual ("hogehoge.xsl", testPath, "ChangeExtension #07");
			testPath = Path.ChangeExtension ("hogehoge", ".xsl");
			Assert.AreEqual ("hogehoge.xsl", testPath, "ChangeExtension #08");
			testPath = Path.ChangeExtension ("hogehoge.xml", "xsl");
			Assert.AreEqual ("hogehoge.xsl", testPath, "ChangeExtension #09");
			testPath = Path.ChangeExtension ("hogehoge", "xsl");
			Assert.AreEqual ("hogehoge.xsl", testPath, "ChangeExtension #10");
			testPath = Path.ChangeExtension ("hogehoge.xml", String.Empty);
			Assert.AreEqual ("hogehoge.", testPath, "ChangeExtension #11");
			testPath = Path.ChangeExtension ("hogehoge", String.Empty);
			Assert.AreEqual ("hogehoge.", testPath, "ChangeExtension #12");
			testPath = Path.ChangeExtension ("hogehoge.", null);
			Assert.AreEqual ("hogehoge", testPath, "ChangeExtension #13");
			testPath = Path.ChangeExtension ("hogehoge", null);
			Assert.AreEqual ("hogehoge", testPath, "ChangeExtension #14");
			testPath = Path.ChangeExtension (String.Empty, null);
			Assert.AreEqual (String.Empty, testPath, "ChangeExtension #15");
			testPath = Path.ChangeExtension (String.Empty, "bashrc");
			Assert.AreEqual (String.Empty, testPath, "ChangeExtension #16");
			testPath = Path.ChangeExtension (String.Empty, ".bashrc");
			Assert.AreEqual (String.Empty, testPath, "ChangeExtension #17");
			testPath = Path.ChangeExtension (null, null);
			Assert.IsNull (testPath, "ChangeExtension #18");
		}

		[Test]
		public void ChangeExtension_Extension_InvalidPathChars () 
		{
			string fn = Path.ChangeExtension ("file.ext", "<");
			Assert.AreEqual ("file.<", fn, "Invalid filename");
		}

		[Test]
		public void ChangeExtension_Path_InvalidPathChars ()
		{
			try {
				Path.ChangeExtension ("fi\0le.ext", ".extension");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Combine ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/etc/init.d";
			files [(int) OsType.Windows] = Environment.GetEnvironmentVariable ("SYSTEMROOT") + @"\system32";
			files [(int) OsType.Mac] = "foo:bar";

			string testPath = Path.Combine (path2, path3);
			Assert.AreEqual (files [(int) OS], testPath, "Combine #01");

			testPath = Path.Combine ("one", String.Empty);
			Assert.AreEqual ("one", testPath, "Combine #02");

			testPath = Path.Combine (String.Empty, "one");
			Assert.AreEqual ("one", testPath, "Combine #03");

			string current = Directory.GetCurrentDirectory ();
			testPath = Path.Combine (current, "one");

			string expected = current + DSC + "one";
			Assert.AreEqual (expected, testPath, "Combine #04");

			testPath = Path.Combine ("one", current);
			// LAMESPEC noted in Path.cs
			Assert.AreEqual (current, testPath, "Combine #05");

			testPath = Path.Combine (current, expected);
			Assert.AreEqual (expected, testPath, "Combine #06");

			testPath = DSC + "one";
			testPath = Path.Combine (testPath, "two" + DSC);
			expected = DSC + "one" + DSC + "two" + DSC;
			Assert.AreEqual (expected, testPath, "Combine #06");

			testPath = "one" + DSC;
			testPath = Path.Combine (testPath, DSC + "two");
			expected = DSC + "two";
			Assert.AreEqual (expected, testPath, "Combine #06");

			testPath = "one" + DSC;
			testPath = Path.Combine (testPath, "two" + DSC);
			expected = "one" + DSC + "two" + DSC;
			Assert.AreEqual (expected, testPath, "Combine #07");
		}

		[Test]
		public void Combine_Path1_InvalidPathChars ()
		{
			try {
				Path.Combine ("a\0", "one");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Combine_Path1_Null ()
		{
			try {
				Path.Combine (null, "one");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path1", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Combine_Path2_InvalidPathChars ()
		{
			try {
				Path.Combine ("one", "a\0");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Combine_Path2_Null ()
		{
			try {
				Path.Combine ("one", null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path2", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetDirectoryName ()
		{
			string [] files = new string [3];
			files [(int) OsType.Unix] = "/foo";
			files [(int) OsType.Windows] = "c:\\foo";
			files [(int) OsType.Mac] = "foo";

			string testDirName = Path.GetDirectoryName (path1);
			Assert.AreEqual (files [(int) OS], testDirName, "#A1");
			testDirName = Path.GetDirectoryName (files [(int) OS] + DSC);
			Assert.AreEqual (files [(int) OS], testDirName, "#A2");

			if (Windows) {
				Assert.AreEqual ("C:\\foo", Path.GetDirectoryName ("C:\\foo\\foo.txt"), "#B1");
				Assert.AreEqual (null, Path.GetDirectoryName ("C:"), "#B2");
				Assert.AreEqual (null, Path.GetDirectoryName (@"C:\"), "#B3");
				Assert.AreEqual (@"C:\", Path.GetDirectoryName (@"C:\dir"), "#B4");
				Assert.AreEqual (@"C:\dir", Path.GetDirectoryName (@"C:\dir\"), "#B5");
				Assert.AreEqual (@"C:\dir", Path.GetDirectoryName (@"C:\dir\dir"), "#B6");
				Assert.AreEqual (@"C:\dir\dir", Path.GetDirectoryName (@"C:\dir\dir\"), "#B7");

				Assert.AreEqual ("\\foo\\bar", Path.GetDirectoryName ("/foo//bar/dingus"), "#C1");
				Assert.AreEqual ("foo\\bar", Path.GetDirectoryName ("foo/bar/"), "#C2");
				Assert.AreEqual ("foo\\bar", Path.GetDirectoryName ("foo/bar\\xxx"), "#C3");
				Assert.AreEqual ("\\\\host\\dir\\dir2", Path.GetDirectoryName ("\\\\host\\dir\\\\dir2\\path"), "#C4");

				// UNC tests
				Assert.AreEqual (null, Path.GetDirectoryName (@"\\"), "#D1");
				Assert.AreEqual (null, Path.GetDirectoryName (@"\\server"), "#D2");
				Assert.AreEqual (null, Path.GetDirectoryName (@"\\server\share"), "#D3");
				Assert.AreEqual (@"\\server\share", Path.GetDirectoryName (@"\\server\share\"), "#D4");
				Assert.AreEqual (@"\\server\share", Path.GetDirectoryName (@"\\server\share\dir"), "#D5");
				Assert.AreEqual (@"\\server\share\dir", Path.GetDirectoryName (@"\\server\share\dir\subdir"), "#D6");
			} else {
				Assert.AreEqual ("/etc", Path.GetDirectoryName ("/etc/hostname"), "#B1");
				Assert.AreEqual ("/foo/bar", Path.GetDirectoryName ("/foo//bar/dingus"), "#B2");
				Assert.AreEqual ("foo/bar", Path.GetDirectoryName ("foo/bar/"), "#B3");
				Assert.AreEqual ("/", Path.GetDirectoryName ("/tmp"), "#B4");
				Assert.IsNull (Path.GetDirectoryName ("/"), "#B5");
				Assert.AreEqual ("a", Path.GetDirectoryName ("a//b"), "#B6");
			}
		}

		[Test]
		public void GetDirectoryName_Path_Empty ()
		{
			try {
				Path.GetDirectoryName (String.Empty);
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
		public void GetDirectoryName_Path_InvalidPathChars ()
		{
			try {
				Path.GetDirectoryName ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetDirectoryName_Path_Null ()
		{
			Assert.IsNull (Path.GetDirectoryName (null));
		}

		[Test]
		public void GetDirectoryName_Path_Whitespace ()
		{
			try {
				Path.GetDirectoryName ("   ");
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
		public void GetExtension ()
		{
			string testExtn = Path.GetExtension (path1);

			Assert.AreEqual (".txt", testExtn, "GetExtension #01");

			testExtn = Path.GetExtension (path2);
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #02");

			testExtn = Path.GetExtension (String.Empty);
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #03");

			testExtn = Path.GetExtension (null);
			Assert.AreEqual (null, testExtn, "GetExtension #04");

			testExtn = Path.GetExtension (" ");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #05");

			testExtn = Path.GetExtension (path1 + ".doc");
			Assert.AreEqual (".doc", testExtn, "GetExtension #06");

			testExtn = Path.GetExtension (path1 + ".doc" + DSC + "a.txt");
			Assert.AreEqual (".txt", testExtn, "GetExtension #07");

			testExtn = Path.GetExtension (".");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #08");

			testExtn = Path.GetExtension ("end.");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #09");

			testExtn = Path.GetExtension (".start");
			Assert.AreEqual (".start", testExtn, "GetExtension #10");

			testExtn = Path.GetExtension (".a");
			Assert.AreEqual (".a", testExtn, "GetExtension #11");

			testExtn = Path.GetExtension ("a.");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #12");

			testExtn = Path.GetExtension ("a");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #13");

			testExtn = Path.GetExtension ("makefile");
			Assert.AreEqual (String.Empty, testExtn, "GetExtension #14");
		}

		[Test]
		public void GetExtension_Path_InvalidPathChars ()
		{
			try {
				Path.GetExtension ("hi\0world.txt");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFileName ()
		{
			string testFileName = Path.GetFileName (path1);

			Assert.AreEqual ("test.txt", testFileName, "#1");
			testFileName = Path.GetFileName (null);
			Assert.AreEqual (null, testFileName, "#2");
			testFileName = Path.GetFileName (String.Empty);
			Assert.AreEqual (String.Empty, testFileName, "#3");
			testFileName = Path.GetFileName (" ");
			Assert.AreEqual (" ", testFileName, "#4");
		}

		[Test]
		public void GetFileName_Path_InvalidPathChars ()
		{
			try {
				Path.GetFileName ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFileNameWithoutExtension ()
		{
			string testFileName = Path.GetFileNameWithoutExtension (path1);

			Assert.AreEqual ("test", testFileName, "GetFileNameWithoutExtension #01");

			testFileName = Path.GetFileNameWithoutExtension (null);
			Assert.AreEqual (null, testFileName, "GetFileNameWithoutExtension #02");

			testFileName = Path.GetFileNameWithoutExtension (String.Empty);
			Assert.AreEqual (String.Empty, testFileName, "GetFileNameWithoutExtension #03");
		}

		[Test]
		public void GetFileNameWithoutExtension_Path_InvalidPathChars ()
		{
			try {
				Path.GetFileNameWithoutExtension ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFullPath ()
		{
			string current = Directory.GetCurrentDirectory ();

			string testFullPath = Path.GetFullPath ("foo.txt");
			string expected = current + DSC + "foo.txt";
			Assert.AreEqual (expected, testFullPath, "GetFullPath #01");

			testFullPath = Path.GetFullPath ("a//./.././foo.txt");
			Assert.AreEqual (expected, testFullPath, "GetFullPath #02");
		}

		[Test]
		public void GetFullPath_Unix ()
		{
			if (Windows)
				return;

			string root =  "/";
			string [,] test = new string [,] {
				{"root////././././././../root/././../root", "root"},
				{"root/", "root/"},
				{"root/./", "root/"},
				{"root/./", "root/"},
				{"root/../", String.Empty},
				{"root/../", String.Empty},
				{"root/../..", String.Empty},
				{"root/.hiddenfile", "root/.hiddenfile"},
				{"root/. /", "root/. /"},
				{"root/.. /", "root/.. /"},
				{"root/..weirdname", "root/..weirdname"},
				{"root/..", String.Empty},
				{"root/../a/b/../../..", String.Empty},
				{"root/./..", String.Empty},
				{"..", String.Empty},
				{".", String.Empty},
				{"root//dir", "root/dir"},
				{"root/.              /", "root/.              /"},
				{"root/..             /", "root/..             /"},
				{"root/      .              /", "root/      .              /"},
				{"root/      ..             /", "root/      ..             /"},
				{"root/./", "root/"},
				//ERROR! Paths are trimmed
				// I don't understand this comment^^.
				// No trimming occurs but the paths are not equal. That's why the test fails. Commented out.
				//{"root/..                      /", "root/..                   /"},
				{".//", String.Empty}
			};

			for (int i = 0; i < test.GetUpperBound (0); i++) {
				Assert.AreEqual (root + test [i, 1], Path.GetFullPath (root + test [i, 0]),
						 String.Format ("GetFullPathUnix #{0}", i));
			}

			Assert.AreEqual ("/", Path.GetFullPath ("/"), "#01");
			Assert.AreEqual ("/hey", Path.GetFullPath ("/hey"), "#02");
			Assert.AreEqual (Environment.CurrentDirectory, Path.GetFullPath ("."), "#03");
			Assert.AreEqual (Path.Combine (Environment.CurrentDirectory, "hey"),
					     Path.GetFullPath ("hey"), "#04");
		}

		[Test]
		public void GetFullPath_Windows ()
		{
			if (!Windows)
				return;

			string root =  "C:\\";
			string [,] test = new string [,] {
				{"root////././././././../root/././../root", "root"},
				{"root/", "root\\"},
				{"root/./", "root\\"},
				{"root/./", "root\\"},
				{"root/../", ""},
				{"root/../", ""},
				{"root/../..", ""},
				{"root/.hiddenfile", "root\\.hiddenfile"},
				{"root/. /", "root\\"},
				{"root/.. /", ""},
				{"root/..weirdname", "root\\..weirdname"},
				{"root/..", ""},
				{"root/../a/b/../../..", ""},
				{"root/./..", ""},
				{"..", ""},
				{".", ""},
				{"root//dir", "root\\dir"},
				{"root/.              /", "root\\"},
				{"root/..             /", ""},
#if !NET_2_0
				{"root/      .              /", "root\\"},
				{"root/      ..             /", ""},
#endif
				{"root/./", "root\\"},
				{"root/..                      /", ""},
				{".//", ""}
			};

			for (int i = 0; i < test.GetUpperBound (0); i++) {
				try {
					Assert.AreEqual (root + test [i, 1], Path.GetFullPath (root + test [i, 0]),
							 String.Format ("GetFullPathWindows #{0}", i));
				} catch (Exception ex) { 
					Assert.Fail (String.Format ("GetFullPathWindows #{0} (\"{1}\") failed: {2}", 
						i, root + test [i, 0], ex.GetType ()));
				}
			}

			// UNC tests
			string root2 = @"\\server\share";
			root = @"\\server\share\";
			test = new string [,] {		
				{"root////././././././../root/././../root", "root"},
				{"root/", "root\\"},
				{"root/./", "root\\"},
				{"root/./", "root\\"},
				{"root/../", ""},
				{"root/../", ""},
				{"root/../..", null},
				{"root/.hiddenfile", "root\\.hiddenfile"},
				{"root/. /", "root\\"},
				{"root/.. /", ""},
				{"root/..weirdname", "root\\..weirdname"},
				{"root/..", null},
				{"root/../a/b/../../..", null},
				{"root/./..", null},
				{"..", null},
				{".", null},
				{"root//dir", "root\\dir"},
				{"root/.              /", "root\\"},
				{"root/..             /", ""},
#if !NET_2_0
				{"root/      .              /", "root\\"},
				{"root/      ..             /", ""},
#endif
				{"root/./", "root\\"},
				{"root/..                      /", ""},
				{".//", ""}
			};

			for (int i = 0; i < test.GetUpperBound (0); i++) {
				// "null" means we have to compare against "root2"
				string res = test [i, 1] != null
					? root + test [i, 1]
					: root2;
				try {
					Assert.AreEqual (res, Path.GetFullPath (root + test [i, 0]),
							 String.Format ("GetFullPathWindows UNC #{0}", i));
				} catch (AssertionException) {
					throw;
				} catch (Exception ex) {
					Assert.Fail (String.Format ("GetFullPathWindows UNC #{0} (\"{1}\") failed: {2}",
						i, root + test [i, 0], ex.GetType ()));
				}
			}

			test = new string [,] {		
				{"root////././././././../root/././../root", "root"},
				{"root/", "root\\"},
				{"root/./", "root\\"},
				{"root/./", "root\\"},
				{"root/../", ""},
				{"root/../", ""},
				{"root/../..", null},
				{"root/.hiddenfile", "root\\.hiddenfile"},
				{"root/. /", "root\\"},
				{"root/.. /", ""},
				{"root/..weirdname", "root\\..weirdname"},
				{"root/..", null},
				{"root/../a/b/../../..", null},
				{"root/./..", null},
				{"..", null},
				{".", null},
				{"root//dir", "root\\dir"},
				{"root/.              /", "root\\"},
				{"root/..             /", ""},
#if !NET_2_0
				{"root/      .              /", "root\\"},
				{"root/      ..             /", ""},
#endif
				{"root/./", "root\\"},
				{"root/..                      /", ""},
				{".//", ""}
			};

			string root3 = @"//server/share";
			root = @"//server/share/";
			bool needSlashConvert = Path.DirectorySeparatorChar != '/';

			for (int i = 0; i < test.GetUpperBound (0); i++) {
				// "null" means we have to compare against "root2"
				string res = test [i, 1] != null
					? root + test [i, 1]
					: root3;
				if (needSlashConvert)
					res = res.Replace ('/', Path.DirectorySeparatorChar);
				try {
					Assert.AreEqual (res, Path.GetFullPath (root + test [i, 0]),
							 String.Format ("GetFullPathWindows UNC[2] #{0}", i));
				} catch (AssertionException) {
					throw;
				} catch (Exception ex) {
					Assert.Fail (String.Format ("GetFullPathWindows UNC[2] #{0} (\"{1}\") failed: {2}",
						i, root + test [i, 0], ex.GetType ()));
				}
			}
		}

		[Test]
		public void GetFullPath_Path_Empty ()
		{
			try {
				Path.GetFullPath (String.Empty);
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
		public void GetFullPath_Path_EndingSeparator ()
		{
			string fp = Path.GetFullPath ("something/");
			char end = fp [fp.Length - 1];
			Assert.IsTrue (end == Path.DirectorySeparatorChar);
		}

		[Test]
		public void GetFullPath_Path_InvalidPathChars ()
		{
			try {
				Path.GetFullPath ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFullPath_Path_Null ()
		{
			try {
				Path.GetFullPath (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path", ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetFullPath_Path_Whitespace ()
		{
			try {
				Path.GetFullPath ("  ");
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
		public void GetFullPath2 ()
		{
			if (Windows) {
				Assert.AreEqual (@"Z:\", Path.GetFullPath ("Z:"), "GetFullPath w#01");
#if !TARGET_JVM // Java full (canonical) path always starts with caps drive letter
				Assert.AreEqual (@"c:\abc\def", Path.GetFullPath (@"c:\abc\def"), "GetFullPath w#02");
#endif
				Assert.IsTrue (Path.GetFullPath (@"\").EndsWith (@"\"), "GetFullPath w#03");
				// "\\\\" is not allowed
				Assert.IsTrue (Path.GetFullPath ("/").EndsWith (@"\"), "GetFullPath w#05");
				// "//" is not allowed
				Assert.IsTrue (Path.GetFullPath ("readme.txt").EndsWith (@"\readme.txt"), "GetFullPath w#07");
				Assert.IsTrue (Path.GetFullPath ("c").EndsWith (@"\c"), "GetFullPath w#08");
				Assert.IsTrue (Path.GetFullPath (@"abc\def").EndsWith (@"abc\def"), "GetFullPath w#09");
				Assert.IsTrue (Path.GetFullPath (@"\abc\def").EndsWith (@"\abc\def"), "GetFullPath w#10");
				Assert.AreEqual (@"\\abc\def", Path.GetFullPath (@"\\abc\def"), "GetFullPath w#11");
				Assert.AreEqual (Directory.GetCurrentDirectory () + @"\abc\def", Path.GetFullPath (@"abc//def"), "GetFullPath w#12");
				Assert.AreEqual (Directory.GetCurrentDirectory ().Substring (0, 2) + @"\abc\def", Path.GetFullPath ("/abc/def"), "GetFullPath w#13");
				Assert.AreEqual (@"\\abc\def", Path.GetFullPath ("//abc/def"), "GetFullPath w#14");
			} else {
				Assert.AreEqual ("/", Path.GetFullPath ("/"), "#01");
				Assert.AreEqual ("/hey", Path.GetFullPath ("/hey"), "#02");
				Assert.AreEqual (Environment.CurrentDirectory, Path.GetFullPath ("."), "#03");
				Assert.AreEqual (Path.Combine (Environment.CurrentDirectory, "hey"),
						     Path.GetFullPath ("hey"), "#04");
			}
		}

		[Test]
		public void GetPathRoot ()
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
			Assert.AreEqual (expected, pathRoot, "GetPathRoot #01");
		}

		[Test]
		public void GetPathRoot2 ()
		{
			// note: this method doesn't call Directory.GetCurrentDirectory so it can be
			// reused for partial trust unit tests in PathCas.cs

			string pathRoot;
			
			pathRoot = Path.GetPathRoot ("hola");
			Assert.AreEqual (String.Empty, pathRoot, "#A1");
			pathRoot = Path.GetPathRoot (null);
			Assert.AreEqual (null, pathRoot, "#A2");

			if (Windows) {
				Assert.AreEqual ("z:", Path.GetPathRoot ("z:"), "GetPathRoot w#01");
				Assert.AreEqual ("c:\\", Path.GetPathRoot ("c:\\abc\\def"), "GetPathRoot w#02");
				Assert.AreEqual ("\\", Path.GetPathRoot ("\\"), "GetPathRoot w#03");
				Assert.AreEqual ("\\\\", Path.GetPathRoot ("\\\\"), "GetPathRoot w#04");
				Assert.AreEqual ("\\", Path.GetPathRoot ("/"), "GetPathRoot w#05");
				Assert.AreEqual ("\\\\", Path.GetPathRoot ("//"), "GetPathRoot w#06");
				Assert.AreEqual (String.Empty, Path.GetPathRoot ("readme.txt"), "GetPathRoot w#07");
				Assert.AreEqual (String.Empty, Path.GetPathRoot ("c"), "GetPathRoot w#08");
				Assert.AreEqual (String.Empty, Path.GetPathRoot ("abc\\def"), "GetPathRoot w#09");
				Assert.AreEqual ("\\", Path.GetPathRoot ("\\abc\\def"), "GetPathRoot w#10");
				Assert.AreEqual ("\\\\abc\\def", Path.GetPathRoot ("\\\\abc\\def"), "GetPathRoot w#11");
				Assert.AreEqual (String.Empty, Path.GetPathRoot ("abc//def"), "GetPathRoot w#12");
				Assert.AreEqual ("\\", Path.GetPathRoot ("/abc/def"), "GetPathRoot w#13");
				Assert.AreEqual ("\\\\abc\\def", Path.GetPathRoot ("//abc/def"), "GetPathRoot w#14");
				Assert.AreEqual (@"C:\", Path.GetPathRoot (@"C:\"), "GetPathRoot w#15");
				Assert.AreEqual (@"C:\", Path.GetPathRoot (@"C:\\"), "GetPathRoot w#16");
				Assert.AreEqual ("\\\\abc\\def", Path.GetPathRoot ("\\\\abc\\def\\ghi"), "GetPathRoot w#17");
			} else {
				// TODO: Same tests for Unix.
			}
		}

		[Test]
		public void GetPathRoot_Path_Empty ()
		{
			try {
				Path.GetPathRoot (String.Empty);
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
#if ONLY_1_1
		[Category ("NotWorking")] // we also throw ArgumentException on 1.0 profile
#endif
		public void GetPathRoot_Path_InvalidPathChars ()
		{
#if NET_2_0
			try {
				Path.GetPathRoot ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#else
			Assert.AreEqual (String.Empty, Path.GetPathRoot ("hi\0world"));
#endif
		}

		[Test]
		public void GetPathRoot_Path_Whitespace ()
		{
			try {
				Path.GetPathRoot ("  ");
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
		public void GetTempPath ()
		{
			string getTempPath = Path.GetTempPath ();
			Assert.IsTrue (getTempPath != String.Empty, "GetTempPath #01");
			Assert.IsTrue (Path.IsPathRooted (getTempPath), "GetTempPath #02");
			Assert.AreEqual (Path.DirectorySeparatorChar, getTempPath [getTempPath.Length - 1], "GetTempPath #03");
		}

		[Test]
		public void GetTempFileName ()
		{
			string getTempFileName = null;
			try {
				getTempFileName = Path.GetTempFileName ();
				Assert.IsTrue (getTempFileName != String.Empty, "GetTempFileName #01");
				Assert.IsTrue (File.Exists (getTempFileName), "GetTempFileName #02");
			} finally {
				if (getTempFileName != null && getTempFileName != String.Empty){
					File.Delete (getTempFileName);
				}
			}
		}

		[Test]
		public void HasExtension ()
		{
			Assert.AreEqual (true, Path.HasExtension ("foo.txt"), "HasExtension #01");
			Assert.AreEqual (false, Path.HasExtension ("foo"), "HasExtension #02");
			Assert.AreEqual (true, Path.HasExtension (path1), "HasExtension #03");
			Assert.AreEqual (false, Path.HasExtension (path2), "HasExtension #04");
			Assert.AreEqual (false, Path.HasExtension (null), "HasExtension #05");
			Assert.AreEqual (false, Path.HasExtension (String.Empty), "HasExtension #06");
			Assert.AreEqual (false, Path.HasExtension (" "), "HasExtension #07");
			Assert.AreEqual (false, Path.HasExtension ("."), "HasExtension #08");
			Assert.AreEqual (false, Path.HasExtension ("end."), "HasExtension #09");
			Assert.AreEqual (true, Path.HasExtension (".start"), "HasExtension #10");
			Assert.AreEqual (true, Path.HasExtension (".a"), "HasExtension #11");
			Assert.AreEqual (false, Path.HasExtension ("a."), "HasExtension #12");
			Assert.AreEqual (false, Path.HasExtension ("Makefile"), "HasExtension #13");
		}

		[Test]
		public void HasExtension_Path_InvalidPathChars ()
		{
			try {
				Path.HasExtension ("hi\0world.txt");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void IsPathRooted ()
		{
			Assert.IsTrue (Path.IsPathRooted (path2), "IsPathRooted #01");
			Assert.IsTrue (!Path.IsPathRooted (path3), "IsPathRooted #02");
			Assert.IsTrue (!Path.IsPathRooted (null), "IsPathRooted #03");
			Assert.IsTrue (!Path.IsPathRooted (String.Empty), "IsPathRooted #04");
			Assert.IsTrue (!Path.IsPathRooted (" "), "IsPathRooted #05");
			Assert.IsTrue (Path.IsPathRooted ("/"), "IsPathRooted #06");
			Assert.IsTrue (Path.IsPathRooted ("//"), "IsPathRooted #07");
			Assert.IsTrue (!Path.IsPathRooted (":"), "IsPathRooted #08");

			if (Windows) {
				Assert.IsTrue (Path.IsPathRooted ("\\"), "IsPathRooted #09");
				Assert.IsTrue (Path.IsPathRooted ("\\\\"), "IsPathRooted #10");
				Assert.IsTrue (Path.IsPathRooted ("z:"), "IsPathRooted #11");
				Assert.IsTrue (Path.IsPathRooted ("z:\\"), "IsPathRooted #12");
				Assert.IsTrue (Path.IsPathRooted ("z:\\topdir"), "IsPathRooted #13");
				// This looks MS BUG. It is treated as absolute path
				Assert.IsTrue (Path.IsPathRooted ("z:curdir"), "IsPathRooted #14");
				Assert.IsTrue (Path.IsPathRooted ("\\abc\\def"), "IsPathRooted #15");
			} else {
				if (Environment.GetEnvironmentVariable ("MONO_IOMAP") == "all"){
					Assert.IsTrue (Path.IsPathRooted ("\\"), "IsPathRooted #16");
					Assert.IsTrue (Path.IsPathRooted ("\\\\"), "IsPathRooted #17");
				} else {
					Assert.IsTrue (!Path.IsPathRooted ("\\"), "IsPathRooted #09");
					Assert.IsTrue (!Path.IsPathRooted ("\\\\"), "IsPathRooted #10");
					Assert.IsTrue (!Path.IsPathRooted ("z:"), "IsPathRooted #11");
				}
			}
		}

		[Test]
		public void IsPathRooted_Path_Empty ()
		{
			Assert.IsTrue (!Path.IsPathRooted (String.Empty));
		}

		[Test]
		public void IsPathRooted_Path_InvalidPathChars ()
		{
			try {
				Path.IsPathRooted ("hi\0world");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Illegal characters in path.
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void IsPathRooted_Path_Null ()
		{
			Assert.IsTrue (!Path.IsPathRooted (null));
		}

		[Test]
		public void IsPathRooted_Path_Whitespace ()
		{
			Assert.IsTrue (!Path.IsPathRooted ("  "));
		}

		[Test]
		public void CanonicalizeDots ()
		{
			string current = Path.GetFullPath (".");
			Assert.IsTrue (!current.EndsWith ("."), "TestCanonicalizeDotst #01");
			string parent = Path.GetFullPath ("..");
			Assert.IsTrue (!current.EndsWith (".."), "TestCanonicalizeDotst #02");
		}
#if !MOBILE
		[Test]
		public void WindowsSystem32_76191 ()
		{
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128) || (platform == 6))
				return;

			string curdir = Directory.GetCurrentDirectory ();
			try {
#if TARGET_JVM
				string system = "C:\\WINDOWS\\system32\\";
#else
				string system = Environment.SystemDirectory;
#endif
				Directory.SetCurrentDirectory (system);
				string drive = system.Substring (0, 2);
				Assert.AreEqual (system, Path.GetFullPath (drive), "current dir");
			}
			finally {
				Directory.SetCurrentDirectory (curdir);
			}
		}

		[Test]
		public void WindowsSystem32_77007 ()
		{
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128) || (platform == 6))
				return;

			string curdir = Directory.GetCurrentDirectory ();
			try {
#if TARGET_JVM
				string system = "C:\\WINDOWS\\system32\\";
#else
				string system = Environment.SystemDirectory;
#endif
				Directory.SetCurrentDirectory (system);
				// e.g. C:dir (no backslash) will return CurrentDirectory + dir
				string dir = system.Substring (0, 2) + "dir";
				Assert.AreEqual (Path.Combine (system, "dir"), Path.GetFullPath (dir), "current dir");
			}
			finally {
				Directory.SetCurrentDirectory (curdir);
			}
		}
#endif
		[Test]
#if TARGET_JVM
		[Ignore("Java full (canonical) path always returns windows dir in caps")]
#endif
		public void WindowsDriveC14N_77058 ()
		{
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128) || (platform == 6))
				return;

			Assert.AreEqual (@"C:\Windows\dir", Path.GetFullPath (@"C:\Windows\System32\..\dir"), "1");
			Assert.AreEqual (@"C:\dir", Path.GetFullPath (@"C:\Windows\System32\..\..\dir"), "2");
			Assert.AreEqual (@"C:\dir", Path.GetFullPath (@"C:\Windows\System32\..\..\..\dir"), "3");
			Assert.AreEqual (@"C:\dir", Path.GetFullPath (@"C:\Windows\System32\..\..\..\..\dir"), "4");
			Assert.AreEqual (@"C:\dir\", Path.GetFullPath (@"C:\Windows\System32\..\.\..\.\..\dir\"), "5");
		}

		[Test]
		public void InvalidPathChars_Values ()
		{
			char[] invalid = Path.InvalidPathChars;
			if (Windows) {
#if NET_2_0
				Assert.AreEqual (36, invalid.Length, "Length");
#else
				Assert.AreEqual (15, invalid.Length, "Length");
#endif
				foreach (char c in invalid) {
					int i = (int) c;
#if NET_2_0
					if (i < 32)
						continue;
#else
					if ((i == 0) || (i == 8) || ((i > 15) && (i < 19)) || ((i > 19) && (i < 26)))
						continue;
#endif
					// in both 1.1 SP1 and 2.0
					if ((i == 34) || (i == 60) || (i == 62) || (i == 124))
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			} else {
				foreach (char c in invalid) {
					int i = (int) c;
					if (i == 0)
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			}
		}

		[Test]
		public void InvalidPathChars_Modify ()
		{
			char[] expected = Path.InvalidPathChars;
			char[] invalid = Path.InvalidPathChars;
			char original = invalid[0];
			try {
				invalid[0] = 'a';
				// kind of scary
				Assert.IsTrue (expected[0] == 'a', "expected");
				Assert.AreEqual (expected[0], Path.InvalidPathChars[0], "readonly");
			} finally {
				invalid[0] = original;
			}
		}

#if NET_2_0
		[Test]
		public void GetInvalidFileNameChars_Values ()
		{
			char[] invalid = Path.GetInvalidFileNameChars ();
			if (Windows) {
				Assert.AreEqual (41, invalid.Length);
				foreach (char c in invalid) {
					int i = (int) c;
					if (i < 32)
						continue;
					if ((i == 34) || (i == 60) || (i == 62) || (i == 124))
						continue;
					// ':', '*', '?', '\', '/'
					if ((i == 58) || (i == 42) || (i == 63) || (i == 92) || (i == 47))
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			} else {
				foreach (char c in invalid) {
					int i = (int) c;
					// null or '/'
					if ((i == 0) || (i == 47))
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			}
		}

		[Test]
		public void GetInvalidFileNameChars_Modify ()
		{
			char[] expected = Path.GetInvalidFileNameChars ();
			char[] invalid = Path.GetInvalidFileNameChars ();
			invalid[0] = 'a';
			Assert.IsTrue (expected[0] != 'a', "expected");
			Assert.AreEqual (expected[0], Path.GetInvalidFileNameChars ()[0], "readonly");
		}

		[Test]
		public void GetInvalidPathChars_Values ()
		{
			char[] invalid = Path.GetInvalidPathChars ();
			if (Windows) {
				Assert.AreEqual (36, invalid.Length);
				foreach (char c in invalid) {
					int i = (int) c;
					if (i < 32)
						continue;
					if ((i == 34) || (i == 60) || (i == 62) || (i == 124))
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			} else {
				foreach (char c in invalid) {
					int i = (int) c;
					if (i == 0)
						continue;
					Assert.Fail (String.Format ("'{0}' (#{1}) is invalid", c, i));
				}
			}
		}

		[Test]
		public void GetInvalidPathChars_Order ()
		{
			if (Windows) {
				char [] invalid = Path.GetInvalidPathChars ();
				char [] expected = new char [36] { '\x22', '\x3C', '\x3E', '\x7C', '\x00', '\x01', '\x02',
					'\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D',
					'\x0E', '\x0F', '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18',
					'\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F' };
				Assert.AreEqual (expected.Length, invalid.Length);
				for (int i = 0; i < expected.Length; i++ ) {
					Assert.AreEqual (expected [i], invalid [i], "Character at position " + i);
				}
			}
		}

		[Test]
		public void GetInvalidPathChars_Modify ()
		{
			char[] expected = Path.GetInvalidPathChars ();
			char[] invalid = Path.GetInvalidPathChars ();
			invalid[0] = 'a';
			Assert.IsTrue (expected[0] != 'a', "expected");
			Assert.AreEqual (expected[0], Path.GetInvalidPathChars ()[0], "readonly");
		}

		[Test]
		public void GetRandomFileName ()
		{
			string s = Path.GetRandomFileName ();
			Assert.AreEqual (12, s.Length, "Length");
			char[] invalid = Path.GetInvalidFileNameChars ();
			for (int i=0; i < s.Length; i++) {
				if (i == 8)
					Assert.AreEqual ('.', s[i], "8");
				else
					Assert.IsTrue (Array.IndexOf (invalid, s[i]) == -1, i.ToString ());
			}
		}

		[Test]
		public void GetRandomFileNameIsAlphaNumerical ()
		{
			string [] names = new string [1000];
			for (int i = 0; i < names.Length; i++)
				names [i] = Path.GetRandomFileName ();

			foreach (string name in names) {
				Assert.AreEqual (12, name.Length);
				Assert.AreEqual ('.', name [8]);

				for (int i = 0; i < 12; i++) {
					if (i == 8)
						continue;

					char c = name [i];
					Assert.IsTrue (('a' <= c && c <= 'z') || ('0' <= c && c <= '9'));
				}
			}
		}
#endif
#if NET_4_0
		string Concat (string sep, params string [] parms)
		{
			return String.Join (sep, parms);
		}

		[Test]
		public void Combine_3Params ()
		{
			string sep = Path.DirectorySeparatorChar.ToString ();

			try {
				Path.Combine (null, "two", "three");
				Assert.Fail ("#A1-1");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", null, "three");
				Assert.Fail ("#A1-2");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", null);
				Assert.Fail ("#A1-3");
			} catch {
				// success
			}
			
			Assert.AreEqual (Concat (sep, "one", "two", "three"), Path.Combine ("one", "two", "three"), "#A2-1");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three"), Path.Combine (sep + "one", "two", "three"), "#A2-2");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three"), Path.Combine (sep + "one" + sep, "two", "three"), "#A2-3");
			Assert.AreEqual (Concat (sep, sep + "two", "three"), Path.Combine (sep + "one" + sep, sep + "two", "three"), "#A2-4");
			Assert.AreEqual (Concat (sep, sep + "three"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three"), "#A2-5");

			Assert.AreEqual (Concat (sep, sep + "one" + sep, "two", "three"), Path.Combine (sep + "one" + sep + sep, "two", "three"), "#A3");

			Assert.AreEqual ("", Path.Combine ("", "", ""), "#A4");
		}

		[Test]
		public void Combine_4Params ()
		{
			string sep = Path.DirectorySeparatorChar.ToString ();

			try {
				Path.Combine (null, "two", "three", "four");
				Assert.Fail ("#A1-1");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", null, "three", "four");
				Assert.Fail ("#A1-2");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", null, "four");
				Assert.Fail ("#A1-3");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", "three", null);
				Assert.Fail ("#A1-4");
			} catch {
				// success
			}

			Assert.AreEqual (Concat (sep, "one", "two", "three", "four"), Path.Combine ("one", "two", "three", "four"), "#A2-1");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three", "four"), Path.Combine (sep + "one", "two", "three", "four"), "#A2-2");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three", "four"), Path.Combine (sep + "one" + sep, "two", "three", "four"), "#A2-3");
			Assert.AreEqual (Concat (sep, sep + "two", "three", "four"), Path.Combine (sep + "one" + sep, sep + "two", "three", "four"), "#A2-4");
			Assert.AreEqual (Concat (sep, sep + "three", "four"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three", "four"), "#A2-5");
			Assert.AreEqual (Concat (sep, sep + "four"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three", sep + "four"), "#A2-6");

			Assert.AreEqual (Concat (sep, sep + "one" + sep, "two", "three", "four"), Path.Combine (sep + "one" + sep + sep, "two", "three", "four"), "#A3");

			Assert.AreEqual ("", Path.Combine ("", "", "", ""), "#A4");
		}

		[Test]
		public void Combine_ManyParams ()
		{
			string sep = Path.DirectorySeparatorChar.ToString ();

			try {
				Path.Combine (null, "two", "three", "four", "five");
				Assert.Fail ("#A1-1");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", null, "three", "four", "five");
				Assert.Fail ("#A1-2");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", null, "four", "five");
				Assert.Fail ("#A1-3");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", "three", null, "five");
				Assert.Fail ("#A1-4");
			} catch {
				// success
			}

			try {
				Path.Combine ("one", "two", "three", "four", null);
				Assert.Fail ("#A1-5");
			} catch {
				// success
			}

			Assert.AreEqual (Concat (sep, "one", "two", "three", "four", "five"), Path.Combine ("one", "two", "three", "four", "five"), "#A2-1");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three", "four", "five"), Path.Combine (sep + "one", "two", "three", "four", "five"), "#A2-2");
			Assert.AreEqual (Concat (sep, sep + "one", "two", "three", "four", "five"), Path.Combine (sep + "one" + sep, "two", "three", "four", "five"), "#A2-3");
			Assert.AreEqual (Concat (sep, sep + "two", "three", "four", "five"), Path.Combine (sep + "one" + sep, sep + "two", "three", "four", "five"), "#A2-4");
			Assert.AreEqual (Concat (sep, sep + "three", "four", "five"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three", "four", "five"), "#A2-5");
			Assert.AreEqual (Concat (sep, sep + "four", "five"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three", sep + "four", "five"), "#A2-6");
			Assert.AreEqual (Concat (sep, sep + "five"), Path.Combine (sep + "one" + sep, sep + "two", sep + "three", sep + "four", sep + "five"), "#A2-6");

			Assert.AreEqual (Concat (sep, sep + "one" + sep, "two", "three", "four", "five"), Path.Combine (sep + "one" + sep + sep, "two", "three", "four", "five"), "#A3");

			Assert.AreEqual ("", Path.Combine ("", "", "", "", ""), "#A4");
		}
#endif
	}
}

