//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Threading;

namespace System.ServiceModel.Discovery.Udp
{
	internal class UdpChannelListener : ChannelListenerBase<IDuplexChannel>
	{
		public UdpChannelListener (UdpTransportBindingElement source, BindingContext context)
		{
			Source = source;
			Context = context;
			listen_uri = context.ListenUriRelativeAddress != null ?
				new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress) :
				context.ListenUriBaseAddress;
		}
		
		Uri listen_uri;
		UdpDuplexChannel channel;
		ManualResetEvent accept_wait_handle = new ManualResetEvent (true);
		
		public override Uri Uri {
			get { return listen_uri; }
		}
		
		protected override void OnOpen (TimeSpan timeout)
		{
		}
		
		protected override void OnClose (TimeSpan timeout)
		{
			if (channel != null)
				channel.Close (timeout);
		}
		
		protected override void OnAbort ()
		{
			if (channel != null)
				channel.Abort ();
		}
		
		Action<TimeSpan> open_delegate, close_delegate;
		
		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override void OnEndOpen (IAsyncResult result)
		{
			open_delegate.EndInvoke (result);
		}
		
		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override void OnEndClose (IAsyncResult result)
		{
			close_delegate.EndInvoke (result);
		}

		protected override IDuplexChannel OnAcceptChannel (TimeSpan timeout)
		{
			if (!accept_wait_handle.WaitOne (timeout))
				throw new TimeoutException ();
			accept_wait_handle.Reset ();
			if (State != CommunicationState.Opened)
				return null; // happens during Close() or Abort().
			channel = new UdpDuplexChannel (this);
			channel.Closed += delegate {
				accept_wait_handle.Set ();
			};
			return channel;
		}
		
		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		Func<TimeSpan,IDuplexChannel> accept_delegate;
		
		protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (accept_delegate == null)
				accept_delegate = new Func<TimeSpan,IDuplexChannel> (OnAcceptChannel);
			return accept_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override IDuplexChannel OnEndAcceptChannel (IAsyncResult result)
		{
			return accept_delegate.EndInvoke (result);
		}
		
		Func<TimeSpan,bool> wait_for_channel_delegate;
		
		protected override IAsyncResult OnBeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_for_channel_delegate == null)
				wait_for_channel_delegate = new Func<TimeSpan,bool> (OnWaitForChannel);
			return wait_for_channel_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			return wait_for_channel_delegate.EndInvoke (result);
		}

		public UdpTransportBindingElement Source { get; private set; }
		public BindingContext Context { get; private set; }
	}
}
