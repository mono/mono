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

		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			if ((envelope.Context.Security.Tokens.Count > 0) || (envelope.Context.Security.Elements.Count > 0)) {
				XmlElement security = envelope.Context.Security.GetXml (envelope);
				envelope.Header.AppendChild (security);
			}

			if (envelope.Context.ExtendedSecurity.Count > 0) {
				foreach (Security s in envelope.Context.ExtendedSecurity) {
					envelope.Header.AppendChild (s.GetXml (envelope));
				}
			}
		}
	}
}
