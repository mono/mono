//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation
{
	public class RemoteActivator: MarshalByRefObject
	{
		public IConstructionReturnMessage Activate (IConstructionCallMessage msg)
		{
			if (!RemotingConfiguration.IsActivationAllowed (msg.ActivationType))
				throw new RemotingException ("The type " + msg.ActivationTypeName + " is not allowed to be client activated");

			return msg.Activator.Activate (msg);
		}

		public override Object InitializeLifetimeService()
		{
			ILease lease = (ILease)base.InitializeLifetimeService();
			if (lease.CurrentState == LeaseState.Initial)
			{
				lease.InitialLeaseTime = TimeSpan.FromMinutes(30);
				lease.SponsorshipTimeout = TimeSpan.FromMinutes(1);
				lease.RenewOnCallTime = TimeSpan.FromMinutes(10);
			}
			return lease;
		}
	}
}
