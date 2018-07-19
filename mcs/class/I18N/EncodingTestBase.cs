//
// EncodingTestBase.cs
//
// Author:
//	Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// Copyright (C) 2017 Xamarin, Inc.
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
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoTests.I18N
{
	public class CodePageTestInfo
	{
		public int CodePage;
		public bool IsBrowserDisplay;
		public bool IsBrowserSave;
		public bool IsMailNewsDisplay;
		public bool IsMailNewsSave;
		public byte SuperscriptFiveReplacementChar;
		public byte InfinityReplacementChar;
		public byte FFReplacementChar;
		public char A0Char;
		public char? OneChar;
		public char A8Char;
		public int? CharsWritten;
		public byte? SupplementChar;
		public bool SkipGetBytes7Test;
		public bool SkipEncoderFallbackTest;
		public bool SkipEncoderFallback2Test;
		public override string ToString () => "Codepage " + Convert.ToString (CodePage);
	}

	//
	// NOTE: when adding/updating tests here consider updating
	//       the following files as well since they have similar tests:
	//
	// - mcs/class/corlib/Test/System.Text/ASCIIEncodingTest.cs
	// - mcs/class/corlib/Test/System.Text/Latin1EncodingTest.cs
	//
	public abstract class EncodingTestBase
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
		[TestCaseSource ("codepageTestInfos")]
		public void IsBrowserDisplay (CodePageTestInfo cpInfo)
		{
			Assert.AreEqual (cpInfo.IsBrowserDisplay, Encoding.GetEncoding (cpInfo.CodePage).IsBrowserDisplay);
		}

		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void IsBrowserSave (CodePageTestInfo cpInfo)
		{
			Assert.AreEqual (cpInfo.IsBrowserSave, Encoding.GetEncoding (cpInfo.CodePage).IsBrowserSave);
		}

		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void IsMailNewsDisplay (CodePageTestInfo cpInfo)
		{
			Assert.AreEqual (cpInfo.IsMailNewsDisplay, Encoding.GetEncoding (cpInfo.CodePage).IsMailNewsDisplay);
		}

		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void IsMailNewsSave (CodePageTestInfo cpInfo)
		{
			Assert.AreEqual (cpInfo.IsMailNewsSave, Encoding.GetEncoding (cpInfo.CodePage).IsMailNewsSave);
		}

		[Test] // Test GetBytes(char[])
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes1 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = test_encoding.GetBytes(testchars);
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars[i], (char) bytes[i]);
		}

		[Test] // Test GetBytes(char[], int, int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes2 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = test_encoding.GetBytes(testchars, 1, 1);
			Assert.AreEqual (1, bytes.Length, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test non-Latin1 char in char[]
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes3 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			testchars[2] = (char) 0x100;
			byte[] bytes = test_encoding.GetBytes(testchars);
			Assert.AreEqual ('T', (char) bytes [0], "#1");
			Assert.AreEqual ('e', (char) bytes [1], "#2");
			Assert.AreEqual (cpInfo.FFReplacementChar, (char) bytes [2], "#3");
			Assert.AreEqual ('t', (char) bytes [3], "#4");
		}

		[Test] // Test GetBytes(char[], int, int, byte[], int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes4 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = new Byte[1];
			int cnt = test_encoding.GetBytes(testchars, 1, 1, bytes, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test] // Test GetBytes(string, int, int, byte[], int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes5 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = new Byte[1];
			int cnt = test_encoding.GetBytes("Test", 1, 1, bytes, 0);
			Assert.AreEqual ('e', (char) bytes [0], "#1");
		}

		[Test] // Test GetBytes(string)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes6 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = test_encoding.GetBytes("Test");
			for (int i = 0; i < testchars.Length; i++)
				Assert.AreEqual (testchars [i], (char) bytes [i]);
		}

		[Test] // Test GetBytes(string)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetBytes7 (CodePageTestInfo cpInfo)
		{
			if (cpInfo.SkipGetBytes7Test)
				Assert.Ignore ("Codepage indicates this test should be skipped.");

			var test_encoding = Encoding.GetEncoding (cpInfo.CodePage);

			var expected = new byte [] { 0x3F, 0x20, cpInfo.SuperscriptFiveReplacementChar, 0x20, cpInfo.InfinityReplacementChar };
			if (cpInfo.SupplementChar.HasValue) {
				var expectedNew = new byte [expected.Length + 1];
				expected.CopyTo (expectedNew, 0);
				expectedNew [expected.Length] = cpInfo.SupplementChar.Value;
				expected = expectedNew;
			}
			var actual = test_encoding.GetBytes("\u24c8 \u2075 \u221e"); // normal replacement
			Assert.AreEqual (expected, actual, "#1");

			expected = new byte [] { 0x3F, 0x3F };
			actual = test_encoding.GetBytes("\ud83d\ude0a"); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#2");

			expected = new byte [] { 0x3F, 0x3F, 0x20 };
			actual = test_encoding.GetBytes("\ud83d\ude0a "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#3");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x3F, 0x20, 0x20 };
			actual = test_encoding.GetBytes("  \ud83d\ude0a  "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#4");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x3F, 0x20, 0x20 };
			actual = test_encoding.GetBytes("  \ud834\udd1e  "); // surrogate pair replacement
			Assert.AreEqual (expected, actual, "#5");

			expected = new byte [] { 0x41, 0x42, 0x43, 0x00, 0x41, 0x42, 0x43 };
			actual = test_encoding.GetBytes("ABC\0ABC"); // embedded zero byte not replaced
			Assert.AreEqual (expected, actual, "#6");

			expected = new byte [] { 0x20, 0x20, 0x3F, 0x20, 0x20 };
			actual = test_encoding.GetBytes("  \ud834  "); // invalid surrogate pair replacement
			Assert.AreEqual (expected, actual, "#7");
		}

		[Test] // Test GetChars(byte[])
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetChars1 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			char[] chars = test_encoding.GetChars(testbytes);
			for (int i = 0; i < testbytes.Length; i++)
				Assert.AreEqual (testbytes[i], (byte) chars[i]);
		}

		[Test] // Test GetChars(byte[], int, int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetChars2 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			char[] chars = test_encoding.GetChars(testbytes, 1, 1);
			Assert.AreEqual (1, chars.Length, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test GetChars(byte[], int, int, char[], int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetChars4 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			char[] chars = new char[1];
			int cnt = test_encoding.GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test GetString(char[])
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetString1 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			string str = test_encoding.GetString(testbytes);
			Assert.AreEqual ("Test", str);
		}

		[Test] // Test GetString(char[], int, int)
		[TestCaseSource ("codepageTestInfos")]
		public void TestGetString2 (CodePageTestInfo cpInfo) 
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			string str = test_encoding.GetString(testbytes, 1, 2);
			Assert.AreEqual ("es", str);
		}

		[Test] // Test Decoder
		[TestCaseSource ("codepageTestInfos")]
		public void TestDecoder (CodePageTestInfo cpInfo)
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			char[] chars = new char[1];
			int cnt = test_encoding.GetDecoder().GetChars(testbytes, 1, 1, chars, 0);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testbytes [1], (byte) chars [0], "#2");
		}

		[Test] // Test Decoder
		[TestCaseSource ("codepageTestInfos")]
		public void TestEncoder (CodePageTestInfo cpInfo)
		{
			Encoding test_encoding = Encoding.GetEncoding (cpInfo.CodePage);
			byte[] bytes = new Byte[1];
			int cnt = test_encoding.GetEncoder().GetBytes(testchars, 1, 1, bytes, 0, false);
			Assert.AreEqual (1, cnt, "#1");
			Assert.AreEqual (testchars [1], (char) bytes [0], "#2");
		}

		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void TestZero (CodePageTestInfo cpInfo)
		{
			Encoding encoding = Encoding.GetEncoding (cpInfo.CodePage);
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0]), "#1");
			Assert.AreEqual (string.Empty, encoding.GetString (new byte [0], 0, 0), "#2");
		}

		[Test]
		[ExpectedException (typeof (EncoderFallbackException))]
		[TestCaseSource ("codepageTestInfos")]
		public void EncoderFallback (CodePageTestInfo cpInfo)
		{
			if (cpInfo.SkipEncoderFallbackTest)
				Assert.Ignore ("Codepage indicates this test should be skipped.");

			Encoding e = Encoding.GetEncoding (cpInfo.CodePage).Clone () as Encoding;
			e.EncoderFallback = new EncoderExceptionFallback ();
			e.GetBytes ("\u24c8");
		}

		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void EncoderFallback2 (CodePageTestInfo cpInfo)
		{
			if (cpInfo.SkipEncoderFallback2Test)
				Assert.Ignore ("Codepage indicates this test should be skipped.");

			Encoding e = Encoding.GetEncoding (cpInfo.CodePage).Clone () as Encoding;
			e.EncoderFallback = new BackslashEncoderReplaceFallback ();

			byte[] bytes = e.GetBytes ("a\xac\u1234\u20ac\u8000");
			var expected = new byte[] { 0x61, 0xAC, 0x5C, 0x75, 0x31, 0x32, 0x33, 0x34, 0x5C, 0x75, 0x32, 0x30, 0x61, 0x63, 0x5C, 0x75, 0x38, 0x30, 0x30, 0x30 };
			// FIXME: some codepages do have U+00AC (logical not sign) or U+20AC (euro sign), we need to adapt the tests			
			// Assert.AreEqual (expected, bytes);

			bytes = e.GetBytes ("1\u04d92");
			expected = new byte[] { 0x31, 0x5C, 0x75, 0x30, 0x34, 0x64, 0x39, 0x32 };
			Assert.AreEqual (expected, bytes);

			e.EncoderFallback = new EncoderExceptionOnWrongIndexFallback ('\u04d9', 1);
			bytes = e.GetBytes ("1\u04d92");
			expected = new byte[] { 0x31, 0x21, 0x32 };
			Assert.AreEqual (expected, bytes);

			e.EncoderFallback = new EncoderExceptionOnWrongIndexFallback ('\u04d9', 0);
			bytes = e.GetBytes ("\u04d921");
			expected = new byte[] { 0x21, 0x32, 0x31 };
			Assert.AreEqual (expected, bytes);
		}

		[Test]
	//	[ExpectedException (typeof (ArgumentException))]
		[TestCaseSource ("codepageTestInfos")]
		public void DecoderFallback2 (CodePageTestInfo cpInfo)
		{
			var bytes = new byte[] {
				0x30, 0xa0, 0x31, 0xa8
			};
			var enc = (Encoding)Encoding.GetEncoding (cpInfo.CodePage).Clone ();
			enc.DecoderFallback = new TestFallbackDecoder ();
			
			var chars = new char [7];
			var ret = enc.GetChars (bytes, 0, bytes.Length, chars, 0);
		}
		
		[Test]
		[TestCaseSource ("codepageTestInfos")]
		public void DecoderFallback3 (CodePageTestInfo cpInfo)
		{
			var bytes = new byte[] {
				0x30, 0xa0, 0x31, 0xa8
			};
			var enc = (Encoding)Encoding.GetEncoding (cpInfo.CodePage).Clone ();
			enc.DecoderFallback = new TestFallbackDecoder ();
			
			var chars = new char[] { '9', '8', '7', '6', '5' };
			var ret = enc.GetChars (bytes, 0, bytes.Length, chars, 0);
			
			Assert.That (ret, Is.EqualTo (cpInfo.CharsWritten ?? 4), "ret");
			Assert.That (chars [0], Is.EqualTo ('0'), "chars[0]");
			Assert.That (chars [1], Is.EqualTo (cpInfo.A0Char), "chars[1]");
			Assert.That (chars [2], Is.EqualTo ((cpInfo).OneChar ?? '1'), "chars[2]");
			Assert.That (chars [3], Is.EqualTo ((char)cpInfo.A8Char), "chars[3]");
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

		class BackslashEncoderReplaceFallback : EncoderFallback
		{
			class BackslashReplaceFallbackBuffer : EncoderFallbackBuffer
			{
				List<char> _buffer = new List<char> ();
				int _index;

				public override bool Fallback (char charUnknownHigh, char charUnknownLow, int index)
				{
					throw new NotImplementedException ();
					return false;
				}

				public override bool Fallback (char charUnknown, int index)
				{
					_buffer.Add('\\');
					int val = (int)charUnknown;
					if (val > 0xFF) {
						_buffer.Add ('u');
						AddCharacter (val >> 8);
						AddCharacter (val & 0xFF);
					} else {
						_buffer.Add ('x');
						AddCharacter (charUnknown);
					}
					return true;
				}

				private void AddCharacter (int val)
				{
					AddOneDigit (((val) & 0xF0) >> 4);
					AddOneDigit (val & 0x0F);
				}

				private void AddOneDigit (int val)
				{
					if (val > 9) {
						_buffer.Add ((char)('a' + val - 0x0A));
					} else {
						_buffer.Add ((char)('0' + val));
					}
				}

				public override char GetNextChar ()
				{
					if (_index == _buffer.Count)
						return Char.MinValue;

					return _buffer[_index++];
				}

				public override bool MovePrevious ()
				{
					if (_index > 0){
						_index--;
						return true;
					}
					return false;
				}

				public override int Remaining
				{
					get { return _buffer.Count - _index; }
				}
			}

			public override EncoderFallbackBuffer CreateFallbackBuffer ()
			{
				return new BackslashReplaceFallbackBuffer ();
			}

			public override int MaxCharCount
			{
				get { throw new NotImplementedException (); }
			}
		}

		class EncoderExceptionOnWrongIndexFallback : EncoderFallback
		{
			char _expectedCharUnknown;
			int _expectedIndex;

			public EncoderExceptionOnWrongIndexFallback (char expectedCharUnknown, int expectedIndex)
			{
				_expectedCharUnknown = expectedCharUnknown;
				_expectedIndex = expectedIndex;
			}

			public override EncoderFallbackBuffer CreateFallbackBuffer ()
			{
				return new EncoderExceptionOnWrongIndexFallbackBuffer (_expectedCharUnknown, _expectedIndex);
			}

			public override int MaxCharCount => 1;

			class EncoderExceptionOnWrongIndexFallbackBuffer : EncoderFallbackBuffer
			{
				char _expectedCharUnknown;
				int _expectedIndex;
				bool read;

				public EncoderExceptionOnWrongIndexFallbackBuffer (char expectedCharUnknown, int expectedIndex)
				{
					_expectedCharUnknown = expectedCharUnknown;
					_expectedIndex = expectedIndex;
				}

				public override int Remaining => read ? 0 : 1;

				public override bool Fallback (char charUnknown, int index)
				{
					Assert.AreEqual (_expectedCharUnknown, charUnknown);
					Assert.AreEqual (_expectedIndex, index);
					return true;
				}

				public override bool Fallback (char charUnknownHigh, char charUnknownLow, int index)
				{
					throw new NotImplementedException ();
					return true;
				}

				public override char GetNextChar ()
				{
					if (!read) {
						read = true;
						return '!';
					}
					return '\0';
				}

				public override bool MovePrevious ()
				{
					return false;
				}
			}
		}
	}
}