//
// System.Runtime.Remoting.Channels.ClientChannelSinkStack.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels
{
	public class ClientChannelSinkStack : IClientChannelSinkStack, IClientResponseChannelSinkStack
	{
		// The sink where to send the result of the async call
		private IMessageSink _replySink = null;

		// The stack. It is a chain of ChanelSinkStackEntry.
		ChanelSinkStackEntry _sinkStack = null;

                [MonoTODO ("Initialize new instance with default values")]
                public ClientChannelSinkStack ()
                {
                }
		
		public ClientChannelSinkStack (IMessageSink sink)
		{
			_replySink = sink;
		}

		public void AsyncProcessResponse (ITransportHeaders headers, Stream stream)
		{
			if (_sinkStack == null) throw new RemotingException ("The current sink stack is empty");

			ChanelSinkStackEntry stackEntry = _sinkStack;
			_sinkStack = _sinkStack.Next;

			((IClientChannelSink)stackEntry.Sink).AsyncProcessResponse (this, stackEntry.State, headers, stream);

			// Do not call AsyncProcessResponse for each sink in the stack.
			// The sink must recursively call IClientChannelSinkStack.AsyncProcessResponse
			// after its own processing
		}

		[MonoTODO]
		public void DispatchException (Exception e)
		{
			throw new NotImplementedException ();
		}

		public void DispatchReplyMessage (IMessage msg)
		{
			if (_replySink != null) _replySink.SyncProcessMessage(msg);
		}

		public object Pop (IClientChannelSink sink)
		{
			// Pops until the sink is found

			while (_sinkStack != null)
			{
				ChanelSinkStackEntry stackEntry = _sinkStack;
				_sinkStack = _sinkStack.Next;
				if (stackEntry.Sink == sink) return stackEntry.State;
			}
			throw new RemotingException ("The current sink stack is empty, or the specified sink was never pushed onto the current stack");
		}

		public void Push (IClientChannelSink sink, object state)
		{
			_sinkStack = new ChanelSinkStackEntry (sink, state, _sinkStack);
		}
	}
}
