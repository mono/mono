//
// EncryptedKey.cs: Handles WS-Security EncryptedKey
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class EncryptedKey : IXmlElement {

		private EncryptionKey ek;
		private ReferenceList list;
		private string keyex;
		private string session;
		private SymmetricEncryptionKey key;

		internal EncryptedKey ()
		{
			list = new ReferenceList ();
			keyex = XmlEncryption.AlgorithmURI.RSA15;
			session = XmlEncryption.AlgorithmURI.TripleDES;
		}

#if WSE1
		public EncryptedKey (AsymmetricEncryptionKey key) : this ()
#else
		[Obsolete ("since WSE2 TP")]
		public EncryptedKey (EncryptionKey key) : this ()
#endif
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if ((key.KeyInfo == null) || (key.KeyInfo.Count < 1))
				throw new ArgumentException ("no KeyInfo");
			ek = key;
		}

		public EncryptedKey (XmlElement element) : this ()
		{
			LoadXml (element);
		}

		public string EncryptionMethod {
			get { return keyex; }
		}

		public KeyInfo KeyInfo {
			get { return ek.KeyInfo; }
		}

		public ReferenceList ReferenceList {
			get { return list; }
		}

		public string SessionAlgorithmURI { 
			get { return session; }
			set { 
				switch (value) {
					case null:
						throw new ArgumentNullException ("value");
					case XmlEncryption.AlgorithmURI.AES128:
					case XmlEncryption.AlgorithmURI.AES192:
					case XmlEncryption.AlgorithmURI.AES256:
					case XmlEncryption.AlgorithmURI.TripleDES:
						if (session != value)
							key = null;
						session = value;
						break;
					default:
						throw new SecurityFault ("unsupported algorithm", null);
				}
			}
		}

		internal SymmetricEncryptionKey Key {
			get {
				if (key == null) {
					SymmetricAlgorithm sa;
					switch (session) {
						case XmlEncryption.AlgorithmURI.AES128:
							sa = Rijndael.Create ();
							sa.KeySize = 128;
							break;
						case XmlEncryption.AlgorithmURI.AES192:
							sa = Rijndael.Create ();
							sa.KeySize = 192;
							break;
						case XmlEncryption.AlgorithmURI.AES256:
							sa = Rijndael.Create ();
							sa.KeySize = 256;
							break;
						default:
							sa = TripleDES.Create ();
							break;
					}
					key = new SymmetricEncryptionKey (sa);
				}
				return key;
			}
		}

		[MonoTODO("incomplete")]
		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			XmlAttribute ema = document.CreateAttribute (XmlEncryption.AttributeNames.Algorithm);
			ema.InnerText = keyex;
			XmlElement em = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptionMethod, XmlEncryption.NamespaceURI);
			em.Attributes.Append (ema);

			XmlElement ki = KeyInfo.GetXml ();

			AsymmetricKeyExchangeFormatter fmt = null;
			AsymmetricEncryptionKey aek = (ek as AsymmetricEncryptionKey);
			switch (keyex) {
				case XmlEncryption.AlgorithmURI.RSA15:
					fmt = new RSAPKCS1KeyExchangeFormatter (aek.Algorithm);
					break;
				case XmlEncryption.AlgorithmURI.RSAOAEP:
					fmt = new RSAOAEPKeyExchangeFormatter (aek.Algorithm);
					// TODO: parameters
					break;
				default:
					throw new SecurityFault ("unknown key exchange algorithm", null);
			}
			byte[] enckey = fmt.CreateKeyExchange (Key.Algorithm.Key);

			XmlElement cv = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.CipherValue, XmlEncryption.NamespaceURI);
			cv.InnerText = Convert.ToBase64String (enckey);
			XmlElement cd = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.CipherData, XmlEncryption.NamespaceURI);
			cd.AppendChild (cv);

			XmlElement rl = list.GetXml (document);

			XmlAttribute ekt = document.CreateAttribute (XmlEncryption.AttributeNames.Type);
			ekt.InnerText = XmlEncryption.TypeURI.EncryptedKey;
			XmlElement result = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.EncryptedKey, XmlEncryption.NamespaceURI);
			result.Attributes.Append (ekt);
			result.AppendChild (em);
			result.AppendChild (document.ImportNode (ki, true));
			result.AppendChild (cd);
			result.AppendChild (rl);
			return result;
		}

		[MonoTODO("incomplete")]
		public void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			if ((element.LocalName != XmlEncryption.ElementNames.EncryptedKey) || (element.NamespaceURI != XmlEncryption.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			// TODO
		}
	}
}
