//
// UTF8EncodingTest.cs - NUnit Test Cases for System.Text.UTF8Encoding
//
// Authors:
//	Patrick Kalkman  kalkman@cistron.nl
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Patrick Kalkman
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Text;

#if NET_2_0
using DecoderException = System.Text.DecoderFallbackException;
#else
using DecoderException = System.ArgumentException;
#endif

using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class UTF8EncodingTest 
	{
		private UTF8Encoding utf8;

		[SetUp]
		public void Create () 
		{
			utf8 = new UTF8Encoding (true, true);
		}

		[Test]
		public void IsBrowserDisplay ()
		{
			Assert.IsTrue (utf8.IsBrowserDisplay);
		}

		[Test]
		public void IsBrowserSave ()
		{
			Assert.IsTrue (utf8.IsBrowserSave);
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			Assert.IsTrue (utf8.IsMailNewsDisplay);
		}

		[Test]
		public void IsMailNewsSave ()
		{
			Assert.IsTrue (utf8.IsMailNewsSave);
		}

		[Test]
		public void TestCompat ()
		{
			Assert.IsTrue (new UTF8Encoding ().Equals (new UTF8Encoding ()));
		}
		
		[Test]
		public void TestEncodingGetBytes1()
		{
			UTF8Encoding utf8Enc = new UTF8Encoding ();
			string UniCode = "\u0041\u2262\u0391\u002E";

			// "A<NOT IDENTICAL TO><ALPHA>." may be encoded as 41 E2 89 A2 CE 91 2E 
			// see (RFC 2044)
			byte[] utf8Bytes = utf8Enc.GetBytes (UniCode);

			Assert.AreEqual (0x41, utf8Bytes [0], "UTF #1");
			Assert.AreEqual (0xE2, utf8Bytes [1], "UTF #2");
			Assert.AreEqual (0x89, utf8Bytes [2], "UTF #3");
			Assert.AreEqual (0xA2, utf8Bytes [3], "UTF #4");
			Assert.AreEqual (0xCE, utf8Bytes [4], "UTF #5");
			Assert.AreEqual (0x91, utf8Bytes [5], "UTF #6");
			Assert.AreEqual (0x2E, utf8Bytes [6], "UTF #7");
		}

		[Test]
		public void TestEncodingGetBytes2()
		{
			UTF8Encoding utf8Enc = new UTF8Encoding ();
			string UniCode = "\u0048\u0069\u0020\u004D\u006F\u006D\u0020\u263A\u0021";

			// "Hi Mom <WHITE SMILING FACE>!" may be encoded as 48 69 20 4D 6F 6D 20 E2 98 BA 21 
			// see (RFC 2044)
			byte[] utf8Bytes = new byte [11];

			int ByteCnt = utf8Enc.GetBytes (UniCode.ToCharArray(), 0, UniCode.Length, utf8Bytes, 0);
			Assert.AreEqual (11, ByteCnt, "UTF #1");
			Assert.AreEqual (0x48, utf8Bytes [0], "UTF #2");
			Assert.AreEqual (0x69, utf8Bytes [1], "UTF #3");
			Assert.AreEqual (0x20, utf8Bytes [2], "UTF #4");
			Assert.AreEqual (0x4D, utf8Bytes [3], "UTF #5");
			Assert.AreEqual (0x6F, utf8Bytes [4], "UTF #6");
			Assert.AreEqual (0x6D, utf8Bytes [5], "UTF #7");
			Assert.AreEqual (0x20, utf8Bytes [6], "UTF #8");
			Assert.AreEqual (0xE2, utf8Bytes [7], "UTF #9");
			Assert.AreEqual (0x98, utf8Bytes [8], "UTF #10");
			Assert.AreEqual (0xBA, utf8Bytes [9], "UTF #11");
			Assert.AreEqual (0x21, utf8Bytes [10], "UTF #12");
		}

		[Test]
		public void TestDecodingGetChars1()
		{
			UTF8Encoding utf8Enc = new UTF8Encoding ();
			// 41 E2 89 A2 CE 91 2E may be decoded as "A<NOT IDENTICAL TO><ALPHA>." 
			// see (RFC 2044)
			byte[] utf8Bytes = new byte [] {0x41, 0xE2, 0x89, 0xA2, 0xCE, 0x91, 0x2E};
			char[] UniCodeChars = utf8Enc.GetChars(utf8Bytes);

			Assert.AreEqual (0x0041, UniCodeChars [0], "UTF #1");
			Assert.AreEqual (0x2262, UniCodeChars [1], "UTF #2");
			Assert.AreEqual (0x0391, UniCodeChars [2], "UTF #3");
			Assert.AreEqual (0x002E, UniCodeChars [3], "UTF #4");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void TestMaxCharCount()
		{
			UTF8Encoding UTF8enc = new UTF8Encoding ();
#if NET_2_0
			// hmm, where is this extra 1 coming from?
			Assert.AreEqual (51, UTF8enc.GetMaxCharCount(50), "UTF #1");
#else
			Assert.AreEqual (50, UTF8enc.GetMaxCharCount(50), "UTF #1");
#endif
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void TestMaxByteCount()
		{
			UTF8Encoding UTF8enc = new UTF8Encoding ();
#if NET_2_0
			// maybe under .NET 2.0 insufficient surrogate pair is
			// just not handled, and 3 is Preamble size.
			Assert.AreEqual (153, UTF8enc.GetMaxByteCount(50), "UTF #1");
#else
			Assert.AreEqual (200, UTF8enc.GetMaxByteCount(50), "UTF #1");
#endif
		}

		// regression for bug #59648
		[Test]
		public void TestThrowOnInvalid ()
		{
			UTF8Encoding u = new UTF8Encoding (true, false);

			byte[] data = new byte [] { 0xC0, 0xAF };
#if NET_2_0
			Assert.AreEqual (2, u.GetCharCount (data), "#A0");
			string s = u.GetString (data);
			Assert.AreEqual ("\uFFFD\uFFFD", s, "#A1");
#else
			Assert.AreEqual (0, u.GetCharCount (data), "#A0");
			string s = u.GetString (data);
			Assert.AreEqual (String.Empty, s, "#A1");
#endif

			data = new byte [] { 0x30, 0x31, 0xC0, 0xAF, 0x30, 0x32 };
			s = u.GetString (data);
#if NET_2_0
			Assert.AreEqual (6, s.Length, "#B1");
			Assert.AreEqual (0x30, (int) s [0], "#B2");
			Assert.AreEqual (0x31, (int) s [1], "#B3");
			Assert.AreEqual (0xFFFD, (int) s [2], "#B4");
			Assert.AreEqual (0xFFFD, (int) s [3], "#B5");
			Assert.AreEqual (0x30, (int) s [4], "#B6");
			Assert.AreEqual (0x32, (int) s [5], "#B7");
#else
			Assert.AreEqual (4, s.Length, "#B1");
			Assert.AreEqual (0x30, (int) s [0], "#B2");
			Assert.AreEqual (0x31, (int) s [1], "#B3");
			Assert.AreEqual (0x30, (int) s [2], "#B4");
			Assert.AreEqual (0x32, (int) s [3], "#B5");
#endif
		}

		// UTF8 decoding tests from http://www.cl.cam.ac.uk/~mgk25/

		[Test]
		public void T1_Correct_GreekWord_kosme () 
		{
			byte[] data = { 0xCE, 0xBA, 0xE1, 0xBD, 0xB9, 0xCF, 0x83, 0xCE, 0xBC, 0xCE, 0xB5 };
			string s = utf8.GetString (data);
			// cute but saving source code in unicode can be problematic
			// so we just ensure we can re-encode this
			Assert.AreEqual (BitConverter.ToString (data), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted");
		}

		[Test]
		public void T2_Boundary_1_FirstPossibleSequence_Pass () 
		{
			byte[] data211 = { 0x00 };
			string s = utf8.GetString (data211);
			Assert.AreEqual ("\0", s, "1 byte  (U-00000000)");
			Assert.AreEqual (BitConverter.ToString (data211), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-1");

			byte[] data212 = { 0xC2, 0x80 };
			s = utf8.GetString (data212);
			Assert.AreEqual (128, s [0], "2 bytes (U-00000080)");
			Assert.AreEqual (BitConverter.ToString (data212), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-2");

			byte[] data213 = { 0xE0, 0xA0, 0x80 };
			s = utf8.GetString (data213);
			Assert.AreEqual (2048, s [0], "3 bytes (U-00000800)");
			Assert.AreEqual (BitConverter.ToString (data213), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-3");

			byte[] data214 = { 0xF0, 0x90, 0x80, 0x80 };
			s = utf8.GetString (data214);
			Assert.AreEqual (55296, s [0], "4 bytes (U-00010000)-0");
			Assert.AreEqual (56320, s [1], "4 bytes (U-00010000)-1");
			Assert.AreEqual (BitConverter.ToString (data214), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-4");
		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_1_FirstPossibleSequence_Fail_5 () 
		{
			byte[] data215 = { 0xF8, 0x88, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data215);
			Assert.IsNull (s, "5 bytes (U-00200000)");
			Assert.AreEqual (BitConverter.ToString (data215), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-5");
		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_1_FirstPossibleSequence_Fail_6 () 
		{
			byte[] data216 = { 0xFC, 0x84, 0x80, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data216);
			Assert.IsNull (s, "6 bytes (U-04000000)");
			Assert.AreEqual (BitConverter.ToString (data216), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-6");
		}

		[Test]
		public void T2_Boundary_2_LastPossibleSequence_Pass () 
		{
			byte[] data221 = { 0x7F };
			string s = utf8.GetString (data221);
			Assert.AreEqual (127, s [0], "1 byte  (U-0000007F)");
			Assert.AreEqual (BitConverter.ToString (data221), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-1");

			byte[] data222 = { 0xDF, 0xBF };
			s = utf8.GetString (data222);
			Assert.AreEqual (2047, s [0], "2 bytes (U-000007FF)");
			Assert.AreEqual (BitConverter.ToString (data222), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-2");

			byte[] data223 = { 0xEF, 0xBF, 0xBF };
			s = utf8.GetString (data223);
			Assert.AreEqual (65535, s [0], "3 bytes (U-0000FFFF)");
			Assert.AreEqual (BitConverter.ToString (data223), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-3");

		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_2_LastPossibleSequence_Fail_4 () 
		{
			byte[] data224 = { 0x7F, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data224);
			Assert.IsNull (s, "4 bytes (U-001FFFFF)");
			Assert.AreEqual (BitConverter.ToString (data224), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-4");
		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_2_LastPossibleSequence_Fail_5 () 
		{
			byte[] data225 = { 0xFB, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data225);
			Assert.IsNull (s, "5 bytes (U-03FFFFFF)");
			Assert.AreEqual (BitConverter.ToString (data225), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-5");
		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_2_LastPossibleSequence_Fail_6 () 
		{
			byte[] data226 = { 0xFD, 0xBF, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data226);
			Assert.IsNull (s, "6 bytes (U-7FFFFFFF)");
			Assert.AreEqual (BitConverter.ToString (data226), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-6");
		}

		[Test]
		public void T2_Boundary_3_Other_Pass () 
		{
			byte[] data231 = { 0xED, 0x9F, 0xBF };
			string s = utf8.GetString (data231);
			Assert.AreEqual (55295, s [0], "U-0000D7FF");
			Assert.AreEqual (BitConverter.ToString (data231), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-1");

			byte[] data232 = { 0xEE, 0x80, 0x80 };
			s = utf8.GetString (data232);
			Assert.AreEqual (57344, s [0], "U-0000E000");
			Assert.AreEqual (BitConverter.ToString (data232), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-2");

			byte[] data233 = { 0xEF, 0xBF, 0xBD };
			s = utf8.GetString (data233);
			Assert.AreEqual (65533, s [0], "U-0000FFFD");
			Assert.AreEqual (BitConverter.ToString (data233), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-3");

			byte[] data234 = { 0xF4, 0x8F, 0xBF, 0xBF };
			s = utf8.GetString (data234);
			Assert.AreEqual (56319, s [0], "U-0010FFFF-0");
			Assert.AreEqual (57343, s [1], "U-0010FFFF-1");
			Assert.AreEqual (BitConverter.ToString (data234), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-4");
		}

		[Test]
		// Fail on MS Fx 1.1
		[ExpectedException (typeof (DecoderException))]
		public void T2_Boundary_3_Other_Fail_5 () 
		{
			byte[] data235 = { 0xF4, 0x90, 0x80, 0x80 };
			string s = utf8.GetString (data235);
			Assert.IsNull (s, "U-00110000");
			Assert.AreEqual (BitConverter.ToString (data235), BitConverter.ToString (utf8.GetBytes (s)), "Reconverted-5");
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_311 () 
		{
			byte[] data = { 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_312 () 
		{
			byte[] data = { 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_313 () 
		{
			byte[] data = { 0x80, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_314 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_315 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_316 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_317 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_318 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80, 0xBF, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_1_UnexpectedContinuation_319 () 
		{
			// 64 different continuation characters
			byte[] data = {
				0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 
				0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F, 
				0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 
				0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_2_LonelyStart_321 ()
		{
			byte[] data = { 
				0xC0, 0x20, 0xC1, 0x20, 0xC2, 0x20, 0xC3, 0x20, 0xC4, 0x20, 0xC5, 0x20, 0xC6, 0x20, 0xC7, 0x20, 
				0xC8, 0x20, 0xC9, 0x20, 0xCA, 0x20, 0xCB, 0x20, 0xCC, 0x20, 0xCD, 0x20, 0xCE, 0x20, 0xCF, 0x20, 
				0xD0, 0x20, 0xD1, 0x20, 0xD2, 0x20, 0xD3, 0x20, 0xD4, 0x20, 0xD5, 0x20, 0xD6, 0x20, 0xD7, 0x20, 
				0xD8, 0x20, 0xD9, 0x20, 0xDA, 0x20, 0xDB, 0x20, 0xDC, 0x20, 0xDD, 0x20, 0xDE, 0x20, 0xDF, 0x20 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_2_LonelyStart_322 () 
		{
			byte[] data = { 
				0xE0, 0x20, 0xE1, 0x20, 0xE2, 0x20, 0xE3, 0x20, 0xE4, 0x20, 0xE5, 0x20, 0xE6, 0x20, 0xE7, 0x20, 
				0xE8, 0x20, 0xE9, 0x20, 0xEA, 0x20, 0xEB, 0x20, 0xEC, 0x20, 0xED, 0x20, 0xEE, 0x20, 0xEF, 0x20 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_2_LonelyStart_323 () 
		{
			byte[] data = { 0xF0, 0x20, 0xF1, 0x20, 0xF2, 0x20, 0xF3, 0x20, 0xF4, 0x20, 0xF5, 0x20, 0xF6, 0x20, 0xF7, 0x20 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_2_LonelyStart_324 () 
		{
			byte[] data = { 0xF0, 0x20, 0xF1, 0x20, 0xF2, 0x20, 0xF3, 0x20, 0xF4, 0x20, 0xF5, 0x20, 0xF6, 0x20, 0xF7, 0x20 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_2_LonelyStart_325 () 
		{
			byte[] data = { 0xFC, 0x20, 0xFD, 0x20 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_331 () 
		{
			byte[] data = { 0xC0 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_332 () 
		{
			byte[] data = { 0xE0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_333 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_334 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_335 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_336 () 
		{
			byte[] data = { 0xDF };
			try {
				string s = utf8.GetString (data);
				// exception is "really" expected here
				Assert.AreEqual (String.Empty, s, "MS FX 1.1 behaviour");
			}
			catch (DecoderException) {
				// but Mono doesn't - better stick to the standard
			}
		}

		[Test]
		// MS Fx 1.1 accept this
//		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_337 () 
		{
			byte[] data = { 0xEF, 0xBF };
			try {
				string s = utf8.GetString (data);
				// exception is "really" expected here
				Assert.AreEqual (String.Empty, s, "MS FX 1.1 behaviour");
			}
			catch (DecoderException) {
				// but Mono doesn't - better stick to the standard
			}
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_338 () 
		{
			byte[] data = { 0xF7, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_339 () 
		{
			byte[] data = { 0xF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_3_LastContinuationMissing_3310 () 
		{
			byte[] data = { 0xFD, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_4_ConcatenationImcomplete () 
		{
			byte[] data = {
				0xC0, 0xE0, 0x80, 0xF0, 0x80, 0x80, 0xF8, 0x80, 0x80, 0x80, 0xFC, 0x80, 0x80, 0x80, 0x80, 0xDF, 
				0xEF, 0xBF, 0xF7, 0xBF, 0xBF, 0xFB, 0xBF, 0xBF, 0xBF, 0xFD, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_5_ImpossibleBytes_351 () 
		{
			byte[] data = { 0xFE };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_5_ImpossibleBytes_352 () 
		{
			byte[] data = { 0xFF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T3_Malformed_5_ImpossibleBytes_353 () 
		{
			byte[] data = { 0xFE, 0xFE, 0xFF, 0xFF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		// Overlong == dangereous -> "safe" decoder should reject them

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_1_ASCII_Slash_411 () 
		{
			byte[] data = { 0xC0, 0xAF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_1_ASCII_Slash_412 () 
		{
			byte[] data = { 0xE0, 0x80, 0xAF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_1_ASCII_Slash_413 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80, 0xAF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_1_ASCII_Slash_414 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80, 0xAF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_1_ASCII_Slash_415 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80, 0xAF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_2_MaximumBoundary_421 () 
		{
			byte[] data = { 0xC1, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_2_MaximumBoundary_422 () 
		{
			byte[] data = { 0xE0, 0x9F, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_2_MaximumBoundary_423 () 
		{
			byte[] data = { 0xF0, 0x8F, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_2_MaximumBoundary_424 () 
		{
			byte[] data = { 0xF8, 0x87, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_2_MaximumBoundary_425 () 
		{
			byte[] data = { 0xFC, 0x83, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_3_NUL_431 () 
		{
			byte[] data = { 0xC0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_3_NUL_432 () 
		{
			byte[] data = { 0xE0, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_3_NUL_433 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_3_NUL_434 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
		[ExpectedException (typeof (DecoderException))]
		public void T4_Overlong_3_NUL_435 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
			public void T5_IllegalCodePosition_1_UTF16Surrogates_511 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (55296, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_512 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56191, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_513 ()
		{
			byte[] data = { 0xED, 0xAE, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56192, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_514 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56319, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_515 ()
		{
			byte[] data = { 0xED, 0xB0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56320, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_516 () 
		{
			byte[] data = { 0xED, 0xBE, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (57216, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_1_UTF16Surrogates_517 () 
		{
			byte[] data = { 0xED, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (57343, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_521 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80, 0xED, 0xB0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (55296, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (56320, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_522 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80, 0xED, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (55296, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (57343, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_523 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF, 0xED, 0xB0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56191, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (56320, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_524 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF, 0xED, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56191, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (57343, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_525 () 
		{
			byte[] data = { 0xED, 0xAE, 0x80, 0xED, 0xB0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56192, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (56320, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_526 () 
		{
			byte[] data = { 0xED, 0xAE, 0x80, 0xED, 0xBF, 0x8F };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56192, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (57295, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_527 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF, 0xED, 0xB0, 0x80 };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56319, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (56320, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
// MS Fx 1.1 accept this
		[Category ("NotDotNet")]
		[ExpectedException (typeof (DecoderException))]
#endif
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_528 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF, 0xED, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (56319, s [0], "MS FX 1.1 behaviour");
			Assert.AreEqual (57343, s [1], "MS FX 1.1 behaviour");
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (DecoderException))]
		public void T5_IllegalCodePosition_3_Other_531 () 
		{
			byte[] data = { 0xEF, 0xBF, 0xBE };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (65534, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (DecoderException))]
		public void T5_IllegalCodePosition_3_Other_532 () 
		{
			byte[] data = { 0xEF, 0xBF, 0xBF };
			string s = utf8.GetString (data);
			// exception is "really" expected here
			Assert.AreEqual (65535, s [0], "MS FX 1.1 behaviour");
		}

		[Test]
		// bug #75065 and #73086.
		public void GetCharsFEFF ()
		{
			byte [] data = new byte [] {0xEF, 0xBB, 0xBF};
			Encoding enc = new UTF8Encoding (false, true);
			string s = enc.GetString (data);
			Assert.AreEqual (s, "\uFEFF");

			Encoding utf = Encoding.UTF8;
			char[] testChars = {'\uFEFF','A'};

			byte[] bytes = utf.GetBytes(testChars);
			char[] chars = utf.GetChars(bytes);
			Assert.AreEqual ('\uFEFF', chars [0], "#1");
			Assert.AreEqual ('A', chars [1], "#2");
		}

#if NET_2_0
		[Test]
		public void CloneNotReadOnly ()
		{
			Encoding e = Encoding.GetEncoding (65001).Clone ()
				as Encoding;
			Assert.AreEqual (false, e.IsReadOnly);
			e.EncoderFallback = new EncoderExceptionFallback ();
		}
#endif

		[Test]
#if NET_2_0
		[ExpectedException (typeof (DecoderFallbackException))]
#else
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")] // MS Bug
#endif
		public void Bug77315 ()
		{
			new UTF8Encoding (false, true).GetString (
				new byte [] {0xED, 0xA2, 0x8C});
		}

		[Test]
		public void SufficientByteArray ()
		{
			Encoder e = Encoding.UTF8.GetEncoder ();
			byte [] bytes = new byte [0];

			char [] chars = new char [] {'\uD800'};
			e.GetBytes (chars, 0, 1, bytes, 0, false);
			try {
				int ret = e.GetBytes (chars, 1, 0, bytes, 0, true);
#if NET_2_0
				Assert.AreEqual (0, ret, "drop insufficient char in 2.0: char[]");
#else
				Assert.Fail ("ArgumentException is expected: char[]");
#endif
			} catch (ArgumentException ae) {
#if ! NET_2_0
				throw ae;
#endif
			}

			string s = "\uD800";
			try {
				int ret = Encoding.UTF8.GetBytes (s, 0, 1, bytes, 0);
#if NET_2_0
				Assert.AreEqual (0, ret, "drop insufficient char in 2.0: string");
#else
				Assert.Fail ("ArgumentException is expected: string");
#endif
			} catch (ArgumentException ae) {
#if ! NET_2_0
				throw ae;
#endif
			}
		}
		
		[Test] // bug #565129
		public void SufficientByteArray2 ()
		{
			var u = Encoding.UTF8;
			Assert.AreEqual (3, u.GetByteCount ("\uFFFD"), "#1-1");
			Assert.AreEqual (3, u.GetByteCount ("\uD800"), "#1-2");
			Assert.AreEqual (3, u.GetByteCount ("\uDC00"), "#1-3");
			Assert.AreEqual (4, u.GetByteCount ("\uD800\uDC00"), "#1-4");
			byte [] bytes = new byte [10];
			Assert.AreEqual (3, u.GetBytes ("\uDC00", 0, 1, bytes, 0), "#1-5"); // was bogus

			Assert.AreEqual (3, u.GetBytes ("\uFFFD").Length, "#2-1");
			Assert.AreEqual (3, u.GetBytes ("\uD800").Length, "#2-2");
			Assert.AreEqual (3, u.GetBytes ("\uDC00").Length, "#2-3");
			Assert.AreEqual (4, u.GetBytes ("\uD800\uDC00").Length, "#2-4");

			for (char c = char.MinValue; c < char.MaxValue; c++) {
				byte [] bIn;
				bIn = u.GetBytes (c.ToString ());
			}

			try {
				new UTF8Encoding (false, true).GetBytes (new char [] {'\uDF45', '\uD808'}, 0, 2);
				Assert.Fail ("EncoderFallbackException is expected");
			} catch (EncoderFallbackException) {
			}
		}

#if NET_2_0
		[Test] // bug #77550
		public void DecoderFallbackSimple ()
		{
			UTF8Encoding e = new UTF8Encoding (false, false);
			AssertType.AreEqual (1, e.GetDecoder ().GetCharCount (
					new byte [] {(byte) 183}, 0, 1),
					"#1");
			AssertType.AreEqual (1, e.GetDecoder().GetChars (
					new byte [] {(byte) 183}, 0, 1,
					new char [100], 0),
					"#2");
			AssertType.AreEqual (1, e.GetString (new byte [] {(byte) 183}).Length,
					"#3");
		}

		[Test]
		public void FallbackDefaultEncodingUTF8 ()
		{
			DecoderReplacementFallbackBuffer b =
				Encoding.UTF8.DecoderFallback.CreateFallbackBuffer ()
				as DecoderReplacementFallbackBuffer;
			AssertType.IsTrue (b.Fallback (new byte [] {}, 0), "#1");
			AssertType.IsFalse (b.MovePrevious (), "#2");
			AssertType.AreEqual (1, b.Remaining, "#3");
			AssertType.AreEqual ('\uFFFD', b.GetNextChar (), "#4");
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Bug415628 ()
		{
			using (var f = File.Open ("Test/resources/415628.bin", FileMode.Open)) {
				BinaryReader br = new BinaryReader (f);
				byte [] buf = br.ReadBytes (8000);
				Encoding.UTF8.GetString(buf);
			}
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Bug10788()
		{
			byte[] bytes = new byte[4096];
			char[] chars = new char[10];

			Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 4096, chars, 9, false);
		}

		[Test]
		public void Bug10789()
		{
			byte[] bytes = new byte[4096];
			char[] chars = new char[10];

			try {
				Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 10, false);
				Assert.Fail ("ArgumentException is expected #1");
			} catch (ArgumentException) {
			}

			try {
				Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 1, chars, 11, false);
				Assert.Fail ("ArgumentOutOfRangeException is expected #2");
			} catch (ArgumentOutOfRangeException) {
			}

			int charactersWritten = Encoding.UTF8.GetDecoder ().GetChars (bytes, 0, 0, chars, 10, false);
			Assert.AreEqual (0, charactersWritten, "#3");
		}

		// DecoderFallbackExceptionTest
		//   This struct describes a DecoderFallbackExceptions' test. It
		//   contains the expected indexes (eindex) and bad-bytes lengths
		//   (elen) delivered by the first and subsequent
		//   DecoderFallbackException throwed when the utf8 conversion routines
		//   are exposed by the array of bytes (bytes) contained in this test.
		//   It also has a nice description (description) for documentation and
		//   debugging.
		//
		//   The hardcoded 'eindex' and 'elen' info is the output that you will
		//   got if you run this strings on the MS.NET platform.
		struct DecoderFallbackExceptionTest
		{
			public string description;
			public byte [] bytes;
			public int [] eindex;
			public int [] elen;
			public DecoderFallbackExceptionTest (
					string description,
					int [] eindex,
					int [] elen,
					byte [] bytes)
			{
				this.description = description;
				this.bytes = bytes;
				if (eindex.Length != elen.Length)
					throw new ApplicationException ("eindex.Length != elen.Length in test '" + description + "'");
				this.eindex = eindex;
				this.elen = elen;
			}
		}

		// try to convert the all current test's bytes with Getchars()
		// in only one step
		private void DecoderFallbackExceptions_GetChars (
			char [] chars,
			int testno,
			Decoder dec,
			DecoderFallbackExceptionTest t)
		{
			try {
				dec.GetChars (t.bytes, 0, t.bytes.Length, chars, 0, true);
					Assert.IsTrue (
						t.eindex.Length == 0,
						String.Format (
							"test#{0}-1: UNEXPECTED SUCCESS",
							testno));
			} catch(DecoderFallbackException ex) {
				Assert.IsTrue (
					t.eindex.Length > 0,
					String.Format (
						"test#{0}-1: UNEXPECTED FAIL",
						testno));
				Assert.IsTrue (
					ex.Index == t.eindex[0],
					String.Format (
						"test#{0}-1: Expected exception at {1} not {2}.",
						testno,
						t.eindex[0],
						ex.Index));
				Assert.IsTrue (
					ex.BytesUnknown.Length == t.elen[0],
					String.Format (
						"test#{0}-1: Expected BytesUnknown.Length of {1} not {2}.",
						testno,
						t.elen[0],
						ex.BytesUnknown.Length));
				for (int i = 0; i < ex.BytesUnknown.Length; i++)
					Assert.IsTrue (
						ex.BytesUnknown[i] == t.bytes[ex.Index + i],
						String.Format (
							"test#{0}-1: expected byte {1:X} not {2:X} at {3}.",
							testno,
							t.bytes[ex.Index + i],
							ex.BytesUnknown[i],
							ex.Index + i));
				dec.Reset ();
			}
		}

		// convert bytes to string using a fixed blocksize.
		// If something bad happens, try to recover using the
		// DecoderFallbackException info.
		private void DecoderFallbackExceptions_Convert (
			char [] chars,
			int testno,
			Decoder dec,
			DecoderFallbackExceptionTest t,
			int block_size)
		{
			int charsUsed, bytesUsed;
			bool completed;

			int ce = 0; // current exception
			for (int c = 0; c < t.bytes.Length; ) {
				try {
					int bu = c + block_size > t.bytes.Length
							? t.bytes.Length - c
							: block_size;
					dec.Convert (
						t.bytes, c, bu,
						chars, 0, chars.Length,
						c + bu >= t.bytes.Length,
						out bytesUsed, out charsUsed,
						out completed);
					c += bytesUsed;
				} catch (DecoderFallbackException ex) {
					Assert.IsTrue (
						t.eindex.Length > ce,
						String.Format (
							"test#{0}-2-{1}#{2}: UNEXPECTED FAIL (c={3}, eIndex={4}, eBytesUnknwon={5})",
							testno, block_size, ce, c,
							ex.Index,
							ex.BytesUnknown.Length));
					Assert.IsTrue (
						ex.Index + c == t.eindex[ce],
						String.Format (
							"test#{0}-2-{1}#{2}: Expected at {3} not {4}.",
							testno, block_size, ce,
							t.eindex[ce],
							ex.Index + c));
					Assert.IsTrue (
						ex.BytesUnknown.Length == t.elen[ce],
						String.Format (
							"test#{0}-2-{1}#{2}: Expected BytesUnknown.Length of {3} not {4} @{5}.",
							testno, block_size, ce,
							t.elen[0], ex.BytesUnknown.Length, c));
					for (int i = 0; i < ex.BytesUnknown.Length; i++)
						Assert.IsTrue (
							ex.BytesUnknown[i] == t.bytes[ex.Index + i + c],
							String.Format (
								"test#{0}-2-{1}#{2}: Expected byte {3:X} not {4:X} at {5}.",
								testno, block_size, ce,
								t.bytes[ex.Index + i + c],
								ex.BytesUnknown[i],
								ex.Index + i));
					c += ex.BytesUnknown.Length + ex.Index;
					dec.Reset ();
					ce++;
				}
			}
			Assert.IsTrue (
				ce == t.eindex.Length,
				String.Format (
					"test#{0}-2-{1}: UNEXPECTED SUCCESS (expected {2} exceptions, but happened {3})",
					testno, block_size, t.eindex.Length, ce));
		}

		[Test]
		public void DecoderFallbackExceptions ()
		{

			DecoderFallbackExceptionTest [] tests = new DecoderFallbackExceptionTest []
			{
				/* #1  */
				new DecoderFallbackExceptionTest (
					"Greek word 'kosme'",
					new int [] { },
					new int [] { },
					new byte [] {
						0xce, 0xba, 0xe1, 0xbd, 0xb9, 0xcf,
						0x83, 0xce, 0xbc, 0xce, 0xb5 }),
				/* #2  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 1 byte",
					new int [] { },
					new int [] { },
					new byte [] { 0x00 }),
				/* #3  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 2 bytes",
					new int [] { },
					new int [] { },
					new byte [] { 0xc2, 0x80 }),
				/* #4  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 3 bytes",
					new int [] { },
					new int [] { },
					new byte [] { 0xe0, 0xa0, 0x80 }),
				/* #5  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 4 bytes",
					new int [] { },
					new int [] { },
					new byte [] { 0xf0, 0x90, 0x80, 0x80 }),
				/* #6  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 5 bytes",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xf8, 0x88, 0x80, 0x80, 0x80 }),
				/* #7  */
				new DecoderFallbackExceptionTest (
					"First possible sequence of 6 bytes",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0xfc, 0x84, 0x80, 0x80, 0x80, 0x80 }),
				/* #8  */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 1 byte",
					new int [] { },
					new int [] { },
					new byte [] { 0x7f }),
				/* #9  */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 2 bytes",
					new int [] { },
					new int [] { },
					new byte [] { 0xdf, 0xbf }),
				/* #10 */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 3 bytes",
					new int [] { },
					new int [] { },
					new byte [] { 0xef, 0xbf, 0xbf }),
				/* #11 */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 4 bytes",
					new int [] { 0, 1, 2, 3 },
					new int [] { 1, 1, 1, 1 },
					new byte [] { 0xf7, 0xbf, 0xbf, 0xbf }),
				/* #12 */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 5 bytes",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xfb, 0xbf, 0xbf, 0xbf, 0xbf }),
				/* #13 */
				new DecoderFallbackExceptionTest (
					"Last possible sequence of 6 bytes",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] { 0xfd, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf }),
				/* #14 */
				new DecoderFallbackExceptionTest (
					"U-0000D7FF = ed 9f bf",
					new int [] { },
					new int [] { },
					new byte [] { 0xed, 0x9f, 0xbf }),
				/* #15 */
				new DecoderFallbackExceptionTest (
					"U-0000E000 = ee 80 80",
					new int [] { },
					new int [] { },
					new byte [] { 0xee, 0x80, 0x80 }),
				/* #16 */
				new DecoderFallbackExceptionTest (
					"U-0000FFFD = ef bf bd",
					new int [] { },
					new int [] { },
					new byte [] { 0xef, 0xbf, 0xbd }),
				/* #17 */
				new DecoderFallbackExceptionTest (
					"U-0010FFFF = f4 8f bf bf",
					new int [] { },
					new int [] { },
					new byte [] { 0xf4, 0x8f, 0xbf, 0xbf }),
				/* #18 */
				new DecoderFallbackExceptionTest (
					"U-00110000 = f4 90 80 80",
					new int [] { 0, 2, 3 },
					new int [] { 2, 1, 1 },
					new byte [] { 0xf4, 0x90, 0x80, 0x80 }),
				/* #19 */
				new DecoderFallbackExceptionTest (
					"First continuation byte 0x80",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0x80 }),
				/* #20 */
				new DecoderFallbackExceptionTest (
					"Last  continuation byte 0xbf",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0xbf }),
				/* #21 */
				new DecoderFallbackExceptionTest (
					"2 continuation bytes",
					new int [] { 0, 1 },
					new int [] { 1, 1 },
					new byte [] { 0x80, 0xbf }),
				/* #22 */
				new DecoderFallbackExceptionTest (
					"3 continuation bytes",
					new int [] { 0, 1, 2 },
					new int [] { 1, 1, 1 },
					new byte [] { 0x80, 0xbf, 0x80 }),
				/* #23 */
				new DecoderFallbackExceptionTest (
					"4 continuation bytes",
					new int [] { 0, 1, 2, 3 },
					new int [] { 1, 1, 1, 1 },
					new byte [] { 0x80, 0xbf, 0x80, 0xbf }),
				/* #24 */
				new DecoderFallbackExceptionTest (
					"5 continuation bytes",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0x80, 0xbf, 0x80, 0xbf, 0x80 }),
				/* #25 */
				new DecoderFallbackExceptionTest (
					"6 continuation bytes",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0x80, 0xbf, 0x80, 0xbf, 0x80, 0xbf }),
				/* #26 */
				new DecoderFallbackExceptionTest (
					"7 continuation bytes",
					new int [] { 0, 1, 2, 3, 4, 5, 6 },
					new int [] { 1, 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0x80, 0xbf, 0x80, 0xbf, 0x80, 0xbf,
						0x80 }),
				/* #27 */
				new DecoderFallbackExceptionTest (
					"Sequence of all 64 continuation bytes",
					new int [] {
						 0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
						10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
						20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
						30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
						40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
						50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
						60, 61, 62, 63 },
					new int [] {
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1 },
					new byte [] {
						0x80, 0x81, 0x82, 0x83, 0x84, 0x85,
						0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b,
						0x8c, 0x8d, 0x8e, 0x8f, 0x90, 0x91,
						0x92, 0x93, 0x94, 0x95, 0x96, 0x97,
						0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d,
						0x9e, 0x9f, 0xa0, 0xa1, 0xa2, 0xa3,
						0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9,
						0xaa, 0xab, 0xac, 0xad, 0xae, 0xaf,
						0xb0, 0xb1, 0xb2, 0xb3, 0xb4, 0xb5,
						0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xbb,
						0xbc, 0xbd, 0xbe, 0xbf }),
				/* #28 */
				new DecoderFallbackExceptionTest (
					"All 32 first bytes of 2-byte sequences (0xc0-0xdf), each followed by a space character",
					new int [] {
						 0,  2,  4,  6,  8,
						10, 12, 14, 16, 18,
						20, 22, 24, 26, 28,
						30, 32, 34, 36, 38,
						40, 42, 44, 46, 48,
						50, 52, 54, 56, 58,
						60, 62 },
					new int [] {
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1 },
					new byte [] {
						0xc0, 0x20, 0xc1, 0x20, 0xc2, 0x20,
						0xc3, 0x20, 0xc4, 0x20, 0xc5, 0x20,
						0xc6, 0x20, 0xc7, 0x20, 0xc8, 0x20,
						0xc9, 0x20, 0xca, 0x20, 0xcb, 0x20,
						0xcc, 0x20, 0xcd, 0x20, 0xce, 0x20,
						0xcf, 0x20, 0xd0, 0x20, 0xd1, 0x20,
						0xd2, 0x20, 0xd3, 0x20, 0xd4, 0x20,
						0xd5, 0x20, 0xd6, 0x20, 0xd7, 0x20,
						0xd8, 0x20, 0xd9, 0x20, 0xda, 0x20,
						0xdb, 0x20, 0xdc, 0x20, 0xdd, 0x20,
						0xde, 0x20, 0xdf, 0x20 }),
				/* #29 */
				new DecoderFallbackExceptionTest (
					"All 16 first bytes of 3-byte sequences (0xe0-0xef), each followed by a space character",
					new int [] {
						 0,  2,  4,  6,  8,
						10, 12, 14, 16, 18,
						20, 22, 24, 26, 28,
						30 },
					new int [] {
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1, 1, 1, 1, 1,
						1 },
					new byte [] {
						0xe0, 0x20, 0xe1, 0x20, 0xe2, 0x20,
						0xe3, 0x20, 0xe4, 0x20, 0xe5, 0x20,
						0xe6, 0x20, 0xe7, 0x20, 0xe8, 0x20,
						0xe9, 0x20, 0xea, 0x20, 0xeb, 0x20,
						0xec, 0x20, 0xed, 0x20, 0xee, 0x20,
						0xef, 0x20 }),
				/* #30 */
				new DecoderFallbackExceptionTest (
					"All 8 first bytes of 4-byte sequences (0xf0-0xf7), each followed by a space character",
					new int [] { 0,  2,  4,  6,  8, 10, 12, 14 },
					new int [] { 1, 1, 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0xf0, 0x20, 0xf1, 0x20, 0xf2, 0x20,
						0xf3, 0x20, 0xf4, 0x20, 0xf5, 0x20,
						0xf6, 0x20, 0xf7, 0x20 }),
				/* #31 */
				new DecoderFallbackExceptionTest (
					"All 4 first bytes of 5-byte sequences (0xf8-0xfb), each followed by a space character",
					new int [] { 0, 2, 4, 6 },
					new int [] { 1, 1, 1, 1 },
					new byte [] {
						0xf8, 0x20, 0xf9, 0x20, 0xfa, 0x20,
						0xfb, 0x20 }),
				/* #32 */
				new DecoderFallbackExceptionTest (
					"All 2 first bytes of 6-byte sequences (0xfc-0xfd), each followed by a space character",
					new int [] { 0, 2 },
					new int [] { 1, 1 },
					new byte [] { 0xfc, 0x20, 0xfd, 0x20 }),
				/* #33 */
				new DecoderFallbackExceptionTest (
					"2-byte sequence with last byte missing",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0xc0 }),
				/* #34 */
				new DecoderFallbackExceptionTest (
					"3-byte sequence with last byte missing",
					new int [] { 0 },
					new int [] { 2 },
					new byte [] { 0xe0, 0x80 }),
				/* #35 */
				new DecoderFallbackExceptionTest (
					"4-byte sequence with last byte missing",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xf0, 0x80, 0x80 }),
				/* #36 */
				new DecoderFallbackExceptionTest (
					"5-byte sequence with last byte missing",
					new int [] { 0, 1, 2, 3 },
					new int [] { 1, 1, 1, 1 },
					new byte [] { 0xf8, 0x80, 0x80, 0x80 }),
				/* #37 */
				new DecoderFallbackExceptionTest (
					"6-byte sequence with last byte missing",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xfc, 0x80, 0x80, 0x80, 0x80 }),
				/* #38 */
				new DecoderFallbackExceptionTest (
					"2-byte sequence with last byte missing",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0xdf }),
				/* #39 */
				new DecoderFallbackExceptionTest (
					"3-byte sequence with last byte missing",
					new int [] { 0 },
					new int [] { 2 },
					new byte [] { 0xef, 0xbf }),
				/* #40 */
				new DecoderFallbackExceptionTest (
					"4-byte sequence with last byte missing",
					new int [] { 0, 1, 2 },
					new int [] { 1, 1, 1 },
					new byte [] { 0xf7, 0xbf, 0xbf }),
				/* #41 */
				new DecoderFallbackExceptionTest (
					"5-byte sequence with last byte missing",
					new int [] { 0, 1, 2, 3 },
					new int [] { 1, 1, 1, 1 },
					new byte [] { 0xfb, 0xbf, 0xbf, 0xbf }),
				/* #42 */
				new DecoderFallbackExceptionTest (
					"6-byte sequence with last byte missing",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xfd, 0xbf, 0xbf, 0xbf, 0xbf }),
				/* #43 */
				new DecoderFallbackExceptionTest (
					"All the 10 sequences of 3.3 concatenated",
					new int [] {
						 0,  1,      3,
						 5,  6,  7,  8,  9,
						10, 11, 12, 13, 14,
						15, 16,     18, 19,
						20, 21, 22, 23, 24,
						25, 26, 27, 28, 29 },
					new int [] {
						 1,  2,      2,
						 1,  1,  1,  1,  1,
						 1,  1,  1,  1,  1,
						 1,  2,      1,  1,
						 1,  1,  1,  1,  1,
						 1,  1,  1,  1,  1 },
					new byte [] {
						0xc0, 0xe0, 0x80, 0xf0, 0x80, 0x80,
						0xf8, 0x80, 0x80, 0x80, 0xfc, 0x80,
						0x80, 0x80, 0x80, 0xdf, 0xef, 0xbf,
						0xf7, 0xbf, 0xbf, 0xfb, 0xbf, 0xbf,
						0xbf, 0xfd, 0xbf, 0xbf, 0xbf, 0xbf }),
				/* #44 */
				new DecoderFallbackExceptionTest (
					"Bad chars fe",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0xfe }),
				/* #45 */
				new DecoderFallbackExceptionTest (
					"Bad chars ff",
					new int [] { 0 },
					new int [] { 1 },
					new byte [] { 0xff }),
				/* #46 */
				new DecoderFallbackExceptionTest (
					"Bad chars fe fe ff ff",
					new int [] { 0, 1, 2, 3 },
					new int [] { 1, 1, 1, 1 },
					new byte [] { 0xfe, 0xfe, 0xff, 0xff }),
				/* #47 */
				new DecoderFallbackExceptionTest (
					"Overlong U+002F = c0 af",
					new int [] { 0, 1 },
					new int [] { 1, 1 },
					new byte [] { 0xc0, 0xaf }),
				/* #48 */
				new DecoderFallbackExceptionTest (
					"Overlong U+002F = e0 80 af",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xe0, 0x80, 0xaf }),
				/* #49 */
				new DecoderFallbackExceptionTest (
					"Overlong U+002F = f0 80 80 af",
					new int [] { 0, 2, 3 },
					new int [] { 2, 1, 1 },
					new byte [] { 0xf0, 0x80, 0x80, 0xaf }),
				/* #50 */
				new DecoderFallbackExceptionTest (
					"Overlong U+002F = f8 80 80 80 af",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xf8, 0x80, 0x80, 0x80, 0xaf }),
				/* #51 */
				new DecoderFallbackExceptionTest (
					"Overlong U+002F = fc 80 80 80 80 af",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0xfc, 0x80, 0x80, 0x80, 0x80, 0xaf }),
				/* #52 */
				new DecoderFallbackExceptionTest (
					"Maximum overlong U-0000007F",
					new int [] { 0, 1 },
					new int [] { 1, 1 },
					new byte [] { 0xc1, 0xbf }),
				/* #53 */
				new DecoderFallbackExceptionTest (
					"Maximum overlong U-000007FF",
					new int [] { 0, 2 },
					new int [] { 2, 1, },
					new byte [] { 0xe0, 0x9f, 0xbf }),
				/* #54 */
				new DecoderFallbackExceptionTest (
					"Maximum overlong U-0000FFFF",
					new int [] { 0, 2, 3 },
					new int [] { 2, 1, 1 },
					new byte [] { 0xf0, 0x8f, 0xbf, 0xbf }),
				/* #55 */
				new DecoderFallbackExceptionTest (	
					"Maximum overlong U-001FFFFF",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xf8, 0x87, 0xbf, 0xbf, 0xbf }),
				/* #56 */
				new DecoderFallbackExceptionTest (
					"Maximum overlong U-03FFFFFF",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0xfc, 0x83, 0xbf, 0xbf, 0xbf, 0xbf }),
				/* #57 */
				new DecoderFallbackExceptionTest (
					"Null overlong c0 80",
					new int [] { 0, 1 },
					new int [] { 1, 1 },
					new byte [] { 0xc0, 0x80, 0x22 }),
				/* #58 */
				new DecoderFallbackExceptionTest (
					"Null overlong e0 80 80",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xe0, 0x80, 0x80 }),
				/* #59 */
				new DecoderFallbackExceptionTest (
					"Null overlong f0 80 80 80",
					new int [] { 0, 2, 3 },
					new int [] { 2, 1, 1 },
					new byte [] { 0xf0, 0x80, 0x80, 0x80 }),
				/* #60 */
				new DecoderFallbackExceptionTest (
					"Null overlong f8 80 80 80 80",
					new int [] { 0, 1, 2, 3, 4 },
					new int [] { 1, 1, 1, 1, 1 },
					new byte [] { 0xf8, 0x80, 0x80, 0x80, 0x80 }),
				/* #61 */
				new DecoderFallbackExceptionTest (
					"Null overlong fc 80 80 80 80 80",
					new int [] { 0, 1, 2, 3, 4, 5 },
					new int [] { 1, 1, 1, 1, 1, 1 },
					new byte [] {
						0xfc, 0x80, 0x80, 0x80, 0x80, 0x80 }),
				/* #62 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+D800",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xa0, 0x80 }),
				/* #63 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DB7F",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xad, 0xbf }),
				/* #64 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DB80",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xae, 0x80 }),
				/* #65 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DBFF",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xaf, 0xbf }),
				/* #66 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DC00",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xb0, 0x80 }),
				/* #67 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DF80",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xbe, 0x80 }),
				/* #68 */
				new DecoderFallbackExceptionTest (
					"Single UTF-16 surrogate U+DFFF",
					new int [] { 0, 2 },
					new int [] { 2, 1 },
					new byte [] { 0xed, 0xbf, 0xbf }),
				/* #69 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+D800 U+DC00",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xa0, 0x80, 0xed, 0xb0, 0x80 }),
				/* #70 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+D800 U+DFFF",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xa0, 0x80, 0xed, 0xbf, 0xbf }),
				/* #71 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DB7F U+DC00",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xad, 0xbf, 0xed, 0xb0, 0x80 }),
				/* #72 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DB7F U+DFFF",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xad, 0xbf, 0xed, 0xbf, 0xbf }),
				/* #73 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DB80 U+DC00",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xae, 0x80, 0xed, 0xb0, 0x80 }),
				/* #74 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DB80 U+DFFF",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xae, 0x80, 0xed, 0xbf, 0xbf }),
				/* #75 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DBFF U+DC00",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xaf, 0xbf, 0xed, 0xb0, 0x80 }),
				/* #76 */
				new DecoderFallbackExceptionTest (
					"Paired UTF-16 surrogate U+DBFF U+DFFF",
					new int [] { 0, 2, 3, 5 },
					new int [] { 2, 1, 2, 1 },
					new byte [] {
						0xed, 0xaf, 0xbf, 0xed, 0xbf, 0xbf }),
				/* #77 */
				new DecoderFallbackExceptionTest (
					"Illegal code position U+FFFE",
					new int [] { },
					new int [] { },
					new byte [] { 0xef, 0xbf, 0xbe }),
				/* #78 */
				new DecoderFallbackExceptionTest (
					"Illegal code position U+FFFF",
					new int [] { },
					new int [] { },
					new byte [] { 0xef, 0xbf, 0xbf }),
			};
			Encoding utf8 = Encoding.GetEncoding (
						"utf-8",
						new EncoderExceptionFallback(),
						new DecoderExceptionFallback());
			Decoder dec = utf8.GetDecoder ();
			char [] chars;

			for(int t = 0; t < tests.Length; t++) {
				chars = new char [utf8.GetMaxCharCount (tests[t].bytes.Length)];

				// #1 complete conversion
				DecoderFallbackExceptions_GetChars (chars, t+1, dec, tests[t]);

				// #2 convert with several block_sizes
				for (int bs = 1; bs <= tests[t].bytes.Length; bs++)
					DecoderFallbackExceptions_Convert (chars, t+1, dec, tests[t], bs);
			}
		}

		// EncoderFallbackExceptionTest
		//   This struct describes a EncoderFallbackExceptions' test.
		//   It contains an array (index_fail) which is void if it is a
		//   valid UTF16 string.
		//   If it is an invalid string this array contains indexes
		//   (in 'index_fail') which point to the invalid chars in
		//   'str'.
		//   This array is hardcoded in each tests and it contains the
		//   absolute positions found in a sequence of
		//   EncoderFallbackException exceptions thrown if you convert
		//   this strings on a MS.NET platform.
		struct EncoderFallbackExceptionTest
		{
			public string str;
			public int [] eindex;
			public EncoderFallbackExceptionTest (
					string str,
					int [] eindex)
			{
				this.str = str;
				this.eindex = eindex;
			}
		}

		// try to encode some bytes at once with GetBytes
		private void EncoderFallbackExceptions_GetBytes (
			byte [] bytes,
			int testno,
			Encoder enc,
			EncoderFallbackExceptionTest t)
		{
			try {
				enc.GetBytes (
					t.str.ToCharArray (), 0, t.str.Length,
					bytes, 0, true);
				Assert.IsTrue (
					t.eindex.Length == 0,
					String.Format (
						"test#{0}-1: UNEXPECTED SUCCESS",
						testno));
			} catch(EncoderFallbackException ex) {
				Assert.IsTrue (
					t.eindex.Length > 0,
					String.Format (
						"test#{0}-1: UNEXPECTED FAIL",
						testno));
				Assert.IsTrue (
					ex.Index == t.eindex[0],
					String.Format (
						"test#{0}-1: Expected exception at {1} not {2}.",
						testno, t.eindex[0], ex.Index));
				Assert.IsTrue (
					!ex.IsUnknownSurrogate (),
					String.Format (
						"test#{0}-1: Expected false not {1} in IsUnknownSurrogate().",
						testno,
						ex.IsUnknownSurrogate ()));
				// NOTE: I know that in the previous check we
				// have asserted that ex.IsUnknownSurrogate()
				// is always false, but this does not mean that
				// we don't have to take in consideration its
				// real value for the next check.
				if (ex.IsUnknownSurrogate ())
					Assert.IsTrue (
						ex.CharUnknownHigh == t.str[ex.Index]
						&& ex.CharUnknownLow == t.str[ex.Index + 1],
						String.Format (
							"test#{0}-1: expected ({1:X}, {2:X}) not ({3:X}, {4:X}).",
							testno,
							t.str[ex.Index],
							t.str[ex.Index + 1],
							ex.CharUnknownHigh,
							ex.CharUnknownLow));
				else
					Assert.IsTrue (
						ex.CharUnknown == t.str[ex.Index],
						String.Format (
							"test#{0}-1: expected ({1:X}) not ({2:X}).",
							testno,
							t.str[ex.Index],
							ex.CharUnknown));
				enc.Reset ();
			}
		}

		private void EncoderFallbackExceptions_Convert (
			byte [] bytes,
			int testno,
			Encoder enc,
			EncoderFallbackExceptionTest t,
			int block_size)
		{
			int charsUsed, bytesUsed;
			bool completed;

			int ce = 0; // current exception

			for (int c = 0; c < t.str.Length; ) {
				//Console.WriteLine ("test#{0}-2-{1}: c={2}", testno, block_size, c);
				try {
					int bu = c + block_size > t.str.Length
							? t.str.Length - c
							: block_size;
					enc.Convert (
						t.str.ToCharArray (), c, bu,
						bytes, 0, bytes.Length,
						c + bu >= t.str.Length,
						out charsUsed, out bytesUsed,
						out completed);
					c += charsUsed;
				} catch (EncoderFallbackException ex) {
					//Console.WriteLine (
					//	"test#{0}-2-{1}#{2}: Exception (Index={3}, UnknownSurrogate={4})",
					//	testno, block_size, ce,
					//	ex.Index, ex.IsUnknownSurrogate ());
					Assert.IsTrue (
						ce < t.eindex.Length,
						String.Format (
							"test#{0}-2-{1}#{2}: UNEXPECTED EXCEPTION (Index={3}, UnknownSurrogate={4})",
							testno, block_size, ce,
							ex.Index,
							ex.IsUnknownSurrogate ()));
					Assert.IsTrue (
						ex.Index + c == t.eindex[ce],
						String.Format (
							"test#{0}-2-{1}#{2}: Expected exception at {3} not {4}.",
							testno, block_size, ce,
							t.eindex[ce],
							ex.Index + c));
					Assert.IsTrue (
						!ex.IsUnknownSurrogate (),
						String.Format (
							"test#{0}-2-{1}#{2}: Expected false not {3} in IsUnknownSurrogate().",
							testno, block_size, ce,
							ex.IsUnknownSurrogate ()));
					if (ex.IsUnknownSurrogate ()) {
						Assert.IsTrue (
							ex.CharUnknownHigh == t.str[ex.Index + c]
							&& ex.CharUnknownLow == t.str[ex.Index + c + 1],
							String.Format (
								"test#{0}-2-{1}#{2}: expected ({3:X}, {4:X}) not ({5:X}, {6:X}).",
								testno, block_size, ce,
								t.str[ex.Index + c], t.str[ex.Index + c + 1],
								ex.CharUnknownHigh, ex.CharUnknownLow));
						c += ex.Index + 2;
					} else {
						Assert.IsTrue (
							ex.CharUnknown == t.str[ex.Index + c],
							String.Format (
								"test#{0}-2-{1}#{2}: expected ({3:X}) not ({4:X}).",
								testno, block_size, ce,
								t.str[ex.Index + c],
								ex.CharUnknown));
						c += ex.Index + 1;
					}
					enc.Reset ();
					ce++;
				}
			}
			Assert.IsTrue (
				ce == t.eindex.Length,
				String.Format (
					"test#{0}-2-{1}: UNEXPECTED SUCCESS (expected {2} exceptions, but happened {3})",
					testno, block_size, t.eindex.Length, ce));
		}

		[Test]
		public void EncoderFallbackExceptions ()
		{

			EncoderFallbackExceptionTest [] tests = new EncoderFallbackExceptionTest []
			{
				/* #1  */ new EncoderFallbackExceptionTest ( "Zero \u0000.",                                   new int [] { }),
				/* #2  */ new EncoderFallbackExceptionTest ( "Last before leads \uD7FF.",                      new int [] { }),
				/* #3  */ new EncoderFallbackExceptionTest ( "Using lead \uD800 without a surrogate.",         new int [] { 11 }),
				/* #4  */ new EncoderFallbackExceptionTest ( "Using lead \uD877 without a surrogate.",         new int [] { 11 }),
				/* #5  */ new EncoderFallbackExceptionTest ( "Using lead \uDBFF without a surrogate.",         new int [] { 11 }),
				/* #6  */ new EncoderFallbackExceptionTest ( "Using trail \uDC00 without a lead.",             new int [] { 12 }),
				/* #7  */ new EncoderFallbackExceptionTest ( "Using trail \uDBFF without a lead.",             new int [] { 12 }),
				/* #8  */ new EncoderFallbackExceptionTest ( "First-plane 2nd block \uE000.",                  new int [] { }),
				/* #9  */ new EncoderFallbackExceptionTest ( "First-plane 2nd block \uFFFF.",                  new int [] { }),
				/* #10 */ new EncoderFallbackExceptionTest ( "Playing with first surrogate \uD800\uDC00.",     new int [] { }),
				/* #11 */ new EncoderFallbackExceptionTest ( "Playing before first surrogate \uD800\uDBFF.",   new int [] { 31, 32 }),
				/* #12 */ new EncoderFallbackExceptionTest ( "Playing with last of first plane \uD800\uDFFF.", new int [] { }),
				/* #13 */ new EncoderFallbackExceptionTest ( "Playing with first of last plane \uDBFF\uDC00.", new int [] { }),
				/* #14 */ new EncoderFallbackExceptionTest ( "Playing with last surrogate \uDBFF\uDFFF.",      new int [] { }),
				/* #15 */ new EncoderFallbackExceptionTest ( "Playing after last surrogate \uDBFF\uE000.",     new int [] { 29 }),
				/* #16 */ new EncoderFallbackExceptionTest ( "Incomplete string \uD800",                       new int [] { 18 }),
				/* #17 */ new EncoderFallbackExceptionTest ( "Horrible thing \uD800\uD800.",                   new int [] { 15, 16 }),
			};
			Encoding utf8 = Encoding.GetEncoding (
						"utf-8",
						new EncoderExceptionFallback(),
						new DecoderExceptionFallback());
			Encoder enc = utf8.GetEncoder ();
			byte [] bytes;

			for(int t = 0; t < tests.Length; t++) {
				bytes = new byte [utf8.GetMaxByteCount (tests[t].str.Length)];

				// #1 complete conversion
				EncoderFallbackExceptions_GetBytes (bytes, t+1, enc, tests[t]);

				// #2 convert in two rounds
				for (int bs = 1; bs <= tests[t].str.Length; bs++)
					EncoderFallbackExceptions_Convert (bytes, t+1, enc, tests[t], bs);
			}
		}
	}
}
