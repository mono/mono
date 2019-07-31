#if !MONO_FEATURE_SRE
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[Obsolete ("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
	[ComVisible (true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public sealed class UnmanagedMarshal {

		private UnmanagedMarshal () {}

		public UnmanagedType BaseType { get { throw new PlatformNotSupportedException (); } }
		public int ElementCount { get { throw new PlatformNotSupportedException (); } }
		public UnmanagedType GetUnmanagedType { get { throw new PlatformNotSupportedException (); } }
		public System.Guid IIDGuid { get { throw new PlatformNotSupportedException (); } }

		public static UnmanagedMarshal DefineByValArray (int elemCount) => throw new PlatformNotSupportedException ();
		public static UnmanagedMarshal DefineByValTStr (int elemCount) => throw new PlatformNotSupportedException ();
		public static UnmanagedMarshal DefineLPArray (UnmanagedType elemType) => throw new PlatformNotSupportedException ();
		public static UnmanagedMarshal DefineUnmanagedMarshal (UnmanagedType unmanagedType) => throw new PlatformNotSupportedException ();
	}
}

#endif