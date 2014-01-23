//
// X509CertificateCollection2Test.cs 
//	- NUnit tests for X509CertificateCollection2
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	David Ferguson  <davecferguson@gmail.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Text;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509Certificate2CollectionTest {

		private X509Certificate2Collection empty;
		private X509Certificate2Collection single;
		private X509Certificate2Collection collection;

		private X509Certificate2 cert_empty;
		private X509Certificate2 cert1;
		private X509Certificate2 cert2;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cert_empty = new X509Certificate2 ();
			cert1 = new X509Certificate2 (X509Certificate2Test.farscape_pfx, "farscape", X509KeyStorageFlags.Exportable);
			cert2 = new X509Certificate2 (Encoding.ASCII.GetBytes (X509Certificate2Test.base64_cert));

			empty = new X509Certificate2Collection ();
			single = new X509Certificate2Collection ();
			single.Add (cert1);
			collection = new X509Certificate2Collection (single);
			collection.Add (cert2);
		}

		[Test]
		public void Ctor ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			Assert.AreEqual (0, c.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_X509CertificateCollection2_Null ()
		{
			new X509Certificate2Collection ((X509Certificate2Collection) null);
		}

		[Test]
		public void Ctor_X509CertificateCollection2_Empty ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection (empty);
			Assert.AreEqual (0, c.Count, "Count");
		}

		[Test]
		public void Ctor_X509CertificateCollection2 ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection (collection);
			Assert.AreEqual (2, c.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_X509Certificate2_Null ()
		{
			new X509Certificate2Collection ((X509Certificate2) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_X509Certificate2Array_Null ()
		{
			new X509Certificate2Collection ((X509Certificate2[]) null);
		}

		[Test]
		public void Ctor_X509Certificate2Array_Empty ()
		{
			X509Certificate2[] array = new X509Certificate2 [0];
			X509Certificate2Collection c = new X509Certificate2Collection (array);
			Assert.AreEqual (0, c.Count, "Count");
		}

		[Test]
		public void Ctor_X509Certificate2Array ()
		{
			X509Certificate2[] array = new X509Certificate2[3] { cert1, cert2, cert_empty };
			X509Certificate2Collection c = new X509Certificate2Collection (array);
			Assert.AreEqual (3, c.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_X509Certificate2_Null ()
		{
			collection.Add ((X509Certificate2) null);
		}

		[Test]
		public void Add_X509Certificate2 ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			Assert.AreEqual (0, c.Count, "0");
			c.Add (cert1);
			Assert.AreEqual (1, c.Count, "1");
			// adding invalid certificate
			c.Add (cert_empty);
			Assert.AreEqual (2, c.Count, "2");
			// re-adding same certificate
			c.Add (cert1);
			Assert.AreEqual (3, c.Count, "3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_X509Certificate2Collection_Null ()
		{
			collection.AddRange ((X509Certificate2Collection) null);
		}

		[Test]
		public void AddRange_X509Certificate2Collection ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			c.AddRange (empty);
			Assert.AreEqual (0, c.Count, "0");
			c.AddRange (single);
			Assert.AreEqual (1, c.Count, "1");
			c.AddRange (collection);
			Assert.AreEqual (3, c.Count, "3");
			// re-adding same collection
			c.AddRange (single);
			Assert.AreEqual (4, c.Count, "4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_X509Certificate2Array_Null ()
		{
			collection.AddRange ((X509Certificate2[]) null);
		}

		[Test]
		public void AddRange_X509Certificate2Array ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			c.AddRange (new X509Certificate2 [0]);
			Assert.AreEqual (0, c.Count, "0");
			c.AddRange (new X509Certificate2[3] { cert1, cert2, cert_empty });
			Assert.AreEqual (3, c.Count, "3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Contains_X509Certificate2_Null ()
		{
			empty.Contains ((X509Certificate2) null);
		}

		[Test]
		public void Contains_Empty ()
		{
			Assert.IsFalse (empty.Contains (cert_empty), "empty|cert_empty");
			Assert.IsFalse (empty.Contains (cert1), "empty|cert1");
			Assert.IsFalse (empty.Contains (cert2), "empty|cert2");
		}

		[Test]
		public void Contains ()
		{
			Assert.IsTrue (single.Contains (cert1), "single|cert1");
			Assert.IsFalse (single.Contains (cert2), "single|cert2");

			Assert.IsTrue (collection.Contains (cert1), "multi|cert1");
			Assert.IsTrue (collection.Contains (cert2), "multi|cert2");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Contains_EmptyCert ()
		{
			single.Contains (cert_empty);
			// note: Equals fails, but it works for an empty collection (not called)
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Empty_Authenticode ()
		{
			empty.Export (X509ContentType.Authenticode);
		}

		[Test]
		public void Export_Empty ()
		{
			Assert.IsNull (empty.Export (X509ContentType.Cert), "Cert");
			Assert.IsNull (empty.Export (X509ContentType.Cert, null), "Cert,null");
			Assert.IsNull (empty.Export (X509ContentType.Cert, String.Empty), "Cert,Empty");
			Assert.IsNull (empty.Export (X509ContentType.SerializedCert), "SerializedCert");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Empty_Pfx ()
		{
			byte[] data = empty.Export (X509ContentType.Pfx);
			Assert.IsNotNull (data, "data");
			Assert.AreEqual (X509ContentType.Pfx, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Empty_Pkcs12 ()
		{
			byte[] data = empty.Export (X509ContentType.Pkcs12);
			Assert.IsNotNull (data, "data");
			Assert.AreEqual (X509ContentType.Pkcs12, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Empty_Pkcs7 ()
		{
			byte[] data = empty.Export (X509ContentType.Pkcs7);
			Assert.IsNotNull (data, "data");
			Assert.AreEqual (X509ContentType.Pkcs7, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Empty_SerializedStore ()
		{
			byte[] data = empty.Export (X509ContentType.SerializedStore);
			Assert.IsNotNull (data, "data");
			Assert.AreEqual (X509ContentType.SerializedStore, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Empty_Unknown ()
		{
			empty.Export (X509ContentType.Unknown);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Empty_Bad ()
		{
			empty.Export ((X509ContentType)Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Single_Authenticode ()
		{
			single.Export (X509ContentType.Authenticode);
		}

		[Test]
		public void Export_Single_Cert ()
		{
			byte[] data = single.Export (X509ContentType.Cert);
			Assert.AreEqual (X509ContentType.Cert, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			Assert.AreEqual (data, cert1.RawData, "RawData");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Single_Pfx ()
		{
			byte[] data = single.Export (X509ContentType.Pfx);
			Assert.AreEqual (X509ContentType.Pfx, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Single_Pkcs12 ()
		{
			byte[] data = single.Export (X509ContentType.Pkcs12);
			Assert.AreEqual (X509ContentType.Pkcs12, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Single_Pkcs7 ()
		{
			byte[] data = single.Export (X509ContentType.Pkcs7);
			Assert.AreEqual (X509ContentType.Pkcs7, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Single_SerializedCert ()
		{
			byte[] data = single.Export (X509ContentType.SerializedCert);
			Assert.AreEqual (X509ContentType.SerializedCert, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Single_SerializedStore ()
		{
			byte[] data = single.Export (X509ContentType.SerializedStore);
			Assert.AreEqual (X509ContentType.SerializedStore, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Single_Unknown ()
		{
			single.Export (X509ContentType.Unknown);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Single_Bad ()
		{
			single.Export ((X509ContentType) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Multiple_Authenticode ()
		{
			collection.Export (X509ContentType.Authenticode);
		}

		[Test]
		public void Export_Multiple_Cert ()
		{
			byte[] data = collection.Export (X509ContentType.Cert);
			Assert.AreEqual (X509ContentType.Cert, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// last certificate was exported
			Assert.AreEqual (data, cert2.RawData, "RawData");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert2, c, "Equals");
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Multiple_Pfx ()
		{
			byte[] data = collection.Export (X509ContentType.Pfx);
			Assert.AreEqual (X509ContentType.Pfx, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Multiple_Pkcs12 ()
		{
			byte[] data = collection.Export (X509ContentType.Pkcs12);
			Assert.AreEqual (X509ContentType.Pkcs12, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			Assert.AreEqual (cert1, c, "Equals");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Multiple_Pkcs7 ()
		{
			byte[] data = collection.Export (X509ContentType.Pkcs7);
			Assert.AreEqual (X509ContentType.Pkcs7, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[Category ("NotWorking")]
		public void Export_Multiple_SerializedCert ()
		{
			byte[] data = collection.Export (X509ContentType.SerializedCert);
			Assert.AreEqual (X509ContentType.SerializedCert, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// usable
			X509Certificate2 c = new X509Certificate2 (data);
			// last certificate was exported
			Assert.AreEqual (cert2, c, "Equals");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		[Category ("NotWorking")]
		public void Export_Multiple_SerializedStore ()
		{
			byte[] data = collection.Export (X509ContentType.SerializedStore);
			Assert.AreEqual (X509ContentType.SerializedStore, X509Certificate2.GetCertContentType (data), "GetCertContentType");
			// not usable
			new X509Certificate2 (data);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Multiple_Unknown ()
		{
			collection.Export (X509ContentType.Unknown);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Export_Multiple_Bad ()
		{
			collection.Export ((X509ContentType) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Find_FindValue_Null ()
		{
			empty.Find (X509FindType.FindByApplicationPolicy, null, true);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindType_Bad ()
		{
			empty.Find ((X509FindType)Int32.MinValue, new object(), true);
		}

		[Test]
		public void Find_Empty ()
		{
			string oid = "1.2.3.4";
			Assert.AreEqual (0, empty.Find (X509FindType.FindByApplicationPolicy, oid, false).Count, "Empty|FindByApplicationPolicy");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByCertificatePolicy, oid, false).Count, "Empty|FindByCertificatePolicy");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByExtension, oid, false).Count, "Empty|FindByExtension");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByIssuerDistinguishedName, String.Empty, false).Count, "Empty|FindByIssuerDistinguishedName");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByIssuerName, String.Empty, false).Count, "Empty|FindByIssuerName");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.CrlSign, false).Count, "Empty|FindByKeyUsage");
			Assert.AreEqual (0, empty.Find (X509FindType.FindBySerialNumber, String.Empty, false).Count, "Empty|FindBySerialNumber");
			Assert.AreEqual (0, empty.Find (X509FindType.FindBySubjectDistinguishedName, String.Empty, false).Count, "Empty|FindBySubjectDistinguishedName");
			Assert.AreEqual (0, empty.Find (X509FindType.FindBySubjectKeyIdentifier, String.Empty, false).Count, "Empty|FindBySubjectKeyIdentifier");
			Assert.AreEqual (0, empty.Find (X509FindType.FindBySubjectName, String.Empty, false).Count, "Empty|FindByTemplateName");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByTemplateName, String.Empty, false).Count, "Empty|FindByTemplateName");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByThumbprint, String.Empty, false).Count, "Empty|FindByThumbprint");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByTimeExpired, DateTime.Now, false).Count, "Empty|FindByTimeExpired");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByTimeNotYetValid, DateTime.Now, false).Count, "Empty|FindByTimeNotYetValid");
			Assert.AreEqual (0, empty.Find (X509FindType.FindByTimeValid, DateTime.Now, false).Count, "Empty|FindByTimeValid");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_CollectionWithEmptyCert ()
		{
			new X509Certificate2Collection (cert_empty).Find (X509FindType.FindByIssuerName, String.Empty, false);
		}

		[Test]
		public void Find_FindByApplicationPolicy ()
		{
			X509Certificate2Collection result = collection.Find (X509FindType.FindByApplicationPolicy, "1.2.3.4", false);
			Assert.AreEqual (1, result.Count, "FindByApplicationPolicy/Empty/false");
			Assert.AreEqual (0, result[0].Extensions.Count, "no extension");
			// FIXME - need a negative test case (with extensions)
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Find_FindByApplicationPolicy_NotOid ()
		{
			collection.Find (X509FindType.FindByApplicationPolicy, "policy", false);
		}

		[Test]
		public void Find_FindByCertificatePolicy ()
		{
			Assert.AreEqual (0, collection.Find (X509FindType.FindByCertificatePolicy, "1.2.3.4", false).Count, "FindByApplicationPolicy/Empty/false");
			// FIXME - need a positive test case
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Find_FindByCertificatePolicy_NotOid ()
		{
			collection.Find (X509FindType.FindByCertificatePolicy, "policy", false);
		}

		[Test]
		public void Find_FindByExtension ()
		{
			// partial match
			Assert.AreEqual (0, collection.Find (X509FindType.FindByExtension, "2.5.29", false).Count, "FindByExtension/2.5.29/false");
			// full match
			Assert.AreEqual (1, collection.Find (X509FindType.FindByExtension, "2.5.29.1", false).Count, "FindByExtension/2.5.29.1/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByExtension, "2.5.29.37", false).Count, "FindByExtension/2.5.29.37/false");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Find_FindByExtension_NotOid ()
		{
			collection.Find (X509FindType.FindByExtension, "KeyUsage", false);
		}

		[Test]
		public void Find_FindByIssuerDistinguishedName ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerDistinguishedName, String.Empty, false).Count, "FindByIssuerDistinguishedName/Empty/false");
			// partial match
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerDistinguishedName, "Mono", false).Count, "FindByIssuerDistinguishedName/Mono/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerDistinguishedName, "CASTOR\\poupou", false).Count, "FindByIssuerDistinguishedName/castor/false");
			// full match (requires CN= parts)
			Assert.AreEqual (1, collection.Find (X509FindType.FindByIssuerDistinguishedName, cert1.Issuer, false).Count, "FindByIssuerDistinguishedName/cert1/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByIssuerDistinguishedName, cert2.IssuerName.Name, false).Count, "FindByIssuerDistinguishedName/cert2/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByIssuerDistinguishedName_NotString ()
		{
			collection.Find (X509FindType.FindByIssuerDistinguishedName, 1, false);
		}

		[Test]
		public void Find_FindByIssuerName ()
		{
			// empty
			Assert.AreEqual (collection.Count, collection.Find (X509FindType.FindByIssuerName, String.Empty, false).Count, "FindByIssuerName/Empty/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerName, String.Empty, true).Count, "FindByIssuerName/Empty/true");
			// partial match
			Assert.AreEqual (1, collection.Find (X509FindType.FindByIssuerName, "Mono", false).Count, "FindByIssuerName/Mono/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByIssuerName, "CASTOR\\poupou", false).Count, "FindByIssuerName/castor/false");
			// full match (doesn't like CN= parts)
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerName, cert1.Issuer, false).Count, "FindByIssuerName/cert1/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByIssuerName, cert2.IssuerName.Name, false).Count, "FindByIssuerName/cert2/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByIssuerName_NotString ()
		{
			collection.Find (X509FindType.FindByIssuerName, DateTime.Now, false);
		}

		[Test]
		public void Find_FindByKeyUsage ()
		{
			// empty
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.None, false).Count, "FindByKeyUsage/None/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.None, true).Count, "FindByKeyUsage/None/true");
			// always match if no KeyUsageExtension is present in certificate, EnhancedKeyUsageExtension not considered
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.CrlSign, false).Count, "FindByKeyUsage/CrlSign/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.DataEncipherment, false).Count, "FindByKeyUsage/DataEncipherment/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.DecipherOnly, false).Count, "FindByKeyUsage/DecipherOnly/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false).Count, "FindByKeyUsage/DigitalSignature/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.EncipherOnly, false).Count, "FindByKeyUsage/EncipherOnly/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.KeyAgreement, false).Count, "FindByKeyUsage/KeyAgreement/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.KeyCertSign, false).Count, "FindByKeyUsage/KeyCertSign/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.KeyEncipherment, false).Count, "FindByKeyUsage/KeyEncipherment/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByKeyUsage, X509KeyUsageFlags.NonRepudiation, false).Count, "FindByKeyUsage/NonRepudiation/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByIssuerName_NotX509KeyUsageFlags ()
		{
			collection.Find (X509FindType.FindByKeyUsage, String.Empty, false);
		}

		[Test]
		public void Find_FindBySerialNumber ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySerialNumber, String.Empty, false).Count, "FindBySerialNumber/Empty/false");
			// partial match (start, end)
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySerialNumber, "748B", false).Count, "FindBySerialNumber/748B/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySerialNumber, "4769", false).Count, "FindBySerialNumber/4769/false");
			// full match
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert1.SerialNumber, false).Count, "FindBySerialNumber/cert1/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert2.SerialNumber, false).Count, "FindBySerialNumber/cert2/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert1.SerialNumber.ToLowerInvariant (), false).Count, "FindBySerialNumber/cert1b/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert2.SerialNumber.ToLowerInvariant (), false).Count, "FindBySerialNumber/cert2b/false");
			// full match inverted
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert1.GetSerialNumberString (), false).Count, "FindBySerialNumber/cert1c/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert2.GetSerialNumberString (), false).Count, "FindBySerialNumber/cert2c/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert1.GetSerialNumberString ().ToLowerInvariant (), false).Count, "FindBySerialNumber/cert1d/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySerialNumber, cert2.GetSerialNumberString ().ToLowerInvariant (), false).Count, "FindBySerialNumber/cert2d/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindBySerialNumber_NotString ()
		{
			collection.Find (X509FindType.FindBySerialNumber, DateTime.Now, false);
		}

		[Test]
		public void Find_FindBySubjectDistinguishedName ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectDistinguishedName, String.Empty, false).Count, "FindBySubjectDistinguishedName/Empty/false");
			// partial match
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectDistinguishedName, "Mono", false).Count, "FindBySubjectDistinguishedName/Mono/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectDistinguishedName, "CASTOR\\poupou", false).Count, "FindBySubjectDistinguishedName/castor/false");
			// full match (requires CN= parts) using all lowercase
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySubjectDistinguishedName, cert1.Subject.ToLowerInvariant (), false).Count, "FindBySubjectDistinguishedName/cert1/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySubjectDistinguishedName, cert2.SubjectName.Name.ToLowerInvariant (), false).Count, "FindBySubjectDistinguishedName/cert2/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindBySubjectDistinguishedName_NotString ()
		{
			collection.Find (X509FindType.FindBySubjectDistinguishedName, new object (), false);
		}

		[Test]
		public void Find_FindBySubjectKeyIdentifier ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectKeyIdentifier, String.Empty, false).Count, "FindBySubjectKeyIdentifier/Empty/false");

			X509Certificate2Collection c = new X509Certificate2Collection (collection);
			c.Add (new X509Certificate2 (X509Certificate2Test.cert_8));
			Assert.AreEqual (0, c.Find (X509FindType.FindBySubjectKeyIdentifier, "9D2D73C3B8E34D2928C3", false).Count, "FindBySubjectKeyIdentifier/half/false");
			Assert.AreEqual (1, c.Find (X509FindType.FindBySubjectKeyIdentifier, "9D2D73C3B8E34D2928C365BEA998CBD68A06689C", false).Count, "FindBySubjectKeyIdentifier/full/false");
			Assert.AreEqual (1, c.Find (X509FindType.FindBySubjectKeyIdentifier, "9d2d73c3b8e34d2928c365bea998cbd68a06689c", false).Count, "FindBySubjectKeyIdentifier/full/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindBySubjectKeyIdentifier_NotString ()
		{
			collection.Find (X509FindType.FindBySubjectKeyIdentifier, 1, false);
		}

		[Test]
		public void Find_FindBySubjectName ()
		{
			// empty
			Assert.AreEqual (collection.Count, collection.Find (X509FindType.FindBySubjectName, String.Empty, false).Count, "FindBySubjectName/Empty/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectName, String.Empty, true).Count, "FindBySubjectName/Empty/true");
			// partial match (using inverted case)
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySubjectName, "farscap", false).Count, "FindBySubjectName/Mono/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindBySubjectName, "castor\\POUPOU", false).Count, "FindBySubjectName/castor/false");
			// full match (doesn't like CN= parts)
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectName, cert1.Subject, false).Count, "FindBySubjectName/cert1/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindBySubjectName, cert2.SubjectName.Name, false).Count, "FindBySubjectName/cert2/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindBySubjectName_NotString ()
		{
			collection.Find (X509FindType.FindBySubjectName, 'c', false);
		}

		[Test]
		public void Find_FindByTemplateName ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTemplateName, String.Empty, false).Count, "FindByTemplateName/Empty/false");
			// wilcard match
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTemplateName, "*", false).Count, "FindByTemplateName/Mono/false");
			// FIXME - need a positive test case
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByTemplateName_NotString ()
		{
			collection.Find (X509FindType.FindByTemplateName, 0, false);
		}

		[Test]
		public void Find_FindByThumbprint ()
		{
			// empty
			Assert.AreEqual (0, collection.Find (X509FindType.FindByThumbprint, String.Empty, false).Count, "FindByThumbprint/Empty/false");
			// partial match (start, end)
			Assert.AreEqual (0, collection.Find (X509FindType.FindByThumbprint, "3029", false).Count, "FindByThumbprint/3029/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByThumbprint, "8529", false).Count, "FindByThumbprint/8529/false");
			// full match
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert1.Thumbprint, false).Count, "FindByThumbprint/cert1/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert2.Thumbprint, false).Count, "FindByThumbprint/cert2/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert1.Thumbprint.ToLowerInvariant (), false).Count, "FindByThumbprint/cert1b/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert2.Thumbprint.ToLowerInvariant (), false).Count, "FindByThumbprint/cert2b/false");
			// full match inverted
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert1.GetCertHashString (), false).Count, "FindByThumbprint/cert1c/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert2.GetCertHashString (), false).Count, "FindByThumbprint/cert2c/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert1.GetCertHashString ().ToLowerInvariant (), false).Count, "FindByThumbprint/cert1d/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByThumbprint, cert2.GetCertHashString ().ToLowerInvariant (), false).Count, "FindByThumbprint/cert2d/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByThumbprint_NotString ()
		{
			collection.Find (X509FindType.FindByThumbprint, 0, false);
		}

		[Test]
		public void Find_FindByTimeExpired ()
		{
			// now (valid from today until 2039)
			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeExpired, DateTime.Now, false).Count, "FindByTimeExpired/Now/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeExpired, new DateTime (631108726620000000), false).Count, "FindByTimeExpired/2000/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByTimeExpired, new DateTime (644392619990000000), false).Count, "FindByTimeExpired/2042/false");

			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeExpired, cert1.NotAfter, false).Count, "FindByTimeExpired/cert1.NotAfter/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeExpired, cert1.NotBefore, false).Count, "FindByTimeExpired/cert1.NotBefore/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeExpired, cert2.NotAfter, false).Count, "FindByTimeExpired/cert2.NotAfter/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeExpired, cert2.NotBefore, false).Count, "FindByTimeExpired/cert2.NotBefore/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByTimeExpired_NotDateTime ()
		{
			collection.Find (X509FindType.FindByTimeExpired, String.Empty, false);
		}

		[Test]
		public void Find_FindByTimeNotYetValid ()
		{
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeNotYetValid, DateTime.Now, false).Count, "FindByTimeNotYetValid/Now/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByTimeNotYetValid, new DateTime (631108726620000000), false).Count, "FindByTimeNotYetValid/2000/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeNotYetValid, new DateTime (644392619990000000), false).Count, "FindByTimeNotYetValid/2042/false");

			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeNotYetValid, cert1.NotAfter, false).Count, "FindByTimeNotYetValid/cert1.NotAfter/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeNotYetValid, cert1.NotBefore, false).Count, "FindByTimeNotYetValid/cert1.NotBefore/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeNotYetValid, cert2.NotAfter, false).Count, "FindByTimeNotYetValid/cert2.NotAfter/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeNotYetValid, cert2.NotBefore, false).Count, "FindByTimeNotYetValid/cert2.NotBefore/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByTimeNotYetValid_NotDateTime ()
		{
			collection.Find (X509FindType.FindByTimeNotYetValid, DateTime.Now.ToString (), false);
		}

		[Test]
		public void Find_FindByTimeValid ()
		{
			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeValid, DateTime.Now, false).Count, "FindByTimeValid/Now/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeValid, new DateTime (631108726620000000), false).Count, "FindByTimeValid/2000/false");
			Assert.AreEqual (0, collection.Find (X509FindType.FindByTimeValid, new DateTime (644392619990000000), false).Count, "FindByTimeValid/2042/false");

			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeValid, cert1.NotAfter, false).Count, "FindByTimeValid/cert1.NotAfter/false");
			Assert.AreEqual (1, collection.Find (X509FindType.FindByTimeValid, cert1.NotBefore, false).Count, "FindByTimeValid/cert1.NotBefore/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByTimeValid, cert2.NotAfter, false).Count, "FindByTimeValid/cert2.NotAfter/false");
			Assert.AreEqual (2, collection.Find (X509FindType.FindByTimeValid, cert2.NotBefore, false).Count, "FindByTimeValid/cert2.NotBefore/false");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Find_FindByTimeValid_NotDateTime ()
		{
			collection.Find (X509FindType.FindByTimeValid, String.Empty, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_X509Certificate2_Null ()
		{
			collection.Remove ((X509Certificate2) null);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Remove_X509Certificate2_Empty ()
		{
			single.Remove (cert_empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveRange_X509Certificate2Collection_Null ()
		{
			collection.RemoveRange ((X509Certificate2Collection) null);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RemoveRange_X509Certificate2Collection_EmptyCert ()
		{
			collection.RemoveRange (new X509Certificate2Collection (cert_empty));
		}

		[Test]
		public void RemoveRange_X509Certificate2Collection_Empty ()
		{
			Assert.AreEqual (2, collection.Count, "Count/before");
			collection.RemoveRange (empty);
			Assert.AreEqual (2, collection.Count, "Count/after");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveRange_X509Certificate2Array_Null ()
		{
			collection.RemoveRange ((X509Certificate2[]) null);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RemoveRange_X509Certificate2Array_EmptyCert ()
		{
			collection.RemoveRange (new X509Certificate2[1] { cert_empty });
		}

		[Test]
		public void RemoveRange_X509Certificate2Array_Empty ()
		{
			Assert.AreEqual (2, collection.Count, "Count/before");
			collection.RemoveRange (new X509Certificate2[0]);
			Assert.AreEqual (2, collection.Count, "Count/after");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void MixedCollection_Indexer ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			c.Add (new X509Certificate (X509Certificate2Test.farscape_pfx, "farscape"));
			Assert.IsTrue ((c[0] is X509Certificate), "X509Certificate/0");
			// it's impossible to use the this[int] indexer to get the object in the collection
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void MixedCollection_Enumerator ()
		{
			X509Certificate2Collection c = new X509Certificate2Collection ();
			c.Add (new X509Certificate (X509Certificate2Test.farscape_pfx, "farscape"));
			foreach (object o in c) {
				Assert.IsTrue ((o is X509Certificate), "X509Certificate");
			}
		}

		[Test]
		public void X509Certificate2CollectionFindBySubjectName_Test ()
		{
			// Created with mono makecert
			// makecert -n "O=Root, CN=MyCNName, T=SomeElse" -r <filename>
			const string Cert = "MIIB6zCCAVSgAwIBAgIQEshQw4bf1kSsYsUoLCjlpTANBgkqhkiG9w0BAQUFADA1MQ0wCwYDVQQKEwRSb290MREwDwYDVQQDEwhNeUNOTmFtZTERMA8GA1UEDBMIU29tZUVsc2UwHhcNMTIwMzE1MTUzNjE0WhcNMzkxMjMxMjM1OTU5WjA1MQ0wCwYDVQQKEwRSb290MREwDwYDVQQDEwhNeUNOTmFtZTERMA8GA1UEDBMIU29tZUVsc2UwgZ0wDQYJKoZIhvcNAQEBBQADgYsAMIGHAoGBAI8A3ay+oRKRBAD4oojA2s4feHBHn5OafJ+Vxap7wsd3IF/qBrXdnFLxfvLltCSZDarajwTjxX2rhT7Q0hm3Yyy6DnyaMoL8u//c6HCv47SFNJ4JEgu2WP9M/xNU2m+JiABX+K/nwoWfE1VIYueJWL9ftCBOG099QBCsCrpdFUcbAgERMA0GCSqGSIb3DQEBBQUAA4GBAHV+uMPliZPhfLdcMGIbYQnWY2m4YrU/IvqZK4HTKyzG/heAp7+OvkGiC0YJHtvWehgZUV9ukVEbl93rCKmXlb6BuPgN60U1iLYJQ9nAVHm7fRoAjvjDj3CGFtmYb81sYu8sc5GHqsCbvTKHwW/x2O3uLJBM5ApDlcczmgdm8xqQ";

			var cerBytes = Convert.FromBase64String (Cert);
			var cert = new X509Certificate2 (cerBytes);
			var collection = new X509Certificate2Collection ();

			var found = collection.Find (X509FindType.FindBySubjectName, "SomeElse", false);
			Assert.AreEqual (0, found.Count, "empty");
			
			collection.Add (cert);

			collection.Find (X509FindType.FindBySubjectName, "T=SomeElse", false);
			Assert.AreEqual (0, found.Count, "with prefix");
			
			found = collection.Find (X509FindType.FindBySubjectName, "SomeElse", false);
			Assert.That (found.Count == 1, "full");
			
			found = collection.Find (X509FindType.FindBySubjectName, "Else", false);
			Assert.That (found.Count == 1, "partial");
			
			Assert.That (found [0].SubjectName.Name.Contains ("O=Root"));
			Assert.That (found [0].SubjectName.Name.Contains ("T=SomeElse"));
			Assert.That (found [0].SubjectName.Name.Contains ("CN=MyCNName"));
			found = collection.Find (X509FindType.FindBySubjectName, "MyCNName", false);
			Assert.IsTrue (found.Count == 1);
			Assert.That (found [0].SubjectName.Name.Contains ("O=Root"));
			Assert.That (found [0].SubjectName.Name.Contains ("T=SomeElse"));
			Assert.That (found [0].SubjectName.Name.Contains ("CN=MyCNName"));
			found = collection.Find (X509FindType.FindBySubjectName, "Root", false);
			Assert.IsTrue (found.Count == 1);
			Assert.That (found [0].SubjectName.Name.Contains ("O=Root"));
			Assert.That (found [0].SubjectName.Name.Contains ("T=SomeElse"));
			Assert.That (found [0].SubjectName.Name.Contains ("CN=MyCNName"));
			found = collection.Find (X509FindType.FindBySubjectName, "SomeRandomStringThatDoesn'tExist", false);
			Assert.AreEqual (0, found.Count);
		}
	}
}

#endif
