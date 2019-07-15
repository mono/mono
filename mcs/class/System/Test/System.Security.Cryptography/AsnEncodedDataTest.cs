//
// AsnEncodedDataTest.cs - NUnit tests for AsnEncodedData
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using NUnit.Framework;

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class AsnEncodedDataTest {
		static byte[] asnNullBytes = { 0x05, 0x00 };
		static string asnNullString = "05 00";
		static byte[] asnLongBytes = { 0x30,0x5C,0x02,0x55,0x2D,0x58,0xE9,0xBF,0xF0,0x31,0xCD,0x79,0x06,0x50,0x5A,0xD5,0x9E,0x0E,0x2C,0xE6,0xC2,0xF7,0xF9,0xD2,0xCE,0x55,0x64,0x85,0xB1,0x90,0x9A,0x92,0xB3,0x36,0xC1,0xBC,0xEA,0xC8,0x23,0xB7,0xAB,0x3A,0xA7,0x64,0x63,0x77,0x5F,0x84,0x22,0x8E,0xE5,0xB6,0x45,0xDD,0x46,0xAE,0x0A,0xDD,0x00,0xC2,0x1F,0xBA,0xD9,0xAD,0xC0,0x75,0x62,0xF8,0x95,0x82,0xA2,0x80,0xB1,0x82,0x69,0xFA,0xE1,0xAF,0x7F,0xBC,0x7D,0xE2,0x7C,0x76,0xD5,0xBC,0x2A,0x80,0xFB,0x02,0x03,0x01,0x00,0x01 };
		static string asnLongString = "30 5c 02 55 2d 58 e9 bf f0 31 cd 79 06 50 5a d5 9e 0e 2c e6 c2 f7 f9 d2 ce 55 64 85 b1 90 9a 92 b3 36 c1 bc ea c8 23 b7 ab 3a a7 64 63 77 5f 84 22 8e e5 b6 45 dd 46 ae 0a dd 00 c2 1f ba d9 ad c0 75 62 f8 95 82 a2 80 b1 82 69 fa e1 af 7f bc 7d e2 7c 76 d5 bc 2a 80 fb 02 03 01 00 01";

		[Test]
		public void Constructor_StringData ()
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", asnNullBytes);
			Assert.AreEqual ("oid", aed.Oid.Value, "Oid.Value");
			Assert.IsNull (aed.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (BitConverter.ToString (asnNullBytes), BitConverter.ToString (aed.RawData), "RawData");
			Assert.AreEqual (asnNullString, aed.Format (true), "Format");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNullData () 
		{
			string oid = null; // do not confuse compiler
			AsnEncodedData aed = new AsnEncodedData (oid, asnNullBytes);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringDataNull () 
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", null);
		}

		[Test]
		public void Constructor_OidData () 
		{
			Oid o = new Oid ("1.0");
			AsnEncodedData aed = new AsnEncodedData (o, asnNullBytes);
			Assert.AreEqual ("1.0", aed.Oid.Value, "Oid.Value");
			Assert.IsNull (aed.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (BitConverter.ToString (asnNullBytes), BitConverter.ToString (aed.RawData), "RawData");
			Assert.AreEqual (asnNullString, aed.Format (true), "Format");
		}

		[Test]
		public void Constructor_OidNullData () 
		{
			// this is legal - http://lab.msdn.microsoft.com/ProductFeedback/viewfeedback.aspx?feedbackid=38336cfa-3b97-47da-ad4e-9522d557f001
			Oid o = null;
			AsnEncodedData aed = new AsnEncodedData (o, asnNullBytes);
			Assert.IsNull (aed.Oid, "Oid");
			Assert.AreEqual (BitConverter.ToString (asnNullBytes), BitConverter.ToString (aed.RawData), "RawData");
			Assert.AreEqual (asnNullString, aed.Format (true), "Format");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_OidDataNull () 
		{
			Oid o = new Oid ("1.0");
			AsnEncodedData aed = new AsnEncodedData (o, null);
		}

		[Test]
		public void Constructor_Asn () 
		{
			AsnEncodedData aed = new AsnEncodedData ("oid", asnNullBytes);
			AsnEncodedData aed2 = new AsnEncodedData (aed);
			Assert.AreEqual (aed.Oid.Value, aed2.Oid.Value, "Oid.Value");
			Assert.AreEqual (aed.Oid.FriendlyName, aed2.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (BitConverter.ToString (aed.RawData), BitConverter.ToString (aed2.RawData), "RawData");
			string s1 = aed.Format (false); 
			string s2 = aed.Format (true);
			Assert.AreEqual (s1, s2, "Format");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_ByteArrayNull ()
		{
			byte[] array = null;
			AsnEncodedData aed = new AsnEncodedData (array);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_AsnNull ()
		{
			AsnEncodedData asn = null;
			AsnEncodedData aed = new AsnEncodedData (asn);
		}

		[Test]
		public void Oid_CreatedNull ()
		{
			AsnEncodedData aed = new AsnEncodedData ((Oid)null, asnNullBytes);
			Assert.IsNull (aed.Oid, "Oid 1");
			Oid o = new Oid ("1.2.3");
			aed.Oid = o;
			Assert.AreEqual ("1.2.3", aed.Oid.Value, "Oid 2");
			o.Value = "1.2.4";
			Assert.AreEqual ("1.2.3", aed.Oid.Value, "Oid 3"); // didn't change (copy)
			aed.Oid = null;
			Assert.IsNull (aed.Oid, "Oid 4");
		}

		[Test]
		public void Oid ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", asnNullBytes);
			Assert.AreEqual ("1.2.3", aed.Oid.Value, "Oid 1");
			aed.Oid.Value = "1.2.4";
			Assert.AreEqual ("1.2.4", aed.Oid.Value, "Oid 2"); // didn't change (copy)
			aed.Oid = null;
			Assert.IsNull (aed.Oid, "Oid 3");
		}

		[Test]
		public void RawData_CanModify ()
		{
			byte[] data = (byte[])asnNullBytes.Clone ();
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", data);
			Assert.AreEqual (asnNullString, aed.Format (true), "Format 1");
			data[0] = 0x06;
			Assert.AreEqual (asnNullString, aed.Format (true), "Format 2"); ; // didn't change (copy)
			aed.RawData[0] = 0x07;
			Assert.AreEqual ("07 00", aed.Format (true), "Format 3"); // changed!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RawData ()
		{
			AsnEncodedData aed = new AsnEncodedData ((Oid)null, asnNullBytes);
			Assert.AreEqual (asnNullString, aed.Format (true), "Format 1");
			aed.RawData = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			AsnEncodedData aed = new AsnEncodedData ((Oid)null, asnNullBytes);
			aed.CopyFrom (null);
		}

		[Test]
		public void CopyFrom ()
		{
			Oid o = new Oid ("1.2.3");
			byte[] data = (byte[])asnNullBytes.Clone ();
			AsnEncodedData aed = new AsnEncodedData (o, asnNullBytes);
			AsnEncodedData copy = new AsnEncodedData ((Oid)null, new byte [0]);
			copy.CopyFrom (aed);

			Assert.AreEqual (aed.Oid.Value, copy.Oid.Value, "Oid 1");
			Assert.AreEqual (aed.Format (true), copy.Format (true), "Format 1");

			aed.Oid = new Oid ("1.2.4");
			aed.RawData = new byte[1];

			Assert.AreEqual ("1.2.3", copy.Oid.Value, "Oid 2");
			Assert.AreEqual (asnNullString, copy.Format (true), "Format 2");
		}

		[Test]
		public void Format () 
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.840.113549.1.1.1", asnLongBytes);
			Assert.AreEqual ("1.2.840.113549.1.1.1", aed.Oid.Value, "Oid.Value");
			Assert.AreEqual ("RSA", aed.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (BitConverter.ToString (asnLongBytes), BitConverter.ToString (aed.RawData), "RawData");
			string result = aed.Format (false);
			Assert.AreEqual (asnLongString, result, "Format(false)");
		}

		[Test]
		public void FormatMultiline ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.840.113549.1.1.1", asnLongBytes);
			Assert.AreEqual ("1.2.840.113549.1.1.1", aed.Oid.Value, "Oid.Value");
			Assert.AreEqual ("RSA", aed.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (BitConverter.ToString (asnLongBytes), BitConverter.ToString (aed.RawData), "RawData");
			string result = aed.Format (true);
			Assert.AreEqual (asnLongString, result, "Format(true)");
		}

		[Test]
		[Category ("NotDotNet")] // FriendlyName should not only be English.
		public void Build_X509EnhancedKeyUsageExtension ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			Assert.AreEqual ("30 05 06 03 2a 03 04", aed.Format (true), "Format(true)");
			Assert.AreEqual ("30 05 06 03 2a 03 04", aed.Format (false), "Format(false)");
			aed.Oid = new Oid ("2.5.29.37");
			// and now "AsnEncodedData" knows how to (magically) decode the data without involving the class
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, aed.Format (true), "aed.Format(true)");
			Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", aed.Format (false), "aed.Format(false)");
			// compare with the output of the "appropriate" class
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (aed, false);
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, eku.Format (true), "eku.Format(true)");
			Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", eku.Format (false), "eku.Format(false)");
		}

		[Test]
		[Category ("NotDotNet")] // FriendlyName should not only be English.
		// note: important to emulate in Mono because we need it for SSL/TLS
		public void Build_NetscapeCertTypeExtension ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x03, 0x02, 0x01, 0x06 });
			Assert.AreEqual ("03 02 01 06", aed.Format (true), "Format(true)");
			Assert.AreEqual ("03 02 01 06", aed.Format (false), "Format(false)");
			aed.Oid = new Oid ("2.16.840.1.113730.1.1");
			// and now "AsnEncodedData" knows how to (magically) decode the data without involving the class
			Assert.AreEqual ("SSL CA, SMIME CA (06)", aed.Format (true), "aed.Format(true)");
			Assert.AreEqual ("SSL CA, SMIME CA (06)", aed.Format (false), "aed.Format(false)");
			// note that the Fx doesn't "really" support this extension
			// and strangely no NewLine is being appended to Format(true)
			// finally this also means that the Oid "knowns" about oid not used in the Fx itself
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			Assert.AreEqual ("Netscape Cert Type", aed.Oid.FriendlyName, "FriendlyName");
			// anyway the answer is most probably CryptoAPI
		}

		[Test]
		[Category ("NotDotNet")] // FriendlyName should not only be English.
		// note: important to emulate in Mono because we need it for SSL/TLS
		public void Build_SubjectAltNameExtension ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x11, 0x82, 0x0F, 0x77, 0x77, 0x77, 0x2E, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D });
			Assert.AreEqual ("30 11 82 0f 77 77 77 2e 65 78 61 6d 70 6c 65 2e 63 6f 6d", aed.Format (true), "Format(true)");
			Assert.AreEqual ("30 11 82 0f 77 77 77 2e 65 78 61 6d 70 6c 65 2e 63 6f 6d", aed.Format (false), "Format(false)");
			aed.Oid = new Oid ("2.5.29.17");
			// and now "AsnEncodedData" knows how to (magically) decode the data without involving the class
			Assert.AreEqual ("DNS Name=www.example.com" + Environment.NewLine, aed.Format (true), "aed.Format(true)");
			Assert.AreEqual ("DNS Name=www.example.com", aed.Format (false), "aed.Format(false)");
			// note that the Fx doesn't "really" support this extension
			// finally this also means that the Oid "knowns" about oid not used in the Fx itself
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			Assert.AreEqual ("Subject Alternative Name", aed.Oid.FriendlyName, "FriendlyName");
			// anyway the answer is most probably CryptoAPI
		}
	}
}

