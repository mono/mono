//
// ExtensionsHandler.cs: Extensions Configuration Handler (not GUI specific)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace Mono.Tools.CertView {

	public class X509ExtensionsHandler : DictionarySectionHandler {

		public X509ExtensionsHandler () : base () {}

		public override object Create (object parent, object context, XmlNode section) 
		{
			XmlNodeList xnl = section.SelectNodes ("/X509.Extensions/Extension");
			if (xnl == null)
				return null;

			Hashtable ht = new Hashtable ();
			foreach (XmlNode xn in xnl) {
				XmlAttribute xaOid = xn.Attributes ["OID"];
				XmlAttribute xaClass = xn.Attributes ["Class"];
				if ((xaOid != null) && (xaClass != null))
					ht.Add (xaOid.InnerText, xaClass.InnerText);
			}
			return ht;
		}
	}
}
