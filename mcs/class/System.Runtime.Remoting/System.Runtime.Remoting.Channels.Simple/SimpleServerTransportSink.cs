//
// System.Runtime.Remoting.Channels.Simple.SimpleServerTransportSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
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

using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace System.Runtime.Remoting.Channels.Simple
{
	internal class SimpleServerTransportSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		
		public SimpleServerTransportSink (IServerChannelSink next)
		{
			next_sink = next;
		}
		
		public IServerChannelSink NextChannelSink {
			get {
				return next_sink;
			}
		}

		[MonoTODO]
		public IDictionary Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						IMessage msg, ITransportHeaders headers)
		{
			throw new NotImplementedException ();
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			// this is the first sink, and SimpleServerChannel does not call it.
			throw new NotSupportedException ();
		}

		internal void InternalProcessMessage (Stream network_stream)
		{
			try {
				string uri;
				SimpleMessageFormat.MessageType msg_type;
				MemoryStream msg_stream;

				msg_stream = SimpleMessageFormat.ReceiveMessageStream (network_stream,
										       out msg_type, out uri);
				if (msg_type != SimpleMessageFormat.MessageType.Request)
					throw new RemotingException ("received wrong message type");
				
				TransportHeaders headers = new TransportHeaders ();
				headers ["_requestUri"] = uri;

				IMessage resp_message;
				ITransportHeaders resp_headers;
				Stream resp_stream;
				ServerProcessing res = next_sink.ProcessMessage (null, null, headers, msg_stream,
										 out resp_message, out resp_headers,
										 out resp_stream);

				switch (res) {
				case ServerProcessing.Complete:

					Exception e = ((IMethodReturnMessage)resp_message).Exception;
					if (e != null) {
						// we handle exceptions in the transport channel
						SimpleMessageFormat.SendExceptionMessage (network_stream, e.ToString ());
					} else {
						// send the response
						SimpleMessageFormat.SendMessageStream (network_stream,
										       (MemoryStream)resp_stream, 
										       SimpleMessageFormat.MessageType.Response,
										       null);
					}
					break;
				case ServerProcessing.Async:
				case ServerProcessing.OneWay:
					throw new NotImplementedException ();					
				}
				
			} catch (Exception e) {
				SimpleMessageFormat.SendExceptionMessage (network_stream, e.ToString ());
			}
		}
	}
}
