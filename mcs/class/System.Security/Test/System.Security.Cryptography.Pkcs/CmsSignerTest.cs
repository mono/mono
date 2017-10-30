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
#if !MOBILE

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class CmsSignerTest {

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
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorIssuerAndSerialNumber () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorSubjectKeyIdentifier () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.SubjectKeyIdentifier);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorUnknown ()
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.Unknown);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			// Unknown is converted to IssuerAndSerialNumber
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
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
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNotNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorX509CertificateExEmpty () 
		{
			X509Certificate2 x509 = new X509Certificate2 (); // empty
			CmsSigner ps = new CmsSigner (x509);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNotNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorX509CertificateExNull () 
		{
			X509Certificate2 x509 = null;
			CmsSigner ps = new CmsSigner (x509);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorIssuerAndSerialNumberX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber, x509);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNotNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorSubjectKeyIdentifierX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.SubjectKeyIdentifier, x509);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNotNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void ConstructorUnknownX509CertificateEx () 
		{
			X509Certificate2 x509 = GetValidCertificateWithPrivateKey ();
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.Unknown, x509);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNotNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			// Unknown is converted to IssuerAndSerialNumber
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorIssuerAndSerialNumberX509CertificateExNull () 
		{
			CmsSigner ps = new CmsSigner (SubjectIdentifierType.IssuerAndSerialNumber, null);
			// default properties
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes");
			Assert.IsNull (ps.Certificate, "Certificate");
			Assert.AreEqual (sha1Name, ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual (sha1Oid, ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "IncludeOption");
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "SignerIdentifierType");
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes");
		}

		[Test]
		public void SignedAttributes ()
		{
			CmsSigner ps = new CmsSigner ();
			Assert.AreEqual (0, ps.SignedAttributes.Count, "SignedAttributes=0");
			ps.SignedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			Assert.AreEqual (1, ps.SignedAttributes.Count, "SignedAttributes=1");
		}

		[Test]
		public void Certificate () 
		{
			CmsSigner ps = new CmsSigner ();
			Assert.IsNull (ps.Certificate, "Certificate=default(null)");
			ps.Certificate = GetValidCertificateWithPrivateKey ();
			Assert.IsNotNull (ps.Certificate, "Certificate!=null");
			ps.Certificate = null;
			Assert.IsNull (ps.Certificate, "Certificate=null");
		}

		[Test]
		public void Digest () 
		{
			CmsSigner ps = new CmsSigner ();
			ps.DigestAlgorithm = new Oid ("1.2.840.113549.2.5");
			Assert.AreEqual ("md5", ps.DigestAlgorithm.FriendlyName, "DigestAlgorithm.FriendlyName");
			Assert.AreEqual ("1.2.840.113549.2.5", ps.DigestAlgorithm.Value, "DigestAlgorithm.Value");
			ps.DigestAlgorithm = null;
			Assert.IsNull (ps.DigestAlgorithm, "DigestAlgorithm=null");
		}

		[Test]
		public void IncludeOption () 
		{
			CmsSigner ps = new CmsSigner ();
			ps.IncludeOption = X509IncludeOption.EndCertOnly;
			Assert.AreEqual (X509IncludeOption.EndCertOnly, ps.IncludeOption, "EndCertOnly");
			ps.IncludeOption = X509IncludeOption.ExcludeRoot;
			Assert.AreEqual (X509IncludeOption.ExcludeRoot, ps.IncludeOption, "ExcludeRoot");
			ps.IncludeOption = X509IncludeOption.None;
			Assert.AreEqual (X509IncludeOption.None, ps.IncludeOption, "None");
			ps.IncludeOption = X509IncludeOption.WholeChain;
			Assert.AreEqual (X509IncludeOption.WholeChain, ps.IncludeOption, "WholeChain");
		}

		[Test]
		public void SubjectIdentifierTypeProperty () 
		{
			CmsSigner ps = new CmsSigner ();
			ps.SignerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			Assert.AreEqual (SubjectIdentifierType.IssuerAndSerialNumber, ps.SignerIdentifierType, "IssuerAndSerialNumber");
			ps.SignerIdentifierType = SubjectIdentifierType.SubjectKeyIdentifier;
			Assert.AreEqual (SubjectIdentifierType.SubjectKeyIdentifier, ps.SignerIdentifierType, "SubjectKeyIdentifier");
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
			Assert.AreEqual (0, ps.UnsignedAttributes.Count, "UnsignedAttributes=0");
			ps.UnsignedAttributes.Add (new Pkcs9DocumentDescription ("mono"));
			Assert.AreEqual (1, ps.UnsignedAttributes.Count, "UnsignedAttributes=1");
		}
	}
}
#endif
