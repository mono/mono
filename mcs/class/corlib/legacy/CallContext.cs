namespace System.Runtime.Remoting.Messaging
{
#region Keep this code, it is used by nunit
	public interface ILogicalThreadAffinative
	{
	}
#endregion

	class LogicalCallContext
	{
		LogicalCallContext ()
		{
			throw new PlatformNotSupportedException ();
		}
	}

	class CallContext
	{
		CallContext ()
		{
			throw new PlatformNotSupportedException ();
		}

#region Keep this code, it is used by the runtime
		internal static object SetCurrentCallContext (LogicalCallContext ctx)
		{
			throw new PlatformNotSupportedException ();
		}
#endregion

#region Keep this code, it is used by nunit
		public static void SetData (String name, Object data)
		{
			throw new PlatformNotSupportedException ();
		}
#endregion
	}
}
