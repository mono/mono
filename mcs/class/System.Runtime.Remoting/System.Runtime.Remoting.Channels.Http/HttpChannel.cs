//
// System.Runtime.Remoting.Channels.Http.HttpChannel
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
        public class HttpChannel : BaseChannelWithProperties,
	                           IChannelReceiver, IChannel, 
				   IChannelSender, IChannelReceiverHook
	{
		[MonoTODO]
		public HttpChannel()
		{
		}

		public object ChannelData {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string ChannelName {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public int ChannelPriority {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public string ChannelScheme {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		public IServerChannelSink ChannelSinkChain {
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

		public bool WantsToListen {
			[MonoTODO]
			get { throw new NotImplementedException(); } 

			[MonoTODO]
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public void AddHookChannelUri (string channelUri)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IMessageSink CreateMessageSink (string url, 
						       object remoteChannelData,
						       out string objectURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public string[] GetUrlsForUri (string objectURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public string Parse (string url, out string objectURI)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void StartListening (object data)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void StopListening (object data)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~HttpChannel()
		{
		}
	}
}
