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

		BinaryCore _binaryCore = BinaryCore.DefaultInstance;

		IServerChannelSink next_sink;
		Protocol protocol;
		IChannelReceiver receiver;

		public BinaryServerFormatterSink (BinaryServerFormatterSink.Protocol protocol,
						  IServerChannelSink nextSink,
						  IChannelReceiver receiver)
		{
			this.protocol = protocol;
			this.next_sink = nextSink;
			this.receiver = receiver;
		}

		internal BinaryCore BinaryCore
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
				string url = (string)requestHeaders[CommonTransportKeys.RequestUri];
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
				responseStream = null;
				responseHeaders = new TransportHeaders();

				if (sinkStack != null) responseStream = sinkStack.GetResponseStream (responseMsg, responseHeaders);
				if (responseStream == null) responseStream = new MemoryStream();

				_binaryCore.Serializer.Serialize (responseStream, responseMsg);
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
