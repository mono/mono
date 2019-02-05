#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct FieldToken {

		public static readonly FieldToken Empty = new FieldToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (FieldToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (FieldToken a, FieldToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (FieldToken a, FieldToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
