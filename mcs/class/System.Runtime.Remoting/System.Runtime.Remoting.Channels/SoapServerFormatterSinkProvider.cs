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
		[MonoTODO]
		public SoapServerFormatterSinkProvider ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SoapServerFormatterSinkProvider (IDictionary properties,
							ICollection providerData)
		{
			throw new NotImplementedException ();
		}

		public IServerChannelSinkProvider Next
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetChannelData (IChannelDataStore channelData)
		{
			throw new NotImplementedException ();
		}
	}
}
