//
// ReferralOutputFilter.cs: Referral SOAP Output Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Referral {

	public class ReferralOutputFilter : SoapOutputFilter {

		public ReferralOutputFilter () {}

		[MonoTODO]
		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");
			// TODO
		}
	}
}
