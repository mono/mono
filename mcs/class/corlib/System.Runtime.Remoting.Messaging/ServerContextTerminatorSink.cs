//
// System.Runtime.Remoting.ServerContextTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Messaging
{
	// The final sink of the Server Context Sink Chain.
	// It forwards the message to the object's context sink chain.

	public class ServerContextTerminatorSink: IMessageSink
	{
		public IMessage SyncProcessMessage (IMessage msg)
		{
			ServerIdentity identity = RemotingServices.GetMessageTargetIdentity (msg);
			return identity.SyncObjectProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			ServerIdentity identity = RemotingServices.GetMessageTargetIdentity (msg);
			return identity.AsyncObjectProcessMessage (msg, replySink);
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}
	}
}
