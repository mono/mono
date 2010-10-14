//
// SecurityChannelListener.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels.Security
{
	internal class SecurityChannelListener<TChannel> : ChannelListenerBase<TChannel>
		  where TChannel : class, IChannel
	{
		IChannelListener<TChannel> inner;
		RecipientMessageSecurityBindingSupport security;

		public SecurityChannelListener (
			IChannelListener<TChannel> innerListener, 
			RecipientMessageSecurityBindingSupport security)
		{
			inner = innerListener;
			this.security = security;
		}

		public RecipientMessageSecurityBindingSupport SecuritySupport {
			get { return security; }
		}

		public override T GetProperty<T> ()
		{
			if (typeof (T) == typeof (MessageSecurityBindingSupport))
				return (T) (object) security;
			return base.GetProperty<T> ();
		}

		TChannel CreateSecurityWrapper (TChannel src)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new SecurityReplyChannel ((SecurityChannelListener<IReplyChannel>) (object) this, (IReplyChannel) (object) src);
			throw new NotImplementedException ();
		}

		void AcquireTokens ()
		{
			security.Prepare (this, Uri);
		}

		void ReleaseTokens ()
		{
			security.Release ();
		}

		// ChannelListenerBase

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			return CreateSecurityWrapper (inner.AcceptChannel (timeout));
		}

		protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginAcceptChannel (timeout, callback, state);
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			return CreateSecurityWrapper (inner.EndAcceptChannel (result));
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			return inner.WaitForChannel (timeout);
		}

		protected override IAsyncResult OnBeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginWaitForChannel (timeout, callback, state);
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			return inner.EndWaitForChannel (result);
		}

		public override Uri Uri {
			get { return inner.Uri; }
		}

		// CommunicationObject
		protected override void OnAbort ()
		{
			inner.Abort ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			ReleaseTokens ();
			inner.Close (timeout);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			ReleaseTokens ();
			return inner.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			inner.EndClose (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			AcquireTokens ();
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			AcquireTokens ();
			return inner.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}
	}
}
