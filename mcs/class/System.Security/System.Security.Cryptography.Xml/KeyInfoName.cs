//
// KeyInfoName.cs - KeyInfoName implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoName : KeyInfoClause {

		private string name;

		public KeyInfoName() {}

		public string Value {
			get { return name; }
			set { name = value; }
		}

		public override XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.KeyName, XmlSignature.NamespaceURI);
			xel.InnerText = name;
			return xel;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();
			if ((value.LocalName != XmlSignature.ElementNames.KeyName) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				name = "";
			else
				name = value.InnerText;
		}
	}
}