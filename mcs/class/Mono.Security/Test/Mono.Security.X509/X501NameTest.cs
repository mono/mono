//
// X501NameTest.cs - NUnit Test Cases for the X501Name class
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Text;
using Mono.Security;
using Mono.Security.X509;

using NUnit.Framework;

namespace MonoTests.Mono.Security.X509 {

	[TestFixture]
	public class X501NameTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromString_Null ()
		{
			ASN1 asn = X501.FromString (null);
		}

		[Test]
		public void FromString_Empty ()
		{
			ASN1 asn = X501.FromString (String.Empty);
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (0, asn.Count, "Count");
		}

		[Test]
		public void FromString_Simple ()
		{
			ASN1 asn = X501.FromString ("CN=foobar, C=US");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (2, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("2.5.4.3", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			Assert.AreEqual ("foobar", Encoding.ASCII.GetString (asn[0][0][1].Value), "Value-0");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("US", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");
		}

		[Test] // bug #75780
		public void FromString_Quoted ()
		{
			ASN1 asn = X501.FromString ("CN=\"foo, bar\", C=US");

			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (2, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("2.5.4.3", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			Assert.AreEqual ("foo, bar", Encoding.ASCII.GetString (asn[0][0][1].Value), "Value-0");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("US", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");
		}

		[Test]
		public void FromString_EscapingComma ()
		{
			// from rfc2253
			ASN1 asn = X501.FromString ("CN=L. Eagle,O=Sue\\, Grabbit and Runn,C=GB");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (3, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("2.5.4.3", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			Assert.AreEqual ("L. Eagle", Encoding.ASCII.GetString (asn[0][0][1].Value), "Value-0");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.10", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("Sue, Grabbit and Runn", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");

			Assert.AreEqual (0x31, asn[2].Tag, "Tag-2");
			Assert.AreEqual (1, asn[2].Count, "Count-2");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[2][0][0]), "OID-2");
			Assert.AreEqual ("GB", Encoding.ASCII.GetString (asn[2][0][1].Value), "Value-2");
		}

		[Test]
		public void FromString_OID ()
		{
			// adapted from rfc2253
			ASN1 asn = X501.FromString ("1.3.6.1.4.1.1466.0=Mono,O=Test,C=GB");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (3, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("1.3.6.1.4.1.1466.0", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			Assert.AreEqual ("Mono", Encoding.ASCII.GetString (asn[0][0][1].Value), "Value-0");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.10", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("Test", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");

			Assert.AreEqual (0x31, asn[2].Tag, "Tag-2");
			Assert.AreEqual (1, asn[2].Count, "Count-2");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[2][0][0]), "OID-2");
			Assert.AreEqual ("GB", Encoding.ASCII.GetString (asn[2][0][1].Value), "Value-2");
		}

		[Test]
		[Ignore ("# support isn't implemented")]
		public void FromString_UnrecognizedEncoding ()
		{
			// from rfc2253
			ASN1 asn = X501.FromString ("1.3.6.1.4.1.1466.0=#04024869,O=Test,C=GB");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (3, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("1.3.6.1.4.1.1466.0", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");

			ASN1 octetstring = asn[0][0][1];
			Assert.AreEqual (0x04, octetstring.Tag, "Value-Tag");
			Assert.AreEqual (2, octetstring.Length, "Value-Length");
			Assert.AreEqual ("4869", BitConverter.ToString (octetstring.Value), "Value-Value");
		}

		[Test]
		public void FromString_EscapingCarriageReturn ()
		{
			// from rfc2253
			ASN1 asn = X501.FromString (@"CN=Before\0DAfter,O=Test,C=GB");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (3, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("2.5.4.3", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			Assert.AreEqual ("Before\rAfter", Encoding.ASCII.GetString (asn[0][0][1].Value), "Value-0");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.10", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("Test", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");

			Assert.AreEqual (0x31, asn[2].Tag, "Tag-2");
			Assert.AreEqual (1, asn[2].Count, "Count-2");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[2][0][0]), "OID-2");
			Assert.AreEqual ("GB", Encoding.ASCII.GetString (asn[2][0][1].Value), "Value-2");
		}

		[Test]
		public void FromString_EscapingNonAscii ()
		{
			// adapted from rfc2253
			ASN1 asn = X501.FromString (@"CN=Lu\C4\8Di\C4\87,O=Test,C=GB");
			Assert.AreEqual (0x30, asn.Tag, "Tag");
			Assert.AreEqual (3, asn.Count, "Count");

			Assert.AreEqual (0x31, asn[0].Tag, "Tag-0");
			Assert.AreEqual (1, asn[0].Count, "Count-0");
			Assert.AreEqual ("2.5.4.3", ASN1Convert.ToOid (asn[0][0][0]), "OID-0");
			char[] value = Encoding.BigEndianUnicode.GetChars (asn[0][0][1].Value);
			Assert.AreEqual (0x004C, value[0], "Value-0[0]");
			Assert.AreEqual (0x0075, value[1], "Value-0[1]");
			Assert.AreEqual (0x010D, value[2], "Value-0[2]");
			Assert.AreEqual (0x0069, value[3], "Value-0[3]");
			Assert.AreEqual (0x0107, value[4], "Value-0[4]");

			Assert.AreEqual (0x31, asn[1].Tag, "Tag-1");
			Assert.AreEqual (1, asn[1].Count, "Count-1");
			Assert.AreEqual ("2.5.4.10", ASN1Convert.ToOid (asn[1][0][0]), "OID-1");
			Assert.AreEqual ("Test", Encoding.ASCII.GetString (asn[1][0][1].Value), "Value-1");

			Assert.AreEqual (0x31, asn[2].Tag, "Tag-2");
			Assert.AreEqual (1, asn[2].Count, "Count-2");
			Assert.AreEqual ("2.5.4.6", ASN1Convert.ToOid (asn[2][0][0]), "OID-2");
			Assert.AreEqual ("GB", Encoding.ASCII.GetString (asn[2][0][1].Value), "Value-2");
		}

		[Test]
		[Ignore ("multi-valued support isn't implemented")]
		public void FromString_MultiValued ()
		{
			// from rfc2253
			ASN1 asn = X501.FromString ("OU=Sales+CN=J. Smith,O=Widget Inc.,C=US");
		}

		[Test]
		public void T61String ()
		{
			// http://bugzilla.ximian.com/show_bug.cgi?id=77295
			byte[] sn = { 0x30, 0x81, 0xB5, 0x31, 0x0B, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13, 0x02, 0x44, 0x4B, 0x31, 0x2D, 0x30, 0x2B, 0x06, 
				0x03, 0x55, 0x04, 0x0A, 0x14, 0x24, 0x48, 0x65, 0x64, 0x65, 0x62, 0x79, 0x27, 0x73, 0x20, 0x4D, 0xF8, 0x62, 0x65, 0x6C, 0x68, 0x61, 
				0x6E, 0x64, 0x65, 0x6C, 0x20, 0x2F, 0x2F, 0x20, 0x43, 0x56, 0x52, 0x3A, 0x31, 0x33, 0x34, 0x37, 0x31, 0x39, 0x36, 0x37, 0x31, 0x2F, 
				0x30, 0x2D, 0x06, 0x03, 0x55, 0x04, 0x03, 0x14, 0x26, 0x48, 0x65, 0x64, 0x65, 0x62, 0x79, 0x27, 0x73, 0x20, 0x4D, 0xF8, 0x62, 0x65, 
				0x6C, 0x68, 0x61, 0x6E, 0x64, 0x65, 0x6C, 0x20, 0x2D, 0x20, 0x53, 0x61, 0x6C, 0x67, 0x73, 0x61, 0x66, 0x64, 0x65, 0x6C, 0x69, 0x6E, 
				0x67, 0x65, 0x6E, 0x31, 0x1E, 0x30, 0x1C, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x09, 0x01, 0x16, 0x0F, 0x76, 0x68, 
				0x6D, 0x40, 0x75, 0x73, 0x65, 0x2E, 0x74, 0x65, 0x73, 0x74, 0x2E, 0x64, 0x6B, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x05, 
				0x13, 0x1D, 0x43, 0x56, 0x52, 0x3A, 0x31, 0x33, 0x34, 0x37, 0x31, 0x39, 0x36, 0x37, 0x2D, 0x55, 0x49, 0x44, 0x3A, 0x31, 0x32, 0x31, 
				0x32, 0x31, 0x32, 0x31, 0x32, 0x31, 0x32, 0x31, 0x32 };
			ASN1 asn = new ASN1 (sn);
			Assert.AreEqual ("C=DK, O=Hedeby's Møbelhandel // CVR:13471967, CN=Hedeby's Møbelhandel - Salgsafdelingen, E=vhm@use.test.dk, SERIALNUMBER=CVR:13471967-UID:121212121212", X501.ToString (asn), "ToString-1");
			Assert.AreEqual ("SERIALNUMBER=CVR:13471967-UID:121212121212, E=vhm@use.test.dk, CN=Hedeby's Møbelhandel - Salgsafdelingen, O=Hedeby's Møbelhandel // CVR:13471967, C=DK", X501.ToString (asn, true, ", ", false), "ToString-2");
		}

		[Test]
		public void ToString_BMPQuoting ()
		{
			byte[] sn = { 0x30, 0x4F, 0x31, 0x4d, 0x30, 0x4B, 0x06, 0x03, 0x55, 0x04, 0x03, 0x1E, 0x44, 0x00, 0x4D, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x61,
				0x00, 0x67, 0x00, 0x65, 0x00, 0x64, 0x00, 0x20, 0x00, 0x50, 0x00, 0x4B, 0x00, 0x43, 0x00, 0x53, 0x00, 0x23, 0x00, 0x37, 0x00, 0x20,
				0x00, 0x54, 0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x20, 0x00, 0x52, 0x00, 0x6F, 0x00, 0x6F, 0x00, 0x74, 0x00, 0x20, 0x00, 0x41,
				0x00, 0x75, 0x00, 0x74, 0x00, 0x68, 0x00, 0x6F, 0x00, 0x72, 0x00, 0x69, 0x00, 0x74, 0x00, 0x79 };
			ASN1 asn = new ASN1 (sn);
			Assert.AreEqual ("CN=\"Managed PKCS#7 Test Root Authority\"", X501.ToString (asn), "ToString-1");
		}
	}
}
