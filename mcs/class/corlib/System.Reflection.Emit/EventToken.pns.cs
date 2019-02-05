#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct EventToken {

		public static readonly EventToken Empty = new EventToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (EventToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (EventToken a, EventToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (EventToken a, EventToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
