//
// EncryptedXml.cs - EncryptedXml implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

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

#if NET_2_0

using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
		public const string XmlEncElementContentUrl	= XmlEncNamespaceUrl + "Content";
		public const string XmlEncElementUrl		= XmlEncNamespaceUrl + "Element";
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
		XmlDocument document;

		#endregion // Fields
	
		#region Constructors

		[MonoTODO]
		public EncryptedXml ()
		{
		}

		[MonoTODO]
		public EncryptedXml (XmlDocument document)
		{
			this.document = document;
		}

		[MonoTODO]
		public EncryptedXml (XmlDocument document, Evidence evidence)
		{
			this.document = document;
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
			return Transform (encryptedData.CipherData.CipherValue, symAlg.CreateDecryptor (), symAlg.BlockSize / 8);
		}

		public void DecryptDocument ()
		{
			XmlNodeList nodes = document.GetElementsByTagName ("EncryptedData", XmlEncNamespaceUrl);
			foreach (XmlNode node in nodes) {
				EncryptedData encryptedData = new EncryptedData ();
				encryptedData.LoadXml ((XmlElement) node);
				SymmetricAlgorithm symAlg = GetDecryptionKey (encryptedData, encryptedData.EncryptionMethod.KeyAlgorithm);
				ReplaceData ((XmlElement) node, DecryptData (encryptedData, symAlg));
			}
		}

		public virtual byte[] DecryptEncryptedKey (EncryptedKey encryptedKey)
		{
			SymmetricAlgorithm keyAlg = null;
			foreach (KeyInfoClause innerClause in encryptedKey.KeyInfo) {
				if (innerClause is KeyInfoName) {
					keyAlg = (SymmetricAlgorithm) keyNameMapping [((KeyInfoName) innerClause).Value];
					break;
				}
			}
			return DecryptKey (encryptedKey.CipherData.CipherValue, keyAlg);
		}

		public static byte[] DecryptKey (byte[] keyData, SymmetricAlgorithm symAlg)
		{
			if (symAlg is TripleDES)
				return SymmetricKeyWrap.TripleDESKeyWrapDecrypt (symAlg.Key, keyData);
			if (symAlg is Rijndael)
				return SymmetricKeyWrap.AESKeyWrapDecrypt (symAlg.Key, keyData);
			throw new CryptographicException ("The specified cryptographic transform is not supported.");
		}

		[MonoTODO ("Test this.")]
		public static byte[] DecryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			AsymmetricKeyExchangeDeformatter deformatter = null;
			if (fOAEP) 
				deformatter = new RSAOAEPKeyExchangeDeformatter (rsa);
			else
				deformatter = new RSAPKCS1KeyExchangeDeformatter (rsa);
			return deformatter.DecryptKeyExchange (keyData);
		}

		public EncryptedData Encrypt (XmlElement inputElement, string keyName)
		{
			// There are two keys of note here.
			// 1) KeyAlg: the key-encryption-key is used to wrap a key.  The keyName
			//    parameter will give us the KEK.
			// 2) SymAlg: A 256-bit AES key will be generated to encrypt the contents.
			//    This key will be wrapped using the KEK.

			SymmetricAlgorithm symAlg = SymmetricAlgorithm.Create ("Rijndael");
			symAlg.KeySize = 256;
			symAlg.GenerateKey ();
			symAlg.GenerateIV ();

			SymmetricAlgorithm keyAlg = (SymmetricAlgorithm) keyNameMapping [keyName];
			EncryptedData encryptedData = new EncryptedData ();

			EncryptedKey encryptedKey = new EncryptedKey();
			encryptedKey.EncryptionMethod = new EncryptionMethod (GetKeyWrapAlgorithmUri (keyAlg));
			encryptedKey.CipherData = new CipherData (EncryptKey (symAlg.Key, keyAlg));
			encryptedKey.KeyInfo = new KeyInfo();
			encryptedKey.KeyInfo.AddClause (new KeyInfoName (keyName));
			
			encryptedData.Type = XmlEncElementUrl;
			encryptedData.EncryptionMethod = new EncryptionMethod (GetAlgorithmUri (symAlg));
			encryptedData.KeyInfo = new KeyInfo ();
			encryptedData.KeyInfo.AddClause (new KeyInfoEncryptedKey (encryptedKey));
			encryptedData.CipherData = new CipherData (EncryptData (inputElement, symAlg, false));

			return encryptedData;
		}
		
		[MonoTODO]
		public EncryptedData Encrypt (XmlElement inputElement, X509CertificateEx certificate)
		{
			throw new NotImplementedException ();
		}

		public byte[] EncryptData (byte[] plainText, SymmetricAlgorithm symAlg)
		{
			// Write the symmetric algorithm IV and ciphertext together.
			// We use a memory stream to accomplish this.
			MemoryStream stream = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (stream);

			writer.Write (symAlg.IV);
			writer.Write (Transform (plainText, symAlg.CreateEncryptor ()));
			writer.Flush ();

			byte [] output = stream.ToArray ();

			writer.Close ();
			stream.Close ();

			return output;
		}

		public byte[] EncryptData (XmlElement inputElement, SymmetricAlgorithm symAlg, bool content)
		{
			if (content)
				return EncryptData (Encoding.GetBytes (inputElement.InnerXml), symAlg);
			else
				return EncryptData (Encoding.GetBytes (inputElement.OuterXml), symAlg);
		}

		public static byte[] EncryptKey (byte[] keyData, SymmetricAlgorithm symAlg)
		{
			if (symAlg is TripleDES)
				return SymmetricKeyWrap.TripleDESKeyWrapEncrypt (symAlg.Key, keyData);
			if (symAlg is Rijndael)
				return SymmetricKeyWrap.AESKeyWrapEncrypt (symAlg.Key, keyData);

			throw new CryptographicException ("The specified cryptographic transform is not supported.");
		}

		[MonoTODO ("Test this.")]
		public static byte[] EncryptKey (byte[] keyData, RSA rsa, bool fOAEP)
		{
			AsymmetricKeyExchangeFormatter formatter = null;
			if (fOAEP) 
				formatter = new RSAOAEPKeyExchangeFormatter (rsa);
			else
				formatter = new RSAPKCS1KeyExchangeFormatter (rsa);
			return formatter.CreateKeyExchange (keyData);
		}

		private static SymmetricAlgorithm GetAlgorithm (string symAlgUri)
		{
			SymmetricAlgorithm symAlg = null;

			switch (symAlgUri) {
			case XmlEncAES128Url:
			case XmlEncAES128KeyWrapUrl:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 128;
				break;
			case XmlEncAES192Url:
			case XmlEncAES192KeyWrapUrl:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 192;
				break;
			case XmlEncAES256Url:
			case XmlEncAES256KeyWrapUrl:
				symAlg = SymmetricAlgorithm.Create ("Rijndael");
				symAlg.KeySize = 256;
				break;
			case XmlEncDESUrl:
				symAlg = SymmetricAlgorithm.Create ("DES");
				break;
			case XmlEncTripleDESUrl:
			case XmlEncTripleDESKeyWrapUrl:
				symAlg = SymmetricAlgorithm.Create ("TripleDES");
				break;
			default:
				throw new ArgumentException ("symAlgUri");
			}

			return symAlg;
		}

		private static string GetAlgorithmUri (SymmetricAlgorithm symAlg)
		{
			if (symAlg is Rijndael)
			{
				switch (symAlg.KeySize) {
				case 128:
					return XmlEncAES128Url;
				case 192:
					return XmlEncAES192Url;
				case 256:
					return XmlEncAES256Url;
				}
			}
			else if (symAlg is DES)
				return XmlEncDESUrl;
			else if (symAlg is TripleDES)
				return XmlEncTripleDESUrl;

			throw new ArgumentException ("symAlg");
		}

		private static string GetKeyWrapAlgorithmUri (SymmetricAlgorithm symAlg)
		{
			if (symAlg is Rijndael)
			{
				switch (symAlg.KeySize) {
				case 128:
					return XmlEncAES128KeyWrapUrl;
				case 192:
					return XmlEncAES192KeyWrapUrl;
				case 256:
					return XmlEncAES256KeyWrapUrl;
				}
			}
			else if (symAlg is TripleDES)
				return XmlEncTripleDESKeyWrapUrl;

			throw new ArgumentException ("symAlg");
		}

		public virtual byte[] GetDecryptionIV (EncryptedData encryptedData, string symAlgUri)
		{
			SymmetricAlgorithm symAlg = GetAlgorithm (symAlgUri);
			byte[] iv = new Byte [symAlg.BlockSize / 8];
			Buffer.BlockCopy (encryptedData.CipherData.CipherValue, 0, iv, 0, iv.Length);
			return iv;
		}

		public virtual SymmetricAlgorithm GetDecryptionKey (EncryptedData encryptedData, string symAlgUri)
		{
			SymmetricAlgorithm symAlg = GetAlgorithm (symAlgUri);
			symAlg.IV = GetDecryptionIV (encryptedData, encryptedData.EncryptionMethod.KeyAlgorithm);
			KeyInfo keyInfo = encryptedData.KeyInfo;
			foreach (KeyInfoClause clause in keyInfo) {
				if (clause is KeyInfoEncryptedKey) {
					symAlg.Key = DecryptEncryptedKey (((KeyInfoEncryptedKey) clause).EncryptedKey);
					break;
				}
			}
			return symAlg;
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

		public void ReplaceData (XmlElement inputElement, byte[] decryptedData)
		{
			XmlDocument ownerDocument = inputElement.OwnerDocument;
			XmlTextReader reader = new XmlTextReader (new StringReader (Encoding.GetString (decryptedData, 0, decryptedData.Length)));
			reader.MoveToContent ();
			XmlNode node = ownerDocument.ReadNode (reader);
			inputElement.ParentNode.ReplaceChild (node, inputElement);
		}

		public static void ReplaceElement (XmlElement inputElement, EncryptedData encryptedData, bool content)
		{
			XmlDocument ownerDocument = inputElement.OwnerDocument;
			inputElement.ParentNode.ReplaceChild (encryptedData.GetXml (ownerDocument), inputElement);
		}

		private byte[] Transform (byte[] data, ICryptoTransform transform)
		{
			return Transform (data, transform, 0);
		}

		private byte[] Transform (byte[] data, ICryptoTransform transform, int startIndex)
		{
			MemoryStream output = new MemoryStream ();
			CryptoStream crypto = new CryptoStream (output, transform, CryptoStreamMode.Write);
			crypto.Write (data, startIndex, data.Length - startIndex);

			crypto.FlushFinalBlock ();

			byte[] result = output.ToArray ();

			crypto.Close ();
			output.Close ();

			return result;
		}

		#endregion // Methods
	}
}

#endif
