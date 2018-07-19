// StdioFileStreamTest.cs - NUnit2 Test Cases for Mono.Unix.StdioFileStream class
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
using Mono.Unix;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class StdioFileStreamTest {

		static string BaseTempFolder = Path.Combine (Path.GetTempPath (),
			"MonoTests.Mono.Unix.Tests");
		static string TempFolder;
		static readonly char DSC = Path.DirectorySeparatorChar;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			try {
				// Try to cleanup from any previous NUnit run.
				Directory.Delete (BaseTempFolder, true);
			} catch (Exception) {
			}
		}

		[SetUp]
		public void SetUp ()
		{
			int i = 0;
			do {
				TempFolder = Path.Combine (BaseTempFolder, (++i).ToString());
			} while (Directory.Exists (TempFolder));
			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				// This might throw an exception on Windows
				// since the directory may contain open files.
				Directory.Delete (TempFolder, true);
			} catch (Exception e) {
				Console.WriteLine (e);
			}
		}

		public void TestCtr ()
		{
			string path = TempFolder + DSC + "testfilestream.tmp.1";
			DeleteFile (path);
			StdioFileStream stream = null;
			try {
				stream = new StdioFileStream (path, FileMode.Create);
			} finally {

				if (stream != null)
					stream.Close ();
				DeleteFile (path);                	
			}
		}

		[Test]
		public void CtorArgumentException1 ()
		{
			Assert.Throws<ArgumentException> (() => {
				StdioFileStream stream;
				stream = new StdioFileStream ("", FileMode.Create);
				stream.Close ();
			});
		}

		[Test]
		public void CtorArgumentNullException ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				StdioFileStream stream = new StdioFileStream (null, FileMode.Create);
				stream.Close ();
			});
		}

		[Test]
		public void CtorFileNotFoundException1 ()
		{
			Assert.Throws<FileNotFoundException> (() => {
				string path = TempFolder + DSC + "thisfileshouldnotexists.test";
				DeleteFile (path);
				StdioFileStream stream = null;
				try {
					stream = new StdioFileStream (path, FileMode.Open);
				} finally {
					if (stream != null)
						stream.Close ();
					DeleteFile (path);
				}
			});
		}

		[Test]
		public void CtorFileNotFoundException2 ()
		{
			Assert.Throws<FileNotFoundException> (() => {
				string path = TempFolder + DSC + "thisfileshouldNOTexists.test";
				DeleteFile (path);
				StdioFileStream stream = null;

				try {
					stream = new StdioFileStream (path, FileMode.Truncate);
				} finally {
					if (stream != null)
						stream.Close ();

					DeleteFile (path);
				}
			});
		} 

		[Test]
		public void CtorIOException1 ()
		{
			Assert.Throws<IOException> (() => {
				string path = TempFolder + DSC + "thisfileshouldexists.test";
				StdioFileStream stream = null;
				DeleteFile (path);
				try {
					stream = new StdioFileStream (path, FileMode.CreateNew);
					stream.Close ();
					stream = null;
					stream = new StdioFileStream (path, FileMode.CreateNew);
				} finally {

					if (stream != null)
						stream.Close ();
					DeleteFile (path);
				}
			});
		}

		[Test]
		public void CtorArgumentOutOfRangeException1 ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				StdioFileStream stream = null;
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);
				try {
					stream = new StdioFileStream (path, FileMode.Append | FileMode.CreateNew);
				} finally {
					if (stream != null)
						stream.Close ();
					DeleteFile (path);
				}
			});
		}

		[Test]
		public void CtorArgumentOutOfRangeException2 ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				StdioFileStream stream = null;
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);
				try {
					stream = new StdioFileStream ("test.test.test", FileMode.Append | FileMode.Open);
				} finally {
					if (stream != null)
						stream.Close ();
					DeleteFile (path);
				}
			});
		}

		[Test]
		public void CtorDirectoryNotFoundException ()
		{
			Assert.Throws<DirectoryNotFoundException> (() => {
				string path = TempFolder + DSC + "thisDirectoryShouldNotExists";
				if (Directory.Exists (path))
					Directory.Delete (path, true);

				StdioFileStream stream = null;
				try {
					stream = new StdioFileStream (path + DSC + "eitherthisfile.test", FileMode.CreateNew);
				} finally {

					if (stream != null)
						stream.Close ();

					if (Directory.Exists (path))
						Directory.Delete (path, true);
				}
			});
		}

		[Test]
		public void CtorArgumentException3 ()
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				StdioFileStream stream = null;

				DeleteFile (path);

				try {
					stream = new StdioFileStream (".test.test.test.2", FileMode.Truncate, FileAccess.Read);
				} finally {
					if (stream != null)
						stream.Close ();

					DeleteFile (path);
				}
			});
		}

		// StdioFileStream doesn't mimic the "no writing by another object" rule
		/* [Test] */
		public void CtorIOException ()
		{
			Assert.Throws<IOException> (() => {
				string path = TempFolder + DSC + "CTorIOException.Test";
				StdioFileStream stream = null;
				StdioFileStream stream2 = null;
				DeleteFile (path);

				try {
					stream = new StdioFileStream (path, FileMode.CreateNew);

					// used by an another process
					stream2 = new StdioFileStream (path, FileMode.OpenOrCreate);
				} finally {
					if (stream != null)
						stream.Close ();
					if (stream2 != null)
						stream2.Close ();
					DeleteFile (path);
				}
			});
		}

		[Test]
		public void CtorAccess1Read2Read ()
		{
			StdioFileStream fs = null;
			StdioFileStream fs2 = null;
			string tempPath = Path.Combine (TempFolder, "temp");
			try {
				if (!File.Exists (tempPath)) {
					TextWriter tw = File.CreateText (tempPath);
					tw.Write ("FOO");
					tw.Close ();
				}
				fs = new StdioFileStream (tempPath, FileMode.Open, FileAccess.Read);
				fs2 = new StdioFileStream (tempPath, FileMode.Open, FileAccess.Read);
			} finally {
				if (fs != null)
					fs.Close ();
				if (fs2 != null)
					fs2.Close ();
			}
		}

		[Test]
		public void Write ()
		{
			string path = TempFolder + DSC + "StdioFileStreamTest.Write";

			DeleteFile (path);

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);

			byte[] outbytes = new byte [] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
			byte[] bytes = new byte [15];

			// Check that the data is flushed when we overflow the buffer
			// with a large amount of data
			stream.Write (outbytes, 0, 5);
			stream.Write (outbytes, 5, 10);
			stream.Seek (0, SeekOrigin.Begin);

			stream.Read (bytes, 0, 15);
			for (int i = 0; i < 15; ++i)
				Assert.AreEqual (i + 1, bytes [i]);

			// Check that the data is flushed when we overflow the buffer
			// with a small amount of data
			stream.Write (outbytes, 0, 7);
			stream.Write (outbytes, 7, 7);
			stream.Write (outbytes, 14, 1);

			stream.Seek (15, SeekOrigin.Begin);
			Array.Clear (bytes, 0, bytes.Length);
			stream.Read (bytes, 0, 15);
			for (int i = 0; i < 15; ++i)
				Assert.AreEqual (i + 1, bytes [i]);
			stream.Close ();
		}

		[Test]
		public void Length ()
		{
			// Test that the Length property takes into account the data
			// in the buffer
			string path = TempFolder + DSC + "StdioFileStreamTest.Length";

			DeleteFile (path);

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew);

			byte[] outbytes = new byte [] {1, 2, 3, 4};

			stream.Write (outbytes, 0, 4);
			Assert.AreEqual (stream.Length, 4);
			stream.Close ();
		}

		[Test]
		public void Flush ()
		{
#if XXX
		    // This test depends too much on the internal implementation of stdio's FILE
		    
			string path = TempFolder + DSC + "StdioFileStreamTest.Flush";
			StdioFileStream stream = null;
			StdioFileStream stream2 = null;

			DeleteFile (path);

			try {
				stream = new StdioFileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);
				stream2 = new StdioFileStream (path, FileMode.Open, FileAccess.ReadWrite);

				stream.Write (new byte [] {1, 2, 3, 4, 5}, 0, 5);

				byte [] bytes = new byte [5];
				stream2.Read (bytes, 0, 5);

				Assert.AreEqual (0, bytes [0], "test#01");
				Assert.AreEqual (0, bytes [1], "test#02");
				Assert.AreEqual (0, bytes [2], "test#03");
				Assert.AreEqual (0, bytes [3], "test#04");

				stream.Flush ();
				stream2.Read (bytes, 0, 5);			
				Assert.AreEqual (1, bytes [0], "test#05");
				Assert.AreEqual (2, bytes [1], "test#06");
				Assert.AreEqual (3, bytes [2], "test#07");
				Assert.AreEqual (4, bytes [3], "test#08");
			} finally {
				if (stream != null)
					stream.Close ();
				if (stream2 != null)
					stream2.Close ();

				Console.WriteLine ("P: " + path);
				//DeleteFile (path);
			}
