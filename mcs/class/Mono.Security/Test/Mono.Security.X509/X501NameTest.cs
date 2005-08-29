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
	}
}
