//
// IdentityVerifier.cs
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
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.ServiceModel;

namespace System.ServiceModel.Security
{
	[MonoTODO]
	public abstract class IdentityVerifier
	{
		[MonoTODO]
		public static IdentityVerifier CreateDefault ()
		{
			return new DefaultIdentityVerifier ();
		}

		protected IdentityVerifier ()
		{
		}

		public abstract bool CheckAccess (EndpointIdentity identity,
			AuthorizationContext authContext);

		public abstract bool TryGetIdentity (EndpointAddress reference,
			out EndpointIdentity identity);

		class DefaultIdentityVerifier : IdentityVerifier
		{
			public override bool CheckAccess (
				EndpointIdentity identity,
				AuthorizationContext authContext)
			{
				// FIXME: implement
				throw new NotImplementedException ();
			}

			public override bool TryGetIdentity (
				EndpointAddress reference,
				out EndpointIdentity identity)
			{
				// FIXME: implement
				throw new NotImplementedException ();
			}
		}

	}
}
