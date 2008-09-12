//
// HttpClientTransportSink.cs
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
	class HttpClientTransportSink : IClientChannelSink
	{
		string url;
		
		public HttpClientTransportSink (string url)
		{
			this.url = url;
		}
		
		//always the last sink in the chain
		public IClientChannelSink NextChannelSink {
			get { return null; }
		}
		
		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
			ITransportHeaders headers, Stream requestStream)
		{
			/*
			HttpConnection connection = null;
			bool isOneWay = RemotingServices.IsOneWay (((IMethodMessage)msg).MethodBase);

			try
			{
				if (headers == null) headers = new TransportHeaders();
				headers [CommonTransportKeys.RequestUri] = ((IMethodMessage)msg).Uri;
				
				// Sends the stream using a connection from the pool
				// and creates a WorkItem that will wait for the
				// response of the server

				connection = HttpConnectionPool.GetConnection (_host, _port);
				HttpMessageIO.SendMessageStream (connection.Stream, requestStream, headers, connection.Buffer, isOneWay);
				connection.Stream.Flush ();

				if (!isOneWay) 
				{
					sinkStack.Push (this, connection);
					ThreadPool.QueueUserWorkItem (new WaitCallback(ReadAsyncHttpMessage), sinkStack);
				}
				else
					connection.Release();
			}
			catch
			{
				if (connection != null) connection.Release();
				if (!isOneWay) throw;
			}
			*/
		}
		
		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack, object state, 
			ITransportHeaders headers, Stream stream)
		{
			// Should never be called
			throw new NotSupportedException();
		}
		
		public Stream GetRequestStream (IMessage msg, ITransportHeaders headers)
		{
			return null;
		}
		
		public void ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
			out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			responseStream = null;
			responseHeaders = null;
			/*
			HttpConnection connection = null;
			try
			{
				if (requestHeaders == null) requestHeaders = new TransportHeaders();
				requestHeaders [CommonTransportKeys.RequestUri] = ((IMethodMessage)msg).Uri;
				
				// Sends the message
				connection = HttpConnectionPool.GetConnection (_host, _port);
				HttpMessageIO.SendMessageStream (connection.Stream, requestStream, requestHeaders, connection.Buffer);
				connection.Stream.Flush ();

				// Reads the response
				MessageStatus status = HttpMessageIO.ReceiveMessageStatus (connection.Stream, connection.Buffer);

				if (status != MessageStatus.MethodMessage)
					throw new RemotingException ("Unknown response message from server");

				responseStream = HttpMessageIO.ReceiveMessageStream (connection.Stream, out responseHeaders, connection.Buffer);
			}
			finally
			{
				if (connection != null) 
					connection.Release();
			}
			*/
		}
		
		public IDictionary Properties {
			get { return null; }
		}
	}
}
