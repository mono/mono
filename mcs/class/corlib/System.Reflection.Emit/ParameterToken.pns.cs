#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct ParameterToken {

		public static readonly ParameterToken Empty = new ParameterToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (ParameterToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (ParameterToken a, ParameterToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (ParameterToken a, ParameterToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
