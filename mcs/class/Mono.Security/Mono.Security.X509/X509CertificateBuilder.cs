//
// X509CertificateBuilder.cs: Handles building of X.509 certificates.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)

using System;
using System.Security.Cryptography;

namespace Mono.Security.X509 {
	// From RFC3280
	/*
	 * Certificate  ::=  SEQUENCE  {
	 *      tbsCertificate       TBSCertificate,
	 *      signatureAlgorithm   AlgorithmIdentifier,
	 *      signature            BIT STRING  
	 * }
	 * TBSCertificate  ::=  SEQUENCE  {
	 *      version         [0]  Version DEFAULT v1,
	 *      serialNumber         CertificateSerialNumber,
	 *      signature            AlgorithmIdentifier,
	 *      issuer               Name,
	 *      validity             Validity,
	 *      subject              Name,
	 *      subjectPublicKeyInfo SubjectPublicKeyInfo,
	 *      issuerUniqueID  [1]  IMPLICIT UniqueIdentifier OPTIONAL,
	 *                           -- If present, version MUST be v2 or v3
	 *      subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL,
	 *                           -- If present, version MUST be v2 or v3
	 *      extensions      [3]  Extensions OPTIONAL
	 *                           -- If present, version MUST be v3 --  
	 * }
	 * Version  ::=  INTEGER  {  v1(0), v2(1), v3(2)  }
	 * CertificateSerialNumber  ::=  INTEGER
	 * Validity ::= SEQUENCE {
	 *      notBefore      Time,
	 *      notAfter       Time  
	 * }
	 * Time ::= CHOICE {
	 *      utcTime        UTCTime,
	 *      generalTime    GeneralizedTime 
	 * }
	 */
	public class X509CertificateBuilder : X509Builder {
 
		private byte version;
		private byte[] sn;
		private string issuer;
		private DateTime notBefore;
		private DateTime notAfter;
		private string subject;
		private AsymmetricAlgorithm aa;
		private byte[] issuerUniqueID;
		private byte[] subjectUniqueID;
		private X509Extensions extensions;

		public X509CertificateBuilder () : this (3) {}
	
		public X509CertificateBuilder (byte version) 
		{
			if (version > 3)
				throw new ArgumentException ("Invalid certificate version");
			this.version = version;
			extensions = new X509Extensions ();
		}

		public byte Version {
			get { return version; }
			set { version = value; }
		}

		public byte[] SerialNumber {
			get { return sn; }
			set { sn = value; }
		}

		public string IssuerName {
			get { return issuer; }
			set { issuer = value; }
		}

		public DateTime NotBefore {
			get { return notBefore; }
			set { notBefore = value; }
		}

		public DateTime NotAfter {
			get { return notAfter; }
			set { notAfter = value; }
		}

		public string SubjectName {
			get { return subject; }
			set { subject = value; }
		}

		public AsymmetricAlgorithm SubjectPublicKey {
			get { return aa; }
			set { aa = value; }
		}

		public byte[] IssuerUniqueID {
			get { return issuerUniqueID; }
			set { issuerUniqueID = value; }
		}

		public byte[] SubjectUniqueID {
			get { return subjectUniqueID; }
			set { subjectUniqueID = value; }
		}

		public X509Extensions Extensions {
			get { return extensions; }
		}


