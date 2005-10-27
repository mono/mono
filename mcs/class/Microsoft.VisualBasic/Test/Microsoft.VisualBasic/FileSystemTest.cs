// FileSystemTest.cs - 	NUnit Test Cases for vb module FileSystem 
//						(class Microsoft.VisualBasic.FileSystem)
//
// Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) Rafael Teixeira
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		public void TestCurDir()
		{
			string dir = FileSystem.CurDir ();
			AssertEquals ("CurDir#01", Environment.CurrentDirectory, dir);
		}
	}
}
