//
// EncryptedKey.cs: Handles WS-Security EncryptedKey
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

namespace Microsoft.Web.Services.Security {

	public class EncryptedKey : IXmlElement {

		private AsymmetricEncryptionKey aek;
		private KeyInfo ki;
		private ReferenceList list;

		internal EncryptedKey () 
		{
			list = new ReferenceList ();
		}

		public EncryptedKey (AsymmetricEncryptionKey key) : this ()
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			aek = key;
			ki = ki.KeyInfo;
		}

		public EncryptedKey (XmlElement element) : this ()
		{
			LoadXml (element);
		}

		public string EncryptionMethod {
			get { return null; }
		}

		public KeyInfo KeyInfo {
			get { return ki; }
		}

		public ReferenceList ReferenceList {
			get { return list; }
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			return ki.GetXml ();
		}

		public void LoadXml (XmlElement element) 
		{
			if ((element.LocalName != "") || (element.NamespaceURI != ""))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			ki = new KeyInfo ();
			try {
				ki.LoadXml (element);
			}
			catch {
				throw new ArgumentException ("element has no KeyInfo");
			}
		}
	}
}
