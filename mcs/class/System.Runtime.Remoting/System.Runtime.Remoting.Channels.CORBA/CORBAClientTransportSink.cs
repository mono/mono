//
// System.Runtime.Remoting.Channels.CORBA.CORBAClientTransportSink.cs
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
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.CORBA
{

	internal class CORBAClientTransportSink : IClientChannelSink
	{
		string host;
		string object_uri;
		int port;
		
		TcpClient tcpclient;
		
		public CORBAClientTransportSink (string url)
		{
			host = CORBAChannel.ParseCORBAURL (url, out object_uri, out port);
			tcpclient = new TcpClient ();
		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				// we are the last one
				return null;
			}
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
						 ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();			
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state, ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			// no direct access to the stream
			return null;
		}
		
		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// get a network stream
			tcpclient.Connect (host, port);
			Stream network_stream = tcpclient.GetStream ();

			// send the message
			IIOPMessage.SendMessageStream (network_stream, (MemoryStream)requestStream,
						       IIOPMessage.MessageType.Request,
						       object_uri);
			
			// read the response fro the network an copy it to a memory stream
			IIOPMessage.MessageType msg_type;
			string uri;
			MemoryStream mem_stream =
				IIOPMessage.ReceiveMessageStream (network_stream, out msg_type, out uri);

			// close the stream
			tcpclient.Close ();

			switch (msg_type) {
			case IIOPMessage.MessageType.Response:
				//fixme: read response message
				responseHeaders = null;
			
				responseStream = mem_stream;
		
				break;
			default:
				throw new Exception ("unknown response mesage header");
			}
		}
			
	}
}	
