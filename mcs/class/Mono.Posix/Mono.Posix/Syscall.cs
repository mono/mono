//
// Mono.Posix.Syscall.cs: System calls to Posix subsystem features
//

using System;
using System.Runtime.InteropServices;

namespace Mono.Posix {

	public unsafe class Syscall {
		[DllImport ("libc", EntryPoint="gethostname")]
		static unsafe extern int syscall_gethostname (byte *p, int len);

		public static string GetHostName ()
		{
			byte [] buf = new byte [256];
			unsafe {
				fixed (byte *p = &buf [0]){
					gethostname (p, 256);
				}
			}
			return new String(Encoding.UTF8.GetChars (buf));
		}
	}
}
