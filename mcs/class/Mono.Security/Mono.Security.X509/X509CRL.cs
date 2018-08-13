//
// X509CRL.cs: Handles X.509 certificates revocation lists.
//
// Author:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright (C) 2004,2006 Novell Inc. (http://www.novell.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
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
#if !INSIDE_CORLIB && !INSIDE_SYSTEM
	public 
#endif
	class X509Crl {

		public class X509CrlEntry {

			private byte[] sn;
			private DateTime revocationDate;
			private X509ExtensionCollection extensions;

			internal X509CrlEntry (byte[] serialNumber, DateTime revocationDate, X509ExtensionCollection extensions) 
			{
				sn = serialNumber;
				this.revocationDate = revocationDate;
				if (extensions == null)
					this.extensions = new X509ExtensionCollection ();
				else
					this.extensions = extensions;
			}

			internal X509CrlEntry (ASN1 entry) 
			{
				sn = entry [0].Value;
				Array.Reverse (sn);
				revocationDate = ASN1Convert.ToDateTime (entry [1]);
				extensions = new X509ExtensionCollection (entry [2]);
			}

			public byte[] SerialNumber {
				get { return (byte[]) sn.Clone (); }
			}

			public DateTime RevocationDate {
				get { return revocationDate; }
			}

			public X509ExtensionCollection Extensions {
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
		private X509ExtensionCollection extensions;
		private byte[] encoded;
		private byte[] hash_value;

		public X509Crl (byte[] crl) 
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
				signatureOID = ASN1Convert.ToOid (toBeSigned [n++][0]);
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
				// this is OPTIONAL so it may not be present if no entries exists
				if ((next != null) && (next.Tag == 0x30)) {
					ASN1 revokedCertificates = next;
					for (int i=0; i < revokedCertificates.Count; i++) {
						entries.Add (new X509CrlEntry (revokedCertificates [i]));
					}
				} else {
					n--;
				}
				// CertificateList / TBSCertList / crlExtensions [0] Extensions OPTIONAL }
				ASN1 extns = toBeSigned [n];
				if ((extns != null) && (extns.Tag == 0xA0) && (extns.Count == 1))
					extensions = new X509ExtensionCollection (extns [0]);
				else
					extensions = new X509ExtensionCollection (null); // result in a read only object
				// CertificateList / AlgorithmIdentifier
				string signatureAlgorithm = ASN1Convert.ToOid (encodedCRL [1][0]);
				if (signatureOID != signatureAlgorithm)
					throw new CryptographicException (e + " [Non-matching signature algorithms in CRL]");

				// CertificateList / BIT STRING 
				byte[] bitstring = encodedCRL [2].Value;
				// first byte contains unused bits in first byte
				signature = new byte [bitstring.Length - 1];
				Buffer.BlockCopy (bitstring, 1, signature, 0, signature.Length);
			}
			catch {
				throw new CryptographicException (e);
			}
		}

		public ArrayList Entries {
			get { return ArrayList.ReadOnly (entries); }
		}

		public X509CrlEntry this [int index] {
			get { return (X509CrlEntry) entries [index]; }
		}

		public X509CrlEntry this [byte[] serialNumber] {
			get { return GetCrlEntry (serialNumber); }
		}

		public X509ExtensionCollection Extensions {
			get { return extensions; }
		}

		public byte[] Hash {
			get {
				if (hash_value == null) {
					ASN1 encodedCRL = new ASN1 (encoded);
					byte[] toBeSigned = encodedCRL [0].GetBytes ();
					using (var ha = PKCS1.CreateFromOid (signatureOID))
						hash_value = ha.ComputeHash (toBeSigned);
				}
				return hash_value;
			}
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
			get { 
				if (signature == null)
					return null;
				return (byte[]) signature.Clone ();
			}
		}

		public byte[] RawData {
			get { return (byte[]) encoded.Clone (); }
		}

		public byte Version {
			get { return version; }
		}

		public bool IsCurrent {
			get { return WasCurrent (DateTime.Now); }
		}

		public bool WasCurrent (DateTime instant) 
		{
			if (nextUpdate == DateTime.MinValue)
				return (instant >= thisUpdate);
			else
				return ((instant >= thisUpdate) && (instant <= nextUpdate));
		}

		public byte[] GetBytes () 
		{
			return (byte[]) encoded.Clone ();
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

		public X509CrlEntry GetCrlEntry (X509Certificate x509) 
		{
			if (x509 == null)
				throw new ArgumentNullException ("x509");

			return GetCrlEntry (x509.SerialNumber);
		}

		public X509CrlEntry GetCrlEntry (byte[] serialNumber) 
		{
			if (serialNumber == null)
				throw new ArgumentNullException ("serialNumber");

			for (int i=0; i < entries.Count; i++) {
				X509CrlEntry entry = (X509CrlEntry) entries [i];
				if (Compare (serialNumber, entry.SerialNumber))
					return entry;
			}
			return null;
		}

		public bool VerifySignature (X509Certificate x509) 
		{
			if (x509 == null)
				throw new ArgumentNullException ("x509");

			// 1. x509 certificate must be a CA certificate (unknown for v1 or v2 certs)
			if (x509.Version >= 3) {
				BasicConstraintsExtension basicConstraints = null;
				// 1.2. Check for ca = true in BasicConstraint
				X509Extension ext = x509.Extensions ["2.5.29.19"];
				if (ext != null) {
					basicConstraints = new BasicConstraintsExtension (ext);
					if (!basicConstraints.CertificateAuthority)
						return false;
				}
				// 1.1. Check for "cRLSign" bit in KeyUsage extension
				ext = x509.Extensions ["2.5.29.15"];
				if (ext != null) {
					KeyUsageExtension keyUsage = new KeyUsageExtension (ext);
					if (!keyUsage.Support (KeyUsages.cRLSign)) {
						// 2nd chance if basicConstraints is CertificateAuthority
						// and KeyUsage support digitalSignature
						if ((basicConstraints == null) || !keyUsage.Support (KeyUsages.digitalSignature))
							return false;
					}
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

		internal bool VerifySignature (DSA dsa) 
		{
			if (signatureOID != "1.2.840.10040.4.3")
				throw new CryptographicException ("Unsupported hash algorithm: " + signatureOID);
			DSASignatureDeformatter v = new DSASignatureDeformatter (dsa);
			// only SHA-1 is supported
			v.SetHashAlgorithm ("SHA1");
			ASN1 sign = new ASN1 (signature);
			if ((sign == null) || (sign.Count != 2))
				return false;
			// parts may be less than 20 bytes (i.e. first bytes were 0x00)
			byte[] part1 = sign [0].Value;
			byte[] part2 = sign [1].Value;
			byte[] sig = new byte [40];
			// parts may be less than 20 bytes (i.e. first bytes were 0x00)
			// parts may be more than 20 bytes (i.e. first byte > 0x80, negative)
			int s1 = System.Math.Max (0, part1.Length - 20);
			int e1 = System.Math.Max (0, 20 - part1.Length);
			Buffer.BlockCopy (part1, s1, sig, e1, part1.Length - s1);
			int s2 = System.Math.Max (0, part2.Length - 20);
			int e2 = System.Math.Max (20, 40 - part2.Length);
			Buffer.BlockCopy (part2, s2, sig, e2, part2.Length - s2);
			return v.VerifySignature (Hash, sig);
		}

		internal bool VerifySignature (RSA rsa) 
		{
			RSAPKCS1SignatureDeformatter v = new RSAPKCS1SignatureDeformatter (rsa);
			v.SetHashAlgorithm (PKCS1.HashNameFromOid (signatureOID));
			return v.VerifySignature (Hash, signature);
		}

		public bool VerifySignature (AsymmetricAlgorithm aa) 
		{
			if (aa == null)
				throw new ArgumentNullException ("aa");

			// only validate the signature (in case we don't have the CA certificate)
			if (aa is RSA)
				return VerifySignature (aa as RSA);
			else if (aa is DSA)
				return VerifySignature (aa as DSA);
			else
				throw new NotSupportedException ("Unknown Asymmetric Algorithm " + aa.ToString ());
		}

		static public X509Crl CreateFromFile (string filename) 
		{
			byte[] crl = null;
			using (FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				crl = new byte [fs.Length];
				fs.Read (crl, 0, crl.Length);
				fs.Close ();
			}
			return new X509Crl (crl);
		}
	}
}
