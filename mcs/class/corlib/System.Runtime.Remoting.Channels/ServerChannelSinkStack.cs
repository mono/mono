//
// System.Runtime.Remoting.Channels.ServerChannelSinkStack.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Collections;

namespace System.Runtime.Remoting.Channels
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class ServerChannelSinkStack : IServerChannelSinkStack, IServerResponseChannelSinkStack
	{
		// The stack. It is a chain of ChanelSinkStackEntry.
		ChanelSinkStackEntry _sinkStack = null;

		public ServerChannelSinkStack ()
		{
		}

		public Stream GetResponseStream (IMessage msg, ITransportHeaders headers)
	    {
			if (_sinkStack == null) throw new RemotingException ("The sink stack is empty");
			return ((IServerChannelSink)_sinkStack.Sink).GetResponseStream (this, _sinkStack.State, msg, headers);
		}

		public object Pop (IServerChannelSink sink)
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

		public void Push (IServerChannelSink sink, object state)
		{
			_sinkStack = new ChanelSinkStackEntry (sink, state, _sinkStack);
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

		public void AsyncProcessResponse (IMessage msg, ITransportHeaders headers, Stream stream)
		{
			if (_sinkStack == null) throw new RemotingException ("The current sink stack is empty");

			ChanelSinkStackEntry stackEntry = _sinkStack;
			_sinkStack = _sinkStack.Next;
			((IServerChannelSink)stackEntry.Sink).AsyncProcessResponse (this, stackEntry.State, msg, headers, stream);

			// Do not call AsyncProcessResponse for each sink in the stack.
			// The sink must recursively call IServerChannelSinkStack.AsyncProcessResponse
			// after its own processing
		}
	}
}
