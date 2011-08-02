//
// System.Security.Cryptography.Pkcs.SignedCms class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.Pkcs {

	public sealed class SignedCms {

		private ContentInfo _content;
		private bool _detached;
		private SignerInfoCollection _info;
		private X509Certificate2Collection _certs;
		private SubjectIdentifierType _type;
		private int _version;

		// constructors

		public SignedCms () 
		{
			_certs = new X509Certificate2Collection ();
			_info = new SignerInfoCollection ();
		}

		public SignedCms (ContentInfo content) 
			: this (content, false)
		{
		}

		public SignedCms (ContentInfo content, bool detached) 
			: this ()
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			_content = content;
			_detached = detached;
		}

		public SignedCms (SubjectIdentifierType signerIdentifierType) : this ()
		{
			_type = signerIdentifierType;
		}

		public SignedCms (SubjectIdentifierType signerIdentifierType, ContentInfo content) 
			: this (content, false) 
		{
			_type = signerIdentifierType;
		}

		public SignedCms (SubjectIdentifierType signerIdentifierType, ContentInfo content, bool detached) 
			: this (content, detached) 
		{
			_type = signerIdentifierType;
		}

		// properties

		public X509Certificate2Collection Certificates { 
			get { return _certs; }
		}

		public ContentInfo ContentInfo { 
			get { 
				if (_content == null) {
					Oid oid = new Oid (PKCS7.Oid.data);
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

		[MonoTODO]
		public void CheckSignature (bool verifySignatureOnly)
		{
			foreach (SignerInfo si in _info) {
				si.CheckSignature (verifySignatureOnly);
			}
		}

		[MonoTODO]
		public void CheckSignature (X509Certificate2Collection extraStore, bool verifySignatureOnly) 
		{
			foreach (SignerInfo si in _info) {
				si.CheckSignature (extraStore, verifySignatureOnly);
			}
		}

		[MonoTODO]
		public void CheckHash () 
		{
			throw new InvalidOperationException ("");
		}

		[MonoTODO]
		public void ComputeSignature () 
		{
			throw new CryptographicException ("");
		}

		[MonoTODO]
		public void ComputeSignature (CmsSigner signer)
		{
			ComputeSignature ();
		}

		[MonoTODO]
		public void ComputeSignature (CmsSigner signer, bool silent)
		{
			ComputeSignature ();
		}

		private string ToString (byte[] array, bool reverse) 
		{
			StringBuilder sb = new StringBuilder ();
			if (reverse) {
				for (int i=array.Length - 1; i >= 0; i--)
					sb.Append (array [i].ToString ("X2"));
			} else {
				for (int i=0; i < array.Length; i++)
					sb.Append (array [i].ToString ("X2"));
			}
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
			if (ci.ContentType != PKCS7.Oid.signedData) 
				throw new Exception ("");

			PKCS7.SignedData sd = new PKCS7.SignedData (ci.Content);
			SubjectIdentifierType type = SubjectIdentifierType.Unknown;
			object o = null;

			X509Certificate2 x509 = null;
			if (sd.SignerInfo.Certificate != null) {
				x509 = new X509Certificate2 (sd.SignerInfo.Certificate.RawData);
			}
			else if ((sd.SignerInfo.IssuerName != null) && (sd.SignerInfo.SerialNumber != null)) {
				byte[] serial = sd.SignerInfo.SerialNumber;
				Array.Reverse (serial); // ???
				type = SubjectIdentifierType.IssuerAndSerialNumber;
				X509IssuerSerial xis = new X509IssuerSerial ();
				xis.IssuerName = sd.SignerInfo.IssuerName;
				xis.SerialNumber = ToString (serial, true);
				o = xis;
				// TODO: move to a FindCertificate (issuer, serial, collection)
				foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
					if (x.IssuerName == sd.SignerInfo.IssuerName) {
						if (ToString (x.SerialNumber, true) == xis.SerialNumber) {
							x509 = new X509Certificate2 (x.RawData);
							break;
						}
					}
				}
			}
			else if (sd.SignerInfo.SubjectKeyIdentifier != null) {
				string ski = ToString (sd.SignerInfo.SubjectKeyIdentifier, false);
				type = SubjectIdentifierType.SubjectKeyIdentifier;
				o = (object) ski;
				// TODO: move to a FindCertificate (ski, collection)
				foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
					if (ToString (GetKeyIdentifier (x), false) == ski) {
						x509 = new X509Certificate2 (x.RawData);
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

			if (!_detached || _content == null) {
				if (content[0] == null)
					throw new ArgumentException ("ContentInfo has no content. Detached signature ?");

				_content = new ContentInfo (oid, content[0].Value);
			}

			foreach (Mono.Security.X509.X509Certificate x in sd.Certificates) {
				_certs.Add (new X509Certificate2 (x.RawData));
			}

			_version = sd.Version;
		}

		[MonoTODO]
		public byte[] Encode ()
		{
/*			Mono.Security.X509.X509Certificate x509 = null;
			Cms.SignerInfo si = new Cms.SignerInfo ();
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

			Cms.SignedData sd = new Cms.SignedData ();
			sd.Version = _version;
			sd.SignerInfo = si;

			Cms.ContentInfo ci = new Cms.ContentInfo (Cms.signedData);
			ci.Content = sd.ASN1;
			return ci.GetBytes ();*/
			return null;
		}

		[MonoTODO]
		public void RemoveSignature (SignerInfo signerInfo)
		{
		}

		[MonoTODO]
		public void RemoveSignature (int index)
		{
		}
	}
}

#endif
