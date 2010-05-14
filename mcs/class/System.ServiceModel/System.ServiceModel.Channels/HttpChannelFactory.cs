//
// HttpChannelFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal class HttpChannelFactory<TChannel> : TransportChannelFactoryBase<TChannel>
	{
#if NET_2_1
		IHttpCookieContainerManager cookie_manager;
#endif

		public HttpChannelFactory (HttpTransportBindingElement source, BindingContext ctx)
			: base (source, ctx)
		{
			ClientCredentials = ctx.BindingParameters.Find<ClientCredentials> ();
			foreach (BindingElement be in ctx.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					break;
				}
#if NET_2_1
				var cbe = be as HttpCookieContainerBindingElement;
				if (cbe != null)
					cookie_manager = cbe.GetProperty<IHttpCookieContainerManager> (ctx);
#endif
			}
			if (MessageEncoder == null)
				MessageEncoder = new TextMessageEncoder (MessageVersion.Default, Encoding.UTF8);
		}

		public ClientCredentials ClientCredentials { get; private set; }

		protected override TChannel OnCreateChannel (
			EndpointAddress address, Uri via)
		{
			ThrowIfDisposedOrNotOpen ();

			if (Transport.Scheme != address.Uri.Scheme)
				throw new ArgumentException (String.Format ("Argument EndpointAddress has unsupported URI scheme: {0}", address.Uri.Scheme));

			if (MessageEncoder.MessageVersion.Addressing.Equals (AddressingVersion.None) &&
			    via != null && !address.Uri.Equals (via))
				throw new ArgumentException (String.Format ("The endpoint address '{0}' and via uri '{1}' must match when the corresponding binding has addressing version in the message version value as None.", address.Uri, via));

			Type t = typeof (TChannel);
			if (t == typeof (IRequestChannel))
				return (TChannel) (object) new HttpRequestChannel ((HttpChannelFactory<IRequestChannel>) (object) this, address, via);
			else if (t == typeof (IOutputChannel))
				// FIXME: implement
				throw new NotImplementedException ();
			throw new InvalidOperationException (String.Format ("channel type {0} is not supported.", typeof (TChannel).Name));
		}

		Action<TimeSpan> open_delegate;

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			if (open_delegate == null)
				throw new InvalidOperationException ("Async open operation has not started");
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		public override T GetProperty<T> ()
		{
#if NET_2_1
			if (cookie_manager is T)
				return (T) (object) cookie_manager;
#endif
			return base.GetProperty<T> ();
		}
	}
}
