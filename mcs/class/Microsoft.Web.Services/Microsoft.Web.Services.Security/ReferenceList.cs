//
// ReferenceList.cs: Handles WS-Security ReferenceList
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
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Xml;

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
			list.Add (reference);
		}

		public bool Contains (string reference) 
		{
			if (reference == null)
				throw new ArgumentNullException ("reference");
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
			// TODO
			return null;
		}

		public void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != "") || (element.NamespaceURI != ""))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			// TODO
		}
	}
}
