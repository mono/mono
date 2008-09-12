//
// HttpServerTransportSink.cs
// 
// Author:
//   Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http
{
	class HttpServerTransportSink : IServerChannelSink
	{
		
		IServerChannelSink nextSink;
		
		public HttpServerTransportSink (IServerChannelSink nextSink)
		{
			this.nextSink = nextSink;
		}
		
		public IServerChannelSink NextChannelSink {
			get { return nextSink; }
		}
		
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers, Stream responseStream)
		{
			/*
			ClientConnection connection = (ClientConnection)state;
			
			NetworkStream ns = new NetworkStream (connection.Socket);
			HttpMessageIO.SendMessageStream (ns, responseStream, headers, connection.Buffer);
			ns.Flush ();
			ns.Close ();
			*/
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
			IMessage msg, ITransportHeaders headers)
		{
			return null;
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack, IMessage requestMsg,
			ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			responseMsg = null;
			responseHeaders = null;
			responseStream = null;
			
			// this is the first sink, and HttpServerChannel does not call it.
			throw new NotSupportedException ();
		}
		
		public IDictionary Properties {
			get { 
				if (nextSink != null)
					return nextSink.Properties;
				else return null;
			}
		}
		
	}
}
