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

		public IMessage SyncProcessMessage (IMessage msg)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			identity.NotifyServerDynamicSinks (true, msg, false, false);
			IMessage res = _nextSink.SyncProcessMessage (msg);
			identity.NotifyServerDynamicSinks (false, msg, false, false);
			return res;
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			if (identity.HasServerDynamicSinks)
			{
				identity.NotifyServerDynamicSinks (true, msg, false, true);
				IMessage res = _nextSink.SyncProcessMessage (msg);
				replySink = new ServerObjectReplySink(identity, replySink);
			}
			return _nextSink.AsyncProcessMessage (msg, replySink);
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
