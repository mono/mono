// 
// DuplexSessionChannelBase.cs
// 
// Author:
//     Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels;

namespace System.ServiceModel.Channels
{
	internal abstract class DuplexChannelBase : ChannelBase, IDuplexChannel
	{
		ChannelFactoryBase channel_factory_base;
		ChannelListenerBase channel_listener_base;
		EndpointAddress local_address;
		EndpointAddress remote_address;
		Uri via;
		
		public DuplexChannelBase (ChannelFactoryBase factory, EndpointAddress remoteAddress, Uri via) : base (factory)
		{
			channel_factory_base = factory;
			remote_address = remoteAddress;
			this.via = via;
			SetupDelegates ();
		}
		
		public DuplexChannelBase (ChannelListenerBase listener) : base (listener)
		{
			channel_listener_base = listener;
			SetupDelegates ();
		}

		public abstract EndpointAddress LocalAddress { get; }
		
		public EndpointAddress RemoteAddress {
			get { return remote_address; }
		}

		public Uri Via {
			get { return via; }
		}

		void SetupDelegates ()
		{
			send_handler = new AsyncSendHandler (Send);
			receive_handler = new AsyncReceiveHandler (Receive);
			wait_handler = new AsyncWaitForMessageHandler (WaitForMessage);
		}

		// Send

		delegate void AsyncSendHandler (Message message, TimeSpan timeout);
		AsyncSendHandler send_handler;

		public virtual IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			return BeginSend (message, DefaultSendTimeout, callback, state);
		}

		public virtual IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return send_handler.BeginInvoke (message, timeout, callback, state);
		}

		public virtual void EndSend (IAsyncResult result)
		{
			send_handler.EndInvoke (result);
		}

		public virtual void Send (Message message)
		{
			Send (message, this.DefaultSendTimeout);
		}

		public abstract void Send (Message message, TimeSpan timeout);

		// Receive

		delegate Message AsyncReceiveHandler (TimeSpan timeout);
		AsyncReceiveHandler receive_handler;

		public virtual IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return BeginReceive (this.DefaultReceiveTimeout, callback, state);
		}

		public virtual IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return receive_handler.BeginInvoke (timeout, callback, state);
		}

		public virtual Message EndReceive (IAsyncResult result)
		{
			return receive_handler.EndInvoke (result);
		}

		public virtual Message Receive ()
		{
			return Receive (this.DefaultReceiveTimeout);
		}

		public abstract Message Receive (TimeSpan timeout);

		// TryReceive
		// FIXME: apply those "async to call sync" pattern too (but how?)

		delegate bool TryReceiveHandler (TimeSpan timeout, out Message msg);

		TryReceiveHandler try_receive_handler;

		public virtual IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			Message dummy;
			if (try_receive_handler == null)
				try_receive_handler = new TryReceiveHandler (TryReceive);
			return try_receive_handler.BeginInvoke (timeout, out dummy, callback, state);
		}
		
		public virtual bool EndTryReceive (IAsyncResult result, out Message message)
		{
			return try_receive_handler.EndInvoke (out message, result);
		}
		
		public virtual bool TryReceive (TimeSpan timeout, out Message message)
		{
			return EndTryReceive (BeginTryReceive (timeout, null, null), out message);
		}

		// WaitForMessage

		delegate bool AsyncWaitForMessageHandler (TimeSpan timeout);
		AsyncWaitForMessageHandler wait_handler;

		public virtual IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return wait_handler.BeginInvoke (timeout, callback, state);
		}
		
		public virtual bool EndWaitForMessage (IAsyncResult result)
		{
			return wait_handler.EndInvoke (result);
		}
		
		public abstract bool WaitForMessage (TimeSpan timeout);
	}
}
