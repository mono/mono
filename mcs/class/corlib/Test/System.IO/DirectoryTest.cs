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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.IO
{

[TestFixture]
public class DirectoryTest
{
	string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	static readonly char DSC = Path.DirectorySeparatorChar;

	[SetUp]
	public void SetUp ()
	{
		if (!Directory.Exists (TempFolder))
			Directory.CreateDirectory (TempFolder);

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
	}
	
	[TearDown]
	public void TearDown () {
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}

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
	
	[Test]
	public void CreateDirectoryNotSupportedException ()
	{
		DeleteDirectory (":");
		try {
			DirectoryInfo info = Directory.CreateDirectory (":");
			Assert.Fail ();
		} catch (ArgumentException) {
		}
		DeleteDirectory (":");
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CreateDirectoryArgumentNullException ()
	{
		DirectoryInfo info = Directory.CreateDirectory (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException1 ()
	{
		DirectoryInfo info = Directory.CreateDirectory ("");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException2 ()
	{
		DirectoryInfo info = Directory.CreateDirectory ("            ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException3 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test";
		DeleteDirectory (path);
		try {
			path += '\x00';
			path += ".2";
			DirectoryInfo info = Directory.CreateDirectory (path);
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
#if NET_2_0
			Assert.Fail ("#1");
		} catch (IOException ex) {
			Assert.AreEqual (typeof (IOException), ex.GetType (), "#2");
			Assert.IsNotNull (ex.Message, "#3");
			Assert.IsNull (ex.InnerException, "#4");
#else
			Assert.IsFalse (dinfo.Exists, "#2");
			Assert.IsTrue (dinfo.FullName.EndsWith ("DirectoryTest.Test.ExistsAsFile"), "#3");
			Assert.AreEqual ("DirectoryTest.Test.ExistsAsFile", dinfo.Name, "#4");
#endif
		} finally {
			DeleteDirectory (path);
			DeleteFile (path);
		}
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
		Directory.Delete ("");
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
	public void Exists ()
	{
		Assert.IsFalse (Directory.Exists (null as string));
	}

#if !TARGET_JVM // We don't support yet the Process class.
	[Test]
	[Category("NotDotNet")]
	public void ExistsAccessDenied ()
	{
		// bug #78239

		if (Path.DirectorySeparatorChar == '\\')
			return; // this test does not work on Windows.

		string path = TempFolder + DSC + "ExistsAccessDenied";

		Directory.CreateDirectory (path);
		Process.Start ("/bin/chmod", "000 " + path).WaitForExit ();
		try {
			Assert.IsFalse (Directory.Exists(path + DSC + "b"));
		} finally {
			Process.Start ("/bin/chmod", "755 " + path).WaitForExit ();
			Directory.Delete (path);
		}
	}
#endif
	
	[Test]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	[ExpectedException(typeof(ArgumentNullException))]
    public void GetCreationTimeException1 ()
	{
		Directory.GetCreationTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeException2 ()
	{
		Directory.GetCreationTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException(typeof(IOException))]
#endif
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeException_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetCreationTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeException4 ()
	{
		Directory.GetCreationTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeUtcException1 ()
	{
		Directory.GetCreationTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeUtcException2 ()
	{
		Directory.GetCreationTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTimeUtc.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetCreationTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeUtcException4 ()
	{
		Directory.GetCreationTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetCreationTime not supported for TARGET_JVM
	public void GetCreationTimeUtcException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTime_Null ()
	{
		Directory.GetLastAccessTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeException2 ()
	{
		Directory.GetLastAccessTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTime.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetLastAccessTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeException4 ()
	{
		Directory.GetLastAccessTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeException5 ()
	{
		Directory.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeUtc_Null ()
	{
		Directory.GetLastAccessTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeUtcException2 ()
	{
		Directory.GetLastAccessTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastAccessTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
	public void GetLastAccessTimeUtcException4 ()
	{
		Directory.GetLastAccessTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // GetLastAccessTime not supported for TARGET_JVM
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
		Directory.GetLastWriteTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastWriteTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assert.AreEqual (expectedTime.Year, time.Year, "#1");
			Assert.AreEqual (expectedTime.Month, time.Month, "#2");
			Assert.AreEqual (expectedTime.Day, time.Day, "#3");
			Assert.AreEqual (expectedTime.Hour, time.Hour, "#4");
			Assert.AreEqual (expectedTime.Second, time.Second, "#5");
			Assert.AreEqual (expectedTime.Millisecond, time.Millisecond, "#6");
#endif
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
		Directory.GetLastWriteTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastWriteTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTimeUtc (path);

#if NET_2_0
			Assert.AreEqual (1601, time.Year, "#1");
			Assert.AreEqual (1, time.Month, "#2");
			Assert.AreEqual (1, time.Day, "#3");
			Assert.AreEqual (0, time.Hour, "#4");
			Assert.AreEqual (0, time.Second, "#5");
			Assert.AreEqual (0, time.Millisecond, "#6");
#endif
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
	[Category("TargetJvmNotSupported")] // CreationTime not supported for TARGET_JVM
	public void CreationTime ()
	{
		// check for Unix platforms - see FAQ for more details
		// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
		int platform = (int) Environment.OSVersion.Platform;
		if ((platform == 4) || (platform == 128))
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
	[Category("TargetJvmNotSupported")] // LastAccessTime not supported for TARGET_JVM
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
		Directory.SetLastWriteTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
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
		Directory.SetLastWriteTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
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
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
	public void SetLastAccessTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetLastAccessTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
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
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
	public void SetCreationTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	[Category("TargetJvmNotSupported")] // SetCreationTime not supported for TARGET_JVM
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

	[Test]
	public void GetParentOfRootDirectory ()
	{
		DirectoryInfo info;

		info = Directory.GetParent (Path.GetPathRoot (Path.GetTempPath ()));
		Assert.IsNull (info);
	}
	
	[Test]
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


	[Test]
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
