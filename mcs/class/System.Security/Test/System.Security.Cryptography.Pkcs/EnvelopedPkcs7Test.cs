//
// EnvelopedPkcs7Test.cs - NUnit tests for EnvelopedPkcs7
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
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class EnvelopedPkcs7Test : Assertion {

		static byte[] asnNull = { 0x05, 0x00 };
		static string tdesOid = "1.2.840.113549.3.7";
		static string tdesName = "3des";
		static string p7DataOid = "1.2.840.113549.1.7.1";
		static string p7DataName = "PKCS 7 Data";

		private void DefaultProperties (EnvelopedPkcs7 ep, int contentLength, int version) 
		{
			AssertEquals ("Certificates", 0, ep.Certificates.Count);
			AssertEquals ("ContentEncryptionAlgorithm.KeyLength", 0, ep.ContentEncryptionAlgorithm.KeyLength);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.FriendlyName", tdesName, ep.ContentEncryptionAlgorithm.Oid.FriendlyName);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.Value", tdesOid, ep.ContentEncryptionAlgorithm.Oid.Value);
			AssertEquals ("ContentEncryptionAlgorithm.Parameters", 0, ep.ContentEncryptionAlgorithm.Parameters.Length);
			AssertEquals ("ContentInfo.ContentType.FriendlyName", p7DataName, ep.ContentInfo.ContentType.FriendlyName);
			AssertEquals ("ContentInfo.ContentType.Value", p7DataOid, ep.ContentInfo.ContentType.Value);
			AssertEquals ("ContentInfo.Content", contentLength, ep.ContentInfo.Content.Length);
			AssertEquals ("RecipientInfos", 0, ep.RecipientInfos.Count);
			AssertEquals ("UnprotectedAttributes", 0, ep.UnprotectedAttributes.Count);
			AssertEquals ("Version", version, ep.Version);
		}

		private X509CertificateEx GetCertificate (bool includePrivateKey) 
		{
			return new X509CertificateEx (@"c:\farscape.p12.pfx", "farscape");
		}

		[Test]
		public void ConstructorEmpty () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			DefaultProperties (ep, 0, 0);
		}

		[Test]
		public void ConstructorContentInfo () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (ci);
			DefaultProperties (ep, asnNull.Length, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorContentInfoNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (null);
		}

		[Test]
		public void ConstructorContentInfoAlgorithmIdentifier () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (ci, ai);
			DefaultProperties (ep, 2, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorContentInfoNullAlgorithmIdentifier () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (null, ai);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorContentInfoAlgorithmIdentifierNull () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (ci, null);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeIssuerAndSerialNumberContentInfoAlgorithmIdentifier () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.IssuerAndSerialNumber, ci, ai);
			DefaultProperties (ep, 2, 0);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeSubjectKeyIdentifierContentInfoAlgorithmIdentifier () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.SubjectKeyIdentifier, ci, ai);
			DefaultProperties (ep, 2, 2);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeUnknownContentInfoAlgorithmIdentifier () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.Unknown, ci, ai);
			DefaultProperties (ep, 2, 0);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeIssuerAndSerialNumberContentInfo () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.IssuerAndSerialNumber, ci);
			DefaultProperties (ep, 2, 0);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeSubjectKeyIdentifierContentInfo () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.SubjectKeyIdentifier, ci);
			DefaultProperties (ep, 2, 2);
		}

		[Test]
		public void ConstructorSubjectIdentifierTypeUnknownContentInfo () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.Unknown, ci);
			DefaultProperties (ep, 2, 0);
		}

		[Test]
		public void Decode () 
		{
			byte[] encoded = { 0x30, 0x82, 0x01, 0x1C, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x03, 0xA0, 0x82, 0x01, 0x0D, 0x30, 0x82, 0x01, 0x09, 0x02, 0x01, 0x00, 0x31, 0x81, 0xD6, 0x30, 0x81, 0xD3, 0x02, 0x01, 0x00, 0x30, 0x3C, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x02, 0x10, 0x91, 0xC4, 0x4B, 0x0D, 0xB7, 0xD8, 0x10, 0x84, 0x42, 0x26, 0x71, 0xB3, 0x97, 0xB5, 0x00, 0x97, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x04, 0x81, 0x80, 0xCA, 0x4B, 0x97, 0x9C, 0xAB, 0x79, 0xC6, 0xDF, 0x6A, 0x27, 0xC7, 0x24, 0xC4, 0x5E, 0x3B, 0x31, 0xAD, 0xBC, 0x25, 0xE6, 0x38, 0x5E, 0x79, 0x26, 0x0E, 0x68, 0x46, 0x1D, 0x21, 0x81, 0x38, 0x92, 0xEC, 0xCB, 0x7C, 0x91, 0xD6, 0x09, 0x38, 0x91, 0xCE, 0x50, 0x5B, 0x70, 0x31, 0xB0, 0x9F, 0xFC, 0xE2, 0xEE, 0x45, 0xBC, 0x4B, 0xF8, 0x9A, 0xD9, 0xEE, 0xE7, 0x4A, 0x3D, 0xCD, 0x8D, 0xFF, 0x10, 0xAB, 0xC8, 0x19, 0x05, 0x54, 0x5E, 0x40, 0x7A, 0xBE, 0x2B, 0xD7, 0x22, 0x97, 0xF3, 0x23, 0xAF, 0x50, 0xF5, 0xEB, 0x43, 0x06, 0xC3, 0xFB, 0x17, 0xCA, 0xBD, 0xAD, 0x28, 0xD8, 0x10, 0x0F, 0x61, 0xCE, 0xF8, 0x25, 0x70, 0xF6, 0xC8, 0x1E, 0x7F, 0x82, 0xE5, 0x94, 0xEB, 0x11, 0xBF, 0xB8, 0x6F, 0xEE, 0x79, 0xCD, 0x63, 0xDD, 0x59, 0x8D, 0x25, 0x0E, 0x78, 0x55, 0xCE, 0x21, 0xBA, 0x13, 0x6B, 0x30, 0x2B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x01, 0x30, 0x14, 0x06, 0x08, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x03, 0x07, 0x04, 0x08, 0x8C, 0x5D, 0xC9, 0x87, 0x88, 0x9C, 0x05, 0x72, 0x80, 0x08, 0x2C, 0xAF, 0x82, 0x91, 0xEC, 0xAD, 0xC5, 0xB5 };
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Decode (encoded);
			// properties
			AssertEquals ("Certificates", 0, ep.Certificates.Count);
			AssertEquals ("ContentEncryptionAlgorithm.KeyLength", 192, ep.ContentEncryptionAlgorithm.KeyLength);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.FriendlyName", tdesName, ep.ContentEncryptionAlgorithm.Oid.FriendlyName);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.Value", tdesOid, ep.ContentEncryptionAlgorithm.Oid.Value);
			AssertEquals ("ContentEncryptionAlgorithm.Parameters", 16, ep.ContentEncryptionAlgorithm.Parameters.Length);
			AssertEquals ("ContentInfo.ContentType.FriendlyName", p7DataName, ep.ContentInfo.ContentType.FriendlyName);
			AssertEquals ("ContentInfo.ContentType.Value", p7DataOid, ep.ContentInfo.ContentType.Value);
			AssertEquals ("ContentInfo.Content", 14, ep.ContentInfo.Content.Length);
			AssertEquals ("RecipientInfos", 1, ep.RecipientInfos.Count);
			RecipientInfo ri = ep.RecipientInfos [0];
			Assert ("RecipientInfos is KeyTransRecipientInfo", (ri is KeyTransRecipientInfo));
			AssertEquals ("UnprotectedAttributes", 0, ep.UnprotectedAttributes.Count);
			AssertEquals ("Version", 0, ep.Version);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecodeNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Decode (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EncodeEmpty () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			byte[] encoded = ep.Encode ();
		}

		[Test]
		public void Decrypt () 
		{
			byte[] encoded = { 0x30, 0x82, 0x01, 0x1C, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x03, 0xA0, 0x82, 0x01, 0x0D, 0x30, 0x82, 0x01, 0x09, 0x02, 0x01, 0x00, 0x31, 0x81, 0xD6, 0x30, 0x81, 0xD3, 0x02, 0x01, 0x00, 0x30, 0x3C, 0x30, 0x28, 0x31, 0x26, 0x30, 0x24, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x1D, 0x4D, 0x6F, 0x74, 0x75, 0x73, 0x20, 0x54, 0x65, 0x63, 0x68, 0x6E, 0x6F, 0x6C, 0x6F, 0x67, 0x69, 0x65, 0x73, 0x20, 0x69, 0x6E, 0x63, 0x2E, 0x28, 0x74, 0x65, 0x73, 0x74, 0x29, 0x02, 0x10, 0x91, 0xC4, 0x4B, 0x0D, 0xB7, 0xD8, 0x10, 0x84, 0x42, 0x26, 0x71, 0xB3, 0x97, 0xB5, 0x00, 0x97, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x04, 0x81, 0x80, 0xCA, 0x4B, 0x97, 0x9C, 0xAB, 0x79, 0xC6, 0xDF, 0x6A, 0x27, 0xC7, 0x24, 0xC4, 0x5E, 0x3B, 0x31, 0xAD, 0xBC, 0x25, 0xE6, 0x38, 0x5E, 0x79, 0x26, 0x0E, 0x68, 0x46, 0x1D, 0x21, 0x81, 0x38, 0x92, 0xEC, 0xCB, 0x7C, 0x91, 0xD6, 0x09, 0x38, 0x91, 0xCE, 0x50, 0x5B, 0x70, 0x31, 0xB0, 0x9F, 0xFC, 0xE2, 0xEE, 0x45, 0xBC, 0x4B, 0xF8, 0x9A, 0xD9, 0xEE, 0xE7, 0x4A, 0x3D, 0xCD, 0x8D, 0xFF, 0x10, 0xAB, 0xC8, 0x19, 0x05, 0x54, 0x5E, 0x40, 0x7A, 0xBE, 0x2B, 0xD7, 0x22, 0x97, 0xF3, 0x23, 0xAF, 0x50, 0xF5, 0xEB, 0x43, 0x06, 0xC3, 0xFB, 0x17, 0xCA, 0xBD, 0xAD, 0x28, 0xD8, 0x10, 0x0F, 0x61, 0xCE, 0xF8, 0x25, 0x70, 0xF6, 0xC8, 0x1E, 0x7F, 0x82, 0xE5, 0x94, 0xEB, 0x11, 0xBF, 0xB8, 0x6F, 0xEE, 0x79, 0xCD, 0x63, 0xDD, 0x59, 0x8D, 0x25, 0x0E, 0x78, 0x55, 0xCE, 0x21, 0xBA, 0x13, 0x6B, 0x30, 0x2B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x07, 0x01, 0x30, 0x14, 0x06, 0x08, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x03, 0x07, 0x04, 0x08, 0x8C, 0x5D, 0xC9, 0x87, 0x88, 0x9C, 0x05, 0x72, 0x80, 0x08, 0x2C, 0xAF, 0x82, 0x91, 0xEC, 0xAD, 0xC5, 0xB5 };
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Decode (encoded);

			X509CertificateEx x509 = GetCertificate (true);
			X509CertificateExCollection xc = new X509CertificateExCollection ();
			xc.Add (x509);
			ep.Decrypt (xc);
			// properties
			AssertEquals ("Certificates", 0, ep.Certificates.Count);
			AssertEquals ("ContentEncryptionAlgorithm.KeyLength", 192, ep.ContentEncryptionAlgorithm.KeyLength);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.FriendlyName", tdesName, ep.ContentEncryptionAlgorithm.Oid.FriendlyName);
			AssertEquals ("ContentEncryptionAlgorithm.Oid.Value", tdesOid, ep.ContentEncryptionAlgorithm.Oid.Value);
			AssertEquals ("ContentEncryptionAlgorithm.Parameters", 16, ep.ContentEncryptionAlgorithm.Parameters.Length);
			AssertEquals ("ContentInfo.ContentType.FriendlyName", p7DataName, ep.ContentInfo.ContentType.FriendlyName);
			AssertEquals ("ContentInfo.ContentType.Value", p7DataOid, ep.ContentInfo.ContentType.Value);
			AssertEquals ("ContentInfo.Content", "05-00", BitConverter.ToString (ep.ContentInfo.Content));
			AssertEquals ("RecipientInfos", 1, ep.RecipientInfos.Count);
			AssertEquals ("UnprotectedAttributes", 0, ep.UnprotectedAttributes.Count);
			AssertEquals ("Version", 0, ep.Version);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DecryptEmpty () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Decrypt ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecryptRecipientInfoNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			RecipientInfo ri = null; // do not confuse compiler
			ep.Decrypt (ri);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecryptX509CertificateExCollectionNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			X509CertificateExCollection xec = null; // do not confuse compiler
			ep.Decrypt (xec);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecryptRecipientInfoX509CertificateExCollectionNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			X509CertificateExCollection xec = new X509CertificateExCollection ();
			ep.Decrypt (null, xec);
		}

