//
// SecurityInputFilter.cs: Security SOAP Input Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Xml;

namespace Microsoft.Web.Services.Security {

	public class SecurityInputFilter : SoapInputFilter {

		public SecurityInputFilter () {}

		[MonoTODO ("incomplete")]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			XmlNodeList xnl = envelope.Header.GetElementsByTagName (WSSecurity.ElementNames.Security, WSSecurity.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				XmlElement security = (XmlElement) xnl [0];
				envelope.Context.Security.LoadXml (security);

				// TODO - read ExtendedSecurity
			}
		}
	}
}
