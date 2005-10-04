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
//    Bitfields were flagged with the [Map] attribute, and a helper program
//    generates a set of map_XXXX routines that we can call to convert
//    from our value definitions to the value definitions expected by the
//    OS.
//
//    Methods that require tuning are bound as `internal syscal_NAME' methods
//    and then a `NAME' method is exposed.
//
// Deprecated Warning:
//
//    This class is deprecated, and exists only for backward compatibility.
//    Please use and maintain Mono.Unix.Native.Syscall.
//
//    The [Map] attributes have been removed.  The naming and methodology of
//    the mapping routines has changed.  The old map functions still exist in
//    MonoPosixHelper, but they will not be updated any further.
//    Consequently, there is little point in maintaining the [Map] attributes
//    in this file, as they would only bloat MonoPosixHelper.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.Posix {

	[Flags]
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.OpenFlags")]
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
	
	[Flags]
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.FilePermissions")]
	public enum FileMode {
		S_ISUID   = 2048,
		S_ISGID   = 1024,
		S_ISVTX   = 512,
		S_IRUSR   = 256,
		S_IWUSR   = 128,
		S_IXUSR   = 64,
		S_IRGRP   = 32,
		S_IWGRP   = 16,
		S_IXGRP   = 8,
		S_IROTH   = 4,
		S_IWOTH   = 2,
		S_IXOTH   = 1
	}

	[Flags]
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.WaitOptions")]
	public enum WaitOptions {
		WNOHANG,
		WUNTRACED
	}

	[Flags]
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.AccessModes")]
	public enum AccessMode {
		R_OK = 1,
		W_OK = 2,
		X_OK = 4,
		F_OK = 8
	}

	
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.Signum")]
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
	
	[CLSCompliant (false)]
	[Obsolete ("Use Mono.Unix.Native.Syscall.")]
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

		[DllImport ("libc", EntryPoint="creat", SetLastError=true)]
		internal static extern int syscall_creat (string pathname, int flags);

		public static int creat (string pathname, FileMode flags)
		{
			return syscall_creat (pathname, map_Mono_Posix_FileMode (flags));
		}

		[DllImport ("libc", SetLastError=true)]
		public static extern int link (string oldPath, string newPath);

		[DllImport ("libc", SetLastError=true)]
		public static extern int unlink (string path);

		[DllImport ("libc", SetLastError=true)]
		public static extern int symlink (string oldpath, string newpath);
		
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

		[DllImport ("libc", EntryPoint="access", SetLastError=true)]
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
		
		[DllImport ("libc", EntryPoint="mkdir", SetLastError=true)]
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
		public static extern int getgid ();

		
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

		// these don't exactly match POSIX, but it's a nice way to get user/group names
		
		[DllImport ("MonoPosixHelper", SetLastError=true)]
		private static extern string helper_Mono_Posix_GetUserName (int uid);

		[DllImport ("MonoPosixHelper", SetLastError=true)]
		private static extern string helper_Mono_Posix_GetGroupName (int gid);
		
		public static string getusername(int uid) { return helper_Mono_Posix_GetUserName(uid); }
		public static string getgroupname(int gid) { return helper_Mono_Posix_GetGroupName(gid); }
		
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

		[CLSCompliant (false)]
		public static string gethostname ()
		{
			return GetHostName ();
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
		

		[DllImport ("MonoPosixHelper")]
		internal extern static int helper_Mono_Posix_Stat (string filename, bool dereference,
			out int device, out int inode, out int mode,
			out int nlinks, out int uid, out int gid,
			out int rdev, out long size, out long blksize, out long blocks,
			out long atime, out long mtime, out long ctime);
		
		private static int stat2(string filename, bool dereference, out Stat stat) {
			int device, inode, mode;
			int nlinks, uid, gid, rdev;
			long size, blksize, blocks;
			long atime, mtime, ctime;

			int ret = helper_Mono_Posix_Stat(filename, dereference,
				out device, out inode, out mode,
				out nlinks, out uid, out gid,
				out rdev, out size, out blksize, out blocks,
				out atime, out mtime, out ctime);
				
			stat = new Stat(
				device, inode, mode,
				nlinks, uid, gid,
				rdev, size, blksize, blocks,
				atime, mtime, ctime);

			if (ret != 0) return ret;

			return 0;
		}

		public static int stat(string filename, out Stat stat) {
			return stat2(filename, false, out stat);
		}

		public static int lstat(string filename, out Stat stat) {
			return stat2(filename, true, out stat);
		}
		
		[DllImport ("libc")]
		private static extern int readlink(string path, byte[] buffer, int buflen);

		public static string readlink(string path) {
			byte[] buf = new byte[512];
			int ret = readlink(path, buf, buf.Length);
			if (ret == -1) return null;
			char[] cbuf = new char[512];
			int chars = System.Text.Encoding.Default.GetChars(buf, 0, ret, cbuf, 0);
			return new String(cbuf, 0, chars);
		}

		[DllImport ("libc", EntryPoint="strerror")]
		static extern IntPtr _strerror(int errnum);

		public static string strerror (int errnum)
		{
			return Marshal.PtrToStringAnsi (_strerror (errnum));
		}

		[DllImport ("libc")]
		public static extern IntPtr opendir(string path);

		[DllImport ("libc")]
		public static extern int closedir(IntPtr dir);
		
		[DllImport ("MonoPosixHelper", EntryPoint="helper_Mono_Posix_readdir")]
		public static extern string readdir(IntPtr dir);
		
	}
	
	[Obsolete ("Use Mono.Unix.Native.FilePermissions")]
	public enum StatModeMasks {
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFMT")]
		TypeMask = 0xF000, // bitmask for the file type bitfields
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_RWXU")]
		OwnerMask = 0x1C0, // mask for file owner permissions
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_RWXG")]
		GroupMask = 0x38, // mask for group permissions
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_RWXO")]
		OthersMask = 0x7, // mask for permissions for others (not in group)
	}
	
	[Flags]
	[Obsolete ("Use Mono.Unix.Native.FilePermissions")]
	public enum StatMode {
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFSOCK")]
		Socket = 0xC000, // socket
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFLNK")]
		SymLink = 0xA000, // symbolic link
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFREG")]
		Regular = 0x8000, // regular file
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFBLK")]
		BlockDevice = 0x6000, // block device
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFDIR")]
		Directory = 0x4000, // directory
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFCHR")]
		CharDevice = 0x2000, // character device
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IFIFO")]
		FIFO = 0x1000, // fifo
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_ISUID")]
		SUid = 0x800, // set UID bit
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_ISGID")]
		SGid = 0x400, // set GID bit
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_ISVTX")]
		Sticky = 0x200, // sticky bit
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IRUSR")]
		OwnerRead = 0x100, // owner has read permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IWUSR")]
		OwnerWrite = 0x80, // owner has write permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IXUSR")]
		OwnerExecute = 0x40, // owner has execute permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IRGRP")]
		GroupRead = 0x20, // group has read permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IWGRP")]
		GroupWrite = 0x10, // group has write permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IXGRP")]
		GroupExecute = 0x8, // group has execute permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IROTH")]
		OthersRead = 0x4, // others have read permission
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IWOTH")]
		OthersWrite = 0x2, // others have write permisson
		[Obsolete ("Use Mono.Unix.Native.FilePermissions.S_IXOTH")]
		OthersExecute = 0x1, // others have execute permission	
	}
	
	[Obsolete ("Use Mono.Unix.Native.Stat")]
	public struct Stat {
		[Obsolete ("Use Mono.Unix.Native.Stat.st_dev")]
		public readonly int Device;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_ino")]
		public readonly int INode;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_mode")]
		public readonly StatMode Mode;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_nlink")]
		public readonly int NLinks;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_uid")]
		public readonly int Uid;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_gid")]
		public readonly int Gid;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_rdev")]
		public readonly long DeviceType;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_size")]
		public readonly long Size;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_blksize")]
		public readonly long BlockSize;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_blocks")]
		public readonly long Blocks;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_atime")]
		public readonly DateTime ATime;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_mtime")]
		public readonly DateTime MTime;
		[Obsolete ("Use Mono.Unix.Native.Stat.st_ctime")]
		public readonly DateTime CTime;
		
		[Obsolete ("Use Mono.Unix.Native.NativeConvert.LocalUnixEpoch")]
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);
		
		[Obsolete ("Use Mono.Unix.Native.NativeConvert.ToDateTime")]
		public static DateTime UnixToDateTime(long unix) {
			return UnixEpoch.Add(TimeSpan.FromSeconds(unix)).ToLocalTime();
		}

		internal Stat(
			int device, int inode, int mode,
			int nlinks, int uid, int gid,
			int rdev, long size, long blksize, long blocks,
			long atime, long mtime, long ctime) {
			Device = device;
			INode = inode;
			Mode = (StatMode)mode;
			NLinks = nlinks;
			Uid = uid;
			Gid = gid;
			DeviceType = rdev;
			Size = size;
			BlockSize = blksize;
			Blocks = blocks;
			if (atime != 0)
				ATime = UnixToDateTime(atime);
			else
				ATime = new DateTime();
			if (mtime != 0)
				MTime = UnixToDateTime(mtime);
			else
				MTime = new DateTime();
			if (ctime != 0)
				CTime = UnixToDateTime(ctime);
			else
				CTime = new DateTime();
		}
	}
	

}
