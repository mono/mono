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
			dsa = (DSA)DSA.Create ();
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
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.KeyValue, XmlSignature.NamespaceURI);
			xel.SetAttribute ("xmlns", XmlSignature.NamespaceURI);
			xel.InnerXml = dsa.ToXmlString (false);
			return xel;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName != XmlSignature.ElementNames.KeyValue) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ("value");

			dsa.FromXmlString (value.InnerXml);
		}
	}
}
