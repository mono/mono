//
// SecurityTokenReferenceType.cs - SecurityTokenReferenceType
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Security {

	public abstract class SecurityTokenReferenceType : KeyInfoClause, IXmlElement {

		[MonoTODO]
		public SecurityTokenReferenceType () {}

		[MonoTODO]
		public SecurityTokenReferenceType (KeyIdentifier identifier) {}

		[MonoTODO]
		public SecurityTokenReferenceType (SecurityToken token) {}

		[MonoTODO]
		public SecurityTokenReferenceType (XmlElement element) {}

		[MonoTODO]
		public IList AnyAttributes {
			get { return null; }
		}

		[MonoTODO]
		public IList AnyElements {
			get { return null; }
		}

		[MonoTODO]
		public KeyIdentifier KeyIdentifier {
			get { return null; }
			set { ; }
		}

		[MonoTODO]
		public string Reference {
			get { return null; }
			set { ; }
		}

		[MonoTODO]
		protected void GetChildXml (XmlDocument document, XmlElement element) {}

		[MonoTODO]
		protected void LoadChildXml (XmlElement element) {}

		[MonoTODO]
		public override XmlElement GetXml () 
		{
			XmlDocument document = new XmlDocument ();
			return GetXml (document);
		}

		[MonoTODO]
		public virtual XmlElement GetXml (XmlDocument document)
		{
			return null;
		}
		
		[MonoTODO]
		public override void LoadXml (XmlElement element) {}
	}
}
