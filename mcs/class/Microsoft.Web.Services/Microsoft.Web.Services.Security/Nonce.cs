//
// Nonce.cs: Handles WS-Security Nonce
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.Web.Services;

#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	// References:
	// a.	Web Services Security Addendum, Version 1.0, August 18, 2002
	//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnglobspec/html/ws-security.asp
	// b.	Understanding WS-Security
	//	http://msdn.microsoft.com/webservices/building/frameworkandstudio/default.aspx?pull=/library/en-us/dnwssecur/html/understw.asp

	public class Nonce : IXmlElement {

		private byte[] nonce;

		internal Nonce () 
		{
			nonce = new byte [16]; // see reference b.
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			rng.GetBytes (nonce);
		}

		internal Nonce (XmlElement element)
		{
			LoadXml (element);
		}

		public string Value {
			get { return Convert.ToBase64String (nonce); }
		}

		public byte[] GetValueBytes() 
		{
			return (byte[]) nonce.Clone ();
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Nonce, WSSecurity.NamespaceURI);
			xel.InnerText = Convert.ToBase64String (nonce);
			return xel;
		}

		public void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != WSSecurity.ElementNames.Nonce) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new ArgumentException ("invalid LocalName or NamespaceURI");

			XmlAttribute xa = element.Attributes [WSSecurity.AttributeNames.EncodingType, WSSecurity.NamespaceURI];
			if ((xa == null) || (xa.Value == "Base64Binary")) {
				nonce = Convert.FromBase64String (element.InnerText);
			}
			else 
				throw new NotSupportedException (xa.Value);
		}
	}
}
