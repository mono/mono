//
// KeyIdentifier.cs: Handles WS-Security KeyIdentifier
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
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Security {

	public class KeyIdentifier : IXmlElement {

		private byte[] kivalue;
		private XmlQualifiedName vtype;

		public KeyIdentifier (byte[] identifier) 
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");
			kivalue = (byte[]) identifier.Clone ();
		}

		public KeyIdentifier (XmlElement element) 
		{
			LoadXml (element);
		}

		public KeyIdentifier (byte[] identifier, XmlQualifiedName valueType) 
		{
			if (identifier == null)
				throw new ArgumentNullException ("identifier");
			kivalue = (byte[]) identifier.Clone ();
			vtype = valueType;
		}

		public byte[] Value {
			get { return (byte[]) kivalue.Clone (); }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				kivalue = value;
			}
		}

		public XmlQualifiedName ValueType {
			get { return vtype; }
			set { vtype = value; }
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			return null;
		}

		public void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != WSSecurity.ElementNames.KeyIdentifier) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			// TODO
		}
	}
}
