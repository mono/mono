//
// System.Runtime.Remoting.Channels.IClientResponseChannelSinkStack.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels {

	public interface IClientResponseChannelSinkStack
	{
		void AsyncProcessResponse (ITransportHeaders headers, Stream stream);

		void DispatchException (Exception e);

		void DispatchReplyMessage (IMessage msg);
	}
}
