//
// System.Runtime.Remoting.Activation.AppDomainLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation
{
	public class AppDomainLevelActivator: IActivator
	{
		string _activationUrl;
		IActivator _next;

		public AppDomainLevelActivator(string activationUrl, IActivator next)
		{
			_activationUrl = activationUrl;
			_next = next;
		}

		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.AppDomain; }
		}

		public IActivator NextActivator 
		{
			get { return _next; }
			set { _next = value; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage ctorCall)
		{
			IConstructionReturnMessage response;

			// Create the object by calling the remote activation service

			RemoteActivator remoteActivator = (RemoteActivator) RemotingServices.Connect (typeof (RemoteActivator), _activationUrl);
			ctorCall.Activator = ctorCall.Activator.NextActivator;

			response = remoteActivator.Activate (ctorCall);

			// Create the client identity for the remote object

			ObjRef objRef = (ObjRef) response.ReturnValue;
			if (RemotingServices.GetIdentityForUri (objRef.URI) != null)
				throw new RemotingException("Inconsistent state during activation; there may be two proxies for the same object");

			Identity identity = RemotingServices.GetOrCreateClientIdentity (objRef, ctorCall.ActivationType);
			RemotingServices.SetMessageTargetIdentity (ctorCall, identity);
			return response;
		}
	}
}
