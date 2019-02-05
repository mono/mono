#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Serializable]
	public struct PropertyToken {

		public static readonly PropertyToken Empty = new PropertyToken ();

		public int Token {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public bool Equals (PropertyToken obj)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Equality (PropertyToken a, PropertyToken b)
		{
			throw new PlatformNotSupportedException ();
		}

		public static bool op_Inequality (PropertyToken a, PropertyToken b)
		{
			throw new PlatformNotSupportedException ();
		}
	}

}

#endif
