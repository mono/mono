namespace System.Runtime.Remoting
{
	public static class RemotingServices
	{
#region Keep this code, it is used by the runtime

		internal static object GetServerObject (string uri)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static byte[] SerializeCallData (object obj)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static object DeserializeCallData (byte[] array)
		{
			throw new PlatformNotSupportedException ();
		}

		internal static byte[] SerializeExceptionData (Exception ex)
		{
			throw new PlatformNotSupportedException ();
		}

#endregion
	}
}
