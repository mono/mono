// BinaryReaderTest.cs - NUnit Test Cases for the SystemIO.BinaryReader class
//
// Eduardo Garcia Cebollero (kiwnix@yahoo.es)
//
// (C) Eduardo Garcia Cebollero.
// (C) Ximian, Inc.  http://www.ximian.com
// 
using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class BinaryReaderTest : Assertion
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
				AssertNotNull ("#03 no binary reader created", r);
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
				AssertEquals ("#11 No well readed boolean: ", a [0], b);
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
				AssertEquals ("#13 No well readed byte: ", a [0], b);
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
				AssertEquals ("#15 No well readed Char", a [0], c);
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
						AssertEquals("#2E Wrong Readed Int32 in iteration "+ i,arr_int [i],arr_int2 [i]);
					} catch (IOException e) {
						Fail("#2F Unexpected IO Exception" + e.ToString());
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
				AssertEquals ("#20 the stream pointer have been altered in peek", char1, char2);
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
				AssertEquals ("#22 the stream Has been altered in Seek", char1, char2);
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
				AssertEquals ("#24 the stream Has been altered in Seek", char1, char2);
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
					AssertEquals("#26 Not well readed Byte", int1, arr_byte[0]);
				} catch (Exception e) {
				Fail ("#27 Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(-1,SeekOrigin.End);
					int1 = r.ReadByte();
					AssertEquals("#28 Not well readed Byte",int1,arr_byte[9]);
				} catch (Exception e) {
				Fail ("#29 Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(3,SeekOrigin.Begin);
					int1 = r.ReadByte();
					AssertEquals("#2A Not well readed Byte",int1,arr_byte[3]);
				} catch (Exception e) {
					Fail ("#2B Unexpected exception thrown: " + e.ToString ());
				}
				}
				{
				try {
					r.BaseStream.Seek(2,SeekOrigin.Current);
					int1 = r.ReadByte();
					AssertEquals("#2C Not well readed Int32",int1,arr_byte [6]);
				} catch (Exception e) {
				Fail ("#2D Unexpected exception thrown: " + e.ToString ());
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
		
			AssertEquals ("test#01", 0, reader.Read ());
			AssertEquals ("test#02", 1, reader.Read ());
			AssertEquals ("test#03", 2, reader.Read ());
			AssertEquals ("test#04", 3, reader.Read ());
			AssertEquals ("test#05", -1, reader.Read ());		
		} finally {
			reader.Close ();
			stream.Close ();
		}
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
		
			AssertEquals ("test#01", 0, reader.PeekChar ());
			AssertEquals ("test#02", 0, reader.PeekChar ());
			AssertEquals ("test#03", 0, reader.Read ());
			AssertEquals ("test#03", 1, reader.Read ());
			AssertEquals ("test#03", 2, reader.PeekChar ());
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
		
			AssertEquals ("test#01", 4, reader.BaseStream.Length);
			AssertEquals ("test#02", true, reader.BaseStream.CanRead);		
			reader.Close ();
			AssertEquals ("test#03", null, reader.BaseStream);
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
			AssertEquals ("test#01", 0, bytes [0]);
			AssertEquals ("test#02", 1, bytes [1]);
			AssertEquals ("test#03", 2, bytes [2]);

			bytes = new byte [6];
			reader.Read (bytes, 3, 3);
			AssertEquals ("test#04", 0, bytes [0]);
			AssertEquals ("test#05", 0, bytes [1]);
			AssertEquals ("test#06", 0, bytes [2]);
			AssertEquals ("test#07", 3, bytes [3]);
			AssertEquals ("test#08", 4, bytes [4]);
			AssertEquals ("test#09", 5, bytes [5]);
			
			bytes = new byte [2];
			reader.Read (bytes, 0, 2);
			AssertEquals ("test#10", 0, bytes [0]);
			AssertEquals ("test#11", 0, bytes [1]);				
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
			AssertEquals ("test#01", 'm', chars [0]);
			AssertEquals ("test#02", 'o', chars [1]);
			AssertEquals ("test#03", 'n', chars [2]);

			chars = new char [6];
			reader.Read (chars, 3, 3);
			AssertEquals ("test#04", 0, chars [0]);
			AssertEquals ("test#05", 0, chars [1]);
			AssertEquals ("test#06", 0, chars [2]);
			AssertEquals ("test#07", 'o', chars [3]);
			AssertEquals ("test#08", ':', chars [4]);
			AssertEquals ("test#09", ':', chars [5]);
			
			chars = new char [2];
			reader.Read (chars, 0, 2);
			AssertEquals ("test#08", 0, chars [0]);
			AssertEquals ("test#09", 0, chars [1]);
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
			AssertEquals ("test#01", false, reader.ReadBoolean ());
			AssertEquals ("test#02", true, reader.ReadBoolean ());
			AssertEquals ("test#03", true, reader.ReadBoolean ());
			AssertEquals ("test#04", false, reader.ReadBoolean ());
			AssertEquals ("test#05", true, reader.ReadBoolean ());		
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
		
			AssertEquals ("test#01", 0, reader.ReadByte ());
			AssertEquals ("test#02", 1, reader.ReadByte ());
			AssertEquals ("test#03", 99, reader.ReadByte ());
			AssertEquals ("test#04", 0, reader.ReadByte ());
			AssertEquals ("test#05", 13, reader.ReadByte ());		
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
			AssertEquals ("test#01", 0, bytes [0]);
			AssertEquals ("test#02", 1, bytes [1]);
		
			bytes = reader.ReadBytes (2);
			AssertEquals ("test#03", 99, bytes [0]);
			AssertEquals ("test#04", 0, bytes [1]);
		
			bytes = reader.ReadBytes (2);
			AssertEquals ("test#05", 13, bytes [0]);
			AssertEquals ("test#06", 1, bytes.Length);
			
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

			AssertEquals ("test#01", 0, reader.ReadChar ());
			AssertEquals ("test#02", 1, reader.ReadChar ());
			AssertEquals ("test#03", 99, reader.ReadChar ());
			AssertEquals ("test#04", 0, reader.ReadChar ());
			AssertEquals ("test#05", 13, reader.ReadChar ());
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
			AssertEquals ("test#01", 0, chars [0]);
			AssertEquals ("test#02", 1, chars [1]);
		
			chars = reader.ReadChars (2);
			AssertEquals ("test#03", 99, chars [0]);
			AssertEquals ("test#04", 0, chars [1]);
		
			chars = reader.ReadChars (2);
			AssertEquals ("test#05", 13, chars [0]);
			AssertEquals ("test#06", 1, chars.Length);
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
			stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
			reader = new BinaryReader (stream);		
			AssertEquals ("test#01", -18295873486192640, reader.ReadDecimal ());
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
			AssertEquals ("test#01", 1.89131277973112E-307, reader.ReadDouble ());
			AssertEquals ("test#02", 1.2024538023802E+111, reader.ReadDouble ());	
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
			AssertEquals ("test#01", 321, reader.ReadInt16 ());
			AssertEquals ("test#02", 11040, reader.ReadInt16 ());
			AssertEquals ("test#03", 773, reader.ReadInt16 ());
			AssertEquals ("test#04", 54, reader.ReadInt16 ());		
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
			AssertEquals ("test#01", 723517761, reader.ReadInt32 ());
			AssertEquals ("test#02", 3539717, reader.ReadInt32 ());
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
			AssertEquals ("test#01", 15202969475612993, reader.ReadInt64 ());
			AssertEquals ("test#02", 2471354792417887522, reader.ReadInt64 ());
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
			AssertEquals ("test#01", 65, reader.ReadSByte ());
			AssertEquals ("test#02", -56, reader.ReadSByte ());
			AssertEquals ("test#03", 32, reader.ReadSByte ());
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
			AssertEquals ("test#01", 7.183757E-41, reader.ReadSingle ());
			AssertEquals ("test#01", 3.820471E-37, reader.ReadSingle ());
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
			AssertEquals ("test#01", "mono::", reader.ReadString ());
		
			stream = new MemoryStream (new byte [] {2,109, 111, 3, 111, 58, 58});
			reader = new BinaryReader (stream);
			AssertEquals ("test#02", "mo", reader.ReadString ());
			AssertEquals ("test#03", "o::", reader.ReadString ());
		} finally {
			reader.Close ();
			stream.Close ();			
		}
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
	public void ReadUInt16 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {200, 200, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		try {
			AssertEquals ("test#01", 51400, reader.ReadUInt16 ());
			AssertEquals ("test#02", 11040, reader.ReadUInt16 ());
			AssertEquals ("test#03", 773, reader.ReadUInt16 ());
			AssertEquals ("test#04", 54, reader.ReadUInt16 ());		
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
			AssertEquals ("test#01", 723517761, reader.ReadUInt32 ());
			AssertEquals ("test#02", 3539717, reader.ReadUInt32 ());
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
			AssertEquals ("test#01", 15202969475612993, reader.ReadUInt64 ());
			AssertEquals ("test#02", 2471354792417887522, reader.ReadUInt64 ());
		} finally {
			reader.Close ();
			stream.Close ();			
		}
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
}
}
