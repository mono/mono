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

namespace MonoTests.System.IO {

[TestFixture]
public class DirectoryTest {
	
	
	public DirectoryTest () 
	{
		;
	}
	
	~DirectoryTest ()
	{
		string dir = ".DirectoryTest.Test.1";		
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.2";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);

		dir = ".DirectoryTest.Test.Delete.1";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.3";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.4";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.5";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.6";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.7";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.8";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.9";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.10";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.11";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.12";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.13";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.14";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.15";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.16";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.17";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		dir = ".DirectoryTest.Test.18";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		

	}
	
	[TearDown]
	public void TearDown ()
	{
		/*string dir;
		dir = ".DirectoryTest.Test.1";
		
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.2";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);

		dir = ".DirectoryTest.Test.Delete.1";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.4";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.5";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.6";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.7";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.8";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.9";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.10";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.11";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		
		dir = ".DirectoryTest.Test.12";
		if (Directory.Exists (dir))
			Directory.Delete (dir, true);
		*/
	}

	[Test]
	public void CreateDirectory ()
	{
		if (Directory.Exists (".DirectoryTest.Test.1"))
			Directory.Delete (".DirectoryTest.Test.1");
		
		DirectoryInfo info = Directory.CreateDirectory (".DirectoryTest.Test.1");
		Assertion.AssertEquals ("test#01", true, info.Exists);
		Assertion.AssertEquals ("test#02", ".1", info.Extension);
		Assertion.AssertEquals ("test#03", true, info.FullName.EndsWith (".DirectoryTest.Test.1"));
		Assertion.AssertEquals ("test#04", ".DirectoryTest.Test.1", info.Name);
		Assertion.AssertEquals ("test#05", true, Directory.GetCurrentDirectory ().EndsWith (info.Parent.Name));
		
		if (Directory.Exists (".DirectoryTest.Test.1"))
			Directory.Delete (".DirectoryTest.Test.1");
	}
	
	[Test]
	[ExpectedException(typeof(NotSupportedException))]	
	public void CreateDirectoryNotSupportedException ()
	{
		if (Directory.Exists (":"))
			Directory.Delete (":");
		
		DirectoryInfo info = Directory.CreateDirectory (":");

		if (Directory.Exists (":"))
			Directory.Delete (":");		
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
		if (Directory.Exists (".DirectoryTest.Test.2"))
			Directory.Delete (".DirectoryTest.Test.2");

		string path = ".DirectoryTest.Test.";
		path += Path.InvalidPathChars [0];
		path += ".2";
		DirectoryInfo info = Directory.CreateDirectory (path);		
		
		if (Directory.Exists (path))
			Directory.Delete (path);
		
	}

	[Test]
	public void Delete ()
	{
		string path = ".DirectoryTest.Test.Delete.1";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
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
		string path = ".DirectoryTest.Test.4";
		if (Directory.Exists (path))
			Directory.Delete (path);
		
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
		string path = ".DirectoryTest.Test.5";
		if (Directory.Exists (path))
			Directory.Delete (path);

		Directory.Delete (path);		
	}

	[Test]	
	[ExpectedException(typeof(IOException))]
	public void DeleteArgumentException4 ()
	{
		string path = ".DirectoryTest.Test.6";
		if (Directory.Exists (path))
			Directory.Delete (path, true);

		Directory.CreateDirectory (path);
		File.Create (path + "/.DirectoryTest.Test.6");
	
		Directory.Delete (path);
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
		string path = ".DirectoryTest.Test.7";
		if (Directory.Exists (path))
			Directory.Delete (path, true);

		Directory.GetCreationTime (path);
	}

	[Test]
	public void Move ()
	{
		string path = ".DirectoryTest.Test.9";
		string path2 = ".DirectoryTest.Test.10";
		
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		if (Directory.Exists (path2))
			Directory.Delete (path2, true);
		
		Directory.CreateDirectory (path);
		Directory.CreateDirectory (path + "/" + path);
		Assertion.AssertEquals ("test#01", true, Directory.Exists (path + "/" + path));
		
		Directory.Move (path, path2);
		Assertion.AssertEquals ("test#02", false, Directory.Exists (path + "/" + path));		
		Assertion.AssertEquals ("test#03", true, Directory.Exists (path2 + "/" + path));
		
		if (Directory.Exists (path2 + "/" + path))
			Directory.Delete (path2 + "/" + path, true);
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException1 ()
	{
		string path = ".DirectoryTest.Test.8";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
		Directory.Move (path, path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException2 ()
	{
		string path = ".DirectoryTest.Test.11";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
		Directory.Move ("", path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException3 ()
	{
		string path = ".DirectoryTest.Test.12";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
		Directory.Move ("             ", path);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException4 ()
	{
		string path = ".DirectoryTest.Test.13";
		path += Path.InvalidPathChars [0];
		string path2 = ".DirectoryTest.Test.13";		
		
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		if (Directory.Exists (path2))
			Directory.Delete (path2, true);
		
		Directory.CreateDirectory (path2);		
		Directory.Move (path2, path);
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void MoveException5 ()
	{
		string path = ".DirectoryTest.Test.14";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
		Directory.Move (path, path + "Test.Test");		
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException6 ()
	{
		string path = ".DirectoryTest.Test.15";
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		
		Directory.CreateDirectory (path);
		
		Directory.Move (path, path + "/" + path);
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException7 ()
	{
		string path = ".DirectoryTest.Test.16";
		string path2 = ".DirectoryTest.Test.17";
		
		if (Directory.Exists (path))
			Directory.Delete (path, true);
		if (Directory.Exists (path2))
			Directory.Delete (path2, true);
		
		Directory.CreateDirectory (path);
		Directory.CreateDirectory (path2);
		Directory.Move (path, path2);

		if (Directory.Exists (path))
			Directory.Delete (path, true);
		if (Directory.Exists (path2))
			Directory.Delete (path2, true);
		
	}

}
}
