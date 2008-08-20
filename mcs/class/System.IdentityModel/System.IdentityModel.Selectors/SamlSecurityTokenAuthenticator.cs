//
// SamlSecurityTokenAuthenticator.cs
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Security.Principal;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public class SamlSecurityTokenAuthenticator
		: SecurityTokenAuthenticator
	{
		IList<SecurityTokenAuthenticator> authenticators;
		TimeSpan max_clock_skew;

		public SamlSecurityTokenAuthenticator (
			IList<SecurityTokenAuthenticator> supportingAuthenticators)
			: this (supportingAuthenticators, TimeSpan.MaxValue)
		{
		}

		public SamlSecurityTokenAuthenticator (
			IList<SecurityTokenAuthenticator> supportingAuthenticators,
			TimeSpan maxClockSkew)
		{
			if (supportingAuthenticators == null)
				throw new ArgumentNullException ("supportingAuthenticators");
			authenticators = supportingAuthenticators;
			max_clock_skew = maxClockSkew;
		}

		protected override bool CanValidateTokenCore (SecurityToken token)
		{
			return token is SamlSecurityToken;
		}

		[MonoTODO]
		protected override ReadOnlyCollection<IAuthorizationPolicy>
			ValidateTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ClaimSet ResolveClaimSet (SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		public virtual ClaimSet ResolveClaimSet (SecurityToken token)
		{
			return ResolveClaimSet (new SecurityKeyIdentifier (
				token.CreateKeyIdentifierClause<SamlAssertionKeyIdentifierClause> ()));
		}

		[MonoTODO]
		public virtual IIdentity ResolveIdentity (SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		public virtual IIdentity ResolveIdentity (SecurityToken token)
		{
			return ResolveIdentity (new SecurityKeyIdentifier (
				token.CreateKeyIdentifierClause<SamlAssertionKeyIdentifierClause> ()));
		}

		class SamlAuthorizationPolicy : SystemIdentityAuthorizationPolicy
		{
			SamlSecurityTokenAuthenticator authenticator;
			SamlSecurityToken token;

			public SamlAuthorizationPolicy (SamlSecurityTokenAuthenticator authenticator, SamlSecurityToken token)
				: base (new UniqueId ().ToString ())
			{
				this.authenticator = authenticator;
				this.token = token;
			}

			public override DateTime ExpirationTime {
				get { return token.ValidTo; }
			}

			public override ClaimSet CreateClaims ()
			{
				return authenticator.ResolveClaimSet (token);
			}

			public override IIdentity CreateIdentity ()
			{
				return authenticator.ResolveIdentity (token);
			}
		}
	}
}
