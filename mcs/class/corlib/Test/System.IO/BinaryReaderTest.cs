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
		protected override void SetUp () { }
		
		private string _codeFileName = "resources" + Path.DirectorySeparatorChar + "AFile.txt";
			
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
	}
}

