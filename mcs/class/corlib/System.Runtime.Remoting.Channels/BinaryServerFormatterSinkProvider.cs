//
// System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryServerFormatterSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		[MonoTODO]
		public BinaryServerFormatterSinkProvider ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BinaryServerFormatterSinkProvider (IDictionary properties,
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
