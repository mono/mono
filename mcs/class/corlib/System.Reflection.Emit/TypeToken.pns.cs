#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct TypeToken {

		public static readonly TypeToken Empty = new TypeToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (TypeToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (TypeToken a, TypeToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (TypeToken a, TypeToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
