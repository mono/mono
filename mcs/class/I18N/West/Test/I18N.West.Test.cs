//
// I18N.West.Test.cs
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

using MonoTests.Helpers;

namespace MonoTests.I18N.West
{
	[TestFixture]
	public class TestWest
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
			actual = enc.GetBytes (decoded);
			Assert.AreEqual (encoded, actual,
				"GetBytes(string)");

			// simple char[] case
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

			actual = enc.GetChars (encoded, 0, encoded.Length);
			Assert.AreEqual (decoded.ToCharArray (), actual,
				"GetChars(byte[], 0, len)");
		}

		[Test]
		public void CP437_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/box-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/box-437.txt"), 437);
		}

		[Test]
		public void CP437_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/box-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/box-437.txt"), 437);
		}

		[Test]
		public void CP850_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-850.txt"), 850);
		}

		[Test]
		public void CP850_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-850.txt"), 850);
		}

		[Test]
		public void CP860_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/portguese-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/portguese-860.txt"), 860);
		}

		[Test]
		public void CP860_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/portguese-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/portguese-860.txt"), 860);
		}

		[Test]
		public void CP861_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic2-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic2-861.txt"), 861);
		}

		[Test]
		public void CP861_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic2-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic2-861.txt"), 861);
		}

		[Test]
		public void CP863_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/french2-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/french2-863.txt"), 863);
		}

		[Test]
		public void CP863_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/french2-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/french2-863.txt"), 863);
		}

		[Test]
		public void CP865_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/nordic-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/nordic-865.txt"), 865);
		}

		[Test]
		public void CP865_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/nordic-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/nordic-865.txt"), 865);
		}

		[Test]
		public void CP1250_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/polish-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/polish-1250.txt"), 1250);
		}

		[Test]
		public void CP1250_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/polish-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/polish-1250.txt"), 1250);
		}

		[Test]
		public void CP1252_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/norwegian-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/norwegian-1252.txt"), 1252);
		}

		[Test]
		public void CP1252_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/norwegian-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/norwegian-1252.txt"), 1252);
		}

		[Test]
		public void CP1253_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-1253.txt"), 1253);
		}

		[Test]
		public void CP1253_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-1253.txt"), 1253);
		}

		[Test]
		public void CP10000_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/french-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/french-10000.txt"), 10000);
		}

		[Test]
		public void CP10000_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/french-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/french-10000.txt"), 10000);
		}

		[Test]
		public void CP10079_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic-10079.txt"), 10079);
		}

		[Test]
		public void CP10079_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/icelandic-10079.txt"), 10079);
		}

		[Test]
		public void CP28592_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/hungarian-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/hungarian-28592.txt"), 28592);
		}

		[Test]
		public void CP28592_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/hungarian-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/hungarian-28592.txt"), 28592);
		}

		// FIXME: Which language is good enough to test 28593 ???

		[Test]
		public void CP28597_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-28597.txt"), 28597);
		}

		[Test]
		public void CP28597_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/greek-28597.txt"), 28597);
		}

		// FIXME: Which language is good enough to test 28605 ???
		[Test]
		public void CP28605_Encode ()
		{
			AssertEncode (TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-28605.txt"), 28605);
		}

		[Test]
		public void CP28605_Decode ()
		{
			AssertDecode (TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-utf8.txt"), TestResourceHelper.GetFullPathOfResource ("Test/texts/latin-28605.txt"), 28605);
		}

		[Test]
		public void Bug79951 ()
		{
			byte [] expected = new byte [] {0x71, 0x77, 0x65, 0x65, 0x72, 0x74, 0x79, 0x75, 0x69, 0x6F, 0xA2, 0x70, 0x61, 0x61, 0x73, 0x73, 0x64, 0x66, 0x67, 0x68, 0x6A, 0x6B, 0x6C, 0x6C, 0x7A, 0x7A, 0x78, 0x7A, 0x63, 0x63, 0x76, 0x62, 0x6E, 0x6E, 0x6D};
			string l = "qwe\u0119rtyuio\xF3pa\u0105s\u015Bdfghjkl\u0142z\u017Cx\u017Ac\u0107vbn\u0144m";
			Encoding e437 = Encoding.GetEncoding (437);
			byte [] actual = e437.GetBytes (l);
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void Bug408381 ()
		{
			byte [] expected = new byte [] {0x27, 0x3F, 0x27};
			byte [] utf8bytes = new byte [] {0x27, 0xC2, 0x92, 0x27};
			string strToEncode = Encoding.UTF8.GetString(utf8bytes);
			Encoding cp1252 = Encoding.GetEncoding (1252);
			byte [] actual = cp1252.GetBytes (strToEncode);
			Assert.AreEqual (expected, actual);
		}
	}
}
