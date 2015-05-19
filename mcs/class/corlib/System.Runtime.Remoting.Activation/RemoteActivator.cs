//
// System.Runtime.Remoting.RemoteActivator.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

			object[] activationAttributes = null;
			if (msg.ActivationType.IsContextful)
				activationAttributes = new object[] { new RemoteActivationAttribute (msg.ContextProperties) };

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
