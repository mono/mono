//
// Latin1EncodingTest.cs
//
// Author:
//	Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// Copyright (C) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Constraints;

#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

namespace MonoTests.System.Text
{
	[TestFixture]
	public class Latin1EncodingTest
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
			Assert.IsTrue (Encoding.GetEncoding ("latin1").IsBrowserDisplay);
		}

		[Test]
		public void IsBrowserSave ()
		{
			Assert.IsTrue (Encoding.GetEncoding ("latin1").IsBrowserSave);
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			Assert.IsTrue (Encoding.GetEncoding ("latin1").IsMailNewsDisplay);
		}

		[Test]
		public void IsMailNewsSave ()
		{
			Assert.IsTrue (Encoding.GetEncoding ("latin1").IsMailNewsSave);
		}

		[Test] // Test GetBytes(char[])
		public void TestGetBytes1 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = latin1_encoding.GetBytes(testchars);
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars[i], (char) bytes[i]);
		}

		[Test] // Test GetBytes(char[], int, int)
		public void TestGetBytes2 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = latin1_encoding.GetBytes(testchars, 1, 1);
			Assert.AreEqual (1, bytes.Length, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test non-Latin1 char in char[]
		public void TestGetBytes3 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			testchars[2] = (char) 0x100;
			byte[] bytes = latin1_encoding.GetBytes(testchars);
			Assert.AreEqual ('T', (char) bytes [0], "#1");
			Assert.AreEqual ('e', (char) bytes [1], "#2");
			Assert.AreEqual ('?', (char) bytes [2], "#3");
			Assert.AreEqual ('t', (char) bytes [3], "#4");
		}

		[Test] // Test GetBytes(char[], int, int, byte[], int)
		public void TestGetBytes4 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = new Byte[1];
			int cnt = latin1_encoding.GetBytes(testchars, 1, 1, bytes, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test GetBytes(string, int, int, byte[], int)
		public void TestGetBytes5 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = new Byte[1];
			int cnt = latin1_encoding.GetBytes("Test", 1, 1, bytes, 0);
			Assert.AreEqual ('e', (char) bytes [0], "#1");
		}

		[Test] // Test GetBytes(string)
		public void TestGetBytes6 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = latin1_encoding.GetBytes("Test");
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars [i], (char) bytes [i]);
		}

		[Test] // Test GetBytes(string)
		public void TestGetBytes7 ()
		{
			var latin1_encoding = Encoding.GetEncoding ("latin1");

			var expected = new byte [] { 0x3F, 0x20, 0x3F, 0x20, 0x3F };
			var actual = latin1_encoding.GetBytes("\u24c8 \u2075 \u221e"); // normal replacement
			Assert.AreEqual (expected, actual, "#1");

			expected = new byte [] { 0x3F, 0x3F };
			actual = latin1_encoding.GetBytes("\ud83d\ude0a"); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#2");

			expected = new byte [] { 0x3F, 0x3F, 0x20 };
			actual = latin1_encoding.GetBytes("\ud83d\ude0a "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#3");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x3F, 0x20, 0x20 };
			actual = latin1_encoding.GetBytes("  \ud83d\ude0a  "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#4");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x3F, 0x20, 0x20 };
			actual = latin1_encoding.GetBytes("  \ud834\udd1e  "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#5");

			expected = new byte [] { 0x41, 0x42, 0x43, 0x00, 0x41, 0x42, 0x43 };
			actual = latin1_encoding.GetBytes("ABC\0ABC"); // embedded zero byte not replaced
			Assert.AreEqual (expected, actual, "#6");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x20, 0x20 };
			actual = latin1_encoding.GetBytes("  \ud834  "); // invalid surrogate pair replacement
			Assert.AreEqual (expected, actual, "#7");
		}

		[Test] // Test GetChars(byte[])
		public void TestGetChars1 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			char[] chars = latin1_encoding.GetChars(testbytes);
			for (int i = 0; i < testbytes.Length; i++)
				Assert.AreEqual (testbytes[i], (byte) chars[i]);
		}

		[Test] // Test GetChars(byte[], int, int)
		public void TestGetChars2 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			char[] chars = latin1_encoding.GetChars(testbytes, 1, 1);
			Assert.AreEqual (1, chars.Length, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test GetChars(byte[], int, int, char[], int)
		public void TestGetChars4 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			char[] chars = new char[1];
			int cnt = latin1_encoding.GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test GetString(char[])
		public void TestGetString1 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			string str = latin1_encoding.GetString(testbytes);
			Assert.AreEqual ("Test", str);
		}

		[Test] // Test GetString(char[], int, int)
		public void TestGetString2 () 
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			string str = latin1_encoding.GetString(testbytes, 1, 2);
			Assert.AreEqual ("es", str);
		}

		[Test] // Test Decoder
		public void TestDecoder ()
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			char[] chars = new char[1];
			int cnt = latin1_encoding.GetDecoder().GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test Decoder
		public void TestEncoder ()
		{
			Encoding latin1_encoding = Encoding.GetEncoding ("latin1");
			byte[] bytes = new Byte[1];
			int cnt = latin1_encoding.GetEncoder().GetBytes(testchars, 1, 1, bytes, 0, false);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test]
		public void TestZero ()
		{
			Encoding encoding = Encoding.GetEncoding ("latin1");
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0]), "#1");
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0], 0, 0), "#2");
		}

		[Test]
		[ExpectedException (typeof (EncoderFallbackException))]
		public void EncoderFallback ()
		{
			Encoding e = Encoding.GetEncoding ("latin1").Clone () as Encoding;
			e.EncoderFallback = new EncoderExceptionFallback ();
			e.GetBytes ("\u24c8");
		}

		[Test]
	//	[ExpectedException (typeof (ArgumentException))]
		public void DecoderFallback2 ()
		{
			var bytes = new byte[] {
				0x30, 0xa0, 0x31, 0xa8
			};
			var enc = (Encoding)Encoding.GetEncoding ("latin1").Clone ();
			enc.DecoderFallback = new TestFallbackDecoder ();
			
			var chars = new char [7];
			var ret = enc.GetChars (bytes, 0, bytes.Length, chars, 0);
			Console.WriteLine (ret);
			
			for (int i = 0; i < chars.Length; i++) {
				Console.Write ("{0:x2} ", (int)chars [i]);
			}
			Console.WriteLine ();
		}
		
		[Test]
		public void DecoderFallback3 ()
		{
			var bytes = new byte[] {
				0x30, 0xa0, 0x31, 0xa8
			};
			var enc = (Encoding)Encoding.GetEncoding ("latin1").Clone ();
			enc.DecoderFallback = new TestFallbackDecoder ();
			
			var chars = new char[] { '9', '8', '7', '6', '5' };
			var ret = enc.GetChars (bytes, 0, bytes.Length, chars, 0);
			
			Assert.That (ret, Is.EqualTo (4), "ret");
			Assert.That (chars [0], Is.EqualTo ('0'), "chars[0]");
			Assert.That (chars [1], Is.EqualTo ((char)0xA0), "chars[1]");
			Assert.That (chars [2], Is.EqualTo ('1'), "chars[2]");
			Assert.That (chars [3], Is.EqualTo ((char)0xA8), "chars[3]");
			Assert.That (chars [4], Is.EqualTo ('5'), "chars[4]");
		}
		
		class TestFallbackDecoder : DecoderFallback {
			const int count = 2;
			
			public override int MaxCharCount {
				get { return count; }
			}
			
			public override DecoderFallbackBuffer CreateFallbackBuffer ()
			{
				return new Buffer ();
			}
			
			class Buffer : DecoderFallbackBuffer {
				char[] queue;
				int index;
				
				public override int Remaining {
					get {
						return queue.Length - index;
					}
				}
				
				public override char GetNextChar ()
				{
					return index < queue.Length ? queue [index++] : '\0';
				}
				
				public override bool Fallback (byte[] bytes, int unused)
				{
					queue = new char[bytes.Length * count];
					index = 0;
					for (int i = 0; i < bytes.Length; i++) {
						for (int j = 0; j < count; j++)
							queue [index++] = (char)(bytes [i]+j);
					}
					return true;
				}
				
				public override bool MovePrevious ()
				{
					throw new NotImplementedException ();
				}
				
				public override void Reset ()
				{
					base.Reset ();
				}
			}
		}

	}
}
