//
// SecurityOutputFilter.cs: Security SOAP Output Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Xml;
#if WSE2
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Security {

	public class SecurityOutputFilter : SoapOutputFilter {

		public SecurityOutputFilter () {}

		[MonoTODO]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			if ((envelope.Context.Security.Tokens.Count > 0) || (envelope.Context.Security.Elements.Count > 0) || (envelope.Context.ExtendedSecurity.Count > 0)) {
				XmlNode xn = envelope.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Security, WSSecurity.NamespaceURI);
				XmlAttribute xa = envelope.CreateAttribute (Soap.Prefix, Soap.AttributeNames.MustUnderstand, Soap.NamespaceURI);
				xa.InnerText = "1";
				xn.Attributes.Append (xa);
				envelope.Header.AppendChild (xn);

				foreach (SecurityToken st in envelope.Context.Security.Tokens) {
					xn.AppendChild (st.GetXml (envelope));
				}

				foreach (ISecurityElement se in envelope.Context.Security.Elements) {
					if (se is EncryptedData) {
						EncryptedData ed = (se as EncryptedData);
						ed.Encrypt (envelope);
						xn.AppendChild (ed.EncryptedKey.GetXml (envelope));
					}
					// TODO - other elements
				}

				foreach (Security s in envelope.Context.ExtendedSecurity) {
					xn.AppendChild (s.GetXml (envelope));
				}
			}
		}
	}
}
