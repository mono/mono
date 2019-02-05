#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct SignatureToken {

		public static readonly SignatureToken Empty = new SignatureToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (SignatureToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (SignatureToken a, SignatureToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (SignatureToken a, SignatureToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
