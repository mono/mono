//
// System.Runtime.Remoting.Channels.IServerChannelSinkProvider.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	public interface IServerChannelSinkProvider
	{
		IServerChannelSinkProvider Next { get;  set; }

		IServerChannelSink CreateSink (IChannelReceiver channel)

		void GetChannelData (IChannelDataStore channelData);
	}
}
