#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct MethodToken {

		public static readonly MethodToken Empty = new MethodToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (MethodToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (MethodToken a, MethodToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (MethodToken a, MethodToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