#endif
		}

		[Test]
		public void TestDefaultProperties ()
		{
#if XXX
			string path = TempFolder + Path.DirectorySeparatorChar + "testStdioFileStream.tmp.2";
			DeleteFile (path);

			StdioFileStream stream = new StdioFileStream (path, FileMode.Create);

			Assert.AreEqual (true, stream.CanRead, "test#01");
			Assert.AreEqual (true, stream.CanSeek, "test#02");
			Assert.AreEqual (true, stream.CanWrite, "test#03");
			Assert.AreEqual (0, stream.Position, "test#06");
			Assert.AreEqual ("Mono.Unix.StdioFileStream", stream.ToString(), "test#07");
			stream.Close ();
			DeleteFile (path);

			stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
			Assert.AreEqual (true, stream.CanRead, "test#08");
			Assert.AreEqual (true, stream.CanSeek, "test#09");
			Assert.AreEqual (false, stream.CanWrite, "test#10");
			Assert.AreEqual (0, stream.Position, "test#13");
			Assert.AreEqual ("Mono.Unix.StdioFileStream", stream.ToString(), "test#14");                	
			stream.Close ();

			stream = new StdioFileStream (path, FileMode.Truncate, FileAccess.Write);
			Assert.AreEqual (false, stream.CanRead, "test#15");
			Assert.AreEqual (true, stream.CanSeek, "test#16");
			Assert.AreEqual (true, stream.CanWrite, "test#17");
			Assert.AreEqual (0, stream.Position, "test#20");
			Assert.AreEqual ("Mono.Unix.StdioFileStream", stream.ToString(), "test#21");                	
			stream.Close ();
			DeleteFile (path);
#endif
		}

		// HACK: the values for `fp.ToString()' assume glibc, and may change under
		// a different C library (due to structure of fpos_t).
		[Test]
		public void PositionAfterWrite ()
		{
#if XXX
			string path = TempFolder + DSC + "FST.Position.Test";
			DeleteFile (path);			

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew, 
				FileAccess.ReadWrite);

			FilePosition fp;

			Assert.AreEqual (0, stream.Position, "test #01");
			Assert.AreEqual ("(Mono.Unix.FilePosition 00000000", 
				(fp = stream.FilePosition).ToString().Substring (0, 32), "test#02");
			fp.Dispose ();

			byte[] message = new byte[]{
				(byte) 'H', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o', (byte) ' ',
				(byte) 'W', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd',
			};

			stream.Write (message, 0, message.Length);

			Assert.AreEqual (11, stream.Position, "test #03");
			Assert.AreEqual (message.Length, stream.Position, "test #04");
			Assert.AreEqual ("(Mono.Unix.FilePosition 0B000000", 
				(fp = stream.FilePosition).ToString().Substring (0, 32), "test#04");
			fp.Dispose ();
