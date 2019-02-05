#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct StringToken {

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (StringToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (StringToken a, StringToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (StringToken a, StringToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
