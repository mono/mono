//
// KeyInfoRetrievalMethod.cs - KeyInfoRetrievalMethod implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoRetrievalMethod : KeyInfoClause {

		private string URI;

		public KeyInfoRetrievalMethod () {}

		public KeyInfoRetrievalMethod (string strUri) 
		{
			URI = strUri;
		}

		public string Uri {
			get { return URI; }
			set { URI = value; }
		}

		public override XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.RetrievalMethod, XmlSignature.NamespaceURI);
			if (URI != null)
				xel.SetAttribute (XmlSignature.AttributeNames.URI, URI);
			return xel;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName != XmlSignature.ElementNames.RetrievalMethod) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				URI = ""; // not null - so we return URI="" as attribute !!!
			else
				URI = value.Attributes [XmlSignature.AttributeNames.URI].Value;
		}
	}
}