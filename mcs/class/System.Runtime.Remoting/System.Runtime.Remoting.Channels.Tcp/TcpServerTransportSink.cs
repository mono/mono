//
// System.Runtime.Remoting.Channels.Tcp.TcpServerTransportSink.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

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
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.IO;

namespace System.Runtime.Remoting.Channels.Tcp
{
	internal class TcpServerTransportSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		
		public TcpServerTransportSink (IServerChannelSink next)
		{
			next_sink = next;
		}
		
		public IServerChannelSink NextChannelSink 
		{
			get 
			{
				return next_sink;
			}
		}

		public IDictionary Properties 
		{
			get 
			{
				if (next_sink != null) return next_sink.Properties;
				else return null;
			}
		}

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers, Stream responseStream)
		{
			ClientConnection connection = (ClientConnection)state;
			TcpMessageIO.SendMessageStream (connection.Stream, responseStream, headers, connection.Buffer);
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers)
		{
			return null;
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
			IMessage requestMsg,
			ITransportHeaders requestHeaders,
			Stream requestStream,
			out IMessage responseMsg,
			out ITransportHeaders responseHeaders,
			out Stream responseStream)
		{
			// this is the first sink, and TcpServerChannel does not call it.
			throw new NotSupportedException ();
		}

		internal void InternalProcessMessage (ClientConnection connection)
		{
			// Reads the headers and the request stream

			Stream requestStream;
			ITransportHeaders requestHeaders;

			requestStream = TcpMessageIO.ReceiveMessageStream (connection.Stream, out requestHeaders, connection.Buffer);
			requestHeaders [CommonTransportKeys.IPAddress] = connection.ClientAddress;
			requestHeaders [CommonTransportKeys.ConnectionId] = connection.Id;

			// Pushes the connection object together with the sink. This information
			// will be used for sending the response in an async call.

			ServerChannelSinkStack sinkStack = new ServerChannelSinkStack();
			sinkStack.Push(this, connection);

			ITransportHeaders responseHeaders;
			Stream responseStream;
			IMessage responseMsg;

			ServerProcessing proc = next_sink.ProcessMessage(sinkStack, null, requestHeaders, requestStream, out responseMsg, out responseHeaders, out responseStream);

			switch (proc)
			{
				case ServerProcessing.Complete:
					TcpMessageIO.SendMessageStream (connection.Stream, responseStream, responseHeaders, connection.Buffer);
					break;

				case ServerProcessing.Async:
				case ServerProcessing.OneWay:
					break;
			}
		}
	}
}

