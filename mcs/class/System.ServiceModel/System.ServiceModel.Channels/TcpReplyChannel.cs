//
// HttpReplyChannel.cs
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
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class TcpReplyChannel : ReplyChannelBase
	{
		TcpClient client;
		TcpChannelInfo info;
		TcpBinaryFrameManager frame;

		public TcpReplyChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpClient client)
			: base (listener)
		{
			this.client = client;
			this.info = info;
		}

		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));
			var msg = frame.ReadUnsizedMessage (timeout);
			return new TcpRequestContext (this, msg);
		}

		class TcpRequestContext : InternalRequestContext
		{
			public TcpRequestContext (TcpReplyChannel owner, Message request)
				: base (owner.Manager)
			{
				this.owner = owner;
				this.request = request;
			}

			TcpReplyChannel owner;
			Message request;

			public override Message RequestMessage {
				get { return request; }
			}

			public override void Abort ()
			{
				Close (TimeSpan.Zero);
			}

			public override void Close (TimeSpan timeout)
			{
			}

			public override void Reply (Message message, TimeSpan timeout)
			{
				DateTime start = DateTime.Now;
				owner.frame.WriteUnsizedMessage (message, timeout);
				owner.frame.ReadUnsizedMessageTerminator (timeout - (DateTime.Now - start));
				owner.frame.ProcessEndRecordRecipient ();
			}
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			try {
				DateTime start = DateTime.Now;
				context = ReceiveRequest (timeout);
				if (context != null)
					return true;
				// received EndRecord, so close the session and return false instead.
				// (Closing channel here might not be a good idea, but right now I have no better way.)
				Close (timeout - (DateTime.Now - start));
				return false;
			} catch (TimeoutException) {
				context = null;
				return false;
			}
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override EndpointAddress LocalAddress {
			get { throw new NotImplementedException (); }
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (client != null)
				client.Close ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			NetworkStream ns = client.GetStream ();
			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.SingletonUnsizedMode, ns, true) { Encoder = this.Encoder, EncodingRecord = TcpBinaryFrameManager.EncodingBinary };
			frame.ProcessPreambleRecipient ();
			frame.ProcessPreambleAckRecipient ();
		}
	}
}
