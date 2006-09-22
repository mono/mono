//
// EncryptedXmlTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml
{
	[TestFixture]
	public class EncryptedXmlTest
	{
		[Test]
		public void Sample1 ()
		{
			AssertDecryption1 ("Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml");
		}

		void AssertDecryption1 (string filename)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (filename);
			EncryptedXml encxml = new EncryptedXml (doc);
			RSACryptoServiceProvider rsa = new X509Certificate2 ("Test/System.Security.Cryptography.Xml/sample.pfx", "mono").PrivateKey as RSACryptoServiceProvider;
			XmlNamespaceManager nm = new XmlNamespaceManager (doc.NameTable);
			nm.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
			nm.AddNamespace ("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			nm.AddNamespace ("e", EncryptedXml.XmlEncNamespaceUrl);
			XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Header/o:Security/e:EncryptedKey", nm) as XmlElement;
			EncryptedKey ekey = new EncryptedKey ();
			ekey.LoadXml (el);
			byte [] key = rsa.Decrypt (ekey.CipherData.CipherValue, true);
			Rijndael aes = new RijndaelManaged ();
			aes.Key = key;
			aes.Mode = CipherMode.CBC;
			ArrayList al = new ArrayList ();
			foreach (XmlElement ed in doc.SelectNodes ("//e:EncryptedData", nm))
				al.Add (ed);
			foreach (XmlElement ed in al) {
				EncryptedData edata = new EncryptedData ();
				edata.LoadXml (ed);
				encxml.ReplaceData (ed, encxml.DecryptData (edata, aes));
			}
		}

		[Test]
		public void Sample2 ()
		{
			RijndaelManaged aes = new RijndaelManaged ();
			aes.Mode = CipherMode.CBC;
			aes.KeySize = 256;
			aes.Key = Convert.FromBase64String ("o/ilseZu+keLBBWGGPlUHweqxIPc4gzZEFWr2nBt640=");
			aes.Padding = PaddingMode.Zeros;

			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load ("Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml");
			EncryptedXml encxml = new EncryptedXml (doc);
			EncryptedData edata = new EncryptedData ();
			edata.LoadXml (doc.DocumentElement);
			encxml.ReplaceData (doc.DocumentElement, encxml.DecryptData (edata, aes));
		}

		[Test]
		public void Sample3 ()
		{
			AssertDecryption1 ("Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml");
		}

		[Test]
		public void RoundtripSample1 ()
		{
			StringWriter sw = new StringWriter ();

			// Encryption
			{
				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.LoadXml ("<root>  <child>sample</child>   </root>");

				XmlElement body = doc.DocumentElement;

				RijndaelManaged aes = new RijndaelManaged ();
				aes.Mode = CipherMode.CBC;
				aes.KeySize = 256;
				aes.IV = Convert.FromBase64String ("pBUM5P03rZ6AE4ZK5EyBrw==");
				aes.Key = Convert.FromBase64String ("o/ilseZu+keLBBWGGPlUHweqxIPc4gzZEFWr2nBt640=");
				aes.Padding = PaddingMode.Zeros;

				EncryptedXml exml = new EncryptedXml ();
				byte [] encrypted = exml.EncryptData (body, aes, false);
				EncryptedData edata = new EncryptedData ();
				edata.Type = EncryptedXml.XmlEncElementUrl;
				edata.EncryptionMethod = new EncryptionMethod (EncryptedXml.XmlEncAES256Url);
				EncryptedKey ekey = new EncryptedKey ();
				// omit key encryption, here for testing
				byte [] encKeyBytes = aes.Key;
				ekey.CipherData = new CipherData (encKeyBytes);
				ekey.EncryptionMethod = new EncryptionMethod (EncryptedXml.XmlEncRSA15Url);
				DataReference dr = new DataReference ();
				dr.Uri = "_0";
				ekey.AddReference (dr);
				edata.KeyInfo.AddClause (new KeyInfoEncryptedKey (ekey));
				edata.KeyInfo = new KeyInfo ();
				ekey.KeyInfo.AddClause (new RSAKeyValue (RSA.Create ()));
				edata.CipherData.CipherValue = encrypted;
				EncryptedXml.ReplaceElement (doc.DocumentElement, edata, false);
				doc.Save (new XmlTextWriter (sw));
			}

			// Decryption
			{
				RijndaelManaged aes = new RijndaelManaged ();
				aes.Mode = CipherMode.CBC;
				aes.KeySize = 256;
				aes.Key = Convert.FromBase64String (
				        "o/ilseZu+keLBBWGGPlUHweqxIPc4gzZEFWr2nBt640=");
				aes.Padding = PaddingMode.Zeros;

				XmlDocument doc = new XmlDocument ();
				doc.PreserveWhitespace = true;
				doc.LoadXml (sw.ToString ());
				EncryptedXml encxml = new EncryptedXml (doc);
				EncryptedData edata = new EncryptedData ();
				edata.LoadXml (doc.DocumentElement);
				encxml.ReplaceData (doc.DocumentElement, encxml.DecryptData (edata, aes));
			}
		}
	}
}
#endif
