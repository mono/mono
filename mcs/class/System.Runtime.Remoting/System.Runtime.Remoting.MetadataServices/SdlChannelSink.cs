//
// System.Runtime.Remoting.MetadataServices.SdlChannelSink
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Collections;

namespace System.Runtime.Remoting.MetadataServices
{
        public class SdlChannelSink : IServerChannelSink, IChannelSinkBase
	{
		[MonoTODO]
		public SdlChannelSink()
		{
		}

		public IServerChannelSink NextChannelSink {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public IDictionary Properties {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}
		
		[MonoTODO]
		public void AsyncProcessResponse (IServerResponseChannelSinkStack sinkStack,
						  object state,
						  IMessage msg,
						  ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public Stream GetResponseStream (IServerResponseChannelSinkStack sinkStack,
						 object state,
						 IMessage msg,
						 ITransportHeaders headers)
		{
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public ServerProcessing ProcessMessage (IServerChannelSinkStack sinkStack,
							IMessage requestMsg,
							ITransportHeaders requestHeaders,
							Stream requestStream,
							out IMessage responseMsg,
							out ITransportHeaders responseHeaders,
							out Stream responseStream)
		{
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		~SdlChannelSink()
		{
		}
	}
}
