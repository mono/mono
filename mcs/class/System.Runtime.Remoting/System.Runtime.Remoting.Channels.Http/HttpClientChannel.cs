//
// System.Runtime.Remoting.Channels.Http.HttpClientChannel
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Http 
{
        public class HttpClientChannel : BaseChannelWithProperties,
	                                 IChannelSender, IChannel 
	{
		[MonoTODO]
		public HttpClientChannel()
		{
		}

		public string ChannelName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public int ChannelPriority {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public override object this [object key] {
			[MonoTODO]
			get { throw new NotImplementedException(); } 

			[MonoTODO]
			set { throw new NotImplementedException(); }
		}

		public override ICollection Keys {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public override IDictionary Properties {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public IMessageSink CreateMessageSink (string url, 
						       object remoteChannelData,
						       out string objectURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public string Parse (string url, out string objectURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~HttpClientChannel()
		{
		}
	}	
}
