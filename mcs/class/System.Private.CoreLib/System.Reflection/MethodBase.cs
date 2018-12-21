namespace System.Reflection
{
	partial class MethodBase
	{
		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle) { throw null; }
		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle, RuntimeTypeHandle declaringType) { throw null; }

		public static MethodBase GetCurrentMethod() { throw null; }

		internal virtual ParameterInfo[] GetParametersNoCopy ()
		{
			throw new NotImplementedException ();
		}

		internal virtual ParameterInfo[] GetParametersInternal ()
		{
			throw new NotImplementedException ();
		}

		internal virtual int GetParametersCount ()
		{
			throw new NotImplementedException ();
		}

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle)
		{
			throw new NotImplementedException ();
		}
	}
}