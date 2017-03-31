//
// KerberosSecurityTokenProvider.cs
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
using System.Net;
using System.Security.Principal;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Selectors
{
	public class KerberosSecurityTokenProvider : SecurityTokenProvider
	{
		public KerberosSecurityTokenProvider (string servicePrincipalName)
			: this (servicePrincipalName, TokenImpersonationLevel.Identification)
		{
		}

		[MonoTODO]
		public KerberosSecurityTokenProvider (string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel)
			: this (servicePrincipalName, tokenImpersonationLevel, CredentialCache.DefaultNetworkCredentials)
		{
		}

		[MonoTODO]
		public KerberosSecurityTokenProvider (string servicePrincipalName, TokenImpersonationLevel tokenImpersonationLevel, NetworkCredential networkCredential)
		{
			name = servicePrincipalName;
			impersonation_level = tokenImpersonationLevel;
			this.credential = networkCredential;
		}

		string name;
		TokenImpersonationLevel impersonation_level;
		NetworkCredential credential;

		public string ServicePrincipalName {
			get { return name; }
		}

		public TokenImpersonationLevel TokenImpersonationLevel {
			get { return impersonation_level; }
		}

		public NetworkCredential NetworkCredential {
			get { return credential; }
		}

		[MonoTODO]
		protected override SecurityToken GetTokenCore (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
