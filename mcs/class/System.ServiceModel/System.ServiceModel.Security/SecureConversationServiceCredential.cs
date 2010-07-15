//
// SecureConversationServiceCredential.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Tokens;
using System.Web.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	public sealed class SecureConversationServiceCredential
	{
		Collection<Type> ctx_claim_types = new Collection<Type> (new Type [] {
			typeof (SamlAuthorizationDecisionClaimResource),
			typeof (SamlAuthenticationClaimResource),
			typeof (SamlAccessDecision),
			typeof (SamlAuthorityBinding),
			typeof (SamlNameIdentifierClaimResource)});
		SecurityStateEncoder encoder =
			new DataProtectionSecurityStateEncoder ();

		internal SecureConversationServiceCredential ()
		{
		}

		internal SecureConversationServiceCredential Clone ()
		{
			var ret = (SecureConversationServiceCredential) MemberwiseClone ();
			ret.ctx_claim_types = new Collection<Type> (ctx_claim_types);
			return ret;
		}

		public Collection<Type> SecurityContextClaimTypes {
			get { return ctx_claim_types; }
		}

		public SecurityStateEncoder SecurityStateEncoder {
			get { return encoder; }
			set { encoder = value; }
		}
	}
}
