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
        public class FileStreamTest : Assertion
        {
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
		}

		[SetUp]
                public void SetUp ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);

			Directory.CreateDirectory (TempFolder);
                }

                public void TestCtr ()
                {
			string path = TempFolder + "/testfilestream.tmp.1";
			DeleteFile (path);
			FileStream stream = null;
			try {
				stream = new FileStream (path, FileMode.Create);
			} finally {

				if (stream != null)
					stream.Close ();
				DeleteFile (path);                	
			}
                }

		[Test]
		[ExpectedException (typeof (ArgumentException))]
                public void CtorArgumentException1 ()
		{
			FileStream stream;
                	stream = new FileStream ("", FileMode.Create);
			stream.Close ();
		}			

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorArgumentNullException ()
		{
			FileStream stream = new FileStream (null, FileMode.Create);
			stream.Close ();
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void CtorFileNotFoundException1 ()
		{
			string path = TempFolder + "/thisfileshouldnotexists.test";
			DeleteFile (path);
			FileStream stream = null;
                	try {                		
                		stream = new FileStream (TempFolder + "/thisfileshouldnotexists.test", FileMode.Open);
                	} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	}
		}			

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void CtorFileNotFoundException2 ()
		{
			string path = TempFolder + "/thisfileshouldNOTexists.test";
			DeleteFile (path);
			FileStream stream = null;

                	try {
                		stream = new FileStream (TempFolder + "/thisfileshouldNOTexists.test", FileMode.Truncate);
                	} finally {
				if (stream != null)
					stream.Close ();

				DeleteFile (path);
                	}
		} 

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CtorIOException1 ()
		{
			string path = TempFolder + "/thisfileshouldexists.test";
			FileStream stream = null;
			DeleteFile (path);
                	try {
                		stream = new FileStream (path, FileMode.CreateNew);
                		stream.Close ();
                		stream = null;
                		stream = new FileStream (path, FileMode.CreateNew);
                	} finally {
                		
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	} 

		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorArgumentOutOfRangeException1 ()
		{
			FileStream stream = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);
                	try {
                		stream = new FileStream (path, FileMode.Append | FileMode.CreateNew);
                	} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	}                	
		}			

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorArgumentOutOfRangeException2 ()
		{
			FileStream stream = null;
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);
                	try {
                		stream = new FileStream ("test.test.test", FileMode.Append | FileMode.Open);
                	} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	}                	
		}

                [Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void CtorDirectoryNotFoundException ()
		{
			string path = TempFolder + "/thisDicrectoryShouldNotExists";
			if (Directory.Exists (path))
				Directory.Delete (path, true);

			FileStream stream = null;				
                	try {
                		stream = new FileStream (path + "/eitherthisfile.test", FileMode.CreateNew);
                	} finally {

				if (stream != null)
					stream.Close ();

				if (Directory.Exists (path))
					Directory.Delete (path, true);
                	}                		
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorArgumentOutOfRangeException3 ()
		{
			string path = TempFolder + "/CtorArgumentOutOfRangeException1";
			DeleteFile (path);
			
			FileStream stream = null;
			try {
				stream = new FileStream (path, FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Inheritable);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorArgumentOutOfRangeException4 ()
		{
			string path = TempFolder + "/CtorArgumentOutOfRangeException2";
			DeleteFile (path);

			FileStream stream = null;
			try {
				stream = new FileStream (path, FileMode.Truncate, FileAccess.Read, FileShare.ReadWrite, -1);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}			
				
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorArgumentException2 ()
		{
               		// FileMode.CreateNew && FileAccess.Read

			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
                	FileStream stream = null;

			DeleteFile (path);

                	try {
                		stream = new FileStream (".test.test.test.2", FileMode.CreateNew, FileAccess.Read, FileShare.None | FileShare.Write);
                	} finally {                		

				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	}
		}


		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorArgumentOutOfRangeException5 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);

			FileStream stream = null;
                	try {
                		stream = new FileStream (path, FileMode.CreateNew, FileAccess.Read, FileShare.Inheritable | FileShare.ReadWrite);
                	} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
                	}
		}		

		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CtorArgumentException3 ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
                	FileStream stream = null;

			DeleteFile (path);
                	
                	try {
                		stream = new FileStream (".test.test.test.2", FileMode.Truncate, FileAccess.Read);
                	} finally {
				if (stream != null)
					stream.Close ();
				
				DeleteFile (path);
                	}                	
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CtorIOException2 ()
		{
			FileStream stream = null;
                	try {
                		stream = new FileStream (new IntPtr (12), FileAccess.Read);
                	} finally {
				if (stream != null)
					stream.Close ();
                	}
		}

		[Test]
		[ExpectedException(typeof(IOException))]
		public void CtorIOException ()
		{			
			string path = TempFolder + "/CTorIOException.Test";
			FileStream stream = null;
			FileStream stream2 = null;
			DeleteFile (path);

			try {
				stream = new FileStream (path, FileMode.CreateNew);
			
				// used by an another process
				stream2 = new FileStream (path, FileMode.OpenOrCreate);
			} finally {
				if (stream != null)
					stream.Close ();
				if (stream2 != null)
					stream2.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Flush ()
		{
			string path = TempFolder + "/FileStreamTest.Flush";
			FileStream stream = null;
			FileStream stream2 = null;

			DeleteFile (path);
			
			try {
				stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
				stream2 = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

				stream.Write (new byte [] {1, 2, 3, 4, 5}, 0, 5);
						
				byte [] bytes = new byte [5];
				stream2.Read (bytes, 0, 5);
				
				AssertEquals ("test#01", 0, bytes [0]);
				AssertEquals ("test#02", 0, bytes [1]);
				AssertEquals ("test#03", 0, bytes [2]);
				AssertEquals ("test#04", 0, bytes [3]);
				
				stream.Flush ();
				stream2.Read (bytes, 0, 5);			
				AssertEquals ("test#05", 1, bytes [0]);
				AssertEquals ("test#06", 2, bytes [1]);
				AssertEquals ("test#07", 3, bytes [2]);
				AssertEquals ("test#08", 4, bytes [3]);
			} finally {
				if (stream != null)
					stream.Close ();
				if (stream2 != null)
					stream2.Close ();
				
				DeleteFile (path);
			}
		}
                
                public void TestDefaultProperties ()
                {
			string path = TempFolder + Path.DirectorySeparatorChar + "testfilestream.tmp.2";
			DeleteFile (path);

                	FileStream stream = new FileStream (path, FileMode.Create);
                	
                	AssertEquals ("test#01", true, stream.CanRead);
                	AssertEquals ("test#02", true, stream.CanSeek);
                	AssertEquals ("test#03", true, stream.CanWrite);
                	AssertEquals ("test#04", false, stream.IsAsync);
                	AssertEquals ("test#05", true, stream.Name.EndsWith (path));
                	AssertEquals ("test#06", 0, stream.Position);
                	AssertEquals ("test#07", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	DeleteFile (path);

                	stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
                	AssertEquals ("test#08", true, stream.CanRead);
                	AssertEquals ("test#09", true, stream.CanSeek);
                	AssertEquals ("test#10", false, stream.CanWrite);
                	AssertEquals ("test#11", false, stream.IsAsync);
                	AssertEquals ("test#12", true, stream.Name.EndsWith (path));
                	AssertEquals ("test#13", 0, stream.Position);
                	AssertEquals ("test#14", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
                	
               		stream = new FileStream (path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
                	AssertEquals ("test#15", false, stream.CanRead);
                	AssertEquals ("test#16", true, stream.CanSeek);
                	AssertEquals ("test#17", true, stream.CanWrite);
                	AssertEquals ("test#18", false, stream.IsAsync);
                	AssertEquals ("test#19", true, stream.Name.EndsWith ("testfilestream.tmp.2"));
                	AssertEquals ("test#20", 0, stream.Position);
                	AssertEquals ("test#21", "System.IO.FileStream", stream.ToString());                	
                	stream.Close ();
			DeleteFile (path);
                }
                
                public void TestLock()
                {
			string path = TempFolder + Path.DirectorySeparatorChar + "TestLock";
                	DeleteFile (path);

                	FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);
                	                	
	               	stream.Write (new Byte [] {0,1,2,3,4,5,6,7,8,9,10}, 0, 10);                              	
                	stream.Close ();

                	stream = new FileStream (path, FileMode.Open, FileAccess.ReadWrite);
                	
                	stream.Lock (0, 5);
                	
                	FileStream stream2 = new FileStream (path , FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                	
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
                	
                	DeleteFile (path);                		
                }

                [Test]
                public void Seek ()
                {
                	string path = TempFolder + "/FST.Seek.Test";
                	DeleteFile (path);			

                	FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
                	FileStream stream2 = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                	
                	stream.Write (new byte [] {1, 2, 3, 4, 5, 6, 7, 8, 10}, 0, 9);
                	AssertEquals ("test#01", 5, stream2.Seek (5, SeekOrigin.Begin));
                	AssertEquals ("test#02", -1, stream2.ReadByte ());
                	
                	AssertEquals ("test#03", 2, stream2.Seek (-3, SeekOrigin.Current));
                	AssertEquals ("test#04", -1, stream2.ReadByte ());
                	
                	AssertEquals ("test#05", 12, stream.Seek (3, SeekOrigin.Current));
                	AssertEquals ("test#06", -1, stream.ReadByte ());

                	AssertEquals ("test#07", 5, stream.Seek (-7, SeekOrigin.Current));
                	AssertEquals ("test#08", 6, stream.ReadByte ());

                	AssertEquals ("test#09", 5, stream2.Seek (5, SeekOrigin.Begin));
                	AssertEquals ("test#10", 6, stream2.ReadByte ());
                	                	
                	stream.Close ();
                	stream2.Close ();
			
			DeleteFile (path);
                }

                public void TestSeek ()
                {
					string path = TempFolder + Path.DirectorySeparatorChar + "TestSeek";
					DeleteFile (path);

					FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
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

					// Test that seeks work correctly when seeking inside the buffer
					stream.Seek (0, SeekOrigin.Begin);
					stream.WriteByte (0);
					stream.WriteByte (1);
					stream.Seek (0, SeekOrigin.Begin);
					byte[] buf = new byte [1];
					buf [0] = 2;
					stream.Write (buf, 0, 1);
					stream.Write (buf, 0, 1);
					stream.Flush ();
					stream.Seek (0, SeekOrigin.Begin);
					AssertEquals ("test#08", 2, stream.ReadByte ());
					AssertEquals ("test#09", 2, stream.ReadByte ());

					stream.Close ();

					DeleteFile (path);
                }
                
                public void TestClose ()
                {
			string path = TempFolder + Path.DirectorySeparatorChar + "TestClose";
			DeleteFile (path);
                	
                	FileStream stream = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);

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
                	AssertEquals ("test#09", true, stream.Name.EndsWith (path));

			DeleteFile (path);                	
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
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);

			FileStream stream = null;
			byte[] buffer;

			try {
				buffer = Encoding.ASCII.GetBytes ("test");
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
				stream.Write (buffer, 0, buffer.Length);
			} finally {
				if (stream != null)
					stream.Close();
				DeleteFile (path);
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
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);

			FileStream stream = null;

			try {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
				stream.WriteByte (Byte.MinValue);
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
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
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);

			FileStream stream = null;
			byte[] buffer = new byte [100];

			try {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
				stream.Read (buffer, 0, buffer.Length);
			} finally {
				if (stream != null)
					stream.Close ();
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
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);

			FileStream stream = null;

			try {
				stream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
				int readByte = stream.ReadByte ();
			} finally {
				if (stream != null)
					stream.Close();
				DeleteFile (path);
			}
		}

		private void DeleteFile (string path) 
		{
			if (File.Exists (path))
				File.Delete (path);
		}
			
			
        }
}

