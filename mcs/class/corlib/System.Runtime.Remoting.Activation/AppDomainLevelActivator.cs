//
// System.Runtime.Remoting.Activation.AppDomainLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
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
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation
{
	internal class AppDomainLevelActivator: IActivator
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
			#if !DISABLE_REMOTING
			IConstructionReturnMessage response;

			// Create the object by calling the remote activation service

			IActivator remoteActivator = (IActivator) RemotingServices.Connect (typeof (IActivator), _activationUrl);
			ctorCall.Activator = ctorCall.Activator.NextActivator;

			try
			{
				response = remoteActivator.Activate (ctorCall);
			}
			catch (Exception ex)
			{
				return new ConstructionResponse (ex, ctorCall);
			}

			// Create the client identity for the remote object

			ObjRef objRef = (ObjRef) response.ReturnValue;
			if (RemotingServices.GetIdentityForUri (objRef.URI) != null)
				throw new RemotingException("Inconsistent state during activation; there may be two proxies for the same object");

			object proxy;
			
			// We pass null for proxyType because we don't really to attach the identity
			// to a proxy, we already have one.
			Identity identity = RemotingServices.GetOrCreateClientIdentity (objRef, null, out proxy);
			RemotingServices.SetMessageTargetIdentity (ctorCall, identity);
			return response;
			#else
			return null;
			#endif
		}
	}
}
