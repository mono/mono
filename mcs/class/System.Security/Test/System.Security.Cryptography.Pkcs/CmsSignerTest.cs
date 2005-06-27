//
// CmsSignerTest.cs - NUnit tests for CmsSigner
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class CmsSignerTest : Assertion {

		static byte[] asnNull = { 0x05, 0x00 };
		static string sha1Oid = "1.3.14.3.2.26";
		static string sha1Name = "sha1";
		static string rsaOid = "1.2.840.113549.1.1.1";
		static string rsaName = "RSA";

		[Test]
		public void ConstructorEmpty () 
		{
			CmsSigner ps = new CmsSigner ();
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorIssuerAndSerialNumber () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorSubjectKeyIdentifier () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.SubjectKeyIdentifier);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorUnknown ()
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.Unknown);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			// Unknown is converted to IssuerAndSerialNumber
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		// TODO: return valid x509 certifiate with private key
		private X509Certificate2 GetValidCertificateWithPrivateKey () 
		{
			X509Certificate2 x509 = new X509Certificate2 ();
			return x509;
		}

		[Test]
		public void ConstructorX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorX509CertificateExEmpty () 
		{
			X509Certificate2 x509 = new X509Certificate2 (); // empty
			CmsSigner ps = new CmsSigner (x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorX509CertificateExNull () 
		{
			X509Certificate2 x509 = null;
			CmsSigner ps = new CmsSigner (x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorIssuerAndSerialNumberX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber, x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorSubjectKeyIdentifierX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.SubjectKeyIdentifier, x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void ConstructorUnknownX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.Unknown, x509);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNotNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			// Unknown is converted to IssuerAndSerialNumber
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorIssuerAndSerialNumberX509CertificateExNull () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber, null);
			// default properties
			AssertEquals ("SignedAttributes", 0, ps.SignedAttributes.Count);
			AssertNull ("Certificate", ps.Certificate);
			AssertEquals ("DigestAlgorithm.FriendlyName", sha1Name, ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", sha1Oid, ps.DigestAlgorithm.Value);
			AssertEquals ("IncludeOption", X509IncludeOption.ExcludeRoot, ps.IncludeOption);
			AssertEquals ("SignerIdentifierType", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			AssertEquals ("UnsignedAttributes", 0, ps.UnsignedAttributes.Count);
		}

		[Test]
		public void SignedAttributes ()
		{
			CmsSigner ps = new CmsSigner ();
			AssertEquals ("SignedAttributes=0", 0, ps.SignedAttributes.Count);
			ps.SignedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			AssertEquals ("SignedAttributes=1", 1, ps.SignedAttributes.Count);
		}

		[Test]
		public void Certificate () 
		{
			CmsSigner ps = new CmsSigner ();
			AssertNull ("Certificate=default(null)", ps.Certificate);
			ps.Certificate = GetValidCertificateWithPrivateKey ();
			AssertNotNull ("Certificate!=null", ps.Certificate);
			ps.Certificate = null;
			AssertNull ("Certificate=null", ps.Certificate);
		}

		[Test]
		public void Digest () 
		{
			CmsSigner ps = new CmsSigner ();
			ps.DigestAlgorithm = new Oid ("1.2.840.113549.2.5");
			AssertEquals ("DigestAlgorithm.FriendlyName", "md5", ps.DigestAlgorithm.FriendlyName);
			AssertEquals ("DigestAlgorithm.Value", "1.2.840.113549.2.5", ps.DigestAlgorithm.Value);
			ps.DigestAlgorithm = null;
			AssertNull ("DigestAlgorithm=null", ps.DigestAlgorithm);
		}

		[Test]
		public void IncludeOption () 
		{
			CmsSigner ps = new CmsSigner ();
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
			CmsSigner ps = new CmsSigner ();
			ps.SignerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			AssertEquals ("IssuerAndSerialNumber", SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType);
			ps.SignerIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;
			AssertEquals ("SubjectKeyIdentifier", SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SubjectIdentifierTypeUnknown () 
		{
			CmsSigner ps = new CmsSigner ();
			ps.SignerIdentifierType = SubjectIdentifierType.Unknown;
		}

		[Test]
		public void UnauthenticatedAttributes () 
		{
			CmsSigner ps = new CmsSigner ();
			AssertEquals ("UnsignedAttributes=0", 0, ps.UnsignedAttributes.Count);
			ps.UnsignedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			AssertEquals ("UnsignedAttributes=1", 1, ps.UnsignedAttributes.Count);
		}
	}
}

#endif
