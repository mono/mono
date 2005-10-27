//
// UnixEncodingTest.cs - NUnit Test Cases for Mono.Unix.UnixEncoding
//
// Authors:
//	Patrick Kalkman  kalkman@cistron.nl
//	Sebastien Pouliot (spouliot@motus.com)
//	Jonathan Pryor  <jonpryor@vt.edu>
//
// (C) 2003 Patrick Kalkman
// (C) 2004 Novell (http://www.novell.com)
// (C) 2005 Jonathan Pryor
//

using NUnit.Framework;
using System;
using System.Text;
using Mono.Unix;

namespace MonoTests.Mono.Unix {

	[TestFixture]
	public class UnixEncodingTest : Assertion {

		private UnixEncoding unix;

		[SetUp]
		public void Create () 
		{
			unix = new UnixEncoding ();
		}

		[Test]
		public void TestEncodingGetBytes1()
		{
			UnixEncoding unixEnc = new UnixEncoding ();
			string UniCode = "\u0041\u2262\u0391\u002E";

			// "A<NOT IDENTICAL TO><ALPHA>." may be encoded as 41 E2 89 A2 CE 91 2E 
			// see (RFC 2044)
			byte[] unixBytes = unixEnc.GetBytes (UniCode);

			Assertion.AssertEquals ("UTF #1", 0x41, unixBytes [0]);
			Assertion.AssertEquals ("UTF #2", 0xE2, unixBytes [1]);
			Assertion.AssertEquals ("UTF #3", 0x89, unixBytes [2]);
			Assertion.AssertEquals ("UTF #4", 0xA2, unixBytes [3]);
			Assertion.AssertEquals ("UTF #5", 0xCE, unixBytes [4]);
			Assertion.AssertEquals ("UTF #6", 0x91, unixBytes [5]);
			Assertion.AssertEquals ("UTF #7", 0x2E, unixBytes [6]);
		}

		[Test]
		public void TestEncodingGetBytes2()
		{
			UnixEncoding unixEnc = new UnixEncoding ();
			string UniCode = "\u0048\u0069\u0020\u004D\u006F\u006D\u0020\u263A\u0021";

			// "Hi Mom <WHITE SMILING FACE>!" may be encoded as 48 69 20 4D 6F 6D 20 E2 98 BA 21 
			// see (RFC 2044)
			byte[] unixBytes = new byte [11];

			int ByteCnt = unixEnc.GetBytes (UniCode.ToCharArray(), 0, UniCode.Length, unixBytes, 0);

			Assertion.AssertEquals ("UTF #1", 11, ByteCnt);
			Assertion.AssertEquals ("UTF #2", 0x48, unixBytes [0]);
			Assertion.AssertEquals ("UTF #3", 0x69, unixBytes [1]);
			Assertion.AssertEquals ("UTF #4", 0x20, unixBytes [2]);
			Assertion.AssertEquals ("UTF #5", 0x4D, unixBytes [3]);
			Assertion.AssertEquals ("UTF #6", 0x6F, unixBytes [4]);
			Assertion.AssertEquals ("UTF #7", 0x6D, unixBytes [5]);
			Assertion.AssertEquals ("UTF #8", 0x20, unixBytes [6]);
			Assertion.AssertEquals ("UTF #9", 0xE2, unixBytes [7]);
			Assertion.AssertEquals ("UTF #10", 0x98, unixBytes [8]);
			Assertion.AssertEquals ("UTF #11", 0xBA, unixBytes [9]);
			Assertion.AssertEquals ("UTF #12", 0x21, unixBytes [10]);
		}

