// FileStreamTests.cs - NUnit2 Test Cases for System.IO.FileStream class
//
// Authors:
// 	Ville Palo (vi64pa@koti.soon.fi)
// 	Gert Driesen (gert.driesen@ardatis.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
// 
// (C) Ville Palo
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// 


using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{
        public class FileStreamTest : TestCase
        {
		string bazFileName = Path.Combine ("resources", "baz");
                public FileStreamTest() {}
                
		[SetUp]
                protected override void SetUp ()
		{
			File.Delete (bazFileName);
                }

		[TearDown]
		protected override void TearDown ()
		{
			File.Delete (bazFileName);
		}
                
                public void TestCtr ()
                {
                	FileStream stream = new FileStream ("testfilestream.tmp.1", FileMode.Create);
                	stream.Close ();
                	File.Delete ("testfilestream.tmp.1");
                	
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
                		stream = new FileStream (".test.test.test.2", FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Inheritable);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#01", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}

                	try {
                		stream = new FileStream (".test.test.test.2", FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Write);
                		Fail ();
                	} catch (Exception e) {                		
                		// FileMode.CreateNew && FileAccess.Read
                		AssertEquals ("test#02", typeof (ArgumentException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream (".test.test.test.2", FileMode.CreateNew, FileAccess.Read, FileShare.Inheritable | FileShare.ReadWrite);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#03", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}
                	
                	try {
                		stream = new FileStream (".test.test.test.2", FileMode.Truncate, FileAccess.Read);
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
                		stream = new FileStream (".test.test.test.2", FileMode.Truncate, FileAccess.Read, FileShare.ReadWrite, -1);
                		Fail ();
                	} catch (Exception e) {
                		// FileMode.Truncate && FileAccess.Read
                		AssertEquals ("test#06", typeof (ArgumentOutOfRangeException), e.GetType ());
                	}                	
                	
                }

		[Test]
		[ExpectedException(typeof(IOException))]
		public void CtorIOException ()
		{			
			string path = "resources/CTorIOException.Test";
			if (File.Exists (path)) 
				File.Delete (path);
			
			FileStream stream = new FileStream (path, FileMode.CreateNew);
			
			// used by an another process
			FileStream stream2 = new FileStream (path, FileMode.OpenOrCreate);
		}
		
		[Test]
		public void Flush ()
		{
			string path = "resources/FileStreamTest.Flush";
			if (File.Exists (path))
				File.Delete (path);
			
			FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
			FileStream stream2 = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

			stream.Write (new byte [] {1, 2, 3, 4, 5}, 0, 5);
						
			byte [] bytes = new byte [5];
			stream2.Read (bytes, 0, 5);
			
			Assertion.AssertEquals ("test#01", 0, bytes [0]);
			Assertion.AssertEquals ("test#02", 0, bytes [1]);
			Assertion.AssertEquals ("test#03", 0, bytes [2]);
			Assertion.AssertEquals ("test#04", 0, bytes [3]);
			
			stream.Flush ();
			stream2.Read (bytes, 0, 5);			
			Assertion.AssertEquals ("test#05", 1, bytes [0]);
			Assertion.AssertEquals ("test#06", 2, bytes [1]);
			Assertion.AssertEquals ("test#07", 3, bytes [2]);
			Assertion.AssertEquals ("test#08", 4, bytes [3]);
			stream.Close ();
			stream2.Close ();
			
			if (File.Exists (path))
				File.Delete (path);			
		}
                
                public void TestDefaultProperties ()
                {
                	FileStream stream = new FileStream ("testfilestream.tmp.2", FileMode.Create);
                	
                	AssertEquals ("test#01", true, stream.CanRead);
                	AssertEquals ("test#02", true, stream.CanSeek);
                	AssertEquals ("test#03", true, stream.CanWrite);
                	AssertEquals ("test#04", false, stream.IsAsync);
                	AssertEquals ("test#05", true, stream.Name.EndsWith ("testfilestream.tmp.2"));
                	AssertEquals ("test#06", 0, stream.Position);
                	AssertEquals ("test#07", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	File.Delete ("testfilestream.tmp.2");

                	stream = new FileStream ("testfilestream.tmp.2", FileMode.OpenOrCreate, FileAccess.Read);
                	AssertEquals ("test#08", true, stream.CanRead);
                	AssertEquals ("test#09", true, stream.CanSeek);
                	AssertEquals ("test#10", false, stream.CanWrite);
                	AssertEquals ("test#11", false, stream.IsAsync);
                	AssertEquals ("test#12", true, stream.Name.EndsWith ("testfilestream.tmp.2"));
                	AssertEquals ("test#13", 0, stream.Position);
                	AssertEquals ("test#14", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	
               		stream = new FileStream ("testfilestream.tmp.2", FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                	AssertEquals ("test#15", false, stream.CanRead);
                	AssertEquals ("test#16", true, stream.CanSeek);
                	AssertEquals ("test#17", true, stream.CanWrite);
                	AssertEquals ("test#18", false, stream.IsAsync);
                	AssertEquals ("test#19", true, stream.Name.EndsWith ("testfilestream.tmp.2"));
                	AssertEquals ("test#20", 0, stream.Position);
                	AssertEquals ("test#21", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	File.Delete ("testfilestream.tmp.2");                	

                }
                
                public void TestLock()
                {
                	if (File.Exists (".testFileStream.Test.1"))
                		File.Delete (".testFileStream.Test.1");
                	
                	FileStream stream = new FileStream (".testFileStream.Test.1", FileMode.CreateNew, FileAccess.ReadWrite);
                	                	
	               	stream.Write (new Byte [] {0,1,2,3,4,5,6,7,8,9,10}, 0, 10);                              	
                	stream.Close ();

                	stream = new FileStream (".testFileStream.Test.1", FileMode.Open, FileAccess.ReadWrite);
                	
                	stream.Lock (0, 5);
                	
                	FileStream stream2 = new FileStream (".testFileStream.Test.1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                	
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
                	
                	if (File.Exists (".testFileStream.Test.1"))
                		File.Delete (".testFileStream.Test.1");
                	                		
                }

                [Test]
                public void Seek ()
                {
                	string path = "resources/FST.Seek.Test";
                	if (File.Exists (path))
                		File.Delete (path);
                	
                	FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                	FileStream stream2 = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                	
                	stream.Write (new byte [] {1, 2, 3, 4, 5, 6, 7, 8, 10}, 0, 9);
                	Assertion.AssertEquals ("test#01", 5, stream2.Seek (5, SeekOrigin.Begin));
                	Assertion.AssertEquals ("test#02", -1, stream2.ReadByte ());
                	
                	Assertion.AssertEquals ("test#03", 2, stream2.Seek (-3, SeekOrigin.Current));
                	Assertion.AssertEquals ("test#04", -1, stream2.ReadByte ());
                	
                	Assertion.AssertEquals ("test#05", 12, stream.Seek (3, SeekOrigin.Current));
                	Assertion.AssertEquals ("test#06", -1, stream.ReadByte ());

                	Assertion.AssertEquals ("test#07", 5, stream.Seek (-7, SeekOrigin.Current));
                	Assertion.AssertEquals ("test#08", 6, stream.ReadByte ());

                	Assertion.AssertEquals ("test#09", 5, stream2.Seek (5, SeekOrigin.Begin));
                	Assertion.AssertEquals ("test#10", 6, stream2.ReadByte ());
                	                	
                	stream.Close ();
                	stream2.Close ();

                	if (File.Exists (path))
                		File.Delete (path);                	
                }

                public void TestSeek ()
                {
                	if (File.Exists (".testFileStream.Test.2"))
                		File.Delete (".testFileStream.Test.2");
			
			FileStream stream = new FileStream (".testFileStream.Test.2", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                	stream.Write (new byte[] {1, 2, 3, 4, 5, 6, 7, 8 , 9, 10}, 0, 10);
                	
                	stream.Seek (5, SeekOrigin.End);
                	AssertEquals ("test#01", -1, stream.ReadByte ());

                	stream.Seek (-5, SeekOrigin.End);
                	AssertEquals ("test#02", 6, stream.ReadByte ());
                	
                	try {
                		stream.Seek (-11, SeekOrigin.End);
                		Fail ();
                	} catch (Exception e) {
                		AssertEquals ("test#03", typeof (IOException), e.GetType ());
                	}
                	
                	stream.Seek (19, SeekOrigin.Begin);
			AssertEquals ("test#04", -1, stream.ReadByte ());

			stream.Seek (1, SeekOrigin.Begin);
                	AssertEquals ("test#05", 2, stream.ReadByte ());
                	
			stream.Seek (3, SeekOrigin.Current);
                	AssertEquals ("test#06", 6, stream.ReadByte ());

			stream.Seek (-2, SeekOrigin.Current);
                	AssertEquals ("test#07", 5, stream.ReadByte ());

			stream.Flush ();
                	stream.Close ();
                	if (File.Exists (".testFileStream.Test.2"))
                		File.Delete (".testFileStream.Test.2");
                	
                }
                
                public void TestClose ()
                {
                	if (File.Exists (".testFileStream.Test.3"))
                		File.Delete (".testFileStream.Test.3");
                	
                	FileStream stream = new FileStream (".testFileStream.Test.3", FileMode.CreateNew, FileAccess.ReadWrite);

                	stream.Write (new byte [] {1, 2, 3, 4}, 0, 4);
                	stream.ReadByte ();                	
                	stream.Close ();
			
			try {                	
                		stream.ReadByte ();
				Fail ();
			} catch (Exception e) {
				AssertEquals ("test#01", typeof (ObjectDisposedException), e.GetType ());
			}
			
			try {                	
                		stream.WriteByte (64);
				Fail ();
			} catch (Exception e) {
				AssertEquals ("test#02", typeof (ObjectDisposedException), e.GetType ());
			}
			
			try {                	
                		stream.Flush ();
				Fail ();
			} catch (Exception e) {
				AssertEquals ("test#03", typeof (ObjectDisposedException), e.GetType ());
			}
			
			try { 
				long l = stream.Length;
				Fail ();
			} catch (Exception e) {
				AssertEquals ("test#04", typeof (ObjectDisposedException), e.GetType ());
			}
			
			try { 
				long l = stream.Position;
				Fail ();
			} catch (Exception e) {
				AssertEquals ("test#05", typeof (ObjectDisposedException), e.GetType ());
			}

			AssertEquals ("test#06", false, stream.CanRead);
                	AssertEquals ("test#07", false, stream.CanSeek);
                	AssertEquals ("test#08", false, stream.CanWrite);                	
                	AssertEquals ("test#09", true, stream.Name.EndsWith (".testFileStream.Test.3"));
                	
                	if (File.Exists (".testFileStream.Test.3"))
                		File.Delete (".testFileStream.Test.3");
                }


		/// <summary>
		/// Checks whether the <see cref="FileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Read" /> and the
		/// <see cref="FileStream.Write(byte[], int, int)" /> method is called.
		/// </summary>
		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void TestWriteVerifyAccessMode ()
		{
			FileStream bazFileStream = null;
			byte[] buffer;

			try {
				buffer = Encoding.ASCII.GetBytes ("test");
				bazFileStream = new FileStream (bazFileName, FileMode.OpenOrCreate, FileAccess.Read);
				bazFileStream.Write (buffer, 0, buffer.Length);
			} finally {
				if (bazFileStream != null)
					bazFileStream.Close();
			}
		}

		/// <summary>
		/// Checks whether the <see cref="FileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Read" /> and the
		/// <see cref="FileStream.WriteByte(byte)" /> method is called.
		/// </summary>
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestWriteByteVerifyAccessMode ()
		{
			FileStream bazFileStream = null;

			try {
				bazFileStream = new FileStream (bazFileName, FileMode.OpenOrCreate, FileAccess.Read);
				bazFileStream.WriteByte (Byte.MinValue);
			} finally {
				if (bazFileStream != null)
					bazFileStream.Close ();
			}
		}

		/// <summary>
		/// Checks whether the <see cref="FileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Write" /> and the
		/// <see cref="FileStream.Read(byte[], int, int)" /> method is called.
		/// </summary>
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestReadVerifyAccessMode ()
		{
			FileStream bazFileStream = null;
			byte[] buffer = new byte [100];

			try {
				bazFileStream = new FileStream (bazFileName, FileMode.OpenOrCreate, FileAccess.Write);
				bazFileStream.Read (buffer, 0, buffer.Length);
			} finally {
				if (bazFileStream != null)
					bazFileStream.Close ();
			}
		}

		/// <summary>
		/// Checks whether the <see cref="FileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Write" /> and the
		/// <see cref="FileStream.ReadByte()" /> method is called.
		/// </summary>
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestReadByteVerifyAccessMode ()
		{
			FileStream bazFileStream = null;

			try {
				bazFileStream = new FileStream (bazFileName, FileMode.OpenOrCreate, FileAccess.Write);
				int readByte = bazFileStream.ReadByte ();
			} finally {
				if (bazFileStream != null)
					bazFileStream.Close();
			}
		}
        }
}

