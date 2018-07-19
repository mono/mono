
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{
	static class NativeMethods
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern int GetCurrentProcessId ();
	}
}