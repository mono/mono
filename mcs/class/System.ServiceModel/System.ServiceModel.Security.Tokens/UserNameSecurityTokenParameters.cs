//
// UserNameSecurityTokenParameters.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#endif
using System.ServiceModel.Security;

namespace System.ServiceModel.Security.Tokens
{
	public class UserNameSecurityTokenParameters : SecurityTokenParameters
	{
		public UserNameSecurityTokenParameters ()
		{
			RequireDerivedKeys = false;
		}

		protected UserNameSecurityTokenParameters (UserNameSecurityTokenParameters other)
			: base (other)
		{
		}

		protected override bool HasAsymmetricKey {
			get { return false; }
		}

		protected override bool SupportsClientAuthentication {
			get { return true; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return true; }
		}

		protected override bool SupportsServerAuthentication {
			get { return false; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new UserNameSecurityTokenParameters (this);
		}

#if !MOBILE && !XAMMAC_4_5
		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			if (token == null)
				throw new ArgumentNullException ("token");
			if (token is UserNameSecurityToken &&
			    referenceStyle == SecurityTokenReferenceStyle.Internal)
				return new LocalIdKeyIdentifierClause (token.Id);
			// External reference is not supported.
			throw new NotSupportedException (String.Format ("This security token '{0}' with {1} reference mode is not supported.", token, referenceStyle));
		}

		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			requirement.TokenType = SecurityTokenTypes.UserName;
			requirement.RequireCryptographicToken = true;
		}
#endif
	}
}
