//
// Microsoft.Web.Services.Referral.ReferralException.cs
//
// Authors:
//	Daniel Kornhauser <dkor@alum.mit.edu>
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Ximian, Inc. 2003.
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Referral {
	
	[Serializable]
	public class ReferralException : SystemException {

		public ReferralException (string message) : base (message) {}

		public ReferralException (string message, Exception ex) : base (message, ex) {}
	}
}

