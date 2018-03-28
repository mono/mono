//
// SpnEndpointIdentity.cs
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
#if !MOBILE && !XAMMAC_4_5
using System.IdentityModel.Claims;
#endif
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;


namespace System.ServiceModel
{
	public class SpnEndpointIdentity : EndpointIdentity
	{
#if !MOBILE && !XAMMAC_4_5
		public SpnEndpointIdentity (Claim identity)
		{
			Initialize (identity);
		}

		public SpnEndpointIdentity (string spnName)
			: this (Claim.CreateSpnClaim (spnName))
		{
		}
#else
		public SpnEndpointIdentity (string spnName)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public static TimeSpan SpnLookupTime {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
