// ASCIIEncodingTest - NUnit Test Cases for the System.Text.ASCIIEncoding class
// 
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

using System;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Text
{
	public class ASCIIEncodingTest
	{
		private char[] testchars;
		private byte[] testbytes;

		[SetUp]
		public void SetUp ()
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

		[Test]
		public void IsBrowserDisplay ()
		{
			Assert.IsFalse (Encoding.ASCII.IsBrowserDisplay);
		}

		[Test]
		public void IsBrowserSave ()
		{
			Assert.IsFalse (Encoding.ASCII.IsBrowserSave);
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			Assert.IsFalse (Encoding.ASCII.IsMailNewsDisplay);
		}

		[Test]
		public void IsMailNewsSave ()
		{
			Assert.IsFalse (Encoding.ASCII.IsMailNewsSave);
		}

		[Test] // Test GetBytes(char[])
		public void TestGetBytes1 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes(testchars);
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars[i], (char) bytes[i]);
		}

		[Test] // Test GetBytes(char[], int, int)
		public void TestGetBytes2 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes(testchars, 1, 1);
			Assert.AreEqual (1, bytes.Length, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test non-ASCII char in char[]
		public void TestGetBytes3 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			testchars[2] = (char) 0x80;
			byte[] bytes = ascii_encoding.GetBytes(testchars);
			Assert.AreEqual ('T', (char) bytes [0], "#1");
			Assert.AreEqual ('e', (char) bytes [1], "#2");
			Assert.AreEqual ('?', (char) bytes [2], "#3");
			Assert.AreEqual ('t', (char) bytes [3], "#4");
		}

		[Test] // Test GetBytes(char[], int, int, byte[], int)
		public void TestGetBytes4 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetBytes(testchars, 1, 1, bytes, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test GetBytes(string, int, int, byte[], int)
		public void TestGetBytes5 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetBytes("Test", 1, 1, bytes, 0);
			Assert.AreEqual ('e', (char) bytes [0], "#1");
		}

		[Test] // Test GetBytes(string)
		public void TestGetBytes6 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = ascii_encoding.GetBytes("Test");
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars [i], (char) bytes [i]);
		}

		[Test] // Test GetChars(byte[])
		public void TestGetChars1 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = ascii_encoding.GetChars(testbytes);
			for (int i = 0; i < testbytes.Length; i++)
				Assert.AreEqual (testbytes[i], (byte) chars[i]);
		}

		[Test] // Test GetChars(byte[], int, int)
		public void TestGetChars2 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = ascii_encoding.GetChars(testbytes, 1, 1);
			Assert.AreEqual (1, chars.Length, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test non-ASCII char in byte[]
		public void TestGetChars3 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			testbytes[2] = 0x80;
			char[] chars = ascii_encoding.GetChars(testbytes);
			Assert.AreEqual ('T', chars [0], "#1");
			Assert.AreEqual ('e', chars [1], "#2");
			Assert.AreEqual ('?', chars [2], "#3");
			Assert.AreEqual ('t', chars [3], "#4");
		}

		[Test] // Test GetChars(byte[], int, int, char[], int)
		public void TestGetChars4 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = new char[1];
			int cnt = ascii_encoding.GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test GetString(char[])
		public void TestGetString1 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			string str = ascii_encoding.GetString(testbytes);
			Assert.AreEqual ("Test", str);
		}

		[Test] // Test GetString(char[], int, int)
		public void TestGetString2 () 
		{
			Encoding ascii_encoding = Encoding.ASCII;
			string str = ascii_encoding.GetString(testbytes, 1, 2);
			Assert.AreEqual ("es", str);
		}

		[Test] // Test invalid byte handling
		public void TestGetString3 () 
		{
			Encoding encoding = Encoding.ASCII;
			byte [] bytes = new byte [] {0x61, 0xE1, 0xE2};
			string s = encoding.GetString (bytes, 0, 3);
#if NET_2_0
			Assert.AreEqual ("a??", s);
#else
			Assert.AreEqual ("aab", s);
#endif
		}

		[Test] // Test Decoder
		public void TestDecoder ()
		{
			Encoding ascii_encoding = Encoding.ASCII;
			char[] chars = new char[1];
			int cnt = ascii_encoding.GetDecoder().GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test Decoder
		public void TestEncoder ()
		{
			Encoding ascii_encoding = Encoding.ASCII;
			byte[] bytes = new Byte[1];
			int cnt = ascii_encoding.GetEncoder().GetBytes(testchars, 1, 1, bytes, 0, false);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test]
		public void TestZero ()
		{
			Encoding encoding = Encoding.ASCII;
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0]), "#1");
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0], 0, 0), "#2");
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (DecoderFallbackException))]
		public void DecoderFallback ()
		{
			Encoding e = Encoding.ASCII.Clone () as Encoding;
			e.DecoderFallback = new DecoderExceptionFallback ();
			e.GetChars (new byte [] {0x80});
		}
#endif
	}
}
