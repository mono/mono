//
// System.Runtime.Remoting.Channels.IChannel.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	public interface IChannel
	{
		string ChannelName { get; }

		int ChannelPriorirty { get; }

		string Parse (string url, out string objectURI);
	}
}
