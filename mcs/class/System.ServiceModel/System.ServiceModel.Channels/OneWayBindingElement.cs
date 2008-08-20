//
// OneWayBindingElement.cs
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
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Channels
{
	public sealed class OneWayBindingElement : BindingElement
	{
		public OneWayBindingElement ()
		{
			pool = new ChannelPoolSettings ();
		}

		OneWayBindingElement (OneWayBindingElement other)
		{
			pool = new ChannelPoolSettings (other.pool);
		}

		ChannelPoolSettings pool;

		public ChannelPoolSettings ChannelPoolSettings {
			get { return pool; }
		}

		[MonoTODO ("It generates just pass-thru factory")]
		public override IChannelFactory<TChannel>
			BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (typeof (TChannel) == typeof (IOutputSessionChannel) ||
			    typeof (TChannel) == typeof (IOutputChannel))
				return new OneWayChannelFactory<TChannel> (context.BuildInnerChannelFactory<TChannel> ());
			throw new ArgumentException (String.Format ("The requested channel type '{0}' is not supported by this binding element", typeof (TChannel)));
		}

		[MonoTODO ("It generates just pass-thru listener")]
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			if (typeof (TChannel) == typeof (IInputSessionChannel) ||
			    typeof (TChannel) == typeof (IInputChannel))
				return new OneWayChannelListener<TChannel> (context.BuildInnerChannelListener<TChannel> ());
			throw new ArgumentException (String.Format ("The requested channel type '{0}' is not supported by this binding element", typeof (TChannel)));
		}

		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return typeof (TChannel) == typeof (IOutputSessionChannel) ||
				typeof (TChannel) == typeof (IOutputChannel);
		}

		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			return typeof (TChannel) == typeof (IInputSessionChannel) ||
				typeof (TChannel) == typeof (IInputChannel);
		}

		public override BindingElement Clone ()
		{
			return new OneWayBindingElement (this);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}

	class OneWayChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		IChannelFactory<TChannel> inner;

		public OneWayChannelFactory (IChannelFactory<TChannel> inner)
		{
			this.inner = inner;
		}

		protected override TChannel OnCreateChannel (EndpointAddress address, Uri via)
		{
			return inner.CreateChannel (address, via);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			inner.EndClose (result);
		}
	}

	class OneWayChannelListener<TChannel> : ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		IChannelListener<TChannel> inner;

		public OneWayChannelListener (IChannelListener<TChannel> inner)
		{
			this.inner = inner;
		}

		public override Uri Uri {
			get { return inner.Uri; }
		}

		protected override void OnAbort ()
		{
			inner.Abort ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			inner.EndClose (result);
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

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			return inner.AcceptChannel (timeout);
		}

		protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginAcceptChannel (timeout, callback, state);
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			return inner.EndAcceptChannel (result);
		}
	}
}
