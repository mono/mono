//
// System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class BinaryClientFormatterSinkProvider :
		IClientFormatterSinkProvider, IClientChannelSinkProvider
	{
		[MonoTODO]
		public BinaryClientFormatterSinkProvider ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BinaryClientFormatterSinkProvider (IDictionary properties,
							  ICollection providerData)
	        {
			throw new NotImplementedException ();
		}

		public IClientChannelSinkProvider Next
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IClientChannelSink CreateSink (IChannelSender channel,
						      string url,
						      object remoteChannelData)
		{
			throw new NotImplementedException ();
		}		
	}
}
