//
// System.Runtime.Remoting.StackBuilderSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Reflection;

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
			CheckParameters (msg);

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

		void CheckParameters (IMessage msg)
		{
			IMethodCallMessage mcm = (IMethodCallMessage) msg;
			
			MethodInfo mi = (MethodInfo) mcm.MethodBase;

			ParameterInfo[] parameters = mi.GetParameters();
			int narg = 0;

			foreach (ParameterInfo pi in parameters)
			{
				object pval = mcm.GetArg (narg++);
				Type pt = pi.ParameterType;
				if (pt.IsByRef) pt = pt.GetElementType ();
				
				if (pval != null && !pt.IsInstanceOfType (pval))
					throw new RemotingException ("Cannot cast argument of type '" + pval.GetType().AssemblyQualifiedName +
						"' to type '" + pt.AssemblyQualifiedName + "'");
			}
		}
	}
}
