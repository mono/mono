//
// System.Runtime.Remoting.LeaseSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Lifetime
{
	// This sink updates lifetime information
	// about an object

	internal class LeaseSink: IMessageSink
	{
		IMessageSink _nextSink;

		public LeaseSink (IMessageSink nextSink)
		{
			_nextSink = nextSink;
		}

		[MonoTODO("Manage leases")]
		public IMessage SyncProcessMessage (IMessage msg)
		{
			return _nextSink.SyncProcessMessage (msg);
		}

		[MonoTODO("Manage leases")]
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
