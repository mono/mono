//
// Mono.Posix.Syscall.cs: System calls to Posix subsystem features
//

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.Posix {

	public class Syscall {
		[DllImport ("libc", EntryPoint="gethostname")]
		static extern int syscall_gethostname (byte[] p, int len);

		public static string GetHostName ()
		{
			byte [] buf = new byte [256];
			int res = syscall_gethostname (buf, buf.Length);
			if (res == -1)
				return "localhost";
			for (res = 0; res < buf.Length; ++res) {
				if (buf [res] == 0)
					break;
			}
				
			return Encoding.UTF8.GetString (buf, 0, res);
		}
	}
}
