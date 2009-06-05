//
// System.Security.Cryptography.Pkcs.EnvelopedCms class
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

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.Pkcs {

	// References
	// a.	PKCS #7: Cryptographic Message Syntax, Version 1.5, Section 10
	//	http://www.faqs.org/rfcs/rfc2315.html

	public sealed class EnvelopedCms {

		private ContentInfo _content;
		private AlgorithmIdentifier _identifier;
		private X509Certificate2Collection _certs;
		private RecipientInfoCollection _recipients;
		private CryptographicAttributeObjectCollection _uattribs;
		private SubjectIdentifierType _idType;
		private int _version;

		// constructors

		public EnvelopedCms () 
		{
			_certs = new X509Certificate2Collection ();
			_recipients = new RecipientInfoCollection ();
			_uattribs = new CryptographicAttributeObjectCollection ();
		}

		public EnvelopedCms (ContentInfo content) : this ()
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			_content = content;
		}

		public EnvelopedCms (ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
			: this (contentInfo) 
		{
			if (encryptionAlgorithm == null)
				throw new ArgumentNullException ("encryptionAlgorithm");

			_identifier = encryptionAlgorithm;
		}

		public EnvelopedCms (SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo) 
			: this (contentInfo) 
		{
			_idType = recipientIdentifierType;
			if (_idType == SubjectIdentifierType.SubjectKeyIdentifier)
				_version = 2;
		}

		public EnvelopedCms (SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
			: this (contentInfo, encryptionAlgorithm) 
		{
			_idType = recipientIdentifierType;
			if (_idType == SubjectIdentifierType.SubjectKeyIdentifier)
				_version = 2;
		}

		// properties

		public X509Certificate2Collection Certificates {
			get { return _certs; }
		}

		public AlgorithmIdentifier ContentEncryptionAlgorithm {
			get { 
				if (_identifier == null)
					_identifier = new AlgorithmIdentifier ();
				return _identifier; 
			}
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

		public RecipientInfoCollection RecipientInfos {
			get { return _recipients; }
		}

		public CryptographicAttributeObjectCollection UnprotectedAttributes { 
			get { return _uattribs; }
		}

		public int Version {
			get { return _version; }
		}

		// methods

		private X509IssuerSerial GetIssuerSerial (string issuer, byte[] serial) 
		{
			X509IssuerSerial xis = new X509IssuerSerial ();
			xis.IssuerName = issuer;
			StringBuilder sb = new StringBuilder ();
			foreach (byte b in serial)
				sb.Append (b.ToString ("X2"));
			xis.SerialNumber = sb.ToString ();
			return xis;
		}

		[MonoTODO]
		public void Decode (byte[] encodedMessage)
		{
			if (encodedMessage == null)
				throw new ArgumentNullException ("encodedMessage");

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (encodedMessage);
			if (ci.ContentType != PKCS7.Oid.envelopedData)
				throw new Exception ("");

			PKCS7.EnvelopedData ed = new PKCS7.EnvelopedData (ci.Content);

			Oid oid = new Oid (ed.ContentInfo.ContentType);
			_content = new ContentInfo (oid, new byte [0]); //ed.ContentInfo.Content.Value);

			foreach (PKCS7.RecipientInfo ri in ed.RecipientInfos) {
				Oid o = new Oid (ri.Oid);
				AlgorithmIdentifier ai = new AlgorithmIdentifier (o);
				SubjectIdentifier si = null;
				if (ri.SubjectKeyIdentifier != null) {
					si = new SubjectIdentifier (SubjectIdentifierType.SubjectKeyIdentifier, ri.SubjectKeyIdentifier);
				}
				else if ((ri.Issuer != null) && (ri.Serial != null)) {
					X509IssuerSerial xis = GetIssuerSerial (ri.Issuer, ri.Serial);
					si = new SubjectIdentifier (SubjectIdentifierType.IssuerAndSerialNumber, (object)xis);
				}
				
				KeyTransRecipientInfo _keyTrans = new KeyTransRecipientInfo (ri.Key, ai, si, ri.Version);
				_recipients.Add (_keyTrans);
			}

			// TODO - Certificates
			// TODO - UnprotectedAttributes 

			_version = ed.Version;
		}

		[MonoTODO]
		public void Decrypt () 
		{
			throw new InvalidOperationException ("not encrypted");
		}

		[MonoTODO]
		public void Decrypt (RecipientInfo recipientInfo) 
		{
			if (recipientInfo == null)
				throw new ArgumentNullException ("recipientInfo");
			Decrypt ();
		}

		[MonoTODO]
		public void Decrypt (RecipientInfo recipientInfo, X509Certificate2Collection extraStore)
		{
			if (recipientInfo == null)
				throw new ArgumentNullException ("recipientInfo");
			if (extraStore == null)
				throw new ArgumentNullException ("extraStore");
			Decrypt ();
		}

		[MonoTODO]
		public void Decrypt (X509Certificate2Collection extraStore) 
		{
			if (extraStore == null)
				throw new ArgumentNullException ("extraStore");
			Decrypt ();
		}

		[MonoTODO]
		public byte[] Encode ()
		{
			throw new InvalidOperationException ("not encrypted");
		}

		[MonoTODO]
		public void Encrypt () 
		{
			if ((_content == null) || (_content.Content == null) || (_content.Content.Length == 0))
				throw new CryptographicException ("no content to encrypt");
		}

		[MonoTODO]
		public void Encrypt (CmsRecipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");
			// TODO
			Encrypt ();
		}

		[MonoTODO]
		public void Encrypt (CmsRecipientCollection recipients)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");
			// ? foreach on Encrypt CmsRecipient ?
		}
	}
}

#endif
