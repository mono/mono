//
// System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class SoapServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		private IServerChannelSinkProvider _next;
//		////[MonoTODO]
		public SoapServerFormatterSinkProvider ()
		{
//			throw new NotImplementedException ();
		}

//		////[MonoTODO]
		public SoapServerFormatterSinkProvider (IDictionary properties,
							ICollection providerData)
		{
//			throw new NotImplementedException ();
		}

		public IServerChannelSinkProvider Next
		{
//			////[MonoTODO]
			get {
				return _next;
			}

//			////[MonoTODO]
			set {
				_next = value;
			}
		}

//		////[MonoTODO]
		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink chain = _next.CreateSink(channel);
			IServerChannelSink sinkFormatter = new SoapServerFormatterSink(SoapServerFormatterSink.Protocol.Http, chain, channel);
			
			return sinkFormatter;
		}

//		////[MonoTODO]
		public void GetChannelData (IChannelDataStore channelData)
		{
			if(_next != null)
				_next.GetChannelData(channelData);
		}
	}
}
