//
// RoutingInputFilter.cs: Routing SOAP Input Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Routing {

	public class RoutingInputFilter : SoapInputFilter {

		public RoutingInputFilter () {}

		[MonoTODO]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");
			// TODO
		}
	}
}
