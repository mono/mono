//
// System.Runtime.Remoting.Channels.IClientChannelSink.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IClientChannelSink
	{
		IClientChannelSink NextChannelSink { get; }

		void AsyncProcessRequest (IClientChannelSinkStack sinkStack, IMessage msg,
					  ITransportHeaders headers, Stream stream);

		void AsyncProcessRequest (IClientChannelSinkStack sinkStack, object state,
					  ITransportHeaders headers, Stream stream);

		Stream GetRequestStream (IMessage msg, ITransportHeaders headers);

		void ProcessMessage (IMessage msg, ITransportHeaders requestHeaders, Stream requestStream,
				     out ITransportHeaders responseHeaders, out Stream responseStream);
	}
}
