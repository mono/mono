//
// SspiSecurityToken.cs
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
using System.Collections.ObjectModel;
using System.Net;
using System.Xml;
using System.Security.Principal;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security.Tokens
{
	public class SspiSecurityToken : SecurityToken
	{
		string id;
		DateTime valid_from = DateTime.Now.ToUniversalTime ();
		DateTime valid_to;
		ReadOnlyCollection<SecurityKey> keys;
		bool extract_groups, allow_unauth;

		[MonoTODO]
		public SspiSecurityToken (NetworkCredential networkCredential,
			bool extractGroupsForWindowsAccounts,
			bool allowUnauthenticatedCallers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SspiSecurityToken (
			TokenImpersonationLevel impersonationLevel,
			bool allowNtlm, NetworkCredential networkCredential)
		{
			throw new NotImplementedException ();
		}

		public override DateTime ValidFrom {
			get { return valid_from; }
		}

		[MonoTODO]
		public override DateTime ValidTo {
			get { return valid_to; }
		}

		[MonoTODO]
		public override string Id {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { throw new NotImplementedException (); }
		}
	}
}
