//
// Mono.Remoting.Channels.Unix.UnixBinaryClientFormatterSink.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
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
using System.Runtime.Serialization.Formatters.Binary;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixBinaryClientFormatterSink : IClientFormatterSink, IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		UnixBinaryCore _binaryCore = UnixBinaryCore.DefaultInstance;
		IClientChannelSink _nextInChain;

		public UnixBinaryClientFormatterSink (IClientChannelSink nextSink)
		{
			_nextInChain = nextSink;
		}

		internal UnixBinaryCore BinaryCore
		{
			get { return _binaryCore; }
			set { _binaryCore = value; }
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				return _nextInChain;
			}
		}

		public IMessageSink NextSink
		{
			get {
				// This is the last sink in the IMessageSink sink chain
				return null;
			}
		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
						 IMessage msg,
						 ITransportHeaders headers,
						 Stream stream)
	        {
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException("UnixBinaryClientFormatterSink must be the first sink in the IClientChannelSink chain");
		}

		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state,
						  ITransportHeaders headers,
						  Stream stream)
		{
			IMessage replyMessage = (IMessage)_binaryCore.Deserializer.DeserializeMethodResponse (stream, null, (IMethodCallMessage)state);
			sinkStack.DispatchReplyMessage (replyMessage);
		}

		public Stream GetRequestStream (IMessage msg,
						ITransportHeaders headers)
		{
			// never called
			throw new NotSupportedException ();
		}

		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException ();
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg,
			IMessageSink replySink)
		{
			ITransportHeaders transportHeaders = new TransportHeaders();
			Stream stream = _nextInChain.GetRequestStream(msg, transportHeaders);
			if (stream == null) stream = new MemoryStream ();

			_binaryCore.Serializer.Serialize (stream, msg, null);
			if (stream is MemoryStream) stream.Position = 0;

			ClientChannelSinkStack stack = new ClientChannelSinkStack(replySink);
			stack.Push (this, msg);

			_nextInChain.AsyncProcessRequest (stack, msg, transportHeaders, stream);

			// FIXME: No idea about how to implement IMessageCtrl
			return null;	
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			try {

				ITransportHeaders call_headers = new TransportHeaders();
				call_headers["__RequestUri"] = ((IMethodCallMessage)msg).Uri;
				call_headers["Content-Type"] = "application/octet-stream";

				Stream call_stream = _nextInChain.GetRequestStream(msg, call_headers);
				if (call_stream == null) call_stream = new MemoryStream ();

				// Serialize msg to the stream

				_binaryCore.Serializer.Serialize (call_stream, msg, null);
				if (call_stream is MemoryStream) call_stream.Position = 0;

				Stream response_stream;
				ITransportHeaders response_headers;

				_nextInChain.ProcessMessage (msg, call_headers, call_stream, out response_headers,
							    out response_stream);

				// Deserialize response_stream

				return (IMessage) _binaryCore.Deserializer.DeserializeMethodResponse (response_stream, null, (IMethodCallMessage)msg);
				
			} catch (Exception e) {
				 return new ReturnMessage (e, (IMethodCallMessage)msg);
			}
		}
	}
}
