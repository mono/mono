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
		IServerChannelSinkProvider _next;
		
		public SdlChannelSinkProvider()
		{
		}

		public IServerChannelSinkProvider Next 
		{
			get { return _next; } 
			set { _next = value; }
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink next = (_next != null) ? _next.CreateSink (channel) : null;
			return new SdlChannelSink (channel, next);
		}

		public void GetChannelData (IChannelDataStore localChannelData)
		{
		}
	}
}
