//
// AuthenticodeDeformatter.cs: Authenticode signature validator
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
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
		private bool trustedRoot;
		private bool trustedTimestampRoot;
		private byte[] entry;

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
			FileName = fileName;
		}

		public string FileName {
			get { return filename; }
			set { 
				Reset ();
				try {
					CheckSignature (value); 
				}
				catch (SecurityException) {
					throw;
				}
				catch (Exception) {
					reason = 1;
				}
			}
		}

		public byte[] Hash {
			get { 
				if (signedHash == null)
					return null;
				return (byte[]) signedHash.Value.Clone ();
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
			if (entry == null) {
				reason = 1;
				return false;
			}

			if (signingCertificate == null) {
				reason = 7;
				return false;
			}

			if ((signerChain.Root == null) || !trustedRoot) {
				reason = 6;
				return false;
			}

			if (timestamp != DateTime.MinValue) {
				if ((timestampChain.Root == null) || !trustedTimestampRoot) {
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

			if (reason == -1)
				reason = 0;
			return true;
		}

		public byte[] Signature {
			get {
				if (entry == null)
					return null;
				return (byte[]) entry.Clone (); 
			}
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
			Open (filename);
			entry = GetSecurityEntry ();
			if (entry == null) {
				// no signature is present
				reason = 1;
				Close ();
				return false;
			}

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (entry);
			if (ci.ContentType != PKCS7.Oid.signedData) {
				Close ();
				return false;
			}

			PKCS7.SignedData sd = new PKCS7.SignedData (ci.Content);
			if (sd.ContentInfo.ContentType != spcIndirectDataContext) {
				Close ();
				return false;
			}

			coll = sd.Certificates;

			ASN1 spc = sd.ContentInfo.Content;
			signedHash = spc [0][1][1];

			HashAlgorithm ha = null; 
			switch (signedHash.Length) {
				case 16:
					ha = HashAlgorithm.Create ("MD5"); 
					hash = GetHash (ha);
					break;
				case 20:
					ha = HashAlgorithm.Create ("SHA1");
					hash = GetHash (ha);
					break;
				default:
					reason = 5;
					Close ();
					return false;
			}
			Close ();

			if (!signedHash.CompareValue (hash)) {
				reason = 2;
			}

			// messageDigest is a hash of spcIndirectDataContext (which includes the file hash)
			byte[] spcIDC = spc [0].Value;
			ha.Initialize (); // re-using hash instance
			byte[] messageDigest = ha.ComputeHash (spcIDC);

			bool sign = VerifySignature (sd, messageDigest, ha);
			return (sign && (reason == 0));
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
		private bool VerifySignature (PKCS7.SignedData sd, byte[] calculatedMessageDigest, HashAlgorithm ha) 
		{
			string contentType = null;
			ASN1 messageDigest = null;
//			string spcStatementType = null;
//			string spcSpOpusInfo = null;

			for (int i=0; i < sd.SignerInfo.AuthenticatedAttributes.Count; i++) {
				ASN1 attr = (ASN1) sd.SignerInfo.AuthenticatedAttributes [i];
				string oid = ASN1Convert.ToOid (attr[0]);
				switch (oid) {
					case "1.2.840.113549.1.9.3":
						// contentType
						contentType = ASN1Convert.ToOid (attr[1][0]);
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
//						spcStatementType = ASN1Convert.ToOid (attr[1][0][0]);
						break;
					case "1.3.6.1.4.1.311.2.1.12":
						// spcSpOpusInfo (Microsoft code signing)
/*						try {
							spcSpOpusInfo = System.Text.Encoding.UTF8.GetString (attr[1][0][0][0].Value);
						}
						catch (NullReferenceException) {
							spcSpOpusInfo = null;
						}*/
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
			string hashOID = CryptoConfig.MapNameToOID (ha.ToString ());
			
			// change to SET OF (not [0]) as per PKCS #7 1.5
			ASN1 aa = new ASN1 (0x31);
			foreach (ASN1 a in sd.SignerInfo.AuthenticatedAttributes)
				aa.Add (a);
			ha.Initialize ();
			byte[] p7hash = ha.ComputeHash (aa.GetBytes ());

			byte[] signature = sd.SignerInfo.Signature;
			// we need to find the specified certificate
			string issuer = sd.SignerInfo.IssuerName;
			byte[] serial = sd.SignerInfo.SerialNumber;
			foreach (X509Certificate x509 in coll) {
				if (CompareIssuerSerial (issuer, serial, x509)) {
					// don't verify is key size don't match
					if (x509.PublicKey.Length > (signature.Length >> 3)) {
						// return the signing certificate even if the signature isn't correct
						// (required behaviour for 2.0 support)
						signingCertificate = x509;
						RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) x509.RSA;
						if (rsa.VerifyHash (p7hash, hashOID, signature)) {
							signerChain.LoadCertificates (coll);
							trustedRoot = signerChain.Build (x509);
							break; 
						}
					}
				}
			}

			// timestamp signature is optional
			if (sd.SignerInfo.UnauthenticatedAttributes.Count == 0) {
				trustedTimestampRoot = true;
			}  else {
				for (int i = 0; i < sd.SignerInfo.UnauthenticatedAttributes.Count; i++) {
					ASN1 attr = (ASN1) sd.SignerInfo.UnauthenticatedAttributes[i];
					string oid = ASN1Convert.ToOid (attr[0]);
					switch (oid) {
					case PKCS7.Oid.countersignature:
						// SEQUENCE {
						//   OBJECT IDENTIFIER
						//     countersignature (1 2 840 113549 1 9 6)
						//   SET {
						PKCS7.SignerInfo cs = new PKCS7.SignerInfo (attr[1]);
						trustedTimestampRoot = VerifyCounterSignature (cs, signature);
						break;
					default:
						// we don't support other unauthenticated attributes
						break;
					}
				}
			}

			return (trustedRoot && trustedTimestampRoot);
		}

		private bool VerifyCounterSignature (PKCS7.SignerInfo cs, byte[] signature) 
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
				string oid = ASN1Convert.ToOid (attr[0]);
				switch (oid) {
					case "1.2.840.113549.1.9.3":
						// contentType
						contentType = ASN1Convert.ToOid (attr[1][0]);
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

			if (contentType != PKCS7.Oid.data) 
				return false;

			// verify message digest
			if (messageDigest == null)
				return false;
			// TODO: must be read from the ASN.1 structure
			string hashName = null;
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
					if (x509.PublicKey.Length > counterSignature.Length) {
						RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) x509.RSA;
						// we need to HACK around bad (PKCS#1 1.5) signatures made by Verisign Timestamp Service
						// and this means copying stuff into our own RSAManaged to get the required flexibility
						RSAManaged rsam = new RSAManaged ();
						rsam.ImportParameters (rsa.ExportParameters (false));
						if (PKCS1.Verify_v15 (rsam, ha, p7hash, counterSignature, true)) {
							timestampChain.LoadCertificates (coll);
							return (timestampChain.Build (x509));
						}
					}
				}
			}
			// no certificate can verify this signature!
			return false;
		}

		private void Reset ()
		{
			filename = null;
			entry = null;
			hash = null;
			signedHash = null;
			signingCertificate = null;
			reason = -1;
			trustedRoot = false;
			trustedTimestampRoot = false;
			signerChain.Reset ();
			timestampChain.Reset ();
			timestamp = DateTime.MinValue;
		}
	}
}
