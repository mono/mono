//
// BinarySecurityToken.cs: Handles WS-Security BinarySecurityToken
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

	public abstract class BinarySecurityToken : SecurityToken {

		private const string name = "Base64Binary";
		private const string ns = "http://schemas.xmlsoap.org/ws/2002/07/secext";

		private XmlQualifiedName encoding;
		private XmlQualifiedName valueType;

		public BinarySecurityToken (XmlElement element) : base (element)
		{
			encoding = new XmlQualifiedName (name, ns);
		}

		public BinarySecurityToken (XmlQualifiedName valueType) 
		{
			if (valueType == null)
				throw new ArgumentNullException ("valueType");

			encoding = new XmlQualifiedName (name, ns);
			this.valueType = valueType;
		}

		public XmlQualifiedName EncodingType {
			get { return encoding; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				encoding = value; 
			}
		}

		[MonoTODO ("used ?")]
		public virtual byte[] RawData {
			get { return null; }
			set { ; }
		}

		public virtual XmlQualifiedName ValueType {
			get { return valueType; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				valueType = value; 
			}
		}

		public override XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			// TODO check namespace and valueType
			throw new SecurityFormatException ("namespace");
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			if ((element.LocalName != name) || (element.NamespaceURI != ns))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");
			// TODO
		}
	}
}
