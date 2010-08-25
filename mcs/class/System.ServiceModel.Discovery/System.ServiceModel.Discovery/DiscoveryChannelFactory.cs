//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	internal class DiscoveryChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		public DiscoveryChannelFactory (DiscoveryClientBindingElement source, BindingContext context)
		{
			Source = source;
			this.inner = context.BuildInnerChannelFactory<TChannel> ();
		}

		IChannelFactory<TChannel> inner;
		internal DiscoveryClientBindingElement Source { get; private set; }
		internal IChannelFactory<TChannel> InnerFactory {
			get { return inner; }
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginOpen (timeout, callback, state);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginClose (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			inner.EndClose (result);
		}

		protected override TChannel OnCreateChannel (EndpointAddress address, Uri uri)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			if (address != DiscoveryClientBindingElement.DiscoveryEndpointAddress)
				throw new ArgumentException ("Only DiscoveryEndpointAddress is expected as the argument EndpointAddress");

			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new DiscoveryRequestChannel (AsFactory<IRequestChannel> (), address, uri);
			
			throw new NotSupportedException ();
		}

		DiscoveryChannelFactory<T> AsFactory<T> ()
		{
			return (DiscoveryChannelFactory<T>) (object) this;
		}
	}
}

