//
// System.Runtime.Remoting.ActivationServices.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Activation
{
	internal class ActivationServices
	{
		static IActivator _constructionActivator = new ConstructionLevelActivator ();

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
			ctorCall.Activator = _constructionActivator;

			if (!type.IsContextful) return ctorCall;

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
				// A new context is needed. Collect the context properties and set
				// the context level activator.

				ctorCall.SetActivationAttributes (attributes.ToArray());
				ctorCall.Activator = new ContextLevelActivator (ctorCall.Activator);

				foreach (IContextAttribute attr in attributes)
					attr.GetPropertiesForNewContext (ctorCall);
			}

			return ctorCall;
		}

		public static IMessage CreateInstanceFromMessage (IConstructionCallMessage ctorCall)
		{
			object obj = AllocateUninitializedClassInstance (ctorCall.ActivationType);
			ctorCall.MethodBase.Invoke (obj, ctorCall.Args);

			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (ctorCall);

			identity.AttachServerObject ((MarshalByRefObject) obj, Threading.Thread.CurrentContext);

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

			return null;
		}

		// Allocates an uninitialized instance. It never creates proxies.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern object AllocateUninitializedClassInstance (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void EnableProxyActivation (Type type, bool enable);
	}
}
