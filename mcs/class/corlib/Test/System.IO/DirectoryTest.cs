//
// System.IO.Directory
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2003 Ville Palo
//
// TODO: Find out why ArgumentOutOfRange tests does not release directories properly
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

#if !MOBILE
using Mono.Unix;
#endif
using NUnit.Framework;

namespace MonoTests.System.IO
{

[TestFixture]
public class DirectoryTest
{
	static readonly string TempSubFolder = "MonoTests.System.IO.Tests";
	string TempFolder = Path.Combine (Path.GetTempPath (), TempSubFolder);
	static readonly char DSC = Path.DirectorySeparatorChar;

	[SetUp]
	public void SetUp ()
	{
		if (!Directory.Exists (TempFolder))
			Directory.CreateDirectory (TempFolder);

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
	}
	
	[TearDown]
	public void TearDown ()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}
#if !MOBILE
	[Test] //BXC #12461
	public void EnumerateFilesListSymlinks ()
	{
		if (!RunningOnUnix)
			Assert.Ignore ("Not running on Unix.");

		var afile = Path.Combine (TempFolder, "afile.src");
		var bfile = Path.Combine (TempFolder, "bfile.src");
		var cdir = Path.Combine (TempFolder, "cdir.src");

		File.AppendAllText (afile, "hello");
		var info = new UnixFileInfo (afile);
		info.CreateSymbolicLink (bfile);
		Directory.CreateDirectory (cdir);

		var files0 = Directory.GetFiles (TempFolder, "*.src");
		Array.Sort (files0);
		Assert.AreEqual (2, files0.Length, "#1");
		Assert.AreEqual (afile, files0 [0], "#2");
		Assert.AreEqual (bfile, files0 [1], "#3");

		var files1 = new List<string> (Directory.EnumerateFiles (TempFolder, "*.src")).ToArray ();
		Array.Sort (files1);
		Assert.AreEqual (2, files1.Length, "#1.b");
		Assert.AreEqual (afile, files1 [0], "#2.b");
		Assert.AreEqual (bfile, files1 [1], "#3.b");

		var files2 = Directory.GetFileSystemEntries (TempFolder, "*.src");
		Array.Sort (files2);
		Assert.AreEqual (3, files2.Length, "#1.c");
		Assert.AreEqual (afile, files2 [0], "#2.c");
		Assert.AreEqual (bfile, files2 [1], "#3.c");
		Assert.AreEqual (cdir, files2 [2], "#4.c");

		var files3 = new List<string> (Directory.EnumerateFileSystemEntries (TempFolder, "*.src")).ToArray ();
		Array.Sort (files3);
		Assert.AreEqual (3, files3.Length, "#1.d");
		Assert.AreEqual (afile, files3 [0], "#2.d");
		Assert.AreEqual (bfile, files3 [1], "#3.d");
		Assert.AreEqual (cdir, files3 [2], "#4.d");
	}
#endif
	[Test]
	public void CreateDirectory ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.1";
		DeleteDirectory (path);
		try {
			DirectoryInfo info = Directory.CreateDirectory (path);
			Assert.IsTrue (info.Exists, "#1");
			Assert.AreEqual (".1", info.Extension, "#2");
			Assert.IsTrue (info.FullName.EndsWith ("DirectoryTest.Test.1"), "#3");
			Assert.AreEqual ("DirectoryTest.Test.1", info.Name, "#4");
		} finally {
			DeleteDirectory (path);
		}
	}
	
	/* Commented out: a directory named ":" is legal in unix
	[Test]
	public void CreateDirectoryNotSupportedException ()
	{
		DeleteDirectory (":");
		try {
			DirectoryInfo info = Directory.CreateDirectory (":");
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The path is not of a legal form
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		}
		DeleteDirectory (":");
	}
	*/

	[Test]
	public void CreateDirectory_Path_Null ()
	{
		try {
			Directory.CreateDirectory (null as string);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("path", ex.ParamName, "#5");
		}
	}

	[Test]
	public void CreateDirectory_Path_Empty ()
	{
		try {
			Directory.CreateDirectory (string.Empty);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// Path cannot be the empty string or all whitespace
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	public void CreateDirectory_Path_Whitespace ()
	{
		try {
			Directory.CreateDirectory ("            ");
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
	public void CreateDirectory_Path_InvalidChars ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test";
		DeleteDirectory (path);
		try {
			path += '\x00';
			path += ".2";
			DirectoryInfo info = Directory.CreateDirectory (path);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			// The path contains illegal characters
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNull (ex.ParamName, "#5");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void CreateDirectoryAlreadyExists ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.Exists";
		DeleteDirectory (path);
		try {
			DirectoryInfo info1 = Directory.CreateDirectory (path);
			DirectoryInfo info2 = Directory.CreateDirectory (path);

			Assert.IsTrue (info2.Exists, "#1");
			Assert.IsTrue (info2.FullName.EndsWith ("DirectoryTest.Test.Exists"), "#2");
			Assert.AreEqual ("DirectoryTest.Test.Exists", info2.Name, "#3");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void CreateDirectoryAlreadyExistsAsFile ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.ExistsAsFile";
		DeleteDirectory (path);
		DeleteFile (path);
		try {
			FileStream fstream = File.Create (path);
			fstream.Close();

			DirectoryInfo dinfo = Directory.CreateDirectory (path);
			Assert.Fail ("#1");
		} catch (IOException ex) {
			Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
			// exception message contains the path
			Assert.IsTrue (ex.Message.Contains (path), "#3");
			Assert.IsNull (ex.InnerException, "#4");
		} finally {
			DeleteDirectory (path);
			DeleteFile (path);
		}
	}

	[Test]
	public void CreateDirectoryRelativePath ()
	{
		var path = Path.Combine (TempFolder, "relativepath", "not_this_folder");
		path = Path.Combine (path, "..");

		var res = Directory.CreateDirectory (path);
		Assert.AreEqual (Path.GetFullPath (path), res.ToString (), "#1");
		Assert.IsTrue (Directory.Exists (Path.Combine (TempFolder, "relativepath")), "#2");
	}

	[Test]
	public void Delete ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.Delete.1";
		DeleteDirectory (path);
		try {
			Directory.CreateDirectory (path);
			Assert.IsTrue (Directory.Exists (path), "#1");
		
			Directory.CreateDirectory (path + DSC + "DirectoryTest.Test.Delete.1.2");
			Assert.IsTrue (Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"), "#2");
		
			Directory.Delete (path + DSC + "DirectoryTest.Test.Delete.1.2");
			Assert.IsFalse (Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"), "#3");
			Assert.IsTrue (Directory.Exists (path), "#4");
		
			Directory.Delete (path);
			Assert.IsFalse (Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"), "#5");
			Assert.IsFalse (Directory.Exists (path), "#6");
	
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path + DSC + "DirectoryTest.Test.Delete.1.2");
			Assert.IsTrue (Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"), "#7");
			Assert.IsTrue (Directory.Exists (path), "#8");
		
			Directory.Delete (path, true);
			Assert.IsFalse (Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"), "#9");
			Assert.IsFalse (Directory.Exists (path), "#10");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException ()
	{
		Directory.Delete (string.Empty);
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException2 ()
	{
		Directory.Delete ("     ");
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException3 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.4";
		DeleteDirectory (path);
		
		path += Path.InvalidPathChars [0];
		Directory.Delete (path);
	}

	[Test]	
	[ExpectedException(typeof(ArgumentNullException))]
	public void DeleteArgumentNullException ()
	{
		Directory.Delete (null as string);
	}

	[Test]	
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void DeleteDirectoryNotFoundException ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.5";
		DeleteDirectory (path);
		
		Directory.Delete (path);
	}

	[Test]	
	[ExpectedException(typeof(IOException))]
	public void DeleteArgumentException4 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.6";
		DeleteDirectory (path);
		FileStream s = null;
		Directory.CreateDirectory (path);
		try {
			s = File.Create (path + DSC + "DirectoryTest.Test.6");
			Directory.Delete (path);
		} finally {
			if (s != null)
				s.Close ();
			DeleteDirectory (path);
		};
	}

	[Test]
	public void DeleteDirectoryOnExistingFileName ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.ExistsAsFile";
		DeleteDirectory (path);
		DeleteFile (path);
		try {
			FileStream fstream = File.Create (path);
			fstream.Close ();

			Directory.Delete (path);
			Assert.Fail ("#1");
		}
		catch (IOException ex) {
			Assert.IsNull (ex.InnerException, "#4");
		}
		finally {
			DeleteDirectory (path);
			DeleteFile (path);
		}
	}

	[Test]
	public void Exists ()
	{
		Assert.IsFalse (Directory.Exists (null as string));
	}

#if !MOBILE // We don't support yet the Process class.
	[Test] // bug #78239
	public void ExistsAccessDenied ()
	{
		if (!RunningOnUnix)
			Assert.Ignore ("Not running on Unix."); // this test does not work on Windows.

		string path = TempFolder + DSC + "ExistsAccessDenied";

		Directory.CreateDirectory (path);
		global::Mono.Posix.Syscall.chmod (path, 0);
		try {
			Assert.IsFalse (Directory.Exists(path + DSC + "b"));
		} finally {
			global::Mono.Posix.Syscall.chmod (path, (global::Mono.Posix.FileMode) 755);
			Directory.Delete (path);
		}
	}
#endif
	
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetCreationTimeException1 ()
	{
		Directory.GetCreationTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeException2 ()
	{
		Directory.GetCreationTime (string.Empty);
	}
	
	[Test]
	public void GetCreationTimeException_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetCreationTime (path);

			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeException4 ()
	{
		Directory.GetCreationTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetCreationTimeUtcException1 ()
	{
		Directory.GetCreationTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeUtcException2 ()
	{
		Directory.GetCreationTimeUtc (string.Empty);
	}
	
	[Test]
	public void GetCreationTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTimeUtc.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetCreationTimeUtc (path);

			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeUtcException4 ()
	{
		Directory.GetCreationTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetCreationTimeUtcException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetLastAccessTime_Null ()
	{
		Directory.GetLastAccessTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeException2 ()
	{
		Directory.GetLastAccessTime (string.Empty);
	}
	
	[Test]
	public void GetLastAccessTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTime.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetLastAccessTime (path);

			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeException4 ()
	{
		Directory.GetLastAccessTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeException5 ()
	{
		Directory.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetLastAccessTimeUtc_Null ()
	{
		Directory.GetLastAccessTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeUtcException2 ()
	{
		Directory.GetLastAccessTimeUtc (string.Empty);
	}
	
	[Test]
	public void GetLastAccessTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastAccessTimeUtc (path);

			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeUtcException4 ()
	{
		Directory.GetLastAccessTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastAccessTimeUtcException5 ()
	{
		Directory.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetLastWriteTimeException1 ()
	{
		Directory.GetLastWriteTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeException2 ()
	{
		Directory.GetLastWriteTime (string.Empty);
	}
	
	[Test]
	public void GetLastWriteTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTime (path);

			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeException4 ()
	{
		Directory.GetLastWriteTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeException5 ()
	{
		Directory.GetLastWriteTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void GetLastWriteTimeUtcException1 ()
	{
		Directory.GetLastWriteTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeUtcException2 ()
	{
		Directory.GetLastWriteTimeUtc (string.Empty);
	}
	
	[Test]
	public void GetLastWriteTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTimeUtc (path);

			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
		} finally {
			DeleteDirectory (path);
		}
		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeUtcException4 ()
	{
		Directory.GetLastWriteTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void GetLastWriteTimeUtcException5 ()
	{
		Directory.GetLastWriteTimeUtc (Path.InvalidPathChars[0].ToString ());
	}

	[Test]
	public void Move_DestDirName_Empty ()
	{
		try {
			Directory.Move (TempFolder, string.Empty);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty file name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNotNull (ex.ParamName, "#A5");
			Assert.AreEqual ("destDirName", ex.ParamName, "#A6");
		}

		try {
			Directory.Move (TempFolder, "             ");
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// The path is not of a legal form
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void Move_DestDirName_Null ()
	{
		try {
			Directory.Move (TempFolder, (string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("destDirName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void Move_SourceDirName_Empty ()
	{
		try {
			Directory.Move (string.Empty, TempFolder);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			// Empty file name is not legal
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsNotNull (ex.ParamName, "#A5");
			Assert.AreEqual ("sourceDirName", ex.ParamName, "#A6");
		}

		try {
			Directory.Move ("             ", TempFolder);
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			// The path is not of a legal form
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void Move_SourceDirName_Null ()
	{
		try {
			Directory.Move ((string) null, TempFolder);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("sourceDirName", ex.ParamName, "#6");
		}
	}

	[Test]
	public void MoveDirectory ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.9";
		string path2 = TempFolder + DSC + "DirectoryTest.Test.10";
		DeleteDirectory (path);
		DeleteDirectory (path2);
		try {
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path + DSC + "dir");
			Assert.IsTrue (Directory.Exists (path + DSC + "dir"), "#1");
		
			Directory.Move (path, path2);
			Assert.IsFalse (Directory.Exists (path + DSC + "dir"), "#2");
			Assert.IsTrue (Directory.Exists (path2 + DSC + "dir"), "#3");
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);
			if (Directory.Exists (path2 + DSC + "dir"))
				Directory.Delete (path2 + DSC + "dir", true);
		}
	}

	[Test]
	[ExpectedException (typeof (IOException))]
	public void MoveDirectory_Same ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.8";
		DeleteDirectory (path);
		try {
			Directory.Move (path, path);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void MoveFile ()
	{
		string tempFile1 = Path.Combine (TempFolder, "temp1.txt");
		string tempFile2 = Path.Combine (TempFolder, "temp2.txt");

		using (StreamWriter sw = File.CreateText (tempFile1)) {
			sw.Write ("temp1");
		}
		Assert.IsFalse (File.Exists (tempFile2), "#1");
		Directory.Move (tempFile1, tempFile2);
		Assert.IsFalse (File.Exists (tempFile1), "#2");
		Assert.IsTrue (File.Exists (tempFile2), "#3");
		using (StreamReader sr = File.OpenText (tempFile2)) {
			Assert.AreEqual ("temp1", sr.ReadToEnd (), "#4");
		}
	}

	[Test]
	public void MoveFile_DestDir_Exists ()
	{
		string tempFile = Path.Combine (TempFolder, "temp1.txt");
		string tempDir = Path.Combine (TempFolder, "temp2");

		using (StreamWriter sw = File.CreateText (tempFile)) {
			sw.Write ("temp1");
		}
		Directory.CreateDirectory (tempDir);

		try {
			Directory.Move (tempFile, tempDir);
			Assert.Fail ("#A1");
		} catch (IOException ex) {
			// Cannot create a file when that file already exists
			Assert.AreEqual (typeof (IOException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		Assert.IsTrue (File.Exists (tempFile), "#B1");
		Assert.IsFalse (File.Exists (tempDir), "#B2");
		Assert.IsTrue (Directory.Exists (tempDir), "#B3");
	}

	[Test]
	public void MoveFile_DestFile_Exists ()
	{
		string tempFile1 = Path.Combine (TempFolder, "temp1.txt");
		string tempFile2 = Path.Combine (TempFolder, "temp2.txt");

		using (StreamWriter sw = File.CreateText (tempFile1)) {
			sw.Write ("temp1");
		}
		using (StreamWriter sw = File.CreateText (tempFile2)) {
			sw.Write ("temp2");
		}

		try {
			Directory.Move (tempFile1, tempFile2);
			Assert.Fail ("#A1");
		} catch (IOException ex) {
			// Cannot create a file when that file already exists
			Assert.AreEqual (typeof (IOException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		Assert.IsTrue (File.Exists (tempFile1), "#B1");
		using (StreamReader sr = File.OpenText (tempFile1)) {
			Assert.AreEqual ("temp1", sr.ReadToEnd (), "#B2");
		}

		Assert.IsTrue (File.Exists (tempFile2), "#C1");
		using (StreamReader sr = File.OpenText (tempFile2)) {
			Assert.AreEqual ("temp2", sr.ReadToEnd (), "#C2");
		}
	}

	[Test]
	public void MoveFile_Same ()
	{
		string tempFile = Path.Combine (TempFolder, "temp.txt");

		try {
			Directory.Move (tempFile, tempFile);
			Assert.Fail ("#1");
		} catch (IOException ex) {
			// Source and destination path must be different
			Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Ignore ("On IA64, causes nunit to abort due to bug #76388")]
	public void MoveException4 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.13";
		path += Path.InvalidPathChars [0];
		string path2 = TempFolder + DSC + "DirectoryTest.Test.13";
		DeleteDirectory (path);
		DeleteDirectory (path2);
		try {
			Directory.CreateDirectory (path2);
			Directory.Move (path2, path);
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);
		}
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void MoveException5 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.14";
		DeleteDirectory (path);
		try {
			Directory.Move (path, path + "Test.Test");
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path + "Test.Test");
		}
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveDirectory_Dest_SubDir ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.15";
		DeleteDirectory (path);
		try {
			Directory.CreateDirectory (path);
			Directory.Move (path, path + DSC + "dir");
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path + DSC + "dir");
		}
	}

	[Test]
	[ExpectedException (typeof (IOException))]
	public void MoveDirectory_Dest_Exists ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.16";
		string path2 = TempFolder + DSC + "DirectoryTest.Test.17";
		
		DeleteDirectory (path);
		DeleteDirectory (path2);
		try {
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path2);
			Directory.Move (path, path2);
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);
		}
	}
	
	[Test]
	public void CreationTime ()
	{
		if (RunningOnUnix)
			Assert.Ignore ("Unix doesn't support CreationTime");

		string path = TempFolder + DSC + "DirectoryTest.CreationTime.1";
		DeleteDirectory (path);
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetCreationTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetCreationTime (path);
			Assert.AreEqual (2003, time.Year, "#A1");
			Assert.AreEqual (6, time.Month, "#A2");
			Assert.AreEqual (4, time.Day, "#A3");
			Assert.AreEqual (6, time.Hour, "#A4");
			Assert.AreEqual (4, time.Minute, "#A5");
			Assert.AreEqual (0, time.Second, "#A6");
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetCreationTimeUtc (path));
			Assert.AreEqual (2003, time.Year, "#B1");
			Assert.AreEqual (6, time.Month, "#B2");
			Assert.AreEqual (4, time.Day, "#B3");
			Assert.AreEqual (6, time.Hour, "#B4");
			Assert.AreEqual (4, time.Minute, "#B5");
			Assert.AreEqual (0, time.Second, "#B6");

			Directory.SetCreationTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetCreationTime (path));
			Assert.AreEqual (2003, time.Year, "#C1");
			Assert.AreEqual (6, time.Month, "#C2");
			Assert.AreEqual (4, time.Day, "#C3");
			Assert.AreEqual (6, time.Hour, "#C4");
			Assert.AreEqual (4, time.Minute, "#C5");
			Assert.AreEqual (0, time.Second, "#C6");

			time = Directory.GetCreationTimeUtc (path);
			Assert.AreEqual (2003, time.Year, "#D1");
			Assert.AreEqual (6, time.Month, "#D2");
			Assert.AreEqual (4, time.Day, "#D3");
			Assert.AreEqual (6, time.Hour, "#D4");
			Assert.AreEqual (4, time.Minute, "#D5");
			Assert.AreEqual (0, time.Second, "#D6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[Category ("NotWasm")]
	public void LastAccessTime ()
	{
		string path = TempFolder + DSC + "DirectoryTest.AccessTime.1";
		DeleteDirectory (path);
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetLastAccessTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetLastAccessTime (path);
			Assert.AreEqual (2003, time.Year, "#A1");
			Assert.AreEqual (6, time.Month, "#A2");
			Assert.AreEqual (4, time.Day, "#A3");
			Assert.AreEqual (6, time.Hour, "#A4");
			Assert.AreEqual (4, time.Minute, "#A5");
			Assert.AreEqual (0, time.Second, "#A6");
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastAccessTimeUtc (path));
			Assert.AreEqual (2003, time.Year, "#B1");
			Assert.AreEqual (6, time.Month, "#B2");
			Assert.AreEqual (4, time.Day, "#B3");
			Assert.AreEqual (6, time.Hour, "#B4");
			Assert.AreEqual (4, time.Minute, "#B5");
			Assert.AreEqual (0, time.Second, "#B6");

			Directory.SetLastAccessTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastAccessTime (path));
			Assert.AreEqual (2003, time.Year, "#C1");
			Assert.AreEqual (6, time.Month, "#C2");
			Assert.AreEqual (4, time.Day, "#C3");
			Assert.AreEqual (6, time.Hour, "#C4");
			Assert.AreEqual (4, time.Minute, "#C5");
			Assert.AreEqual (0, time.Second, "#C6");

			time = Directory.GetLastAccessTimeUtc (path);
			Assert.AreEqual (2003, time.Year, "#D1");
			Assert.AreEqual (6, time.Month, "#D2");
			Assert.AreEqual (4, time.Day, "#D3");
			Assert.AreEqual (6, time.Hour, "#D4");
			Assert.AreEqual (4, time.Minute, "#D5");
			Assert.AreEqual (0, time.Second, "#D6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void LastWriteTime ()
	{
		string path = TempFolder + DSC + "DirectoryTest.WriteTime.1";
		DeleteDirectory (path);
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetLastWriteTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetLastWriteTime (path);
			Assert.AreEqual (2003, time.Year, "#A1");
			Assert.AreEqual (6, time.Month, "#A2");
			Assert.AreEqual (4, time.Day, "#A3");
			Assert.AreEqual (6, time.Hour, "#A4");
			Assert.AreEqual (4, time.Minute, "#A5");
			Assert.AreEqual (0, time.Second, "#A6");
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastWriteTimeUtc (path));
			Assert.AreEqual (2003, time.Year, "#B1");
			Assert.AreEqual (6, time.Month, "#B2");
			Assert.AreEqual (4, time.Day, "#B3");
			Assert.AreEqual (6, time.Hour, "#B4");
			Assert.AreEqual (4, time.Minute, "#B5");
			Assert.AreEqual (0, time.Second, "#B6");

			Directory.SetLastWriteTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastWriteTime (path));
			Assert.AreEqual (2003, time.Year, "#C1");
			Assert.AreEqual (6, time.Month, "#C2");
			Assert.AreEqual (4, time.Day, "#C3");
			Assert.AreEqual (6, time.Hour, "#C4");
			Assert.AreEqual (4, time.Minute, "#C5");
			Assert.AreEqual (0, time.Second, "#C6");

			time = Directory.GetLastWriteTimeUtc (path);
			Assert.AreEqual (2003, time.Year, "#D1");
			Assert.AreEqual (6, time.Month, "#D2");
			Assert.AreEqual (4, time.Day, "#D3");
			Assert.AreEqual (6, time.Hour, "#D4");
			Assert.AreEqual (4, time.Minute, "#D5");
			Assert.AreEqual (0, time.Second, "#D6");
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetLastWriteTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTime (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetLastWriteTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTime.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastWriteTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastWriteTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastWriteTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetLastWriteTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + Path.DirectorySeparatorChar + "DirectoryTest.SetLastWriteTime.1";
//
//		try {
//			if (!Directory.Exists (path))
//				Directory.CreateDirectory (path);
//		
//			Directory.SetLastWriteTime (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetLastWriteTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastWriteTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTimeUtc (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetLastWriteTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTimeUtc.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastWriteTimeUtc (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastWriteTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastWriteTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetLastWriteTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastWriteTimeUtc (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetLastAccessTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetLastAccessTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTime.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastAccessTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetLastAccessTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTime.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastAccessTime (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetLastAccessTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetLastAccessTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastAccessTimeUtc (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetLastAccessTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetLastAccessTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastAccessTimeUtc (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetCreationTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetCreationTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetCreationTime.2";
		DeleteDirectory (path);
		
		try {
			Directory.SetCreationTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetCreationTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetCreationTime.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetCreationTime (path, time);
//			DeleteDirectory (path);
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetCreationTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc (string.Empty, time);
	}
	
	[Test]
	[ExpectedException()]
	public void SetCreationTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		
		try {
			Directory.SetCreationTimeUtc (path, time);
			DeleteDirectory (path);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void SetCreationTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]
//	public void SetCreationTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetCreationTimeUtc (path, time);
//			DeleteDirectory (path);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	public void GetDirectories ()
	{
		string path = TempFolder;
		string DirPath = TempFolder + Path.DirectorySeparatorChar + ".GetDirectories";
		DeleteDirectory (DirPath);
		
		try {
			Directory.CreateDirectory (DirPath);
		
			string [] dirs = Directory.GetDirectories (path);
		
			foreach (string directory in dirs) {
			
				if (directory == DirPath)
					return;
			}
		
			Assert.Fail ("Directory Not Found");
		} finally {
			DeleteDirectory (DirPath);
		}
	}

	[Test] // bug #346123
	[Category ("NotWasm")]
	public void GetDirectories_Backslash ()
	{
		if (!RunningOnUnix)
			// on Windows, backslash is used as directory separator
			Assert.Ignore ("Not running on Unix.");

		string dir = Path.Combine (TempFolder, @"sub\dir");
		Directory.CreateDirectory (dir);

		Assert.IsTrue (Directory.Exists (dir), "#A1");
		Assert.IsFalse (Directory.Exists (Path.Combine (TempFolder, "dir")), "#A2");

		string [] dirs = Directory.GetDirectories (TempFolder);
		Assert.AreEqual (1, dirs.Length, "#B1");
		Assert.AreEqual (dir, dirs [0], "#B2");
	}

	[Test]
	public void GetParentOfRootDirectory ()
	{
		DirectoryInfo info;

		info = Directory.GetParent (Path.GetPathRoot (Path.GetTempPath ()));
		Assert.IsNull (info);
	}

	[Test]
	public void GetDirectoryRoot ()
	{
		if (RunningOnUnix)
		{
			string path = "/usr/lib";
			Assert.AreEqual ("/", Directory.GetDirectoryRoot (path));
		}
		else
		{
			Assert.Ignore ("TODO: no proper implementation on Windows.");
			string path = "C:\\Windows";
			Assert.AreEqual ("C:\\", Directory.GetDirectoryRoot (path));
		}
	}

	[Test]
	[Category ("NotWasm")]
	public void GetFiles ()
	{
		string path = TempFolder;
		string DirPath = TempFolder + Path.DirectorySeparatorChar + ".GetFiles";
		if (File.Exists (DirPath))
			File.Delete (DirPath);
		
		try {
			File.Create (DirPath).Close ();
			string [] files = Directory.GetFiles (TempFolder);
			foreach (string directory in files) {
			
				if (directory == DirPath)
					return;
			}
		
			Assert.Fail ("File Not Found");
		} finally {
			if (File.Exists (DirPath))
				File.Delete (DirPath);
		}
	}

	[Test] // bug #346123
	[Category ("NotWasm")]
	public void GetFiles_Backslash ()
	{
		if (!RunningOnUnix)
			// on Windows, backslash is used as directory separator
			Assert.Ignore ("Not running on Unix.");

		string file = Path.Combine (TempFolder, @"doc\temp1.file");
		File.Create (file).Close ();

		Assert.IsTrue (File.Exists (file), "#A1");
		Assert.IsFalse (File.Exists (Path.Combine (TempFolder, "temp1.file")), "#A2");

		string [] files = Directory.GetFiles (TempFolder);
		Assert.AreEqual (1, files.Length, "#B1");
		Assert.AreEqual (file, files [0], "#B2");
	}

	[Test] // bug #82212 and bug #325107
	public void GetFiles_Pattern ()
	{
		string [] files = Directory.GetFiles (TempFolder, "*.*");
		Assert.IsNotNull (files, "#A1");
		Assert.AreEqual (0, files.Length, "#A2");

		string tempFile1 = Path.Combine (TempFolder, "tempFile1");
		File.Create (tempFile1).Close ();

		files = Directory.GetFiles (TempFolder, "*.*");
		Assert.IsNotNull (files, "#B1");
		Assert.AreEqual (1, files.Length, "#B2");
		Assert.AreEqual (tempFile1, files [0], "#B3");

		string tempFile2 = Path.Combine (TempFolder, "FileTemp2.tmp");
		File.Create (tempFile2).Close ();

		files = Directory.GetFiles (TempFolder, "*.*");
		Assert.IsNotNull (files, "#C1");
		Assert.AreEqual (2, files.Length, "#C2");

		files = Directory.GetFiles (TempFolder, "temp*.*");
		Assert.IsNotNull (files, "#D1");
		Assert.AreEqual (1, files.Length, "#D2");
		Assert.AreEqual (tempFile1, files [0], "#D3");

		string tempFile3 = Path.Combine (TempFolder, "tempFile3.txt");
		File.Create (tempFile3).Close ();

		files = Directory.GetFiles (TempFolder, "*File*.*");
		Assert.IsNotNull (files, "#E1");
		Assert.AreEqual (3, files.Length, "#E2");

		files = Directory.GetFiles (TempFolder, "*File*.tmp");
		Assert.IsNotNull (files, "#F1");
		Assert.AreEqual (1, files.Length, "#F2");
		Assert.AreEqual (tempFile2, files [0], "#F3");

		files = Directory.GetFiles (TempFolder, "*tempFile*");
		Assert.IsNotNull (files, "#G1");
		Assert.AreEqual (2, files.Length, "#G2");

		files = Directory.GetFiles (TempFolder, "*tempFile1");
		Assert.IsNotNull (files, "#H1");
		Assert.AreEqual (1, files.Length, "#H2");
		Assert.AreEqual (tempFile1, files [0], "#H3");

		files = Directory.GetFiles (TempFolder, "*.txt");
		Assert.IsNotNull (files, "#I1");
		Assert.AreEqual (1, files.Length, "#I2");
		Assert.AreEqual (tempFile3, files [0], "#I3");

		files = Directory.GetFiles (TempFolder, "*.t*");
		Assert.IsNotNull (files, "#J1");
		Assert.AreEqual (2, files.Length, "#J2");

		files = Directory.GetFiles (TempFolder, "temp*.*");
		Assert.IsNotNull (files, "#K1");
		Assert.AreEqual (2, files.Length, "#K2");

		File.Delete (tempFile1);

		files = Directory.GetFiles (TempFolder, "temp*.*");
		Assert.IsNotNull (files, "#L1");
		Assert.AreEqual (1, files.Length, "#L2");
		Assert.AreEqual (tempFile3, files [0], "#L3");

		files = Directory.GetFiles (TempFolder, ".*");
		Assert.IsNotNull (files, "#M1");
		Assert.AreEqual (0, files.Length, "#M2");

		string tempFile4 = Path.Combine (TempFolder, "tempFile4.");
		File.Create (tempFile4).Close ();

		files = Directory.GetFiles (TempFolder, "temp*.");
		Assert.IsNotNull (files, "#N1");
		Assert.AreEqual (1, files.Length, "#N2");
		if (RunningOnUnix)
			Assert.AreEqual (tempFile4, files [0], "#N3");
		else // on Windows, the trailing dot is automatically trimmed
			Assert.AreEqual (Path.Combine (TempFolder, "tempFile4"), files [0], "#N3");
	}

	[Test]
	public void GetFiles_580090 ()
	{
		string cwd = Directory.GetCurrentDirectory ();
		Directory.SetCurrentDirectory (Path.GetTempPath ());

		string tempFile = Path.Combine (TempFolder, "tempFile.txt");
		File.Create (tempFile).Close ();

		try {
			string [] files = Directory.GetFiles (".", TempSubFolder + DSC + "*.t*");
			Assert.IsNotNull (files, "#J1");
			Assert.AreEqual (1, files.Length, "#J2");
		}
		finally	{
			Directory.SetCurrentDirectory (cwd);
		}
	}

	
	[Test]
	public void GetFiles_SubDirInPattern ()
	{
		string DirPath = TempFolder + Path.DirectorySeparatorChar + "GetFiles_SubDirInPattern";
		if (Directory.Exists (DirPath))
			Directory.Delete (DirPath, true);

		Directory.CreateDirectory ($"{DirPath}{Path.DirectorySeparatorChar}something{Path.DirectorySeparatorChar}else");
		File.WriteAllText($"{DirPath}{Path.DirectorySeparatorChar}something{Path.DirectorySeparatorChar}else{Path.DirectorySeparatorChar}file", "hello");

		var r = Directory.GetFiles (DirPath, $"something{Path.DirectorySeparatorChar}else{Path.DirectorySeparatorChar}*", SearchOption.AllDirectories);
		Assert.AreEqual (new string[] { Path.Combine (DirPath, "something", "else", "file") }, r);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SetCurrentDirectoryNull ()
	{
		Directory.SetCurrentDirectory (null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void SetCurrentDirectoryEmpty ()
	{
		Directory.SetCurrentDirectory (String.Empty);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void SetCurrentDirectoryWhitespace ()
	{
		Directory.SetCurrentDirectory (" ");
	}

	[Test] // https://github.com/mono/mono/issues/13030
	public void GetLogicalDrivesNotEmpty ()
	{
		CollectionAssert.IsNotEmpty (Directory.GetLogicalDrives ());
	}

	[Test]
	[Category("AndroidSdksNotWorking")]
	public void GetNoFiles () // Bug 58875. This throwed an exception on windows.
	{
		DirectoryInfo dir = new DirectoryInfo (".");
		dir.GetFiles ("*.nonext");
	}

	[Test]
	public void FilenameOnly () // bug 78209
	{
		Directory.GetParent ("somefile");
	}

	private static bool RunningOnUnix {
		get {
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			return ((platform == 4) || (platform == 128) || (platform == 6));
		}
	}

#if !MOBILE
	[Test]
	public void ResolvePathBeforeDirectoryExists ()
	{
		if (!RunningOnUnix)
			Assert.Ignore ("Not running on Unix.");

		string cwd = Directory.GetCurrentDirectory ();

		string root = Path.Combine (TempFolder, "test_ResolvePathBeforeExists");
		string testPath = Path.Combine (root, "test");
		string test2Path = Path.Combine (testPath, "test2");
		string testFile = Path.Combine (test2Path, "test_file");
		string symlinkPath = Path.Combine (root, "test3");
		try 
		{	
			Directory.CreateDirectory (root);
			Directory.SetCurrentDirectory (root);
			Directory.CreateDirectory (testPath);
			Directory.CreateDirectory (test2Path);
			File.WriteAllText (testFile, "hello");

			var info = new UnixFileInfo (test2Path);
			info.CreateSymbolicLink (symlinkPath);

			var partial_path_with_symlink = "test3/../test3"; // test3 is a symlink to test/test2

			Assert.AreEqual (Directory.Exists (partial_path_with_symlink), true);
		}
		finally 
		{
			DeleteFile (symlinkPath);
			DeleteFile (testFile);
			DeleteDirectory (test2Path);
			DeleteDirectory (testPath);
			Directory.SetCurrentDirectory (cwd);
			DeleteDirectory (root);
		}
	}
#endif

	private void DeleteDirectory (string path)
	{
		if (Directory.Exists (path))
			Directory.Delete (path, true);
	}

	private void DeleteFile (string path)
	{
		if (File.Exists (path))
			File.Delete (path);
	}
}
}
