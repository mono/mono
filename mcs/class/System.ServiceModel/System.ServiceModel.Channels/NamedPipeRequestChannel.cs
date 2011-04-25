//
// NamedPipeRequestChannel.cs
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
using System.ServiceModel.Description;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class NamedPipeRequestChannel : RequestChannelBase
	{
		TcpBinaryFrameManager frame;

		public NamedPipeRequestChannel (ChannelFactoryBase factory, MessageEncoder encoder, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			this.Encoder = encoder;
		}

		public MessageEncoder Encoder { get; private set; }

		protected override void OnAbort ()
		{
			OnClose (TimeSpan.Zero);
		}

		protected override void OnClose (TimeSpan timeout)
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		void CreateClient (TimeSpan timeout)
		{
			int explicitPort = Via.Port;
			var stream = new NamedPipeClientStream (".", Via.LocalPath.Substring (1).Replace ('/', '\\'), PipeDirection.InOut);
			stream.Connect ();
			frame = new TcpBinaryFrameManager (TcpBinaryFrameManager.SingletonUnsizedMode, stream, false) {
				Encoder = this.Encoder,
				Via = this.Via };
			frame.ProcessPreambleInitiator ();
			frame.ProcessPreambleAckInitiator ();
		}

		public override Message Request (Message input, TimeSpan timeout)
		{
			DateTime start = DateTime.Now;

			CreateClient (timeout);

			if (input.Headers.To == null)
				input.Headers.To = RemoteAddress.Uri;
			if (input.Headers.MessageId == null)
				input.Headers.MessageId = new UniqueId ();

			frame.WriteUnsizedMessage (input, timeout - (DateTime.Now - start));

			frame.WriteEndRecord ();

			var ret = frame.ReadUnsizedMessage (timeout - (DateTime.Now - start));
			frame.ReadEndRecord ();
			return ret;
		}
	}
}
