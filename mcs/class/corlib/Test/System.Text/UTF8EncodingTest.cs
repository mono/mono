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
			} catch (ArgumentException) {
			}

			string s = "\uD800";
			try {
				int ret = Encoding.UTF8.GetBytes (s, 0, 1, bytes, 0);
#if NET_2_0
				Assert.AreEqual (0, ret, "drop insufficient char in 2.0: string");
#else
				Assert.Fail ("ArgumentException is expected: string");
#endif
			} catch (ArgumentException) {
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
		public void Bug415628 ()
		{
			using (var f = File.Open ("Test/resources/415628.bin", FileMode.Open)) {
				BinaryReader br = new BinaryReader (f);
				byte [] buf = br.ReadBytes (8000);
				Encoding.UTF8.GetString(buf);
			}
		}
		
		[Test]
		[ExpectedException (typeof (EncoderFallbackException))]
		public void EncoderFallback () // bug #565129
		{
			// with GetByteCount()
			var s = new char[] { '\udf45', '\ud808' };
			new UTF8Encoding (false, true).GetBytes (s, 0, 2);
		}

		[Test]
		[ExpectedException (typeof (EncoderFallbackException))]
		public void EncoderFallback2 () // bug #565129
		{
			// with GetByteCount()
			var s = new char[] { '\udf45', '\ud808' };
			// without GetByteCount()
			byte [] bytes = new byte [6];
			new UTF8Encoding (false, true).GetBytes (s, 0, 2, bytes, 0);
		}
#endif
	}
}
