//
// System.Runtime.Remoting.Activation.ConstructionLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	public class ConstructionLevelActivator: IActivator
	{
		IActivator _next;

		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.Construction; }
		}

		public IActivator NextActivator 
		{
			get { return _next; }
			set { _next = value; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage msg)
		{
			// The StackBuilderSink at the end of the server context sink chain will do the job
			
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			if (identity.Context == null) identity.Context = Threading.Thread.CurrentContext;

			return (IConstructionReturnMessage) identity.Context.GetServerContextSinkChain().SyncProcessMessage (msg);
		}
	}
}
