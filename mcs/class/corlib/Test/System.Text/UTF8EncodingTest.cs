// UTF8EncodingTest.cs - NUnit Test Cases for System.Text.UTF8Encoding
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
        public class UTF8EncodingTest 
        {
                [SetUp]
                public void GetReady() {}
                
                [TearDown]
                public void Clean() {}
                
                [Test]
                public void TestEncodingGetBytes1()
                {
                        UTF8Encoding utf8Enc = new UTF8Encoding ();
                        string UniCode = "\u0041\u2262\u0391\u002E";
                        
                        // "A<NOT IDENTICAL TO><ALPHA>." may be encoded as 41 E2 89 A2 CE 91 2E 
                        // see (RFC 2044)
                        byte[] utf8Bytes = utf8Enc.GetBytes (UniCode);
                        
                        Assertion.AssertEquals ("UTF #1", 0x41, utf8Bytes [0]);
                        Assertion.AssertEquals ("UTF #2", 0xE2, utf8Bytes [1]);
                        Assertion.AssertEquals ("UTF #3", 0x89, utf8Bytes [2]);
                        Assertion.AssertEquals ("UTF #4", 0xA2, utf8Bytes [3]);
                        Assertion.AssertEquals ("UTF #5", 0xCE, utf8Bytes [4]);
                        Assertion.AssertEquals ("UTF #6", 0x91, utf8Bytes [5]);
                        Assertion.AssertEquals ("UTF #7", 0x2E, utf8Bytes [6]);
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
                        
                        Assertion.AssertEquals ("UTF #1", 11, ByteCnt);
                        Assertion.AssertEquals ("UTF #2", 0x48, utf8Bytes [0]);
                        Assertion.AssertEquals ("UTF #3", 0x69, utf8Bytes [1]);
                        Assertion.AssertEquals ("UTF #4", 0x20, utf8Bytes [2]);
                        Assertion.AssertEquals ("UTF #5", 0x4D, utf8Bytes [3]);
                        Assertion.AssertEquals ("UTF #6", 0x6F, utf8Bytes [4]);
                        Assertion.AssertEquals ("UTF #7", 0x6D, utf8Bytes [5]);
                        Assertion.AssertEquals ("UTF #8", 0x20, utf8Bytes [6]);
                        Assertion.AssertEquals ("UTF #9", 0xE2, utf8Bytes [7]);
                        Assertion.AssertEquals ("UTF #10", 0x98, utf8Bytes [8]);
                        Assertion.AssertEquals ("UTF #11", 0xBA, utf8Bytes [9]);
                        Assertion.AssertEquals ("UTF #12", 0x21, utf8Bytes [10]);
                }
        
                [Test]
                public void TestDecodingGetChars1()
                {
                        UTF8Encoding utf8Enc = new UTF8Encoding ();
                        // 41 E2 89 A2 CE 91 2E may be decoded as "A<NOT IDENTICAL TO><ALPHA>." 
                        // see (RFC 2044)
                        byte[] utf8Bytes = new byte [] {0x41, 0xE2, 0x89, 0xA2, 0xCE, 0x91, 0x2E};
                        char[] UniCodeChars = utf8Enc.GetChars(utf8Bytes);
                             
                        Assertion.AssertEquals ("UTF #1", 0x0041, UniCodeChars [0]);
                        Assertion.AssertEquals ("UTF #2", 0x2262, UniCodeChars [1]);
                        Assertion.AssertEquals ("UTF #3", 0x0391, UniCodeChars [2]);
                        Assertion.AssertEquals ("UTF #4", 0x002E, UniCodeChars [3]);
                }
                
                [Test]
                public void TestMaxCharCount()
                {
                        UTF8Encoding UTF8enc = new UTF8Encoding ();
                        Assertion.AssertEquals ("UTF #1", 50, UTF8enc.GetMaxCharCount(50));
                }
        
                [Test]
                public void TestMaxByteCount()
                {
                        UTF8Encoding UTF8enc = new UTF8Encoding ();
                        Assertion.AssertEquals ("UTF #1", 200, UTF8enc.GetMaxByteCount(50));
                }

                
        }
}



