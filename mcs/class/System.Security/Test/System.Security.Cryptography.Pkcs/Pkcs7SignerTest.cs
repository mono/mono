//
// Pkcs7SignerTest.cs - NUnit tests for Pkcs7Signer
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

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs7SignerTest : Assertion {

		static byte[] asnNull = { 0x05, 0x00 };
		static string sha1Oid = "1.3.14.3.2.26";
		static string sha1Name = "sha1";
		static string rsaOid = "1.2.840.113549.1.1.1";
		static string rsaName = "RSA";

		[Test]
		public void ConstructorEmpty () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorIssuerAndSerialNumber () 
		{
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.IssuerAndSerialNumber);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorSubjectKeyIdentifier () 
		{
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.SubjectKeyIdentifier);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorUnknown ()
		{
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.Unknown);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			// Unknown is converted to IssuerAndSerialNumber
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		// TODO: return valid x509 certifiate with private key
		private X509CertificateEx GetValidCertificateWithPrivateKey () 
		{
			X509CertificateEx x509 = new X509CertificateEx ();
			return x509;
		}

		[Test]
		public void ConstructorX509CertificateEx () 
		{
			X509CertificateEx x509 = GetValidCertificateWithPrivateKey ();
			Pkcs7Signer ps = new Pkcs7Signer (x509);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorX509CertificateExEmpty () 
		{
			X509CertificateEx x509 = new X509CertificateEx (); // empty
			Pkcs7Signer ps = new Pkcs7Signer (x509);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorX509CertificateExNull () 
		{
			Pkcs7Signer ps = new Pkcs7Signer (null);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorIssuerAndSerialNumberX509CertificateEx () 
		{
			X509CertificateEx x509 = GetValidCertificateWithPrivateKey ();
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.IssuerAndSerialNumber, x509);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorSubjectKeyIdentifierX509CertificateEx () 
		{
			X509CertificateEx x509 = GetValidCertificateWithPrivateKey ();
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.SubjectKeyIdentifier, x509);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void ConstructorUnknownX509CertificateEx () 
		{
			X509CertificateEx x509 = GetValidCertificateWithPrivateKey ();
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.Unknown, x509);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			// Unknown is converted to IssuerAndSerialNumber
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorIssuerAndSerialNumberX509CertificateExNull () 
		{
			Pkcs7Signer ps = new Pkcs7Signer (SubjectIdentifierType.IssuerAndSerialNumber, null);
			// default properties
			AssertEquals ("AuthenticatedAttributes", 0, ps.AuthenticatedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnauthenticatedAttributes", 0, ps.UnauthenticatedAttributes.Count);
		}

		[Test]
		public void AuthenticatedAttributes () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			AssertEquals ("AuthenticatedAttributes=0", 0, ps.AuthenticatedAttributes.Count);
			ps.AuthenticatedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			AssertEquals ("AuthenticatedAttributes=1", 1, ps.AuthenticatedAttributes.Count);
		}

		[Test]
		public void Certificate () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			AssertNull ("Certificate=default(null)", ps.Certificate);
			ps.Certificate = GetValidCertificateWithPrivateKey ();
			AssertNotNull ("Certificate!=null", ps.Certificate);
			ps.Certificate = null;
			AssertNull ("Certificate=null", ps.Certificate);
		}

		[Test]
		public void Digest () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			ps.DigestAlgorithm = new Oid ("1.2.840.113549.2.5");
			AssertEquals ("DigestAlgorithm.FriendlyName", "md5", ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", "1.2.840.113549.2.5", ps.DigestAlgorithm.Value);
			ps.DigestAlgorithm = null;
			AssertNull ("DigestAlgorithm=null", ps.DigestAlgorithm);
		}

		[Test]
		public void IncludeOption () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			ps.IncludeOption = X509IncludeOption.EndCertOnly;
			AssertEquals ("EndCertOnly", X509IncludeOption.EndCertOnly, ps.IncludeOption);
			ps.IncludeOption = X509IncludeOption.ExcludeRoot;
			AssertEquals ("ExcludeRoot", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			ps.IncludeOption = X509IncludeOption.None;
			AssertEquals ("None", X509IncludeOption.None, ps.IncludeOption);
			ps.IncludeOption = X509IncludeOption.WholeChain;
			AssertEquals ("WholeChain", X509IncludeOption.WholeChain, ps.IncludeOption);
		}

		[Test]
		public void SubjectIdentifierTypeProperty () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			ps.SignerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			AssertEquals ("IssuerAndSerialNumber", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			ps.SignerIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;
			AssertEquals ("SubjectKeyIdentifier", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SubjectIdentifierTypeUnknown () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			ps.SignerIdentifierType = SubjectIdentifierType.Unknown;
		}

		[Test]
		public void UnauthenticatedAttributes () 
		{
			Pkcs7Signer ps = new Pkcs7Signer ();
			AssertEquals ("UnauthenticatedAttributes=0", 0, ps.UnauthenticatedAttributes.Count);
			ps.UnauthenticatedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			AssertEquals ("UnauthenticatedAttributes=1", 1, ps.UnauthenticatedAttributes.Count);
		}
	}
}

#endif