#endif
		}

		[Test]
		public void Seek ()
		{
			string path = TempFolder + DSC + "FST.Seek.Test";
			DeleteFile (path);			

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);
			StdioFileStream stream2 = new StdioFileStream (path, FileMode.Open, FileAccess.ReadWrite);

			stream.Write (new byte [] {1, 2, 3, 4, 5, 6, 7, 8, 10}, 0, 9);
			Assert.AreEqual (5, stream2.Seek (5, SeekOrigin.Begin), "test#01");
			Assert.AreEqual (-1, stream2.ReadByte (), "test#02");

			Assert.AreEqual (2, stream2.Seek (-3, SeekOrigin.Current), "test#03");
			Assert.AreEqual (-1, stream2.ReadByte (), "test#04");

			Assert.AreEqual (12, stream.Seek (3, SeekOrigin.Current), "test#05");
			Assert.AreEqual (-1, stream.ReadByte (), "test#06");

			Assert.AreEqual (5, stream.Seek (-7, SeekOrigin.Current), "test#07");
			Assert.AreEqual (6, stream.ReadByte (), "test#08");

			Assert.AreEqual (5, stream2.Seek (5, SeekOrigin.Begin), "test#09");
			Assert.AreEqual (6, stream2.ReadByte (), "test#10");

			stream.Close ();
			stream2.Close ();

			DeleteFile (path);
		}

		[Test]
		public void TestSeek ()
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "TestSeek";
			DeleteFile (path);

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);
			stream.Write (new byte[] {1, 2, 3, 4, 5, 6, 7, 8 , 9, 10}, 0, 10);

			stream.Seek (5, SeekOrigin.End);
			Assert.AreEqual (-1, stream.ReadByte (), "test#01");

			stream.Seek (-5, SeekOrigin.End);
			Assert.AreEqual (6, stream.ReadByte (), "test#02");

			try {
				stream.Seek (-11, SeekOrigin.End);
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (IOException), e.GetType (), "test#03");
			}

			stream.Seek (19, SeekOrigin.Begin);
			Assert.AreEqual (-1, stream.ReadByte (), "test#04");

			stream.Seek (1, SeekOrigin.Begin);
			Assert.AreEqual (2, stream.ReadByte (), "test#05");

			stream.Seek (3, SeekOrigin.Current);
			Assert.AreEqual (6, stream.ReadByte (), "test#06");

			stream.Seek (-2, SeekOrigin.Current);
			Assert.AreEqual (5, stream.ReadByte (), "test#07");

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
			Assert.AreEqual (2, stream.ReadByte (), "test#08");
			Assert.AreEqual (2, stream.ReadByte (), "test#09");

			stream.Close ();

			DeleteFile (path);
		}

		[Test]
		public void TestClose ()
		{
#if FALSE
			string path = TempFolder + Path.DirectorySeparatorChar + "TestClose";
			DeleteFile (path);

			StdioFileStream stream = new StdioFileStream (path, FileMode.CreateNew, FileAccess.ReadWrite);

			stream.Write (new byte [] {1, 2, 3, 4}, 0, 4);
			stream.ReadByte ();                	
			stream.Close ();

			try {                	
				stream.ReadByte ();
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#01");
			}

			try {                	
				stream.WriteByte (64);
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#02");
			}

			try {                	
				stream.Flush ();
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#03");
			}

			try { 
				long l = stream.Length;
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#04");
			}

			try { 
				long l = stream.Position;
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#05");
			}

			try { 
				FilePosition fp = stream.FilePosition;
				fp.Dispose ();
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ObjectDisposedException), e.GetType (), "test#05");
			}

			Assert.AreEqual (false, stream.CanRead, "test#06");
			Assert.AreEqual (false, stream.CanSeek, "test#07");
			Assert.AreEqual (false, stream.CanWrite, "test#08");                	

			DeleteFile (path);                	
