// Microsfot.Web.Services.ReferralException.cs
//
// Microsoft.Web.Services.Referral.ReferralException.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) Ximian, Inc. 2003.
//

// TODO: Figure out what the Uri parameter does.

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Referral {
	
	[Serializable]
	public class ReferralFormatException : SoapHeaderException
	{
                Uri reference;
                
		public ReferralFormatException (string message)
                        : base (message, XmlQualifiedName.Empty)
		{
		}

		public ReferralFormatException (Uri refid, string message)
                        : base (message, XmlQualifiedName.Empty)
		{
                        reference = refid;
		}

		public ReferralFormatException (Uri refid, string message, Exception innerException)
                        : base (message, XmlQualifiedName.Empty, innerException)
		{
                        reference = refid;
                }
	}
}

