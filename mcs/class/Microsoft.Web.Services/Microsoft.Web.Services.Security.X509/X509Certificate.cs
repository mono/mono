//
// X509Certificates.cs: Handles (a little better) X509 certificates.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security;
using MX = Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Microsoft.Web.Services.Security.X509 {

	public sealed class X509Certificate : System.Security.Cryptography.X509Certificates.X509Certificate, IDisposable {

		// do not includes: KeyUsage.keyEncipherment and KeyUsage.keyAgreement (see "OWN.cer")
		private static KeyUsage[] dataEncryption = new KeyUsage [3] { KeyUsage.dataEncipherment, KeyUsage.decipherOnly, KeyUsage.encipherOnly };
		// do not includes KeyUsage.cRLSign, KeyUsage.keyCertSign
		private static KeyUsage[] digitalSignature = new KeyUsage [2] { KeyUsage.digitalSignature, KeyUsage.nonRepudiation };

		private MX.X509Certificate x509;
		private bool m_disposed;

		public X509Certificate (byte[] rawCertificate) : base (rawCertificate) 
		{
			x509 = new MX.X509Certificate (rawCertificate);
			m_disposed = false;
		}

		public X509Certificate (IntPtr handle) : base (handle) 
		{
			x509 = new MX.X509Certificate (base.GetRawCertData ());
			m_disposed = false;
		}

		~X509Certificate () 
		{
			Dispose ();
		}

		// IDisposable
		public void Dispose () 
		{
			if (!m_disposed) {
				// Finalization is now unnecessary
				GC.SuppressFinalize (this);
			}
			// call base class 
			// no need as they all are abstract before us
			m_disposed = true;
		}

		// LAMESPEC: Do not confuse with CreateFromCertFile
		public static X509Certificate CreateCertFromFile (string fileName) 
		{
			System.Security.Cryptography.X509Certificates.X509Certificate x = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile (fileName);
			return new X509Certificate (x.GetRawCertData ());
		}

		// LAMESPEC: Do not confuse with CreateFromSignedFile 
		public static X509Certificate CreateCertFromSignedFile (string fileName) 
		{
			System.Security.Cryptography.X509Certificates.X509Certificate x = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile (fileName);
			return new X509Certificate (x.GetRawCertData ());
		}

		// I knew that using string for date were a bad idea!
		public bool IsCurrent {
			get { return x509.IsCurrent; }
		}

		// Well it seems that DSA certificates aren't popular enough :-(
		// Note: Private key isn't available
		[MonoTODO("Private key isn't yet supported - requires cert stores")]
		public RSA Key {
			get {
				throw new Win32Exception (0, "Private key is unavailable (TODO)");
			}
		}

		// Well it seems that DSA certificates aren't popular enough :-(
		public RSA PublicKey {
			get { return x509.RSA; }
		}

		// Just KeyUsage or also ExtendedKeyUsage ?
		// it doesn't seems to interpret NetscapeCertType
		private bool Supports (KeyUsage[] usages) 
		{
			// X.509 KeyUsage
			MX.X509Extension extn = x509.Extensions ["2.5.29.15"];
			if (extn != null) {
				KeyUsageExtension keyUsage = new KeyUsageExtension (extn);
				foreach (KeyUsage usage in usages) {
					if (keyUsage.Support (usage))
						return true;
				}
				return false;
			}
			// DEPRECATED keyAttributes
			extn = x509.Extensions ["2.5.29.2"];
			if (extn != null) {
				KeyAttributesExtension keyAttr = new KeyAttributesExtension (extn);
				foreach (KeyUsage usage in usages) {
					if (keyAttr.Support (usage))
						return true;
				}
				return false;
			}
			// key usage isn't specified (so it's not limited)
			return true;
		}

		public bool SupportsDataEncryption {
			get { 
				// always true for older certificates
				if (x509.Version < 3)
					return true;
				return Supports (dataEncryption); 
			}
		}

		public bool SupportsDigitalSignature {
			get { 
				// always true for older certificates
				if (x509.Version < 3)
					return true;
				return Supports (digitalSignature); 
			}
		}

		public static X509Certificate FromBase64String (string rawString) 
		{
			byte[] cert = Convert.FromBase64String (rawString);
			return new X509Certificate (cert);
		}

		public byte[] GetKeyIdentifier () 
		{
			// if present in certificate return value of the SubjectKeyIdentifier
			MX.X509Extension extn = x509.Extensions ["2.5.29.14"];
			if (extn != null) {
				ASN1 bs = new ASN1 (extn.Value.Value);
				return bs.Value;
			}
			// strangely DEPRECATED keyAttributes isn't used here (like KeyUsage)

			// if not then we must calculate the SubjectKeyIdentifier ourselve
			// Note: MS does that hash on the complete subjectPublicKeyInfo (unlike PKIX)
			// http://groups.google.ca/groups?selm=e7RqM%24plCHA.1488%40tkmsftngp02&oe=UTF-8&output=gplain
			ASN1 subjectPublicKeyInfo = new ASN1 (0x30);
			ASN1 algo = subjectPublicKeyInfo.Add (new ASN1 (0x30));
			algo.Add (new ASN1 (CryptoConfig.EncodeOID (x509.KeyAlgorithm)));
			// FIXME: does it work for DSA certs (without an 2.5.29.14 extension ?)
			algo.Add (new ASN1 (x509.KeyAlgorithmParameters)); 
			byte[] pubkey = x509.PublicKey;
			byte[] bsvalue = new byte [pubkey.Length + 1]; // add unused bits (0) before the public key
			Array.Copy (pubkey, 0, bsvalue, 1, pubkey.Length);
			subjectPublicKeyInfo.Add (new ASN1 (0x03, bsvalue));
			SHA1 sha = SHA1.Create ();
			return sha.ComputeHash (subjectPublicKeyInfo.GetBytes ());
		}
#if !WSE1
		public string GetSubjectAlternativeName () 
		{
			// if present in certificate return value of the SubjectAltName
			MX.X509Extension extn = x509.Extensions ["2.5.29.17"];
			if (extn != null)
				return null;
			SubjectAltNameExtension altname = new SubjectAltNameExtension (extn);
			return ((altname.RFC822.Length > 0) ? altname.RFC822 [0] : null);
		}
#endif
		// overloaded but WHY ?
		public override int GetHashCode () 
		{
			return base.GetHashCode ();
		}

		public string ToBase64String () 
		{
			return Convert.ToBase64String (base.GetRawCertData ());
		}
	}
}
