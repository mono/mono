//
// System.Runtime.Remoting.Channels.IChannelReceiverHook.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//


namespace System.Runtime.Remoting.Channels {

	public interface IChannelReceiverHook
	{
		string ChannelScheme { get; }

		IServerChannelSink ChannelSinkChain { get; }

		bool WantsToListen { get; }

		void AddHookChannelUri (string channelUri);
	}
}
