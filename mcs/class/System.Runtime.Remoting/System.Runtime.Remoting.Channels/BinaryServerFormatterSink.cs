//
// System.Runtime.Remoting.Channels.BinaryServerFormatterSink.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public class BinaryServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}

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

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						IMessage msg, ITransportHeaders headers)
		{
			// never called 
			throw new NotSupportedException ();
		}
		
		[MonoTODO]
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
							out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			throw new NotImplementedException ();
		}
	}
}
