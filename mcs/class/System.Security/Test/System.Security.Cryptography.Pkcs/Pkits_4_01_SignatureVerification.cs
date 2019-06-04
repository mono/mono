//
// Pkits_4_01_SignatureVerification.cs -
//	NUnit tests for Pkits 4.1 : Signature Verification
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
#if !MOBILE

using NUnit.Framework;

using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	[Category ("PKITS")]
	[Category ("NotWorking")]
	public class Pkits_4_01_SignatureVerification: PkitsTest {

		public X509Certificate2 BadSignedCACert {
			get { return GetCertificate ("BadSignedCACert.crt"); }
		}

		public X509Certificate2 DSACACert {
			get { return GetCertificate ("DSACACert.crt"); }
		}

		public X509Certificate2 DSAParametersInheritedCACert {
			get { return GetCertificate ("DSAParametersInheritedCACert.crt"); }
		}

		[Test]
		public void T1_ValidSignature ()
		{
			byte[] data = GetData ("SignedValidSignaturesTest1.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsTrue (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("ValidCertificatePathTest1EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (GoodCACert), "GoodCACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-3B-20-63-68-61-72-73-65-74-3D-69-73-6F-2D-38-38-35-39-2D-31-0D-0A-43-6F-6E-74-65-6E-74-2D-54-72-61-6E-73-66-65-72-2D-45-6E-63-6F-64-69-6E-67-3A-20-37-62-69-74-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=Good CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("01", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}

		[Test]
		public void T2_InvalidCASignature ()
		{
			byte[] data = GetData ("SignedInvalidCASignatureTest2.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsFalse (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("InvalidCASignatureTest2EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (BadSignedCACert), "BadSignedCACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-3B-20-63-68-61-72-73-65-74-3D-69-73-6F-2D-38-38-35-39-2D-31-0D-0A-43-6F-6E-74-65-6E-74-2D-54-72-61-6E-73-66-65-72-2D-45-6E-63-6F-64-69-6E-67-3A-20-37-62-69-74-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=Bad Signed CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("01", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}

		[Test]
		public void T3_InvalidEESignature ()
		{
			byte[] data = GetData ("SignedInvalidEESignatureTest3.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsFalse (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("InvalidEESignatureTest3EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (GoodCACert), "GoodCACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-3B-20-63-68-61-72-73-65-74-3D-69-73-6F-2D-38-38-35-39-2D-31-0D-0A-43-6F-6E-74-65-6E-74-2D-54-72-61-6E-73-66-65-72-2D-45-6E-63-6F-64-69-6E-67-3A-20-37-62-69-74-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=Good CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("02", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}

		[Test]
		public void T4_ValidDSASignatures ()
		{
			byte[] data = GetData ("SignedValidDSASignaturesTest4.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsTrue (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("ValidDSASignaturesTest4EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (DSACACert), "DSACACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-0D-0A-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E-0D-0A", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=DSA CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("01", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}

		[Test]
		public void T5_ValidDSAParameterInheritance ()
		{
			byte[] data = GetData ("SignedValidDSAParameterInheritanceTest5.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsTrue (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("ValidDSAParameterInheritanceTest5EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (DSAParametersInheritedCACert), "DSAParametersInheritedCACert");
			Assert.IsTrue (cms.Certificates.Contains (DSACACert), "DSACACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-0D-0A-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E-0D-0A", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=DSA Parameters Inherited CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("01", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}

		[Test]
		public void T6_InvalidDSASignatures ()
		{
			byte[] data = GetData ("SignedInvalidDSASignatureTest6.eml");
			SignedCms cms = new SignedCms ();
			cms.Decode (data);
			Assert.IsTrue (CheckHash (cms), "CheckHash");
			Assert.IsFalse (CheckSignature (cms), "CheckSignature");

			X509Certificate2 ee = GetCertificate ("InvalidDSASignatureTest6EE.crt");
			// certificates aren't in any particuliar order
			Assert.IsTrue (cms.Certificates.Contains (ee), "EE");
			Assert.IsTrue (cms.Certificates.Contains (DSACACert), "DSACACert");
			Assert.IsFalse (cms.Detached, "Detached");
			Assert.AreEqual (1, cms.Version, "Version");
			Assert.AreEqual ("1.2.840.113549.1.7.1", cms.ContentInfo.ContentType.Value, "ContentInfo.Oid");
			Assert.AreEqual ("43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-74-65-78-74-2F-70-6C-61-69-6E-0D-0A-0D-0A-0D-0A-54-68-69-73-20-69-73-20-61-20-73-61-6D-70-6C-65-20-73-69-67-6E-65-64-20-6D-65-73-73-61-67-65-2E-0D-0A", BitConverter.ToString (cms.ContentInfo.Content), "ContentInfo.Content");
			Assert.AreEqual (1, cms.SignerInfos.Count, "SignerInfos.Count");
			Assert.AreEqual (ee, cms.SignerInfos[0].Certificate, "SignerInfos[0].Certificate");
			Assert.AreEqual (0, cms.SignerInfos[0].CounterSignerInfos.Count, "SignerInfos[0].CounterSignerInfos.Count");
			Assert.AreEqual ("1.3.14.3.2.26", cms.SignerInfos[0].DigestAlgorithm.Value, "cms.SignerInfos[0].DigestAlgorithm");
			Assert.AreEqual (0, cms.SignerInfos[0].SignedAttributes.Count, "SignerInfos[0].SignedAttributes.Count");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, cms.SignerInfos[0].SignerIdentifier.Type, "SignerInfos[0].SignerIdentifier.Type");
			X509IssuerSerial xis = (X509IssuerSerial) cms.SignerInfos[0].SignerIdentifier.Value;
			Assert.AreEqual ("CN=DSA CA, O=Test Certificates, C=US", xis.IssuerName, "SignerInfos[0].SignerIdentifier.Value.IssuerName");
			Assert.AreEqual ("03", xis.SerialNumber, "SignerInfos[0].SignerIdentifier.Value.SerialNumber");
			Assert.AreEqual (0, cms.SignerInfos[0].UnsignedAttributes.Count, "SignerInfos[0].UnsignedAttributes.Count");
			Assert.AreEqual (1, cms.SignerInfos[0].Version, "SignerInfos[0].Version");
		}
	}
}
#endif
