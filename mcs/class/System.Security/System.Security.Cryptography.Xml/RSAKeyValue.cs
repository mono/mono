//
// RSAKeyValue.cs - RSAKeyValue implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class RSAKeyValue : KeyInfoClause {

		private RSA rsa;

		public RSAKeyValue () 
		{
			rsa = RSA.Create ();
		}

		public RSAKeyValue (RSA key) 
		{
			rsa = key;
		}

		public RSA Key {
			get { return rsa; }
			set { rsa = value; }
		}

		public override XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<KeyValue xmlns=\"" + XmlSignature.NamespaceURI + "\">" + rsa.ToXmlString (false) + "</KeyValue>");
			return document.DocumentElement;

			// FIX: this way we get a xmlns="" in RSAKeyValue
/*			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.KeyValue, XmlSignature.NamespaceURI);
			xel.InnerXml = rsa.ToXmlString (false);
			return xel;*/
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			// FIXME: again hack to match MS implementation (required for previous hack)
			if ((value.LocalName != XmlSignature.ElementNames.KeyValue) || ((value.NamespaceURI != XmlSignature.NamespaceURI) && (value.GetAttribute("xmlns") != XmlSignature.NamespaceURI)))
				throw new CryptographicException ("value");

			rsa.FromXmlString (value.InnerXml);
		}
	}
}