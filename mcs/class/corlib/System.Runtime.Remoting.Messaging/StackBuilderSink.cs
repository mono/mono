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
		MarshalByRefObject _target;

		public StackBuilderSink (MarshalByRefObject obj)
		{
			_target = obj;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			// Makes the real call to the object
			return RemotingServices.InternalExecuteMessage (_target, (IMethodCallMessage)msg);
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
