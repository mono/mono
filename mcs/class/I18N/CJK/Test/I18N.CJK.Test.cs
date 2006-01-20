//
// I18N.CJK.Test.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.I18N.CJK
{
	[TestFixture]
	public class TestCJK
	{
		void AssertEncode (string utf8file, string decfile, int codepage)
		{
			string decoded = null;
			byte [] encoded = null;
			using (StreamReader sr = new StreamReader (utf8file,
				Encoding.UTF8)) {
				decoded = sr.ReadToEnd ();
			}
			using (FileStream fs = File.OpenRead (decfile)) {
				encoded = new byte [fs.Length];
				fs.Read (encoded, 0, (int) fs.Length);
			}
			Encoding enc = Encoding.GetEncoding (codepage);
			byte [] actual;

			// simple string case
			//Assert.AreEqual (encoded.Length,
			//	enc.GetByteCount (decoded),
			//	"GetByteCount(string)");
			actual = enc.GetBytes (decoded);
			Assert.AreEqual (encoded, actual,
				"GetBytes(string)");

			// simple char[] case
			Assert.AreEqual (encoded.Length,
				enc.GetByteCount (decoded.ToCharArray (), 0, decoded.Length),
				"GetByteCount(char[], 0, len)");
			actual = enc.GetBytes (decoded.ToCharArray (), 0, decoded.Length);
			Assert.AreEqual (encoded, actual,
				"GetBytes(char[], 0, len)");
		}

		void AssertDecode (string utf8file, string decfile, int codepage)
		{
			string decoded = null;
			byte [] encoded = null;
			using (StreamReader sr = new StreamReader (utf8file,
				Encoding.UTF8)) {
				decoded = sr.ReadToEnd ();
			}
			using (FileStream fs = File.OpenRead (decfile)) {
				encoded = new byte [fs.Length];
				fs.Read (encoded, 0, (int) fs.Length);
			}
			Encoding enc = Encoding.GetEncoding (codepage);
			char [] actual;

			Assert.AreEqual (decoded.Length,
				enc.GetCharCount (encoded, 0, encoded.Length),
				"GetCharCount(byte[], 0, len)");
			actual = enc.GetChars (encoded, 0, encoded.Length);
			Assert.AreEqual (decoded.ToCharArray (), actual,
				"GetChars(byte[], 0, len)");
		}

		#region Chinese

		// GB2312

		[Test]
		public void CP936_Encode ()
		{
			AssertEncode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-936.txt", 936);
		}

		[Test]
		public void CP936_Decode ()
		{
			AssertDecode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-936.txt", 936);
		}

		// BIG5

		[Test]
		public void CP950_Encode ()
		{
			AssertEncode ("Test/texts/chinese2-utf8.txt", "Test/texts/chinese2-950.txt", 950);
		}

		[Test]
		public void CP950_Decode ()
		{
			AssertDecode ("Test/texts/chinese2-utf8.txt", "Test/texts/chinese2-950.txt", 950);
		}

		// GB18030

		[Test]
		public void CP54936_Encode ()
		{
			AssertEncode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-54936.txt", 54936);
		}

		[Test]
		public void CP54936_Decode ()
		{
			AssertDecode ("Test/texts/chinese-utf8.txt", "Test/texts/chinese-54936.txt", 54936);
		}

		#endregion

		#region Japanese

		// Shift_JIS

		[Test]
		public void CP932_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-932.txt", 932);
		}

		[Test]
		public void CP932_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-932.txt", 932);
		}

		// EUC-JP

		[Test]
		public void CP51932_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-51932.txt", 51932);
		}

		[Test]
		public void CP51932_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-51932.txt", 51932);
		}

		// ISO-2022-JP

		[Test]
		public void CP50220_Encode ()
		{
			AssertEncode ("Test/texts/japanese2-utf8.txt", "Test/texts/japanese2-50220.txt", 50220);
		}

		[Test]
		public void CP50220_Decode ()
		{
			AssertDecode ("Test/texts/japanese2-utf8.txt", "Test/texts/japanese2-50220.txt", 50220);
		}

		[Test]
		public void CP50221_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50221.txt", 50221);
		}

		[Test]
		public void CP50221_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50221.txt", 50221);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS is buggy here
#endif
		public void CP50222_Encode ()
		{
			AssertEncode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50222.txt", 50222);
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // MS is buggy here
#endif
		public void CP50222_Decode ()
		{
			AssertDecode ("Test/texts/japanese-utf8.txt", "Test/texts/japanese-50222.txt", 50222);
		}

		[Test]
		public void Bug77723 ()
		{
			Encoding e = Encoding.GetEncoding (51932);
			for (int i = 0; i < 0x10000; i++)
				e.GetBytes (new char [] { (char)i });
		}

		[Test]
		public void Bug77224 ()
		{
			Encoding e = Encoding.GetEncoding (932);
			for (int i = 0; i < 0x10000; i++)
				e.GetBytes (new char [] {(char) i});
		}

		GetCharsAllBytePairs (int enc)
		{
			Encoding e = Encoding.GetEncoding (enc);
			byte [] bytes = new byte [2];
			for (int i0 = 0; i0 < 0x100; i0++) {
				bytes [0] = (byte) i0;
				for (int i1 = 0; i1 < 0x100; i1++) {
					bytes [1] = (byte) i1;
					e.GetChars (bytes);
				}
			}
		}

		[Test]
		public void Bug77222 ()
		{
			GetCharsAllBytePairs (51932);
		}

		[Test]
		public void Bug77238 ()
		{
			GetCharsAllBytePairs (936);
		}

		[Test]
		public void Bug7774 ()
		{
			GetCharsAllBytePairs (950);
		}

		#endregion

		#region Korean

		[Test]
		public void CP949_Encode ()
		{
			AssertEncode ("Test/texts/korean-utf8.txt", "Test/texts/korean-949.txt", 949);
		}

		[Test]
		public void CP949_Decode ()
		{
			AssertDecode ("Test/texts/korean-utf8.txt", "Test/texts/korean-949.txt", 949);
		}

		#endregion
	}
}