		/* SubjectPublicKeyInfo  ::=  SEQUENCE  {
		 *      algorithm            AlgorithmIdentifier,
		 *      subjectPublicKey     BIT STRING  }
		 */
		private ASN1 SubjectPublicKeyInfo () 
		{
			ASN1 keyInfo = new ASN1 (0x30);
			if (aa is RSA) {
				keyInfo.Add (PKCS7.AlgorithmIdentifier ("1.2.840.113549.1.1.1"));
				RSAParameters p = (aa as RSA).ExportParameters (false);
				/* RSAPublicKey ::= SEQUENCE {
				 *       modulus            INTEGER,    -- n
				 *       publicExponent     INTEGER  }  -- e
				 */
				ASN1 key = new ASN1 (0x30);
				key.Add (ASN1Convert.FromUnsignedBigInteger (p.Modulus));
				key.Add (ASN1Convert.FromUnsignedBigInteger (p.Exponent));
				keyInfo.Add (new ASN1 (UniqueIdentifier (key.GetBytes ())));
			}
			else if (aa is DSA) {
				DSAParameters p = (aa as DSA).ExportParameters (false);
				/* Dss-Parms  ::=  SEQUENCE  {
				 *       p             INTEGER,
				 *       q             INTEGER,
				 *       g             INTEGER  }
				 */
				ASN1 param = new ASN1 (0x30);
				param.Add (ASN1Convert.FromUnsignedBigInteger (p.P));
				param.Add (ASN1Convert.FromUnsignedBigInteger (p.Q));
				param.Add (ASN1Convert.FromUnsignedBigInteger (p.G));
				keyInfo.Add (PKCS7.AlgorithmIdentifier ("1.2.840.10040.4.1", param));
				ASN1 key = keyInfo.Add (new ASN1 (0x03));
				// DSAPublicKey ::= INTEGER  -- public key, y
				key.Add (ASN1Convert.FromUnsignedBigInteger (p.Y));
			}
			else
				throw new NotSupportedException ("Unknown Asymmetric Algorithm " + aa.ToString ());
			return keyInfo;
		}

		private byte[] UniqueIdentifier (byte[] id) 
		{
			// UniqueIdentifier  ::=  BIT STRING
			ASN1 uid = new ASN1 (0x03);
			// first byte in a BITSTRING is the number of unused bits in the first byte
			byte[] v = new byte [id.Length + 1];
			Array.Copy (id, 0, v, 1, id.Length);
			uid.Value = v;
			return uid.GetBytes ();
		}

		protected override ASN1 ToBeSigned (string oid) 
		{
			// TBSCertificate
			ASN1 tbsCert = new ASN1 (0x30);

			if (version > 1) {
				// TBSCertificate / [0] Version DEFAULT v1,
				byte[] ver = { (byte)(version - 1) };
				ASN1 v = tbsCert.Add (new ASN1 (0xA0));
				v.Add (new ASN1 (0x02, ver));
			}

			// TBSCertificate / CertificateSerialNumber,
			tbsCert.Add (new ASN1 (0x02, sn));

			// TBSCertificate / AlgorithmIdentifier,
                        tbsCert.Add (PKCS7.AlgorithmIdentifier (oid));

			// TBSCertificate / Name
			tbsCert.Add (X501.FromString (issuer));

			// TBSCertificate / Validity
			ASN1 validity = tbsCert.Add (new ASN1 (0x30));
			// TBSCertificate / Validity / Time
			validity.Add (ASN1Convert.FromDateTime (notBefore));
			// TBSCertificate / Validity / Time
			validity.Add (ASN1Convert.FromDateTime (notAfter));

			// TBSCertificate / Name
			tbsCert.Add (X501.FromString (subject));

			// TBSCertificate / SubjectPublicKeyInfo
			ASN1 keyInfo = tbsCert.Add (SubjectPublicKeyInfo ());
                        
			if (version > 1) {
				// TBSCertificate / [1]  IMPLICIT UniqueIdentifier OPTIONAL
				if (issuerUniqueID != null)
					tbsCert.Add (new ASN1 (0xA1, UniqueIdentifier (issuerUniqueID)));

				// TBSCertificate / [2]  IMPLICIT UniqueIdentifier OPTIONAL
				if (subjectUniqueID != null)
					tbsCert.Add (new ASN1 (0xA1, UniqueIdentifier (subjectUniqueID)));

				// TBSCertificate / [3]  Extensions OPTIONAL
				if ((version > 2) &&  (extensions.Count > 0))
					tbsCert.Add (new ASN1 (0xA3, extensions.GetBytes ()));
			}

			return tbsCert;
		}
	}
}
