//
// EncryptedXml.cs - EncryptedXml implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Collections;
using System.IO;
using System.Security.Cryptography;
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

		public byte[] DecryptData (EncryptedData encryptedData, SymmetricAlgorithm symAlg)
		{
			return Transform (encryptedData.CipherData.CipherValue, symAlg.CreateDecryptor ());
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
			if (symAlg is TripleDES)
				return SymmetricKeyWrap.TripleDESKeyWrapDecrypt (symAlg.Key, keyData);
			if (symAlg is Rijndael)
				return SymmetricKeyWrap.TripleDESKeyWrapDecrypt (symAlg.Key, keyData);

			throw new CryptographicException ("The specified cryptographic transform is not supported.");
		}

		[MonoTODO]
		public static byte[] DecryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			throw new NotImplementedException ();
		}

		public byte[] EncryptData (XmlElement inputElement, SymmetricAlgorithm symAlg, bool content)
		{
			if (content)
				return Transform (Encoding.GetBytes (inputElement.InnerXml), symAlg.CreateEncryptor ());
			else
				return Transform (Encoding.GetBytes (inputElement.OuterXml), symAlg.CreateEncryptor ());
		}

		[MonoTODO ("Do we need to support more algorithms?")]
		public static byte[] EncryptKey (byte[] keyData, SymmetricAlgorithm symAlg)
		{
			if (symAlg is TripleDES)
				return SymmetricKeyWrap.TripleDESKeyWrapEncrypt (symAlg.Key, keyData);
			if (symAlg is Rijndael)
				return SymmetricKeyWrap.AESKeyWrapEncrypt (symAlg.Key, keyData);

			throw new CryptographicException ("The specified cryptographic transform is not supported.");
		}

		[MonoTODO ("Not sure what this is for.")]
		public static byte[] EncryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			throw new NotImplementedException ();
		}

		private static SymmetricAlgorithm GetAlgorithm (string symAlgUri)
		{
			SymmetricAlgorithm symAlg = null;

			switch (symAlgUri) {
			case XmlEncAES128Url:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 128;
				break;
			case XmlEncAES192Url:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 192;
				break;
			case XmlEncAES256Url:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 256;
				break;
			case XmlEncDESUrl:
				symAlg = SymmetricAlgorithm.Create ("DES");
				break;
			case XmlEncTripleDESUrl:
				symAlg = SymmetricAlgorithm.Create ("TripleDES");
				break;
			default:
				throw new ArgumentException ("symAlgUri");
			}

			return symAlg;
		}

		[MonoTODO]
		public virtual byte[] GetDecryptionIV (EncryptedData encryptedData, string symAlgUri)
		{
			SymmetricAlgorithm symAlg = GetAlgorithm (symAlgUri);

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual SymmetricAlgorithm GetDecryptionKey (EncryptedData encryptedData, string symAlgUri)
		{
			SymmetricAlgorithm symAlg = GetAlgorithm (symAlgUri);

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

		private byte[] Transform (byte[] data, ICryptoTransform transform)
		{
			MemoryStream output = new MemoryStream ();
			CryptoStream crypto = new CryptoStream (output, transform, CryptoStreamMode.Write);
			crypto.Write (data, 0, data.Length);
			crypto.Close ();
			output.Close ();

			return output.ToArray ();
		}

		#endregion // Methods
	}
}

#endif
