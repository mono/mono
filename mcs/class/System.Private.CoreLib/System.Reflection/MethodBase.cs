using System.Runtime.CompilerServices;

namespace System.Reflection
{
	partial class MethodBase
	{
		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle)
		{
			if (handle.IsNullHandle ())
				throw new ArgumentException (Environment.GetResourceString("Argument_InvalidHandle"));

			MethodBase m = RuntimeMethodInfo.GetMethodFromHandleInternalType (handle.Value, IntPtr.Zero);
			if (m == null)
				throw new ArgumentException ("The handle is invalid.");

			Type declaringType = m.DeclaringType;
			if (declaringType != null && declaringType.IsGenericType)
				throw new ArgumentException (String.Format (Environment.GetResourceString ("Argument_MethodDeclaringTypeGeneric"),
															m, declaringType.GetGenericTypeDefinition ()));

			return m;
		}

		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.IsNullHandle ())
				throw new ArgumentException (Environment.GetResourceString("Argument_InvalidHandle"));
			MethodBase m = RuntimeMethodInfo.GetMethodFromHandleInternalType (handle.Value, declaringType.Value);
			if (m == null)
				throw new ArgumentException ("The handle is invalid.");
			return m;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static MethodBase GetCurrentMethod ();

		internal virtual ParameterInfo[] GetParametersNoCopy ()
		{
			return GetParametersInternal ();
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