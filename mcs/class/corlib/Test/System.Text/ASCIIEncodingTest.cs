// ASCIIEncodingTest - NUnit Test Cases for the System.Text.ASCIIEncoding class
// 
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

using NUnit.Framework;
using System.Text;
using System;


namespace MonoTests.System.Text {

	public class ASCIIEncodingTest : TestCase {

		private char[] testchars;
		private byte[] testbytes;

		protected override void SetUp ()
		{
			testchars = new char[4];
			testchars[0] = 'T';
			testchars[1] = 'e';
			testchars[2] = 's';
			testchars[3] = 't';
			testbytes = new byte[4];
			testbytes[0] = (byte) 'T';
			testbytes[1] = (byte) 'e';
			testbytes[2] = (byte) 's';
			testbytes[3] = (byte) 't';
		}

		// Test GetBytes(char[])
		public void TestGetBytes1 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes(testchars);
			for (int i = 0; i < testchars.Length; i++)
                		AssertEquals (testchars[i], (char) bytes[i]);
        	}

		// Test GetBytes(char[], int, int)
		public void TestGetBytes2 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes(testchars, 1, 1);
                	AssertEquals (1, bytes.Length);
                	AssertEquals (testchars[1], (char) bytes[0]);
        	}

		// Test non-ASCII char in char[]
		public void TestGetBytes3 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			testchars[2] = (char) 0x80;
			byte[] bytes = ascii_encoding.GetBytes(testchars);
                	AssertEquals ('T', (char) bytes[0]);
                	AssertEquals ('e', (char) bytes[1]);
                	AssertEquals ('?', (char) bytes[2]);
                	AssertEquals ('t', (char) bytes[3]);
        	}

		// Test GetBytes(char[], int, int, byte[], int)
		public void TestGetBytes4 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetBytes(testchars, 1, 1, bytes, 0);
                	AssertEquals (1, cnt);
                	AssertEquals (testchars[1], (char) bytes[0]);
        	}

		// Test GetBytes(string, int, int, byte[], int)
		public void TestGetBytes5 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetBytes("Test", 1, 1, bytes, 0);
                	AssertEquals ('e', (char) bytes[0]);
        	}

		// Test GetBytes(string)
		public void TestGetBytes6 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes("Test");
			for (int i = 0; i < testchars.Length; i++)
                		AssertEquals (testchars[i], (char) bytes[i]);
        	}

		// Test GetChars(byte[])
		public void TestGetChars1 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = ascii_encoding.GetChars(testbytes);
			for (int i = 0; i < testbytes.Length; i++)
                		AssertEquals (testbytes[i], (byte) chars[i]);
        	}

		// Test GetChars(byte[], int, int)
		public void TestGetChars2 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = ascii_encoding.GetChars(testbytes, 1, 1);
                	AssertEquals (1, chars.Length);
                	AssertEquals (testbytes[1], (byte) chars[0]);
        	}

		// Test non-ASCII char in byte[]
		public void TestGetChars3 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			testbytes[2] = 0x80;
			char[] chars = ascii_encoding.GetChars(testbytes);
                	AssertEquals ('T', chars[0]);
                	AssertEquals ('e', chars[1]);
                	AssertEquals ('?', chars[2]);
                	AssertEquals ('t', chars[3]);
        	}

		// Test GetChars(byte[], int, int, char[], int)
		public void TestGetChars4 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = new char[1];
			int cnt = ascii_encoding.GetChars(testbytes, 1, 1, chars, 0);
                	AssertEquals (1, cnt);
                	AssertEquals (testbytes[1], (byte) chars[0]);
        	}

		// Test GetString(char[])
		public void TestGetString1 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			string str = ascii_encoding.GetString(testbytes);
                	AssertEquals ("Test", str);
        	}

		// Test GetString(char[], int, int)
		public void TestGetString2 () 
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			string str = ascii_encoding.GetString(testbytes, 1, 2);
                	AssertEquals ("es", str);
        	}

		// Test Decoder
		public void TestDecoder ()
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = new char[1];
			int cnt = ascii_encoding.GetDecoder().GetChars(testbytes, 1, 1, chars, 0);
                	AssertEquals (1, cnt);
                	AssertEquals (testbytes[1], (byte) chars[0]);
		}

		// Test Decoder
		public void TestEncoder ()
		{
                	Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetEncoder().GetBytes(testchars, 1, 1, bytes, 0, false);
                	AssertEquals (1, cnt);
                	AssertEquals (testchars[1], (char) bytes[0]);
		}

		public void TestZero ()
		{
			Encoding encoding = Encoding.ASCII;
			AssertEquals ("#01", encoding.GetString (new byte [0]), "");
			AssertEquals ("#02", encoding.GetString (new byte [0], 0, 0), "");
		}

	}

}
