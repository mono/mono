//
// System.Runtime.Remoting.RemoteActivator.cs
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
	internal class RemoteActivator: MarshalByRefObject, IActivator
	{
		public IConstructionReturnMessage Activate (IConstructionCallMessage msg)
		{
			if (!RemotingConfiguration.IsActivationAllowed (msg.ActivationType))
				throw new RemotingException ("The type " + msg.ActivationTypeName + " is not allowed to be client activated");

			object[] activationAttributes = new object[] { new RemoteActivationAttribute (msg.ContextProperties) };
			MarshalByRefObject newObject = (MarshalByRefObject) Activator.CreateInstance (msg.ActivationType, msg.Args, activationAttributes);

			// The activator must return a ConstructionResponse with an ObjRef as return value.
			// It avoids the automatic creation of a proxy in the client.

			ObjRef objref = RemotingServices.Marshal (newObject);
			return new ConstructionResponse (objref, null, msg);
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

		public ActivatorLevel Level 
		{
			get { throw new NotSupportedException (); }
		}

		public IActivator NextActivator 
		{
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
}
