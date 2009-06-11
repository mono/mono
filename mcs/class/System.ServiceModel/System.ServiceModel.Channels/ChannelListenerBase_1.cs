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

namespace System.ServiceModel.Channels
{
	internal abstract class InternalChannelListenerBase<TChannel>
		: ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		protected InternalChannelListenerBase ()
			: base ()
		{
		}

		protected InternalChannelListenerBase (IDefaultCommunicationTimeouts timeouts)
			: base (timeouts)
		{
		}

		Func<TimeSpan,TChannel> accept_channel_delegate;
		Func<TimeSpan,bool> wait_delegate;
		Action<TimeSpan> open_delegate, close_delegate;

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback,
			object asyncState)
		{
			if (accept_channel_delegate == null)
				accept_channel_delegate = new Func<TimeSpan,TChannel> (OnAcceptChannel);
			return accept_channel_delegate.BeginInvoke (timeout, callback, asyncState);
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			if (accept_channel_delegate == null)
				throw new InvalidOperationException ("Async AcceptChannel operation has not started");
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
