//
// TimestampOutputFilter.cs: Timestamp SOAP Output Filter
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

	public class TimestampOutputFilter : SoapOutputFilter {

		public TimestampOutputFilter () {}

		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");

			// internal method
			envelope.Context.Timestamp.SetTimestamp (DateTime.UtcNow);
			XmlElement xel = envelope.Context.Timestamp.GetXml (envelope);
			if (envelope.Header == null) {
				XmlElement header = envelope.CreateHeader ();
				envelope.Envelope.PrependChild (xel);
			}
			envelope.Header.AppendChild (xel);
		}

	}
}
