//
// HttpChannelListener.cs
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
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal class HttpSimpleChannelListener<TChannel> : HttpChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		HttpListenerManager<TChannel> httpChannelManager;

		public HttpSimpleChannelListener (HttpTransportBindingElement source,
			BindingContext context)
			: base (source, context)
		{
		}

		public HttpListener Http {
			get {  return httpChannelManager.HttpListener; }
		}

		protected override TChannel CreateChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new HttpSimpleReplyChannel ((HttpSimpleChannelListener<IReplyChannel>) (object) this);

			// FIXME: implement more
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			base.OnOpen (timeout);
			StartListening (timeout);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			base.OnClose (timeout);
			httpChannelManager.Stop ();
		}

		void StartListening (TimeSpan timeout)
		{
			httpChannelManager = new HttpListenerManager<TChannel> (this);
			httpChannelManager.Open (timeout);
		}
	}

	internal class AspNetChannelListener<TChannel> : HttpChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		public AspNetChannelListener (HttpTransportBindingElement source,
			BindingContext context)
			: base (source, context)
		{
		}

		protected override TChannel CreateChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new AspNetReplyChannel ((AspNetChannelListener<IReplyChannel>) (object) this);

			// FIXME: implement more
			throw new NotImplementedException ();
		}
	}

	internal abstract class HttpChannelListenerBase<TChannel> : ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		HttpTransportBindingElement source;
		BindingContext context;
		Uri listen_uri;
		List<TChannel> channels = new List<TChannel> ();
		MessageEncoder encoder;

		public HttpChannelListenerBase (HttpTransportBindingElement source,
			BindingContext context)
			: base (context.Binding)
		{
			// FIXME: consider ListenUriMode
			// FIXME: there should be some way to post-provide Uri in case of null listenerUri in context.
			listen_uri = context.ListenUriBaseAddress != null ?
				new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress) : null;
			foreach (BindingElement be in context.RemainingBindingElements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					encoder = CreateEncoder<TChannel> (mbe);
					break;
				}
			}
			if (encoder == null)
				encoder = new TextMessageEncoder (MessageVersion.Default, Encoding.UTF8);
		}

		public MessageEncoder MessageEncoder {
			get { return encoder; }
		}

		public override Uri Uri {
			get { return listen_uri; }
		}

		protected IList<TChannel> Channels {
			get { return channels; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			TChannel ch = CreateChannel (timeout);
			Channels.Add (ch);
			return ch;
		}

		protected abstract TChannel CreateChannel (TimeSpan timeout);

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback,
			object asyncState)
		{
			throw new NotImplementedException ();
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("find out what to do here.")]
		protected override void OnAbort ()
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override void OnClose (TimeSpan timeout)
		{
			foreach (TChannel ch in Channels)
				ch.Close(timeout);
		}
	}
}
