//
// Mono.Posix.Syscall.cs: System calls to Posix subsystem features
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
//
// (C) 2003 Novell, Inc.
//
// This file implements the low-level syscall interface to the POSIX
// subsystem.
//
// This file tries to stay close to the low-level API as much as possible
// using enumerations, structures and in a few cases, using existing .NET
// data types.
//
// Implementation notes:
//
//    Since the values for the various constants on the API changes
//    from system to system (even Linux on different architectures will
//    have different values), we define our own set of values, and we
//    use a set of C helper routines to map from the constants we define
//    to the values of the native OS.
//
//    Bitfields are flagged with the [Map] attribute, and a helper program
//    generates a set of map_XXXX routines that we can call to convert
//    from our value definitions to the value definitions expected by the
//    OS.
//
//    Methods that require tuning are bound as `internal syscal_NAME' methods
//    and then a `NAME' method is exposed.
//

using System;
using System.Text;
using System.Runtime.InteropServices;

[assembly:Mono.Posix.IncludeAttribute (new string [] {"sys/types.h", "sys/stat.h", "sys/wait.h", "unistd.h", "fcntl.h"},
				       new string [] {"_GNU_SOURCE"})]
namespace Mono.Posix {

	[Map][Flags]
	public enum OpenFlags {
		//
		// One of these
		//
		O_RDONLY    = 0,
		O_WRONLY    = 1,
		O_RDWR      = 2,

		//
		// Or-ed with zero or more of these
		//
		O_CREAT     = 4,
		O_EXCL      = 8,
		O_NOCTTY    = 16,
		O_TRUNC     = 32,
		O_APPEND    = 64,
		O_NONBLOCK  = 128,
		O_SYNC      = 256,

		//
		// These are non-Posix, think of a way of exposing
		// this for Linux users.
		//
		
		// O_NOFOLLOW  = 512,
		// O_DIRECTORY = 1024,
		// O_DIRECT    = 2048,
		// O_ASYNC     = 4096,
		// O_LARGEFILE = 8192
	}
	
	[Flags][Map]
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

	[Flags][Map]
	public enum WaitOptions {
		WNOHANG,
		WUNTRACED
	}

	[Flags][Map]
	public enum AccessMode {
		R_OK = 1,
		W_OK = 2,
		X_OK = 4,
		F_OK = 8
	}

	[Map]
	public enum Signals {
		SIGHUP, SIGINT, SIGQUIT, SIGILL, SIGTRAP, SIGABRT, SIGBUS,
		SIGFPE, SIGKILL, SIGUSR1, SIGSEGV, SIGUSR2, SIGPIPE,
		SIGALRM, SIGTERM, SIGCHLD, SIGCONT, SIGSTOP, SIGTSTP,
		SIGTTIN, SIGTTOU, SIGURG, SIGXCPU, SIGXFSZ, SIGVTALRM,
		SIGPROF, SIGWINCH, SIGIO,
		
		// SIGPWR,
		SIGSYS,
		// SIGRTMIN
	}
	
	public class Syscall {
		[DllImport ("libc", SetLastError=true)]
		public static extern int exit (int status);

		[DllImport ("libc", SetLastError=true)]
		public static extern int fork ();

		[DllImport ("libc", SetLastError=true)]
		public unsafe static extern IntPtr read (int fileDescriptor, void *buf, IntPtr count);

		[DllImport ("libc", SetLastError=true)]
		public unsafe static extern IntPtr write (int fileDescriptor, void *buf, IntPtr count);

		[DllImport ("libc", EntryPoint="open", SetLastError=true)]
		internal static extern int syscall_open (string pathname, int flags, int mode);

		[DllImport ("MonoPosixHelper")]
		internal extern static int map_Mono_Posix_OpenFlags (OpenFlags flags);
		[DllImport ("MonoPosixHelper")]
		internal extern static int map_Mono_Posix_FileMode (FileMode mode);
		
		public static int open (string pathname, OpenFlags flags)
		{
			if ((flags & OpenFlags.O_CREAT) != 0)
				throw new ArgumentException ("If you pass O_CREAT, you must call the method with the mode flag");
			
			int posix_flags = map_Mono_Posix_OpenFlags (flags);
			return syscall_open (pathname, posix_flags, 0);
		}

		public static int open (string pathname, OpenFlags flags, FileMode mode)
		{
			int posix_flags = map_Mono_Posix_OpenFlags (flags);
			int posix_mode = map_Mono_Posix_FileMode (mode);

			return syscall_open (pathname, posix_flags, posix_mode);
		}
		

		[DllImport ("libc", SetLastError=true)]
		public static extern int close (int fileDescriptor);

		[DllImport ("libc", EntryPoint="waitpid", SetLastError=true)]
		unsafe internal static extern int syscall_waitpid (int pid, int * status, int options);

		[DllImport ("MonoPosixHelper")]
		internal extern static int map_Mono_Posix_WaitOptions (WaitOptions wait_options);
		
		public static int waitpid (int pid, out int status, WaitOptions options)
		{
			unsafe {
				int s = 0;
				int r = syscall_waitpid (pid, &s, map_Mono_Posix_WaitOptions (options));
				status = s;
				return r;
			}
		}
		
		public static int waitpid (int pid, WaitOptions options)
		{
			unsafe {
				return syscall_waitpid (pid, null, map_Mono_Posix_WaitOptions (options));
			}
		}

