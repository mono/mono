//
// X509EnhancedKeyUsageExtensionTest.cs 
//	- NUnit tests for X509EnhancedKeyUsageExtension
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
	public class X509EnhancedKeyUsageExtensionTest {

		private const string oid = "2.5.29.37";
		private const string fname = "Enhanced Key Usage";

		[Test]
		public void ConstructorEmpty ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			Assert.IsFalse (eku.Critical, "Critical");
			Assert.IsNull (eku.RawData, "RawData");
			Assert.AreEqual (oid, eku.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, eku.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (String.Empty, eku.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, eku.Format (false), "Format(false)");
		}

		[Test]
		public void ConstructorEmpty_EnhancedKeyUsages ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			Assert.AreEqual (0, eku.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
		}

		[Test]
		public void ConstructorAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (aed, true);
			Assert.IsTrue (eku.Critical, "Critical");
			Assert.AreEqual (7, eku.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, eku.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, eku.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (1, eku.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
			Assert.AreEqual ("1.2.3.4", eku.EnhancedKeyUsages[0].Value, "EnhancedKeyUsages Oid");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine, eku.Format (true), "Format(true)");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)", eku.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsn ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[0]);
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (aed, true);
			Assert.AreEqual (String.Empty, eku.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, eku.Format (false), "Format(false)");
			OidCollection oc = eku.EnhancedKeyUsages;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnTag ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x05, 0x00 });
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (aed, true);
			OidCollection oc = eku.EnhancedKeyUsages;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnLength ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x30, 0x01 });
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (aed, true);
			OidCollection oc = eku.EnhancedKeyUsages;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnEncodedData_Null ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ((AsnEncodedData)null, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidCollection_Null ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ((OidCollection)null, true);
		}

		[Test]
		public void ConstructorOidCollection ()
		{
			OidCollection oc = new OidCollection ();
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (oc, true);
			Assert.AreEqual ("30-00", BitConverter.ToString (eku.RawData), "RawData");
			Assert.AreEqual (0, eku.EnhancedKeyUsages.Count, "Count 0");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Information Not Available", eku.Format (true), "Format(true)");
			//Assert.AreEqual ("Information Not Available", eku.Format (false), "Format(false)");
			oc.Add (new Oid ("1.2.3.4"));
			Assert.AreEqual (0, eku.EnhancedKeyUsages.Count, "Count still 0");
			int n = eku.EnhancedKeyUsages.Add (new Oid ("1.2.3"));
			Assert.AreEqual (0, n, "Add");
			Assert.AreEqual (0, eku.EnhancedKeyUsages.Count, "Count again 0");	// readonly!
			Assert.AreEqual (1, oc.Count, "Count 1 - oc");
			Assert.AreEqual ("1.2.3.4", oc [0].Value, "Value - oc");

			oc.Add (new Oid ("1.3.6.1.5.5.7.3.1"));
			eku = new X509EnhancedKeyUsageExtension (oc, true);
			Assert.AreEqual (2, eku.EnhancedKeyUsages.Count, "Count 2");
			Assert.AreEqual ("1.2.3.4", eku.EnhancedKeyUsages[0].Value, "Value - 1");
			Assert.AreEqual ("1.3.6.1.5.5.7.3.1", eku.EnhancedKeyUsages[1].Value, "Value - 2");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4)" + Environment.NewLine + "Server Authentication (1.3.6.1.5.5.7.3.1)" + Environment.NewLine,
			//	eku.Format (true), "Format(true)");
			//Assert.AreEqual ("Unknown Key Usage (1.2.3.4), Server Authentication (1.3.6.1.5.5.7.3.1)", eku.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WrongExtension_X509EnhancedKeyUsageExtension ()
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			eku.CopyFrom (ku);
		}

		[Test]
		public void WrongExtension_X509Extension ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.2.3.4"));
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (oc, false);
			Assert.AreEqual (1, eku.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
			Assert.IsFalse (eku.Critical, "Critical");
			eku.CopyFrom (ex);
			Assert.IsTrue (eku.Critical, "Critical");
			Assert.AreEqual (String.Empty, BitConverter.ToString (eku.RawData), "RawData");
			Assert.AreEqual ("1.2.3", eku.Oid.Value, "Oid.Value");
			Assert.IsNull (eku.Oid.FriendlyName, "Oid.FriendlyName");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void WrongExtension_X509Extension_KeyUsages ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			eku.CopyFrom (ex);
			Assert.AreEqual (0, eku.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[0]);
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (new OidCollection (), true);
			eku.CopyFrom (aed); // note: not the same behaviour than using the constructor!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension ();
			eku.CopyFrom (null);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.2.3.4"));
			X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension (oc, true);
			Assert.IsTrue (eku.Critical, "Critical");
			byte[] raw = eku.RawData;
			Assert.AreEqual ("30-05-06-03-2A-03-04", BitConverter.ToString (raw), "RawData");

			AsnEncodedData aed = new AsnEncodedData (raw);
			X509EnhancedKeyUsageExtension copy = new X509EnhancedKeyUsageExtension (aed, false);
			Assert.IsFalse (copy.Critical, "Critical");
			Assert.AreEqual (7, copy.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, copy.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, copy.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (1, copy.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
			Assert.AreEqual ("1.2.3.4", copy.EnhancedKeyUsages[0].Value, "EnhancedKeyUsages Oid");
		}

#if !MOBILE
		[Test]
		public void CreateViaCryptoConfig ()
		{
			// extensions can be created with CryptoConfig
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			X509EnhancedKeyUsageExtension eku = (X509EnhancedKeyUsageExtension) CryptoConfig.CreateFromName (oid, new object[2] { aed, true });
			Assert.IsTrue (eku.Critical, "Critical");
			Assert.AreEqual (7, eku.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, eku.Oid.Value, "Oid.Value");
			Assert.AreEqual (1, eku.EnhancedKeyUsages.Count, "EnhancedKeyUsages");
			Assert.AreEqual ("1.2.3.4", eku.EnhancedKeyUsages[0].Value, "EnhancedKeyUsages Oid");
		}
#endif
	}
}

#endif
