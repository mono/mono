//
// InterceptorBindingElement.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Security.Cryptography.Xml;
using System.Threading;
using NUnit.Framework;

using MonoTests.System.ServiceModel.Channels;

namespace MonoTests.System.ServiceModel.Channels
{
	public delegate void InterceptorRequestContextHandler (MessageBuffer src);

	class InterceptorBindingElement : BindingElement
	{
		InterceptorRequestContextHandler handler;

		public InterceptorBindingElement (InterceptorRequestContextHandler handler)
		{
			this.handler = handler;
		}

		public InterceptorRequestContextHandler Handler {
			get { return handler; }
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return true;
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return true;
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			return new InterceptorChannelListener<TChannel> (this, context);
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			return new InterceptorChannelFactory<TChannel> (this, context);
		}

		public override T GetProperty<T> (BindingContext ctx)
		{
			return ctx.GetInnerProperty<T> ();
		}

		public override BindingElement Clone ()
		{
			return this;
		}
	}

	abstract class DefaultTimeoutCommunicationObject : CommunicationObject
	{
		IDefaultCommunicationTimeouts timeouts;
		public DefaultTimeoutCommunicationObject (IDefaultCommunicationTimeouts timeouts)
		{
			this.timeouts = timeouts;
		}

		protected override TimeSpan DefaultOpenTimeout {
			get { return timeouts.OpenTimeout; }
		}
		protected override TimeSpan DefaultCloseTimeout {
			get { return timeouts.CloseTimeout; }
		}
	}

	class InterceptorChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		InterceptorBindingElement element;
		IChannelFactory<TChannel> inner;

		public InterceptorChannelFactory (InterceptorBindingElement element, BindingContext context)
			: base (null)
		{
			this.element = element;
			inner = context.BuildInnerChannelFactory<TChannel> ();
		}

		public InterceptorBindingElement Element {
			get { return element; }
		}

		public override T GetProperty<T> ()
		{
			return inner.GetProperty<T> ();
		}

		protected override TChannel OnCreateChannel (EndpointAddress address, Uri uri)
		{
			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new InterceptorRequestChannel (
					(InterceptorChannelFactory<IRequestChannel>) (object) this,
					(IRequestChannel) (object) inner.CreateChannel (address, uri));
			throw new NotImplementedException ();
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

		protected override void OnAbort ()
		{
			inner.Abort ();
		}
	}

	class InterceptorChannelListener<TChannel> : ChannelListenerBase<TChannel> where TChannel : class, IChannel
	{
		InterceptorBindingElement element;
		IChannelListener<TChannel> inner;

		public InterceptorChannelListener (InterceptorBindingElement element, BindingContext context)
		{
			this.element = element;
			inner = context.BuildInnerChannelListener<TChannel> ();
		}

		public InterceptorBindingElement Element {
			get { return element; }
		}

		public override Uri Uri {
			get { return inner.Uri; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new InterceptorReplyChannel (
					(InterceptorChannelListener<IReplyChannel>) (object) this,
					(IReplyChannel) (object) inner.AcceptChannel (timeout));
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginAcceptChannel (timeout, callback, state);
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new InterceptorReplyChannel (
					(InterceptorChannelListener<IReplyChannel>) (object) this,
					(IReplyChannel) (object) inner.EndAcceptChannel (result));
			throw new NotImplementedException ();
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
	}

	class InterceptorRequestChannel : ChannelBase, IRequestChannel
	{
		InterceptorChannelFactory<IRequestChannel> source;
		IRequestChannel inner;

		public InterceptorRequestChannel (InterceptorChannelFactory<IRequestChannel> source, IRequestChannel inner)
			: base (source)
		{
			this.source = source;
			this.inner = inner;
		}

		public EndpointAddress RemoteAddress {
			get { return inner.RemoteAddress; }
		}

		public Uri Via {
			get { return inner.Via; }
		}

		public Message Request (Message message)
		{
			return inner.Request (message);
		}
		public Message Request (Message message, TimeSpan timeout)
		{
			return inner.Request (message, timeout);
		}
		public IAsyncResult BeginRequest (Message message, AsyncCallback callback, object state)
		{
			return inner.BeginRequest (message, callback, state);
		}
		public IAsyncResult BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginRequest (message, timeout, callback, state);
		}
		public Message EndRequest (IAsyncResult result)
		{
			return inner.EndRequest (result);
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
	}

	class InterceptorReplyChannel : ReplyChannelBase
	{
		InterceptorChannelListener<IReplyChannel> source;
		IReplyChannel inner;

		public InterceptorReplyChannel (InterceptorChannelListener<IReplyChannel> source, IReplyChannel inner)
			: base (source)
		{
			this.source = source;
			this.inner = inner;
		}

		public override EndpointAddress LocalAddress {
			get { return inner.LocalAddress; }
		}

		RequestContext Inspect (RequestContext src)
		{
			CopyRequestContext ret = new CopyRequestContext (src);
			source.Element.Handler (ret.Buffer);
			return ret;
		}

		public override RequestContext ReceiveRequest ()
		{
			return Inspect (inner.ReceiveRequest ());
		}

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			return Inspect (inner.ReceiveRequest (timeout));
		}

		public override IAsyncResult BeginReceiveRequest (AsyncCallback callback, object state)
		{
			return inner.BeginReceiveRequest (callback, state);
		}

		public override IAsyncResult BeginReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginReceiveRequest (timeout, callback, state);
		}

		public override RequestContext EndReceiveRequest (IAsyncResult result)
		{
			return Inspect (inner.EndReceiveRequest (result));
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			if (inner.TryReceiveRequest (timeout, out context)) {
				context = Inspect (context);
				return true;
			}
			context = null;
			return false;
		}

		public override IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginTryReceiveRequest (timeout, callback, state);
		}

		public override bool EndTryReceiveRequest (IAsyncResult result, out RequestContext context)
		{
			if (inner.EndTryReceiveRequest (result, out context)) {
				context = Inspect (context);
				return true;
			}
			context = null;
			return false;
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			return inner.WaitForRequest (timeout);
		}

		public override IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginWaitForRequest (timeout, callback, state);
		}

		public override bool EndWaitForRequest (IAsyncResult result)
		{
			return inner.EndWaitForRequest (result);
		}

		protected override void OnAbort ()
		{
			inner.Abort ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}
	}

	class CopyRequestContext : RequestContext
	{
		RequestContext src;
		MessageBuffer buffer;
		Message copy_msg;

		public CopyRequestContext (RequestContext src)
		{
			this.src = src;
			buffer = src.RequestMessage.CreateBufferedCopy (0x10000);
			copy_msg = buffer.CreateMessage ();
		}

		public MessageBuffer Buffer {
			get { return buffer; }
		}

		public override void Abort ()
		{
			src.Abort ();
		}

		public override void Close ()
		{
			src.Close ();
		}

		public override void Close (TimeSpan timeout)
		{
			src.Close (timeout);
		}

		public override IAsyncResult BeginReply (Message message, AsyncCallback callback, object state)
		{
			return src.BeginReply (message, callback, state);
		}

		public override IAsyncResult BeginReply (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return src.BeginReply (message, timeout, callback, state);
		}

		public override void EndReply (IAsyncResult result)
		{
			src.EndReply (result);
		}

		public override void Reply (Message message)
		{
			src.Reply (message);
		}

		public override void Reply (Message message, TimeSpan timeout)
		{
			src.Reply (message, timeout);
		}

		public override Message RequestMessage {
			get { return copy_msg; }
		}
	}
}
#endif
