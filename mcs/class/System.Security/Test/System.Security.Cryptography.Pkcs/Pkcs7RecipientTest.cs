//
// Pkcs7RecipientTest.cs - NUnit tests for Pkcs7Recipient
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs7RecipientTest : Assertion {

		private X509CertificateEx GetCertificate (bool includePrivateKey) 
		{
			return new X509CertificateEx (@"c:\farscape.p12.pfx", "farscape");
		}

		[Test]
		public void IssuerAndSerialNumber () 
		{
			X509CertificateEx x509 = GetCertificate (true); 
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.IssuerAndSerialNumber, x509);
			AssertEquals ("RecipientIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, p7r.RecipientIdentifierType);
			AssertEquals ("Certificate", x509.Thumbprint, p7r.Certificate.Thumbprint);
		}

		[Test]
		public void SubjectKeyIdentifier () 
		{
			X509CertificateEx x509 = GetCertificate (true); 
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.SubjectKeyIdentifier, x509);
			AssertEquals ("RecipientIdentifierType", SubjectIdentifierType.SubjectKeyIdentifier, p7r.RecipientIdentifierType);
			AssertEquals ("Certificate", x509.Thumbprint, p7r.Certificate.Thumbprint);
		}

		[Test]
		public void Unknown () 
		{
			X509CertificateEx x509 = GetCertificate (true); 
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.Unknown, x509);
			AssertEquals ("RecipientIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, p7r.RecipientIdentifierType);
			AssertEquals ("Certificate", x509.Thumbprint, p7r.Certificate.Thumbprint);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullCertificate () 
		{
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.IssuerAndSerialNumber, null);
		}
	}
}

#endif
