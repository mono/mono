//
// SignedPkcs7.cs - System.Security.Cryptography.Pkcs.SignedPkcs7
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.Pkcs {

	public sealed class SignedPkcs7 {

		private ContentInfo _content;
		private bool _detached;
		private SignerInfoCollection _info;
		private X509CertificateExCollection _certs;
		private SubjectIdentifierType _type;
		private int _version;

		// constructors

		public SignedPkcs7 () 
		{
			_certs = new X509CertificateExCollection ();
			_info = new SignerInfoCollection ();
		}

		public SignedPkcs7 (ContentInfo content) : this (content, false) {}

		public SignedPkcs7 (ContentInfo content, bool detached) : this ()
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			_content = content;
			_detached = detached;
		}

		public SignedPkcs7 (SubjectIdentifierType signerIdentifierType) : this ()
		{
			_type = signerIdentifierType;
			_version = ((_type == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0);
		}

		public SignedPkcs7 (SubjectIdentifierType signerIdentifierType, ContentInfo content) : this (content, false) 
		{
			_type = signerIdentifierType;
			_version = ((_type == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0);
		}

		public SignedPkcs7 (SubjectIdentifierType signerIdentifierType, ContentInfo content, bool detached) : this (content, detached) 
		{
			_type = signerIdentifierType;
			_version = ((_type == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0);
		}

		// properties

		public X509CertificateExCollection Certificates { 
			get { return _certs; }
		}

		public ContentInfo ContentInfo { 
			get { 
				if (_content == null) {
					Oid oid = new Oid (PKCS7.data);
					_content = new ContentInfo (oid, new byte [0]);
				}
				return _content; 
			}
		}

		public bool Detached { 
			get { return _detached; }
		}

		public SignerInfoCollection SignerInfos {
			get { return _info; }
		}

		public int Version { 
			get { return _version; }
		}

		// methods

		public void CheckSignature (bool verifySignatureOnly)
		{
			foreach (SignerInfo si in _info) {
				si.CheckSignature (verifySignatureOnly);
			}
		}

		public void CheckSignature (X509CertificateExCollection extraStore, bool verifySignatureOnly) 
		{
			foreach (SignerInfo si in _info) {
				si.CheckSignature (extraStore, verifySignatureOnly);
			}
		}

		[MonoTODO]
		public void ComputeSignature () 
		{
			throw new CryptographicException ("");
		}

		[MonoTODO]
		public void ComputeSignature (Pkcs7Signer signer)
		{
			ComputeSignature ();
		}

		private string ToString (byte[] array) 
		{
			StringBuilder sb = new StringBuilder ();
			foreach (byte b in array)
				sb.Append (b.ToString ("X2"));
			return sb.ToString ();
		}

		private byte[] GetKeyIdentifier (Mono.Security.X509.X509Certificate x509) 
		{
			// if present in certificate return value of the SubjectKeyIdentifier
			Mono.Security.X509.X509Extension extn = x509.Extensions ["2.5.29.14"];
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

		[MonoTODO("incomplete - missing attributes")]
		public void Decode (byte[] encodedMessage) 
		{
			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (encodedMessage);
			if (ci.ContentType != PKCS7.signedData) 
				throw new Exception ("");

			PKCS7.SignedData sd = new PKCS7.SignedData (ci.Content);
			SubjectIdentifierType type = SubjectIdentifierType.Unknown;
			object o = null;

			X509CertificateEx x509 = null;
			if (sd.SignerInfo.Certificate != null) {
				x509 = new X509CertificateEx (sd.SignerInfo.Certificate.RawData);
			}
			else if ((sd.SignerInfo.IssuerName != null) && (sd.SignerInfo.SerialNumber != null)) {
				byte[] serial = sd.SignerInfo.SerialNumber;
				Array.Reverse (serial); // ???
				type = SubjectIdentifierType.IssuerAndSerialNumber;
				X509IssuerSerial xis = new X509IssuerSerial ();
				xis.IssuerName = sd.SignerInfo.IssuerName;
				xis.SerialNumber = ToString (serial);
				o = xis;
				// TODO: move to a FindCertificate (issuer, serial, collection)
				foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
					if (x.IssuerName == sd.SignerInfo.IssuerName) {
						if (ToString (x.SerialNumber) == xis.SerialNumber) {
							x509 = new X509CertificateEx (x.RawData);
							break;
						}
					}
				}
			}
			else if (sd.SignerInfo.SubjectKeyIdentifier != null) {
				string ski = ToString (sd.SignerInfo.SubjectKeyIdentifier);
				type = SubjectIdentifierType.SubjectKeyIdentifier;
				o = (object) ski;
				// TODO: move to a FindCertificate (ski, collection)
				foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
					if (ToString (GetKeyIdentifier (x)) == ski) {
						x509 = new X509CertificateEx (x.RawData);
						break;
					}
				}
			}

			SignerInfo si = new SignerInfo (sd.SignerInfo.HashName, x509, type, o, sd.SignerInfo.Version);
			// si.AuthenticatedAttributes
			// si.UnauthenticatedAttributes
			_info.Add (si);

			ASN1 content = sd.ContentInfo.Content;
			Oid oid = new Oid (sd.ContentInfo.ContentType);
			_content = new ContentInfo (oid, content[0].Value);

			foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
				_certs.Add (new X509CertificateEx (x.RawData));
			}

			_version = sd.Version;
		}

		[MonoTODO]
		public byte[] Encode ()
		{
			Mono.Security.X509.X509Certificate x509 = null;
/*			PKCS7.SignerInfo si = new PKCS7.SignerInfo ();
			switch (_type) {
				case SubjectIdentifierType.SubjectKeyIdentifier:
					si.SubjectKeyIdentifier = GetKeyIdentifier (x509);
					break;
				default: 
					// SubjectIdentifierType.IssuerAndSerialNumber 
					si.IssuerName = x509.IssuerName;
					si.SerialNumber = x509.SerialNumber;
					break;
			}

			PKCS7.SignedData sd = new PKCS7.SignedData ();
			sd.Version = _version;
			sd.SignerInfo = si;

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (PKCS7.signedData);
			ci.Content = sd.ASN1;
			return ci.GetBytes ();*/
			return null;
		}

		// counterSsignerInfo -> counterSignerInfo
		[MonoTODO]
		public void RemoveSignature (SignerInfo counterSsignerInfo)
		{
		}
	}
}

#endif