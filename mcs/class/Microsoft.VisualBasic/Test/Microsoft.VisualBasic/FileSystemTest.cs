// FileSystemTest.cs - 	NUnit Test Cases for vb module FileSystem 
//						(class Microsoft.VisualBasic.FileSystem)
//
// Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) Rafael Teixeira
// 

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;

namespace MonoTests.Microsoft.VisualBasic
{

	[TestFixture]
	public class FileSystemTest : Assertion {
	
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

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

	}
}
