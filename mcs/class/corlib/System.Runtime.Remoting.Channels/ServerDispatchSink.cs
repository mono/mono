//
// System.Runtime.Remoting.Channels/ServerDispatchSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	internal class ServerDispatchSink : IServerChannelSink, IChannelSinkBase
	{
		public ServerDispatchSink ()
		{
		}

		public IServerChannelSink NextChannelSink {
			get {
				return null;
			}
		}

		public IDictionary Properties {
			get {
				return null;
			}
		}

		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			// Never called (this sink does not push itself into the sink stack)
			throw new NotSupportedException ();
		}

		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						 IMessage msg, ITransportHeaders headers)
		{
			return null;
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			responseHeaders = null;			
			responseStream = null;

			return ChannelServices.DispatchMessage(sinkStack, requestMsg, out responseMsg);
		}
	}
}
