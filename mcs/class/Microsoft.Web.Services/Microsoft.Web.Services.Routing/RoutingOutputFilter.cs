//
// SecurityOutputFilter.cs: Security SOAP Output Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Routing {

	public class RoutingOutputFilter : SoapOutputFilter {

		public RoutingOutputFilter () {}

		[MonoTODO]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");
			// TODO
#if WSE2
//Quick quick quick Hack for some Addressing stuff
			if(envelope.Context.To != null) {	
				envelope.CreateHeader ().AppendChild (envelope.Context.To.GetXml (envelope));
			}
			if(envelope.Context.Action != null) {
				envelope.CreateHeader ().AppendChild (envelope.Context.Action.GetXml (envelope));
			}
		
#endif
		}
	}
}
