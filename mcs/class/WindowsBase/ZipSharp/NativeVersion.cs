using System;

namespace zipsharp {
	static class NativeVersion {

		/// <summary>
		/// ZipSharp code needs different code paths
		/// depending on the size of the C long type on the underlying platform. On
		/// gcc/clang the C long type follows the bitness of the targeted architecture,
		/// it's 32-bit on 32-bit systems and 64-bit on 64-bit systems. With the VS
		/// compiler however, the C long type is always 32-bit regardless of the
		/// target architecture. zlib and minizip uses C long in a number of 
		/// different function signatures and structs. 
		/// 
		/// This field is used to easily determine if the 32 bit version of 
		/// functions and structs should be used when interacting with zlib.
		/// </summary>
		public static readonly bool Use32Bit = IntPtr.Size == 4 || Environment.OSVersion.Platform != PlatformID.Unix;
	}
}
