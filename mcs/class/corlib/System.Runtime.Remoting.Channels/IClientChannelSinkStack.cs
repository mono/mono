//
// System.Runtime.Remoting.Channels.IClientChannelSinkStack.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

namespace System.Runtime.Remoting.Channels {

	public interface IClientChannelSinkStack : IClientResponseChannelSinkStack
	{
		object Pop (IClientChannelSink sink);

		void Push (IClientChannelSink sink, object state);
	}
}
