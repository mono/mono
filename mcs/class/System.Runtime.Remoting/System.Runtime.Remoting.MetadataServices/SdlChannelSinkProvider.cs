//
// System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.MetadataServices
{
	public class SdlChannelSinkProvider : IServerChannelSinkProvider
	{
		[MonoTODO]
		public SdlChannelSinkProvider()
		{
		}

		public IServerChannelSinkProvider Next {
			[MonoTODO]
			get { throw new NotImplementedException(); } 
			[MonoTODO]
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void GetChannelData (IChannelDataStore localChannelData)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~SdlChannelSinkProvider()
		{
		}

	}
}
