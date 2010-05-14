//
// ChannelListenerBase.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal abstract class InternalChannelListenerBase<TChannel>
		: ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		protected InternalChannelListenerBase (BindingContext context)
			: base (context.Binding)
		{
			// for ListenUriMode(.Unique), the URI uniqueness
			// should be assured by the derived types.
			listen_uri =
				context.ListenUriRelativeAddress != null ?
				new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress) :
				context.ListenUriBaseAddress;
		}

		Uri listen_uri;
		Func<TimeSpan,TChannel> accept_channel_delegate;
		Func<TimeSpan,bool> wait_delegate;
		Action<TimeSpan> open_delegate, close_delegate;

		public MessageEncoder MessageEncoder { get; internal set; }

		public override Uri Uri {
			get { return listen_uri; }
		}

		protected Thread CurrentAsyncThread { get; private set; }
		protected IAsyncResult CurrentAsyncResult { get; private set; }

		protected override void OnAbort ()
		{
			if (CurrentAsyncThread != null)
				CurrentAsyncThread.Abort (); // it is not beautiful but there is no other way to stop it.
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (CurrentAsyncThread != null)
				if (!CancelAsync (timeout))
					if (CurrentAsyncThread != null) // being careful
						CurrentAsyncThread.Abort (); // it is not beautiful but there is no other way to stop it.
		}

		// cancel ongoing async operations and return if it was 
		// completed successfully. If not, it will abort.
		public virtual bool CancelAsync (TimeSpan timeout)
		{
			return CurrentAsyncResult == null || CurrentAsyncResult.AsyncWaitHandle.WaitOne (timeout);
		}

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback,
			object asyncState)
		{
			//if (CurrentAsyncResult != null)
			//	throw new InvalidOperationException ("Another AcceptChannel operation is in progress");

			ManualResetEvent wait = new ManualResetEvent (false);

			if (accept_channel_delegate == null)
				accept_channel_delegate = new Func<TimeSpan,TChannel> (delegate (TimeSpan tout) {
					wait.WaitOne (); // make sure that CurrentAsyncResult is set.
					CurrentAsyncThread = Thread.CurrentThread;

					try {
						return OnAcceptChannel (tout);
					} finally {
						CurrentAsyncThread = null;
						CurrentAsyncResult = null;
					}
				});

			CurrentAsyncResult = accept_channel_delegate.BeginInvoke (timeout, callback, asyncState);
			wait.Set ();
			return CurrentAsyncResult;
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			if (accept_channel_delegate == null)
				throw new InvalidOperationException ("Async AcceptChannel operation has not started");
			// FIXME: what's wrong with this?
			//if (CurrentAsyncResult == null)
			//	throw new InvalidOperationException ("Async AcceptChannel operation has not started. Argument result was: " + result);
			return accept_channel_delegate.EndInvoke (result);
		}

		protected override IAsyncResult OnBeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_delegate == null)
				wait_delegate = new Func<TimeSpan,bool> (OnWaitForChannel);
			return wait_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			if (wait_delegate == null)
				throw new InvalidOperationException ("Async WaitForChannel operation has not started");
			return wait_delegate.EndInvoke (result);
		}

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
				throw new InvalidOperationException ("Async Open operation has not started");
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
				throw new InvalidOperationException ("Async Close operation has not started");
			close_delegate.EndInvoke (result);
		}
	}

	public abstract class ChannelListenerBase<TChannel>
		: ChannelListenerBase, IChannelListener<TChannel>, 
		IChannelListener,  ICommunicationObject
		where TChannel : class, IChannel
	{
		IDefaultCommunicationTimeouts timeouts;

		protected ChannelListenerBase ()
			: this (DefaultCommunicationTimeouts.Instance)
		{
		}

		protected ChannelListenerBase (
			IDefaultCommunicationTimeouts timeouts)
		{
			if (timeouts == null)
				throw new ArgumentNullException ("timeouts");
			this.timeouts = timeouts;
		}

		public TChannel AcceptChannel ()
		{
			return AcceptChannel (timeouts.ReceiveTimeout);
		}

		public TChannel AcceptChannel (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();
			return OnAcceptChannel (timeout);
		}

		public IAsyncResult BeginAcceptChannel (
			AsyncCallback callback, object asyncState)
		{
			return BeginAcceptChannel (
				timeouts.ReceiveTimeout, callback, asyncState);
		}

		public IAsyncResult BeginAcceptChannel (TimeSpan timeout,
			AsyncCallback callback, object asyncState)
		{
			return OnBeginAcceptChannel (timeout, callback, asyncState);
		}

		public TChannel EndAcceptChannel (IAsyncResult result)
		{
			return OnEndAcceptChannel (result);
		}

		protected abstract TChannel OnAcceptChannel (TimeSpan timeout);

		protected abstract IAsyncResult OnBeginAcceptChannel (TimeSpan timeout,
			AsyncCallback callback, object asyncState);

		protected abstract TChannel OnEndAcceptChannel (IAsyncResult result);
	}
}
