//
// System.Runtime.Remoting.Channels.BinaryServerFormatterSink.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels {

	public class BinaryServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}

		static BinaryFormatter _serializationFormatter;
		static BinaryFormatter _deserializationFormatter;

		IServerChannelSink next_sink;
		Protocol protocol;
		IChannelReceiver receiver;

		static BinaryServerFormatterSink()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext(StreamingContextStates.Remoting, null);

			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
		}
		
		public BinaryServerFormatterSink (BinaryServerFormatterSink.Protocol protocol,
						  IServerChannelSink nextSink,
						  IChannelReceiver receiver)
		{
			this.protocol = protocol;
			this.next_sink = nextSink;
			this.receiver = receiver;
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

			_serializationFormatter.Serialize (stream, message, null);
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

			requestMsg = (IMessage) _deserializationFormatter.Deserialize (requestStream, null);

			string url = (string)requestHeaders[CommonTransportKeys.RequestUri];
			string uri;
			receiver.Parse (url, out uri);
			if (uri == null) uri = url;
			requestMsg.Properties["__Uri"] = uri;

			// Fixme: check if the message is an async msg

			ServerProcessing res = next_sink.ProcessMessage (sinkStack, requestMsg, requestHeaders, null, out responseMsg, out responseHeaders, out responseStream);

			if (res == ServerProcessing.Complete)
			{
				responseStream = null;
				responseHeaders = new TransportHeaders();

				if (sinkStack != null) responseStream = sinkStack.GetResponseStream (responseMsg, responseHeaders);
				if (responseStream == null) responseStream = new MemoryStream();

				_serializationFormatter.Serialize (responseStream, responseMsg);
				if (responseStream is MemoryStream) responseStream.Position = 0;

				sinkStack.Pop (this);
			}
			return res;
		}

	}
}
