//
// System.Runtime.Remoting.Messaging.EnvoyTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class EnvoyTerminatorSink: IMessageSink
	{
		public static EnvoyTerminatorSink Instance = new EnvoyTerminatorSink();

		public IMessage SyncProcessMessage (IMessage msg)
		{
			return Thread.CurrentContext.GetClientContextSinkChain ().SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			return Thread.CurrentContext.GetClientContextSinkChain ().AsyncProcessMessage (msg, replySink);
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}
	}
}
