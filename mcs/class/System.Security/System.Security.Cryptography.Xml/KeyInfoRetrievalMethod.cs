//
// KeyInfoRetrievalMethod.cs - KeyInfoRetrievalMethod implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//      Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
//

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoRetrievalMethod : KeyInfoClause {

		private string URI;
		private XmlElement element;

#if NET_1_2
		string type;
#endif

		public KeyInfoRetrievalMethod () {}

		public KeyInfoRetrievalMethod (string strUri) 
		{
			URI = strUri;
		}

#if NET_1_2
		public KeyInfoRetrievalMethod (string strUri, string strType)
			: this (strUri)
		{
			Type = strType;
		}

		public string Type {
			get { return type; }
			set {
				element = null;
				type = value;
			}
		}
#endif

		public string Uri {
			get { return URI; }
			set {
				element = null;
				URI = value;
			}
		}


		public override XmlElement GetXml () 
		{
			if (element != null)
				return element;

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.RetrievalMethod, XmlSignature.NamespaceURI);
			if (URI != null)
				xel.SetAttribute (XmlSignature.AttributeNames.URI, URI);
#if NET_1_2
			if (Type != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Type, Type);
#endif
			return xel;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName != XmlSignature.ElementNames.RetrievalMethod) || (value.NamespaceURI != XmlSignature.NamespaceURI)) {
				URI = ""; // not null - so we return URI="" as attribute !!!
			} else {
				URI = value.Attributes [XmlSignature.AttributeNames.URI].Value;
#if NET_1_2
				if (value.HasAttribute (XmlSignature.AttributeNames.Type))
					Type = value.Attributes [XmlSignature.AttributeNames.Type].Value;
#endif
				element = value;
			}
		}
	}
}
