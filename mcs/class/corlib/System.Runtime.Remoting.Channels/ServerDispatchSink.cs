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

		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack, object state,
						  IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack, object state,
						 IMessage msg, ITransportHeaders headers)
		{
			throw new NotImplementedException ();
		}
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			IMethodCallMessage call = (IMethodCallMessage)requestMsg;
			
			string uri = (string)requestHeaders ["_requestUri"];
			
			MarshalByRefObject svr = RemotingServices.GetServerForUri (uri);
			if (svr == null)
				throw new RemotingException ("no registered server for uri " + uri); 

			responseMsg = RemotingServices.ExecuteMessage (svr, call);
			responseHeaders = null;			
			responseStream = null;
			
			return ServerProcessing.Complete;
		}
	}
}
