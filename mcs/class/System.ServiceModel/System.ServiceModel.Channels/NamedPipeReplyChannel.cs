//
// NamedPipeReplyChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels.NetTcp;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class NamedPipeReplyChannel : InternalReplyChannelBase
	{
		NamedPipeServerStream server;
		TcpBinaryFrameManager frame;

		public NamedPipeReplyChannel (ChannelListenerBase listener, MessageEncoder encoder, NamedPipeServerStream server)
			: base (listener)
		{
			this.server = server;
			this.Encoder = encoder;
		}

		public MessageEncoder Encoder { get; private set; }

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			// It is used while it is already closed.
			if (server == null || !server.IsConnected)
				return null;

			if (timeout <= TimeSpan.Zero)
				throw new ArgumentException (String.Format ("Timeout value must be positive value. It was {0}", timeout));
			var msg = frame.ReadUnsizedMessage (timeout);
			frame.ReadEndRecord ();
			return new NamedPipeRequestContext (this, msg);
		}

		class NamedPipeRequestContext : InternalRequestContext
		{
			public NamedPipeRequestContext (NamedPipeReplyChannel owner, Message request)
				: base (owner.Manager)
			{
				this.owner = owner;
				this.request = request;
			}

			NamedPipeReplyChannel owner;
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
				if (message.Headers.RelatesTo == null)
					message.Headers.RelatesTo = request.Headers.MessageId;

				owner.frame.WriteUnsizedMessage (message, timeout);
				owner.frame.WriteEndRecord ();
				owner.server.Close ();
				owner.server = null;
			}
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			try {
				context = ReceiveRequest (timeout);
				return context != null;
			} catch (TimeoutException) {
				context = null;
				return false;
			}
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// FIXME: use timeout
			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.SingletonUnsizedMode, server, true) { Encoder = this.Encoder };
			frame.ProcessPreambleRecipient ();
			frame.ProcessPreambleAckRecipient ();
		}
	}
}
