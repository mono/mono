//
// System.Runtime.Remoting.Channels.ServerDispatchSinkProvider.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	internal class ServerDispatchSinkProvider :
		IServerFormatterSinkProvider, IServerChannelSinkProvider
	{
		public ServerDispatchSinkProvider ()
		{
		}

		public ServerDispatchSinkProvider (IDictionary properties, ICollection providerData)
	    {
		}

		public IServerChannelSinkProvider Next
		{
			get {
				return null;
			}

			set {
				throw new NotSupportedException ();
			}
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			return new ServerDispatchSink ();
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
			// no idea why we need this
		}
	}
}
