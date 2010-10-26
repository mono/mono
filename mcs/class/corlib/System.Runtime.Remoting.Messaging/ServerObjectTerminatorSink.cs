//
// System.Runtime.Remoting.ServerObjectTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
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

using System;

namespace System.Runtime.Remoting.Messaging
{
	// The final sink of the Server Object Sink Chain.
	// It invokes object dynamic sinks and forwards the message 
	// to the StackBuilderSink

	internal class ServerObjectTerminatorSink: IMessageSink
	{
		IMessageSink _nextSink;

		public ServerObjectTerminatorSink(IMessageSink nextSink)
		{
			_nextSink = nextSink;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			#if !DISABLE_REMOTING
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			identity.NotifyServerDynamicSinks (true, msg, false, false);
			IMessage res = _nextSink.SyncProcessMessage (msg);
			identity.NotifyServerDynamicSinks (false, msg, false, false);
			return res;
			#else
			IMessage res = _nextSink.SyncProcessMessage (msg);
			return res;
			#endif
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			#if !DISABLE_REMOTING
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			if (identity.HasServerDynamicSinks)
			{
				identity.NotifyServerDynamicSinks (true, msg, false, true);
				if (replySink != null) replySink = new ServerObjectReplySink(identity, replySink);
			}
			
			IMessageCtrl res = _nextSink.AsyncProcessMessage (msg, replySink);

			if (replySink == null)
				identity.NotifyServerDynamicSinks (false, msg, true, true);

			return res;
			#else
			IMessageCtrl res = _nextSink.AsyncProcessMessage (msg, replySink);
			return res;
			#endif
		}

		public IMessageSink NextSink 
		{ 
			get { return _nextSink; }
		}
	}

	class ServerObjectReplySink: IMessageSink
	{
		IMessageSink _replySink;
		ServerIdentity _identity;

		public ServerObjectReplySink (ServerIdentity identity, IMessageSink replySink)
		{
			_replySink = replySink;
			_identity = identity;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			_identity.NotifyServerDynamicSinks (false, msg, true, true);
			return _replySink.SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			throw new NotSupportedException ();
		}

		public IMessageSink NextSink 
		{ 
			get { return _replySink; }
		}
	}
}
