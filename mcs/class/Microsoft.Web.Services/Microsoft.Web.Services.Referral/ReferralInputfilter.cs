//
// ReferralInputFilter.cs: Referral SOAP Input Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Referral {

	public class ReferralInputFilter : SoapInputFilter {

		public ReferralInputFilter () {}

		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			if (envelope == null)
				throw new ArgumentNullException ("envelope");
			// TODO
		}
	}
}
