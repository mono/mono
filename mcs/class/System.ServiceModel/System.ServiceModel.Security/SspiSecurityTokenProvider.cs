//
// SspiSecurityTokenProvider.cs
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
using System.Net;
using System.Security.Principal;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

using System.ServiceModel.Security.Tokens;

// mhm, why is this class not in S.SM.S.Tokens??
namespace System.ServiceModel.Security
{
	// Anyways we won't support SSPI until it becomes open.
	public class SspiSecurityTokenProvider : SecurityTokenProvider
	{
		[MonoTODO]
		public SspiSecurityTokenProvider (NetworkCredential credential,
			bool extractGroupsForWindowsAccounts,
			bool allowUnauthenticatedCallers)
		{
			throw new NotImplementedException ();
		}

		public SspiSecurityTokenProvider (NetworkCredential credential, 
			bool allowNtlm, TokenImpersonationLevel impersonationLevel)
		{
			if (credential == null)
				throw new ArgumentNullException ("credential");
			this.credential = credential;
			allow_ntlm = allowNtlm;
			impersonation_level = impersonationLevel;
		}

		NetworkCredential credential;
		bool allow_ntlm;
		TokenImpersonationLevel impersonation_level;

		// SecurityTokenProvider

		[MonoTODO]
		protected override SecurityToken GetTokenCore (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
