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

	public class SoapServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		
		[MonoTODO]
		public SoapServerFormatterSink (SoapServerFormatterSink.Protocol protocol,
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
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
					  ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Stream GetReponseStream (IMessage msg, ITransportHeaders headers)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
							out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			throw new NotImplementedException ();
		}

		[Serializable]
		public enum Protocol
		{
			Http = 0,
			Other = 1,
		}
	}
}
