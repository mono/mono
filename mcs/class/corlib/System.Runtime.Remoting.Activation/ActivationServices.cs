//
// System.Runtime.Remoting.ActivationServices.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Activation
{
	internal class ActivationServices
	{
		public static IMessage CreateInstanceFromMessage (IConstructionCallMessage ctorCall)
		{
			object obj = AllocateUninitializedClassInstance (ctorCall.ActivationType);
			ctorCall.MethodBase.Invoke (obj, ctorCall.Args);

			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (ctorCall);

//			FIXME: restore when Thread.CurrentContext workds
//			identity.AttachServerObject ((MarshalByRefObject) obj, Threading.Thread.CurrentContext);
			identity.AttachServerObject ((MarshalByRefObject) obj, System.Runtime.Remoting.Contexts.Context.DefaultContext);

			return new ConstructionResponse (obj, null, ctorCall);
		}

		internal static object CreateProxyForType (Type type)
		{
			// Called by the runtime when creating an instance of a type
			// that has been registered as remotely activated.

			// First of all check for remote activation. If the object is not remote, then
			// it may be contextbound.

			ActivatedClientTypeEntry activatedEntry = RemotingConfiguration.IsRemotelyActivatedClientType (type);
			if (activatedEntry != null)
				return RemotingServices.CreateClientProxy (activatedEntry);

			WellKnownClientTypeEntry wellknownEntry = RemotingConfiguration.IsWellKnownClientType (type);
			if (wellknownEntry != null)
				return RemotingServices.CreateClientProxy (wellknownEntry);

			if (type.IsContextful)
				return RemotingServices.CreateClientProxyForContextBound (type);

			return null;
		}

		// Allocates an uninitialized instance. It never creates proxies.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern object AllocateUninitializedClassInstance (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void EnableProxyActivation (Type type, bool enable);
	}
}
