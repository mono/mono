//
// System.Runtime.Remoting.Activation.ContextLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	public class ContextLevelActivator: IActivator
	{
		IActivator _next;

		public ContextLevelActivator (IActivator next)
		{
			_next = next;
		}

		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.Context; }
		}

		public IActivator NextActivator 
		{
			get { return _next; }
			set { _next = value; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage ctorCall)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (ctorCall);
			identity.Context = Context.CreateNewContext (ctorCall);

			Context oldContext = Context.SwitchToContext (identity.Context);
			try
			{
				IConstructionReturnMessage response = (IConstructionReturnMessage) _next.Activate (ctorCall);
				return response;
			}
			finally
			{
				Context.SwitchToContext (oldContext);
			}
		}
	}
}