#endif
		}


		/// <summary>
		/// Checks whether the <see cref="StdioFileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Read" /> and the
		/// <see cref="StdioFileStream.Write(byte[], int, int)" /> method is called.
		/// </summary>
		[Test]
		public void TestWriteVerifyAccessMode ()
		{
			Assert.Throws<NotSupportedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				StdioFileStream stream = null;
				byte[] buffer;

				try {
					buffer = Encoding.ASCII.GetBytes ("test");
					stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
					stream.Write (buffer, 0, buffer.Length);
				} finally {
					if (stream != null)
						stream.Close();
					DeleteFile (path);
				}
			});
		}

		/// <summary>
		/// Checks whether the <see cref="StdioFileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Read" /> and the
		/// <see cref="StdioFileStream.WriteByte(byte)" /> method is called.
		/// </summary>
		[Test]
		public void TestWriteByteVerifyAccessMode ()
		{
			Assert.Throws<NotSupportedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				StdioFileStream stream = null;

				try {
					stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
					stream.WriteByte (Byte.MinValue);
				} finally {
					if (stream != null)
						stream.Close ();
					DeleteFile (path);
				}
			});
		}

		/// <summary>
		/// Checks whether the <see cref="StdioFileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Write" /> and the
		/// <see cref="StdioFileStream.Read(byte[], int, int)" /> method is called.
		/// </summary>
		[Test]
		public void TestReadVerifyAccessMode ()
		{
			Assert.Throws<NotSupportedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				StdioFileStream stream = null;
				byte[] buffer = new byte [100];

				try {
					stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
					stream.Read (buffer, 0, buffer.Length);
				} finally {
					if (stream != null)
						stream.Close ();
				}
			});
		}

		/// <summary>
		/// Checks whether the <see cref="StdioFileStream" /> throws a <see cref="NotSupportedException" />
		/// when the stream is opened with access mode <see cref="FileAccess.Write" /> and the
		/// <see cref="StdioFileStream.ReadByte()" /> method is called.
		/// </summary>
		[Test]
		public void TestReadByteVerifyAccessMode ()
		{
			Assert.Throws<NotSupportedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				StdioFileStream stream = null;

				try {
					stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
					int readByte = stream.ReadByte ();
				} finally {
					if (stream != null)
						stream.Close();
					DeleteFile (path);
				}
			});
		}

		// Check that the stream is flushed even when it doesn't own the
		// handle
		[Test]
		public void TestFlushNotOwningHandle ()
		{
			string path = Path.Combine (TempFolder, "TestFlushNotOwningHandle");
			DeleteFile (path);

			StdioFileStream s = new StdioFileStream (path, FileMode.Create);
			using (StdioFileStream s2 = new StdioFileStream (s.Handle, FileAccess.Write, false)) {
				byte[] buf = new byte [2];
				buf [0] = (int)'1';
				s2.Write (buf, 0, 1);
			}

			s.Position = 0;
			Assert.AreEqual (s.ReadByte (), (int)'1');
			s.Close ();
		}

		private void DeleteFile (string path) 
		{
			if (File.Exists (path))
				File.Delete (path);
		}

		[Test]
		public void Read_OffsetNegative ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
					stream.Read (new byte[0], -1, 1);
				}
			});
		}

		[Test]
		public void Read_OffsetOverflow ()
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
					stream.Read (new byte[0], Int32.MaxValue, 1);
				}
			});
		}

		[Test]
		public void Read_CountNegative ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
					stream.Read (new byte[0], 1, -1);
				}
			});
		}

		[Test]
		public void Read_CountOverflow ()
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
					stream.Read (new byte[0], 1, Int32.MaxValue);
				}
			});
		}

		[Test]
		public void Write_OffsetNegative ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write)) {
					stream.Write (new byte[0], -1, 1);
				}
			});
		}

		[Test]
		public void Write_OffsetOverflow ()
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write)) {
					stream.Write (new byte[0], Int32.MaxValue, 1);
				}
			});
		}

		[Test]
		public void Write_CountNegative ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write)) {
					stream.Write (new byte[0], 1, -1);
				}
			});
		}

		[Test]
		public void Write_CountOverflow ()
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write)) {
					stream.Write (new byte[0], 1, Int32.MaxValue);
				}
			});
		}

		[Test]
		public void Seek_InvalidSeekOrigin () 
		{
			Assert.Throws<ArgumentException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);

				using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
					stream.Seek (0, (SeekOrigin) (-1));
				}
			});
		}

		//
		// This test is invalid as StdioFileStream does not check for
		// -1 as a special invalid file handle, it tests against *zero* 
		// only.
		// See bug: 76506
		//
		//[Test]
		//[ExpectedException (typeof (ArgumentException))]
		//public void Constructor_InvalidFileHandle () 
		//{
		//		new StdioFileStream ((IntPtr)(-1), FileAccess.Read);
		//}

		[Test]
		public void Position_Disposed () 
		{
			Assert.Throws<ObjectDisposedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);
				StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read);
				stream.Close ();
				stream.Position = 0;
			});
		}

		[Test]
		public void Flush_Disposed () 
		{
			Assert.Throws<ObjectDisposedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);
				StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
				stream.Close ();
				stream.Flush ();
			});
		}

		[Test]
		public void Seek_Disposed () 
		{
			Assert.Throws<ObjectDisposedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				DeleteFile (path);
				StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Write);
				stream.Close ();
				stream.Seek (0, SeekOrigin.Begin);
			});
		}

		[Test]
		public void ReadBytePastEndOfStream () 
		{
			string path = TempFolder + Path.DirectorySeparatorChar + "temp";
			DeleteFile (path);
			using (StdioFileStream stream = new StdioFileStream (path, FileMode.OpenOrCreate, FileAccess.Read)) {
				stream.Seek (0, SeekOrigin.End);
				Assert.AreEqual (-1, stream.ReadByte (), "ReadByte");
				stream.Close ();
			}
		}

		[Test]
		public void SetLengthWithClosedBaseStream ()
		{
			Assert.Throws<NotSupportedException> (() => {
				string path = TempFolder + Path.DirectorySeparatorChar + "temp";
				StdioFileStream fs = new StdioFileStream (path, FileMode.Create);
				BufferedStream bs = new BufferedStream (fs);
				fs.Close ();

				bs.SetLength (1000);
			});
		}
	}
}

