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
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal abstract class InternalReplyChannelBase : ReplyChannelBase
	{
		public InternalReplyChannelBase (ChannelListenerBase listener)
			: base (listener)
		{
			local_address = new EndpointAddress (listener.Uri);
		}

		EndpointAddress local_address;

		public override EndpointAddress LocalAddress {
			get { return local_address; }
		}
	}

	internal abstract class ReplyChannelBase : ChannelBase, IReplyChannel
	{
		public ReplyChannelBase (ChannelListenerBase listener)
			: base (listener)
		{
			this.listener = listener;
		}

		ChannelListenerBase listener;

		public ChannelListenerBase Listener {
			get { return listener; }
		}

		public abstract EndpointAddress LocalAddress { get; }

		public override T GetProperty<T> ()
		{
			if (typeof (T) == typeof (MessageVersion) && listener is IHasMessageEncoder)
				return (T) (object) ((IHasMessageEncoder) listener).MessageEncoder.MessageVersion;
			if (typeof (T) == typeof (IChannelListener))
				return (T) (object) listener;
			return base.GetProperty<T> ();
		}

		// FIXME: this is wrong. Implement all of them in each channel.
		protected override void OnAbort ()
		{
			OnClose (TimeSpan.Zero);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (CurrentAsyncThread != null)
				if (!CancelAsync (timeout))
					CurrentAsyncThread.Abort ();
		}

		public virtual bool CancelAsync (TimeSpan timeout)
		{
			// FIXME: It should wait for the actual completion.
			return CurrentAsyncResult == null;
			//return CurrentAsyncResult == null || CurrentAsyncResult.AsyncWaitHandle.WaitOne (timeout);
		}

		public virtual bool TryReceiveRequest ()
		{
			RequestContext dummy;
			return TryReceiveRequest (DefaultReceiveTimeout, out dummy);
		}

		public abstract bool TryReceiveRequest (TimeSpan timeout, out RequestContext context);

		delegate bool TryReceiveDelegate (TimeSpan timeout, out RequestContext context);
		TryReceiveDelegate try_recv_delegate;

		object async_result_lock = new object ();
		protected Thread CurrentAsyncThread { get; private set; }
		protected IAsyncResult CurrentAsyncResult { get; private set; }

		public virtual IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (CurrentAsyncResult != null)
				throw new InvalidOperationException ("Another async TryReceiveRequest operation is in progress");
			if (try_recv_delegate == null)
				try_recv_delegate = new TryReceiveDelegate (delegate (TimeSpan tout, out RequestContext ctx) {
					lock (async_result_lock) {
						if (CurrentAsyncResult != null)
							CurrentAsyncThread = Thread.CurrentThread;
					}
					try {
						return TryReceiveRequest (tout, out ctx);
					} finally {
						lock (async_result_lock) {
							CurrentAsyncResult = null;
							CurrentAsyncThread = null;
						}
					}
					});
			RequestContext dummy;
			IAsyncResult result;
			lock (async_result_lock) {
				result = CurrentAsyncResult = try_recv_delegate.BeginInvoke (timeout, out dummy, callback, state);
			}
			// Note that at this point CurrentAsyncResult can be null here if delegate has run to completion
			return result;
		}

		public virtual bool EndTryReceiveRequest (IAsyncResult result)
		{
			RequestContext dummy;
			return EndTryReceiveRequest (result, out dummy);
		}

		public virtual bool EndTryReceiveRequest (IAsyncResult result, out RequestContext context)
		{
			if (try_recv_delegate == null)
				throw new InvalidOperationException ("BeginTryReceiveRequest operation has not started");
			return try_recv_delegate.EndInvoke (out context, result);
		}

		public virtual bool WaitForRequest ()
		{
			return WaitForRequest (DefaultReceiveTimeout);
		}

		public abstract bool WaitForRequest (TimeSpan timeout);

		Func<TimeSpan,bool> wait_delegate;

		public virtual IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_delegate == null)
				wait_delegate = new Func<TimeSpan,bool> (WaitForRequest);
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
			return ReceiveRequest (DefaultReceiveTimeout);
		}

		public abstract RequestContext ReceiveRequest (TimeSpan timeout);

		public virtual IAsyncResult BeginReceiveRequest (AsyncCallback callback, object state)
		{
			return BeginReceiveRequest (DefaultReceiveTimeout, callback, state);
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

		Action<TimeSpan> open_delegate, close_delegate;

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
				throw new InvalidOperationException ("async open operation has not started");
			open_delegate.EndInvoke (result);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			if (close_delegate == null)
				throw new InvalidOperationException ("async close operation has not started");
			close_delegate.EndInvoke (result);
		}
	}
}
