//
// SecurityTokenReference.cs: Handles WS-Security SecurityTokenReference
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class SecurityTokenReference : KeyInfoClause, IXmlElement {
// TODO WSE2 public class SecurityTokenReference : SecurityTokenReferenceType

		private KeyIdentifier kid;
		private string reference;

		public SecurityTokenReference () : base () {}

		public SecurityTokenReference (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public KeyIdentifier KeyIdentifier {
			get { return kid; }
			set { kid = value; }
		}

		public string Reference {
			get { return reference; }
			set { reference = value; }
		}

		// note: old format used by S.S.C.Xml
		public override XmlElement GetXml () 
		{
			return GetXml (new XmlDocument ());
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			XmlElement str = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.SecurityTokenReference, WSSecurity.NamespaceURI);
			if (kid != null)
				str.AppendChild (kid.GetXml (document));
			if (reference != null) {
				XmlAttribute uri = document.CreateAttribute (WSSecurity.AttributeNames.Uri);
				uri.InnerText = "#" + reference;
				XmlElement r = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Reference, WSSecurity.NamespaceURI);
				r.Attributes.Append (uri);
				str.AppendChild (r);
			}
			return str;
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			if ((element.LocalName != WSSecurity.ElementNames.SecurityTokenReference) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			XmlNodeList xnl = element.GetElementsByTagName (WSSecurity.ElementNames.KeyIdentifier, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count == 1)) {
				kid = new KeyIdentifier ((xnl [0] as XmlElement));
			}

			xnl = element.GetElementsByTagName (WSSecurity.ElementNames.Reference, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count == 1)) {
				XmlAttribute uri = xnl [0].Attributes [WSSecurity.AttributeNames.Uri];
				if (uri != null) {
					reference = uri.InnerText;
					if (reference [0] == '#')
						reference = reference.Substring (1);
				}
			}
		}
	}
}