		[Test]
		public void TestDecodingGetChars1()
		{
			UnixEncoding unixEnc = new UnixEncoding ();
			// 41 E2 89 A2 CE 91 2E may be decoded as "A<NOT IDENTICAL TO><ALPHA>." 
			// see (RFC 2044)
			byte[] unixBytes = new byte [] {0x41, 0xE2, 0x89, 0xA2, 0xCE, 0x91, 0x2E};
			char[] UniCodeChars = unixEnc.GetChars(unixBytes);

			Assertion.AssertEquals ("UTF #1", 0x0041, UniCodeChars [0]);
			Assertion.AssertEquals ("UTF #2", 0x2262, UniCodeChars [1]);
			Assertion.AssertEquals ("UTF #3", 0x0391, UniCodeChars [2]);
			Assertion.AssertEquals ("UTF #4", 0x002E, UniCodeChars [3]);
		}

		[Test]
		public void TestMaxCharCount()
		{
			UnixEncoding unixenc = new UnixEncoding ();
			Assertion.AssertEquals ("UTF #1", 50, unixenc.GetMaxCharCount(50));
		}

		[Test]
		public void TestMaxByteCount()
		{
			UnixEncoding unixenc = new UnixEncoding ();
			Assertion.AssertEquals ("UTF #1", 200, unixenc.GetMaxByteCount(50));
		}

		// regression for bug #59648
		[Test]
		public void TestThrowOnInvalid ()
		{
			UnixEncoding u = new UnixEncoding ();

			byte[] data = new byte [] { 0xC0, 0xAF };
			string s = u.GetString (data);
			AssertEquals (4, s.Length);
			AssertEquals (0x0000, (int) s [0]);
			AssertEquals (0xC0,   (int) s [1]);
			AssertEquals (0x0000, (int) s [2]);
			AssertEquals (0xAF,   (int) s [3]);
			AssertEquals ("Output-TestThrowOnInvalid", "\u0000\xC0\u0000\xAF", s);
			AssertEquals ("Reconverted-TestThrowOnInvalid", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));

			data = new byte [] { 0x30, 0x31, 0xC0, 0xAF, 0x30, 0x32 };
			s = u.GetString (data);
			AssertEquals (8, s.Length);
			AssertEquals (0x30,   (int) s [0]);
			AssertEquals (0x31,   (int) s [1]);
			AssertEquals (0x0000, (int) s [2]);
			AssertEquals (0xC0,   (int) s [3]);
			AssertEquals (0x0000, (int) s [4]);
			AssertEquals (0xAF,   (int) s [5]);
			AssertEquals (0x30,   (int) s [6]);
			AssertEquals (0x32,   (int) s [7]);

