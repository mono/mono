//
// AuthenticodeDeformatter.cs: Authenticode signature validator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.Authenticode {

	// References:
	// a.	http://www.cs.auckland.ac.nz/~pgut001/pubs/authenticode.txt

#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class AuthenticodeDeformatter : AuthenticodeBase {

		private string filename;
		private byte[] hash;
		private X509CertificateCollection coll;
		private ASN1 signedHash;
		private DateTime timestamp;
		private X509Certificate signingCertificate;
		private int reason;

		private X509Chain signerChain;
		private X509Chain timestampChain;

		public AuthenticodeDeformatter () : base ()
		{
			reason = -1;
			signerChain = new X509Chain ();
			timestampChain = new X509Chain ();
		}

		public AuthenticodeDeformatter (string fileName) : this () 
		{
			if (!CheckSignature (fileName)) {
				// invalid or no signature
				if (signedHash != null)
					throw new COMException ("Invalid signature");
				// no exception is thrown when there's no signature in the PE file
			}
		}

		public string FileName {
			get { return filename; }
			set { CheckSignature (value); }
		}

		public byte[] Hash {
			get { 
				if (signedHash == null)
					return null;
				return signedHash.Value; 
			}
		}

		public int Reason {
			get { 
				if (reason == -1)
					IsTrusted ();
				return reason; 
			}
		}

		public bool IsTrusted ()
		{
			if (rawData == null) {
				reason = 1;
				return false;
			}

			if (signingCertificate == null) {
				reason = 7;
				return false;
			}

			if (signerChain.Root == null) {
				reason = 6;
				return false;
			}

			if (timestamp != DateTime.MinValue) {
				if (timestampChain.Root == null) {
					reason = 6;
					return false;
				}

				// check that file was timestamped when certificates were valid
				if (!signingCertificate.WasCurrent (Timestamp)) {
					reason = 4;
					return false;
				}
			}
			else if (!signingCertificate.IsCurrent) {
				// signature only valid if the certificate is valid
				reason = 8;
				return false;
			}

			reason = 0;
			return true;
		}

		public byte[] Signature {
			get { return rawData; }
		}

		public DateTime Timestamp {
			get { return timestamp; }
		}

		public X509CertificateCollection Certificates {
			get { return coll; }
		}

		public X509Certificate SigningCertificate {
			get { return signingCertificate; }
		}

		private bool CheckSignature (string fileName) 
		{
			filename = fileName;

			// by default we try with MD5
			string hashName = "MD5";
			// compare the signature's hash with the hash of the file
			hash = HashFile (filename, hashName);

			// is a signature present ?
			if (rawData == null)
				return false;

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (rawData);
			if (ci.ContentType != PKCS7.signedData)
				return false;

			PKCS7.SignedData sd = new PKCS7.SignedData (ci.Content);
			if (sd.ContentInfo.ContentType != spcIndirectDataContext)
				return false;

			coll = sd.Certificates;

			ASN1 spc = sd.ContentInfo.Content;
			signedHash = spc [0][1][1];
			if (signedHash.Length == 20) {
				// seems to be SHA-1, restart hashing
				hashName = "SHA1";
				hash = HashFile (filename, hashName);
			}

			if (!signedHash.CompareValue (hash))
				return false;

			// messageDigest is a hash of spcIndirectDataContext (which includes the file hash)
			byte[] spcIDC = spc [0].Value;
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			byte[] messageDigest = ha.ComputeHash (spcIDC);

			return VerifySignature (sd, messageDigest, hashName);
		}

		private bool CompareIssuerSerial (string issuer, byte[] serial, X509Certificate x509) 
		{
			if (issuer != x509.IssuerName)
				return false;
			if (serial.Length != x509.SerialNumber.Length)
				return false;
			// MS shows the serial number inversed (so is Mono.Security.X509.X509Certificate)
			int n = serial.Length;
			for (int i=0; i < serial.Length; i++) {
				if (serial [i] != x509.SerialNumber [--n])
					return false;
			}
			// must be true
			return true;
		}

		//private bool VerifySignature (ASN1 cs, byte[] calculatedMessageDigest, string hashName) 
		private bool VerifySignature (PKCS7.SignedData sd, byte[] calculatedMessageDigest, string hashName) 
		{
			string contentType = null;
			ASN1 messageDigest = null;
			string spcStatementType = null;
			string spcSpOpusInfo = null;

			for (int i=0; i < sd.SignerInfo.AuthenticatedAttributes.Count; i++) {
				ASN1 attr = (ASN1) sd.SignerInfo.AuthenticatedAttributes [i];
				string oid = ASN1Convert.ToOID (attr[0]);
				switch (oid) {
					case "1.2.840.113549.1.9.3":
						// contentType
						contentType = ASN1Convert.ToOID (attr[1][0]);
						break;
					case "1.2.840.113549.1.9.4":
						// messageDigest
						messageDigest = attr[1][0];
						break;
					case "1.3.6.1.4.1.311.2.1.11":
						// spcStatementType (Microsoft code signing)
						// possible values
						// - individualCodeSigning (1 3 6 1 4 1 311 2 1 21)
						// - commercialCodeSigning (1 3 6 1 4 1 311 2 1 22)
						spcStatementType = ASN1Convert.ToOID (attr[1][0][0]);
						break;
					case "1.3.6.1.4.1.311.2.1.12":
						// spcSpOpusInfo (Microsoft code signing)
						try {
							spcSpOpusInfo = System.Text.Encoding.UTF8.GetString (attr[1][0][1][0].Value);
						}
						catch {
							spcSpOpusInfo = null;
						}
						break;
					default:
						break;
				}
			}
			if (contentType != spcIndirectDataContext)
				return false;

			// verify message digest
			if (messageDigest == null)
				return false;
			if (!messageDigest.CompareValue (calculatedMessageDigest))
				return false;

			// verify signature
			string hashOID = CryptoConfig.MapNameToOID (hashName);
			
			// change to SET OF (not [0]) as per PKCS #7 1.5
			ASN1 aa = new ASN1 (0x31);
			foreach (ASN1 a in sd.SignerInfo.AuthenticatedAttributes)
				aa.Add (a);
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			byte[] p7hash = ha.ComputeHash (aa.GetBytes ());

			byte[] signature = sd.SignerInfo.Signature;
			// we need to find the specified certificate
			string issuer = sd.SignerInfo.IssuerName;
			byte[] serial = sd.SignerInfo.SerialNumber;
			foreach (X509Certificate x509 in coll) {
				if (CompareIssuerSerial (issuer, serial, x509)) {
					// don't verify is key size don't match
					if (x509.PublicKey.Length > (signature.Length >> 3)) {
						RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) x509.RSA;
						if (rsa.VerifyHash (p7hash, hashOID, signature)) {
							signerChain.LoadCertificates (coll);
							if (signerChain.GetChain (x509) != null)
								signingCertificate = x509;
							else
								return false;
						}
					}
				}
			}

			for (int i=0; i < sd.SignerInfo.UnauthenticatedAttributes.Count; i++) {
				ASN1 attr = (ASN1) sd.SignerInfo.UnauthenticatedAttributes [i];
				string oid = ASN1Convert.ToOID (attr [0]);
				switch (oid) {
					case PKCS7.countersignature:
						// SEQUENCE {
						//   OBJECT IDENTIFIER
						//     countersignature (1 2 840 113549 1 9 6)
						//   SET {
						PKCS7.SignerInfo cs = new PKCS7.SignerInfo (attr [1]);
						return VerifyCounterSignature (cs, signature, hashName);
					default:
						// we don't support other unauthenticated attributes
						break;
				}
			}

			return true;
		}

		private bool VerifyCounterSignature (PKCS7.SignerInfo cs, byte[] signature, string hashName) 
		{
			// SEQUENCE {
			//   INTEGER 1
			if (cs.Version != 1)
				return false;
			//   SEQUENCE {
			//      SEQUENCE {

			string contentType = null;
			ASN1 messageDigest = null;
			for (int i=0; i < cs.AuthenticatedAttributes.Count; i++) {
				// SEQUENCE {
				//   OBJECT IDENTIFIER
				ASN1 attr = (ASN1) cs.AuthenticatedAttributes [i];
				string oid = ASN1Convert.ToOID (attr[0]);
				switch (oid) {
					case "1.2.840.113549.1.9.3":
						// contentType
						contentType = ASN1Convert.ToOID (attr[1][0]);
						break;
					case "1.2.840.113549.1.9.4":
						// messageDigest
						messageDigest = attr[1][0];
						break;
					case "1.2.840.113549.1.9.5":
						// SEQUENCE {
						//   OBJECT IDENTIFIER
						//     signingTime (1 2 840 113549 1 9 5)
						//   SET {
						//     UTCTime '030124013651Z'
						//   }
						// }
						timestamp = ASN1Convert.ToDateTime (attr[1][0]);
						break;
					default:
						break;
				}
			}

			if (contentType != PKCS7.data) 
				return false;

			// verify message digest
			if (messageDigest == null)
				return false;
			// TODO: must be read from the ASN.1 structure
			switch (messageDigest.Length) {
				case 16:
					hashName = "MD5";
					break;
				case 20:
					hashName = "SHA1";
					break;
			}
			HashAlgorithm ha = HashAlgorithm.Create (hashName);
			if (!messageDigest.CompareValue (ha.ComputeHash (signature)))
				return false;

			// verify signature
			byte[] counterSignature = cs.Signature;
			string hashOID = CryptoConfig.MapNameToOID (hashName);

			// change to SET OF (not [0]) as per PKCS #7 1.5
			ASN1 aa = new ASN1 (0x31);
			foreach (ASN1 a in cs.AuthenticatedAttributes)
				aa.Add (a);
			byte[] p7hash = ha.ComputeHash (aa.GetBytes ());

			// we need to try all certificates
			string issuer = cs.IssuerName;
			byte[] serial = cs.SerialNumber;
			foreach (X509Certificate x509 in coll) {
				if (CompareIssuerSerial (issuer, serial, x509)) {
					// don't verify if key size don't match
					if (x509.PublicKey.Length > (counterSignature.Length >> 3)) {
						RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) x509.RSA;
						if (rsa.VerifyHash (p7hash, hashOID, counterSignature)) {
							timestampChain.LoadCertificates (coll);
							return (timestampChain.GetChain (x509) != null);
						}
					}
				}
			}
			// no certificate can verify this signature!
			return false;
		}
	}
}
