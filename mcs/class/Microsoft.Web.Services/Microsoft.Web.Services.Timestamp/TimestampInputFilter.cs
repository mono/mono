//
// TimestampInputFilter.cs: Timestamp SOAP Input Filter
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
using System.Xml;

namespace Microsoft.Web.Services.Timestamp {

	// Reference:
	// 1.	Inside the Web Services Enhancements Pipeline
	//	http://msdn.microsoft.com/library/en-us/dnwebsrv/html/insidewsepipe.asp

	public class TimestampInputFilter : SoapInputFilter {

		public TimestampInputFilter () {}

		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			if (envelope.Header != null) {
				XmlNodeList xnl = envelope.Header.GetElementsByTagName (WSTimestamp.ElementNames.Timestamp, WSTimestamp.NamespaceURI);
				if ((xnl != null) && (xnl.Count > 0)) {
					XmlElement xel = (XmlElement) xnl [0];
					envelope.Context.Timestamp.LoadXml (xel);
					if (envelope.Context.Timestamp.Expires < DateTime.UtcNow)
						throw new TimestampFault ("Message Expired", null);
					envelope.Header.RemoveChild (xel);
				}
			}
		}
	}
}
