//
// System.Runtime.Remoting.Channels.ServerChannelSinkStack.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public class ServerChannelSinkStack : IServerChannelSinkStack,
		IServerResponseChannelSinkStack
	{
		public ServerChannelSinkStack ()
		{
		}

		[MonoTODO]
		public Stream GetResponseStream (IMessage msg,
						 ITransportHeaders headers)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Pop (IServerChannelSink sink)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Push (IServerChannelSink sink, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ServerCallback (IAsyncResult ar)
	        {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Store (IServerChannelSink sink, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void StoreAndDispatch (IServerChannelSink sink, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AsyncProcessResponse (IMessage msg, ITransportHeaders headers, Stream stream)
		{
			throw new NotImplementedException ();
		}
	}
}
