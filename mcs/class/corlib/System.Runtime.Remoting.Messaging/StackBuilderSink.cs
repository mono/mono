//
// System.Runtime.Remoting.StackBuilderSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;

namespace System.Runtime.Remoting.Messaging
{
	// Sink that calls the real method of the object

	public class StackBuilderSink: IMessageSink
	{
		public IMessage SyncProcessMessage (IMessage msg)
		{
			// Makes the real call to the object

			MarshalByRefObject obj = RemotingServices.GetServerForUri (((IMethodMessage)msg).Uri);
			//return RemotingServices.InternalExecuteMessage (obj, (IMethodCallMessage)msg);
			return null;
		}

		[MonoTODO]
		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			throw new NotImplementedException ();
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}
	}
}
