//
// System.Runtime.Remoting.Channels.IChannelSender.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IChannelSender : IChannel
	{
		IMessageSink CreateMessageSink (string url, object remoteChannelData, out string objectURI);
	}
}
