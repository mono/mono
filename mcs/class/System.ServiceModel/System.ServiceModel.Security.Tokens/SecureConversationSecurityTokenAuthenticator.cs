//
// SecureConversationSecurityTokenAuthenticator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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

using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Security.Tokens
{
	class SecureConversationSecurityTokenAuthenticator : CommunicationSecurityTokenAuthenticator
	{
		SecurityTokenRequirement req;
		SecurityContextSecurityTokenAuthenticator sc_auth;
		SecurityContextSecurityTokenResolver sc_res;
		WsscAuthenticatorCommunicationObject comm;

		public SecureConversationSecurityTokenAuthenticator (
			SecurityTokenRequirement r,
			SecurityContextSecurityTokenAuthenticator scAuth,
			SecurityContextSecurityTokenResolver scResolver)
		{
			this.req = r;
			this.sc_auth = scAuth;
			this.sc_res = scResolver;
			comm = new WsscAuthenticatorCommunicationObject ();
		}

		public override AuthenticatorCommunicationObject Communication {
			get { return comm; }
		}

		[MonoTODO]
		protected override bool CanValidateTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}
	}

	class WsscAuthenticatorCommunicationObject : AuthenticatorCommunicationObject
	{
		WSTrustSecurityTokenServiceProxy proxy;

		protected internal override TimeSpan DefaultCloseTimeout {
			get { throw new NotImplementedException (); }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { throw new NotImplementedException (); }
		}

		public override Message ProcessNegotiation (Message request, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (State == CommunicationState.Opened)
				throw new InvalidOperationException ("Already opened.");

			EnsureProperties ();

			proxy = new WSTrustSecurityTokenServiceProxy (
				IssuerBinding, IssuerAddress);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (proxy != null)
				proxy.Close ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
