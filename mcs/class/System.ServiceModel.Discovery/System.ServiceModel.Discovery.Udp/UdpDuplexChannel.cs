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

namespace System.ServiceModel.Discovery
{
	internal class UdpDuplexChannel : ChannelBase, IDuplexChannel
	{
		// channel factory
		public UdpDuplexChannel (UdpChannelFactory factory, BindingContext ctx, EndpointAddress address, Uri via)
			: base (factory)
		{
			binding_element = factory.Source;
			RemoteAddress = address;
			Via = via;
		}
		
		public UdpDuplexChannel (UdpChannelListener listener, Uri listenUri)
			: base (listener)
		{
			binding_element = listener.Source;
			LocalAddress = new EndpointAddress (listenUri);
		}
		
		MessageEncoder message_encoder; // FIXME: fill it
		UdpClient client;
		UdpTransportSettings settings;
		UdpTransportBindingElement binding_element;
		
		// for servers
		public EndpointAddress LocalAddress { get; private set; }
		// for clients
		public EndpointAddress RemoteAddress { get; private set; }
		
		public Uri Via { get; private set; }
		
		public void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}

		public void Send (Message message, TimeSpan timeout)
		{
			if (State != CommunicationState.Opened)
				throw new InvalidOperationException ("The UDP channel must be opened before sending a message.");

			var ms = new MemoryStream ();
			message_encoder.WriteMessage (message, ms);
			client.Send (ms.GetBuffer (), (int) ms.Length);
		}

		public bool WaitForMessage (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public Message Receive ()
		{
			return Receive (DefaultReceiveTimeout);
		}

		public Message Receive (TimeSpan timeout)
		{
			Message msg;
			if (!TryReceive (timeout, out msg))
				throw new TimeoutException ();
			return msg;
		}

		public bool TryReceive (TimeSpan timeout, out Message msg)
		{
			throw new NotImplementedException ();
		}

		protected override void OnAbort ()
		{
			if (client != null)
				client.Close ();
			client = null;
		}
		
		Action<TimeSpan> open_delegate, close_delegate;
		
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
		
		protected override void OnClose (TimeSpan timeout)
		{
			if (client != null)
				client.Close ();
			client = null;
		}
		
		protected override void OnOpen (TimeSpan timeout)
		{
			if (RemoteAddress != null) {
				client = new UdpClient ();
				client.Connect (RemoteAddress.Uri.Host, RemoteAddress.Uri.Port);
			} else {
				client = new UdpClient (LocalAddress.Uri.Host, LocalAddress.Uri.Port);
			}

			// FIXME: apply UdpTransportSetting here.
		}
		
		Func<TimeSpan,Message> receive_delegate;
		
		public IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return BeginReceive (DefaultReceiveTimeout, callback, state);
		}
		
		public IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (receive_delegate == null)
				receive_delegate = new Func<TimeSpan,Message> (Receive);
			return receive_delegate.BeginInvoke (timeout, callback, state);
		}
		
		public Message EndReceive (IAsyncResult result)
		{
			return receive_delegate.EndInvoke (result);
		}
		
		delegate bool TryReceiveDelegate (TimeSpan timeout, out Message msg);
		TryReceiveDelegate try_receive_delegate;

		public IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (try_receive_delegate == null)
				try_receive_delegate = new TryReceiveDelegate (TryReceive);
			Message dummy;
			return try_receive_delegate.BeginInvoke (timeout, out dummy, callback, state);
		}
		
		public bool EndTryReceive (IAsyncResult result, out Message msg)
		{
			return try_receive_delegate.EndInvoke (out msg, result);
		}

		Func<TimeSpan,bool> wait_delegate;
		
		public IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_delegate == null)
				wait_delegate = new Func<TimeSpan,bool> (WaitForMessage);
			return wait_delegate.BeginInvoke (timeout, callback, state);
		}
		
		public bool EndWaitForMessage (IAsyncResult result)
		{
			return wait_delegate.EndInvoke (result);
		}

		Action<Message,TimeSpan> send_delegate;
		
		public IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			return BeginSend (message, DefaultSendTimeout, callback, state);
		}
		
		public IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (send_delegate == null)
				send_delegate = new Action<Message,TimeSpan> (Send);
			return send_delegate.BeginInvoke (message, timeout, callback, state);
		}
		
		public void EndSend (IAsyncResult result)
		{
			send_delegate.EndInvoke (result);
		}
	}
}
