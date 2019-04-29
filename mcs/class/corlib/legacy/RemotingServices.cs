using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting
{
	public static class RemotingServices
	{
#region Keep this code, it is used by the runtime

		// This method is called by the runtime
		internal static object GetServerObject (string uri)
		{
			throw new PlatformNotSupportedException ();
		}

		// This method is called by the runtime
		internal static byte[] SerializeCallData (object obj)
		{
			throw new PlatformNotSupportedException ();
		}

		// This method is called by the runtime
		internal static object DeserializeCallData (byte[] array)
		{
			throw new PlatformNotSupportedException ();
		}

		// This method is called by the runtime
		internal static byte[] SerializeExceptionData (Exception ex)
		{
			throw new PlatformNotSupportedException ();
		}

#endregion

#if !FIXME
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static object InternalExecute (MethodBase method, Object obj,
							       Object[] parameters, out object [] out_args);

		// Returns the actual implementation of @method in @type.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static MethodBase GetVirtualMethod (Type type, MethodBase method);

		public static bool IsTransparentProxy (object proxy)
		{
			throw new NotSupportedException ();
		}

		internal static IMethodReturnMessage InternalExecuteMessage (
		        MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			throw new PlatformNotSupportedException ();
		}

		public static IMethodReturnMessage ExecuteMessage (
			MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			throw new PlatformNotSupportedException ();
		}

		[System.Runtime.InteropServices.ComVisible (true)]
		public static object Connect (Type classToProxy, string url)
		{
			throw new PlatformNotSupportedException ();
		}

		[System.Runtime.InteropServices.ComVisible (true)]
		public static object Connect (Type classToProxy, string url, object data)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool Disconnect (MarshalByRefObject obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static Type GetServerTypeForUri (string URI)
		{
			throw new PlatformNotSupportedException ();
		}

		public static string GetObjectUri (MarshalByRefObject obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static RealProxy GetRealProxy (object proxy)
		{
			throw new PlatformNotSupportedException ();
		}

		public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static MethodBase GetMethodBaseFromName (Type type, string methodName, Type[] signature)
		{
			throw new PlatformNotSupportedException ();
		}

		static MethodBase FindInterfaceMethod (Type type, string methodName, Type[] signature)
		{
			throw new PlatformNotSupportedException ();
		}

		public static void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			throw new PlatformNotSupportedException ();
		}

		public static object GetLifetimeService (MarshalByRefObject obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static IMessageSink GetEnvoyChainForProxy (MarshalByRefObject obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static string GetSessionIdForMethodMessage(IMethodMessage msg)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool IsMethodOverloaded(IMethodMessage msg)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool IsObjectOutOfAppDomain(object tp)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool IsObjectOutOfContext(object tp)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool IsOneWay(MethodBase method)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static bool IsAsyncMessage(IMessage msg)
		{
			throw new PlatformNotSupportedException ();
		}

		public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
		{
			throw new PlatformNotSupportedException ();
		}

		#region Internal Methods

		internal static object CreateClientProxy (Type objectType, string url, object[] activationAttributes)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static object CreateClientProxyForContextBound (Type type, object[] activationAttributes)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static object GetDomainProxy(AppDomain domain)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static bool UpdateOutArgObject (ParameterInfo pi, object local, object remote)
		{
			throw new PlatformNotSupportedException ();
		}

		#endregion
		#endif
	}
}
