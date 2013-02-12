//
// I18N.MidEast.Test.cs
//
// Author:
//	Mikko Korkalo    <mikko@korkalo.fi>
//      Based on I18N.CJK.Test.cs by Atsushi Enomoto
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright (C) 2013 Mikko Korkalo
//

using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.I18N.MidEast
{
	[TestFixture]
	public class TestMidEast
	{
		private global::I18N.Common.Manager Manager = global::I18N.Common.Manager.PrimaryManager;

		//MidEast codepages
		int[] cps = { 1254, 28599, 1254, 1255, 28598, 28596, 38598, 1256 };

		void AssertEncode (string utf8file, string decfile, int codepage)
		{
			AssertEncode(utf8file, decfile, codepage, null);
		}
		void AssertEncode (string utf8file, string decfile, int codepage,
					string fallbackString)
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
			//Manager does not support EncoderFallback, and changing it afterwards seems not possible.
			//Encoding enc = Manager.GetEncoding(codepage);
			Encoding enc;
			if (fallbackString != null) {
#if NET_2_0
				enc = Encoding.GetEncoding(codepage,
					 new System.Text.EncoderReplacementFallback(fallbackString),
					 new System.Text.DecoderReplacementFallback("irrelevant")
					);
#else
				throw new InvalidOperationException("DotNet < 2.0 doesn't support encoder fallback");
#endif
			} else {
				enc = Encoding.GetEncoding(codepage);
			}
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
			Encoding enc = Manager.GetEncoding (codepage);
			char [] actual;

			Assert.AreEqual (decoded.Length,
				enc.GetCharCount (encoded, 0, encoded.Length),
				"GetCharCount(byte[], 0, len)");
			actual = enc.GetChars (encoded, 0, encoded.Length);
			Assert.AreEqual (decoded.ToCharArray (), actual,
				"GetChars(byte[], 0, len)");
		}

		public void HandleFallback_Encode_Generic(int cp)
		{
			AssertEncode ("Test/texts/encoder-handlefallback-generic-utf8.txt", "Test/texts/encoder-handlefallback-generic-output.txt", cp, "error_foobar");
		}
		
		[Test]
		public void HandleFallback_Encode_All()
		{
			foreach (int cp in cps)
				HandleFallback_Encode_Generic(cp);
		}
		[Test]
		public void Ascii_Test_All()
		{
			foreach (int cp in cps)
				AssertEncode("Test/texts/ascii-test.txt", "Test/texts/ascii-test.txt", cp);
		}

		[Test]
		public void CP1254_Encode ()
		{
			AssertEncode ("Test/texts/turkish-utf8.txt", "Test/texts/turkish-1254.txt", 1254);
		}
	}
}
