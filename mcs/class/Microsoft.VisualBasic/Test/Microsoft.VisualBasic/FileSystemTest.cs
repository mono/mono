// FileSystemTest.cs - 	NUnit Test Cases for vb module FileSystem 
//						(class Microsoft.VisualBasic.FileSystem)
//
// Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) Rafael Teixeira
// 

using NUnit.Framework;
using System;
using System.IO;
using Microsoft.VisualBasic;

namespace MonoTests.Microsoft.VisualBasic
{

	[TestFixture]
	public class FileSystemTest : Assertion {
		string oldDir = "";
		
		[SetUp]
		public void GetReady() 
		{
			oldDir = Environment.CurrentDirectory;
		}

		[TearDown]
		public void Clean() 
		{
			// reset dir
			Environment.CurrentDirectory = oldDir;
		}	
		
		// ChDir
		
		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void ChDirArg1()
		{
			FileSystem.ChDir ("");
		}
		
		[Test]
		[ExpectedException (typeof(FileNotFoundException))]
		public void ChDirArg2()
		{
			FileSystem.ChDir ("z:\\home\rob");
		}
		
		[Test]
		public void TestChDir()
		{
			// change the current dir to parent dir
			DirectoryInfo parent = Directory.GetParent (Environment.CurrentDirectory);
			FileSystem.ChDir (parent.FullName);
			AssertEquals ("CHDIR#01", parent.FullName, Environment.CurrentDirectory);			
		}
		
		[Test]
		[ExpectedException (typeof(FileNotFoundException))]
		public void ChDriveArgs1()
		{
			FileSystem.ChDir ("z:\\home\rob");
		}
		
		[Test]
		public void TestChDrive()
		{
			string[] drives = Directory.GetLogicalDrives ();
			
			foreach (string drive in drives) {
				// skip diskdrive, if no disk is present it fails.
				if (drive.ToLower()[0] == 'a')
					continue;
				FileSystem.ChDrive (drive);
				Assert ("ChDrive#01", Environment.CurrentDirectory.StartsWith (drive));
			}
		}
		
		[Test]
		public void TestCurDir()
		{
			string dir = FileSystem.CurDir ();
			AssertEquals ("CurDir#01", Environment.CurrentDirectory, dir);
		}
/*
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDir() {
			FileSystem.Dir();
			Fail ("Calling as the first thing the parameterless overload of Dir didn't throw an exception");
		}
	
		[Test]
		public void TestDirWithSourceFile() {
			AssertEquals("Didn't found the source file with pattern './Microsoft.VisualBasic/FileSystem.cs'",
				"FileSystem.cs",
				FileSystem.Dir("./Microsoft.VisualBasic/FileSystem.cs", FileAttribute.Normal)) ;
		}
*/
	}
}
