//
// AsnEncodedDataTest.cs - NUnit tests for AsnEncodedData
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class AsnEncodedDataTest : Assertion {

		static byte[] asnNullBytes = { 0x05, 0x00 };
		static string asnNullString = "05 00";
		static byte[] asnLongBytes = { 0x30,0x5C,0x02,0x55,0x2D,0x58,0xE9,0xBF,0xF0,0x31,0xCD,0x79,0x06,0x50,0x5A,0xD5,0x9E,0x0E,0x2C,0xE6,0xC2,0xF7,0xF9,0xD2,0xCE,0x55,0x64,0x85,0xB1,0x90,0x9A,0x92,0xB3,0x36,0xC1,0xBC,0xEA,0xC8,0x23,0xB7,0xAB,0x3A,0xA7,0x64,0x63,0x77,0x5F,0x84,0x22,0x8E,0xE5,0xB6,0x45,0xDD,0x46,0xAE,0x0A,0xDD,0x00,0xC2,0x1F,0xBA,0xD9,0xAD,0xC0,0x75,0x62,0xF8,0x95,0x82,0xA2,0x80,0xB1,0x82,0x69,0xFA,0xE1,0xAF,0x7F,0xBC,0x7D,0xE2,0x7C,0x76,0xD5,0xBC,0x2A,0x80,0xFB,0x02,0x03,0x01,0x00,0x01 };
		static string asnLongString = "30 5c 02 55 2d 58 e9 bf f0 31 cd 79 06 50 5a d5 9e 0e 2c e6 c2 f7 f9 d2 ce 55 64 85 b1 90 9a 92 b3 36 c1 bc ea c8 23 b7 ab 3a a7 64 63 77 5f 84 22 8e e5 b6 45 dd 46 ae 0a dd 00 c2 1f ba d9 ad c0 75 62 f8 95 82 a2 80 b1 82 69 fa e1 af 7f bc 7d e2 7c 76 d5 bc 2a 80 fb 02 03 01 00 01";

		private void AssertEquals (string message, byte[] expected, byte[] actual) 
		{
			AssertEquals (message, BitConverter.ToString (expected), BitConverter.ToString (actual));
		}

		[Test]
		public void ConstructorStringData ()
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", asnNullBytes);
			AssertEquals ("Format", asnNullString, aed.Format (true));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStringNullData () 
		{
			string oid = null; // do not confuse compiler
			AsnEncodedData aed = new AsnEncodedData (oid, asnNullBytes);
			AssertEquals ("Format", asnNullString, aed.Format (true));
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorStringDataNull () 
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", null);
			AssertEquals ("Format", asnNullString, aed.Format (true));
		}

		[Test]
		public void ConstructorOidData () 
		{
			Oid o = new Oid ("1.0");
			AsnEncodedData aed = new AsnEncodedData (o, asnNullBytes);
			AssertEquals ("Format", asnNullString, aed.Format (true));
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorOidNullData () 
		{
			Oid o = null;
			AsnEncodedData aed = new AsnEncodedData (o, asnNullBytes);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorOidDataNull () 
		{
			Oid o = new Oid ("1.0");
			AsnEncodedData aed = new AsnEncodedData (o, null);
		}

		[Test]
		public void ConstructorAsn () 
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", asnNullBytes);
			AsnEncodedData aed2 = new AsnEncodedData (aed);
			AssertEquals ("FriendlyName", aed.RawData, aed2.RawData);
			string s1 = aed.Format (false); 
			string s2 = aed.Format (true);
			AssertEquals ("Format", s1, s2);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnNull ()
		{
			AsnEncodedData aed = new AsnEncodedData (null);
		}

		[Test]
		public void Format () 
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.840.113549.1.1.1", asnLongBytes);
			string result = aed.Format (false);
			AssertEquals ("Format(false)", asnLongString, result);
		}

		[Test]
		public void FormatMultiline ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.840.113549.1.1.1", asnLongBytes);
			string result = aed.Format (true);
			AssertEquals ("Format(true)", asnLongString, result);
		}
	}
}

#endif
