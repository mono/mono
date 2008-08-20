//
// X509SecurityTokenAuthenticator.cs
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
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace System.IdentityModel.Selectors
{
	public class X509SecurityTokenAuthenticator
		: SecurityTokenAuthenticator
	{
		bool map_to_windows, include_win_groups;
		X509CertificateValidator validator;

		public X509SecurityTokenAuthenticator ()
			: this (X509CertificateValidator.ChainTrust)
		{
		}

		public X509SecurityTokenAuthenticator (X509CertificateValidator validator)
			: this (validator, false)
		{
		}

		public X509SecurityTokenAuthenticator (X509CertificateValidator validator, bool mapToWindows)
			: this (validator, mapToWindows, false)
		{
		}

		public X509SecurityTokenAuthenticator (X509CertificateValidator validator, bool mapToWindows, bool includeWindowsGroups)
		{
			if (validator == null)
				throw new ArgumentNullException ("validator");
			this.validator = validator;
			map_to_windows = mapToWindows;
			include_win_groups = includeWindowsGroups;

			if (map_to_windows || include_win_groups)
				throw new NotSupportedException ("Why on earth do you expect that mapToWindows or includeWindowsGroups are supported here?");
		}

		protected override bool CanValidateTokenCore (SecurityToken token)
		{
			return token is X509SecurityToken;
		}

		protected override ReadOnlyCollection<IAuthorizationPolicy>
			ValidateTokenCore (SecurityToken token)
		{
			X509SecurityToken xt = token as X509SecurityToken;
			if (xt == null)
				throw new InvalidOperationException (String.Format ("Security token '{0}' cannot be validated by this security token authenticator.", xt));
			validator.Validate (xt.Certificate);
			IAuthorizationPolicy policy =
				new X509AuthorizationPolicy (xt.Certificate);
			return new ReadOnlyCollection<IAuthorizationPolicy> (new IAuthorizationPolicy [] {policy});
		}

		class X509AuthorizationPolicy : SystemIdentityAuthorizationPolicy
		{
			X509Certificate2 cert;

			public X509AuthorizationPolicy (X509Certificate2 cert)
				: base (new UniqueId ().ToString ())
			{
				this.cert = cert;
			}

			public override DateTime ExpirationTime {
				// FIXME: should it really be converted to UTC?
				get { return cert.NotAfter.ToUniversalTime (); }
			}

			public override ClaimSet CreateClaims ()
			{
				return new DefaultClaimSet (Claim.CreateX500DistinguishedNameClaim (cert.SubjectName));
			}

			public override IIdentity CreateIdentity ()
			{
				return new GenericIdentity (String.Concat (cert.SubjectName, "; ", cert.Thumbprint), "X509");
			}
		}
	}
}
