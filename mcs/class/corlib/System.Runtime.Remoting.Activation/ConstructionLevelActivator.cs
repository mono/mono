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
	internal class ConstructionLevelActivator: IActivator
	{
		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.Construction; }
		}

		public IActivator NextActivator 
		{
			get { return null; }
			set { ; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage msg)
		{
			// The StackBuilderSink at the end of the server context sink chain will do the job
			return (IConstructionReturnMessage) Threading.Thread.CurrentContext.GetServerContextSinkChain().SyncProcessMessage (msg);
		}
	}
}
