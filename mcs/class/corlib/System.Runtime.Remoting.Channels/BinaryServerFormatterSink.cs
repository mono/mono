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
		IServerChannelSink next_sink;
		
		[MonoTODO]
		public BinaryServerFormatterSink (BinaryServerFormatterSink.Protocol protocol,
						  IServerChannelSink nextSink,
						  IChannelReceiver receiver)
		{
			this.next_sink = nextSink;
		}

		public IServerChannelSink NextChannelSink {
			get {
				return next_sink;
			}
		}

		[MonoTODO]
		public IDictionary Properties {
			get {
				throw new NonImplementedException ();
			}
		}

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
					  ITransportHeaders headers, Stream stream)
		{
			throw new NonImplementedException ();
		}

		[MonoTODO]
		public Stream GetReponseStream (IMessage msg, ITransportHeaders headers)
		{
			throw new NonImplementedException ();
		}
		
		[MonoTODO]
		void ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
				     out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			throw new NonImplementedException ();
		}
		
		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}
	}
}
