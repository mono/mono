// 
// TcpDuplexSessionChannel.cs
// 
// Author: 
//	Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class TcpDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{
		class TcpDuplexSession : DuplexSessionBase
		{
			TcpDuplexSessionChannel owner;

			internal TcpDuplexSession (TcpDuplexSessionChannel owner)
			{
				this.owner = owner;
			}

			public override TimeSpan DefaultCloseTimeout {
				get { return owner.DefaultCloseTimeout; }
			}

			public override void Close (TimeSpan timeout)
			{
				owner.DiscardSession ();
			}
		}

		TcpChannelInfo info;
		TcpClient client;
		bool is_service_side;
		TcpBinaryFrameManager frame;
		TcpDuplexSession session; // do not use this directly. Use Session instead.
		EndpointAddress counterpart_address;
		
		public TcpDuplexSessionChannel (ChannelFactoryBase factory, TcpChannelInfo info, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			this.info = info;

			// make sure to acquire TcpClient here.
			int explicitPort = Via.Port;
			client = new TcpClient (Via.Host, explicitPort <= 0 ? TcpTransportBindingElement.DefaultPort : explicitPort);
			counterpart_address = GetEndpointAddressFromTcpClient (client);
		}
		
		public TcpDuplexSessionChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpClient client)
			: base (listener)
		{
			is_service_side = true;
			this.client = client;
			this.info = info;
			counterpart_address = GetEndpointAddressFromTcpClient (client);
		}

		EndpointAddress GetEndpointAddressFromTcpClient (TcpClient client)
		{
			IPEndPoint ep = (IPEndPoint) client.Client.RemoteEndPoint;
			return new EndpointAddress (new Uri ("net.tcp://" + ep));
		}

		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override EndpointAddress RemoteAddress {
			get { return base.RemoteAddress ?? counterpart_address; }
		}

		public override EndpointAddress LocalAddress {
			get { return base.LocalAddress ?? counterpart_address; }
		}

		public IDuplexSession Session {
			get {
				if (session == null)
					session = new TcpDuplexSession (this);
				return session;
			}
		}

		internal TcpClient TcpClient {
			get { return client; }
		}

		void DiscardSession ()
		{
			if (client.Connected)
				frame.WriteEndRecord ();
			session = null;
		}

		public override void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}
		
		public override void Send (Message message, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));

			if (!is_service_side) {
				if (message.Headers.To == null)
					message.Headers.To = RemoteAddress.Uri;
			}

			client.SendTimeout = (int) timeout.TotalMilliseconds;
			frame.WriteSizedMessage (message);
		}
		
		public override bool TryReceive (TimeSpan timeout, out Message message)
		{
			ThrowIfDisposedOrNotOpen ();

			// FIXME: there seems to be some pipeline or channel-
			// recycling issues, which could be mostly workarounded 
			// by delaying input receiver.
			// This place is not ideal, but it covers both loops in
			// ChannelDispatcher and DuplexClientRuntimeChannel.
			Thread.Sleep (50);

			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));
			client.ReceiveTimeout = (int) timeout.TotalMilliseconds;
			message = frame.ReadSizedMessage ();
			// FIXME: this may not be precise, but connection might be reused for some weird socket state transition (that's what happens). So as a workaround, avoid closing the session by sending EndRecord from this channel at OnClose().
			if (message == null) {
				session = null;
				return false;
			}
			return true;
		}
		
		public override bool WaitForMessage (TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			if (client.Available > 0)
				return true;

			DateTime start = DateTime.Now;
			do {
				Thread.Sleep (50);
				if (client.Available > 0)
					return true;
			} while (DateTime.Now - start < timeout);
			return false;
		}
		
		// CommunicationObject
		
		[MonoTODO]
		protected override void OnAbort ()
		{
			if (session != null)
				session.Close (TimeSpan.FromTicks (0));

			if (client != null)
				client.Close ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (session != null)
				session.Close (timeout);

			if (client != null)
				client.Close ();
		}
		
		protected override void OnOpen (TimeSpan timeout)
		{
			if (! is_service_side) {
				NetworkStream ns = client.GetStream ();
				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, ns, is_service_side) {
					Encoder = this.Encoder,
					Via = this.Via };
				frame.ProcessPreambleInitiator ();
				frame.ProcessPreambleAckInitiator ();
			} else {
				// server side
				Stream s = client.GetStream ();

				frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.DuplexMode, s, is_service_side) { Encoder = this.Encoder };

				// FIXME: use retrieved record properties in the request processing.

				frame.ProcessPreambleRecipient ();
				frame.ProcessPreambleAckRecipient ();
			}
		}
	}
}
