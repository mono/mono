//
// Signature.cs: Handles WS-Security Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	public sealed class Signature : SignedXml, ISecurityElement {

		private SecurityToken token;
		private SignatureOptions options;

		public Signature (SecurityToken token) 
		{
			if (token == null)
				throw new ArgumentNullException ("token");
			if (!token.SupportsDigitalSignature)
				throw new ArgumentException ("!SupportsDigitalSignature");
			this.token = token;
		}

		public SecurityToken SecurityToken {
			get { return token; }
		}

		public SignatureOptions SignatureOptions {
			get { return options; }
			set { options = value; }
		}

		[MonoTODO]
		public override bool CheckSignature () 
		{
			// note: strangely this is the only (of 3) CheckSignature methods overriden !?!
			return base.CheckSignature ();
		}

		[MonoTODO]
		public override XmlElement GetIdElement (XmlDocument document, string idValue) 
		{
			return base.GetIdElement (document, idValue);
		}

		[MonoTODO]
		public void LoadXml (Security container, XmlElement value)
		{
			if (container == null)
				throw new ArgumentNullException ("container");
			// TODO
			if (value == null)
				throw new SecurityFault ("value == null", null);
		}
	}
}
