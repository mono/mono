// UTF7EncodingTest.cs - NUnit Test Cases for System.Text.UTF7Encoding
//
// Patrick Kalkman  kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
// 
using NUnit.Framework;
using System;
using System.Text;

namespace MonoTests.System.Text
{

        [TestFixture]
        public class UTF7EncodingTest 
        {
                [SetUp]
                public void GetReady() {}
                
                [TearDown]
                public void Clean() {}
                
                [Test]
                public void TestDirectlyEncoded1() 
                {
                        // Unicode characters a-z, A-Z, 0-9 and '()_./:? are directly encoded.
                        string UniCodeString = "\u0061\u007A\u0041\u005A\u0030\u0039\u0027\u003F";
                        byte[] UTF7Bytes = null;
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assertion.AssertEquals ("UTF7 #1", 0x61, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #2", 0x7A, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #3", 0x41, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #4", 0x5A, UTF7Bytes [3]);
                        Assertion.AssertEquals ("UTF7 #5", 0x30, UTF7Bytes [4]);
                        Assertion.AssertEquals ("UTF7 #6", 0x39, UTF7Bytes [5]);
                        Assertion.AssertEquals ("UTF7 #7", 0x27, UTF7Bytes [6]);
                        Assertion.AssertEquals ("UTF7 #8", 0x3F, UTF7Bytes [7]);
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
                
		        Assertion.AssertEquals ("UTF7 #1", 0x61, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #2", 0x7A, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #3", 0x41, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #4", 0x5A, UTF7Bytes [3]);
                        Assertion.AssertEquals ("UTF7 #5", 0x30, UTF7Bytes [4]);
                        Assertion.AssertEquals ("UTF7 #6", 0x39, UTF7Bytes [5]);
                        Assertion.AssertEquals ("UTF7 #7", 0x27, UTF7Bytes [6]);
                        Assertion.AssertEquals ("UTF7 #8", 0x3F, UTF7Bytes [7]);
		}
        
                [Test]
                public void TestEncodeOptionalEncoded()
                {
                        string UniCodeString = "\u0021\u0026\u002A\u003B";
                        byte[] UTF7Bytes = null;
                        
                        //Optional Characters are allowed.	
                        UTF7Encoding UTF7enc = new UTF7Encoding (true); 
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assertion.AssertEquals ("UTF7 #1", 0x21, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #2", 0x26, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #3", 0x2A, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #4", 0x3B, UTF7Bytes [3]);
                        
                        //Optional characters are not allowed.
                        UTF7enc = new UTF7Encoding (false);
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        Assertion.AssertEquals ("UTF7 #5", 0x2B, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #6", 0x41, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #7", 0x43, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #8", 0x45, UTF7Bytes [3]);
                        Assertion.AssertEquals ("UTF7 #6", 0x41, UTF7Bytes [1]);
                }
        
                [Test]
                public void TestEncodeUnicodeShifted1()
                {
                        string UniCodeString = "\u0041\u2262\u0391\u002E";
                        byte[] UTF7Bytes = null;
                        
                        UTF7Encoding UTF7enc = new UTF7Encoding();
                        UTF7Bytes = UTF7enc.GetBytes (UniCodeString);
                        
                        //"A<NOT IDENTICAL TO><ALPHA>." is encoded as A+ImIDkQ-. see RFC 1642
                        Assertion.AssertEquals ("UTF7 #1", 0x41, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #2", 0x2B, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #3", 0x49, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #4", 0x6D, UTF7Bytes [3]);
                        Assertion.AssertEquals ("UTF7 #5", 0x49, UTF7Bytes [4]);
                        Assertion.AssertEquals ("UTF7 #6", 0x44, UTF7Bytes [5]);
                        Assertion.AssertEquals ("UTF7 #7", 0x6B, UTF7Bytes [6]);
                        Assertion.AssertEquals ("UTF7 #8", 0x51, UTF7Bytes [7]);
                        Assertion.AssertEquals ("UTF7 #9", 0x2D, UTF7Bytes [8]);
                        Assertion.AssertEquals ("UTF7 #10", 0x2E, UTF7Bytes [9]);
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
                        Assertion.AssertEquals ("UTF7 #1", 0x41, UTF7Bytes [0]);
                        Assertion.AssertEquals ("UTF7 #2", 0x2B, UTF7Bytes [1]);
                        Assertion.AssertEquals ("UTF7 #3", 0x49, UTF7Bytes [2]);
                        Assertion.AssertEquals ("UTF7 #4", 0x6D, UTF7Bytes [3]);
                        Assertion.AssertEquals ("UTF7 #5", 0x49, UTF7Bytes [4]);
                        Assertion.AssertEquals ("UTF7 #6", 0x44, UTF7Bytes [5]);
                        Assertion.AssertEquals ("UTF7 #7", 0x6B, UTF7Bytes [6]);
                        Assertion.AssertEquals ("UTF7 #8", 0x51, UTF7Bytes [7]);
                        Assertion.AssertEquals ("UTF7 #9", 0x2D, UTF7Bytes [8]);
                        Assertion.AssertEquals ("UTF7 #10", 0x2E, UTF7Bytes [9]);
                }
        
                [Test]
                public void TestDecodeUnicodeShifted1()
                {
                        string UniCodeString = "\u0041\u2262\u0391\u002E";
                        byte[] UTF7Bytes = new byte [] {0x41, 0x2B, 0x49, 0x6D, 0x49, 0x44, 0x6B,  0x51, 0x2D, 0x2E};
                        
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        char[] UniCodeChars = UTF7enc.GetChars (UTF7Bytes);
                        //A+ImIDkQ-. is decoded as "A<NOT IDENTICAL TO><ALPHA>." see RFC 1642
                        Assertion.AssertEquals ("UTF #1", UniCodeString.ToCharArray() [0], UniCodeChars [0]);
                        Assertion.AssertEquals ("UTF #2", UniCodeString.ToCharArray() [1], UniCodeChars [1]);
                        Assertion.AssertEquals ("UTF #3", UniCodeString.ToCharArray() [2], UniCodeChars [2]);
                        Assertion.AssertEquals ("UTF #4", UniCodeString.ToCharArray() [3], UniCodeChars [3]);
                }
        
                [Test]
                public void TestMaxCharCount()
                {
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        Assertion.AssertEquals ("UTF #1", 50, UTF7enc.GetMaxCharCount(50));
                }
        
                [Test]
                public void TestMaxByteCount()
                {
                        UTF7Encoding UTF7enc = new UTF7Encoding ();
                        Assertion.AssertEquals ("UTF #1", 136, UTF7enc.GetMaxByteCount(50));
                }
        }
}



