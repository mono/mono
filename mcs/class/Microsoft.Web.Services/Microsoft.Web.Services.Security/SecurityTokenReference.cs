//
// SecurityTokenReference.cs: Handles WS-Security SecurityTokenReference
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
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class SecurityTokenReference : KeyInfoClause, IXmlElement {

		private KeyIdentifier kid;
		private string reference;

		public SecurityTokenReference () 
		{
		}

		public SecurityTokenReference (XmlElement element) 
		{
		}

		public KeyIdentifier KeyIdentifier {
			get { return kid; }
			set { kid = value; }
		}

		public string Reference {
			get { return reference; }
			set { reference = value; }
		}

		public override XmlElement GetXml() 
		{
			// TODO
			return null;
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			// TODO
			return null;
		}

		public override void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != "") || (element.NamespaceURI != ""))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			// TODO
		}
	}
}
