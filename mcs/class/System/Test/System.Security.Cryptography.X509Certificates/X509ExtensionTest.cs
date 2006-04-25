//
// X509ExtensionTest.cs 
//	- NUnit tests for X509Extension
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

	// used to test the protected constructor properly
	public class X509Ex : X509Extension {

		public X509Ex ()
		{
		}
	}

	[TestFixture]
	public class X509ExtensionTest {

		[Test]
		public void ConstructorEmpty ()
		{
			X509Ex ex = new X509Ex ();
			Assert.IsFalse (ex.Critical, "Critical");
			Assert.IsNull (ex.RawData, "RawData");
			Assert.IsNull (ex.Oid, "Oid");
			Assert.AreEqual (String.Empty, ex.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ex.Format (false), "Format(false)");
			
			ex.Critical = true;
			Assert.IsTrue (ex.Critical, "Critical 2");
			ex.Oid = new Oid ("2.5.29.37");
			Assert.AreEqual ("2.5.29.37", ex.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Enhanced Key Usage", ex.Oid.FriendlyName, "Oid.FriendlyName");
			ex.RawData = new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 };
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, ex.Format (true), "Format(true)");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", ex.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAsnEncodedData_WithNullOid ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			X509Extension eku = new X509Extension (aed, true);
		}

		[Test]
		public void ConstructorAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new Oid ("2.5.29.37"), new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			X509Extension ex = new X509Extension (aed, true);
			Assert.IsTrue (ex.Critical, "Critical");
			Assert.AreEqual (7, ex.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual ("2.5.29.37", ex.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Enhanced Key Usage", ex.Oid.FriendlyName, "Oid.FriendlyName");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, ex.Format (true), "Format(true)");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", ex.Format (false), "Format(false)");
		}

		[Test]
		public void ConstructorAsnEncodedData_BadAsn ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[0]);
			X509Extension ex = new X509Extension (aed, true);
			Assert.AreEqual (String.Empty, ex.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ex.Format (false), "Format(false)");
			// no exception for an "empty" extension
		}

		[Test]
		public void ConstructorAsnEncodedData_BadAsnTag ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x05, 0x00 });
			X509Extension ex = new X509Extension (aed, true);
			Assert.AreEqual ("05 00", ex.Format (true), "Format(true)");
			Assert.AreEqual ("05 00", ex.Format (false), "Format(false)");
			// no exception for an "unknown" (ASN.1 NULL) extension
		}

		[Test]
		public void ConstructorAsnEncodedData_BadAsnLength ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x30, 0x01 });
			X509Extension ex = new X509Extension (aed, true);
			Assert.AreEqual ("30 01", ex.Format (true), "Format(true)");
			Assert.AreEqual ("30 01", ex.Format (false), "Format(false)");
			// no exception for an bad (invalid length) extension
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnEncodedData_Null ()
		{
			X509Extension ex = new X509Extension ((AsnEncodedData)null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOid_Null ()
		{
			X509Extension ex = new X509Extension ((Oid)null, new byte[] { 0x30, 0x01 }, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOid_RawNull ()
		{
			X509Extension ex = new X509Extension (new Oid ("1.2.3"), null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorString_Null ()
		{
			X509Extension ex = new X509Extension ((string)null, new byte[] { 0x30, 0x01 }, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorString_RawNull ()
		{
			X509Extension ex = new X509Extension ("1.2.3", null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			X509Ex ex = new X509Ex ();
			ex.CopyFrom (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom_AsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new Oid ("2.5.29.37"), new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			// this is recognized as an Enhanced Key Usages extension
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, aed.Format (true), "aed.Format(true)");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", aed.Format (false), "aed.Format(false)");
			X509Ex ex = new X509Ex ();
			// but won't be accepted by the CopyFrom method (no a X509Extension)
			ex.CopyFrom (aed);
		}

		[Test]
		public void Build_NetscapeCertTypeExtension ()
		{
			X509Extension ex = new X509Extension (new Oid ("2.16.840.1.113730.1.1"), new byte[] { 0x03, 0x02, 0x00, 0xFF }, false);
			// strangely no NewLine is being appended to Format(true)
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("SSL Client Authentication, SSL Server Authentication, SMIME, Signature, Unknown cert type, SSL CA, SMIME CA, Signature CA (ff)", ex.Format (true), "aed.Format(true)");
			//Assert.AreEqual ("SSL Client Authentication, SSL Server Authentication, SMIME, Signature, Unknown cert type, SSL CA, SMIME CA, Signature CA (ff)", ex.Format (false), "aed.Format(false)");
		}
	}
}

#endif
