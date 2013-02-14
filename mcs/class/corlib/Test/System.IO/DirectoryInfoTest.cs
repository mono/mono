// DirectoryInfoTest.cs - NUnit Test Cases for System.IO.DirectoryInfo class
//
// Authors
//	Ville Palo (vi64pa@koti.soon.fi)
//	Sebastien Pouliot  <sebastien@ximian.com>
// 
// (C) 2003 Ville Palo
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// 

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class DirectoryInfoTest
	{
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		static readonly char DSC = Path.DirectorySeparatorChar;
		string current;

		[SetUp]
		protected void SetUp ()
		{
			current = Directory.GetCurrentDirectory ();
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		protected void TearDown ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.SetCurrentDirectory (current);
		}

		[Test] // ctor (String)
		public void Constructor1 ()
		{
			string path = TempFolder + DSC + "DIT.Ctr.Test";
			DeleteDir (path);

			DirectoryInfo info = new DirectoryInfo (path);
			Assert.AreEqual ("DIT.Ctr.Test", info.Name, "#1");
			Assert.IsFalse (info.Exists, "#2");
			Assert.AreEqual (".Test", info.Extension, "#3");
			Assert.AreEqual ("DIT.Ctr.Test", info.Name, "#4");
		}

		[Test] // ctor (String)
		public void Constructor1_Path_Null ()
		{
			try {
				new DirectoryInfo (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_Path_Empty ()
		{
			try {
				new DirectoryInfo (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty file name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_Path_Whitespace ()
		{
			try {
				new DirectoryInfo ("   ");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The path is not of a legal form
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor1_Path_InvalidPathChars ()
		{
			string path = string.Empty;
			foreach (char c in Path.InvalidPathChars)
				path += c;
			try {
				new DirectoryInfo (path);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The path contains illegal characters
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void Exists ()
		{
			string path = TempFolder + DSC + "DIT.Exists.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.IsFalse (info.Exists, "#1");

				Directory.CreateDirectory (path);
				Assert.IsFalse (info.Exists, "#2");
				info = new DirectoryInfo (path);
				Assert.IsTrue (info.Exists, "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Name ()
		{
			string path = TempFolder + DSC + "DIT.Name.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.AreEqual ("DIT.Name.Test", info.Name, "#1");

				info = Directory.CreateDirectory (path);
				Assert.AreEqual ("DIT.Name.Test", info.Name, "#2");

				info = Directory.CreateDirectory ("whatever");
				Assert.AreEqual ("whatever", info.Name, "#3");

				if (RunningOnUnix) {
					info = new DirectoryInfo ("/");
					Assert.AreEqual ("/", info.Name, "#4");

					info = new DirectoryInfo ("test/");
					Assert.AreEqual ("test", info.Name, "#5");

					info = new DirectoryInfo ("/test");
					Assert.AreEqual ("test", info.Name, "#4");

					info = new DirectoryInfo ("/test/");
					Assert.AreEqual ("test", info.Name, "#4");
				} else {
					info = new DirectoryInfo (@"c:");
					Assert.AreEqual (@"C:\", info.Name, "#4");

					info = new DirectoryInfo (@"c:\");
					Assert.AreEqual (@"c:\", info.Name, "#5");

					info = new DirectoryInfo (@"c:\test");
					Assert.AreEqual ("test", info.Name, "#6");

					info = new DirectoryInfo (@"c:\test\");
					Assert.AreEqual ("test", info.Name, "#7");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void Parent ()
		{
			string path = TempFolder + DSC + "DIT.Parent.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.AreEqual ("MonoTests.System.IO.Tests", info.Parent.Name, "#1");

				info = Directory.CreateDirectory (path);
				Assert.AreEqual ("MonoTests.System.IO.Tests", info.Parent.Name, "#2");

				info = new DirectoryInfo ("test");
				Assert.AreEqual (Directory.GetCurrentDirectory (), info.Parent.FullName, "#3");

				if (RunningOnUnix) {
					info = new DirectoryInfo ("/");
					Assert.IsNull (info.Parent, "#4");

					info = new DirectoryInfo ("test/");
					Assert.IsNotNull (info.Parent, "#5a");
					Assert.AreEqual (Directory.GetCurrentDirectory (), info.Parent.FullName, "#5b");

					info = new DirectoryInfo ("/test");
					Assert.IsNotNull (info.Parent, "#6a");
					Assert.AreEqual ("/", info.Parent.FullName, "#6b");

					info = new DirectoryInfo ("/test/");
					Assert.IsNotNull (info.Parent, "#7a");
					Assert.AreEqual ("/", info.Parent.FullName, "#7b");
				} else {
					info = new DirectoryInfo (@"c:");
					Assert.IsNull (info.Parent, "#4");

					info = new DirectoryInfo (@"c:\");
					Assert.IsNull (info.Parent, "#5");

					info = new DirectoryInfo (@"c:\test");
					Assert.IsNotNull (info.Parent, "#6a");
					Assert.AreEqual (@"c:\", info.Parent.FullName, "#6b");

					info = new DirectoryInfo (@"c:\test\");
					Assert.IsNotNull (info.Parent, "#7a");
					Assert.AreEqual (@"c:\", info.Parent.FullName, "#7b");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void Create ()
		{
			string path = TempFolder + DSC + "DIT.Create.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.IsFalse (info.Exists, "#1");
				info.Create ();
				Assert.IsFalse (info.Exists, "#2");
				info = new DirectoryInfo (path);
				Assert.IsTrue (info.Exists, "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void CreateSubdirectory ()
		{
			string sub_path = Path.Combine ("test01", "test02");
			try {
				DirectoryInfo info = new DirectoryInfo (TempFolder);
				DirectoryInfo sub = info.CreateSubdirectory (sub_path);
				Assert.IsNotNull (sub, "#1");
				Assert.AreEqual (Path.Combine (TempFolder, sub_path), sub.FullName, "#2");
				Assert.IsTrue (Directory.Exists (sub.FullName), "#3");
			} finally {
				DeleteDir (Path.Combine (TempFolder, sub_path));
			}
		}

		[Test]
		public void CreateSubdirectory_Path_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.CreateSubdirectory (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path", ex.ParamName, "#5");
			}
		}
		
		[Test]
		public void CreateSubdirectory_Path_Empty ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.CreateSubdirectory (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty file name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Delete ()
		public void Delete1 ()
		{
			string path = TempFolder + DSC + "DIT.Delete1.Test";
			DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.IsTrue (info.Exists, "#1");
				
				info.Delete ();
				Assert.IsTrue (info.Exists, "#2");
				
				info = new DirectoryInfo (path);
				Assert.IsFalse (info.Exists, "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // Delete ()
		public void Delete1_DirectoryNotEmpty ()
		{
			string path = TempFolder + DSC + "DIT.DeleteIOException1.Test";
			DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				try {
					info.Delete ();
					Assert.Fail ("#1");
				} catch (IOException ex) {
					// The directory is not empty
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // Delete (Boolean)
		public void Delete2 ()
		{
			string path = TempFolder + DSC + "DIT.Delete2.Test";
			DeleteDir (path);

			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				Assert.IsTrue (info.Exists, "#1");

				info.Delete (true);
				Assert.IsTrue (info.Exists, "#2");

				info = new DirectoryInfo (path);
				Assert.IsFalse (info.Exists, "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // Delete (Boolean)
		public void Delete2_DirectoryNotEmpty ()
		{
			string path = TempFolder + DSC + "DIT.DeleteIOException2.Test";
			DeleteDir (path);
			
			try {
				Directory.CreateDirectory (path);
				File.Create (path + DSC + "test").Close ();
				DirectoryInfo info = new DirectoryInfo (path);
				try {
					info.Delete (false);
					Assert.Fail ("#1");
				} catch (IOException ex) {
					// The directory is not empty
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // bug #75443
		public void FullName ()
		{
			DirectoryInfo di = new DirectoryInfo ("something");
			Assert.IsFalse (di.Exists, "#A1");
			Assert.AreEqual (Path.Combine (Directory.GetCurrentDirectory (), "something"), di.FullName, "#A2");

			di = new DirectoryInfo ("something" + Path.DirectorySeparatorChar);
			Assert.IsFalse (di.Exists, "#B1");
			Assert.AreEqual (Path.DirectorySeparatorChar, di.FullName [di.FullName.Length - 1], "#B2");

			di = new DirectoryInfo ("something" + Path.AltDirectorySeparatorChar);
			Assert.IsFalse (di.Exists, "#C1");
			Assert.AreEqual (Path.DirectorySeparatorChar, di.FullName [di.FullName.Length - 1], "#C2");

			if (RunningOnUnix) {
				di = new DirectoryInfo ("/");
				Assert.AreEqual ("/", di.FullName, "#D1");

				di = new DirectoryInfo ("test/");
				Assert.AreEqual (Path.Combine (Directory.GetCurrentDirectory (), "test/"), di.FullName, "#D2");

				di = new DirectoryInfo ("/test");
				Assert.AreEqual ("/test", di.FullName, "#D3");

				di = new DirectoryInfo ("/test/");
				Assert.AreEqual ("/test/", di.FullName, "#D4");
			} else {
				di = new DirectoryInfo (@"c:");
				Assert.AreEqual (@"C:\", di.FullName, "#D1");

				di = new DirectoryInfo (@"c:\");
				Assert.AreEqual (@"c:\", di.FullName, "#D2");

				di = new DirectoryInfo (@"c:\test");
				Assert.AreEqual (@"c:\test", di.FullName, "#D3");

				di = new DirectoryInfo (@"c:\test\");
				Assert.AreEqual (@"c:\test\", di.FullName, "#D4");
			}
		}

		[Test]
		public void FullName_RootDirectory ()
		{
			DirectoryInfo di = new DirectoryInfo (String.Empty + Path.DirectorySeparatorChar);
			if (RunningOnUnix) {
				// can't be sure of the root drive under windows
				Assert.AreEqual ("/", di.FullName, "#1");
			}
			Assert.IsNull (di.Parent, "#2");

			di = new DirectoryInfo (String.Empty + Path.AltDirectorySeparatorChar);
			if (RunningOnUnix) {
				// can't be sure of the root drive under windows
				Assert.AreEqual ("/", di.FullName, "#3");
			}
			Assert.IsNull (di.Parent, "#4");
		}

		[Test] // GetDirectories ()
		public void GetDirectories1 ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectories1.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assert.AreEqual (0, info.GetDirectories ().Length, "#1");
				
				Directory.CreateDirectory (path + DSC + "1");
				Directory.CreateDirectory (path + DSC + "2");
				File.Create (path + DSC + "filetest").Close ();
				Assert.AreEqual (2, info.GetDirectories ().Length, "#2");
				
				Directory.Delete (path + DSC + 2);
				Assert.AreEqual (1, info.GetDirectories ().Length, "#3");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetDirectories ()
		public void GetDirectories1_DirectoryDoesNotExist ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectoriesDirectoryNotFoundException1.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetDirectories ();
				Assert.Fail ("#1");
			} catch (DirectoryNotFoundException ex) {
				// Could not find a part of '...'
				Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (path) != -1, "#5");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetDirectories (String)
		public void GetDirectories2 ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectories2.Test";
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assert.AreEqual (0, info.GetDirectories ("*").Length, "#1");
				
				Directory.CreateDirectory (path + DSC + "test120");
				Directory.CreateDirectory (path + DSC + "test210");
				Directory.CreateDirectory (path + DSC + "atest330");
				Directory.CreateDirectory (path + DSC + "test220");
				Directory.CreateDirectory (path + DSC + "rest");
				Directory.CreateDirectory (path + DSC + "rest" + DSC + "subdir");
				File.Create (path + DSC + "filetest").Close ();

				Assert.AreEqual (5, info.GetDirectories ("*").Length, "#2");
				Assert.AreEqual (3, info.GetDirectories ("test*").Length, "#3");
				Assert.AreEqual (2, info.GetDirectories ("test?20").Length, "#4");
				Assert.AreEqual (0, info.GetDirectories ("test?").Length, "#5");
				Assert.AreEqual (0, info.GetDirectories ("test[12]*").Length, "#6");
				Assert.AreEqual (2, info.GetDirectories ("test2*0").Length, "#7");
				Assert.AreEqual (4, info.GetDirectories ("*test*").Length, "#8");
#if NET_2_0
				Assert.AreEqual (6, info.GetDirectories ("*", SearchOption.AllDirectories).Length, "#9");
#endif
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetDirectories (String)
		public void GetDirectories2_DirectoryDoesNotExist ()
		{
			string path = TempFolder + DSC + "DIT.GetDirectoriesDirectoryNotFoundException2.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetDirectories ("*");
				Assert.Fail ("#1");
			} catch (DirectoryNotFoundException ex) {
				// Could not find a part of '...'
				Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (path) != -1, "#5");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetDirectories (String)
		public void GetDirectories2_SearchPattern_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.GetDirectories (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("searchPattern", ex.ParamName, "#5");
			}
		}

		[Test] //GetDirectories (String, SearchOptions)
		public void GetDirectiories_SearchOptionAllDirectories ()
		{
			string directoryToBeLookedFor = "lookforme";
			DirectoryInfo baseDir = Directory.CreateDirectory(Path.Combine(TempFolder, "GetDirectiories_SearchOptionAllDirectories"));
			DirectoryInfo subdir = baseDir.CreateSubdirectory("subdir");
			DirectoryInfo subsubdir = subdir.CreateSubdirectory(directoryToBeLookedFor);
			DirectoryInfo[] directoriesFound = baseDir.GetDirectories(directoryToBeLookedFor, SearchOption.AllDirectories);
			Assert.AreEqual(1, directoriesFound.Length, "There should be exactly one directory with the specified name.");
			Assert.AreEqual(directoryToBeLookedFor, directoriesFound[0].Name, "The name of the directory found should match the expected one.");
		}

#if NET_2_0
		[Test] // GetDirectories (String, SearchOption)
		public void GetDirectories3_SearchPattern_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.GetDirectories (null, SearchOption.AllDirectories);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("searchPattern", ex.ParamName, "#5");
			}
		}
#endif

		[Test] // GetFiles ()
		public void GetFiles1 ()
		{
			string path = TempFolder + DSC + "DIT.GetFiles1.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assert.AreEqual (0, info.GetFiles ().Length, "#1");
				File.Create (path + DSC + "file1").Close ();
				File.Create (path + DSC + "file2").Close ();
				Directory.CreateDirectory (path + DSC + "directory1");
				Assert.AreEqual (2, info.GetFiles ().Length, "#2");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetFiles ()
		public void GetFiles1_DirectoryDoesNotExist ()
		{
			string path = TempFolder + DSC + "DIT.GetFilesDirectoryNotFoundException1.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetFiles ();
				Assert.Fail ("#1");
			} catch (DirectoryNotFoundException ex) {
				// Could not find a part of '...'
				Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (path) != -1, "#5");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetFiles (String)
		public void GetFiles2 ()
		{
			string path = TempFolder + DSC + "DIT.GetFiles2.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				Assert.AreEqual (0, info.GetFiles ("*").Length, "#1");
				File.Create (path + DSC + "file120file").Close ();
				File.Create (path + DSC + "file220file").Close ();
				File.Create (path + DSC + "afile330file").Close ();
				File.Create (path + DSC + "test.abc").Close ();
				File.Create (path + DSC + "test.abcd").Close ();
				File.Create (path + DSC + "test.abcdef").Close ();
				Directory.CreateDirectory (path + DSC + "dir");

				Assert.AreEqual (6, info.GetFiles ("*").Length, "#2");
				Assert.AreEqual (2, info.GetFiles ("file*file").Length, "#3");
				Assert.AreEqual (3, info.GetFiles ("*file*").Length, "#4");
				Assert.AreEqual (2, info.GetFiles ("file?20file").Length, "#5");
				Assert.AreEqual (1, info.GetFiles ("*.abcd").Length, "#6");
				Assert.AreEqual (2, info.GetFiles ("*.abcd*").Length, "#7");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetFiles (String)
		public void GetFiles2_DirectoryDoesNotExist ()
		{
			string path = TempFolder + DSC + "DIT.GetFilesDirectoryNotFoundException2.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				info.GetFiles ("*");
				Assert.Fail ("#1");
			} catch (DirectoryNotFoundException ex) {
				// Could not find a part of '...'
				Assert.AreEqual (typeof (DirectoryNotFoundException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf (path) != -1, "#5");
			} finally {
				DeleteDir (path);
			}
		}

		[Test] // GetFiles (String)
		public void GetFiles2_SearchPattern_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.GetFiles (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("searchPattern", ex.ParamName, "#5");
			}
		}

#if NET_2_0
		[Test] // GetFiles (String, SearchOption)
		public void GetFiles3_SearchPattern_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.GetFiles (null, SearchOption.TopDirectoryOnly);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("searchPattern", ex.ParamName, "#5");
			}
		}
#endif

		[Test]
		public void GetFileSystemInfos2_SearchPattern_Null ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			try {
				info.GetFileSystemInfos (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("searchPattern", ex.ParamName, "#5");
			}
		}

		[Test]
		public void MoveTo ()
		{
			string path1 = TempFolder + DSC + "DIT.MoveTo.Soucre.Test";
			string path2 = TempFolder + DSC + "DIT.MoveTo.Dest.Test";
			DeleteDir (path1);
			DeleteDir (path2);

			try {
				DirectoryInfo info1 = Directory.CreateDirectory (path1);
				DirectoryInfo info2 = new DirectoryInfo (path2);

				Assert.IsTrue (info1.Exists, "#A1");
				Assert.IsFalse (info2.Exists, "#A2");

				info1.MoveTo (path2);
				Assert.IsTrue (info1.Exists, "#B1");
				Assert.IsFalse (info2.Exists, "#B2");

				info1 = new DirectoryInfo (path1);
				info2 = new DirectoryInfo (path2);
				Assert.IsFalse (info1.Exists, "#C1");
				Assert.IsTrue (info2.Exists, "#C2");
			} finally {
				DeleteDir (path1);
				DeleteDir (path2);
			}
		}

		[Test]
		public void MoveTo_DestDirName_Empty ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException1.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				try {
					info.MoveTo (string.Empty);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Empty file name is not legal
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destDirName", ex.ParamName, "#5");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void MoveTo_DestDirName_Null ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentNullException.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				try {
					info.MoveTo (null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("destDirName", ex.ParamName, "#5");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void MoveTo_DestDirName_InvalidPathChars ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException3.Test";
			DeleteDir (path);
			
			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				try {
					info.MoveTo (Path.InvalidPathChars [0].ToString ());
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The path contains illegal characters
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void MoveTo_DestDirName_Whitespace ()
		{
			string path = TempFolder + DSC + "DIT.MoveToArgumentException2.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				try {
					info.MoveTo ("    ");
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The path is not of a legal form
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		public void MoveTo_SourceDest_NotDifferent ()
		{
			string path = TempFolder + DSC + "DIT.MoveToIOException1.Test";
			DeleteDir (path);

			try {
				DirectoryInfo info = Directory.CreateDirectory (path);
				try {
					info.MoveTo (path);
					Assert.Fail ("#A1");
				} catch (IOException ex) {
					// Source and destination path must be different
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#A2");
					Assert.IsNull (ex.InnerException, "#A3");
					Assert.IsNotNull (ex.Message, "#A4");
				}
			} finally {
				DeleteDir (path);
			}

			try {
				DirectoryInfo info = new DirectoryInfo (path);
				try {
					info.MoveTo (path);
					Assert.Fail ("#B1");
				} catch (IOException ex) {
					// Source and destination path must be different
					Assert.AreEqual (typeof (IOException), ex.GetType (), "#B2");
					Assert.IsNull (ex.InnerException, "#B3");
					Assert.IsNotNull (ex.Message, "#B4");
				}
			} finally {
				DeleteDir (path);
			}
		}
		
		[Test]
		public void MoveTo_UpdateProperties ()
		{
			string path = TempFolder + DSC + "DIT.MoveToUpdateProperties.Test";
			string path2 = TempFolder + DSC + "DIT.MoveToUpdateProperties2.Test";
			string path3 = path2 + DSC + "DIT.MoveToUpdateProperties3.Test";
			DeleteDir (path);
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path2);

			DirectoryInfo info = new DirectoryInfo(path);
			
			Assert.IsTrue (Directory.Exists(info.FullName));
			Assert.AreEqual (path, info.FullName);
			Assert.AreEqual ("DIT.MoveToUpdateProperties.Test", info.Name);
			Assert.AreEqual (TempFolder, info.Parent.FullName);
			Assert.AreEqual (path, info.ToString ());

			info.MoveTo (path3);
			Assert.IsTrue (Directory.Exists(info.FullName));
			Assert.AreEqual (path3, info.FullName);
			Assert.AreEqual ("DIT.MoveToUpdateProperties3.Test", info.Name);
			Assert.AreEqual (path2, info.Parent.FullName);
			Assert.AreEqual (path3, info.ToString ());
		}

		[Test]
		public void DirectoryNameWithSpace ()
		{
			DeleteDir ("this has a space at the end ");
			string path = Path.Combine (TempFolder, "this has a space at the end ");
			Directory.CreateDirectory (path);
			DirectoryInfo i = new DirectoryInfo (path);
			string dummy = null;
			foreach (FileInfo f in i.GetFiles ())
				dummy = f.Name;
		}

		[Test]
		public void LastWriteTime ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.LastWriteTime = new DateTime (2003, 6, 4, 6, 4, 0);

			DateTime time = Directory.GetLastWriteTime (TempFolder);
			Assert.AreEqual (2003, time.Year, "#A1");
			Assert.AreEqual (6, time.Month, "#A2");
			Assert.AreEqual (4, time.Day, "#A3");
			Assert.AreEqual (6, time.Hour, "#A4");
			Assert.AreEqual (4, time.Minute, "#A5");
			Assert.AreEqual (0, time.Second, "#A6");

			time = TimeZone.CurrentTimeZone.ToLocalTime (
				Directory.GetLastWriteTimeUtc (TempFolder));
			Assert.AreEqual (2003, time.Year, "#B1");
			Assert.AreEqual (6, time.Month, "#B2");
			Assert.AreEqual (4, time.Day, "#B3");
			Assert.AreEqual (6, time.Hour, "#B4");
			Assert.AreEqual (4, time.Minute, "#B5");
			Assert.AreEqual (0, time.Second, "#B6");
		}

		[Test]
		public void LastWriteTimeUtc ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.LastWriteTimeUtc = new DateTime (2003, 6, 4, 6, 4, 0);

			DateTime time = TimeZone.CurrentTimeZone.ToUniversalTime (
				Directory.GetLastWriteTime (TempFolder));
			Assert.AreEqual (2003, time.Year, "#A1");
			Assert.AreEqual (6, time.Month, "#A2");
			Assert.AreEqual (4, time.Day, "#A3");
			Assert.AreEqual (6, time.Hour, "#A4");
			Assert.AreEqual (4, time.Minute, "#A5");
			Assert.AreEqual (0, time.Second, "#A6");

			time = Directory.GetLastWriteTimeUtc (TempFolder);
			Assert.AreEqual (2003, time.Year, "#B1");
			Assert.AreEqual (6, time.Month, "#B2");
			Assert.AreEqual (4, time.Day, "#B3");
			Assert.AreEqual (6, time.Hour, "#B4");
			Assert.AreEqual (4, time.Minute, "#B5");
			Assert.AreEqual (0, time.Second, "#B6");
		}

		[Test]
		[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
		public void LastAccessTime ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.LastAccessTime = DateTime.Now;
		}

		[Test]
		[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
		public void LastAccessTimeUtc ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.LastAccessTimeUtc = DateTime.Now;
		}

		[Test]
		[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
		public void CreationTime ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.CreationTime = DateTime.Now;
		}

		[Test]
		[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
		public void CreationTimeUtc ()
		{
			DirectoryInfo info = new DirectoryInfo (TempFolder);
			info.CreationTimeUtc = DateTime.Now;
		}

		[Test]
		public void Name_Bug76903 ()
		{
			CheckName ("/usr/share");
			CheckName ("/usr/share/");
			CheckName ("/usr/share/.");
			CheckName ("/usr/share/./");
			CheckName ("/usr/share/blabla/../");
			CheckName ("/usr/lib/../share/.");
		}

		[Test]
		public void Hang_76191 ()
		{
			// from bug #76191 (hangs on Windows)
			DirectoryInfo di = new DirectoryInfo (Environment.CurrentDirectory);
			Stack s = new Stack ();
			s.Push (di);
			while (di.Parent != null) {
				di = di.Parent;
				s.Push (di);
			}
			while (s.Count > 0) {
				di = (DirectoryInfo) s.Pop ();
				Assert.IsTrue (di.Exists, di.Name);
			}
		}

		[Test]
		public void WindowsSystem32_76191 ()
		{
			if (RunningOnUnix)
				return;

			Directory.SetCurrentDirectory (@"C:\WINDOWS\system32");
			WindowsParentFullName ("C:", "C:\\WINDOWS");
			WindowsParentFullName ("C:\\", null);
			WindowsParentFullName ("C:\\dir", "C:\\");
			WindowsParentFullName ("C:\\dir\\", "C:\\");
			WindowsParentFullName ("C:\\dir\\dir", "C:\\dir");
			WindowsParentFullName ("C:\\dir\\dir\\", "C:\\dir");
		}

		[Test]
		public void Parent_Bug77090 ()
		{
			DirectoryInfo di = new DirectoryInfo ("/home");
			if (Path.DirectorySeparatorChar == '\\') {
				Assert.IsTrue (di.Parent.Name.EndsWith (":\\"), "#1");
			} else
				Assert.AreEqual ("/", di.Parent.Name, "#1");
			Assert.IsNull (di.Parent.Parent, "#2");
		}

		[Test]
		public void ToStringTest ()
		{
			DirectoryInfo info;

			info = new DirectoryInfo ("Test");
			Assert.AreEqual ("Test", info.ToString (), "#1");
			info = new DirectoryInfo (TempFolder + DSC + "ToString.Test");
			Assert.AreEqual (TempFolder + DSC + "ToString.Test", info.ToString ());
		}

#if !MOBILE
		[Test]
		public void Serialization ()
		{
			DirectoryInfo info;
			SerializationInfo si;

			info = new DirectoryInfo ("Test");
			si = new SerializationInfo (typeof (DirectoryInfo), new FormatterConverter ());
			info.GetObjectData (si, new StreamingContext ());

			Assert.AreEqual (2, si.MemberCount, "#A1");
			Assert.AreEqual ("Test", si.GetString ("OriginalPath"), "#A2");
			Assert.AreEqual (Path.Combine (Directory.GetCurrentDirectory (), "Test"), si.GetString ("FullPath"), "#A3");

			info = new DirectoryInfo (TempFolder);
			si = new SerializationInfo (typeof (DirectoryInfo), new FormatterConverter ());
			info.GetObjectData (si, new StreamingContext ());

			Assert.AreEqual (2, si.MemberCount, "#B1");
			Assert.AreEqual (TempFolder, si.GetString ("OriginalPath"), "#B2");
			Assert.AreEqual (TempFolder, si.GetString ("FullPath"), "#B3");
		}

		[Test]
		public void Deserialization ()
		{
			DirectoryInfo info = new DirectoryInfo ("Test");

			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, info);
			ms.Position = 0;

			DirectoryInfo clone = (DirectoryInfo) bf.Deserialize (ms);
			Assert.AreEqual (info.Name, clone.Name, "#1");
			Assert.AreEqual (info.FullName, clone.FullName, "#2");
		}

		// Needed so that UnixSymbolicLinkInfo doesn't have to
		// be JITted on windows
		private void Symlink_helper ()
		{
			string path = TempFolder + DSC + "DIT.Symlink";
			string dir = path + DSC + "dir";
			string link = path + DSC + "link";

			DeleteDir (path);

			try {
				Directory.CreateDirectory (path);
				Directory.CreateDirectory (dir);
				Mono.Unix.UnixSymbolicLinkInfo li = new Mono.Unix.UnixSymbolicLinkInfo (link);
				li.CreateSymbolicLinkTo (dir);

				DirectoryInfo info = new DirectoryInfo (path);
				DirectoryInfo[] dirs = info.GetDirectories ();
				Assert.AreEqual (2, dirs.Length, "#1");
			} finally {
				DeleteDir (path);
			}
		}

		[Test]
		[Category ("NotDotNet")]
		public void Symlink ()
		{
			// This test only applies to Linux and
			// Linux-like platforms but mono-on-windows
			// doesn't set the NotDotNet category
			if (!RunningOnUnix) {
				return;
			}

			Symlink_helper ();
		}
#endif
		static bool RunningOnUnix {
			get {
				int p = (int) Environment.OSVersion.Platform;
				return ((p == 4) || (p == 128) || (p == 6));
			}
		}

		void WindowsParentFullName (string name, string expected)
		{
			DirectoryInfo di = new DirectoryInfo (name);
			if (di.Parent == null)
				Assert.IsNull (expected, name);
			else
				Assert.AreEqual (expected, di.Parent.FullName, name);
		}

		void CheckName (string name)
		{
			DirectoryInfo di = new DirectoryInfo (name);
			Assert.AreEqual ("share", di.Name, name + ".Name");
			Assert.AreEqual ("usr", di.Parent.Name, name + ".Parent.Name");
		}

		void DeleteDir (string path)
		{
			if (Directory.Exists (path))
				Directory.Delete (path, true);
		}
	}
}
