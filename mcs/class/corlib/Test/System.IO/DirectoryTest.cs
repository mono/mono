//
// System.IO.Directory
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2003 Ville Palo
//

using NUnit.Framework;
using System.IO;
using System.Text;
using System;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.IO {

[TestFixture]
public class DirectoryTest {
	
	string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	
	public DirectoryTest () 
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
		Directory.CreateDirectory (TempFolder);
	}

	~DirectoryTest ()
	{
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}
	
	[SetUp]
	public void SetUp ()
	{
		if (!Directory.Exists (TempFolder))				
			Directory.CreateDirectory (TempFolder);

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
	}
	
	
	[Test]
	public void CreateDirectory ()
	{
		string path = TempFolder + "/DirectoryTest.Test.1";
		DeleteDirectory (path);
		
		DirectoryInfo info = Directory.CreateDirectory (path);
		Assertion.AssertEquals ("test#01", true, info.Exists);
		Assertion.AssertEquals ("test#02", ".1", info.Extension);
		Assertion.AssertEquals ("test#03", true, info.FullName.EndsWith ("DirectoryTest.Test.1"));
		Assertion.AssertEquals ("test#04", "DirectoryTest.Test.1", info.Name);

		DeleteDirectory (path);		
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void CreateDirectoryNotSupportedException ()
	{
		DeleteDirectory (":");
		DirectoryInfo info = Directory.CreateDirectory (":");
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
		string path = TempFolder + "/DirectoryTest.Test";
		DeleteDirectory (path);

		path += Path.InvalidPathChars [0];
		path += ".2";
		DirectoryInfo info = Directory.CreateDirectory (path);		
		
		DeleteDirectory (path);
	}

	[Test]
	public void Delete ()
	{
		string path = TempFolder + "/DirectoryTest.Test.Delete.1";
		DeleteDirectory (path);
		
		Directory.CreateDirectory (path);
		Assertion.AssertEquals ("test#01", true, Directory.Exists (path));
		
		Directory.CreateDirectory (path + "/DirectoryTest.Test.Delete.1.2");
		Assertion.AssertEquals ("test#02", true, Directory.Exists (path + "/DirectoryTest.Test.Delete.1.2"));
		
		Directory.Delete (path + "/DirectoryTest.Test.Delete.1.2");
		Assertion.AssertEquals ("test#03", false, Directory.Exists (path + "/DirectoryTest.Test.Delete.1.2"));
		Assertion.AssertEquals ("test#04", true, Directory.Exists (path));
		
		Directory.Delete (path);
		Assertion.AssertEquals ("test#05", false, Directory.Exists (path + "/DirectoryTest.Test.Delete.1.2"));
		Assertion.AssertEquals ("test#06", false, Directory.Exists (path));		
	
		Directory.CreateDirectory (path);
		Directory.CreateDirectory (path + "/DirectoryTest.Test.Delete.1.2");
		Assertion.AssertEquals ("test#07", true, Directory.Exists (path + "/DirectoryTest.Test.Delete.1.2"));
		Assertion.AssertEquals ("test#08", true, Directory.Exists (path));
		
		Directory.Delete (path, true);
		Assertion.AssertEquals ("test#09", false, Directory.Exists (path + "/DirectoryTest.Test.Delete.1.2"));
		Assertion.AssertEquals ("test#10", false, Directory.Exists (path));
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
		string path = TempFolder + "/DirectoryTest.Test.4";
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
		string path = TempFolder + "/DirectoryTest.Test.5";
		DeleteDirectory (path);
		
		Directory.Delete (path);		
	}

	[Test]	
	[ExpectedException(typeof(IOException))]
	public void DeleteArgumentException4 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.6";
		DeleteDirectory (path);
		
		Directory.CreateDirectory (path);
		File.Create (path + "/DirectoryTest.Test.6");

		DeleteDirectory (path);	
	}

	[Test]
	public void Exists ()
	{
		Assertion.AssertEquals ("test#01", false, Directory.Exists (null as string));
	}
	
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
		Directory.GetCreationTime ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetCreationTimeException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetCreationTime.1";
		DeleteDirectory (path);
		
		Directory.GetCreationTime (path);
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
		Directory.GetCreationTimeUtc ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetCreationTimeUtcException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetCreationTimeUtc.1";
		DeleteDirectory (path);
		
		Directory.GetCreationTimeUtc (path);
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
	public void GetLastAccessTimeException1 ()
	{
		Directory.GetLastAccessTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeException2 ()
	{
		Directory.GetLastAccessTime ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetLastAccessTimeException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetLastAccessTime.1";
		DeleteDirectory (path);
		
		Directory.GetLastAccessTime (path);
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
	public void GetLastAccessTimeUtcException1 ()
	{
		Directory.GetLastAccessTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeUtcException2 ()
	{
		Directory.GetLastAccessTimeUtc ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetLastAccessTimeUtcException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetLastAccessTimeUtc.1";
		DeleteDirectory (path);
		
		Directory.GetLastAccessTimeUtc (path);
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
		Directory.GetLastWriteTime ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetLastWriteTimeException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetLastWriteTime.1";
		DeleteDirectory (path);
		
		Directory.GetLastWriteTime (path);
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
		Directory.GetLastAccessTimeUtc ("");
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void GetLastWriteTimeUtcException3 ()
	{
		string path = TempFolder + "/DirectoryTest.GetLastWriteTimeUtc.1";
		DeleteDirectory (path);
		Directory.GetLastAccessTimeUtc (path);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeUtcException4 ()
	{
		Directory.GetLastAccessTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeUtcException5 ()
	{
		Directory.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	public void Move ()
	{
		string path = TempFolder + "/DirectoryTest.Test.9";
		string path2 = TempFolder + "/DirectoryTest.Test.10";
		DeleteDirectory (path);
		DeleteDirectory (path2);
		
		Directory.CreateDirectory (path);
		Directory.CreateDirectory (path + "/" + "dir");
		Assertion.AssertEquals ("test#01", true, Directory.Exists (path + "/" + "dir"));
		
		Directory.Move (path, path2);
		Assertion.AssertEquals ("test#02", false, Directory.Exists (path + "/" + "dir"));		
		Assertion.AssertEquals ("test#03", true, Directory.Exists (path2 + "/" + "dir"));
		
		if (Directory.Exists (path2 + "/" + "dir"))
			Directory.Delete (path2 + "/" + "dir", true);
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException1 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.8";
		DeleteDirectory (path);		
		Directory.Move (path, path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException2 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.11";
		DeleteDirectory (path);		
		Directory.Move ("", path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException3 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.12";
		DeleteDirectory (path);		
		Directory.Move ("             ", path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException4 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.13";
		path += Path.InvalidPathChars [0];
		string path2 = TempFolder + "/DirectoryTest.Test.13";		
		DeleteDirectory (path);
		DeleteDirectory (path2);

		Directory.CreateDirectory (path2);		
		Directory.Move (path2, path);
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void MoveException5 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.14";
		DeleteDirectory (path);
		
		Directory.Move (path, path + "Test.Test");		
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException6 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.15";
		DeleteDirectory (path);

		Directory.CreateDirectory (path);		
		Directory.Move (path, path + "/" + "dir");
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException7 ()
	{
		string path = TempFolder + "/DirectoryTest.Test.16";
		string path2 = TempFolder + "/DirectoryTest.Test.17";
		
		DeleteDirectory (path);
		DeleteDirectory (path2);		
		
		Directory.CreateDirectory (path);
		Directory.CreateDirectory (path2);
		Directory.Move (path, path2);

		DeleteDirectory (path);
		DeleteDirectory (path2);		
	}
	
	[Test]
	public void CreationTime ()
	{
		string path = TempFolder + "/DirectoryTest.CreationTime.1";
		DeleteDirectory (path);
		
		Directory.CreateDirectory (path);
		Directory.SetCreationTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

		DateTime time = Directory.GetCreationTime (path);		
		Assertion.AssertEquals ("test#01", 2003, time.Year);
		Assertion.AssertEquals ("test#02", 6, time.Month);
		Assertion.AssertEquals ("test#03", 4, time.Day);
		Assertion.AssertEquals ("test#04", 6, time.Hour);
		Assertion.AssertEquals ("test#05", 4, time.Minute);
		Assertion.AssertEquals ("test#06", 0, time.Second);
		
		time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetCreationTimeUtc (path));
		Assertion.AssertEquals ("test#07", 2003, time.Year);
		Assertion.AssertEquals ("test#08", 6, time.Month);
		Assertion.AssertEquals ("test#09", 4, time.Day);
		Assertion.AssertEquals ("test#10", 6, time.Hour);
		Assertion.AssertEquals ("test#11", 4, time.Minute);
		Assertion.AssertEquals ("test#12", 0, time.Second);

		Directory.SetCreationTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
		time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetCreationTime (path));
		Assertion.AssertEquals ("test#13", 2003, time.Year);
		Assertion.AssertEquals ("test#14", 6, time.Month);
		Assertion.AssertEquals ("test#15", 4, time.Day);
		Assertion.AssertEquals ("test#16", 6, time.Hour);
		Assertion.AssertEquals ("test#17", 4, time.Minute);
		Assertion.AssertEquals ("test#18", 0, time.Second);

		time = Directory.GetCreationTimeUtc (path);		
		Assertion.AssertEquals ("test#19", 2003, time.Year);
		Assertion.AssertEquals ("test#20", 6, time.Month);
		Assertion.AssertEquals ("test#21", 4, time.Day);
		Assertion.AssertEquals ("test#22", 6, time.Hour);
		Assertion.AssertEquals ("test#23", 4, time.Minute);
		Assertion.AssertEquals ("test#24", 0, time.Second);

		DeleteDirectory (path);	
	}

	[Test]
	public void LastAccessTime ()
	{
		string path = TempFolder + "/DirectoryTest.AccessTime.1";
		DeleteDirectory (path);
		
		Directory.CreateDirectory (path);
		Directory.SetLastAccessTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

		DateTime time = Directory.GetLastAccessTime (path);		
		Assertion.AssertEquals ("test#01", 2003, time.Year);
		Assertion.AssertEquals ("test#02", 6, time.Month);
		Assertion.AssertEquals ("test#03", 4, time.Day);
		Assertion.AssertEquals ("test#04", 6, time.Hour);
		Assertion.AssertEquals ("test#05", 4, time.Minute);
		Assertion.AssertEquals ("test#06", 0, time.Second);
		
		time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastAccessTimeUtc (path));
		Assertion.AssertEquals ("test#07", 2003, time.Year);
		Assertion.AssertEquals ("test#08", 6, time.Month);
		Assertion.AssertEquals ("test#09", 4, time.Day);
		Assertion.AssertEquals ("test#10", 6, time.Hour);
		Assertion.AssertEquals ("test#11", 4, time.Minute);
		Assertion.AssertEquals ("test#12", 0, time.Second);

		Directory.SetLastAccessTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
		time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastAccessTime (path));
		Assertion.AssertEquals ("test#13", 2003, time.Year);
		Assertion.AssertEquals ("test#14", 6, time.Month);
		Assertion.AssertEquals ("test#15", 4, time.Day);
		Assertion.AssertEquals ("test#16", 6, time.Hour);
		Assertion.AssertEquals ("test#17", 4, time.Minute);
		Assertion.AssertEquals ("test#18", 0, time.Second);

		time = Directory.GetLastAccessTimeUtc (path);		
		Assertion.AssertEquals ("test#19", 2003, time.Year);
		Assertion.AssertEquals ("test#20", 6, time.Month);
		Assertion.AssertEquals ("test#21", 4, time.Day);
		Assertion.AssertEquals ("test#22", 6, time.Hour);
		Assertion.AssertEquals ("test#23", 4, time.Minute);
		Assertion.AssertEquals ("test#24", 0, time.Second);

		DeleteDirectory (path);
	}

	[Test]
	public void LastWriteTime ()
	{
		string path = TempFolder + "/DirectoryTest.WriteTime.1";
		DeleteDirectory (path);		
		
		Directory.CreateDirectory (path);
		Directory.SetLastWriteTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

		DateTime time = Directory.GetLastWriteTime (path);		
		Assertion.AssertEquals ("test#01", 2003, time.Year);
		Assertion.AssertEquals ("test#02", 6, time.Month);
		Assertion.AssertEquals ("test#03", 4, time.Day);
		Assertion.AssertEquals ("test#04", 6, time.Hour);
		Assertion.AssertEquals ("test#05", 4, time.Minute);
		Assertion.AssertEquals ("test#06", 0, time.Second);
		
		time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastWriteTimeUtc (path));
		Assertion.AssertEquals ("test#07", 2003, time.Year);
		Assertion.AssertEquals ("test#08", 6, time.Month);
		Assertion.AssertEquals ("test#09", 4, time.Day);
		Assertion.AssertEquals ("test#10", 6, time.Hour);
		Assertion.AssertEquals ("test#11", 4, time.Minute);
		Assertion.AssertEquals ("test#12", 0, time.Second);

		Directory.SetLastWriteTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
		time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastWriteTime (path));
		Assertion.AssertEquals ("test#13", 2003, time.Year);
		Assertion.AssertEquals ("test#14", 6, time.Month);
		Assertion.AssertEquals ("test#15", 4, time.Day);
		Assertion.AssertEquals ("test#16", 6, time.Hour);
		Assertion.AssertEquals ("test#17", 4, time.Minute);
		Assertion.AssertEquals ("test#18", 0, time.Second);

		time = Directory.GetLastWriteTimeUtc (path);		
		Assertion.AssertEquals ("test#19", 2003, time.Year);
		Assertion.AssertEquals ("test#20", 6, time.Month);
		Assertion.AssertEquals ("test#21", 4, time.Day);
		Assertion.AssertEquals ("test#22", 6, time.Hour);
		Assertion.AssertEquals ("test#23", 4, time.Minute);
		Assertion.AssertEquals ("test#24", 0, time.Second);

		DeleteDirectory (path);		
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
		string path = TempFolder + "/DirectoryTest.SetLastWriteTime.2";
		DeleteDirectory (path);
		
		Directory.SetLastWriteTime (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetLastWriteTimeException6 ()
	{
		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetLastWriteTime.1";

		if (!Directory.Exists (path))			
			Directory.CreateDirectory (path);
		
		Directory.SetLastWriteTime (path, time);

	}

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
		string path = TempFolder + "/DirectoryTest.SetLastWriteTimeUtc.2";
		DeleteDirectory (path);
		Directory.SetLastWriteTimeUtc (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetLastWriteTimeUtcException6 ()
	{
		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetLastWriteTimeUtc.1";

		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		
		Directory.SetLastWriteTimeUtc (path, time);
	}

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
		Directory.SetLastAccessTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastAccessTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + "/DirectoryTest.SetLastAccessTime.2";
		DeleteDirectory (path);
		
		Directory.SetLastAccessTime (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetLastAccessTimeException6 ()
	{
		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetLastAccessTime.1";

		if (!Directory.Exists (path))			
			Directory.CreateDirectory (path);
		
		Directory.SetLastAccessTime (path, time);

	}

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
		Directory.SetLastAccessTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastAccessTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + "/DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		Directory.SetLastAccessTimeUtc (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetLastAccessTimeUtcException6 ()
	{
		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetLastAccessTimeUtc.1";

		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		
		Directory.SetLastAccessTimeUtc (path, time);
	}

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
		Directory.SetCreationTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetCreationTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + "/DirectoryTest.SetCreationTime.2";
		DeleteDirectory (path);
		
		Directory.SetCreationTime (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetCreationTimeException6 ()
	{
		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetCreationTime.1";

		if (!Directory.Exists (path))			
			Directory.CreateDirectory (path);
		
		Directory.SetCreationTime (path, time);

	}

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
		Directory.SetCreationTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetCreationTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + "/DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		Directory.SetCreationTimeUtc (path, time);
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

	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
	public void SetCreationTimeUtcException6 ()
	{
		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
		string path = TempFolder + "/DirectoryTest.SetLastAccessTimeUtc.1";

		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		
		Directory.SetCreationTimeUtc (path, time);
	}

	private void DeleteDirectory (string path)
	{
		if (Directory.Exists (path))
			Directory.Delete (path, true);		
	}
}
}
