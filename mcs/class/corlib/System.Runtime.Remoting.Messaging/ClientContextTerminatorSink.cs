//
// System.Runtime.Remoting.Messaging.ClientContextTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Messaging
{
	public class ClientContextTerminatorSink: IMessageSink
	{
		public ClientContextTerminatorSink()
		{
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
			return identity.ChannelSink.SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
			return identity.ChannelSink.AsyncProcessMessage (msg, replySink);
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}	
	}
}
