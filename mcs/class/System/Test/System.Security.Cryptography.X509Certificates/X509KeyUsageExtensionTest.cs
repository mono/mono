//
// X509KeyUsageExtensionTest.cs - NUnit tests for X509KeyUsageExtension
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509KeyUsageExtensionTest {

		private const string oid = "2.5.29.15";
		private const string fname = "Key Usage";

		[Test]
		public void ConstructorEmpty () 
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			Assert.IsFalse (ku.Critical , "Critical");
			Assert.IsNull (ku.RawData, "RawData");
			Assert.AreEqual (oid, ku.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ku.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (String.Empty, ku.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ku.Format (false), "Format(false)");
			Assert.AreEqual (0, (int)ku.KeyUsages, "KeyUsages");
		}

		[Test]
		public void ConstructorAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x03, 0x01, 0x00 });
			X509KeyUsageExtension ku = new X509KeyUsageExtension (aed, true);
			Assert.IsTrue (ku.Critical, "Critical");
			Assert.AreEqual (3, ku.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, ku.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ku.Oid.FriendlyName, "Oid.FriendlyName");
			//Assert.AreEqual ("Information Not Available", ku.Format (true), "Format(true)");
			//Assert.AreEqual ("Information Not Available", ku.Format (false), "Format(false)");
			Assert.AreEqual (0, (int)ku.KeyUsages, "KeyUsages");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsn ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[0]);
			X509KeyUsageExtension ku = new X509KeyUsageExtension (aed, true);
			Assert.AreEqual (String.Empty, ku.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ku.Format (false), "Format(false)");
			X509KeyUsageFlags kuf = ku.KeyUsages;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnTag ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x05, 0x00 });
			X509KeyUsageExtension ku = new X509KeyUsageExtension (aed, true);
			Assert.AreEqual ("0500", ku.Format (true), "Format(true)");
			Assert.AreEqual ("0500", ku.Format (false), "Format(false)");
			X509KeyUsageFlags kuf = ku.KeyUsages;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnLength ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x03, 0x01 });
			X509KeyUsageExtension ku = new X509KeyUsageExtension (aed, true);
			Assert.AreEqual ("0301", ku.Format (true), "Format(true)");
			Assert.AreEqual ("0301", ku.Format (false), "Format(false)");
			X509KeyUsageFlags kuf = ku.KeyUsages;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnEncodedData_Null ()
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension (null, true);
		}

		[Test]
		// [ExpectedException (typeof (...))]
		public void ConstructorKeyUsage_Invalid ()
		{
			X509KeyUsageFlags kuf = (X509KeyUsageFlags)Int32.MinValue;
			X509KeyUsageExtension ku = new X509KeyUsageExtension (kuf, false);
			Assert.AreEqual (0, (int)ku.KeyUsages, "KeyUsages");
			Assert.AreEqual ("03-01-00", BitConverter.ToString (ku.RawData), "RawData");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Information Not Available", ku.Format (true), "Format(true)");
			//Assert.AreEqual ("Information Not Available", ku.Format (false), "Format(false)");
		}

		private X509KeyUsageExtension ValidateKeyUsage (X509KeyUsageFlags kuf, string rawdata)
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension (kuf, false);
			Assert.IsFalse (ku.Critical, kuf.ToString () + ".Critical");
			Assert.AreEqual (rawdata, BitConverter.ToString (ku.RawData), kuf.ToString () + ".RawData");
			Assert.AreEqual (oid, ku.Oid.Value, kuf.ToString () + ".Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, ku.Oid.FriendlyName, kuf.ToString () + ".Oid.FriendlyName");
			Assert.AreEqual (kuf, ku.KeyUsages, kuf.ToString () + ".KeyUsages");
			return ku;
		}

		[Test]
		public void ConstructorKeyUsage_CRLSign ()
		{
			X509KeyUsageExtension ku = ValidateKeyUsage (X509KeyUsageFlags.CrlSign, "03-02-01-02");
			Assert.AreEqual ("Off-line CRL Signing, CRL Signing (02)", ku.Format (false), "CRLSign");

			ku = ValidateKeyUsage (X509KeyUsageFlags.DataEncipherment, "03-02-04-10");
			Assert.AreEqual ("Data Encipherment (10)", ku.Format (false), "DataEncipherment");

			ku = ValidateKeyUsage (X509KeyUsageFlags.DecipherOnly, "03-03-07-00-80");
			Assert.AreEqual ("Decipher Only (00 80)", ku.Format (false), "DecipherOnly");
			
			ku = ValidateKeyUsage (X509KeyUsageFlags.DigitalSignature, "03-02-07-80");
			Assert.AreEqual ("Digital Signature (80)", ku.Format (false), "DigitalSignature");
			
			ku = ValidateKeyUsage (X509KeyUsageFlags.EncipherOnly, "03-02-00-01");
			Assert.AreEqual ("Encipher Only (01)", ku.Format (false), "EncipherOnly");
			
			ku = ValidateKeyUsage (X509KeyUsageFlags.KeyAgreement, "03-02-03-08");
			Assert.AreEqual ("Key Agreement (08)", ku.Format (false), "KeyAgreement");
			
			ku = ValidateKeyUsage (X509KeyUsageFlags.KeyCertSign, "03-02-02-04");
			Assert.AreEqual ("Certificate Signing (04)", ku.Format (false), "KeyCertSign");

			ku = ValidateKeyUsage (X509KeyUsageFlags.KeyEncipherment, "03-02-05-20");
			Assert.AreEqual ("Key Encipherment (20)", ku.Format (false), "KeyEncipherment");

			ku = ValidateKeyUsage (X509KeyUsageFlags.NonRepudiation, "03-02-06-40");
			Assert.AreEqual ("Non-Repudiation (40)", ku.Format (false), "NonRepudiation");
			
			ValidateKeyUsage (X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.DecipherOnly, "03-03-07-10-80");
			ValidateKeyUsage (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DecipherOnly, "03-03-07-80-80");
			ValidateKeyUsage (X509KeyUsageFlags.EncipherOnly | X509KeyUsageFlags.DecipherOnly, "03-03-07-01-80");
			ValidateKeyUsage (X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.DataEncipherment, "03-02-04-50");

			ku = ValidateKeyUsage (X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.DecipherOnly |
				X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.EncipherOnly | X509KeyUsageFlags.KeyAgreement | 
				X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.NonRepudiation, "03-03-07-FF-80");
			Assert.AreEqual ("Digital Signature, Non-Repudiation, Key Encipherment, Data Encipherment, Key Agreement, Certificate Signing, Off-line CRL Signing, CRL Signing, Encipher Only, Decipher Only (ff 80)" + Environment.NewLine,
				ku.Format (true), "All");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WrongExtension_X509EnhancedKeyUsageExtension ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			ku.CopyFrom (eku);
		}

		[Test]
		public void WrongExtension_X509Extension ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte [0], true);
			X509KeyUsageExtension ku = new X509KeyUsageExtension (X509KeyUsageFlags.CrlSign, true);
			ku.CopyFrom (ex);
			Assert.IsTrue (ku.Critical, "Critical");
			Assert.AreEqual (String.Empty, BitConverter.ToString (ku.RawData), "RawData");
			Assert.AreEqual ("1.2.3", ku.Oid.Value, "Oid.Value");
			Assert.IsNull (ku.Oid.FriendlyName, "Oid.FriendlyName");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void WrongExtension_X509Extension_KeyUsages ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			ku.CopyFrom (ex);
			Assert.AreEqual (0, ku.KeyUsages, "KeyUsages");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[0]);
			X509KeyUsageExtension ku = new X509KeyUsageExtension (X509KeyUsageFlags.CrlSign, true);
			ku.CopyFrom (aed); // note: not the same behaviour than using the constructor!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			X509KeyUsageExtension eku = new X509KeyUsageExtension ();
			eku.CopyFrom (null);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension (X509KeyUsageFlags.CrlSign, true);
			Assert.IsTrue (ku.Critical, "Critical");
			byte[] raw = ku.RawData;
			Assert.AreEqual ("03-02-01-02", BitConverter.ToString (raw), "RawData");
  
			AsnEncodedData aed = new AsnEncodedData (raw);
			X509KeyUsageExtension copy = new X509KeyUsageExtension (aed, false);
			Assert.IsFalse (copy.Critical, "Critical");
			Assert.AreEqual (4, copy.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, copy.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, copy.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (X509KeyUsageFlags.CrlSign, copy.KeyUsages, "KeyUsages");
		}

#if !MOBILE
		[Test]
		public void CreateViaCryptoConfig ()
		{
			// extensions can be created with CryptoConfig
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x03, 0x01, 0x00 });
			X509KeyUsageExtension ku = (X509KeyUsageExtension) CryptoConfig.CreateFromName (oid, new object[2] { aed, true });
			Assert.IsTrue (ku.Critical, "Critical");
			Assert.AreEqual (3, ku.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, ku.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, (int) ku.KeyUsages, "KeyUsages");
		}
#endif
	}
}

#endif
