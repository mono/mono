//
// SecurityToken.cs: Handles WS-Security SecurityToken
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

	public abstract class SecurityToken : IXmlElement {

		private string id;

		public SecurityToken () 
		{
			// generate Id like WSDK
			id = "SecurityToken-" + Guid.NewGuid ().ToString ();
		}

		public SecurityToken (XmlElement element)
		{
			LoadXml (element);
		}

		public abstract AuthenticationKey AuthenticationKey {get;}

		public abstract DecryptionKey DecryptionKey {get;}

		public abstract EncryptionKey EncryptionKey {get;}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public abstract SignatureKey SignatureKey {get;}

		public abstract bool SupportsDataEncryption {get;}

		public abstract bool SupportsDigitalSignature {get;}

		public abstract XmlElement GetXml (XmlDocument document);

		public abstract void LoadXml (XmlElement element);

		public abstract void Verify();
	}
}
