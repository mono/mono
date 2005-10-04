//
// Mono.Remoting.Channels.Unix.UnixBinaryServerFormatterSink.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
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
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace Mono.Remoting.Channels.Unix {

	internal class UnixBinaryServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		UnixBinaryCore _binaryCore = UnixBinaryCore.DefaultInstance;

		IServerChannelSink next_sink;
		IChannelReceiver receiver;

		public UnixBinaryServerFormatterSink (IServerChannelSink nextSink, IChannelReceiver receiver)
		{
			this.next_sink = nextSink;
			this.receiver = receiver;
		}

		internal UnixBinaryCore BinaryCore
		{
			get { return _binaryCore; }
			set { _binaryCore = value; }
		}
		
		public IServerChannelSink NextChannelSink {
			get {
				return next_sink;
			}
		}

		public IDictionary Properties {
			get {
				return null;
			}
		}

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage message, ITransportHeaders headers, Stream stream)
		{
			ITransportHeaders responseHeaders = new TransportHeaders();

			if (sinkStack != null) stream = sinkStack.GetResponseStream (message, responseHeaders);
			if (stream == null) stream = new MemoryStream();

			_binaryCore.Serializer.Serialize (stream, message, null);
			if (stream is MemoryStream) stream.Position = 0;

			sinkStack.AsyncProcessResponse (message, responseHeaders, stream);
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						IMessage msg, ITransportHeaders headers)
		{
			return null;
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
							out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			sinkStack.Push (this, null);
			ServerProcessing res;

			try
			{
				string url = (string)requestHeaders["__RequestUri"];
				string uri;
				receiver.Parse (url, out uri);
				if (uri == null) uri = url;

				MethodCallHeaderHandler mhh = new MethodCallHeaderHandler(uri);
				requestMsg = (IMessage) _binaryCore.Deserializer.Deserialize (requestStream, new HeaderHandler(mhh.HandleHeaders));

				res = next_sink.ProcessMessage (sinkStack, requestMsg, requestHeaders, null, out responseMsg, out responseHeaders, out responseStream);
			}
			catch (Exception ex)
			{
				responseMsg = new ReturnMessage (ex, (IMethodCallMessage)requestMsg);
				res = ServerProcessing.Complete;
				responseHeaders = null;
				responseStream = null;
			}
			
			if (res == ServerProcessing.Complete)
			{
				for (int n=0; n<3; n++) {
					responseStream = null;
					responseHeaders = new TransportHeaders();

					if (sinkStack != null) responseStream = sinkStack.GetResponseStream (responseMsg, responseHeaders);
					if (responseStream == null) responseStream = new MemoryStream();

					try {
						_binaryCore.Serializer.Serialize (responseStream, responseMsg);
						break;
					} catch (Exception ex) {
						if (n == 2) throw ex;
						else responseMsg = new ReturnMessage (ex, (IMethodCallMessage)requestMsg);
					}
				}
				
				if (responseStream is MemoryStream) responseStream.Position = 0;
				

				sinkStack.Pop (this);
			}
			return res;
		}

	}

	internal class MethodCallHeaderHandler
	{
		string _uri;

		public MethodCallHeaderHandler (string uri)
		{
			_uri = uri;
		}

		public object HandleHeaders (Header[] headers)
		{
			return _uri;
		}
	}
}
