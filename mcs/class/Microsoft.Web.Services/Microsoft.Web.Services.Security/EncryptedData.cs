//
// EncryptedData.cs: Handles WS-Security EncryptedData
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
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public sealed class EncryptedData : ISecurityElement, IXmlElement {

		private EncryptionKey encryptionKey;
		private EncryptedKey encryptedKey;
		private ReferenceList list;

		public EncryptedData (EncryptionKey key) 
		{
			this.encryptionKey = key;
		}

		// TODO
		public EncryptedData (SecurityToken token) {}

		// TODO
		public EncryptedData (XmlElement element) {}

		public EncryptedData (XmlElement element, EncryptedKey encryptedKey) 
		{
			// TODO
			this.encryptedKey = encryptedKey;
		}

		public EncryptedKey EncryptedKey {
			get { return encryptedKey; }
		}

		public XmlElement Decrypt() 
		{
			// TODO
			return null;
		}

		public void Encrypt (XmlDocument message) 
		{
			// TODO
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			// TODO
			return null;
		}

		public void LoadXml (XmlElement element) 
		{
			// TODO
		}
	}
}