		[DllImport ("MonoPosixHelper", EntryPoint="wifexited")]
		public static extern int WIFEXITED (int status);
		[DllImport ("MonoPosixHelper", EntryPoint="wexitstatus")]
		public static extern int WEXITSTATUS (int status);
		[DllImport ("MonoPosixHelper", EntryPoint="wifsignaled")]
		public static extern int WIFSIGNALED (int status);
		[DllImport ("MonoPosixHelper", EntryPoint="wtermsig")]
		public static extern int WTERMSIG (int status);
		[DllImport ("MonoPosixHelper", EntryPoint="wifstopped")]
		public static extern int WIFSTOPPED (int status);
		[DllImport ("MonoPosixHelper", EntryPoint="wstopsig")]
		public static extern int WSTOPSIG (int status);

		[DllImport ("libc", SetLastError=true)]
		internal static extern int syscall_creat (string pathname, int flags);

		public static int creat (string pathname, FileMode flags)
		{
			return syscall_creat (pathname, map_Mono_Posix_FileMode (flags));
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int link (string oldPath, string newPath);

		[DllImport ("libc", SetLastError=true)]
		public static extern int unlink (string path);

		// TODO: execve

		[DllImport ("libc", SetLastError=true)]
		public static extern int chdir (string path);

		// TODO: time
		// TODO: mknod

		
		[DllImport ("libc", EntryPoint="chmod", SetLastError=true)]
		internal static extern int syscall_chmod (string path, int mode);

		public static int chmod (string path, FileMode mode)
		{
			return syscall_chmod (path, map_Mono_Posix_FileMode (mode));
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int chown (string path, int owner, int group);
		[DllImport ("libc", SetLastError=true)]
		public static extern int lchown (string path, int owner, int group);
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int lseek (int fileDescriptor, int offset, int whence);

		[DllImport ("libc", SetLastError=true)]
		public static extern int getpid ();
		
		// TODO: mount
		// TODO: umount

		[DllImport ("libc", SetLastError=true)]
		public static extern int setuid (int uid);
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int getuid ();

		// TODO: stime
		// TODO: ptrace
		
		[DllImport ("libc")]
		public static extern uint alarm (uint seconds);

		[DllImport ("libc", SetLastError=true)]
		public static extern int pause ();

		// TODO: utime

		[DllImport ("libc")]
		internal extern static int syscall_access (string pathname, int mode);

		[DllImport ("MonoPosixHelper")]
		internal extern static int map_Mono_Posix_AccessMode (AccessMode mode);

		public static int access (string pathname, AccessMode mode)
		{
			return syscall_access (pathname, map_Mono_Posix_AccessMode (mode));
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int nice (int increment);

		// TODO: ftime

		[DllImport ("libc")]
		public static extern void sync ();

		[DllImport ("libc", SetLastError=true)]
		public static extern void kill (int pid, int sig);

		[DllImport ("libc", SetLastError=true)]
		public static extern int rename (string oldPath, string newPath);
		
		[DllImport ("libc", SetLastError=true)]
		internal extern static int syscall_mkdir (string pathname, int mode);

		public static int mkdir (string pathname, FileMode mode)
		{
			return syscall_mkdir (pathname, map_Mono_Posix_FileMode (mode));
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int rmdir (string path);

		[DllImport ("libc", SetLastError=true)]
		public static extern int dup (int fileDescriptor);

		// TODO: pipe
		// TODO: times

		[DllImport ("libc", SetLastError=true)]
		public static extern int setgid (int gid);
		[DllImport ("libc", SetLastError=true)]
		public static extern int getgid (int gid);

		
		public delegate void sighandler_t (int v);
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int signal (int signum, sighandler_t handler);
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int geteuid ();
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int getegid ();

		// TODO: fcntl
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int setpgid (int pid, int pgid);

		// TODO: ulimit

		[DllImport ("libc")]
		public static extern int umask (int umask);

		[DllImport ("libc", SetLastError=true)]
		public static extern int chroot (string path);

		[DllImport ("libc", SetLastError=true)]
		public static extern int dup2 (int oldFileDescriptor, int newFileDescriptor);

		[DllImport ("libc", SetLastError=true)]
		public static extern int getppid ();

		[DllImport ("libc", SetLastError=true)]
		public static extern int getpgrp ();

		[DllImport ("libc", SetLastError=true)]
		public static extern int setsid ();
		
		// TODO: sigaction
		
		[DllImport ("libc", SetLastError=true)]
		public static extern int setreuid (int ruid, int euid);

		[DllImport ("libc", SetLastError=true)]
		public static extern int setregid (int rgid, int egid);

		// TODO: sigsuspend
		// TODO: sigpending
		// TODO: setrlimit
		// TODO: getrlimit
		// TODO: getrusage
		// TODO: gettimeofday
		// TODO: settimeofday
		
		[DllImport ("libc", EntryPoint="gethostname", SetLastError=true)]
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

		public static string gethostname ()
		{
			return GetHostName ();
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

		[DllImport ("libc", EntryPoint="isatty")]
		static extern int syscall_isatty (int desc);
		
		public static bool isatty (int desc)
		{
			int res = syscall_isatty (desc);
			if (res == 1)
				return true;
			else
				return false;
		}
	}
}