			AssertEquals ("Output-TestThrowOnInvalid2", "\x30\x31\u0000\xC0\u0000\xAF\x30\x32", s);
			AssertEquals ("Reconverted-TestThrowOnInvalid2", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		// unix decoding tests from http://www.cl.cam.ac.uk/~mgk25/

		[Test]
		public void T1_Correct_GreekWord_kosme () 
		{
			byte[] data = { 0xCE, 0xBA, 0xE1, 0xBD, 0xB9, 0xCF, 0x83, 0xCE, 0xBC, 0xCE, 0xB5 };
			string s = unix.GetString (data);
			// cute but saving source code in unicode can be problematic
			// so we just ensure we can re-encode this
			AssertEquals ("Reconverted", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_1_FirstPossibleSequence_Pass () 
		{
			byte[] data211 = { 0x00 };
			string s = unix.GetString (data211);
			AssertEquals ("1 byte  (U-00000000)", "\0", s);
			AssertEquals ("Reconverted-1", BitConverter.ToString (data211), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data212 = { 0xC2, 0x80 };
			s = unix.GetString (data212);
			AssertEquals ("2 bytes (U-00000080)", 128, s [0]);
			AssertEquals ("Reconverted-2", BitConverter.ToString (data212), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data213 = { 0xE0, 0xA0, 0x80 };
			s = unix.GetString (data213);
			AssertEquals ("3 bytes (U-00000800)", 2048, s [0]);
			AssertEquals ("Reconverted-3", BitConverter.ToString (data213), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data214 = { 0xF0, 0x90, 0x80, 0x80 };
			s = unix.GetString (data214);
			AssertEquals ("4 bytes (U-00010000)-0", 55296, s [0]);
			AssertEquals ("4 bytes (U-00010000)-1", 56320, s [1]);
			AssertEquals ("Reconverted-4", BitConverter.ToString (data214), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_1_FirstPossibleSequence_Fail_5 () 
		{
			byte[] data215 = { 0xF8, 0x88, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data215);
			AssertEquals ("Output-5", "\u0000\xF8\u0000\x88\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-5", BitConverter.ToString (data215), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_1_FirstPossibleSequence_Fail_6 () 
		{
			byte[] data216 = { 0xFC, 0x84, 0x80, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data216);
			AssertEquals ("Output-6", "\u0000\xFC\u0000\x84\u0000\x80\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-6", BitConverter.ToString (data216), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_2_LastPossibleSequence_Pass () 
		{
			byte[] data221 = { 0x7F };
			string s = unix.GetString (data221);
			AssertEquals ("1 byte  (U-0000007F)", 127, s [0]);
			AssertEquals ("Reconverted-1", BitConverter.ToString (data221), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data222 = { 0xDF, 0xBF };
			s = unix.GetString (data222);
			AssertEquals ("2 bytes (U-000007FF)", 2047, s [0]);
			AssertEquals ("Reconverted-2", BitConverter.ToString (data222), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data223 = { 0xEF, 0xBF, 0xBF };
			s = unix.GetString (data223);
			AssertEquals ("3 bytes (U-0000FFFF)", 65535, s [0]);
			AssertEquals ("Reconverted-3", BitConverter.ToString (data223), BitConverter.ToString (unix.GetBytes (s)));

		}

		[Test]
		public void T2_Boundary_2_LastPossibleSequence_Fail_4 () 
		{
			byte[] data224 = { 0x7F, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data224);
			AssertEquals ("Output-4", 
					"\x7F\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-4", BitConverter.ToString (data224), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_2_LastPossibleSequence_Fail_5 () 
		{
			byte[] data225 = { 0xFB, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data225);
			AssertEquals ("Output-5", "\u0000\xFB\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-5", BitConverter.ToString (data225), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_2_LastPossibleSequence_Fail_6 () 
		{
			byte[] data226 = { 0xFD, 0xBF, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data226);
			AssertEquals ("Output-6", "\u0000\xFD\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-6", BitConverter.ToString (data226), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_3_Other_Pass () 
		{
			byte[] data231 = { 0xED, 0x9F, 0xBF };
			string s = unix.GetString (data231);
			AssertEquals ("U-0000D7FF", 55295, s [0]);
			AssertEquals ("Reconverted-1", BitConverter.ToString (data231), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data232 = { 0xEE, 0x80, 0x80 };
			s = unix.GetString (data232);
			AssertEquals ("U-0000E000", 57344, s [0]);
			AssertEquals ("Reconverted-2", BitConverter.ToString (data232), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data233 = { 0xEF, 0xBF, 0xBD };
			s = unix.GetString (data233);
			AssertEquals ("U-0000FFFD", 65533, s [0]);
			AssertEquals ("Reconverted-3", BitConverter.ToString (data233), BitConverter.ToString (unix.GetBytes (s)));

			byte[] data234 = { 0xF4, 0x8F, 0xBF, 0xBF };
			s = unix.GetString (data234);
			AssertEquals ("U-0010FFFF-0", 56319, s [0]);
			AssertEquals ("U-0010FFFF-1", 57343, s [1]);
			AssertEquals ("Reconverted-4", BitConverter.ToString (data234), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T2_Boundary_3_Other_Fail_5 () 
		{
			byte[] data235 = { 0xF4, 0x90, 0x80, 0x80 };
			string s = unix.GetString (data235);
			AssertEquals ("Output-5", "\u0000\xF4\u0000\x90\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-5", BitConverter.ToString (data235), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_311 () 
		{
			byte[] data = { 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-311", "\u0000\x80", s);
			AssertEquals ("Reconverted-311", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_312 () 
		{
			byte[] data = { 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-312", "\u0000\xBF", s);
			AssertEquals ("Reconverted-313", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_313 () 
		{
			byte[] data = { 0x80, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-313", "\u0000\x80\u0000\xBF", s);
			AssertEquals ("Reconverted-313", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_314 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-314", "\u0000\x80\u0000\xBF\u0000\x80", s);
			AssertEquals ("Reconverted-314", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_315 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-315", "\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF", s);
			AssertEquals ("Reconverted-315", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_316 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-316", 
					"\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF\u0000\x80", s);
			AssertEquals ("Reconverted-316", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_317 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-317", 
					"\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF", s);
			AssertEquals ("Reconverted-317", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_318 () 
		{
			byte[] data = { 0x80, 0xBF, 0x80, 0xBF, 0x80, 0xBF, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-318", 
					"\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF\u0000\x80\u0000\xBF\u0000\x80", s);
			AssertEquals ("Reconverted-318", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_1_UnexpectedContinuation_319 () 
		{
			// 64 different continuation characters
			byte[] data = {
				0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 
				0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F, 
				0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 
				0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-319", 
					"\u0000\x80\u0000\x81\u0000\x82\u0000\x83\u0000\x84\u0000\x85\u0000\x86\u0000\x87" +
					"\u0000\x88\u0000\x89\u0000\x8A\u0000\x8B\u0000\x8C\u0000\x8D\u0000\x8E\u0000\x8F" +
					"\u0000\x90\u0000\x91\u0000\x92\u0000\x93\u0000\x94\u0000\x95\u0000\x96\u0000\x97" +
					"\u0000\x98\u0000\x99\u0000\x9A\u0000\x9B\u0000\x9C\u0000\x9D\u0000\x9E\u0000\x9F" +
					"\u0000\xA0\u0000\xA1\u0000\xA2\u0000\xA3\u0000\xA4\u0000\xA5\u0000\xA6\u0000\xA7" +
					"\u0000\xA8\u0000\xA9\u0000\xAA\u0000\xAB\u0000\xAC\u0000\xAD\u0000\xAE\u0000\xAF" +
					"\u0000\xB0\u0000\xB1\u0000\xB2\u0000\xB3\u0000\xB4\u0000\xB5\u0000\xB6\u0000\xB7" +
					"\u0000\xB8\u0000\xB9\u0000\xBA\u0000\xBB\u0000\xBC\u0000\xBD\u0000\xBE\u0000\xBF",
					s);
			AssertEquals ("Reconverted-319", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_2_LonelyStart_321 ()
		{
			byte[] data = { 
				0xC0, 0x20, 0xC1, 0x20, 0xC2, 0x20, 0xC3, 0x20, 0xC4, 0x20, 0xC5, 0x20, 0xC6, 0x20, 0xC7, 0x20, 
				0xC8, 0x20, 0xC9, 0x20, 0xCA, 0x20, 0xCB, 0x20, 0xCC, 0x20, 0xCD, 0x20, 0xCE, 0x20, 0xCF, 0x20, 
				0xD0, 0x20, 0xD1, 0x20, 0xD2, 0x20, 0xD3, 0x20, 0xD4, 0x20, 0xD5, 0x20, 0xD6, 0x20, 0xD7, 0x20, 
				0xD8, 0x20, 0xD9, 0x20, 0xDA, 0x20, 0xDB, 0x20, 0xDC, 0x20, 0xDD, 0x20, 0xDE, 0x20, 0xDF, 0x20 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_2_LonelyStart_321",
					"\u0000\xC0 \u0000\xC1 \u0000\xC2 \u0000\xC3 \u0000\xC4 \u0000\xC5 \u0000\xC6 \u0000\xC7 " +
					"\u0000\xC8 \u0000\xC9 \u0000\xCA \u0000\xCB \u0000\xCC \u0000\xCD \u0000\xCE \u0000\xCF " +
					"\u0000\xD0 \u0000\xD1 \u0000\xD2 \u0000\xD3 \u0000\xD4 \u0000\xD5 \u0000\xD6 \u0000\xD7 " +
					"\u0000\xD8 \u0000\xD9 \u0000\xDA \u0000\xDB \u0000\xDC \u0000\xDD \u0000\xDE \u0000\xDF ",
					s
					);
			AssertEquals ("Reconverted-T3_Malformed_2_LonelyStart_321",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_2_LonelyStart_322 () 
		{
			byte[] data = { 
				0xE0, 0x20, 0xE1, 0x20, 0xE2, 0x20, 0xE3, 0x20, 0xE4, 0x20, 0xE5, 0x20, 0xE6, 0x20, 0xE7, 0x20, 
				0xE8, 0x20, 0xE9, 0x20, 0xEA, 0x20, 0xEB, 0x20, 0xEC, 0x20, 0xED, 0x20, 0xEE, 0x20, 0xEF, 0x20 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_2_LonelyStart_322",
					"\u0000\xE0 \u0000\xE1 \u0000\xE2 \u0000\xE3 \u0000\xE4 \u0000\xE5 \u0000\xE6 \u0000\xE7 " +
					"\u0000\xE8 \u0000\xE9 \u0000\xEA \u0000\xEB \u0000\xEC \u0000\xED \u0000\xEE \u0000\xEF ",
					s
					);
			AssertEquals ("Reconverted-T3_Malformed_2_LonelyStart_322",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_2_LonelyStart_323 () 
		{
			byte[] data = { 0xF0, 0x20, 0xF1, 0x20, 0xF2, 0x20, 0xF3, 0x20, 0xF4, 0x20, 0xF5, 0x20, 0xF6, 0x20, 0xF7, 0x20 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_2_LonelyStart_323",
					"\u0000\xF0 \u0000\xF1 \u0000\xF2 \u0000\xF3 \u0000\xF4 \u0000\xF5 \u0000\xF6 \u0000\xF7 ",
					s
					);
			AssertEquals ("Reconverted-T3_Malformed_2_LonelyStart_323",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_2_LonelyStart_324 () 
		{
			byte[] data = { 0xF0, 0x20, 0xF1, 0x20, 0xF2, 0x20, 0xF3, 0x20, 0xF4, 0x20, 0xF5, 0x20, 0xF6, 0x20, 0xF7, 0x20 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_2_LonelyStart_324",
					"\u0000\xF0 \u0000\xF1 \u0000\xF2 \u0000\xF3 \u0000\xF4 \u0000\xF5 \u0000\xF6 \u0000\xF7 ",
					s
					);
			AssertEquals ("Reconverted-T3_Malformed_2_LonelyStart_324",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_2_LonelyStart_325 () 
		{
			byte[] data = { 0xFC, 0x20, 0xFD, 0x20 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_2_LonelyStart_324", "\u0000\xFC \u0000\xFD ", s);
			AssertEquals ("Reconverted-T3_Malformed_2_LonelyStart_324",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_331 () 
		{
			byte[] data = { 0xC0 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_331", "\u0000\xC0", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_331",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_332 () 
		{
			byte[] data = { 0xE0, 0x80 };
			string s = unix.GetString (data);
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_333 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_333", 
					"\u0000\xF0\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_333",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_334 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_334", 
					"\u0000\xF8\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_334",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_335 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_335", 
					"\u0000\xFC\u0000\x80\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_335",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_336 () 
		{
			byte[] data = { 0xDF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_336", 
					"\u0000\xDF", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_336",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_337 () 
		{
			byte[] data = { 0xEF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_337", 
					"\u0000\xEF\u0000\xBF", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_337",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_338 () 
		{
			byte[] data = { 0xF7, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_3_LastContinuationMissing_338", 
					"\u0000\xF7\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-T3_Malformed_3_LastContinuationMissing_338",
					BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_339 () 
		{
			byte[] data = { 0xF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-339", 
					"\xF\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-339", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_3_LastContinuationMissing_3310 () 
		{
			byte[] data = { 0xFD, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-3310", 
					"\u0000\xFD\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-3310", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_4_ConcatenationImcomplete () 
		{
			byte[] data = {
				0xC0, 0xE0, 0x80, 0xF0, 0x80, 0x80, 0xF8, 0x80, 0x80, 0x80, 0xFC, 0x80, 0x80, 0x80, 0x80, 0xDF, 
				0xEF, 0xBF, 0xF7, 0xBF, 0xBF, 0xFB, 0xBF, 0xBF, 0xBF, 0xFD, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T3_Malformed_4_ConcatenationImcomplete", 
					"\u0000\xC0\u0000\xE0\u0000\x80\u0000\xF0\u0000\x80\u0000\x80\u0000\xF8\u0000\x80" +
					"\u0000\x80\u0000\x80\u0000\xFC\u0000\x80\u0000\x80\u0000\x80\u0000\x80\u0000\xDF" +
					"\u0000\xEF\u0000\xBF\u0000\xF7\u0000\xBF\u0000\xBF\u0000\xFB\u0000\xBF\u0000\xBF" +
					"\u0000\xBF\u0000\xFD\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF",
					s);
			AssertEquals ("Reconverted-T3_Malformed_4_ConcatenationImcomplete", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_5_ImpossibleBytes_351 () 
		{
			byte[] data = { 0xFE };
			string s = unix.GetString (data);
			AssertEquals ("Output-351", "\u0000\xFE", s);
			AssertEquals ("Reconverted-351", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_5_ImpossibleBytes_352 () 
		{
			byte[] data = { 0xFF };
			string s = unix.GetString (data);
			AssertEquals ("Output-352", "\u0000\xFF", s);
			AssertEquals ("Reconverted-352", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T3_Malformed_5_ImpossibleBytes_353 () 
		{
			byte[] data = { 0xFE, 0xFE, 0xFF, 0xFF };
			string s = unix.GetString (data);
			AssertEquals ("Output-352", "\u0000\xFE\u0000\xFE\u0000\xFF\u0000\xFF", s);
			AssertEquals ("Reconverted-352", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		// Overlong == dangereous -> "safe" decoder should reject them

		[Test]
		public void T4_Overlong_1_ASCII_Slash_411 () 
		{
			byte[] data = { 0xC0, 0xAF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_1_ASCII_Slash_411", 
					"\u0000\xC0\u0000\xAF", s);
			AssertEquals ("Reconverted-T4_Overlong_1_ASCII_Slash_411", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_1_ASCII_Slash_412 () 
		{
			byte[] data = { 0xE0, 0x80, 0xAF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_1_ASCII_Slash_413", 
					"\u0000\xE0\u0000\x80\u0000\xAF", s);
			AssertEquals ("Reconverted-T4_Overlong_1_ASCII_Slash_413", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_1_ASCII_Slash_413 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80, 0xAF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_1_ASCII_Slash_412", 
					"\u0000\xF0\u0000\x80\u0000\x80\u0000\xAF", s);
			AssertEquals ("Reconverted-T4_Overlong_1_ASCII_Slash_412", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_1_ASCII_Slash_414 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80, 0xAF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_1_ASCII_Slash_414", 
					"\u0000\xF8\u0000\x80\u0000\x80\u0000\x80\u0000\xAF", s);
			AssertEquals ("Reconverted-T4_Overlong_1_ASCII_Slash_414", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_1_ASCII_Slash_415 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80, 0xAF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_1_ASCII_Slash_415", 
					"\u0000\xFC\u0000\x80\u0000\x80\u0000\x80\u0000\x80\u0000\xAF", s);
			AssertEquals ("Reconverted-T4_Overlong_1_ASCII_Slash_415", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_2_MaximumBoundary_421 () 
		{
			byte[] data = { 0xC1, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_2_MaximumBoundary_421", 
					"\u0000\xC1\u0000\xBF", s);
			AssertEquals ("Reconverted-T4_Overlong_2_MaximumBoundary_421", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_2_MaximumBoundary_422 () 
		{
			byte[] data = { 0xE0, 0x9F, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_2_MaximumBoundary_422", 
					"\u0000\xE0\u0000\x9F\u0000\xBF", s);
			AssertEquals ("Reconverted-T4_Overlong_2_MaximumBoundary_422", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_2_MaximumBoundary_423 () 
		{
			byte[] data = { 0xF0, 0x8F, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_2_MaximumBoundary_423", 
					"\u0000\xF0\u0000\x8F\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-T4_Overlong_2_MaximumBoundary_423", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_2_MaximumBoundary_424 () 
		{
			byte[] data = { 0xF8, 0x87, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_2_MaximumBoundary_424", 
					"\u0000\xF8\u0000\x87\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-T4_Overlong_2_MaximumBoundary_424", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_2_MaximumBoundary_425 () 
		{
			byte[] data = { 0xFC, 0x83, 0xBF, 0xBF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_2_MaximumBoundary_425", 
					"\u0000\xFC\u0000\x83\u0000\xBF\u0000\xBF\u0000\xBF\u0000\xBF", s);
			AssertEquals ("Reconverted-T4_Overlong_2_MaximumBoundary_425", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_3_NUL_431 () 
		{
			byte[] data = { 0xC0, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_3_NUL_431", 
					"\u0000\xC0\u0000\x80", s);
			AssertEquals ("Reconverted-T4_Overlong_3_NUL_431", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_3_NUL_432 () 
		{
			byte[] data = { 0xE0, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_3_NUL_432", 
					"\u0000\xE0\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T4_Overlong_3_NUL_432", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_3_NUL_433 () 
		{
			byte[] data = { 0xF0, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_3_NUL_433", 
					"\u0000\xF0\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T4_Overlong_3_NUL_433", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_3_NUL_434 () 
		{
			byte[] data = { 0xF8, 0x80, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_3_NUL_434", 
					"\u0000\xF8\u0000\x80\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T4_Overlong_3_NUL_434", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
		public void T4_Overlong_3_NUL_435 () 
		{
			byte[] data = { 0xFC, 0x80, 0x80, 0x80, 0x80, 0x80 };
			string s = unix.GetString (data);
			AssertEquals ("Output-T4_Overlong_3_NUL_434", 
					"\u0000\xFC\u0000\x80\u0000\x80\u0000\x80\u0000\x80\u0000\x80", s);
			AssertEquals ("Reconverted-T4_Overlong_3_NUL_434", BitConverter.ToString (data), BitConverter.ToString (unix.GetBytes (s)));
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_511 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 55296, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_512 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56191, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_513 ()
		{
			byte[] data = { 0xED, 0xAE, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56192, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_514 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56319, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_515 ()
		{
			byte[] data = { 0xED, 0xB0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56320, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_516 () 
		{
			byte[] data = { 0xED, 0xBE, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 57216, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_1_UTF16Surrogates_517 () 
		{
			byte[] data = { 0xED, 0xBF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 57343, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_521 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80, 0xED, 0xB0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 55296, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 56320, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_522 () 
		{
			byte[] data = { 0xED, 0xA0, 0x80, 0xED, 0xBF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 55296, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 57343, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_523 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF, 0xED, 0xB0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56191, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 56320, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_524 () 
		{
			byte[] data = { 0xED, 0xAD, 0xBF, 0xED, 0xBF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56191, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 57343, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_525 () 
		{
			byte[] data = { 0xED, 0xAE, 0x80, 0xED, 0xB0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56192, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 56320, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_526 () 
		{
			byte[] data = { 0xED, 0xAE, 0x80, 0xED, 0xBF, 0x8F };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56192, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 57295, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_527 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF, 0xED, 0xB0, 0x80 };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56319, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 56320, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_2_PairedUTF16Surrogates_528 () 
		{
			byte[] data = { 0xED, 0xAF, 0xBF, 0xED, 0xBF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 56319, s [0]);
			AssertEquals ("MS FX 1.1 behaviour", 57343, s [1]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_3_Other_531 () 
		{
			byte[] data = { 0xEF, 0xBF, 0xBE };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 65534, s [0]);
		}

		[Test]
// MS Fx 1.1 accept this
//		[ExpectedException (typeof (ArgumentException))]
		public void T5_IllegalCodePosition_3_Other_532 () 
		{
			byte[] data = { 0xEF, 0xBF, 0xBF };
			string s = unix.GetString (data);
			// exception is "really" expected here
			AssertEquals ("MS FX 1.1 behaviour", 65535, s [0]);
		}

		[Test]
		// bug #75065 and #73086.
		public void GetCharsFEFF ()
		{
			byte [] data = new byte [] {0xEF, 0xBB, 0xBF};
			Encoding enc = new UnixEncoding ();
			string s = enc.GetString (data);
			AssertEquals ("\uFEFF", s);

			Encoding utf = enc;
			char[] testChars = {'\uFEFF','A'};

			byte[] bytes = utf.GetBytes(testChars);
			char[] chars = utf.GetChars(bytes);
			AssertEquals ("#1", '\uFEFF', chars [0]);
			AssertEquals ("#2", 'A', chars [1]);
		}

		[Test]
		public void BinaryFilename ()
		{
			Compare ("BinaryFilename",
				"test\u0000\xffname",
				new byte[]{
					(byte) 't',
					(byte) 'e',
					(byte) 's',
					(byte) 't',
					(byte) 0xff,
					(byte) 'n',
					(byte) 'a',
					(byte) 'm',
					(byte) 'e',
				}
			);
		}

		[Test]
		public void SjisFilename ()
		{
			string fn = 
				"\u0000\x83\x4a\u0000\x83\u0000\x81\u0000\x83\x6e\u0000\x83\u0000\x81\u0000\x83\x6e.txt";
			Compare ("SjisFilename",
				fn,
				new byte[]{
					(byte) 0x83,
					(byte) 0x4a,
					(byte) 0x83,
					(byte) 0x81,
					(byte) 0x83,
					(byte) 0x6e,
					(byte) 0x83,
					(byte) 0x81,
					(byte) 0x83,
					(byte) 0x6e,
					(byte) 0x2e,
					(byte) 0x74,
					(byte) 0x78,
					(byte) 0x74,
				}
			);
		}

		[Test]
		public void SjisFilename2 ()
		{
			string fn = 
				"\u0000\x83\x4a\u0000\x83\u0000\x81\u0000\x83\x6e\u0000\x83\u0000\x81\u0000\x83\x6e.txt";
			Compare ("SjisFilename2",
				"/home/jon/" + fn + "/baz",
				new byte[]{
					(byte) '/',
					(byte) 'h',
					(byte) 'o',
					(byte) 'm',
					(byte) 'e',
					(byte) '/',
					(byte) 'j',
					(byte) 'o',
					(byte) 'n',
					(byte) '/',

					(byte) 0x83,
					(byte) 0x4a,
					(byte) 0x83,
					(byte) 0x81,
					(byte) 0x83,
					(byte) 0x6e,
					(byte) 0x83,
					(byte) 0x81,
					(byte) 0x83,
					(byte) 0x6e,
					(byte) 0x2e,
					(byte) 0x74,
					(byte) 0x78,
					(byte) 0x74,

					(byte) '/',
					(byte) 'b',
					(byte) 'a',
					(byte) 'z',
				}
			);
		}

		private void Compare (string prefix, string start, byte[] end)
		{
			byte[] bytes = unix.GetBytes (start);

			AssertEquals (prefix + ": byte length", end.Length, bytes.Length);

			for (int i = 0; i < Math.Min (bytes.Length, end.Length); ++i)
				AssertEquals (prefix + ": byte " + i, end [i], bytes [i]);

			int cc = unix.GetCharCount (end, 0, end.Length);
			AssertEquals (prefix + ": char count", start.Length, cc);

			char[] chars = new char [cc];
			int r = unix.GetChars (end, 0, end.Length, chars, 0);

			AssertEquals (prefix + ": chars length", start.Length, r);

			for (int i = 0; i < Math.Min (r, start.Length); ++i) {
				AssertEquals (prefix + ": char " + i, start [i], chars [i]);
			}
		}
	}
}
