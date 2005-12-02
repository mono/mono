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
			AssertEncode ("Test/texts/box-utf8.txt", "Test/texts/box-437.txt", 437);
		}

		[Test]
		public void CP437_Decode ()
		{
			AssertDecode ("Test/texts/box-utf8.txt", "Test/texts/box-437.txt", 437);
		}

		[Test]
		public void CP850_Encode ()
		{
			AssertEncode ("Test/texts/latin-utf8.txt", "Test/texts/latin-850.txt", 850);
		}

		[Test]
		public void CP850_Decode ()
		{
			AssertDecode ("Test/texts/latin-utf8.txt", "Test/texts/latin-850.txt", 850);
		}

		[Test]
		public void CP860_Encode ()
		{
			AssertEncode ("Test/texts/portguese-utf8.txt", "Test/texts/portguese-860.txt", 860);
		}

		[Test]
		public void CP860_Decode ()
		{
			AssertDecode ("Test/texts/portguese-utf8.txt", "Test/texts/portguese-860.txt", 860);
		}

		[Test]
		public void CP861_Encode ()
		{
			AssertEncode ("Test/texts/icelandic2-utf8.txt", "Test/texts/icelandic2-861.txt", 861);
		}

		[Test]
		public void CP861_Decode ()
		{
			AssertDecode ("Test/texts/icelandic2-utf8.txt", "Test/texts/icelandic2-861.txt", 861);
		}

		[Test]
		public void CP863_Encode ()
		{
			AssertEncode ("Test/texts/french2-utf8.txt", "Test/texts/french2-863.txt", 863);
		}

		[Test]
		public void CP863_Decode ()
		{
			AssertDecode ("Test/texts/french2-utf8.txt", "Test/texts/french2-863.txt", 863);
		}

		[Test]
		public void CP865_Encode ()
		{
			AssertEncode ("Test/texts/nordic-utf8.txt", "Test/texts/nordic-865.txt", 865);
		}

		[Test]
		public void CP865_Decode ()
		{
			AssertDecode ("Test/texts/nordic-utf8.txt", "Test/texts/nordic-865.txt", 865);
		}

		[Test]
		public void CP1250_Encode ()
		{
			AssertEncode ("Test/texts/polish-utf8.txt", "Test/texts/polish-1250.txt", 1250);
		}

		[Test]
		public void CP1250_Decode ()
		{
			AssertDecode ("Test/texts/polish-utf8.txt", "Test/texts/polish-1250.txt", 1250);
		}

		[Test]
		public void CP1252_Encode ()
		{
			AssertEncode ("Test/texts/norwegian-utf8.txt", "Test/texts/norwegian-1252.txt", 1252);
		}

		[Test]
		public void CP1252_Decode ()
		{
			AssertDecode ("Test/texts/norwegian-utf8.txt", "Test/texts/norwegian-1252.txt", 1252);
		}

		[Test]
		public void CP1253_Encode ()
		{
			AssertEncode ("Test/texts/greek-utf8.txt", "Test/texts/greek-1253.txt", 1253);
		}

		[Test]
		public void CP1253_Decode ()
		{
			AssertDecode ("Test/texts/greek-utf8.txt", "Test/texts/greek-1253.txt", 1253);
		}

		[Test]
		public void CP10000_Encode ()
		{
			AssertEncode ("Test/texts/french-utf8.txt", "Test/texts/french-10000.txt", 10000);
		}

		[Test]
		public void CP10000_Decode ()
		{
			AssertDecode ("Test/texts/french-utf8.txt", "Test/texts/french-10000.txt", 10000);
		}

		[Test]
		public void CP10079_Encode ()
		{
			AssertEncode ("Test/texts/icelandic-utf8.txt", "Test/texts/icelandic-10079.txt", 10079);
		}

		[Test]
		public void CP10079_Decode ()
		{
			AssertDecode ("Test/texts/icelandic-utf8.txt", "Test/texts/icelandic-10079.txt", 10079);
		}

		[Test]
		public void CP28592_Encode ()
		{
			AssertEncode ("Test/texts/hungarian-utf8.txt", "Test/texts/hungarian-28592.txt", 28592);
		}

		[Test]
		public void CP28592_Decode ()
		{
			AssertDecode ("Test/texts/hungarian-utf8.txt", "Test/texts/hungarian-28592.txt", 28592);
		}

		// FIXME: Which language is good enough to test 28593 ???

		[Test]
		public void CP28597_Encode ()
		{
			AssertEncode ("Test/texts/greek-utf8.txt", "Test/texts/greek-28597.txt", 28597);
		}

		[Test]
		public void CP28597_Decode ()
		{
			AssertDecode ("Test/texts/greek-utf8.txt", "Test/texts/greek-28597.txt", 28597);
		}

		// FIXME: Which language is good enough to test 28605 ???
		[Test]
		public void CP28605_Encode ()
		{
			AssertEncode ("Test/texts/latin-utf8.txt", "Test/texts/latin-28605.txt", 28605);
		}

		[Test]
		public void CP28605_Decode ()
		{
			AssertDecode ("Test/texts/latin-utf8.txt", "Test/texts/latin-28605.txt", 28605);
		}
	}
}
