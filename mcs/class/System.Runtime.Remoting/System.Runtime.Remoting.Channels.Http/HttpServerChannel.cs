//
// System.Runtime.Remoting.Channels.Http.HttpServerChannel
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;

namespace System.Runtime.Remoting.Channels.Http 
{
	public class HttpServerChannel : BaseChannelWithProperties,
	                                 IChannelReceiver, IChannel, 
					 IChannelReceiverHook
	{
		[MonoTODO]
		public HttpServerChannel()
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
		public string GetChannelUri()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual string[] GetUrlsForUri (string objectUri)
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
		~HttpServerChannel()
		{
		}
	}
}
