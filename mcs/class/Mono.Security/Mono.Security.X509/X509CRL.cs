//
// X509CRL.cs: Handles X.509 certificates revocation lists.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {
	/*
	 * CertificateList  ::=  SEQUENCE  {
	 *	tbsCertList          TBSCertList,
	 *	signatureAlgorithm   AlgorithmIdentifier,
	 *	signature            BIT STRING  
	 * }
	 * 
	 * TBSCertList  ::=  SEQUENCE  {
	 *	version                 Version OPTIONAL,
	 *		-- if present, MUST be v2
	 *	signature               AlgorithmIdentifier,
	 *	issuer                  Name,
	 *	thisUpdate              Time,
	 *	nextUpdate              Time OPTIONAL,
	 *	revokedCertificates     SEQUENCE OF SEQUENCE  {
	 *		userCertificate         CertificateSerialNumber,
	 *		revocationDate          Time,
	 *		crlEntryExtensions      Extensions OPTIONAL
	 *			-- if present, MUST be v2
	 *	}  OPTIONAL,
	 *	crlExtensions           [0] Extensions OPTIONAL }
	 *		-- if present, MUST be v2
	 */
	public class X509CRL {

		public class X509CRLEntry {

			private byte[] sn;
			private DateTime revocationDate;
			private X509Extensions extensions;

			internal X509CRLEntry (byte[] serialNumber, DateTime revocationDate, X509Extensions extensions) 
			{
				sn = serialNumber;
				this.revocationDate = revocationDate;
				if (extensions == null)
					this.extensions = new X509Extensions ();
				else
					this.extensions = extensions;
			}

			internal X509CRLEntry (ASN1 entry) 
			{
				sn = entry [0].Value;
				Array.Reverse (sn);
				revocationDate = ASN1Convert.ToDateTime (entry [1]);
				extensions = new X509Extensions (entry [2]);
			}

			public byte[] SerialNumber {
				get { return sn; }
			}

			public DateTime RevocationDate {
				get { return revocationDate; }
			}

			public X509Extensions Extensions {
				get { return extensions; }
			}

			public byte[] GetBytes () 
			{
				ASN1 sequence = new ASN1 (0x30);
				sequence.Add (new ASN1 (0x02, sn));
				sequence.Add (ASN1Convert.FromDateTime (revocationDate));
				if (extensions.Count > 0)
					sequence.Add (new ASN1 (extensions.GetBytes ()));
				return sequence.GetBytes ();
			}
		}

		private string issuer;
		private byte version;
		private DateTime thisUpdate;
		private DateTime nextUpdate;
		private ArrayList entries;
		private string signatureOID;
		private byte[] signature;
		private X509Extensions extensions;
		private byte[] encoded;

		public X509CRL (byte[] crl) 
		{
			if (crl == null)
				throw new ArgumentNullException ("crl");
			encoded = (byte[]) crl.Clone ();
			Parse (encoded);
		}

		private void Parse (byte[] crl) 
		{
			string e = "Input data cannot be coded as a valid CRL.";
			try {
				// CertificateList  ::=  SEQUENCE  {
				ASN1 encodedCRL = new ASN1 (encoded);
				if ((encodedCRL.Tag != 0x30) || (encodedCRL.Count != 3))
					throw new CryptographicException (e);

				// CertificateList / TBSCertList,
				ASN1 toBeSigned = encodedCRL [0];
				if ((toBeSigned.Tag != 0x30) || (toBeSigned.Count < 3))
					throw new CryptographicException (e);

				int n = 0;
				// CertificateList / TBSCertList / Version OPTIONAL, -- if present, MUST be v2
				if (toBeSigned [n].Tag == 0x02) {
					version = (byte) (toBeSigned [n++].Value [0] + 1);
				}
				else
					version = 1; // DEFAULT
				// CertificateList / TBSCertList / AlgorithmIdentifier,
				signatureOID = ASN1Convert.ToOID (toBeSigned [n++][0]);
				// CertificateList / TBSCertList / Name,
				issuer = X501.ToString (toBeSigned [n++]);
				// CertificateList / TBSCertList / Time,
				thisUpdate = ASN1Convert.ToDateTime (toBeSigned [n++]);
				// CertificateList / TBSCertList / Time OPTIONAL,
				ASN1 next = toBeSigned [n++];
				if ((next.Tag == 0x17) || (next.Tag == 0x18)) {
					nextUpdate = ASN1Convert.ToDateTime (next);
					next = toBeSigned [n++];
				}
				// CertificateList / TBSCertList / revokedCertificates	SEQUENCE OF SEQUENCE  {
				entries = new ArrayList ();
				ASN1 revokedCertificates = next;
				for (int i=0; i < revokedCertificates.Count; i++) {
					entries.Add (new X509CRLEntry (revokedCertificates [i]));
				}
				// CertificateList / TBSCertList / crlExtensions [0] Extensions OPTIONAL }
				ASN1 extns = toBeSigned [n];
				if ((extns != null) && (extns.Tag == 0xA0) && (extns.Count == 1))
					extensions = new X509Extensions (extns [0]);
				else
					extensions = new X509Extensions (null); // result in a read only object
				// CertificateList / AlgorithmIdentifier
				string signatureAlgorithm = ASN1Convert.ToOID (encodedCRL [1][0]);
				if (signatureOID != signatureAlgorithm)
					throw new CryptographicException (e + " [Non-matching signature algorithms in CRL]");

				// CertificateList / BIT STRING 
				byte[] bitstring = encodedCRL [2].Value;
				// first byte contains unused bits in first byte
				signature = new byte [bitstring.Length - 1];
				Array.Copy (bitstring, 1, signature, 0, signature.Length);
			}
			catch {
				throw new CryptographicException (e);
			}
		}

		public ArrayList Entries {
			get { return ArrayList.ReadOnly (entries); }
		}

		public X509CRLEntry this [int index] {
			get { return (X509CRLEntry) entries [index]; }
		}

		public X509CRLEntry this [byte[] serialNumber] {
			get { return GetCRLEntry (serialNumber); }
		}

		public X509Extensions Extensions {
			get { return extensions; }
		}

		public string IssuerName {
			get { return issuer; }
		}

		public DateTime NextUpdate {
			get { return nextUpdate; }
		}

		public DateTime ThisUpdate {
			get { return thisUpdate; }
		}

		public string SignatureAlgorithm {
			get { return signatureOID; }
		}

		public byte[] Signature {
			get { return signature; }
		}

		public byte Version {
			get { return version; }
		}

		public bool IsCurrent {
			get { return WasCurrent (DateTime.UtcNow); }
		}

		public bool WasCurrent (DateTime date) 
		{
			if (nextUpdate == DateTime.MinValue)
				return (date >= thisUpdate);
			else
				return ((date >= thisUpdate) && (date <= nextUpdate));
		}

		public byte[] GetBytes () 
		{
			return encoded;
		}

		private bool Compare (byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return true;
			if ((array1 == null) || (array2 == null))
				return false;
			if (array1.Length != array2.Length)
				return false;
			for (int i=0; i < array1.Length; i++) {
				if (array1 [i] != array2 [i])
					return false;
			}
			return true;
		}

		public X509CRLEntry GetCRLEntry (X509Certificate x509) 
		{
			if (x509 == null)
				throw new ArgumentNullException ("x509");
			return GetCRLEntry (x509.SerialNumber);
		}

		public X509CRLEntry GetCRLEntry (byte[] serialNumber) 
		{
			if (serialNumber == null)
				throw new ArgumentNullException ("serialNumber");

			for (int i=0; i < entries.Count; i++) {
				X509CRLEntry entry = (X509CRLEntry) entries [i];
				if (Compare (serialNumber, entry.SerialNumber))
					return entry;
			}
			return null;
		}

		public bool VerifySignature (X509Certificate x509) 
		{
			// 1. x509 certificate must be a CA certificate (unknown for v1 or v2 certs)
			if (x509.Version >= 3) {
				// 1.1. Check for "cRLSign" bit in KeyUsage extension
				X509Extension ext = x509.Extensions ["2.5.29.15"];
				if (ext != null) {
					KeyUsageExtension keyUsage = new KeyUsageExtension (ext);
					if (!keyUsage.Support (KeyUsage.cRLSign))
						return false;
				}
				// 1.2. Check for ca = true in BasicConstraint
				ext = x509.Extensions ["2.5.29.19"];
				if (ext != null) {
					BasicConstraintsExtension basicConstraints = new BasicConstraintsExtension (ext);
					if (!basicConstraints.CertificateAuthority)
						return false;
				}
			}
			// 2. CRL issuer must match CA subject name
			if (issuer != x509.SubjectName)
				return false;
			// 3. Check the CRL signature with the CA certificate public key
			switch (signatureOID) {
				case "1.2.840.10040.4.3":
					return VerifySignature (x509.DSA);
				default:
					return VerifySignature (x509.RSA);
			}
		}

		private byte[] GetHash (string hashName) 
		{
			ASN1 encodedCRL = new ASN1 (encoded);
			byte[] toBeSigned = encodedCRL [0].GetBytes ();
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			return ha.ComputeHash (toBeSigned);
		}

		public bool VerifySignature (DSA dsa) 
		{
			if (signatureOID != "1.2.840.10040.4.3")
				throw new CryptographicException ("Unsupported hash algorithm: " + signatureOID);
			DSASignatureDeformatter v = new DSASignatureDeformatter (dsa);
			// only SHA-1 is supported
			string hashName = "SHA1";
			v.SetHashAlgorithm (hashName);
			ASN1 sign = new ASN1 (signature);
			if ((sign == null) || (sign.Count != 2))
				return false;
			// parts may be less than 20 bytes (i.e. first bytes were 0x00)
			byte[] part1 = sign [0].Value;
			byte[] part2 = sign [1].Value;
			byte[] sig = new byte [40];
			Array.Copy (part1, 0, sig, (20 - part1.Length), part1.Length);
			Array.Copy (part2, 0, sig, (40 - part2.Length), part2.Length);
			return v.VerifySignature (GetHash (hashName), sig);
		}

		public bool VerifySignature (RSA rsa) 
		{
			RSAPKCS1SignatureDeformatter v = new RSAPKCS1SignatureDeformatter (rsa);
			string hashName = null;
			switch (signatureOID) {
				// MD2 with RSA encryption 
				case "1.2.840.113549.1.1.2":
					// maybe someone installed MD2 ?
					hashName = "MD2";
					break;
				// MD5 with RSA encryption 
				case "1.2.840.113549.1.1.4":
					hashName = "MD5";
					break;
				// SHA-1 with RSA Encryption 
				case "1.2.840.113549.1.1.5":
					hashName = "SHA1";
					break;
				default:
					throw new CryptographicException ("Unsupported hash algorithm: " + signatureOID);
			}
			v.SetHashAlgorithm (hashName);
			return v.VerifySignature (GetHash (hashName), signature);
		}

		public bool VerifySignature (AsymmetricAlgorithm aa) 
		{
			// only validate the signature (in case we don't have the CA certificate)
			if (aa is RSA)
				return VerifySignature (aa as RSA);
			else if (aa is DSA)
				return VerifySignature (aa as DSA);
			else
				throw new NotSupportedException ("Unknown Asymmetric Algorithm " + aa.ToString ());
		}

		static public X509CRL CreateFromFile (string filename) 
		{
			FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] crl = new byte [fs.Length];
			fs.Read (crl, 0, crl.Length);
			fs.Close ();
			return new X509CRL (crl);
		}
	}
}
