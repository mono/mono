// BinaryReaderTest.cs - NUnit Test Cases for the SystemIO.BinaryReader class
//
// Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//
// (C) Eduardo Garcia Cebollero.
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class BinaryReaderTest
	{		
		static string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
		static string _codeFileName = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
			
                [SetUp]
		public void SetUp() {
			if (!Directory.Exists (TempFolder))
				Directory.CreateDirectory (TempFolder);
			
			if (!File.Exists (_codeFileName))
				File.Create (_codeFileName).Close ();
                }

                [TearDown]
		public void TearDown ()
		{
				if (Directory.Exists (TempFolder))
					Directory.Delete (TempFolder, true);
		}		

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException1 ()
		{
			BinaryReader r = null;
			try {
				r = new BinaryReader ((Stream) null);
			} finally {
				if (r != null)
					r.Close ();				
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException2 ()				
		{
			BinaryReader r = null;
			try {
				r = new BinaryReader ((Stream) null, Encoding.ASCII);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException3 ()				
		{
			BinaryReader r = null;
			try {				
				r = new BinaryReader ((Stream) null, Encoding.Unicode);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException4 ()				
		{	
			BinaryReader r = null;
			try {
				r = new BinaryReader ((Stream) null, Encoding.UTF7);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException5 ()				
		{	
			BinaryReader r = null;
			try {
				r = new BinaryReader ((Stream) null, Encoding.UTF8);
			} finally {
				if (r != null)
					r.Close ();
			}
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]		
		public void CtorArgumentNullException6 ()				
		{	
			byte [] b = new byte [30];
			MemoryStream m = new MemoryStream (b);
			BinaryReader r = null;
			try {
				r = new BinaryReader (m, (Encoding) null);
			} finally {
				m.Close ();
				if (r != null)
					r.Close ();
			}
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorArgumentException ()
		{
			FileStream f = null;
			BinaryReader r = null;
			try {
				f = new FileStream (_codeFileName, FileMode.Open, FileAccess.Write);
				r = new BinaryReader (f);
			} finally {
				if (r != null)
					r.Close ();
				if (f != null)
					f.Close ();				
			}
		}

		[Test]
		public void Ctor1() 
		{
			FileStream f = null;
			BinaryReader r = null;
			try {
				f = new FileStream (_codeFileName, 
							FileMode.Open, 
							FileAccess.Read);
				r = new BinaryReader (f);
				Assert.IsNotNull (r, "#03 no binary reader created");
			} finally {
				if (r != null)
					r.Close ();
				if (f != null)
					f.Close ();
			}							
		}

		//TODO: (TestCtor*) Verify the Use of a wrong Stream
		//TODO: (TestClose*) Verify the Close Method
		[Test]
		public void Close1 ()
		{
			byte [] b = new byte [30];
			MemoryStream m = new MemoryStream (b);
			BinaryReader r = null;
			try {
				r = new BinaryReader (m);
			} finally {
				r.Close ();
				m.Close ();
			}
		}

		//TODO: (TestRead*) Verify Read Method
		[Test]
		public void TestReadBoolean ()
		{
			bool [] a = {true, true, false};
			byte [] arr_a = new byte [3];
			int i = 0;
			foreach (bool a1 in a) {
				  arr_a [i] = Convert.ToByte (a1);
				  i++;
			}
				  
			bool b;
			MemoryStream m = new MemoryStream (arr_a);
			BinaryReader r = null;
			try {	
				r = new BinaryReader (m);
				b = r.ReadBoolean ();
				Assert.AreEqual (a [0], b, "#11 No well readed boolean: ");
			} finally {
				if (r != null)
					r.Close ();				
				m.Close ();		
			}
		}

		[Test]
		public void TestReadByte ()
		{
			byte [] a = {0, 2, 3, 1, 5, 2};
			byte b;
			MemoryStream m = new MemoryStream (a);
			BinaryReader r = null;
			try {
				r = new BinaryReader (m);
				b = r.ReadByte ();
				Assert.AreEqual (a [0], b, "#13 No well readed byte: ");
			} finally {
				if (r != null)
					r.Close ();
				m.Close ();
			}
		}

		[Test]
		public void TestReadChar()
		{
			char [] a = {'a','b','c','d','e'};
			byte [] arr_a = new byte [5];
			int i = 0;
			char c;

			foreach (char a1 in a) {
			   arr_a [i] = Convert.ToByte (a1);
			   i++;
			}

			MemoryStream m = null;
			BinaryReader r = null;
			try {
				m = new MemoryStream (arr_a);
				r = new BinaryReader (m);
				c = r.ReadChar ();
				Assert.AreEqual (a [0], c, "#15 No well readed Char");
			} finally  {
				r.Close ();
				m.Close ();				
			}
		}

		[Test]
		public void TestReadInt32 () //Uses BinaryWriter!!
		{
			int [] arr_int = {1,10,200,3000,40000,500000,6000000};
			byte [] arr_byte = new byte [28]; //Sizeof arr_int * 4
			int [] arr_int2 = new int [7];
			int i;
			
			MemoryStream mem_stream = null;
			BinaryWriter bin_writer = null;
			try {
				mem_stream = new MemoryStream (arr_byte);
				bin_writer = new BinaryWriter (mem_stream);
				foreach (int elem in arr_int)	{
					bin_writer.Write(elem);
				}
			
				mem_stream.Seek(0,SeekOrigin.Begin);
				BinaryReader bin_reader = new BinaryReader (mem_stream);
				bin_reader.BaseStream.Seek(0,SeekOrigin.Begin);
			
				for (i=0;i<7;i++) {
					try{
						arr_int2 [i] = bin_reader.ReadInt32();
						Assert.AreEqual (arr_int [i], arr_int2 [i], "#2E Wrong Readed Int32 in iteration "+ i);
					} catch (IOException e) {
						Assert.Fail ("#2F Unexpected IO Exception" + e.ToString());
					}
				}
			} finally {	
				bin_writer.Close ();
				mem_stream.Close ();				
			}
		}


		//-TODO: (TestRead[Type]*) Verify the ReadBoolean, ReadByte ....
		// ReadBoolean, ReadByte, ReadChar, ReadInt32 Done
		
		//TODO: (TestFillBuffer*) Verify the FillBuffer Method
		[Test]
		public void TestPeekChar ()
		{
			char char1, char2;
			char [] b = {'A', 'B', 'C'};
			byte [] arr_b = new byte [3];
			int i = 0;

			foreach (char b1 in b) {
				arr_b [i] = Convert.ToByte (b1);
				i++;
			}
				  
			MemoryStream m = null;
			BinaryReader r = null;
			
			try {	
				m = new MemoryStream (arr_b);
				r = new BinaryReader (m);
				char1 = (char) r.PeekChar ();
				char2 = (char) r.PeekChar ();
				Assert.AreEqual (char1, char2, "#20 the stream pointer have been altered in peek");
			} finally {
				r.Close ();
				m.Close ();
			}
		}
		
		[Test]
		public void TestBaseSeek1 ()
		{
			char char1, char2;
			char [] b = {'A','B','C','D','E','F'};
			byte [] arr_b = new byte[6];
			int i = 0;
			foreach (char b1 in b) {
				arr_b [i] = Convert.ToByte (b1);
				i++;
			}

			MemoryStream m = null;
			BinaryReader r = null;
			try {
				m = new MemoryStream (arr_b);
				r = new BinaryReader (m);
				char1 = (char) r.PeekChar ();
				r.BaseStream.Seek (0,SeekOrigin.Current);
				char2 = (char) r.PeekChar ();
				Assert.AreEqual (char1, char2, "#22 the stream Has been altered in Seek");
			} finally {
				r.Close ();
				m.Close ();
			}
		}

		[Test]
		public void TestBaseSeek2 ()
		{
			char char1, char2;
			char [] b = {'A','B','C','D','E','F'};
			byte [] arr_b = new byte[6];
			int i = 0;
			foreach (char b1 in b) {
				arr_b [i] = Convert.ToByte (b1);
				i++;
			}
			
			MemoryStream m = null;
			BinaryReader r = null;
			try {
				m = new MemoryStream (arr_b);
				r = new BinaryReader (m);
				char1 = (char) r.PeekChar ();
				r.BaseStream.Seek (3,SeekOrigin.Current);
				r.BaseStream.Seek (-3,SeekOrigin.Current);
				char2 = (char) r.PeekChar ();
				Assert.AreEqual (char1, char2, "#24 the stream Has been altered in Seek");
			} finally {
				r.Close ();
				m.Close ();
			}
		}
		
		[Test]
		public void TestInterleavedSeek1 ()
		{
			byte int1;
			byte [] arr_byte = {0,1,2,3,4,5,6,7,8,9};
			
			MemoryStream m = null;
			BinaryReader r = null;

			try {
				m = new MemoryStream (arr_byte);
				r = new BinaryReader (m);
				{
				try {
					int1 = r.ReadByte();
					Assert.AreEqual (int1, arr_byte[0], "#26 Not well readed Byte");
				} catch (Exception e) {
				Assert.Fail ("#27 Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(-1,SeekOrigin.End);
					int1 = r.ReadByte();
					Assert.AreEqual (int1, arr_byte[9], "#28 Not well readed Byte");
				} catch (Exception e) {
				Assert.Fail ("#29 Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(3,SeekOrigin.Begin);
					int1 = r.ReadByte();
					Assert.AreEqual (int1, arr_byte[3], "#2A Not well readed Byte");
				} catch (Exception e) {
					Assert.Fail ("#2B Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(2,SeekOrigin.Current);
					int1 = r.ReadByte();
					Assert.AreEqual (int1, arr_byte [6], "#2C Not well readed Int32");
				} catch (Exception e) {
				Assert.Fail ("#2D Unexpected exception thrown: " + e.ToString ());
				}
				}
			} finally {
				r.Close ();
				m.Close ();
			}

		}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream () 
	{
		BinaryReader reader = null;
		try {
			reader = new BinaryReader (null);
		} finally {
			if (reader != null)
				reader.Close ();
		}
	}

	/// <summary>
	/// Throws an exception if encoding is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionEncoding () 
	{
		MemoryStream stream = null;	
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (64);	
			reader = new BinaryReader (stream, null);
		} finally {
			if (reader != null)
				reader.Close ();
			if (stream != null)
				stream.Close ();
		}
	}
	
	/// <summary>
	/// Throws an exception if stream does not support writing
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionCannotWrite ()
	{
		string path = TempFolder + "/BinaryReaderTestFile.1";
		DeleteFile (path);
		FileStream file = null;
		BinaryReader breader = null;

		try {
			file = new FileStream (path, FileMode.CreateNew, FileAccess.Read);
			breader = new BinaryReader (file);
		} finally {
			if (breader != null)
				breader.Close ();
			if (file != null)
				file.Close ();
			DeleteFile (path);		
		}
	}

	/// <summary>
	/// Throws an exception if stream is already closed
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionClosedStream ()
	{
		string path = TempFolder + "/BinaryReaderTestFile.2";
		DeleteFile (path);
		FileStream file = null;
		BinaryReader breader = null;
		try {
			file = new FileStream (path, FileMode.CreateNew, FileAccess.Write);
			file.Close ();
			breader = new BinaryReader (file);
		} finally {
			
			if (breader != null)
				breader.Close ();
			
			if (file != null)
				file.Close ();			
			DeleteFile (path);			
		}
	}

	/// <summary>
	/// Throws an exception if stream is closed
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionEncoding () 
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (64);	
			stream.Close ();
		
			reader = new BinaryReader (stream, new ASCIIEncoding ());
		} finally {
			if (reader != null)
				reader.Close ();
			if (stream != null)
				stream.Close ();
		}
		
	}
	
	/// <summary>
	/// Tests read () method
	/// </summary>
	[Test]
	public void Read ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;

		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
		
			Assert.AreEqual (0, reader.Read (), "test#01");
			Assert.AreEqual (1, reader.Read (), "test#02");
			Assert.AreEqual (2, reader.Read (), "test#03");
			Assert.AreEqual (3, reader.Read (), "test#04");
			Assert.AreEqual (-1, reader.Read (), "test#05");		
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Read_Bytes_BufferNull () 
	{
		byte[] b = null;
		new BinaryReader (new MemoryStream ()).Read (b, 0, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Read_Bytes_IndexNegative () 
	{
		byte[] array = new byte [8];
		new BinaryReader (new MemoryStream ()).Read (array, -1, array.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_Bytes_IndexOverflow () 
	{
		byte[] array = new byte [8];
		new BinaryReader (new MemoryStream ()).Read (array, Int32.MaxValue, array.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Read_Bytes_CountNegative () 
	{
		byte[] array = new byte [8];
		new BinaryReader (new MemoryStream ()).Read (array, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_Bytes_CountOverflow () 
	{
		byte[] array = new byte [8];
		new BinaryReader (new MemoryStream ()).Read (array, 0, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void Read_Chars_BufferNull () 
	{
		char[] c = null;
		new BinaryReader (new MemoryStream ()).Read (c, 0, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Read_Chars_IndexNegative () 
	{
		char[] array = new char [8];
		new BinaryReader (new MemoryStream ()).Read (array, -1, array.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_Chars_IndexOverflow () 
	{
		char[] array = new char [8];
		new BinaryReader (new MemoryStream ()).Read (array, Int32.MaxValue, array.Length);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void Read_Chars_CountNegative () 
	{
		char[] array = new char [8];
		new BinaryReader (new MemoryStream ()).Read (array, 0, -1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void Read_Chars_CountOverflow () 
	{
		char[] array = new char [8];
		new BinaryReader (new MemoryStream ()).Read (array, 0, Int32.MaxValue);
	}

	[Test]
	public void PeakChar ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
		
			Assert.AreEqual (0, reader.PeekChar (), "test#01");
			Assert.AreEqual (0, reader.PeekChar (), "test#02");
			Assert.AreEqual (0, reader.Read (), "test#03");
			Assert.AreEqual (1, reader.Read (), "test#03");
			Assert.AreEqual (2, reader.PeekChar (), "test#03");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]		
	public void CloseRead ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;	
		
		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);	
			reader.Close ();
			reader.Read ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]		
	public void ClosePeakChar ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
			reader.Close ();
			reader.PeekChar ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void CloseReadBytes ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
			reader.Close ();
			reader.ReadBytes (1);
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	public void BaseStream ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
		
			Assert.AreEqual (4, reader.BaseStream.Length, "test#01");
			Assert.AreEqual (true, reader.BaseStream.CanRead, "test#02");		
			reader.Close ();
			Assert.AreEqual (null, reader.BaseStream, "test#03");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	
	/// <summary>
	/// Tests read (byte [], int, int) method
	/// </summary>
	[Test]
	public void ReadByteArray ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3, 4, 5};
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {

			stream = new MemoryStream (bytes);
			reader = new BinaryReader (stream);
			
			bytes = new byte [3];
			reader.Read (bytes, 0, 3);
			Assert.AreEqual (0, bytes [0], "test#01");
			Assert.AreEqual (1, bytes [1], "test#02");
			Assert.AreEqual (2, bytes [2], "test#03");

			bytes = new byte [6];
			reader.Read (bytes, 3, 3);
			Assert.AreEqual (0, bytes [0], "test#04");
			Assert.AreEqual (0, bytes [1], "test#05");
			Assert.AreEqual (0, bytes [2], "test#06");
			Assert.AreEqual (3, bytes [3], "test#07");
			Assert.AreEqual (4, bytes [4], "test#08");
			Assert.AreEqual (5, bytes [5], "test#09");
			
			bytes = new byte [2];
			reader.Read (bytes, 0, 2);
			Assert.AreEqual (0, bytes [0], "test#10");
			Assert.AreEqual (0, bytes [1], "test#11");				
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test Read (char [], int, int)
	/// </summary>
	[Test]
	public void ReadCharArray ()
	{
		
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {109, 111, 110, 111, 58, 58});
			reader = new BinaryReader (stream);
			char [] chars = new char [3];
			reader.Read (chars, 0, 3);
			Assert.AreEqual ('m', chars [0], "test#01");
			Assert.AreEqual ('o', chars [1], "test#02");
			Assert.AreEqual ('n', chars [2], "test#03");

			chars = new char [6];
			reader.Read (chars, 3, 3);
			Assert.AreEqual (0, chars [0], "test#04");
			Assert.AreEqual (0, chars [1], "test#05");
			Assert.AreEqual (0, chars [2], "test#06");
			Assert.AreEqual ('o', chars [3], "test#07");
			Assert.AreEqual (':', chars [4], "test#08");
			Assert.AreEqual (':', chars [5], "test#09");
			
			chars = new char [2];
			reader.Read (chars, 0, 2);
			Assert.AreEqual (0, chars [0], "test#08");
			Assert.AreEqual (0, chars [1], "test#09");
		} finally {
			reader.Close ();
			stream.Close ();
		}

	}
	
	/// <summary>
	/// Test ReadBoolean () method.
	/// </summary>
	[Test]
	public void ReadBoolean ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
			Assert.AreEqual (false, reader.ReadBoolean (), "test#01");
			Assert.AreEqual (true, reader.ReadBoolean (), "test#02");
			Assert.AreEqual (true, reader.ReadBoolean (), "test#03");
			Assert.AreEqual (false, reader.ReadBoolean (), "test#04");
			Assert.AreEqual (true, reader.ReadBoolean (), "test#05");		
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadBoolean () method exceptions.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadBooleanException ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {0, 1});
			reader = new BinaryReader (stream);
			reader.ReadBoolean ();
			reader.ReadBoolean ();
			reader.ReadBoolean ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadByte () method.
	/// </summary>
	[Test]
	public void ReadByte ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;

		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
		
			Assert.AreEqual (0, reader.ReadByte (), "test#01");
			Assert.AreEqual (1, reader.ReadByte (), "test#02");
			Assert.AreEqual (99, reader.ReadByte (), "test#03");
			Assert.AreEqual (0, reader.ReadByte (), "test#04");
			Assert.AreEqual (13, reader.ReadByte (), "test#05");		
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadByte () method exceptions.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadByteException ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;

		try {
			stream = new MemoryStream (new byte [] {0, 1});
			reader = new BinaryReader (stream);
			reader.ReadByte ();
			reader.ReadByte ();
			reader.ReadByte ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadBytes (int) method.
	/// </summary>
	[Test]
	public void ReadBytes ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		
		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
			
			byte [] bytes = reader.ReadBytes (2);
			Assert.AreEqual (0, bytes [0], "test#01");
			Assert.AreEqual (1, bytes [1], "test#02");
		
			bytes = reader.ReadBytes (2);
			Assert.AreEqual (99, bytes [0], "test#03");
			Assert.AreEqual (0, bytes [1], "test#04");
		
			bytes = reader.ReadBytes (2);
			Assert.AreEqual (13, bytes [0], "test#05");
			Assert.AreEqual (1, bytes.Length, "test#06");
			
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadBytes (int) method exception.
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ReadBytesException ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;

		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
			reader.ReadBytes (-1);		
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadChar () method.
	/// </summary>
	[Test]
	public void ReadChar ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);

			Assert.AreEqual (0, reader.ReadChar (), "test#01");
			Assert.AreEqual (1, reader.ReadChar (), "test#02");
			Assert.AreEqual (99, reader.ReadChar (), "test#03");
			Assert.AreEqual (0, reader.ReadChar (), "test#04");
			Assert.AreEqual (13, reader.ReadChar (), "test#05");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadChar () method exception.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadCharException ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;

		try {
			stream = new MemoryStream (new byte [] {0, 1});
			reader = new BinaryReader (stream);
			reader.ReadChar ();
			reader.ReadChar ();
			reader.ReadChar ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	/// <summary>
	/// Test ReadChars (int) method.
	/// </summary>
	[Test]
	public void ReadChars ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {			
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
			char [] chars = reader.ReadChars (2);
			Assert.AreEqual (0, chars [0], "test#01");
			Assert.AreEqual (1, chars [1], "test#02");
		
			chars = reader.ReadChars (2);
			Assert.AreEqual (99, chars [0], "test#03");
			Assert.AreEqual (0, chars [1], "test#04");
		
			chars = reader.ReadChars (2);
			Assert.AreEqual (13, chars [0], "test#05");
			Assert.AreEqual (1, chars.Length, "test#06");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	/// <summary>
	/// Test ReadChars (int value) exceptions. If value is negative exception is thrown
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ReadCharsException ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
			reader = new BinaryReader (stream);
			reader.ReadChars (-1);
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	
	/// <summary>
	/// Test ReadDecimal () method.
	/// </summary>
	[Test]
	public void ReadDecimal ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;		
		try {
			stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,128, 0, 0, 0, 0, 0});
			reader = new BinaryReader (stream);		
			Assert.AreEqual (-18295873486192640, reader.ReadDecimal (), "test#01");
		} finally {
			reader.Close ();
			stream.Close ();
		}		
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]
	public void ReadDecimalException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);		
		try {
			reader.ReadDecimal ();
			reader.ReadDecimal ();		
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	public void ReadDouble ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (1.8913127797311212E-307d, reader.ReadDouble (), "test#01");
			Assert.AreEqual (1.2024538023802026E+111d, reader.ReadDouble (), "test#02");	
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]
	public void ReadDoubleException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);
		try {
			reader.ReadDouble ();
			reader.ReadDouble ();
			reader.ReadDouble ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	public void ReadInt16 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (321, reader.ReadInt16 (), "test#01");
			Assert.AreEqual (11040, reader.ReadInt16 (), "test#02");
			Assert.AreEqual (773, reader.ReadInt16 (), "test#03");
			Assert.AreEqual (54, reader.ReadInt16 (), "test#04");		
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt16Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1});
		BinaryReader reader = new BinaryReader (stream);
		try {
			reader.ReadInt16 ();
			reader.ReadInt16 ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	public void ReadInt32 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (723517761, reader.ReadInt32 (), "test#01");
			Assert.AreEqual (3539717, reader.ReadInt32 (), "test#02");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt32Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43});
		BinaryReader reader = new BinaryReader (stream);
		try {
			reader.ReadInt32 ();
			reader.ReadInt32 ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	public void ReadInt64 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (15202969475612993, reader.ReadInt64 (), "test#01");
			Assert.AreEqual (2471354792417887522, reader.ReadInt64 (), "test#02");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt64Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			reader.ReadInt64 ();
			reader.ReadInt64 ();
			reader.ReadInt64 ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	public void ReadSByte ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 32});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			Assert.AreEqual (65, reader.ReadSByte (), "test#01");
			Assert.AreEqual (-56, reader.ReadSByte (), "test#02");
			Assert.AreEqual (32, reader.ReadSByte (), "test#03");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadSByteException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			reader.ReadSByte ();
			reader.ReadSByte ();
			reader.ReadSByte ();		
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}
	
	[Test]
	public void ReadSingle ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 0, 0, 0, 1, 2, 3, 4});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (7.18375658E-41F, reader.ReadSingle (), "test#01");
			Assert.AreEqual (3.82047143E-37F, reader.ReadSingle (), "test#02");
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadSingleException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 0, 0, 0, 1, 2, 3, 4});
		BinaryReader reader = new BinaryReader (stream);
		try {
			reader.ReadSingle ();
			reader.ReadSingle ();
			reader.ReadSingle ();
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}
	
	[Test]
	public void ReadString ()
	{
		MemoryStream stream = null;
		BinaryReader reader = null;
		try {
			stream = new MemoryStream (new byte [] {6,109, 111, 110, 111, 58, 58});
			reader = new BinaryReader (stream);
			Assert.AreEqual ("mono::", reader.ReadString (), "test#01");
		
			stream = new MemoryStream (new byte [] {2,109, 111, 3, 111, 58, 58});
			reader = new BinaryReader (stream);
			Assert.AreEqual ("mo", reader.ReadString (), "test#02");
			Assert.AreEqual ("o::", reader.ReadString (), "test#03");
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}

	[Test]
	public void ReadLongUTF8String ()
	{
		// \u00A9 == (C)
		string s = new String ('\u00A9', 100);
		MemoryStream ms = new MemoryStream ();
		BinaryWriter w = new BinaryWriter (ms);		
		w.Write (s);
		w.Flush ();
		ms.Position = 0;
		BinaryReader r = new BinaryReader (ms);
		Assert.AreEqual (s, r.ReadString ());
	}		
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadStringException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {2,109, 111, 3, 111, 58, 58});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			reader.ReadString ();
			reader.ReadString ();
			reader.ReadString ();
		} finally {
			reader.Close ();
			stream.Close ();
		}
	}

	[Test]
	[ExpectedException (typeof (FormatException))]
	public void ReadStringInvalidLength ()
	{
		int count = 1000;

		byte[] x = new byte[count];

		for (int i = 0; i < count; i++) 
			x[i] = 0xFF;

		BinaryReader reader = new BinaryReader (new MemoryStream (x));
		reader.ReadString ();
	}

	[Test]
	public void ReadUInt16 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {200, 200, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			Assert.AreEqual (51400, reader.ReadUInt16 (), "test#01");
			Assert.AreEqual (11040, reader.ReadUInt16 (), "test#02");
			Assert.AreEqual (773, reader.ReadUInt16 (), "test#03");
			Assert.AreEqual (54, reader.ReadUInt16 (), "test#04");		
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt16Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1});
		BinaryReader reader = new BinaryReader (stream);
		try {
			reader.ReadUInt16 ();
			reader.ReadUInt16 ();
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}

	[Test]
	public void ReadUInt32 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		try {
			Assert.AreEqual (723517761, reader.ReadUInt32 (), "test#01");
			Assert.AreEqual (3539717, reader.ReadUInt32 (), "test#02");
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt32Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			reader.ReadUInt32 ();
			reader.ReadUInt32 ();
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}

	[Test]
	public void ReadUInt64 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			Assert.AreEqual (15202969475612993, reader.ReadUInt64 (), "test#01");
			Assert.AreEqual (2471354792417887522, reader.ReadUInt64 (), "test#02");
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}

	[Test]
	public void Test_ReadZeroBytes ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);

		char [] result = reader.ReadChars (0);
		Assert.AreEqual (result.Length, 0, "ZERO_1");
	}
		
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt64Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			reader.ReadUInt64 ();
			reader.ReadUInt64 ();
			reader.ReadUInt64 ();
		} finally {
			reader.Close ();
			stream.Close ();			
		}
	}	

	private void DeleteFile (string path)
	{
		if (File.Exists (path))
			File.Delete (path);
	}

	class MockBinaryReader : BinaryReader
	{
		public int ReadCharsCounter;
		public int ReadCounter;
		
		public MockBinaryReader (Stream input)
			: base (input)
		{
		}
		
		public override char[] ReadChars (int count)
		{
			++ReadCharsCounter;
			return base.ReadChars (count);
		}
		
		public override int Read (char[] buffer, int index, int count)
		{
			++ReadCounter;
			return base.Read (buffer, index, count);
		}
	}

	[Test]
	public void ReadOverrides ()
	{
		var stream = new MemoryStream ();
		
		using (var writer = new BinaryWriter (stream)) {
			writer.Write ("TEST");
			stream.Seek (0, SeekOrigin.Begin);
		
			using (var reader = new MockBinaryReader (stream)) {
				var readChars = reader.ReadChars (4);
		
				Assert.AreEqual (1, reader.ReadCharsCounter);
				Assert.AreEqual (0, reader.ReadCounter);
		
				reader.Read (readChars, 0, 4);
				Assert.AreEqual (1, reader.ReadCharsCounter);
				Assert.AreEqual (1, reader.ReadCounter);
			}
		}
	}
}
}
