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
					   IMessage msg, ITransportHeaders headers, Stream stream);

		Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
					  IMessage msg, ITransportHeaders headers);

		ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
						 IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream,
						 out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream);
	}
}
