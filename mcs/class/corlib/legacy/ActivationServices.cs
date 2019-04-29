
using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Activation
{
	internal class ActivationServices
	{
		public static IMessage RemoteActivate (IConstructionCallMessage ctorCall)
		{
			throw new PlatformNotSupportedException ();
		}

		public static IMessage CreateInstanceFromMessage (IConstructionCallMessage ctorCall)
		{
			throw new PlatformNotSupportedException ();
		}

		public static object CreateProxyForType (Type type)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static void PushActivationAttributes (Type serverType, Object[] attributes)
		{
			// TODO:
		}

		internal static void PopActivationAttributes (Type serverType)
		{
			// TODO:
		}

		// Allocates an uninitialized instance. It never creates proxies.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object AllocateUninitializedClassInstance (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void EnableProxyActivation (Type type, bool enable);
	}
}
