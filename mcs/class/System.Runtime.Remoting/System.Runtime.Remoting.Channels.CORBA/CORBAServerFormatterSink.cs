//
// System.Runtime.Remoting.Channels.CORBA.CORBAServerFormatterSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.CORBA {

	public class CORBAServerFormatterSink : IServerChannelSink, IChannelSinkBase
	{
		IServerChannelSink next_sink;
		CDRFormatter format = new CDRFormatter ();
	
		public CORBAServerFormatterSink (IServerChannelSink nextSink)
		{
			next_sink = nextSink;
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
		
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			IMessage call;
			
			string uri = (string)requestHeaders ["_requestUri"];

			if (requestMsg == null) {
				call = (IMessage)format.DeserializeRequest (requestStream, uri);
			} else { 
				call = requestMsg;
			}

			next_sink.ProcessMessage (sinkStack, call, requestHeaders, null,
						  out responseMsg, out responseHeaders, out responseStream);
						
			responseStream = new MemoryStream ();

			format.SerializeResponse (responseStream, responseMsg);
			
			return ServerProcessing.Complete;
		}
	}
}
