//
// Security.cs: Handles WS-Security Security
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	public class Security : SoapHeader, IXmlElement {

		private static string SoapActor = "actor"; // not Actor - no capital A
		private static string SoapNamespaceURI = "http://www.w3.org/2001/12/soap-envelope";

		private SecurityElementCollection elems;
		private SecurityTokenCollection tokens;

		public Security (string actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
			Actor = actor;
		}

		public Security (XmlElement element) 
		{
			LoadXml (element);
		}

		public SecurityElementCollection Elements {
			get { return elems; }
		}

		public SecurityTokenCollection Tokens {
			get { return tokens; }
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Security, WSSecurity.NamespaceURI);
			xel.SetAttribute (SoapActor, SoapNamespaceURI, Actor);

			foreach (ISecurityElement se in Elements) {
				if (se is Signature) {
					// TODO
				}
				else if (se is EncryptedData) {
					xel.AppendChild ((se as EncryptedData).GetXml (document));
				}
			}

			foreach (SecurityToken st in Tokens)
				xel.AppendChild (st.GetXml (document));

			return xel;
		}

		// base class doesn't have a LoadXml method
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
						Tokens.Add (unt);
						break;
					case WSSecurity.ElementNames.BinarySecurityToken:
						BinarySecurityToken bst = new BinarySecurityToken (xel);
						Tokens.Add (bst);
						break;
					}
					break;
				}
			}
		}
	}
}
