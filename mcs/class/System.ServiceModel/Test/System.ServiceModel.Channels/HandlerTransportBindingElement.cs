//
// HandlerTransportBindingElement.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace MonoTests.System.ServiceModel.Channels
{
	public delegate Message RequestSender (Message input);
	public delegate Message RequestReceiver ();
	public delegate void ReplyHandler (Message input);

	public class HandlerTransportBindingElement : TransportBindingElement
	{
		RequestSender sender;

		ReplyHandler reply_handler;
		RequestReceiver receiver;

		public HandlerTransportBindingElement (RequestSender sender)
		{
			this.sender = sender;
		}

		public HandlerTransportBindingElement (ReplyHandler handler, RequestReceiver receiver)
		{
			this.reply_handler = handler;
			this.receiver = receiver;
		}

		public RequestSender RequestSender {
			get { return sender; }
		}

		public ReplyHandler ReplyHandler {
			get { return reply_handler; }
		}

		public RequestReceiver RequestReceiver {
			get { return receiver; }
		}

		public override string Scheme {
			get { return "stream"; }
		}

		public override BindingElement Clone ()
		{
			if (sender != null)
				return new HandlerTransportBindingElement (sender);
			else
				return new HandlerTransportBindingElement (reply_handler, receiver);
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return typeof (TChannel) == typeof (IRequestChannel) ||
				typeof (TChannel) == typeof (IRequestChannel);
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return typeof (TChannel) == typeof (IReplyChannel) ||
				typeof (TChannel) == typeof (IInputChannel);
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			return new HandlerTransportChannelFactory<TChannel> (this);
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			// FIXME: pass uri
			return new HandlerTransportChannelListener<TChannel> (this, new Uri ("stream:dummy"));
		}
	}

	public class HandlerTransportChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		HandlerTransportBindingElement source;

		public HandlerTransportChannelFactory (HandlerTransportBindingElement source)
		{
			this.source = source;
		}

		public HandlerTransportBindingElement Source {
			get { return source; }
		}

		protected override TChannel OnCreateChannel (EndpointAddress address, Uri via)
		{
			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new HandlerTransportRequestChannel ((HandlerTransportChannelFactory<IRequestChannel>) (object) this, address, via);
			if (typeof (TChannel) == typeof (IOutputChannel))
				return (TChannel) (object) new HandlerTransportOutputChannel ((HandlerTransportChannelFactory<IOutputChannel>) (object) this, address, via);

			throw new NotSupportedException (String.Format ("Channel '{0}' is not supported.", typeof (TChannel)));
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotSupportedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// do nothing
		}
	}

	public class HandlerTransportOutputChannel : OutputChannelBase
	{
		HandlerTransportChannelFactory<IOutputChannel> source;
		EndpointAddress address;
		Uri via;

		public HandlerTransportOutputChannel (HandlerTransportChannelFactory<IOutputChannel> source, EndpointAddress address, Uri via)
			: base (source)
		{
			this.source = source;
			this.address = address;
			this.via = via;
		}

		public override EndpointAddress RemoteAddress {
			get { return address; }
		}

		public override Uri Via {
			get { return via; }
		}

		public override void Send (Message input, TimeSpan timeout)
		{
			source.Source.RequestSender (input);
		}

		class OutputAsyncResult : IAsyncResult
		{
			Message message;
			object state;
			bool completed = true;

			public OutputAsyncResult (Message message, object state)
			{
				this.message = message;
				this.state = state;
			}

			public Message Message {
				get { return message; }
			}

			public object AsyncState {
				get { return state; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return null; }
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool IsCompleted {
				get { return completed; }
				internal set { completed = value; }
			}
		}

		public override IAsyncResult BeginSend (Message input, TimeSpan timeout, AsyncCallback callback, object state)
		{
			// FIXME: timeout is not considered here.
			return new OutputAsyncResult (input, state);
		}

		public override void EndSend (IAsyncResult result)
		{
			source.Source.RequestSender (((OutputAsyncResult) result).Message);
		}


		protected override void OnAbort ()
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnOpen");
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginOpen");
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndOpen");
		}

		protected override void OnClose (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnClose");
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginClose");
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndClose");
		}
	}

	public class HandlerTransportRequestChannel : RequestChannelBase
	{
		HandlerTransportChannelFactory<IRequestChannel> source;
		EndpointAddress address;
		Uri via;

		public HandlerTransportRequestChannel (HandlerTransportChannelFactory<IRequestChannel> source, EndpointAddress address, Uri via)
			: base (source)
		{
			this.source = source;
			this.address = address;
			this.via = via;
		}

		public override EndpointAddress RemoteAddress {
			get { return address; }
		}

		public override Uri Via {
			get { return via; }
		}

		public override Message Request (Message input, TimeSpan timeout)
		{
			return source.Source.RequestSender (input);
		}

		class RequestAsyncResult : IAsyncResult
		{
			Message message;
			object state;
			bool completed = true;

			public RequestAsyncResult (Message message, object state)
			{
				this.message = message;
				this.state = state;
			}

			public Message Message {
				get { return message; }
			}

			public object AsyncState {
				get { return state; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return null; }
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool IsCompleted {
				get { return completed; }
				internal set { completed = value; }
			}
		}

		public override IAsyncResult BeginRequest (Message input, TimeSpan timeout, AsyncCallback callback, object state)
		{
			// FIXME: timeout is not considered here.
			return new RequestAsyncResult (input, state);
		}

		public override Message EndRequest (IAsyncResult result)
		{
			return source.Source.RequestSender (((RequestAsyncResult) result).Message);
		}


		protected override void OnAbort ()
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnOpen");
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginOpen");
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndOpen");
		}

		protected override void OnClose (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnClose");
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginClose");
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndClose");
		}
	}

	public class HandlerTransportChannelListener<TChannel>
		: ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		HandlerTransportBindingElement source;
		Uri uri;

		public HandlerTransportChannelListener (HandlerTransportBindingElement source, Uri uri)
		{
			this.source = source;
			this.uri = uri;
		}

		public HandlerTransportBindingElement Source {
			get { return source; }
		}


		public override Uri Uri {
			get { return uri; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IReplyChannel))
				return (TChannel) (object) new HandlerTransportReplyChannel ((HandlerTransportChannelListener<IReplyChannel>) (object) this);

			throw new NotSupportedException ();
		}

		protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginAcceptChannel");
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			throw new NotImplementedException ("EndAcceptChannel");
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			return true;
		}

		protected override IAsyncResult OnBeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginWaitForChannel");
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			throw new NotImplementedException ("EndWaitForChannel");
		}


		protected override void OnAbort ()
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnOpen");
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginOpen");
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ("EndOpen");
		}

		protected override void OnClose (TimeSpan timeout)
		{
			//throw new NotImplementedException ("Close");
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginClose");
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndClose");
		}
	}

	public class HandlerTransportReplyChannel : ReplyChannelBase
	{
		EndpointAddress address;
		HandlerTransportChannelListener<IReplyChannel> source;

		public HandlerTransportReplyChannel (HandlerTransportChannelListener<IReplyChannel> source)
			: base (source)
		{
			this.source = source;
			address = new EndpointAddress (source.Uri);
		}

		public HandlerTransportChannelListener<IReplyChannel> Source {
			get { return source; }
		}

		public override EndpointAddress LocalAddress {
			get { return address; }
		}

		class ReceiveRequestAsyncResult : IAsyncResult
		{
			object state;
			bool completed = true;

			public ReceiveRequestAsyncResult (object state)
			{
				this.state = state;
			}

			public object AsyncState {
				get { return state; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return null; }
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool IsCompleted {
				get { return completed; }
				internal set { completed = value; }
			}
		}

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			RequestContext ret;
			if (!TryReceiveRequest (timeout, out ret))
				throw new Exception ();
			return ret;
		}

		public override IAsyncResult BeginReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return new ReceiveRequestAsyncResult (state);
		}

		public override RequestContext EndReceiveRequest (IAsyncResult result)
		{
			//ReceiveRequestAsyncResult r =
			//	(ReceiveRequestAsyncResult) result;
			return new HandlerRequestContext (this);
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext ret)
		{
			ret = new HandlerRequestContext (this);
			return true;
		}

		public override IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			// hack, hack
			return new ReceiveRequestAsyncResult (state);
		}

		public override bool EndTryReceiveRequest (IAsyncResult result, out RequestContext ret)
		{
			// hack, hack
			//ReceiveRequestAsyncResult r =
			//	(ReceiveRequestAsyncResult) result;
			return TryReceiveRequest (TimeSpan.FromSeconds (5), out ret);
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("BeginWaitForRequest");
		}

		public override bool EndWaitForRequest (IAsyncResult result)
		{
			throw new NotImplementedException ("EndWaitForRequest");
		}


		protected override void OnAbort ()
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// throw new NotImplementedException ("OnOpen");
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginOpen");
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ("EndOpen");
		}

		protected override void OnClose (TimeSpan timeout)
		{
			//throw new NotImplementedException ("Close");
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("OnBeginClose");
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ("OnEndClose");
		}
	}

	public class HandlerRequestContext : RequestContext
	{
		HandlerTransportReplyChannel source;
		Message request_message;

		public HandlerRequestContext (HandlerTransportReplyChannel source)
		{
			this.source = source;
			if (source.Source.Source.RequestReceiver != null)
				request_message = source.Source.Source.RequestReceiver ();
		}


		public override void Abort ()
		{
		}

		public override void Close ()
		{
		}

		public override void Close (TimeSpan timeout)
		{
		}

		public override Message RequestMessage {
			get { return request_message; }
		}

		public override void Reply (Message msg)
		{
			source.Source.Source.ReplyHandler (msg);
		}

		public override void Reply (Message msg, TimeSpan timeout)
		{
			source.Source.Source.ReplyHandler (msg);
		}

		public override IAsyncResult BeginReply (Message msg, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("BeginReply");
		}

		public override IAsyncResult BeginReply (Message msg, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ("BeginReply");
		}

		public override void EndReply (IAsyncResult result)
		{
			throw new NotImplementedException ("EndReply");
		}
	}
}
#endif
