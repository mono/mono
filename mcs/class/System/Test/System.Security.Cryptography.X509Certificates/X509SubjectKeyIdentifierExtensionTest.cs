//
// X509SubjectKeyIdentifierExtensionTest.cs 
//	- NUnit tests for X509SubjectKeyIdentifierExtension
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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
using System.Text;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509SubjectKeyIdentifierExtensionTest {

		private const string oid = "2.5.29.14";
		private const string fname = "Subject Key Identifier";

		private PublicKey pk1;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			pk1 = new X509Certificate2 (Encoding.ASCII.GetBytes (X509Certificate2Test.base64_cert)).PublicKey;
		}

		[Test]
		public void ConstructorEmpty ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ();
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.IsNull (ski.RawData, "RawData");
			Assert.AreEqual (oid, ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (String.Empty, ski.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ski.Format (false), "Format(false)");
		}

		[Test]
		public void ConstructorEmpty_SubjectKeyIdentifier ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ();
			Assert.IsNull (ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
		}

		[Test]
		public void ConstructorAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x04, 0x08, 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF });
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (aed, true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual (oid, ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("04-08-01-23-45-67-89-AB-CD-EF", BitConverter.ToString (ski.RawData), "RawData");
			Assert.AreEqual ("0123456789ABCDEF", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("01 23 45 67 89 ab cd ef" + Environment.NewLine, ski.Format (true), "Format(true)");
			Assert.AreEqual ("01 23 45 67 89 ab cd ef", ski.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsn ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[0]);
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (aed, true);
			Assert.AreEqual (String.Empty, ski.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ski.Format (false), "Format(false)");
			string s = ski.SubjectKeyIdentifier;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnTag ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x05, 0x00 });
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (aed, true);
			Assert.AreEqual ("0500", ski.Format (true), "Format(true)");
			Assert.AreEqual ("0500", ski.Format (false), "Format(false)");
			string s = ski.SubjectKeyIdentifier;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnLength ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x30, 0x01 });
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (aed, true);
			Assert.AreEqual ("3001", ski.Format (true), "Format(true)");
			Assert.AreEqual ("3001", ski.Format (false), "Format(false)");
			string s = ski.SubjectKeyIdentifier;
		}

		[Test]
		public void ConstructorAsnEncodedData_SmallestValid ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x04, 0x00 });
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (aed, true);
			Assert.AreEqual (String.Empty, ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-00", BitConverter.ToString (ski.RawData), "RawData");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Information Not Available", ski.Format (true), "Format(true)");
			//Assert.AreEqual ("Information Not Available", ski.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnEncodedData_Null ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ((AsnEncodedData)null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorByteArray_Null ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ((byte[])null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorByteArray_Empty ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (new byte[0], true);
		}

		[Test]
		public void ConstructorByteArray_20 ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (new byte[20], true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("04-14-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00", BitConverter.ToString (ski.RawData), "RawData");
			Assert.AreEqual (oid, ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("0000000000000000000000000000000000000000", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00" + Environment.NewLine, ski.Format (true), "Format(true)");
			Assert.AreEqual ("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", ski.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorString_Null ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ((String)null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_Empty ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (String.Empty, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_Single ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("f", false);
		}

		[Test]
		public void ConstructorString ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("ffFFfFFf", false);
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("04-04-FF-FF-FF-FF", BitConverter.ToString (ski.RawData), "RawData");
			Assert.AreEqual (oid, ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("FFFFFFFF", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("ff ff ff ff" + Environment.NewLine, ski.Format (true), "Format(true)");
			Assert.AreEqual ("ff ff ff ff", ski.Format (false), "Format(false)");
		}

		[Test]
		public void ConstructorString_NotHex ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("Mono::", true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("04-03-FF-FF-FF", BitConverter.ToString (ski.RawData), "RawData");
			Assert.AreEqual (oid, ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("FFFFFF", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("ff ff ff" + Environment.NewLine, ski.Format (true), "Format(true)");
			Assert.AreEqual ("ff ff ff", ski.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPublicKey_Null ()
		{
			new X509SubjectKeyIdentifierExtension ((PublicKey)null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorPublicKeyHash_Null ()
		{
			new X509SubjectKeyIdentifierExtension (null, X509SubjectKeyIdentifierHashAlgorithm.Sha1, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorPublicKeyHash_BadX509SubjectKeyIdentifierHashAlgorithm ()
		{
			new X509SubjectKeyIdentifierExtension (pk1, (X509SubjectKeyIdentifierHashAlgorithm)Int32.MinValue, true);
		}

		[Test]
		public void ConstructorPublicKeyHash_Critical ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("4A0200E2E8D33DBA05FC37BDC36DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-4A-02-00-E2-E8-D3-3D-BA-05-FC-37-BD-C3-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_Sha1_Critical ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.Sha1, true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("4A0200E2E8D33DBA05FC37BDC36DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-4A-02-00-E2-E8-D3-3D-BA-05-FC-37-BD-C3-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_ShortSha1_Critical ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.ShortSha1, true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("436DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-08-43-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_CapiSha1_Critical ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.CapiSha1, true);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("0E73CE0E2E059378FC782707EBF0B4E7AEA652E1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-0E-73-CE-0E-2E-05-93-78-FC-78-27-07-EB-F0-B4-E7-AE-A6-52-E1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, false);
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("4A0200E2E8D33DBA05FC37BDC36DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-4A-02-00-E2-E8-D3-3D-BA-05-FC-37-BD-C3-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_Sha1 ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.Sha1, false);
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("4A0200E2E8D33DBA05FC37BDC36DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-4A-02-00-E2-E8-D3-3D-BA-05-FC-37-BD-C3-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_ShortSha1 ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.ShortSha1, false);
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("436DCF47212D77D1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-08-43-6D-CF-47-21-2D-77-D1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		public void ConstructorPublicKeyHash_CapiSha1 ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension (pk1, X509SubjectKeyIdentifierHashAlgorithm.CapiSha1, false);
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("2.5.29.14", ski.Oid.Value, "Oid");
			Assert.AreEqual ("0E73CE0E2E059378FC782707EBF0B4E7AEA652E1", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-14-0E-73-CE-0E-2E-05-93-78-FC-78-27-07-EB-F0-B4-E7-AE-A6-52-E1", BitConverter.ToString (ski.RawData), "RawData");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WrongExtension_X509KeyUsageExtension ()
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ();
			ski.CopyFrom (ku);
		}

		[Test]
		public void WrongExtension_X509Extension ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("www.example.com", false); // odd length
			Assert.IsFalse (ski.Critical, "Critical");
			Assert.AreEqual ("FFFFFFFFFFFFFF", ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("ff ff ff ff ff ff ff" + Environment.NewLine, ski.Format (true), "Format(true)");
			Assert.AreEqual ("ff ff ff ff ff ff ff", ski.Format (false), "Format(false)");

			ski.CopyFrom (ex);
			Assert.IsTrue (ski.Critical, "Critical");
			Assert.AreEqual (String.Empty, BitConverter.ToString (ski.RawData), "RawData");
			Assert.AreEqual ("1.2.3", ski.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.IsNull (ski.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (String.Empty, ski.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ski.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void WrongExtension_X509Extension_CertificateAuthority ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ();
			ski.CopyFrom (ex);
			string s = ski.SubjectKeyIdentifier;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[0]);
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("www.example.com", false);
			ski.CopyFrom (aed); // note: not the same behaviour than using the constructor!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ();
			ski.CopyFrom (null);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension ("ff", true);
			Assert.IsTrue (ski.Critical, "Critical");
			byte[] raw = ski.RawData;
			Assert.AreEqual ("04-01-FF", BitConverter.ToString (raw), "RawData");

			AsnEncodedData aed = new AsnEncodedData (raw);
			X509SubjectKeyIdentifierExtension copy = new X509SubjectKeyIdentifierExtension (aed, false);
			Assert.IsFalse (copy.Critical, "Critical");
			Assert.AreEqual ("04-01-FF", BitConverter.ToString (copy.RawData), "copy.RawData");
			Assert.AreEqual (oid, copy.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, copy.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("FF", copy.SubjectKeyIdentifier, "SubjectKeyIdentifier");
		}

#if !MOBILE
		[Test]
		public void CreateViaCryptoConfig ()
		{
			// extensions can be created with CryptoConfig
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x04, 0x00 });
			X509SubjectKeyIdentifierExtension ski = (X509SubjectKeyIdentifierExtension) CryptoConfig.CreateFromName (oid, new object[2] { aed, true });
			Assert.AreEqual (String.Empty, ski.SubjectKeyIdentifier, "SubjectKeyIdentifier");
			Assert.AreEqual ("04-00", BitConverter.ToString (ski.RawData), "RawData");
		}
#endif
	}
}

