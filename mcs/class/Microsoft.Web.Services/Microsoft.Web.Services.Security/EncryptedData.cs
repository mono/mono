//
// EncryptedData.cs: Handles WS-Security EncryptedData
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Microsoft.Web.Services.Timestamp;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public sealed class EncryptedData : ISecurityElement, IXmlElement {

		private SecurityToken token;
		private EncryptionKey encryptionKey;
		private EncryptedKey encryptedKey;
		private string reference;
		private string type;
		private KeyInfo ki;
		private XmlElement target;

		public EncryptedData (EncryptionKey key) : this (key, null) {}

		public EncryptedData (SecurityToken token) : this (token, null) {}

		public EncryptedData (XmlElement element) 
		{
			LoadXml (element);
		}

		public EncryptedData (EncryptionKey key, string reference) 
		{
			this.encryptionKey = key;
			this.reference = reference;
		}

		[MonoTODO]
		public EncryptedData (SecurityToken token, string reference)
		{
// to be compatible with WSE1
//			if (token == null)
//				throw new ArgumentNullException ("token");
			if (!token.SupportsDataEncryption)
				throw new NotSupportedException ("!SupportsDataEncryption");
			if ((reference != null) && (reference [0] != '#'))
				throw new ArgumentException ("reference must start with a #");

			this.reference = reference;
			this.token = token;
			encryptionKey = token.EncryptionKey;
			
			AsymmetricEncryptionKey aek = (token.EncryptionKey as AsymmetricEncryptionKey);
			if (aek != null) {
				encryptedKey = new EncryptedKey (aek);
			}
#if !WSE1
			SymmetricEncryptionKey sek = (token.EncryptionKey as SymmetricEncryptionKey);
			if (sek != null) {
				encryptedKey = new EncryptedKey (sek);
			}
#endif
			type = XmlEncryption.TypeURI.Content;
/*			if (reference == null)
				type = XmlEncryption.TypeURI.Content;
			else if (reference [0] == '#')
				type = XmlEncryption.TypeURI.Element;
			else
				throw new ArgumentException ("reference must start with a #");*/
		}

		[MonoTODO]
		public EncryptedData (XmlElement element, EncryptedKey encryptedKey) 
		{
			this.encryptedKey = encryptedKey;
			LoadXml (element);
		}

		public EncryptedKey EncryptedKey {
			get { return encryptedKey; }
		}

		public string EncryptionMethod {
			get { return encryptedKey.SessionAlgorithmURI; }
		}

		public KeyInfo KeyInfo {
			get { return ki; }
		}

		public string Reference {
			get { return reference; }
		}

		public XmlElement TargetElement {
			get { return target; }
		}

		[MonoTODO]
		public string Type {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				switch (value) {
					case XmlEncryption.TypeURI.Content:
						type = value;
						break;
					case XmlEncryption.TypeURI.Element:
						type = value;
						break;
					default:
						throw new ArgumentException ("bad Type");
				}
			}
		}

		[MonoTODO]
		public XmlElement Decrypt ()
		{
			if (target == null)
				throw new InvalidOperationException ("no document to decrypt");
			DecryptionKey dk = token.DecryptionKey;
			if (dk == null)
				throw new InvalidOperationException ("no key to decrypt with");
			return null;
		}

		// copied from SoapEnvelope.cs to avoid creating the object
		private XmlElement GetBody (XmlDocument message) 
		{
			XmlNodeList xnl = message.GetElementsByTagName (Soap.ElementNames.Body, Soap.NamespaceURI);
			return (XmlElement) xnl [0];
		}

		[MonoTODO("incomplete - only works for soap:Body")]
		public void Encrypt (XmlDocument message) 
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			
			if (reference != null) {
				// TODO
			}
			else {
				target = GetBody (message);
			}

			// now the XML encryption stuff
			if (target != null) {
				SymmetricAlgorithm sa = encryptedKey.Key.Algorithm;
				// IMPORTANT: each encryption MUST have it's own IV !
				sa.GenerateIV ();
				ICryptoTransform ct = sa.CreateEncryptor ();
				byte[] toencrypt = Encoding.UTF8.GetBytes (target.InnerXml);
				byte[] encrypted = ct.TransformFinalBlock (toencrypt, 0, toencrypt.Length);
				// it's our responsability to zeroize our copy of the unencrypted data
				Array.Clear (toencrypt, 0, toencrypt.Length);

				XmlAttribute ema = message.CreateAttribute ("Algorithm");
				ema.InnerText = encryptedKey.SessionAlgorithmURI; 
				XmlElement em = message.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptionMethod, XmlEncryption.NamespaceURI);
				em.Attributes.Append (ema);

				XmlElement cv = message.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.CipherValue, XmlEncryption.NamespaceURI);
				byte[] data = new byte [sa.IV.Length + encrypted.Length];
				Buffer.BlockCopy (sa.IV, 0, data, 0, sa.IV.Length);
				Buffer.BlockCopy (encrypted, 0, data, sa.IV.Length, encrypted.Length);
				cv.InnerText = Convert.ToBase64String (data);
				XmlElement cd = message.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.CipherData, XmlEncryption.NamespaceURI);
				cd.AppendChild (cv);

				XmlAttribute edid = message.CreateAttribute ("Id");
				edid.InnerText = "EncryptedContent-" + Guid.NewGuid ().ToString ();
				XmlAttribute edt = message.CreateAttribute ("Type");
				edt.InnerText = XmlEncryption.TypeURI.Content;

				XmlElement ed = message.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptedData, XmlEncryption.NamespaceURI);
				ed.Attributes.Append (edid);
				ed.Attributes.Append (edt);
				ed.AppendChild (em);
				ed.AppendChild (cd);
				target.RemoveAll ();
				target.AppendChild (ed);
				// no sure why but encryption make this Id appear in WSE so...
				XmlAttribute id = message.CreateAttribute (WSTimestamp.Prefix, WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI);
				id.InnerText = "Id-" + Guid.NewGuid ().ToString ();
				target.Attributes.Append (id);

				encryptedKey.ReferenceList.Add (edid.InnerText);
			}
		}

		[MonoTODO("incomplete - minimal implementation")]
		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			XmlAttribute ema = document.CreateAttribute (XmlEncryption.AttributeNames.Algorithm);
			ema.InnerText = encryptedKey.SessionAlgorithmURI;

			XmlElement em = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptionMethod, XmlEncryption.NamespaceURI);
			em.Attributes.Append (ema);

			XmlAttribute edi = document.CreateAttribute (XmlEncryption.AttributeNames.Id);
			XmlAttribute edt = document.CreateAttribute (XmlEncryption.AttributeNames.Type);
			edt.InnerText = XmlEncryption.TypeURI.Content;

			XmlElement ed = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptedData, XmlEncryption.NamespaceURI);
			ed.Attributes.Append (edi);
			ed.Attributes.Append (edt);
			ed.AppendChild (em);
			return ed;
		}

		[MonoTODO]
		public void LoadXml (XmlElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
		}
	}
}
