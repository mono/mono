//
// ReplyChannelBase.cs
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
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
	internal abstract class ReplyChannelBase : ChannelBase, IReplyChannel
	{
		ChannelListenerBase channel_listener;

		public ReplyChannelBase (ChannelListenerBase listener)
			: base (listener)
		{
			this.channel_listener = listener;
		}

		public abstract EndpointAddress LocalAddress { get; }

		public virtual bool TryReceiveRequest ()
		{
			RequestContext dummy;
			return TryReceiveRequest (channel_listener.DefaultReceiveTimeout, out dummy);
		}

		public abstract bool TryReceiveRequest (TimeSpan timeout, out RequestContext context);

		delegate bool TryReceiveDelegate (TimeSpan timeout, out RequestContext context);
		TryReceiveDelegate try_recv_delegate;

		public virtual IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (try_recv_delegate == null)
				try_recv_delegate = new TryReceiveDelegate (TryReceiveRequest);
			RequestContext dummy;
			return try_recv_delegate.BeginInvoke (timeout, out dummy, callback, state);
		}

		public virtual bool EndTryReceiveRequest (IAsyncResult result)
		{
			if (try_recv_delegate == null)
				throw new InvalidOperationException ("BeginTryReceiveRequest operation has not started");
			RequestContext dummy;
			return EndTryReceiveRequest (result, out dummy);
		}

		public virtual bool EndTryReceiveRequest (IAsyncResult result, out RequestContext context)
		{
			return try_recv_delegate.EndInvoke (out context, result);
		}

		public virtual bool WaitForRequest ()
		{
			return WaitForRequest (channel_listener.DefaultReceiveTimeout);
		}

		public abstract bool WaitForRequest (TimeSpan timeout);

		delegate bool WaitDelegate (TimeSpan timeout);
		WaitDelegate wait_delegate;

		public virtual IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_delegate == null)
				wait_delegate = new WaitDelegate (WaitForRequest);
			return wait_delegate.BeginInvoke (timeout, callback, state);
		}

		public virtual bool EndWaitForRequest (IAsyncResult result)
		{
			if (wait_delegate == null)
				throw new InvalidOperationException ("BeginWaitForRequest operation has not started");
			return wait_delegate.EndInvoke (result);
		}

		public virtual RequestContext ReceiveRequest ()
		{
			return ReceiveRequest (channel_listener.DefaultReceiveTimeout);
		}

		public abstract RequestContext ReceiveRequest (TimeSpan timeout);

		public virtual IAsyncResult BeginReceiveRequest (AsyncCallback callback, object state)
		{
			return BeginReceiveRequest (channel_listener.DefaultReceiveTimeout, callback, state);
		}

		Func<TimeSpan,RequestContext> recv_delegate;
		public virtual IAsyncResult BeginReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (recv_delegate == null)
				recv_delegate = new Func<TimeSpan,RequestContext> (ReceiveRequest);
			return recv_delegate.BeginInvoke (timeout, callback, state);
		}

		public virtual RequestContext EndReceiveRequest (IAsyncResult result)
		{
			if (recv_delegate == null)
				throw new InvalidOperationException ("BeginReceiveRequest operation has not started");
			return recv_delegate.EndInvoke (result);
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
	}
}
