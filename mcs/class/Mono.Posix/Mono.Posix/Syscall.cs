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

		[Flags]
		public enum FileMode {
			S_ISUID   = 04000,
			S_ISGID   = 02000,
			S_ISVTX   = 01000,
			S_IRUSR   = 00400,
			S_IWUSR   = 00200,
			S_IXUSR   = 00100,
			S_IRGRP   = 00040,
			S_IWGRP   = 00020,
			S_IXGRP   = 00010,
			S_IROTH   = 00004,
			S_IWOTH   = 00002,
			S_IXOTH   = 00001
		}
		
		public static int chmod (string file, FileMode mode)
	}
}
