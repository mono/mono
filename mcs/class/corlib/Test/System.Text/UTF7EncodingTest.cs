//
// UTF7EncodingTest.cs - NUnit Test Cases for System.Text.UTF7Encoding
//
// Authors
//	Patrick Kalkman  kalkman@cistron.nl
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Patrick Kalkman
// Copyright (C) 2004 Novell (http://www.novell.com)
//
 
using NUnit.Framework;
using System;
using System.Text;

using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Text
{
        [TestFixture]
        public class UTF7EncodingTest 
        {
		[Test]
		public void IsBrowserDisplay ()
		{
			UTF7Encoding utf7 = new UTF7Encoding ();
			Assert.IsTrue (!utf7.IsBrowserDisplay);
		}

		[Test]
		public void IsBrowserSave ()
		{
			UTF7Encoding utf7 = new UTF7Encoding ();
			Assert.IsTrue (!utf7.IsBrowserSave);
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			UTF7Encoding utf7 = new UTF7Encoding ();
			Assert.IsTrue (utf7.IsMailNewsDisplay);
		}

		[Test]
		public void IsMailNewsSave ()
		{
			UTF7Encoding utf7 = new UTF7Encoding ();
			Assert.IsTrue (utf7.IsMailNewsSave);
		}

                [Test]
                public void TestDirectlyEncoded1() 
                {
                        // Unicode characters a-z, A-Z, 0-9 and '()_./:? are directly encoded.
                        string UniCodeString = "\u0061\u007A\u0041\u005A\u0030\u0039\u0027\u003F";
                        byte[] UTF7Bytes = null;
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assert.AreEqual (0x61, UTF7Bytes [0], "UTF7 #1");
                        Assert.AreEqual (0x7A, UTF7Bytes [1], "UTF7 #2");
                        Assert.AreEqual (0x41, UTF7Bytes [2], "UTF7 #3");
                        Assert.AreEqual (0x5A, UTF7Bytes [3], "UTF7 #4");
                        Assert.AreEqual (0x30, UTF7Bytes [4], "UTF7 #5");
                        Assert.AreEqual (0x39, UTF7Bytes [5], "UTF7 #6");
                        Assert.AreEqual (0x27, UTF7Bytes [6], "UTF7 #7");
                        Assert.AreEqual (0x3F, UTF7Bytes [7], "UTF7 #8");
                }
        
                [Test]
                public void TestDirectlyEncoded2()
                {
                        // Unicode characters a-z, A-Z, 0-9 and '()_./:? are directly encoded.
                        string UniCodeString = "\u0061\u007A\u0041\u005A\u0030\u0039\u0027\u003F";
                        byte[] UTF7Bytes = new byte [8];
                        int Length = UniCodeString.Length;
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        
                        int Cnt = UTF7enc.GetBytes (UniCodeString.ToCharArray(), 0, Length, UTF7Bytes, 0);
                
		        Assert.AreEqual (0x61, UTF7Bytes [0], "UTF7 #1");
                        Assert.AreEqual (0x7A, UTF7Bytes [1], "UTF7 #2");
                        Assert.AreEqual (0x41, UTF7Bytes [2], "UTF7 #3");
                        Assert.AreEqual (0x5A, UTF7Bytes [3], "UTF7 #4");
                        Assert.AreEqual (0x30, UTF7Bytes [4], "UTF7 #5");
                        Assert.AreEqual (0x39, UTF7Bytes [5], "UTF7 #6");
                        Assert.AreEqual (0x27, UTF7Bytes [6], "UTF7 #7");
                        Assert.AreEqual (0x3F, UTF7Bytes [7], "UTF7 #8");
		}
        
                [Test]
                public void TestEncodeOptionalEncoded()
                {
                        string UniCodeString = "\u0021\u0026\u002A\u003B";
                        byte[] UTF7Bytes = null;
                        
                        //Optional Characters are allowed.	
                        UTF7Encoding UTF7enc = new UTF7Encoding (true); 
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assert.AreEqual (0x21, UTF7Bytes [0], "UTF7 #1");
                        Assert.AreEqual (0x26, UTF7Bytes [1], "UTF7 #2");
                        Assert.AreEqual (0x2A, UTF7Bytes [2], "UTF7 #3");
                        Assert.AreEqual (0x3B, UTF7Bytes [3], "UTF7 #4");
                        
                        //Optional characters are not allowed.
                        UTF7enc = new UTF7Encoding (false);
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assert.AreEqual (0x2B, UTF7Bytes [0], "UTF7 #5");
                        Assert.AreEqual (0x41, UTF7Bytes [1], "UTF7 #6");
                        Assert.AreEqual (0x43, UTF7Bytes [2], "UTF7 #7");
                        Assert.AreEqual (0x45, UTF7Bytes [3], "UTF7 #8");
                        Assert.AreEqual (0x41, UTF7Bytes [1], "UTF7 #6");
                }
        
                [Test]
                public void TestEncodeUnicodeShifted1()
                {
                        string UniCodeString = "\u0041\u2262\u0391\u002E";
                        byte[] UTF7Bytes = null;
                        
                        UTF7Encoding UTF7enc = new UTF7Encoding();
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        //"A<NOT IDENTICAL TO><ALPHA>." is encoded as A+ImIDkQ-. see RFC 1642
                        Assert.AreEqual (0x41, UTF7Bytes [0], "UTF7 #1");
                        Assert.AreEqual (0x2B, UTF7Bytes [1], "UTF7 #2");
                        Assert.AreEqual (0x49, UTF7Bytes [2], "UTF7 #3");
                        Assert.AreEqual (0x6D, UTF7Bytes [3], "UTF7 #4");
                        Assert.AreEqual (0x49, UTF7Bytes [4], "UTF7 #5");
                        Assert.AreEqual (0x44, UTF7Bytes [5], "UTF7 #6");
                        Assert.AreEqual (0x6B, UTF7Bytes [6], "UTF7 #7");
                        Assert.AreEqual (0x51, UTF7Bytes [7], "UTF7 #8");
                        Assert.AreEqual (0x2D, UTF7Bytes [8], "UTF7 #9");
                        Assert.AreEqual (0x2E, UTF7Bytes [9], "UTF7 #10");
                }
        
                [Test]
                public void TestEncodeUnicodeShifted2()
                {
                        string UniCodeString = "\u0041\u2262\u0391\u002E";
                        byte[] UTF7Bytes = new byte [10];
                        int Length = UniCodeString.Length;
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        
                        int Cnt = UTF7enc.GetBytes (UniCodeString.ToCharArray(), 0, Length, UTF7Bytes, 0);
                        
                        //"A<NOT IDENTICAL TO><ALPHA>." is encoded as A+ImIDkQ-. see RFC 1642
                        Assert.AreEqual (0x41, UTF7Bytes [0], "UTF7 #1");
                        Assert.AreEqual (0x2B, UTF7Bytes [1], "UTF7 #2");
                        Assert.AreEqual (0x49, UTF7Bytes [2], "UTF7 #3");
                        Assert.AreEqual (0x6D, UTF7Bytes [3], "UTF7 #4");
                        Assert.AreEqual (0x49, UTF7Bytes [4], "UTF7 #5");
                        Assert.AreEqual (0x44, UTF7Bytes [5], "UTF7 #6");
                        Assert.AreEqual (0x6B, UTF7Bytes [6], "UTF7 #7");
                        Assert.AreEqual (0x51, UTF7Bytes [7], "UTF7 #8");
                        Assert.AreEqual (0x2D, UTF7Bytes [8], "UTF7 #9");
                        Assert.AreEqual (0x2E, UTF7Bytes [9], "UTF7 #10");
                }
        
		[Test]
		public void RFC1642_Example1 ()
		{
			string UniCodeString = "\u0041\u2262\u0391\u002E";
			char[] expected = UniCodeString.ToCharArray ();

			byte[] UTF7Bytes = new byte [] {0x41, 0x2B, 0x49, 0x6D, 0x49, 0x44, 0x6B,  0x51, 0x2D, 0x2E};
			UTF7Encoding UTF7enc = new UTF7Encoding ();
			char[] actual = UTF7enc.GetChars (UTF7Bytes);

			// "A+ImIDkQ-." is decoded as "A<NOT IDENTICAL TO><ALPHA>." see RFC 1642
			Assert.AreEqual (expected [0], actual [0], "UTF #1");
			Assert.AreEqual (expected [1], actual [1], "UTF #2");
			Assert.AreEqual (expected [2], actual [2], "UTF #3");
			Assert.AreEqual (expected [3], actual [3], "UTF #4");

			Assert.AreEqual (UniCodeString, UTF7enc.GetString (UTF7Bytes), "GetString");
                }

		[Test]
		public void RFC1642_Example2 ()
		{
			string UniCodeString = "\u0048\u0069\u0020\u004D\u006F\u004D\u0020\u263A\u0021";
			char[] expected = UniCodeString.ToCharArray ();

			byte[] UTF7Bytes = new byte[] { 0x48, 0x69, 0x20, 0x4D, 0x6F, 0x4D, 0x20, 0x2B, 0x4A, 0x6A, 0x6F, 0x41, 0x49, 0x51, 0x2D };

			UTF7Encoding UTF7enc = new UTF7Encoding ();
			char[] actual = UTF7enc.GetChars (UTF7Bytes);

			// "Hi Mom +Jjo-!" is decoded as "Hi Mom <WHITE SMILING FACE>!"
			Assert.AreEqual (expected [0], actual [0], "UTF #1");
			Assert.AreEqual (expected [1], actual [1], "UTF #2");
			Assert.AreEqual (expected [2], actual [2], "UTF #3");
			Assert.AreEqual (expected [3], actual [3], "UTF #4");
			Assert.AreEqual (expected [4], actual [4], "UTF #5");
			Assert.AreEqual (expected [5], actual [5], "UTF #6");
			Assert.AreEqual (expected [6], actual [6], "UTF #7");
			Assert.AreEqual (expected [7], actual [7], "UTF #8");
			Assert.AreEqual (expected [8], actual [8], "UTF #9");

			Assert.AreEqual (UniCodeString, UTF7enc.GetString (UTF7Bytes), "GetString");
		}

		[Test]
		public void RFC1642_Example3 ()
		{
			string UniCodeString = "\u65E5\u672C\u8A9E";
			char[] expected = UniCodeString.ToCharArray ();

			byte[] UTF7Bytes = new byte[] { 0x2B, 0x5A, 0x65, 0x56, 0x6E, 0x4C, 0x49, 0x71, 0x65, 0x2D };

			UTF7Encoding UTF7enc = new UTF7Encoding ();
			char[] actual = UTF7enc.GetChars (UTF7Bytes);

			// "+ZeVnLIqe-" is decoded as Japanese "nihongo"
			Assert.AreEqual (expected [0], actual [0], "UTF #1");
			Assert.AreEqual (expected [1], actual [1], "UTF #2");
			Assert.AreEqual (expected [2], actual [2], "UTF #3");

			Assert.AreEqual (UniCodeString, UTF7enc.GetString (UTF7Bytes), "GetString");
		}

		[Test]
		public void RFC1642_Example4 ()
		{
			string UniCodeString = "\u0049\u0074\u0065\u006D\u0020\u0033\u0020\u0069\u0073\u0020\u00A3\u0031\u002E";
			char[] expected = UniCodeString.ToCharArray ();

			byte[] UTF7Bytes = new byte[] { 0x49, 0x74, 0x65, 0x6D, 0x20, 0x33, 0x20, 0x69, 0x73, 0x20, 0x2B, 0x41, 0x4B, 0x4D, 0x2D, 0x31, 0x2E };

			UTF7Encoding UTF7enc = new UTF7Encoding ();
			char[] actual = UTF7enc.GetChars (UTF7Bytes);

			// "Item 3 is +AKM-1." is decoded as "Item 3 is <POUND SIGN>1."
			Assert.AreEqual (expected [0], actual [0], "UTF #1");
			Assert.AreEqual (expected [1], actual [1], "UTF #2");
			Assert.AreEqual (expected [2], actual [2], "UTF #3");
			Assert.AreEqual (expected [3], actual [3], "UTF #4");
			Assert.AreEqual (expected [4], actual [4], "UTF #5");
			Assert.AreEqual (expected [5], actual [5], "UTF #6");
			Assert.AreEqual (expected [6], actual [6], "UTF #7");
			Assert.AreEqual (expected [7], actual [7], "UTF #8");
			Assert.AreEqual (expected [8], actual [8], "UTF #9");
			Assert.AreEqual (expected [9], actual [9], "UTF #10");
			Assert.AreEqual (expected [10], actual [10], "UTF #11");
			Assert.AreEqual (expected [11], actual [11], "UTF #12");
			Assert.AreEqual (expected [12], actual [12], "UTF #13");

			Assert.AreEqual (UniCodeString, UTF7enc.GetString (UTF7Bytes), "GetString");
		}

                [Test]
                public void TestMaxCharCount()
                {
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        Assert.AreEqual (50, UTF7enc.GetMaxCharCount(50), "UTF #1");
                }
        
                [Test]
#if NET_2_0
                [Category ("NotWorking")]
#endif
                public void TestMaxByteCount()
                {
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
#if NET_2_0
                        Assert.AreEqual (152, UTF7enc.GetMaxByteCount(50), "UTF #1");
#else
                        Assert.AreEqual (136, UTF7enc.GetMaxByteCount(50), "UTF #1");
#endif
                }

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotDotNet")] // MS bug
		public void Bug77315 ()
		{
			string s = new UTF7Encoding ().GetString (
				Encoding.ASCII.GetBytes ("+2AA-"));
		}

		[Test]
		public void GetCharCount ()
		{
			string original = "*123456789*123456789*123456789*123456789*123456789*123456789*123456789*123456789";
			byte [] bytes = Encoding.UTF7.GetBytes (original);
			AssertType.AreEqual (112, bytes.Length, "#1");
			AssertType.AreEqual (80, Encoding.UTF7.GetCharCount (bytes), "#2");
			string decoded = Encoding.UTF7.GetString(Encoding.UTF7.GetBytes(original));
			AssertType.AreEqual (original, decoded, "#3");
		}
        }
}



