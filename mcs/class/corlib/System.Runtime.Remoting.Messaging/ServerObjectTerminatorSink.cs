//
// System.Runtime.Remoting.ServerObjectTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
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

		[MonoTODO("Invoke dynamic sinks")]
		public IMessage SyncProcessMessage (IMessage msg)
		{
			return _nextSink.SyncProcessMessage (msg);
		}

		[MonoTODO("Invoke dynamic sinks")]
		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			return _nextSink.AsyncProcessMessage (msg, replySink);
		}

		public IMessageSink NextSink 
		{ 
			get { return _nextSink; }
		}
	}
}
