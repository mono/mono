//
// System.Runtime.Remoting.Channels.IServerChannelSink.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IServerChannelSink : IChannelSinkBase
	{
		IServerChannelSink NextChannelSink { get; }

		void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
					  ITransportHeaders headers, Stream stream);

		Stream GetReponseStream (IMessage msg, ITransportHeaders headers);

		void ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
				     out ITransportHeaders responseHeaders, out Stream responseStream);
	}
}
