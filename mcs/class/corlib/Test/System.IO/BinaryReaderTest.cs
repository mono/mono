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
	public class BinaryReaderTest : TestCase
	{		
		string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
		private string _codeFileName;
			
                public BinaryReaderTest() 
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
			Directory.CreateDirectory (TempFolder);
			_codeFileName = TempFolder + Path.DirectorySeparatorChar + "AFile.txt";
		}

                ~BinaryReaderTest ()
		{
			if (Directory.Exists (TempFolder))
				Directory.Delete (TempFolder, true);
		}

                override protected void SetUp() {

			if (!Directory.Exists (TempFolder))
				Directory.CreateDirectory (TempFolder);
			if (!File.Exists (_codeFileName))
				File.Create (_codeFileName).Close ();
                }

		public void TestCtor1() 
		{
			{
				bool errorThrown = false;
				try {
					BinaryReader r = new BinaryReader ((Stream) null);
				} catch (ArgumentNullException) {
					errorThrown = true;
				}
				Assert ("#01 null string error not thrown", errorThrown);
			}
			{
				bool errorThrown = false;
				FileStream f = new FileStream (_codeFileName, FileMode.Open, FileAccess.Write);
				try {
					BinaryReader r = new BinaryReader (f);
					r.Close ();
				} catch  (ArgumentException) {
					errorThrown = true;
				}
				f.Close ();
				Assert ("#02 no read error not thrown", errorThrown);
			}
			{
				FileStream f = new FileStream (_codeFileName, 
								FileMode.Open, 
								FileAccess.Read);
				BinaryReader r = new BinaryReader (f);
				AssertNotNull ("#03 no binary reader created", r);
				r.Close ();
				f.Close ();
			}
				
		}

		public void TestCtor2 () 
		{
			{
				bool errorThrown = false;
				try {
					BinaryReader r = new BinaryReader ((Stream) null, Encoding.ASCII);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Fail ("#04 Incorrect exception thrown: " + e.ToString ());
				}
				Assert ("#05 null stream error not thrown", errorThrown);
			}
			{
				bool errorThrown = false;
				try {
					BinaryReader r = new BinaryReader ((Stream) null, Encoding.Unicode);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Fail ("#06 Incorrect exception thrown: " + e.ToString ());
				}
				Assert("#07 null stream error not thrown", errorThrown);
			}
			{
				bool errorThrown = false;
				try {
					BinaryReader r = new BinaryReader ((Stream) null, Encoding.UTF7);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Fail ("#08 Incorrect exception thrown: " + e.ToString ());
				}
				Assert ("#09 null stream error not thrown", errorThrown);
			}
			{
				bool errorThrown = false;
				try {
					BinaryReader r = new BinaryReader ((Stream) null, Encoding.UTF8);
				} catch (ArgumentNullException) {
					errorThrown = true;
				} catch (Exception e) {
					Fail ("#0A Incorrect exception thrown: " + e.ToString ());
				}
				Assert ("#0B null stream error not thrown", errorThrown);
			}
		}

		public void TestCtor3 ()
		{
			bool errorThrown = false;
			byte [] b = new byte [30];
			MemoryStream m = new MemoryStream (b);
			try {
				BinaryReader r = new BinaryReader (m, (Encoding) null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			} catch(Exception e) {
				Fail ("#0C Incorrect Exception thrown: " + e.ToString ());
			}
			Assert ("#0D No exception trown: ", errorThrown);
		}

		//TODO: (TestCtor*) Verify the Use of a wrong Stream
		//TODO: (TestClose*) Verify the Close Method
		public void TestClose1 ()
		{
			{
				byte [] b = new byte [30];
				MemoryStream m = new MemoryStream (b);
				try {
					BinaryReader r = new BinaryReader (m);
					r.Close ();
				} catch (Exception e) {
					Fail ("#0E Unhandled Exception: "+ e.ToString ());
				}
			}
		}

		//TODO: (TestRead*) Verify Read Method
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
			try {	
				BinaryReader r = new BinaryReader (m);
				b = r.ReadBoolean ();
				AssertEquals ("#11 No well readed boolean: ", a [0], b);
			} catch (Exception e) {
				Fail ("#12 Unexpected exception thrown: " + e.ToString ());
			}
		}

		public void TestReadByte ()
		{
			byte [] a = {0, 2, 3, 1, 5, 2};
			byte b;
			MemoryStream m = new MemoryStream (a);
			try {
				BinaryReader r = new BinaryReader (m);
				b = r.ReadByte ();
				AssertEquals ("#13 No well readed byte: ", a [0], b);
			} catch (Exception e) {
				Fail ("#14 Unexpected Exception thrown: " + e.ToString ());
			}
		}

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

			MemoryStream m = new MemoryStream (arr_a);
			BinaryReader r = new BinaryReader (m);
			try {
				c = r.ReadChar ();
				AssertEquals ("#15 No well readed Char", a [0], c);
			} catch (Exception e)  {
				Fail ("#16 Unexpeted Exception: " + e.ToString ());
			}
		}

		public void TestReadInt32 () //Uses BinaryWriter!!
		{
			int [] arr_int = {1,10,200,3000,40000,500000,6000000};
			byte [] arr_byte = new byte [28]; //Sizeof arr_int * 4
			int [] arr_int2 = new int [7];
			int i;
			
			MemoryStream mem_stream = new MemoryStream (arr_byte);
			BinaryWriter bin_writer = new BinaryWriter (mem_stream);
			
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
		}


		//-TODO: (TestRead[Type]*) Verify the ReadBoolean, ReadByte ....
		// ReadBoolean, ReadByte, ReadChar, ReadInt32 Done
		
		//TODO: (TestFillBuffer*) Verify the FillBuffer Method
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
				  
			MemoryStream m = new MemoryStream (arr_b);
			BinaryReader r = new BinaryReader (m);
			try {	
				char1 = (char) r.PeekChar ();
				char2 = (char) r.PeekChar ();
				AssertEquals ("#20 the stream pointer have been altered in peek", char1, char2);
			} catch (Exception e) {
				Fail ("#21 Unexpected exception thrown: " + e.ToString ());
			}
		}
		
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

			MemoryStream m = new MemoryStream (arr_b);
			BinaryReader r = new BinaryReader (m);
			try {
				char1 = (char) r.PeekChar ();
				r.BaseStream.Seek (0,SeekOrigin.Current);
				char2 = (char) r.PeekChar ();
				AssertEquals ("#22 the stream Has been altered in Seek", char1, char2);
			} catch (Exception e) {
				Fail ("#23 Unexpected exception thrown: " + e.ToString ());
			}
		}

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
			
			MemoryStream m = new MemoryStream (arr_b);
			BinaryReader r = new BinaryReader (m);
			try {
				char1 = (char) r.PeekChar ();
				r.BaseStream.Seek (3,SeekOrigin.Current);
				r.BaseStream.Seek (-3,SeekOrigin.Current);
				char2 = (char) r.PeekChar ();
				AssertEquals ("#24 the stream Has been altered in Seek", char1, char2);
			} catch (Exception e) {
				Fail ("#25 Unexpected exception thrown: " + e.ToString ());
			}
		}
		public void TestInterleavedSeek1 ()
		{
			byte int1;
			byte [] arr_byte = {0,1,2,3,4,5,6,7,8,9};
			
			MemoryStream m = new MemoryStream (arr_byte);
			BinaryReader r = new BinaryReader (m);

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

		}

	/// <summary>
	/// Throws an exception if stream is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionStream () 
	{
		BinaryReader reader = new BinaryReader (null);
		Assertion.Fail();
	}

	/// <summary>
	/// Throws an exception if encoding is null
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CtorNullExceptionEncoding () 
	{
		MemoryStream stream = new MemoryStream (64);	
		BinaryReader reader = new BinaryReader (stream, null);
		Assertion.Fail();
	}
	
	/// <summary>
	/// Throws an exception if stream does not support writing
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionCannotWrite ()
	{
		string path = TempFolder + "/.BinaryReaderTestFile.1";
		DeleteFile (path);

		FileStream file = new FileStream (path, FileMode.CreateNew, FileAccess.Read);
		BinaryReader breader = new BinaryReader (file);

		if (File.Exists (path))
			File.Delete (path);		
	}

	/// <summary>
	/// Throws an exception if stream is already closed
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionClosedStream ()
	{
		string path = TempFolder + "/.BinaryReaderTestFile.2";
		DeleteFile (path);

		FileStream file = new FileStream (path, FileMode.CreateNew, FileAccess.Write);
		file.Close ();
		BinaryReader breader = new BinaryReader (file);

		if (File.Exists (path))
			File.Delete (path);		
	}

	/// <summary>
	/// Throws an exception if stream is closed
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CtorArgumentExceptionEncoding () 
	{
		MemoryStream stream = new MemoryStream (64);	
		stream.Close ();
		
		BinaryReader reader = new BinaryReader (stream, new ASCIIEncoding ());
		Assertion.Fail();
	}
	
	/// <summary>
	/// Tests read () method
	/// </summary>
	[Test]
	public void Read ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 0, reader.Read ());
		Assertion.AssertEquals ("test#02", 1, reader.Read ());
		Assertion.AssertEquals ("test#03", 2, reader.Read ());
		Assertion.AssertEquals ("test#04", 3, reader.Read ());
		Assertion.AssertEquals ("test#05", -1, reader.Read ());		
	}
	
	[Test]
	public void PeakChar ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 0, reader.PeekChar ());
		Assertion.AssertEquals ("test#02", 0, reader.PeekChar ());
		Assertion.AssertEquals ("test#03", 0, reader.Read ());
		Assertion.AssertEquals ("test#03", 1, reader.Read ());
		Assertion.AssertEquals ("test#03", 2, reader.PeekChar ());
	}
	
	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]		
	public void CloseRead ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		reader.Close ();
		reader.Read ();
	}

	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]		
	public void ClosePeakChar ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		reader.Close ();
		reader.PeekChar ();
	}

	[Test]
	[ExpectedException(typeof(ObjectDisposedException))]
	public void CloseReadBytes ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		reader.Close ();
		reader.ReadBytes (1);
	}

	[Test]
	public void BaseStream ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 4, reader.BaseStream.Length);
		Assertion.AssertEquals ("test#02", true, reader.BaseStream.CanRead);		
		reader.Close ();
		Assertion.AssertEquals ("test#03", null, reader.BaseStream);
	}

	
	/// <summary>
	/// Tests read (byte [], int, int) method
	/// </summary>
	[Test]
	public void ReadByteArray ()
	{
		byte [] bytes = new byte [] {0, 1, 2, 3, 4, 5};
		MemoryStream stream = new MemoryStream (bytes);
		BinaryReader reader = new BinaryReader (stream);
		
		bytes = new byte [3];
		reader.Read (bytes, 0, 3);
		Assertion.AssertEquals ("test#01", 0, bytes [0]);
		Assertion.AssertEquals ("test#02", 1, bytes [1]);
		Assertion.AssertEquals ("test#03", 2, bytes [2]);

		bytes = new byte [6];
		reader.Read (bytes, 3, 3);
		Assertion.AssertEquals ("test#04", 0, bytes [0]);
		Assertion.AssertEquals ("test#05", 0, bytes [1]);
		Assertion.AssertEquals ("test#06", 0, bytes [2]);
		Assertion.AssertEquals ("test#07", 3, bytes [3]);
		Assertion.AssertEquals ("test#08", 4, bytes [4]);
		Assertion.AssertEquals ("test#09", 5, bytes [5]);
		
		bytes = new byte [2];
		reader.Read (bytes, 0, 2);
		Assertion.AssertEquals ("test#10", 0, bytes [0]);
		Assertion.AssertEquals ("test#11", 0, bytes [1]);				
	}
	
	/// <summary>
	/// Test Read (char [], int, int)
	/// </summary>
	[Test]
	public void ReadCharArray ()
	{
		
		MemoryStream stream = new MemoryStream (new byte [] {109, 111, 110, 111, 58, 58});
		BinaryReader reader = new BinaryReader (stream);
		
		char [] chars = new char [3];
		reader.Read (chars, 0, 3);
		Assertion.AssertEquals ("test#01", 'm', chars [0]);
		Assertion.AssertEquals ("test#02", 'o', chars [1]);
		Assertion.AssertEquals ("test#03", 'n', chars [2]);

		chars = new char [6];
		reader.Read (chars, 3, 3);
		Assertion.AssertEquals ("test#04", 0, chars [0]);
		Assertion.AssertEquals ("test#05", 0, chars [1]);
		Assertion.AssertEquals ("test#06", 0, chars [2]);
		Assertion.AssertEquals ("test#07", 'o', chars [3]);
		Assertion.AssertEquals ("test#08", ':', chars [4]);
		Assertion.AssertEquals ("test#09", ':', chars [5]);
		
		chars = new char [2];
		reader.Read (chars, 0, 2);
		Assertion.AssertEquals ("test#08", 0, chars [0]);
		Assertion.AssertEquals ("test#09", 0, chars [1]);

	}
	
	/// <summary>
	/// Test ReadBoolean () method.
	/// </summary>
	[Test]
	public void ReadBoolean ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", false, reader.ReadBoolean ());
		Assertion.AssertEquals ("test#02", true, reader.ReadBoolean ());
		Assertion.AssertEquals ("test#03", true, reader.ReadBoolean ());
		Assertion.AssertEquals ("test#04", false, reader.ReadBoolean ());
		Assertion.AssertEquals ("test#05", true, reader.ReadBoolean ());		
	}
	
	/// <summary>
	/// Test ReadBoolean () method exceptions.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadBooleanException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadBoolean ();
		reader.ReadBoolean ();
		reader.ReadBoolean ();
		Assertion.Fail ();		
	}
	
	/// <summary>
	/// Test ReadByte () method.
	/// </summary>
	[Test]
	public void ReadByte ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 0, reader.ReadByte ());
		Assertion.AssertEquals ("test#02", 1, reader.ReadByte ());
		Assertion.AssertEquals ("test#03", 99, reader.ReadByte ());
		Assertion.AssertEquals ("test#04", 0, reader.ReadByte ());
		Assertion.AssertEquals ("test#05", 13, reader.ReadByte ());		
	}
	
	/// <summary>
	/// Test ReadByte () method exceptions.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadByteException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadByte ();
		reader.ReadByte ();
		reader.ReadByte ();
		Assertion.Fail ();		
	}
	
	/// <summary>
	/// Test ReadBytes (int) method.
	/// </summary>
	[Test]
	public void ReadBytes ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		
		byte [] bytes = reader.ReadBytes (2);
		Assertion.AssertEquals ("test#01", 0, bytes [0]);
		Assertion.AssertEquals ("test#02", 1, bytes [1]);
		
		bytes = reader.ReadBytes (2);
		Assertion.AssertEquals ("test#03", 99, bytes [0]);
		Assertion.AssertEquals ("test#04", 0, bytes [1]);
		
		bytes = reader.ReadBytes (2);
		Assertion.AssertEquals ("test#05", 13, bytes [0]);
		Assertion.AssertEquals ("test#06", 1, bytes.Length);
	}
	
	/// <summary>
	/// Test ReadBytes (int) method exception.
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ReadBytesException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadBytes (-1);		
	}
	
	/// <summary>
	/// Test ReadChar () method.
	/// </summary>
	[Test]
	public void ReadChar ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 0, reader.ReadChar ());
		Assertion.AssertEquals ("test#02", 1, reader.ReadChar ());
		Assertion.AssertEquals ("test#03", 99, reader.ReadChar ());
		Assertion.AssertEquals ("test#04", 0, reader.ReadChar ());
		Assertion.AssertEquals ("test#05", 13, reader.ReadChar ());
	}
	
	/// <summary>
	/// Test ReadChar () method exception.
	/// </summary>
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadCharException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadChar ();
		reader.ReadChar ();
		reader.ReadChar ();
		Assertion.Fail ();
	}

	/// <summary>
	/// Test ReadChars (int) method.
	/// </summary>
	[Test]
	public void ReadChars ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		
		char [] chars = reader.ReadChars (2);
		Assertion.AssertEquals ("test#01", 0, chars [0]);
		Assertion.AssertEquals ("test#02", 1, chars [1]);
		
		chars = reader.ReadChars (2);
		Assertion.AssertEquals ("test#03", 99, chars [0]);
		Assertion.AssertEquals ("test#04", 0, chars [1]);
		
		chars = reader.ReadChars (2);
		Assertion.AssertEquals ("test#05", 13, chars [0]);
		Assertion.AssertEquals ("test#06", 1, chars.Length);
	}
	
	/// <summary>
	/// Test ReadChars (int value) exceptions. If value is negative exception is thrown
	/// </summary>
	[Test]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ReadCharsException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 1, 99, 0, 13});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadChars (-1);
	}
	
	
	/// <summary>
	/// Test ReadDecimal () method.
	/// </summary>
	[Test]
	public void ReadDecimal ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);		
		Assertion.AssertEquals ("test#01", -18295873486192640, reader.ReadDecimal ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]
	public void ReadDecimalException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);		
		reader.ReadDecimal ();
		reader.ReadDecimal ();		
	}
	
	[Test]
	public void ReadDouble ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);

		Assertion.AssertEquals ("test#01", 1.89131277973112E-307, reader.ReadDouble ());
		Assertion.AssertEquals ("test#02", 1.2024538023802E+111, reader.ReadDouble ());	
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]
	public void ReadDoubleException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {0, 0, 0, 0, 0, 0, 65, 0, 0, 0, 0, 0, 0, 0, 0 ,87, 98, 0, 0, 0, 0});
		BinaryReader reader = new BinaryReader (stream);

		reader.ReadDouble ();
		reader.ReadDouble ();
		reader.ReadDouble ();
	}
	
	[Test]
	public void ReadInt16 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 321, reader.ReadInt16 ());
		Assertion.AssertEquals ("test#02", 11040, reader.ReadInt16 ());
		Assertion.AssertEquals ("test#03", 773, reader.ReadInt16 ());
		Assertion.AssertEquals ("test#04", 54, reader.ReadInt16 ());		
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt16Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadInt16 ();
		reader.ReadInt16 ();
	}

	[Test]
	public void ReadInt32 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 723517761, reader.ReadInt32 ());
		Assertion.AssertEquals ("test#02", 3539717, reader.ReadInt32 ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt32Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadInt32 ();
		reader.ReadInt32 ();
	}

	[Test]
	public void ReadInt64 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 15202969475612993, reader.ReadInt64 ());
		Assertion.AssertEquals ("test#02", 2471354792417887522, reader.ReadInt64 ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadInt64Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadInt64 ();
		reader.ReadInt64 ();
		reader.ReadInt64 ();
	}
	
	[Test]
	public void ReadSByte ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 32});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 65, reader.ReadSByte ());
		Assertion.AssertEquals ("test#02", -56, reader.ReadSByte ());
		Assertion.AssertEquals ("test#03", 32, reader.ReadSByte ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadSByteException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadSByte ();
		reader.ReadSByte ();
		reader.ReadSByte ();		
	}
	
	[Test]
	public void ReadSingle ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 0, 0, 0, 1, 2, 3, 4});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 7.183757E-41, reader.ReadSingle ());
		Assertion.AssertEquals ("test#01", 3.820471E-37, reader.ReadSingle ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadSingleException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 200, 0, 0, 0, 1, 2, 3, 4});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadSingle ();
		reader.ReadSingle ();
		reader.ReadSingle ();
	}
	
	[Test]
	public void ReadString ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {6,109, 111, 110, 111, 58, 58});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", "mono::", reader.ReadString ());
		
		stream = new MemoryStream (new byte [] {2,109, 111, 3, 111, 58, 58});
		reader = new BinaryReader (stream);
		Assertion.AssertEquals ("test#02", "mo", reader.ReadString ());
		Assertion.AssertEquals ("test#03", "o::", reader.ReadString ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]		
	public void ReadStringException ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {2,109, 111, 3, 111, 58, 58});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadString ();
		reader.ReadString ();
		reader.ReadString ();
	}
	
	[Test]
	public void ReadUInt16 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {200, 200, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 51400, reader.ReadUInt16 ());
		Assertion.AssertEquals ("test#02", 11040, reader.ReadUInt16 ());
		Assertion.AssertEquals ("test#03", 773, reader.ReadUInt16 ());
		Assertion.AssertEquals ("test#04", 54, reader.ReadUInt16 ());		
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt16Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1});
		BinaryReader reader = new BinaryReader (stream);
		reader.ReadUInt16 ();
		reader.ReadUInt16 ();
	}

	[Test]
	public void ReadUInt32 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 723517761, reader.ReadUInt32 ());
		Assertion.AssertEquals ("test#02", 3539717, reader.ReadUInt32 ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt32Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadUInt32 ();
		reader.ReadUInt32 ();
	}

	[Test]
	public void ReadUInt64 ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		Assertion.AssertEquals ("test#01", 15202969475612993, reader.ReadUInt64 ());
		Assertion.AssertEquals ("test#02", 2471354792417887522, reader.ReadUInt64 ());
	}
	
	[Test]
	[ExpectedException(typeof(EndOfStreamException))]	
	public void ReadUInt64Exception ()
	{
		MemoryStream stream = new MemoryStream (new byte [] {65, 1, 32, 43, 5, 3, 54, 0, 34, 5, 7, 4, 23, 4, 76, 34, 76, 2, 6,45});
		BinaryReader reader = new BinaryReader (stream);
		
		reader.ReadUInt64 ();
		reader.ReadUInt64 ();
		reader.ReadUInt64 ();
	}	

	private void DeleteFile (string path)
	{
		if (File.Exists (path))
			File.Delete (path);
	}	
}
}
