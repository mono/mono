//
// SecurityToken.cs: Handles WS-Security SecurityToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Xml;
using Microsoft.Web.Services;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public abstract class SecurityToken : IXmlElement {

		private string id;
		private IPrincipal principal;

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
#if WSE1
		public abstract void Verify ();
#else
		public abstract bool Equals (SecurityToken token);

		public abstract override int GetHashCode ();

		public abstract bool IsCurrent {get;}

		public virtual IPrincipal Principal {
			get { return principal; }
			set { principal = value; }
		}

//		public virtual TokenType TokenType {get;}
#endif
	}
}
