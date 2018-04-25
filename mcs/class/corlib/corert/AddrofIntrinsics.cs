
using System;

namespace System.Runtime.InteropServices
{
	internal static class AddrofIntrinsics
	{
		// This method is implemented elsewhere in the toolchain
		internal static IntPtr AddrOf<T>(T ftn)
		{
			return Marshal.GetFunctionPointerForDelegate<T>(ftn);
		}
	}
}