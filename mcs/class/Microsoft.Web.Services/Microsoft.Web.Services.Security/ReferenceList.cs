//
// ReferenceList.cs: Handles WS-Security ReferenceList
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class ReferenceList : IXmlElement {

		private ArrayList list;

		public ReferenceList () 
		{
			list = new ArrayList ();
		}

		public ReferenceList (XmlElement element) : this ()
		{
			LoadXml (element);
		}

		public void Add (string reference) 
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			if (reference [0] == '#')
				list.Add (reference.Substring (1));
			else
				list.Add (reference);
		}

		public bool Contains (string reference) 
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
			if (reference [0] == '#')
				return list.Contains (reference.Substring (1));
			else
				return list.Contains (reference);
		}

		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator ();
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			XmlElement rl = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.ReferenceList, XmlEncryption.NamespaceURI);
			foreach (string s in list) {
				XmlElement dr = document.CreateElement (XmlEncryption.Prefix, XmlEncryption.ElementNames.DataReference, XmlEncryption.NamespaceURI);
				XmlAttribute uri = document.CreateAttribute (XmlEncryption.AttributeNames.URI);
				uri.InnerText = "#" + s;
				dr.Attributes.Append (uri);
				rl.AppendChild (dr);
			}
			return rl;
		}

		public void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			if ((element.LocalName != XmlEncryption.ElementNames.ReferenceList) || (element.NamespaceURI != XmlEncryption.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			
			foreach (XmlNode xn in element.ChildNodes) {
				// we just drop other elements
				if ((xn.LocalName == XmlEncryption.ElementNames.DataReference) && (xn.NamespaceURI == XmlEncryption.NamespaceURI)) {
					XmlAttribute uri = xn.Attributes [XmlEncryption.AttributeNames.URI];
					if (uri != null)
						Add (uri.InnerText);
				}
			}
		}
	}
}
