//
// System.Runtime.Remoting.Channels.ClientChannelSinkStack.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public class ClientChannelSinkStack : IClientChannelSinkStack,
		IClientResponseChannelSinkStack
	{
		private IMessageSink replySink = null;
		
		public ClientChannelSinkStack ()
		{
		}

		public ClientChannelSinkStack (IMessageSink sink)
		{
			replySink = sink;
		}

		[MonoTODO]
		public void AsyncProcessResponse (ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DispatchException (Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DispatchReplyMessage (IMessage msg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Pop (IClientChannelSink sink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Push (IClientChannelSink sink, object state)
		{
			throw new NotImplementedException ();
		}
	}
}
