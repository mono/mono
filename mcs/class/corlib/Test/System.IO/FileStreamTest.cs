// TestConvert.cs - NUnit Test Cases for System.Convert class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) Ville Palo
// 


using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System.IO
{
        public class FileStreamTest : TestCase
        {
                public FileStreamTest() {}
                
                protected override void SetUp() {
                }
                protected override void TearDown() {}
                
                public void TestCtr ()
                {
                	FileStream stream = new FileStream ("testfilestream.tmp", FileMode.Create);
                	stream.Close ();
                	File.Delete ("testfilestream.tmp");
                	
                }
                
                public void TestCtorExceptions () 
                {
                	FileStream stream;
                	
                	try {
                		stream = new FileStream ("", FileMode.Create);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#01", typeof (ArgumentException), e.GetType ());                		
                	}

                	try {
                		stream = new FileStream (null, FileMode.Create);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#02", typeof (ArgumentNullException), e.GetType ());                		
                	}
                	
                	try {
                		if (File.Exists ("thisfileshouldnotexists.test"))
                			File.Delete ("thisfileshouldnotexists.test");
                		
                		stream = new FileStream ("thisfileshouldnotexists.test", FileMode.Open);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#03", typeof (FileNotFoundException), e.GetType ());
                	}

                	try {
                		if (File.Exists ("thisfileshouldNOTexists.test"))
                			File.Delete ("thisfileshouldNOTexists.test");
                		stream = new FileStream ("thisfileshouldNOTexists.test", FileMode.Truncate);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#04", typeof (FileNotFoundException), e.GetType ());
                	}

                	try {
                		stream = new FileStream ("thisfileshouldexists.test", FileMode.CreateNew);
                		stream.Close ();
                		stream = null;
                		stream = new FileStream ("thisfileshouldexists.test", FileMode.CreateNew);
                		Fail ();
                	} catch (Exception e) {
                		
                		if (File.Exists ("thisfileshouldexists.test")) // remove file
                			File.Delete ("thisfileshouldexists.test");
                		AssertEquals ("test#04", typeof (IOException), e.GetType ());
                	} 
                	
                	try {
                		if (Directory.Exists ("thisDicrectoryShouldNotExists"))
                			Directory.Delete ("thisDicrectoryShouldNotExists");                		
                		stream = new FileStream ("thisDicrectoryShouldNotExists/eitherthisfile.test", FileMode.CreateNew);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#05", typeof (DirectoryNotFoundException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream ("test.test.test", FileMode.Append | FileMode.CreateNew);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#08", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream ("test.test.test", FileMode.Append | FileMode.Open);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#09", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}                	
                }
                
                public void TestCtorExceptions2 ()
                {
                	FileStream stream;
                	
                	try {
                		stream = new FileStream (".test.test.test", FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Inheritable);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#01", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}

                	try {
                		stream = new FileStream (".test.test.test", FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Write);
                		Fail ();
                	} catch (Exception e) {                		
                		// FileMode.CreateNew && FileAccess.Read
                		AssertEquals ("test#02", typeof (ArgumentException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream (".test.test.test", FileMode.CreateNew, FileAccess.Read, FileShare.Inheritable | FileShare.ReadWrite);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#03", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream (".test.test.test", FileMode.Truncate, FileAccess.Read);
                		Fail ();
                	} catch (Exception e) {
                		// FileMode.Truncate && FileAccess.Read
                		AssertEquals ("test#04", typeof (ArgumentException), e.GetType ());
                	}                	
                	
                	try {
                		stream = new FileStream (new IntPtr (12), FileAccess.Read);
                		Fail ();
                	} catch (Exception e) {
                		// Invalid handle
                		AssertEquals ("test#05", typeof (IOException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream (".test.test.test", FileMode.Truncate, FileAccess.Read, FileShare.ReadWrite, -1);
                		Fail ();
                	} catch (Exception e) {
                		// FileMode.Truncate && FileAccess.Read
                		AssertEquals ("test#06", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}                	
                	
                }
                
                public void TestDefaultProperties ()
                {
                	FileStream stream = new FileStream ("testfilestream.tmp", FileMode.Create);
                	
                	AssertEquals ("test#01", true, stream.CanRead);
                	AssertEquals ("test#02", true, stream.CanSeek);
                	AssertEquals ("test#03", true, stream.CanWrite);
                	AssertEquals ("test#04", false, stream.IsAsync);
                	AssertEquals ("test#05", true, stream.Name.EndsWith ("testfilestream.tmp"));
                	AssertEquals ("test#06", 0, stream.Position);
                	AssertEquals ("test#07", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	File.Delete ("testfilestream.tmp");                	
                }
                
                public void TestLock()
                {
                	if (File.Exists (".testFileStream.Test"))
                		File.Delete (".testFileStream.Test");
                	
                	FileStream stream = new FileStream (".testFileStream.Test", FileMode.CreateNew, FileAccess.ReadWrite);
                	                	
	               	stream.Write (new Byte [] {0,1,2,3,4,5,6,7,8,9,10}, 0, 10);                              	
                	stream.Close ();

                	stream = new FileStream (".testFileStream.Test", FileMode.Open, FileAccess.ReadWrite);
                	
                	stream.Lock (0, 5);
                	
                	FileStream stream2 = new FileStream (".testFileStream.Test", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                	
                	byte [] bytes = new byte [5];
                	try {                		
                		stream2.Read (bytes, 0, 5);
                		Fail ();
                	} catch (Exception e) {
                		
                		// locked
                		AssertEquals ("test#01", typeof (IOException), e.GetType ());
                	}
               		                
                	stream2.Seek (5, SeekOrigin.Begin);               		
               		stream2.Read (bytes, 0, 5);                		
                	
                	AssertEquals ("test#02", 5, bytes [0]);
                	AssertEquals ("test#03", 6, bytes [1]);                	
                	AssertEquals ("test#04", 7, bytes [2]); 
                	AssertEquals ("test#05", 8, bytes [3]);
                	AssertEquals ("test#06", 9, bytes [4]);
                	
                	stream.Unlock (0,5);
                	stream2.Seek (0, SeekOrigin.Begin);	
               		stream2.Read (bytes, 0, 5);
                	
                	AssertEquals ("test#02", 0, bytes [0]);
                	AssertEquals ("test#03", 1, bytes [1]);                	
                	AssertEquals ("test#04", 2, bytes [2]); 
                	AssertEquals ("test#05", 3, bytes [3]);
                	AssertEquals ("test#06", 4, bytes [4]);
                	                	
                	stream.Close ();
                	stream2.Close ();
                	
                	if (File.Exists (".testFileStream.Test"))
                		File.Delete (".testFileStream.Test");
                	                		
                }

        }
}

