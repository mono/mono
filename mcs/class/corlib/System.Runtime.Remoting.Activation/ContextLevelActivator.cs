//
// System.Runtime.Remoting.Activation.ContextLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	internal class ContextLevelActivator: IActivator
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
			ServerIdentity identity = RemotingServices.CreateContextBoundObjectIdentity (ctorCall.ActivationType);
			RemotingServices.SetMessageTargetIdentity (ctorCall, identity);

			ConstructionCall call = ctorCall as ConstructionCall;
			if (call == null || !call.IsContextOk)
			{
				identity.Context = Context.CreateNewContext (ctorCall);
				Context oldContext = Context.SwitchToContext (identity.Context);

				try
				{
					return _next.Activate (ctorCall);
				}
				finally
				{
					Context.SwitchToContext (oldContext);
				}
			}
			else
				return _next.Activate (ctorCall);
		}
	}
}