/*		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecryptX509CertificateExCollectionNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			RecipientInfo ri = 
			ep.Decrypt (ri, null);
		}*/

		private void RoundTrip (byte[] encoded) 
		{
			X509CertificateExCollection xc = new X509CertificateExCollection ();
			xc.Add (GetCertificate (true));
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Decode (encoded);
			ep.Decrypt (xc);
			AssertEquals ("ContentInfo.Content", "05-00", BitConverter.ToString (ep.ContentInfo.Content));
		}

		[Test]
		public void EncryptPkcs7RecipientIssuerAndSerialNumber () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.IssuerAndSerialNumber, ci);

			X509CertificateEx x509 = GetCertificate (false);
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.IssuerAndSerialNumber, x509);
			ep.Encrypt (p7r);
			byte[] encoded = ep.Encode ();
			
			FileStream fs = File.OpenWrite ("EncryptPkcs7RecipientIssuerAndSerialNumber.der");
			fs.Write (encoded, 0, encoded.Length);
			fs.Close ();

			RoundTrip (encoded);
		}

		[Test]
		public void EncryptPkcs7RecipientSubjectKeyIdentifier () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.IssuerAndSerialNumber, ci);

			X509CertificateEx x509 = GetCertificate (false);
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.SubjectKeyIdentifier, x509);
			ep.Encrypt (p7r);
			byte[] encoded = ep.Encode ();
			
			FileStream fs = File.OpenWrite ("EncryptPkcs7RecipientSubjectKeyIdentifier.der");
			fs.Write (encoded, 0, encoded.Length);
			fs.Close ();

			RoundTrip (encoded);
		}

		[Test]
		public void EncryptPkcs7RecipientUnknown () 
		{
			ContentInfo ci = new ContentInfo (asnNull);
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 (SubjectIdentifierType.IssuerAndSerialNumber, ci);

			X509CertificateEx x509 = GetCertificate (false);
			Pkcs7Recipient p7r = new Pkcs7Recipient (SubjectIdentifierType.Unknown, x509);
			ep.Encrypt (p7r);
			byte[] encoded = ep.Encode ();
			
			FileStream fs = File.OpenWrite ("EncryptPkcs7RecipientUnknown.der");
			fs.Write (encoded, 0, encoded.Length);
			fs.Close ();

			RoundTrip (encoded);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void EncryptEmpty () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			ep.Encrypt ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncryptPkcs7RecipientNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			Pkcs7Recipient p7r = null; // do not confuse compiler
			ep.Encrypt (p7r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncryptPkcs7RecipientCollectionNull () 
		{
			EnvelopedPkcs7 ep = new EnvelopedPkcs7 ();
			Pkcs7RecipientCollection p7rc = null; // do not confuse compiler
			ep.Encrypt (p7rc);
		}
	}
}

#endif
