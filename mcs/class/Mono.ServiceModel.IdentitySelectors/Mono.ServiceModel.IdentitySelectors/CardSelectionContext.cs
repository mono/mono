//
// CardSelectionContext.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors
{
	public class CardSelectionContext
	{
		EndpointAddress target;
		EndpointAddress issuer;
		Collection<ClaimTypeRequirement> requirements;
		Collection<XmlElement> additional_parameters;
		Uri policy_link;
		int policy_ver;

		public CardSelectionContext (
			EndpointAddress target,
			EndpointAddress issuer,
			Collection<ClaimTypeRequirement> requirements,
			Collection<XmlElement> additionalRequestParameters,
			Uri policyNoticeLink,
			int policyNoticeVersion)
		{
			this.target = target;
			this.issuer = issuer;
			this.requirements = requirements;
			additional_parameters = additionalRequestParameters;
			policy_link = policyNoticeLink;
			policy_ver = policyNoticeVersion;
		}
	}
}
