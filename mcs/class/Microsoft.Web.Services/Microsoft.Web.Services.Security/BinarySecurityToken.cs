//
// BinarySecurityToken.cs: Handles WS-Security BinarySecurityToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

using Microsoft.Web.Services.Timestamp;

namespace Microsoft.Web.Services.Security {

	public abstract class BinarySecurityToken : SecurityToken {

		private const string name = "Base64Binary";

		private XmlQualifiedName encoding;
		internal XmlQualifiedName valueType;

		public BinarySecurityToken (XmlElement element) : base (element)
		{
		}

		public BinarySecurityToken (XmlQualifiedName valueType) 
		{
			if (valueType == null)
				throw new ArgumentNullException ("valueType");

			encoding = new XmlQualifiedName (name, WSSecurity.NamespaceURI);
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

			XmlAttribute bstvt = document.CreateAttribute (WSSecurity.AttributeNames.ValueType);
			bstvt.InnerText = String.Concat (WSSecurity.Prefix, ":", "X509v3");
			XmlAttribute bstet = document.CreateAttribute (WSSecurity.AttributeNames.EncodingType);
			bstet.InnerText = String.Concat (WSSecurity.Prefix, ":", name);
			XmlAttribute bstid = document.CreateAttribute (WSTimestamp.Prefix, WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI);
			bstid.InnerText = Id;
			
			XmlElement bst = document.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.BinarySecurityToken, WSSecurity.NamespaceURI);
			bst.Attributes.Append (bstvt);
			bst.Attributes.Append (bstet);
			bst.Attributes.Append (bstid);
			bst.InnerText = Convert.ToBase64String (RawData);
			return bst;
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			if ((element.LocalName != WSSecurity.ElementNames.BinarySecurityToken) || (element.NamespaceURI != WSSecurity.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			XmlAttribute xaid = element.Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
			if (xaid != null) {
				Id = xaid.InnerText;
			}

			char[] separator = new char[1] { ':' };
			
			XmlAttribute xavt = element.Attributes [WSTimestamp.AttributeNames.ValueType];
			if (xavt != null) {
				string[] vt = xavt.InnerText.Split (separator);
				string ns = element.GetNamespaceOfPrefix (vt [0]);
				XmlQualifiedName xqnv = new XmlQualifiedName (vt [1], ns);
				ValueType = xqnv;
			}
			
			XmlAttribute xaet = element.Attributes [WSSecurity.AttributeNames.EncodingType];
			if (xaet != null) {
				string[] et = xaet.InnerText.Split (separator);
				string prefix = element.GetPrefixOfNamespace (WSSecurity.NamespaceURI);
				XmlQualifiedName xqne = new XmlQualifiedName (et [1], WSSecurity.NamespaceURI);
				EncodingType = xqne;
				if ((et [0] == prefix) && (et [1] == name))
					RawData = Convert.FromBase64String (element.InnerText);
			}
		}
	}
}
