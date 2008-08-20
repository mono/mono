//
// RsaSecurityTokenAuthenticator.cs
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
using System.Security.Cryptography;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public class RsaSecurityTokenAuthenticator
		: SecurityTokenAuthenticator
	{
		public RsaSecurityTokenAuthenticator ()
		{
		}

		protected override bool CanValidateTokenCore (SecurityToken token)
		{
			return token is RsaSecurityToken;
		}

		[MonoTODO ("hmm, what to validate?")]
		protected override ReadOnlyCollection<IAuthorizationPolicy>
			ValidateTokenCore (SecurityToken token)
		{
			RsaSecurityToken rt = token as RsaSecurityToken;
			if (rt == null)
				throw new InvalidOperationException ("Security token '{0}' cannot be validated by this security token authenticator.");

			IAuthorizationPolicy policy =
				new RsaAuthorizationPolicy (rt.Rsa);
			return new ReadOnlyCollection<IAuthorizationPolicy> (new IAuthorizationPolicy [] {policy});
		}

		class RsaAuthorizationPolicy : IAuthorizationPolicy
		{
			string id;
			RSA rsa;

			public RsaAuthorizationPolicy (RSA rsa)
			{
				id = new UniqueId ().ToString ();
			}

			public ClaimSet Issuer {
				get { return ClaimSet.System; }
			}

			public string Id {
				get { return id; }
			}

			public bool Evaluate (EvaluationContext ec, ref Object state)
			{
				ec.AddClaimSet (this, new DefaultClaimSet (Claim.CreateRsaClaim (rsa)));
				ec.RecordExpirationTime (DateTime.MaxValue.AddDays (-1));
				return true;
			}
		}
	}
}
