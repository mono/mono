//
// System.Runtime.Remoting.Channels.IChannelReceiver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	public interface IChannelReceiver : IChannel
	{
		object ChannelData { get; }

		string [] GetUrlsForUri (string objectUri);

		void StartListening (object data);

		void StopListening (object data);
	}
}
