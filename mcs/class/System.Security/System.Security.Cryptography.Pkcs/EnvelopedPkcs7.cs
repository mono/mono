//
// EnvelopedPkcs7.cs - System.Security.Cryptography.Pkcs.EnvelopedPkcs7
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.Pkcs {

	// References
	// a.	PKCS #7: Cryptographic Message Syntax, Version 1.5, Section 10
	//	http://www.faqs.org/rfcs/rfc2315.html

	public class EnvelopedPkcs7 {

		private ContentInfo _content;
		private AlgorithmIdentifier _identifier;
		private X509CertificateExCollection _certs;
		private RecipientInfoCollection _recipients;
		private Pkcs9AttributeCollection _uattribs;
		private SubjectIdentifierType _idType;
		private int _version;

		// constructors

		public EnvelopedPkcs7 () 
		{
			_certs = new X509CertificateExCollection ();
			_recipients = new RecipientInfoCollection ();
			_uattribs = new Pkcs9AttributeCollection ();
		}

		public EnvelopedPkcs7 (ContentInfo content) : this ()
		{
			if (content == null)
				throw new ArgumentNullException ("content");

			_content = content;
		}

		public EnvelopedPkcs7 (ContentInfo contentInfo,	AlgorithmIdentifier encryptionAlgorithm)
			: this (contentInfo) 
		{
			if (encryptionAlgorithm == null)
				throw new ArgumentNullException ("encryptionAlgorithm");

			_identifier = encryptionAlgorithm;
		}

		public EnvelopedPkcs7 (SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo) 
			: this (contentInfo) 
		{
			_idType = recipientIdentifierType;
			_version = ((_idType == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0);
		}

		public EnvelopedPkcs7 (SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
			: this (contentInfo, encryptionAlgorithm) 
		{
			_idType = recipientIdentifierType;
			_version = ((_idType == SubjectIdentifierType.SubjectKeyIdentifier) ? 2 : 0);
		}

		// properties

		public X509CertificateExCollection Certificates {
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
					Oid oid = new Oid (PKCS7.data);
					_content = new ContentInfo (oid, new byte [0]);
				}
				return _content; 
			}
		}

		public RecipientInfoCollection RecipientInfos {
			get { return _recipients; }
		}

		public Pkcs9AttributeCollection UnprotectedAttributes { 
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
			if (ci.ContentType != PKCS7.envelopedData)
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
		public void Decrypt (RecipientInfo recipientInfo, X509CertificateExCollection extraStore)
		{
			if (recipientInfo == null)
				throw new ArgumentNullException ("recipientInfo");
			if (extraStore == null)
				throw new ArgumentNullException ("extraStore");
			Decrypt ();
		}

		[MonoTODO]
		public void Decrypt (X509CertificateExCollection extraStore) 
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
			if ((_content.Content == null) || (_content.Content.Length == 0))
				throw new CryptographicException ("no content to encrypt");
		}

		[MonoTODO]
		public void Encrypt (Pkcs7Recipient recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException ("recipient");
			// TODO
			Encrypt ();
		}

		[MonoTODO]
		public void Encrypt (Pkcs7RecipientCollection recipients)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");
			// ? foreach on Encrypt Pkcs7Recipient ?
		}
	}
}

#endif