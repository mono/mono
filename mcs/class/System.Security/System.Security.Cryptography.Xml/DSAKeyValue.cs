//
// DSAKeyValue.cs - DSA KeyValue implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class DSAKeyValue : KeyInfoClause {

		private DSA dsa;

		public DSAKeyValue () 
		{
			dsa = DSA.Create ();
		}

		public DSAKeyValue (DSA key) 
		{
			dsa = key;
		}

		public DSA Key 
		{
			get { return dsa; }
			set { dsa = value; }
		}

		public override XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml ("<KeyValue xmlns=\"" + XmlSignature.NamespaceURI + "\">" + dsa.ToXmlString (false) + "</KeyValue>");
			return document.DocumentElement;

			// FIX: this way we get a xmlns="" in DSAKeyValue
/*			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.KeyValue, XmlSignature.NamespaceURI);
			xel.InnerXml = dsa.ToXmlString (false);
			return xel;*/
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			// FIXME: again hack to match MS implementation (required for previous hack)
			if ((value.LocalName != XmlSignature.ElementNames.KeyValue) || ((value.NamespaceURI != XmlSignature.NamespaceURI) && (value.GetAttribute("xmlns") != XmlSignature.NamespaceURI)))
				throw new CryptographicException ("value");

			dsa.FromXmlString (value.InnerXml);
		}
	}
}