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
			get { 
				if (tokens == null)
					tokens = new SecurityTokenCollection ();
				return tokens; 
			}
		}

		[MonoTODO]
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
//FIXME						BinarySecurityToken bst = new BinarySecurityToken (xel);
//FIXME						Tokens.Add (bst);
						break;
					}
					break;
				}
			}
		}
	}
}
