//
// System.Runtime.Remoting.Channels.BinaryClientFormatterSink.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		BinaryCore _binaryCore = BinaryCore.DefaultInstance;
		IClientChannelSink _nextInChain;

		public BinaryClientFormatterSink (IClientChannelSink nextSink)
		{
			_nextInChain = nextSink;
		}

		internal BinaryCore BinaryCore
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
			throw new NotSupportedException("BinaryClientFormatterSink must be the first sink in the IClientChannelSink chain");
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
				call_headers[CommonTransportKeys.RequestUri] = ((IMethodCallMessage)msg).Uri;
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
