//
// PKCS7.cs: PKCS #7 - Cryptographic Message Syntax Standard 
//	http://www.rsasecurity.com/rsalabs/pkcs/pkcs-7/index.html
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//


using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security {

	public class PKCS7 {

		// pkcs 1
		public const string rsaEncryption = "1.2.840.113549.1.1.1";
		// pkcs 7
		public const string data = "1.2.840.113549.1.7.1";
		public const string signedData = "1.2.840.113549.1.7.2";
		// pkcs 9
		public const string contentType = "1.2.840.113549.1.9.3";
		public const string messageDigest  = "1.2.840.113549.1.9.4";

		public PKCS7() {
		}

		static public ASN1 Attribute (string oid, ASN1 value) 
		{
			ASN1 attr = new ASN1 (0x30);
			attr.Add (ASN1Convert.FromOID (oid));
			ASN1 aset = attr.Add (new ASN1 (0x31));
			aset.Add (value);
			return attr;
		}

		static public ASN1 AlgorithmIdentifier (string oid)
		{
			ASN1 ai = new ASN1 (0x30);
			ai.Add (ASN1Convert.FromOID (oid));
			ai.Add (new ASN1 (0x05));	// NULL
			return ai;
		}

		static public ASN1 AlgorithmIdentifier (string oid, ASN1 param) 
		{
			ASN1 ai = new ASN1 (0x30);
			ai.Add (ASN1Convert.FromOID (oid));
			ai.Add (param);
			return ai;
		}

		/*
		 * IssuerAndSerialNumber ::= SEQUENCE {
		 *	issuer Name,
		 *	serialNumber CertificateSerialNumber 
		 * }
		 */
		static public ASN1 IssuerAndSerialNumber (X509Certificate x509) 
		{
			ASN1 issuer = null;
			ASN1 serial = null;
			ASN1 cert = new ASN1 (x509.GetRawCertData ());
			int tbs = 0;
			bool flag = false;
			while (tbs < cert[0][0].Count) {
				ASN1 e = cert[0][0][tbs++];
				if (e.Tag == 0x02)
					serial = e;
				else if (e.Tag == 0x30) {
					if (flag) {
						issuer = e;
						break;
					}
					flag = true;
				}
			}
			ASN1 iasn = new ASN1 (0x30);
			iasn.Add (issuer);
			iasn.Add (serial);
			return iasn;
		}

		/*
		 * ContentInfo ::= SEQUENCE {
		 *	contentType ContentType,
		 *	content [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL 
		 * }
		 * ContentType ::= OBJECT IDENTIFIER
		 */
		public class ContentInfo {

			private string contentType;
			private ASN1 content;

			public ContentInfo () 
			{
				content = new ASN1 (0xA0);
			}

			public ContentInfo (string oid) : this ()
			{
				contentType = oid;
			}

			public ContentInfo (byte[] data) 
				: this (new ASN1 (data)) {}

			public ContentInfo (ASN1 asn1) 
			{
				// SEQUENCE with 1 or 2 elements
				if ((asn1.Tag != 0x30) || ((asn1.Count < 1) && (asn1.Count > 2)))
					throw new ArgumentException ("Invalid ASN1");
				if (asn1[0].Tag != 0x06)
					throw new ArgumentException ("Invalid contentType");
				contentType = ASN1Convert.ToOID (asn1[0]);
				if (asn1.Count > 1) {
					if (asn1[1].Tag != 0xA0)
						throw new ArgumentException ("Invalid content");
					content = asn1[1];
				}
			}

			public ASN1 ASN1 {
				get { return GetASN1(); }
			}

			public ASN1 Content {
				get { return content; }
			}

			public string ContentType {
				get { return contentType; }
				set { contentType = value; }
			}

			internal ASN1 GetASN1 () 
			{
				// ContentInfo ::= SEQUENCE {
				ASN1 contentInfo = new ASN1 (0x30);
				// contentType ContentType, -> ContentType ::= OBJECT IDENTIFIER
				contentInfo.Add (ASN1Convert.FromOID (contentType));
				// content [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL 
				if ((content != null) && (content.Count > 0))
					contentInfo.Add (content);
				return contentInfo;
			}

			public byte[] GetBytes () 
			{
				return GetASN1 ().GetBytes ();
			}
		}

		/*
		 * SignedData ::= SEQUENCE {
		 *	version Version,
		 *	digestAlgorithms DigestAlgorithmIdentifiers,
		 *	contentInfo ContentInfo,
		 *	certificates [0] IMPLICIT ExtendedCertificatesAndCertificates OPTIONAL,
		 *	crls [1] IMPLICIT CertificateRevocationLists OPTIONAL,
		 *	signerInfos SignerInfos 
		 * }
		 */
		public class SignedData {
			private byte version;
			private string hashAlgorithm;
			private ContentInfo contentInfo;
			private X509CertificateCollection certs;
			private ArrayList crls;
			private SignerInfo signerInfo;
			private ASN1 mda;

			public SignedData () 
			{
				version = 1;
				contentInfo = new ContentInfo ();
				certs = new X509CertificateCollection ();
				crls = new ArrayList ();
				signerInfo = new SignerInfo ();
			}

			public SignedData (byte[] data) 
				: this (new ASN1 (data)) {}

			public SignedData (ASN1 asn1) 
			{
				if ((asn1[0].Tag != 0x30) || (asn1[0].Count < 4))
					throw new ArgumentException ("Invalid SignedData");

				if (asn1[0][0].Tag != 0x02)
					throw new ArgumentException ("Invalid version");
				version = asn1[0][0].Value[0];

				// digestInfo

				contentInfo = new ContentInfo (asn1[0][2]);

				int n = 3;
				certs = new X509CertificateCollection ();
				if (asn1[0][n].Tag == 0xA0) {
					for (int i=0; i < asn1[0][n].Count; i++)
						certs.Add (new X509Certificate (asn1[0][n][i].GetBytes ()));
					n++;
				}

				crls = new ArrayList ();
				if (asn1[0][n].Tag == 0xA1) {
					for (int i=0; i < asn1[0][n].Count; i++)
						crls.Add (asn1[0][n][i].GetBytes ());
					n++;
				}

				if (asn1[0][n].Count > 0)
					signerInfo = new SignerInfo (asn1[0][n]);
				else
					signerInfo = new SignerInfo ();
			}

			public ASN1 ASN1 {
				get { return GetASN1(); }
			}

			public X509CertificateCollection Certificates {
				get { return certs; }
			}

			public ContentInfo ContentInfo {
				get { return contentInfo; }
			}

			public ArrayList CRLs {
				get { return crls; }
			}

			public string HashName {
				get { return hashAlgorithm; }
				// todo add validation
				set { 
					hashAlgorithm = value; 
					signerInfo.HashName = value;
				}
			}

			public SignerInfo SignerInfo {
				get { return signerInfo; }
			}

			public byte Version {
				get { return version; }
				set { version = value; }
			}

			internal ASN1 GetASN1 () 
			{
				// SignedData ::= SEQUENCE {
				ASN1 signedData = new ASN1 (0x30);
				// version Version -> Version ::= INTEGER
				byte[] ver = { version };
				signedData.Add (new ASN1 (0x02, ver));
				// digestAlgorithms DigestAlgorithmIdentifiers -> DigestAlgorithmIdentifiers ::= SET OF DigestAlgorithmIdentifier
				ASN1 digestAlgorithms = signedData.Add (new ASN1 (0x31));
				if (hashAlgorithm != null) {
					string hashOid = CryptoConfig.MapNameToOID (hashAlgorithm);
					digestAlgorithms.Add (AlgorithmIdentifier (hashOid));
				}

				// contentInfo ContentInfo,
				ASN1 ci = contentInfo.ASN1;
				signedData.Add (ci);
				if ((mda == null) && (hashAlgorithm != null)) {
					// automatically add the messageDigest authenticated attribute
					HashAlgorithm ha = HashAlgorithm.Create (hashAlgorithm);
					byte[] idcHash = ha.ComputeHash (ci[1][0].Value);
					ASN1 md = new ASN1 (0x30);
					mda = Attribute (messageDigest, md.Add (new ASN1 (0x04, idcHash)));
					signerInfo.AuthenticatedAttributes.Add (mda);
				}

				// certificates [0] IMPLICIT ExtendedCertificatesAndCertificates OPTIONAL,
				if (certs.Count > 0) {
					ASN1 a0 = signedData.Add (new ASN1 (0xA0));
					foreach (X509Certificate x in certs)
						a0.Add (new ASN1 (x.GetRawCertData ()));
				}
				// crls [1] IMPLICIT CertificateRevocationLists OPTIONAL,
				if (crls.Count > 0) {
					ASN1 a1 = signedData.Add (new ASN1 (0xA1));
					foreach (byte[] crl in crls)
						a1.Add (new ASN1 (crl));
				}
				// signerInfos SignerInfos -> SignerInfos ::= SET OF SignerInfo
				ASN1 signerInfos = signedData.Add (new ASN1 (0x31));
				if (signerInfo.Key != null)
					signerInfos.Add (signerInfo.ASN1);
				return signedData;
			}

			public byte[] GetBytes () 
			{
				return GetASN1 ().GetBytes ();
			}
		}

		/*
		 * SignerInfo ::= SEQUENCE {
		 *	version Version,
		 * 	issuerAndSerialNumber IssuerAndSerialNumber,
		 * 	digestAlgorithm DigestAlgorithmIdentifier,
		 * 	authenticatedAttributes [0] IMPLICIT Attributes OPTIONAL,
		 * 	digestEncryptionAlgorithm DigestEncryptionAlgorithmIdentifier,
		 * 	encryptedDigest EncryptedDigest,
		 * 	unauthenticatedAttributes [1] IMPLICIT Attributes OPTIONAL 
		 * }
		 */
		public class SignerInfo {

			private byte version;
			private X509Certificate x509;
			private string hashAlgorithm;
			private AsymmetricAlgorithm key;
			private ArrayList authenticatedAttributes;
			private ArrayList unauthenticatedAttributes;
			private byte[] signature;

			public SignerInfo () 
			{
				version = 1;
				authenticatedAttributes = new ArrayList ();
				unauthenticatedAttributes = new ArrayList ();
			}

			public SignerInfo (byte[] data) 
				: this (new ASN1 (data)) {}

			public SignerInfo (ASN1 asn1) 
			{
				if ((asn1[0].Tag != 0x30) || (asn1[0].Count < 5))
					throw new ArgumentException ("Invalid SignedData");

				if (asn1[0][0].Tag != 0x02)
					throw new ArgumentException ("Invalid version");
				version = asn1[0][0].Value[0];

				// TODO: INCOMPLETE
			}

			public ASN1 ASN1 {
				get { return GetASN1(); }
			}

			public ArrayList AuthenticatedAttributes {
				get { return authenticatedAttributes; }
			}

			public X509Certificate Certificate {
				get { return x509; }
				set { x509 = value; }
			}

			public string HashName {
				get { return hashAlgorithm; }
				// todo add validation
				set { hashAlgorithm = value; }
			}

			public AsymmetricAlgorithm Key {
				get { return key; }
				set { key = value; }
			}

			public byte[] Signature {
				get { return (byte[]) signature.Clone (); }
			}

			public ArrayList UnauthenticatedAttributes {
				get { return unauthenticatedAttributes; }
			}

			public byte Version {
				get { return version; }
				set { version = value; }
			}

			internal ASN1 GetASN1 () 
			{
				if ((key == null) || (hashAlgorithm == null))
					return null;
				byte[] ver = { version };
				ASN1 signerInfo = new ASN1 (0x30);
				// version Version -> Version ::= INTEGER
				signerInfo.Add (new ASN1 (0x02, ver));
				// issuerAndSerialNumber IssuerAndSerialNumber,
				signerInfo.Add (PKCS7.IssuerAndSerialNumber (x509));
				// digestAlgorithm DigestAlgorithmIdentifier,
				string hashOid = CryptoConfig.MapNameToOID (hashAlgorithm);
				signerInfo.Add (AlgorithmIdentifier (hashOid));
				// authenticatedAttributes [0] IMPLICIT Attributes OPTIONAL,
				ASN1 aa = signerInfo.Add (new ASN1 (0xA0));
				if (authenticatedAttributes.Count > 0) {
					foreach (ASN1 attr in authenticatedAttributes)
						aa.Add (attr);
				}
				// digestEncryptionAlgorithm DigestEncryptionAlgorithmIdentifier,
				if (key is RSA) {
					signerInfo.Add (AlgorithmIdentifier (PKCS7.rsaEncryption));

					RSAPKCS1SignatureFormatter r = new RSAPKCS1SignatureFormatter (key);
					r.SetHashAlgorithm (hashAlgorithm);
					byte[] tbs = aa.GetBytes ();
					tbs [0] = 0x31; // not 0xA0 for signature
					HashAlgorithm ha = HashAlgorithm.Create (hashAlgorithm);
					byte[] tbsHash = ha.ComputeHash (tbs);
					signature = r.CreateSignature (tbsHash);
				}
				else if (key is DSA) {
					throw new NotImplementedException ("not yet");
				}
				else
					throw new CryptographicException ("Unknown assymetric algorithm");
				// encryptedDigest EncryptedDigest,
				signerInfo.Add (new ASN1 (0x04, signature));
				// unauthenticatedAttributes [1] IMPLICIT Attributes OPTIONAL 
				if (unauthenticatedAttributes.Count > 0) {
					ASN1 ua = signerInfo.Add (new ASN1 (0xA1));
					foreach (ASN1 attr in unauthenticatedAttributes)
						ua.Add (attr);
				}
				return signerInfo;
			}

			public byte[] GetBytes () 
			{
				return GetASN1 ().GetBytes ();
			}
		}
	}
}
