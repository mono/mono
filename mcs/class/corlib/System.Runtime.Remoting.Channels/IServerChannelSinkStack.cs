//
// System.Runtime.Remoting.Channels.IServerChannelSinkStack.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Channels {

	public interface IServerChannelSinkStack : IServerResponseChannelSinkStack
	{
		object Pop (IServerChannelSink sink);

		object Push (IServerChannelSink sink, object state);

		void ServerCallback (IAsyncResult ar);

		void Store (IServerChannelSink sink, object state);

		void StoreAndDispatch (IServerChannelSink sink, object state);
	}
}
