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

namespace Microsoft.Web.Services.Security {

	public class SecurityOutputFilter : SoapOutputFilter {

		public SecurityOutputFilter () {}

		[MonoTODO ("ExtendedSecurity")]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			if (envelope.Context.Security.Tokens.Count > 0) {
				XmlNode xn = envelope.CreateElement (WSSecurity.Prefix, WSSecurity.ElementNames.Security, WSSecurity.NamespaceURI);
				XmlAttribute xa = envelope.CreateAttribute (Soap.Prefix, Soap.AttributeNames.MustUnderstand, Soap.NamespaceURI);
				xn.Attributes.Append (xa);
				envelope.Header.AppendChild (xn);

				foreach (SecurityToken st in envelope.Context.Security.Tokens) {
					xn.AppendChild (st.GetXml (envelope));
				}
			}
		}
	}
}
