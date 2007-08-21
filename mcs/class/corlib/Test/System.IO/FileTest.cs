//
// FileTest.cs: Test cases for System.IO.File
//
// Author: 
//     Duncan Mak (duncan@ximian.com)
//     Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//
// TODO: Find out why ArgumentOutOfRangeExceptions does not manage to close streams properly
//
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileTest
	{
		CultureInfo old_culture;
		static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		[SetUp]
		public void SetUp ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
			old_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void TestExists ()
		{
			FileStream s = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			try {
				Assert.IsFalse (File.Exists (null), "#1");
				Assert.IsFalse (File.Exists (""), "#2");
				Assert.IsFalse (File.Exists ("  \t\t  \t \n\t\n \n"), "#3");
				DeleteFile (path);
				s = File.Create (path);
				s.Close ();
				Assert.IsTrue (File.Exists (path), "#4");
				Assert.IsFalse (File.Exists (TempFolder + Path.DirectorySeparatorChar + "doesnotexist"), "#5");
			} finally {
				if (s != null)
					s.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void Exists_InvalidFileName () 
		{
			Assert.IsFalse (File.Exists ("><|"), "#1");
			Assert.IsFalse (File.Exists ("?*"), "#2");
		}

		[Test]
		public void Exists_InvalidDirectory () 
		{
			Assert.IsFalse (File.Exists (Path.Combine ("does not exist", "file.txt")));
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void CtorArgumentNullException1 ()
		{
			File.Create (null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorArgumentException1 ()
		{
			File.Create ("");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorArgumentException2 ()
		{
			File.Create (" ");
		}

		[Test]
		[ExpectedException(typeof (DirectoryNotFoundException))]
		public void CtorDirectoryNotFoundException ()
		{
			FileStream stream = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "directory_does_not_exist" + Path.DirectorySeparatorChar + "foo";
			
			try {
				stream = File.Create (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void TestCreate ()
		{
			FileStream stream = null;
			string path = "";
			/* positive test: create resources/foo */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "foo";
				stream = File.Create (path);
				Assert.IsTrue (File.Exists (path), "#1");
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
			
			path = "";
			stream = null;

			/* positive test: repeat test above again to test for overwriting file */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "foo";
				stream = File.Create (path);
				Assert.IsTrue (File.Exists (path), "#2");
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyArgumentNullException1 ()
		{
			File.Copy (null, "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyArgumentNullException2 ()
		{
			File.Copy ("a", null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException1 ()
		{
			File.Copy ("", "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException2 ()
		{
			File.Copy ("a", "");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException3 ()
		{
			File.Copy (" ", "b");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CopyArgumentException4 ()
		{
			File.Copy ("a", " ");
		}

		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void CopyFileNotFoundException ()
		{
			File.Copy ("doesnotexist", "b");
		}

		[ExpectedException(typeof(IOException))]
		public void CopyIOException ()
		{
			DeleteFile (TempFolder + Path.DirectorySeparatorChar + "bar");
			DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");
			try {
				File.Create (TempFolder + Path.DirectorySeparatorChar + "AFile.txt").Close ();
				File.Copy (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", TempFolder + Path.DirectorySeparatorChar + "bar");
				File.Copy (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", TempFolder + Path.DirectorySeparatorChar + "bar");
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "bar");
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");
			}
		}

		[Test]
		public void TestCopy ()
		{
			string path1 = TempFolder + Path.DirectorySeparatorChar + "bar";
			string path2 = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			/* positive test: copy resources/AFile.txt to resources/bar */
			try {
				DeleteFile (path1);
				DeleteFile (path2);

				File.Create (path2).Close ();
				File.Copy (path2, path1);
				Assert.IsTrue (File.Exists (path2), "#A1");
				Assert.IsTrue (File.Exists (path1), "#A2");

				Assert.IsTrue (File.Exists (path1), "#B1");
				File.Copy (path2, path1, true);
				Assert.IsTrue (File.Exists (path2), "#B2");
				Assert.IsTrue (File.Exists (path1), "#B3");
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeleteArgumentNullException ()
		{
			File.Delete (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DeleteArgumentException1 ()
		{
			File.Delete ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DeleteArgumentException2 ()
		{
			File.Delete (" ");
		}

		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void DeleteDirectoryNotFoundException ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "directory_does_not_exist" + Path.DirectorySeparatorChar + "foo";
			if (Directory.Exists (path))
				Directory.Delete (path, true);
			File.Delete (path);
		}


		[Test]
		public void TestDelete ()
		{
			string foopath = TempFolder + Path.DirectorySeparatorChar + "foo";
			DeleteFile (foopath);
			try {
				File.Create (foopath).Close ();
				File.Delete (foopath);
				Assert.IsFalse (File.Exists (foopath));
			} finally {
				DeleteFile (foopath);
			}
		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("NotWorking")]
		public void DeleteOpenStreamException ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "DeleteOpenStreamException";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				File.Delete (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test] // bug #82514
		[Category ("NotWorking")]
		public void GetAttributes_Archive ()
		{
			FileAttributes attrs;

			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.Create (path).Close ();

			attrs = File.GetAttributes (path);
			Assert.IsTrue ((attrs & FileAttributes.Archive) != 0, "#1");

			attrs &= ~FileAttributes.Archive;
			File.SetAttributes (path, attrs);

			attrs = File.GetAttributes (path);
			Assert.IsFalse ((attrs & FileAttributes.Archive) != 0, "#2");
		}

		[Test] // bug #82514
		[Category ("NotWorking")]
		public void GetAttributes_Default_File ()
		{
			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.Create (path).Close ();

			FileAttributes attrs = File.GetAttributes (path);

			Assert.IsTrue ((attrs & FileAttributes.Archive) != 0, "#1");
			Assert.IsFalse ((attrs & FileAttributes.Directory) != 0, "#2");
			Assert.IsFalse ((attrs & FileAttributes.Hidden) != 0, "#3");
			Assert.IsFalse ((attrs & FileAttributes.Normal) != 0, "#4");
			Assert.IsFalse ((attrs & FileAttributes.ReadOnly) != 0, "#5");
			Assert.IsFalse ((attrs & FileAttributes.System) != 0, "#6");
		}

		[Test]
		public void GetAttributes_Default_Directory ()
		{
			FileAttributes attrs = File.GetAttributes (TempFolder);

			Assert.IsFalse ((attrs & FileAttributes.Archive) != 0, "#1");
			Assert.IsTrue ((attrs & FileAttributes.Directory) != 0, "#2");
			Assert.IsFalse ((attrs & FileAttributes.Hidden) != 0, "#3");
			Assert.IsFalse ((attrs & FileAttributes.Normal) != 0, "#4");
			Assert.IsFalse ((attrs & FileAttributes.ReadOnly) != 0, "#5");
			Assert.IsFalse ((attrs & FileAttributes.System) != 0, "#6");
		}

		[Test]
		public void GetAttributes_Directory ()
		{
			FileAttributes attrs = File.GetAttributes (TempFolder);

			Assert.IsTrue ((attrs & FileAttributes.Directory) != 0, "#1");

			attrs &= ~FileAttributes.Directory;
			File.SetAttributes (TempFolder, attrs);

			Assert.IsFalse ((attrs & FileAttributes.Directory) != 0, "#2");

			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.Create (path).Close ();

			attrs = File.GetAttributes (path);
			attrs |= FileAttributes.Directory;
			File.SetAttributes (path, attrs);

			Assert.IsTrue ((attrs & FileAttributes.Directory) != 0, "#3");
		}

		[Test]
		public void GetAttributes_ReadOnly ()
		{
			FileAttributes attrs;

			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.Create (path).Close ();

			attrs = File.GetAttributes (path);
			Assert.IsFalse ((attrs & FileAttributes.ReadOnly) != 0, "#1");

			try {
				attrs |= FileAttributes.ReadOnly;
				File.SetAttributes (path, attrs);

				attrs = File.GetAttributes (path);
				Assert.IsTrue ((attrs & FileAttributes.ReadOnly) != 0, "#2");
			} finally {
				File.SetAttributes (path, FileAttributes.Normal);
			}
		}

		[Test]
		public void GetAttributes_System ()
		{
			if (RunningOnUnix)
				Assert.Ignore ("FileAttributes.System is not supported on Unix.");

			FileAttributes attrs;

			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.Create (path).Close ();

			attrs = File.GetAttributes (path);
			Assert.IsFalse ((attrs & FileAttributes.System) != 0, "#1");

			attrs |= FileAttributes.System;
			File.SetAttributes (path, FileAttributes.System);

			attrs = File.GetAttributes (path);
			Assert.IsTrue ((attrs & FileAttributes.System) != 0, "#2");
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void GetAttributes_Path_DoesNotExist ()
		{
			string path = Path.Combine (TempFolder, "GetAttributes.tmp");
			File.GetAttributes (path);
		}

		[Test]
		public void GetAttributes_Path_Empty ()
		{
			try {
				File.GetAttributes (string.Empty);
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
		public void GetAttributes_Path_Null ()
		{
			try {
				File.GetAttributes (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("path", ex.ParamName, "#6");
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void MoveException1 ()
		{
			File.Move (null, "b");
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void MoveException2 ()
		{
			File.Move ("a", null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException3 ()
		{
			File.Move ("", "b");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException4 ()
		{
			File.Move ("a", "");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException5 ()
		{
			File.Move (" ", "b");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void MoveException6 ()
		{
			File.Move ("a", " ");
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void MoveException7 ()
		{
			DeleteFile (TempFolder + Path.DirectorySeparatorChar + "doesnotexist");
			File.Move (TempFolder + Path.DirectorySeparatorChar + "doesnotexist", "b");
		}

		[Test]
		[ExpectedException(typeof (DirectoryNotFoundException))]
		public void MoveException8 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "foo";
			DeleteFile (path);
			try {
				File.Create (TempFolder + Path.DirectorySeparatorChar + "AFile.txt").Close ();
				File.Copy(TempFolder + Path.DirectorySeparatorChar + "AFile.txt", path, true);
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "doesnotexist" + Path.DirectorySeparatorChar + "b");
				File.Move (TempFolder + Path.DirectorySeparatorChar + "foo", TempFolder + Path.DirectorySeparatorChar + "doesnotexist" + Path.DirectorySeparatorChar + "b");
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "AFile.txt");
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (IOException))]
		public void MoveException9 ()
		{
			File.Create (TempFolder + Path.DirectorySeparatorChar + "foo").Close ();
			try {
				File.Move (TempFolder + Path.DirectorySeparatorChar + "foo", TempFolder);
			} finally {
				DeleteFile (TempFolder + Path.DirectorySeparatorChar + "foo");
			}
		}

		[Test]
		public void TestMove ()
		{
			string bar = TempFolder + Path.DirectorySeparatorChar + "bar";
			string baz = TempFolder + Path.DirectorySeparatorChar + "baz";
			if (!File.Exists (bar)) {
				FileStream f = File.Create(bar);
				f.Close();
			}
			
			Assert.IsTrue (File.Exists (bar), "#1");
			File.Move (bar, baz);
			Assert.IsFalse (File.Exists (bar), "#2");
			Assert.IsTrue (File.Exists (baz), "#3");

			// Test moving of directories
			string dir = Path.Combine (TempFolder, "dir");
			string dir2 = Path.Combine (TempFolder, "dir2");
			string dir_foo = Path.Combine (dir, "foo");
			string dir2_foo = Path.Combine (dir2, "foo");

			if (Directory.Exists (dir))
				Directory.Delete (dir, true);

			Directory.CreateDirectory (dir);
			Directory.CreateDirectory (dir2);
			File.Create (dir_foo).Close ();
			File.Move (dir_foo, dir2_foo);
			Assert.IsTrue (File.Exists (dir2_foo), "#4");
			
			Directory.Delete (dir, true);
			Directory.Delete (dir2, true);
			DeleteFile (dir_foo);
			DeleteFile (dir2_foo);
		}

		[Test]
		public void TestOpen ()
		{
			string path = "";
			FileStream stream = null;
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
				if (!File.Exists (path))
					stream = File.Create (path);
				stream.Close ();
				stream = File.Open (path, FileMode.Open);
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
			
			path = "";
			stream = null;
			/* Exception tests */
			try {
				path = TempFolder + Path.DirectorySeparatorChar + "filedoesnotexist";
				stream = File.Open (path, FileMode.Open);
				Assert.Fail ("File 'filedoesnotexist' should not exist");
			} catch (FileNotFoundException) {
				// do nothing, this is what we expect
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void Open () 
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
			FileStream stream = null;
			try {
				stream = File.Open (path, FileMode.Open);
				Assert.IsTrue (stream.CanRead, "#A1");
				Assert.IsTrue (stream.CanSeek, "#A2");
				Assert.IsTrue (stream.CanWrite, "#A3");
				stream.Close ();

				stream = File.Open (path, FileMode.Open, FileAccess.Write);
				Assert.IsFalse (stream.CanRead, "#B1");
				Assert.IsTrue (stream.CanSeek, "#B2");
				Assert.IsTrue (stream.CanWrite, "#B3");
				stream.Close ();

				stream = File.Open (path, FileMode.Open, FileAccess.Read);
				Assert.IsTrue (stream.CanRead, "#C1");
				Assert.IsTrue (stream.CanSeek, "#C2");
				Assert.IsFalse (stream.CanWrite, "#C3");
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void OpenException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			FileStream stream = null;
			// CreateNew + Read throws an exceptoin
			try {
				stream = File.Open (TempFolder + Path.DirectorySeparatorChar + "AFile.txt", FileMode.CreateNew, FileAccess.Read);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void OpenException2 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			FileStream s = null;
			// Append + Read throws an exceptoin
			if (!File.Exists (path))
				File.Create (path).Close ();
			try {
				s = File.Open (path, FileMode.Append, FileAccess.Read);
			} finally {
				if (s != null)
					s.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void OpenRead ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
			FileStream stream = null;
			
			try {
				stream = File.OpenRead (path);
				Assert.IsTrue (stream.CanRead, "#1");
				Assert.IsTrue (stream.CanSeek, "#2");
				Assert.IsFalse (stream.CanWrite, "#3");
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void OpenWrite ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			if (!File.Exists (path))
				File.Create (path).Close ();
			FileStream stream = null;

			try {
				stream = File.OpenWrite (path);
				Assert.IsFalse (stream.CanRead, "#1");
				Assert.IsTrue (stream.CanSeek, "#2");
				Assert.IsTrue (stream.CanWrite, "#3");
				stream.Close ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void TestGetCreationTime ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "baz";
			DeleteFile (path);

			try {
				File.Create (path).Close();
				DateTime time = File.GetCreationTime (path);
				Assert.IsTrue ((DateTime.Now - time).TotalSeconds < 10);
			} finally {
				DeleteFile (path);
			}
		}

		// Setting the creation time on Unix is not possible
		[Test]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void CreationTime ()
		{
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128))
				return;

			string path = Path.GetTempFileName ();
			try {
				File.SetCreationTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
				DateTime time = File.GetCreationTime (path);
				Assert.AreEqual (2002, time.Year, "#A1");
				Assert.AreEqual (4, time.Month, "#A2");
				Assert.AreEqual (6, time.Day, "#A3");
				Assert.AreEqual (4, time.Hour, "#A4");
				Assert.AreEqual (4, time.Second, "#A5");

				time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetCreationTimeUtc (path));
				Assert.AreEqual (2002, time.Year, "#B1");
				Assert.AreEqual (4, time.Month, "#B2");
				Assert.AreEqual (6, time.Day, "#B3");
				Assert.AreEqual (4, time.Hour, "#B4");
				Assert.AreEqual (4, time.Second, "#B5");

				File.SetCreationTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
				time = File.GetCreationTimeUtc (path);
				Assert.AreEqual (2002, time.Year, "#C1");
				Assert.AreEqual (4, time.Month, "#C2");
				Assert.AreEqual (6, time.Day, "#C3");
				Assert.AreEqual (4, time.Hour, "#C4");
				Assert.AreEqual (4, time.Second, "#C5");

				time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetCreationTime (path));
				Assert.AreEqual (2002, time.Year, "#D1");
				Assert.AreEqual (4, time.Month, "#D2");
				Assert.AreEqual (6, time.Day, "#D3");
				Assert.AreEqual (4, time.Hour, "#D4");
				Assert.AreEqual (4, time.Second, "#D5");
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void LastAccessTime ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "lastAccessTime";
			if (File.Exists (path))
				File.Delete (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				stream.Close ();

				File.SetLastAccessTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
				DateTime time = File.GetLastAccessTime (path);
				Assert.AreEqual (2002, time.Year, "#A1");
				Assert.AreEqual (4, time.Month, "#A2");
				Assert.AreEqual (6, time.Day, "#A3");
				Assert.AreEqual (4, time.Hour, "#A4");
				Assert.AreEqual (4, time.Second, "#A5");

				time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetLastAccessTimeUtc (path));
				Assert.AreEqual (2002, time.Year, "#B1");
				Assert.AreEqual (4, time.Month, "#B2");
				Assert.AreEqual (6, time.Day, "#B3");
				Assert.AreEqual (4, time.Hour, "#B4");
				Assert.AreEqual (4, time.Second, "#B5");

				File.SetLastAccessTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
				time = File.GetLastAccessTimeUtc (path);
				Assert.AreEqual (2002, time.Year, "#C1");
				Assert.AreEqual (4, time.Month, "#C2");
				Assert.AreEqual (6, time.Day, "#C3");
				Assert.AreEqual (4, time.Hour, "#C4");
				Assert.AreEqual (4, time.Second, "#C5");

				time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetLastAccessTime (path));
				Assert.AreEqual (2002, time.Year, "#D1");
				Assert.AreEqual (4, time.Month, "#D2");
				Assert.AreEqual (6, time.Day, "#D3");
				Assert.AreEqual (4, time.Hour, "#D4");
				Assert.AreEqual (4, time.Second, "#D5");
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		public void LastWriteTime ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "lastWriteTime";
			if (File.Exists (path))
				File.Delete (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				stream.Close ();

				File.SetLastWriteTime (path, new DateTime (2002, 4, 6, 4, 6, 4));
				DateTime time = File.GetLastWriteTime (path);
				Assert.AreEqual (2002, time.Year, "#A1");
				Assert.AreEqual (4, time.Month, "#A2");
				Assert.AreEqual (6, time.Day, "#A3");
				Assert.AreEqual (4, time.Hour, "#A4");
				Assert.AreEqual (4, time.Second, "#A5");

				time = TimeZone.CurrentTimeZone.ToLocalTime (File.GetLastWriteTimeUtc (path));
				Assert.AreEqual (2002, time.Year, "#B1");
				Assert.AreEqual (4, time.Month, "#B2");
				Assert.AreEqual (6, time.Day, "#B3");
				Assert.AreEqual (4, time.Hour, "#B4");
				Assert.AreEqual (4, time.Second, "#B5");

				File.SetLastWriteTimeUtc (path, new DateTime (2002, 4, 6, 4, 6, 4));
				time = File.GetLastWriteTimeUtc (path);
				Assert.AreEqual (2002, time.Year, "#C1");
				Assert.AreEqual (4, time.Month, "#C2");
				Assert.AreEqual (6, time.Day, "#C3");
				Assert.AreEqual (4, time.Hour, "#C4");
				Assert.AreEqual (4, time.Second, "#C5");

				time = TimeZone.CurrentTimeZone.ToUniversalTime (File.GetLastWriteTime (path));
				Assert.AreEqual (2002, time.Year, "#D1");
				Assert.AreEqual (4, time.Month, "#D2");
				Assert.AreEqual (6, time.Day, "#D3");
				Assert.AreEqual (4, time.Hour, "#D4");
				Assert.AreEqual (4, time.Second, "#D5");
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeException1 ()
		{
			File.GetCreationTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeException2 ()
		{
			File.GetCreationTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException(typeof(IOException))]
#endif
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetCreationTimeException3";
			DeleteFile (path);
			DateTime time = File.GetCreationTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeException4 ()
		{
			File.GetCreationTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeUtcException1 ()
		{
			File.GetCreationTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeUtcException2 ()
		{
			File.GetCreationTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetCreationTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetCreationTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeUtcException4 ()
		{
			File.GetCreationTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
		public void GetCreationTimeUtcException5 ()
		{
			File.GetCreationTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeException1 ()
		{
			File.GetLastAccessTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeException2 ()
		{
			File.GetLastAccessTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeException3";
			DeleteFile (path);
			DateTime time = File.GetLastAccessTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeException4 ()
		{
			File.GetLastAccessTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeException5 ()
		{
			File.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeUtcException1 ()
		{
			File.GetLastAccessTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeUtcException2 ()
		{
			File.GetLastAccessTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastAccessTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeUtcException4 ()
		{
			File.GetLastAccessTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
		public void GetLastAccessTimeUtcException5 ()
		{
			File.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetLastWriteTimeException1 ()
		{
			File.GetLastWriteTime (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException2 ()
		{
			File.GetLastWriteTime ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		public void GetLastWriteTime_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastAccessTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastWriteTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException4 ()
		{
			File.GetLastWriteTime ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeException5 ()
		{
			File.GetLastWriteTime (Path.InvalidPathChars [0].ToString ());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetLastWriteTimeUtcException1 ()
		{
			File.GetLastWriteTimeUtc (null as string);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException2 ()
		{
			File.GetLastWriteTimeUtc ("");
		}
	
		[Test]
#if !NET_2_0
		[ExpectedException (typeof (IOException))]
#endif
		public void GetLastWriteTimeUtc_NonExistingPath ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "GetLastWriteTimeUtcException3";
			DeleteFile (path);
			DateTime time = File.GetLastWriteTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException4 ()
		{
			File.GetLastWriteTimeUtc ("    ");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetLastWriteTimeUtcException5 ()
		{
			File.GetLastWriteTimeUtc (Path.InvalidPathChars [0].ToString ());
		}		

		[Test]
		public void FileStreamClose ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "FileStreamClose";
			FileStream stream = null;
			try {
				stream = File.Create (path);
				stream.Close ();
				File.Delete (path);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		// SetCreationTime and SetCreationTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeArgumentNullException1 ()
		{
			File.SetCreationTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeArgumenException1 ()
		{
			File.SetCreationTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeArgumenException2 ()
		{
			File.SetCreationTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeArgumenException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetCreationTime (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetCreationTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetCreationTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeArgumentOutOfRangeException1";
//			FileStream stream = null;
//			DeleteFile (path);
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetCreationTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "CreationTimeIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetCreationTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcArgumentNullException1 ()
		{ 
			File.SetCreationTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcArgumenException1 ()
		{
			File.SetCreationTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcArgumenException2 ()
		{
			File.SetCreationTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcArgumentException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetCreationTimeUtc (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetCreationTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetCreationTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetCreationTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
		public void SetCreationTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetCreationTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetCreationTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		// SetLastAccessTime and SetLastAccessTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeArgumentNullException1 ()
		{
			File.SetLastAccessTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeArgumenException1 ()
		{
			File.SetLastAccessTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeArgumenException2 ()
		{
			File.SetLastAccessTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeArgumenException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetLastAccessTime (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastAccessTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastAccessTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastTimeArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastAccessTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "LastAccessIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastAccessTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeUtcArgumentNullException1 ()
		{
			File.SetLastAccessTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetCLastAccessTimeUtcArgumenException1 ()
		{
			File.SetLastAccessTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeUtcArgumenException2 ()
		{
			File.SetLastAccessTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeUtcArgumenException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetLastAccessTimeUtc (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastAccessTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastAccessTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastAccessTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
		[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
		public void SetLastAccessTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastAccessTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastAccessTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		// SetLastWriteTime and SetLastWriteTimeUtc exceptions

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetLastWriteTimeArgumentNullException1 ()
		{
			File.SetLastWriteTime (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeArgumenException1 ()
		{
			File.SetLastWriteTime ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeArgumenException2 ()
		{
			File.SetLastWriteTime ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		public void SetLastWriteTimeArgumenException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetLastWriteTime (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void SetLastWriteTimeFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastWriteTime (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastWriteTimeArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastWriteTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}

		[Test]
		[ExpectedException(typeof (IOException))]
		public void SetLastWriteTimeIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "LastWriteTimeIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastWriteTime (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SetLastWriteTimeUtcArgumentNullException1 ()
		{
			File.SetLastWriteTimeUtc (null as string, new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetCLastWriteTimeUtcArgumenException1 ()
		{
			File.SetLastWriteTimeUtc ("", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void SetLastWriteTimeUtcArgumenException2 ()
		{
			File.SetLastWriteTimeUtc ("     ", new DateTime (2000, 12, 12, 11, 59, 59));
		}

		[Test]
		public void SetLastWriteTimeUtcArgumenException3 ()
		{
			// On Unix there are no invalid path chars.
			if (Path.InvalidPathChars.Length > 1) {
				try {
					File.SetLastWriteTimeUtc (Path.InvalidPathChars [1].ToString (),
						new DateTime (2000, 12, 12, 11, 59, 59));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void SetLastWriteTimeUtcFileNotFoundException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcFileNotFoundException1";
			DeleteFile (path);
			
			File.SetLastWriteTimeUtc (path, new DateTime (2000, 12, 12, 11, 59, 59));
		}

//		[Test]
//		[ExpectedException(typeof (ArgumentOutOfRangeException))]
//		public void SetLastWriteTimeUtcArgumentOutOfRangeException1 ()
//		{
//			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcArgumentOutOfRangeException1";
//			DeleteFile (path);
//			FileStream stream = null;
//			try {
//				stream = File.Create (path);
//				stream.Close ();
//				File.SetLastWriteTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
//			} finally {
//				if (stream != null)
//					stream.Close ();
//				DeleteFile (path);
//			}
//		}
//
		[Test]
		[ExpectedException(typeof (IOException))]
		public void SetLastWriteTimeUtcIOException1 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "SetLastWriteTimeUtcIOException1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = File.Create (path);
				File.SetLastWriteTimeUtc (path, new DateTime (1000, 12, 12, 11, 59, 59));
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void OpenAppend ()
		{
			string fn = Path.GetTempFileName ();
			using (FileStream s = File.Open (fn, FileMode.Append)) {
			}
			DeleteFile (fn);
		}

#if NET_2_0
		void TestRWAT (string s)
		{
			string f = Path.GetTempFileName ();
			try {
				File.WriteAllText (f, s);
				string r = File.ReadAllText (f);
				Assert.AreEqual (s, r);
			} finally {
				DeleteFile (f);
			}
		}
		[Test]
		public void ReadWriteAllText ()
		{
			// The MSDN docs said something about
			// not including a final new line. it looks
			// like that was not true. I'm not sure what
			// that was talking about
			TestRWAT ("");
			TestRWAT ("\r");
			TestRWAT ("\n");
			TestRWAT ("\r\n");
			TestRWAT ("a\r");
			TestRWAT ("a\n");
			TestRWAT ("a\r\n");
			TestRWAT ("a\ra");
			TestRWAT ("a\na");
			TestRWAT ("a\r\na");
			TestRWAT ("a");
			TestRWAT ("\r\r");
			TestRWAT ("\n\n");
			TestRWAT ("\r\n\r\n");
		}
#endif

		static bool RunningOnUnix {
			get {
#if NET_2_0
				return Environment.OSVersion.Platform == PlatformID.Unix;
#else
				int platform = (int) Environment.OSVersion.Platform;
				return platform == 128;
#endif
			}
		}

		private void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}

#if NET_2_0
		[Test]
		public void ReplaceTest ()
		{
			string tmp = Path.Combine (TempFolder, "ReplaceTest");
			Directory.CreateDirectory (tmp);
			string origFile = Path.Combine (tmp, "origFile");
			string replaceFile = Path.Combine (tmp, "replaceFile");
			string backupFile = Path.Combine (tmp, "backupFile");

			using (StreamWriter sw = File.CreateText (origFile)) {
				sw.WriteLine ("origFile");
			}
			using (StreamWriter sw = File.CreateText (replaceFile)) {
				sw.WriteLine ("replaceFile");
			}
			using (StreamWriter sw = File.CreateText (backupFile)) {
				sw.WriteLine ("backupFile");
			}

			File.Replace (origFile, replaceFile, backupFile);
			Assert.IsFalse (File.Exists (origFile), "#1");
			using (StreamReader sr = File.OpenText (replaceFile)) {
				string txt = sr.ReadLine ();
				Assert.AreEqual ("origFile", txt, "#2");
			}
			using (StreamReader sr = File.OpenText (backupFile)) {
				string txt = sr.ReadLine ();
				Assert.AreEqual ("replaceFile", txt, "#3");
			}
		}
#endif
	}
}
