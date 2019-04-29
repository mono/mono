
using System;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Activation
{
	internal class ActivationServices
	{
		// Called from the runtime.
		public static object CreateProxyForType (Type type)
		{
			throw new PlatformNotSupportedException ();
		}

		// Allocates an uninitialized instance. It never creates proxies.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern object AllocateUninitializedClassInstance (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void EnableProxyActivation (Type type, bool enable);
	}
}
