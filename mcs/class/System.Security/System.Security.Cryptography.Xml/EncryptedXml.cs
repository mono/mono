//
// EncryptedXml.cs - EncryptedXml implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Collections;
using System.Security.Policy;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public class EncryptedXml {

		#region Fields

		public const string XmlEncAES128KeyWrapUrl	= XmlEncNamespaceUrl + "kw-aes128";
		public const string XmlEncAES128Url		= XmlEncNamespaceUrl + "aes128-cbc";
		public const string XmlEncAES192KeyWrapUrl	= XmlEncNamespaceUrl + "kw-aes192";
		public const string XmlEncAES192Url		= XmlEncNamespaceUrl + "aes192-cbc";
		public const string XmlEncAES256KeyWrapUrl	= XmlEncNamespaceUrl + "kw-aes256";
		public const string XmlEncAES256Url		= XmlEncNamespaceUrl + "aes256-cbc";
		public const string XmlEncDESUrl		= XmlEncNamespaceUrl + "des-cbc";
		public const string XmlEncElementContentUrl	= XmlEncNamespaceUrl + "ElementContent";
		public const string XmlEncElementUrl		= XmlEncNamespaceUrl + "element";
		public const string XmlEncEncryptedKeyUrl	= XmlEncNamespaceUrl + "EncryptedKey";
		public const string XmlEncNamespaceUrl		= "http://www.w3.org/2001/04/xmlenc#";
		public const string XmlEncRSA1_5Url		= XmlEncNamespaceUrl + "rsa-1_5";
		public const string XmlEncRSAOAEPUrl		= XmlEncNamespaceUrl + "rsa-oaep-mgf1p";
		public const string XmlEncSHA256Url		= XmlEncNamespaceUrl + "sha256";
		public const string XmlEncSHA512Url		= XmlEncNamespaceUrl + "sha512";
		public const string XmlEncTripleDESKeyWrapUrl	= XmlEncNamespaceUrl + "kw-tripledes";
		public const string XmlEncTripleDESUrl		= XmlEncNamespaceUrl + "tripledes-cbc";

		Evidence documentEvidence;
		Encoding encoding = Encoding.UTF8;
		Hashtable keyNameMapping = new Hashtable ();
		CipherMode mode = CipherMode.CBC;
		PaddingMode padding = PaddingMode.ISO10126;
		string recipient;
		XmlResolver resolver;

		#endregion // Fields
	
		#region Constructors

		[MonoTODO]
		public EncryptedXml ()
		{
		}

		[MonoTODO]
		public EncryptedXml (XmlDocument document)
		{
		}

		[MonoTODO]
		public EncryptedXml (XmlDocument document, Evidence evidence)
		{
			DocumentEvidence = evidence;
		}
	
		#endregion // Constructors
	
		#region Properties

		public Evidence DocumentEvidence {
			get { return documentEvidence; }
			set { documentEvidence = value; }
		}

		public Encoding Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public CipherMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		public PaddingMode Padding {
			get { return padding; }
			set { padding = value; }
		}

		public string Recipient {
			get { return recipient; }
			set { recipient = value; }
		}
		
		public XmlResolver Resolver {
			get { return resolver; }
			set { resolver = value; }
		}

		#endregion // Properties

		#region Methods

		public void AddKeyNameMapping (string keyName, object keyObject)
		{
			keyNameMapping [keyName] = keyObject;
		}

		public void ClearKeyNameMappings ()
		{
			keyNameMapping.Clear ();
		}

		[MonoTODO]
		public byte[] DecryptData (EncryptedData encryptedData, SymmetricAlgorithm symAlg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DecryptDocument ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual byte[] DecryptEncryptedKey (EncryptedKey encryptedKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] DecryptKey (byte[] keyData, SymmetricAlgorithm symAlg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] DecryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] EncryptData (XmlElement inputElement, SymmetricAlgorithm symAlg, bool content)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] EncryptKey (byte[] keyData, SymmetricAlgorithm symAlg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] EncryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual byte[] GetDecryptionIV (EncryptedData encryptedData, string symAlgUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual SymmetricAlgorithm GetDecryptionKey (EncryptedData encryptedData, string symAlgUri)
		{
			throw new NotImplementedException ();
		}

		public virtual XmlElement GetIdElement (XmlDocument document, string idValue)
		{
                        // this works only if there's a DTD or XSD available to define the ID
			XmlElement xel = document.GetElementById (idValue);
			if (xel == null) {
				// search an "undefined" ID
				xel = (XmlElement) document.SelectSingleNode ("//*[@Id='" + idValue + "']");
			}
			return xel;
		}

		[MonoTODO]
		public static void ReplaceElement (XmlElement inputElement, EncryptedData encryptedData, bool content)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
