//
// System.Runtime.Remoting.Channels.IClientChannelSinkProvider.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IClientChannelSinkProvider
	{
		IClientChannelSinkProvider Next { get;  set; }

		IClientChannelSink CreateSink (IChannelSender channel,  string url, object remoteChannelData);
	}
}
