//
// Microsoft.Web.Services.PolicyEnforcementException.cs
//
// Author Daniel Kornhauser dkor@alum.mit.edu
//
//  (C) Copyright, Ximian, Inc. 2003.
//

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Policy {

	[Serializable]
	public class PolicyEnforcementException : SoapException
	{
		public PolicyEnforcementException (string message)
                        : base (message, XmlQualifiedName.Empty)
		{
		}
	}
}

