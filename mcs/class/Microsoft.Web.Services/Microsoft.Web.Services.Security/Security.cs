//
// Security.cs: Handles WS-Security Security
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Web.Services.Protocols;
using System.Xml;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class Security : SoapHeader, IXmlElement {

		private static string SoapActor = "actor"; // not Actor - no capital A
		private static string SoapNamespaceURI = "http://www.w3.org/2001/12/soap-envelope";

		private SecurityElementCollection elems;
		private SecurityTokenCollection tokens;

		internal Security () 
		{
			elems = new SecurityElementCollection ();
			tokens = new SecurityTokenCollection ();
		}

		public Security (string actor) : this ()
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
			Actor = actor;
		}

		public Security (XmlElement element) : this ()
		{
			LoadXml (element);
		}

		public SecurityElementCollection Elements {
			get { return elems; }
		}

		public SecurityTokenCollection Tokens {
			get { return tokens; }
		}

		[MonoTODO("incomplete")]
		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Security, WSSecurity.NamespaceURI);
//			xel.SetAttribute (SoapActor, SoapNamespaceURI, Actor);
			XmlAttribute xa = document.CreateAttribute (Soap.Prefix, Soap.AttributeNames.MustUnderstand, Soap.NamespaceURI);
			xa.InnerText = "1";
			xel.Attributes.Append (xa);

			foreach (SecurityToken st in tokens) {
				xel.AppendChild (st.GetXml (document));
			}

			foreach (ISecurityElement se in elems) {
				if (se is EncryptedData) {
					EncryptedData ed = (se as EncryptedData);
					ed.Encrypt (document);
					xel.AppendChild (ed.EncryptedKey.GetXml (document));
				}
				else if (se is Signature) {
					// TODO
				}
				// TODO - other elements
			}

			return xel;
		}

		private XmlNode FindId (XmlNode node, string tag, string id) 
		{
			if ((tag == null) || (node.LocalName == tag)) {
				XmlAttribute xa = node.Attributes ["Id"];
				if ((xa != null) && (xa.InnerText == id))
					return node;
			}
			foreach (XmlNode xn in node.ChildNodes) {
				XmlNode result = FindId (xn, tag, id);
				if (result != null)
					return result;
			}
			return null;
		}

		private void LoadEncryptedKey (XmlElement xel) 
		{
			EncryptedKey ek = new EncryptedKey (xel);
			foreach (string s in ek.ReferenceList) {
				// this won't works - probably not a "true" searchable id
				// XmlElement edxel = xel.OwnerDocument.GetElementById (s);
				XmlElement edxel = (XmlElement) FindId ((XmlNode)xel.OwnerDocument.DocumentElement, "EncryptedData", s);
				EncryptedData ed = new EncryptedData (edxel, ek);
				ed.Decrypt ();
			}
		}

		// base class doesn't have a LoadXml method
		[MonoTODO("incomplete")]
		public void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != WSSecurity.ElementNames.Security) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			// get attributes
			XmlAttribute xa = element.Attributes [SoapActor, SoapNamespaceURI];
			Actor = ((xa == null) ? null : xa.Value);

			Elements.Clear ();
			Tokens.Clear ();
			foreach (XmlNode xn in element.ChildNodes) {
				XmlElement xel = (XmlElement) xn;
				switch (xn.NamespaceURI) {
					case WSSecurity.NamespaceURI:
						switch (xn.LocalName) {
							case WSSecurity.ElementNames.UsernameToken:
								UsernameToken unt = new UsernameToken (xel);
								tokens.Add (unt);
								break;
							case WSSecurity.ElementNames.BinarySecurityToken:
		//FIXME						BinarySecurityToken bst = new BinarySecurityToken (xel);
		//FIXME						tokens.Add (bst);
								break;
							}
						break;
					case XmlEncryption.NamespaceURI:
						switch (xn.LocalName) {
							case XmlEncryption.ElementNames.EncryptedKey:
								LoadEncryptedKey (xel);
								xel.ParentNode.RemoveChild (xel);
								break;
						}
						break;
				}
			}
			// did we process all security headers ?
			if (!element.HasChildNodes) {
				// yeah, remove ourself from the header
				element.ParentNode.RemoveChild (element);
			}
		}
	}
}
