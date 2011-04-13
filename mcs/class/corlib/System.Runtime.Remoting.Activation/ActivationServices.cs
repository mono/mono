//
// System.Runtime.Remoting.ActivationServices.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
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
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Proxies;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Activation
{
	internal class ActivationServices
	{
		static IActivator _constructionActivator;

		static IActivator ConstructionActivator {
			get {
				if (_constructionActivator == null)
					_constructionActivator = new ConstructionLevelActivator ();

				return _constructionActivator;
			}
		}
#if !MOONLIGHT
		public static IMessage Activate (RemotingProxy proxy, ConstructionCall ctorCall)
		{
			IMessage response;
			ctorCall.SourceProxy = proxy;

			if (Thread.CurrentContext.HasExitSinks && !ctorCall.IsContextOk)
				response = Thread.CurrentContext.GetClientContextSinkChain ().SyncProcessMessage (ctorCall);
			else
				response = RemoteActivate (ctorCall);

			if (response is IConstructionReturnMessage && ((IConstructionReturnMessage)response).Exception == null && proxy.ObjectIdentity == null)
			{
				Identity identity = RemotingServices.GetMessageTargetIdentity (ctorCall);
				proxy.AttachIdentity (identity);
			}

			return response;
		}
#endif
		public static IMessage RemoteActivate (IConstructionCallMessage ctorCall)
		{
			try 
			{
				return ctorCall.Activator.Activate (ctorCall);
			}
			catch (Exception ex) 
			{
				return new ReturnMessage (ex, ctorCall);
			}		
		}

		public static object CreateProxyFromAttributes (Type type, object[] activationAttributes)
		{
			string activationUrl = null;
			foreach (object attr in activationAttributes)
			{
				if (!(attr is IContextAttribute)) throw new RemotingException ("Activation attribute does not implement the IContextAttribute interface");
				if (attr is UrlAttribute) activationUrl = ((UrlAttribute)attr).UrlValue;
			}

			if (activationUrl != null)
				return RemotingServices.CreateClientProxy (type, activationUrl, activationAttributes);

			ActivatedClientTypeEntry activatedEntry = RemotingConfiguration.IsRemotelyActivatedClientType (type);
			if (activatedEntry != null)
				return RemotingServices.CreateClientProxy (activatedEntry, activationAttributes);

			if (type.IsContextful)
				return RemotingServices.CreateClientProxyForContextBound (type, activationAttributes);
			
			return null;
		}

		public static ConstructionCall CreateConstructionCall (Type type, string activationUrl, object[] activationAttributes)
		{
			ConstructionCall ctorCall = new ConstructionCall (type);

			if (!type.IsContextful) 
			{
				// Must be a remote activated object
				ctorCall.Activator = new AppDomainLevelActivator (activationUrl, ConstructionActivator);
				ctorCall.IsContextOk = false;	// It'll be activated in a remote context
				return ctorCall;
			}

			// It is a CBO. Need collect context properties and
			// check if a new context is needed.

			IActivator activatorChain = ConstructionActivator;
			activatorChain = new ContextLevelActivator (activatorChain);

			ArrayList attributes = new ArrayList ();
			if (activationAttributes != null) attributes.AddRange (activationAttributes);

			bool isContextOk = (activationUrl == ChannelServices.CrossContextUrl);	// Remote CBOs are always created in a new context
			Context currentContext = Threading.Thread.CurrentContext;

			if (isContextOk) 
			{
				foreach (IContextAttribute attr in attributes) 
				{
					if (!attr.IsContextOK (currentContext, ctorCall)) 
					{
						isContextOk = false;
						break;
					}
				}
			}

			object[] typeAttributes = type.GetCustomAttributes (true);
			foreach (object attr in typeAttributes) 
			{
				if (attr is IContextAttribute) 
				{
					isContextOk = isContextOk && ((IContextAttribute)attr).IsContextOK (currentContext, ctorCall);
					attributes.Add (attr);
				}
			}

			if (!isContextOk)
			{
				// A new context is needed. Collect the context properties and chain
				// the context level activator.

				ctorCall.SetActivationAttributes (attributes.ToArray());

				foreach (IContextAttribute attr in attributes)
					attr.GetPropertiesForNewContext (ctorCall);
			}

			if (activationUrl != ChannelServices.CrossContextUrl)
				activatorChain = new AppDomainLevelActivator (activationUrl, activatorChain);
			
			ctorCall.Activator = activatorChain;
			ctorCall.IsContextOk = isContextOk;

			return ctorCall;
		}

		public static IMessage CreateInstanceFromMessage (IConstructionCallMessage ctorCall)
		{
			object obj = AllocateUninitializedClassInstance (ctorCall.ActivationType);

			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (ctorCall);
			identity.AttachServerObject ((MarshalByRefObject) obj, Threading.Thread.CurrentContext);

			ConstructionCall call = ctorCall as ConstructionCall;
#if !MOONLIGHT
			if (ctorCall.ActivationType.IsContextful && call != null && call.SourceProxy != null)
			{
				call.SourceProxy.AttachIdentity (identity);
				MarshalByRefObject target = (MarshalByRefObject) call.SourceProxy.GetTransparentProxy ();
				RemotingServices.InternalExecuteMessage (target, ctorCall);
			}
			else
#endif
				ctorCall.MethodBase.Invoke (obj, ctorCall.Args);

			return new ConstructionResponse (obj, null, ctorCall);
		}

		public static object CreateProxyForType (Type type)
		{
			// Called by the runtime when creating an instance of a type
			// that has been registered as remotely activated.

			// First of all check for remote activation. If the object is not remote, then
			// it may be contextbound.

			ActivatedClientTypeEntry activatedEntry = RemotingConfiguration.IsRemotelyActivatedClientType (type);
			if (activatedEntry != null)
				return RemotingServices.CreateClientProxy (activatedEntry, null);

			WellKnownClientTypeEntry wellknownEntry = RemotingConfiguration.IsWellKnownClientType (type);
			if (wellknownEntry != null)
				return RemotingServices.CreateClientProxy (wellknownEntry);

			if (type.IsContextful)
				return RemotingServices.CreateClientProxyForContextBound (type, null);
#if !NET_2_1
			if (type.IsCOMObject) {
				return RemotingServices.CreateClientProxyForComInterop (type);
			}
#endif
			return null;
		}

		// Allocates an uninitialized instance. It never creates proxies.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object AllocateUninitializedClassInstance (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void EnableProxyActivation (Type type, bool enable);
	}
}
