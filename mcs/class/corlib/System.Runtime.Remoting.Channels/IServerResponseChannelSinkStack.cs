//
// System.Runtime.Remoting.Channels.IServerResponseChannelSinkStack.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IServerResponseChannelSinkStack
	{
		void AsyncProcessResponse (IMessage msg, ITransportHeaders headers, Stream stream);

		Stream GetResponseStream (IMessage msg, ITransportHeaders headers);
	}
}
