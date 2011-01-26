//
// Mono.Unix/Stdlib.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2006 Jonathan Pryor
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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix.Native;

namespace Mono.Unix.Native {

	#region Enumerations

	[Map]
	public enum Errno : int {
		// errors & their values liberally copied from
		// FC2 /usr/include/asm/errno.h
		
		EPERM           =   1, // Operation not permitted 
		ENOENT          =   2, // No such file or directory 
		ESRCH           =   3, // No such process 
		EINTR           =   4, // Interrupted system call 
		EIO             =   5, // I/O error 
		ENXIO           =   6, // No such device or address 
		E2BIG           =   7, // Arg list too long 
		ENOEXEC         =   8, // Exec format error 
		EBADF           =   9, // Bad file number 
		ECHILD          =  10, // No child processes 
		EAGAIN          =  11, // Try again 
		ENOMEM          =  12, // Out of memory 
		EACCES          =  13, // Permission denied 
		EFAULT          =  14, // Bad address 
		ENOTBLK         =  15, // Block device required 
		EBUSY           =  16, // Device or resource busy 
		EEXIST          =  17, // File exists 
		EXDEV           =  18, // Cross-device link 
		ENODEV          =  19, // No such device 
		ENOTDIR         =  20, // Not a directory 
		EISDIR          =  21, // Is a directory 
		EINVAL          =  22, // Invalid argument 
		ENFILE          =  23, // File table overflow 
		EMFILE          =  24, // Too many open files 
		ENOTTY          =  25, // Not a typewriter 
		ETXTBSY         =  26, // Text file busy 
		EFBIG           =  27, // File too large 
		ENOSPC          =  28, // No space left on device 
		ESPIPE          =  29, // Illegal seek 
		EROFS           =  30, // Read-only file system 
		EMLINK          =  31, // Too many links 
		EPIPE           =  32, // Broken pipe 
		EDOM            =  33, // Math argument out of domain of func 
		ERANGE          =  34, // Math result not representable 
		EDEADLK         =  35, // Resource deadlock would occur 
		ENAMETOOLONG    =  36, // File name too long 
		ENOLCK          =  37, // No record locks available 
		ENOSYS          =  38, // Function not implemented 
		ENOTEMPTY       =  39, // Directory not empty 
		ELOOP           =  40, // Too many symbolic links encountered 
		EWOULDBLOCK     =  EAGAIN, // Operation would block 
		ENOMSG          =  42, // No message of desired type 
		EIDRM           =  43, // Identifier removed 
		ECHRNG          =  44, // Channel number out of range 
		EL2NSYNC        =  45, // Level 2 not synchronized 
		EL3HLT          =  46, // Level 3 halted 
		EL3RST          =  47, // Level 3 reset 
		ELNRNG          =  48, // Link number out of range 
		EUNATCH         =  49, // Protocol driver not attached 
		ENOCSI          =  50, // No CSI structure available 
		EL2HLT          =  51, // Level 2 halted 
		EBADE           =  52, // Invalid exchange 
		EBADR           =  53, // Invalid request descriptor 
		EXFULL          =  54, // Exchange full 
		ENOANO          =  55, // No anode 
		EBADRQC         =  56, // Invalid request code 
		EBADSLT         =  57, // Invalid slot 
                      
		EDEADLOCK	      =  EDEADLK,
                      
		EBFONT          =  59, // Bad font file format 
		ENOSTR          =  60, // Device not a stream 
		ENODATA         =  61, // No data available 
		ETIME           =  62, // Timer expired 
		ENOSR           =  63, // Out of streams resources 
		ENONET          =  64, // Machine is not on the network 
		ENOPKG          =  65, // Package not installed 
		EREMOTE         =  66, // Object is remote 
		ENOLINK         =  67, // Link has been severed 
		EADV            =  68, // Advertise error 
		ESRMNT          =  69, // Srmount error 
		ECOMM           =  70, // Communication error on send 
		EPROTO          =  71, // Protocol error 
		EMULTIHOP       =  72, // Multihop attempted 
		EDOTDOT         =  73, // RFS specific error 
		EBADMSG         =  74, // Not a data message 
		EOVERFLOW       =  75, // Value too large for defined data type 
		ENOTUNIQ        =  76, // Name not unique on network 
		EBADFD          =  77, // File descriptor in bad state 
		EREMCHG         =  78, // Remote address changed 
		ELIBACC         =  79, // Can not access a needed shared library 
		ELIBBAD         =  80, // Accessing a corrupted shared library 
		ELIBSCN         =  81, // .lib section in a.out corrupted 
		ELIBMAX         =  82, // Attempting to link in too many shared libraries 
		ELIBEXEC        =  83, // Cannot exec a shared library directly 
		EILSEQ          =  84, // Illegal byte sequence 
		ERESTART        =  85, // Interrupted system call should be restarted 
		ESTRPIPE        =  86, // Streams pipe error 
		EUSERS          =  87, // Too many users 
		ENOTSOCK        =  88, // Socket operation on non-socket 
		EDESTADDRREQ    =  89, // Destination address required 
		EMSGSIZE        =  90, // Message too long 
		EPROTOTYPE      =  91, // Protocol wrong type for socket 
		ENOPROTOOPT     =  92, // Protocol not available 
		EPROTONOSUPPORT =  93, // Protocol not supported 
		ESOCKTNOSUPPORT	=  94, // Socket type not supported 
		EOPNOTSUPP      =  95, // Operation not supported on transport endpoint 
		EPFNOSUPPORT    =  96, // Protocol family not supported 
		EAFNOSUPPORT    =  97, // Address family not supported by protocol 
		EADDRINUSE      =  98, // Address already in use 
		EADDRNOTAVAIL   =  99, // Cannot assign requested address 
		ENETDOWN        = 100, // Network is down 
		ENETUNREACH     = 101, // Network is unreachable 
		ENETRESET       = 102, // Network dropped connection because of reset 
		ECONNABORTED    = 103, // Software caused connection abort 
		ECONNRESET      = 104, // Connection reset by peer 
		ENOBUFS         = 105, // No buffer space available 
		EISCONN         = 106, // Transport endpoint is already connected 
		ENOTCONN        = 107, // Transport endpoint is not connected 
		ESHUTDOWN       = 108, // Cannot send after transport endpoint shutdown 
		ETOOMANYREFS    = 109, // Too many references: cannot splice 
		ETIMEDOUT       = 110, // Connection timed out 
		ECONNREFUSED    = 111, // Connection refused 
		EHOSTDOWN       = 112, // Host is down 
		EHOSTUNREACH    = 113, // No route to host 
		EALREADY        = 114, // Operation already in progress 
		EINPROGRESS     = 115, // Operation now in progress 
		ESTALE          = 116, // Stale NFS file handle 
		EUCLEAN         = 117, // Structure needs cleaning 
		ENOTNAM         = 118, // Not a XENIX named type file 
		ENAVAIL         = 119, // No XENIX semaphores available 
		EISNAM          = 120, // Is a named type file 
		EREMOTEIO       = 121, // Remote I/O error 
		EDQUOT          = 122, // Quota exceeded 

		ENOMEDIUM       = 123, // No medium found 
		EMEDIUMTYPE     = 124, // Wrong medium type 
	}

	#endregion

	#region Classes

	public sealed class FilePosition : MarshalByRefObject, IDisposable 
#if NET_2_0
		, IEquatable <FilePosition>
#endif
	{

		private static readonly int FilePositionDumpSize = 
			Stdlib.DumpFilePosition (null, new HandleRef (null, IntPtr.Zero), 0);

		private HandleRef pos;

		public FilePosition ()
		{
			IntPtr p = Stdlib.CreateFilePosition ();
			if (p == IntPtr.Zero)
				throw new OutOfMemoryException ("Unable to malloc fpos_t!");
			pos = new HandleRef (this, p);
		}

		internal HandleRef Handle {
			get {return pos;}
		}

		public void Dispose ()
		{
			Cleanup ();
			GC.SuppressFinalize (this);
		}

		private void Cleanup ()
		{
			if (pos.Handle != IntPtr.Zero) {
				Stdlib.free (pos.Handle);
				pos = new HandleRef (this, IntPtr.Zero);
			}
		}

		public override string ToString ()
		{
			return "(" + base.ToString () + " " + GetDump () + ")";
		}

		private string GetDump ()
		{
			if (FilePositionDumpSize <= 0)
				return "internal error";

			StringBuilder buf = new StringBuilder (FilePositionDumpSize+1);

			if (Stdlib.DumpFilePosition (buf, Handle, FilePositionDumpSize+1) <= 0)
				return "internal error dumping fpos_t";

			return buf.ToString ();
		}

		public override bool Equals (object obj)
		{
			FilePosition fp = obj as FilePosition;
			if (obj == null || fp == null)
				return false;
			return ToString().Equals (obj.ToString());
		}

		public bool Equals (FilePosition value)
		{
			if (object.ReferenceEquals (this, value))
				return true;
			return ToString().Equals (value.ToString());
		}

		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		~FilePosition ()
		{
			Cleanup ();
		}

		public static bool operator== (FilePosition lhs, FilePosition rhs)
		{
			return Object.Equals (lhs, rhs);
		}

		public static bool operator!= (FilePosition lhs, FilePosition rhs)
		{
			return !Object.Equals (lhs, rhs);
		}
	}


	public enum SignalAction {
		Default,
		Ignore,
		Error
	}

	//
	// Right now using this attribute gives an assert because it
	// isn't implemented.
	//
#if NET_2_0 && UNMANAGED_FN_PTR_SUPPORT_FIXED
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
#endif
	public delegate void SignalHandler (int signal);

#if !NET_2_0
	internal sealed class SignalWrapper {
		private IntPtr handler;

		internal SignalWrapper (IntPtr handler)
		{
			this.handler = handler;
		}

		public void InvokeSignalHandler (int signum)
		{
			Stdlib.InvokeSignalHandler (signum, handler);
		}
	}
#endif

	internal class XPrintfFunctions
	{
		internal delegate object XPrintf (object[] parameters);

		internal static XPrintf printf;
		internal static XPrintf fprintf;
		internal static XPrintf snprintf;
		internal static XPrintf syslog;

		static XPrintfFunctions ()
		{
			CdeclFunction _printf = new CdeclFunction (Stdlib.LIBC, "printf", typeof(int));
			printf = new XPrintf (_printf.Invoke);

			CdeclFunction _fprintf = new CdeclFunction (Stdlib.LIBC, "fprintf", typeof(int));
			fprintf = new XPrintf (_fprintf.Invoke);

			CdeclFunction _snprintf = new CdeclFunction (Stdlib.MPH, 
					"Mono_Posix_Stdlib_snprintf", typeof(int));
			snprintf = new XPrintf (_snprintf.Invoke);

			CdeclFunction _syslog = new CdeclFunction (Syscall.MPH, 
					"Mono_Posix_Stdlib_syslog2", typeof(int));
			syslog = new XPrintf (_syslog.Invoke);
		}
	}

	//
	// Convention: Functions that are part of the C standard library go here.
	//
	// For example, the man page should say something similar to:
	//
	//    CONFORMING TO
	//           ISO 9899 (''ANSI C'')
	//
	// The intent is that members of this class should be portable to any system
	// supporting the C runtime (read: non-Unix, including Windows).  Using
	// anything from Syscall is non-portable, but restricting yourself to just
	// Stdlib is intended to be portable.
	//
	// The only methods in here should be:
	//  (1) low-level functions (as defined above).
	//  (2) "Trivial" function overloads.  For example, if the parameters to a
	//      function are related (e.g. fwrite(3))
	//  (3) The return type SHOULD NOT be changed.  If you want to provide a
	//      convenience function with a nicer return type, place it into one of
	//      the Mono.Unix.Std* wrapper classes, and give it a .NET-styled name.
	//      - EXCEPTION: No public functions should have a `void' return type.
	//        `void' return types should be replaced with `int'.
	//        Rationality: `void'-return functions typically require a
	//        complicated call sequence, such as clear errno, then call, then
	//        check errno to see if any errors occurred.  This sequence can't 
	//        be done safely in managed code, as errno may change as part of 
	//        the P/Invoke mechanism.
	//        Instead, add a MonoPosixHelper export which does:
	//          errno = 0;
	//          INVOKE SYSCALL;
	//          return errno == 0 ? 0 : -1;
	//        This lets managed code check the return value in the usual manner.
	//  (4) Exceptions SHOULD NOT be thrown.  EXCEPTIONS: 
	//      - If you're wrapping *broken* methods which make assumptions about 
	//        input data, such as that an argument refers to N bytes of data.  
	//        This is currently limited to cuserid(3) and encrypt(3).
	//      - If you call functions which themselves generate exceptions.  
	//        This is the case for using NativeConvert, which will throw an
	//        exception if an invalid/unsupported value is used.
	//
	public class Stdlib
	{
		internal const string LIBC = "msvcrt";
		internal const string MPH  = "MonoPosixHelper";

		internal Stdlib () {}

		#region <errno.h> Declarations
		//
		// <errno.h>  -- COMPLETE
		//

		public static Errno GetLastError ()
		{
			int errno = Marshal.GetLastWin32Error ();
			return NativeConvert.ToErrno (errno);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_SetLastError")]
		private static extern void SetLastError (int error);

		protected static void SetLastError (Errno error)
		{
			int _error = NativeConvert.FromErrno (error);
			SetLastError (_error);
		}

		#endregion

		//
		// <signal.h>
		//
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_InvokeSignalHandler")]
		internal static extern void InvokeSignalHandler (int signum, IntPtr handler);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_SIG_DFL")]
		private static extern IntPtr GetDefaultSignal ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_SIG_ERR")]
		private static extern IntPtr GetErrorSignal ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_SIG_IGN")]
		private static extern IntPtr GetIgnoreSignal ();

		private static readonly IntPtr _SIG_DFL = GetDefaultSignal ();
		private static readonly IntPtr _SIG_ERR = GetErrorSignal ();
		private static readonly IntPtr _SIG_IGN = GetIgnoreSignal ();

		private static void _ErrorHandler (int signum)
		{
			Console.Error.WriteLine ("Error handler invoked for signum " + 
					signum + ".  Don't do that.");
		}

		private static void _DefaultHandler (int signum)
		{
			Console.Error.WriteLine ("Default handler invoked for signum " + 
					signum + ".  Don't do that.");
		}

		private static void _IgnoreHandler (int signum)
		{
			Console.Error.WriteLine ("Ignore handler invoked for signum " + 
					signum + ".  Don't do that.");
		}

		[CLSCompliant (false)]
		public static readonly SignalHandler SIG_DFL = new SignalHandler (_DefaultHandler);
		[CLSCompliant (false)]
		public static readonly SignalHandler SIG_ERR = new SignalHandler (_ErrorHandler);
		[CLSCompliant (false)]
		public static readonly SignalHandler SIG_IGN = new SignalHandler (_IgnoreHandler);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, SignalHandler handler);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, IntPtr handler);

		[CLSCompliant (false)]
		[Obsolete ("This is not safe; " + 
				"use Mono.Unix.UnixSignal for signal delivery or SetSignalAction()")]
		public static SignalHandler signal (Signum signum, SignalHandler handler)
		{
			int _sig = NativeConvert.FromSignum (signum);

			Delegate[] handlers = handler.GetInvocationList ();
			for (int i = 0; i < handlers.Length; ++i) {
				Marshal.Prelink (handlers [i].Method);
			}

			IntPtr r;
			if (handler == SIG_DFL)
				r = sys_signal (_sig, _SIG_DFL);
			else if (handler == SIG_ERR)
				r = sys_signal (_sig, _SIG_ERR);
			else if (handler == SIG_IGN)
				r = sys_signal (_sig, _SIG_IGN);
			else
				r = sys_signal (_sig, handler);
			return TranslateHandler (r);
		}

		private static SignalHandler TranslateHandler (IntPtr handler)
		{
			if (handler == _SIG_DFL)
				return SIG_DFL;
			if (handler == _SIG_ERR)
				return SIG_ERR;
			if (handler == _SIG_IGN)
				return SIG_IGN;
#if NET_2_0
			return (SignalHandler) Marshal.GetDelegateForFunctionPointer (handler, typeof(SignalHandler));
#else
			return new SignalHandler (new SignalWrapper (handler).InvokeSignalHandler);
#endif
		}

		public static int SetSignalAction (Signum signal, SignalAction action)
		{
			return SetSignalAction (NativeConvert.FromSignum (signal), action);
		}

		public static int SetSignalAction (RealTimeSignum rts, SignalAction action)
		{
			return SetSignalAction (NativeConvert.FromRealTimeSignum (rts), action);
		}
		
		private static int SetSignalAction (int signum, SignalAction action)
		{
			IntPtr handler = IntPtr.Zero;
			switch (action) {
				case SignalAction.Default:
					handler = _SIG_DFL;
					break;
				case SignalAction.Ignore:
					handler = _SIG_IGN;
					break;
				case SignalAction.Error:
					handler = _SIG_ERR;
					break;
				default:
					throw new ArgumentException ("Invalid action value.", "action");
			}
			IntPtr r = sys_signal (signum, handler);
			if (r == _SIG_ERR)
				return -1;
			return 0;
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, EntryPoint="raise")]
		private static extern int sys_raise (int sig);

		[CLSCompliant (false)]
		public static int raise (Signum sig)
		{
			return sys_raise (NativeConvert.FromSignum (sig));
		}

		public static int raise (RealTimeSignum rts)
		{
			return sys_raise (NativeConvert.FromRealTimeSignum (rts));
		}

		//
		// <stdio.h> -- COMPLETE except for :
		//    - the scanf(3) family .
		//    - vararg functions.
		//    - Horribly unsafe functions (gets(3)).
		//
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib__IOFBF")]
		private static extern int GetFullyBuffered ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib__IOLBF")]
		private static extern int GetLineBuffered ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib__IONBF")]
		private static extern int GetNonBuffered ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_BUFSIZ")]
		private static extern int GetBufferSize ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_CreateFilePosition")]
		internal static extern IntPtr CreateFilePosition ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_DumpFilePosition")]
		internal static extern int DumpFilePosition (StringBuilder buf, HandleRef handle, int len);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_EOF")]
		private static extern int GetEOF ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_FILENAME_MAX")]
		private static extern int GetFilenameMax ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_FOPEN_MAX")]
		private static extern int GetFopenMax ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_L_tmpnam")]
		private static extern int GetTmpnamLength ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_stdin")]
		private static extern IntPtr GetStandardInput ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_stdout")]
		private static extern IntPtr GetStandardOutput ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_stderr")]
		private static extern IntPtr GetStandardError ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_TMP_MAX")]
		private static extern int GetTmpMax ();

		[CLSCompliant (false)]
		public static readonly int    _IOFBF       = GetFullyBuffered ();
		[CLSCompliant (false)]
		public static readonly int    _IOLBF       = GetLineBuffered ();
		[CLSCompliant (false)]
		public static readonly int    _IONBF       = GetNonBuffered ();
		[CLSCompliant (false)]
		public static readonly int    BUFSIZ       = GetBufferSize ();
		[CLSCompliant (false)]
		public static readonly int    EOF          = GetEOF ();
		[CLSCompliant (false)]
		public static readonly int    FOPEN_MAX    = GetFopenMax ();
		[CLSCompliant (false)]
		public static readonly int    FILENAME_MAX = GetFilenameMax ();
		[CLSCompliant (false)]
		public static readonly int    L_tmpnam     = GetTmpnamLength ();
		public static readonly IntPtr stderr       = GetStandardError ();
		public static readonly IntPtr stdin        = GetStandardInput ();
		public static readonly IntPtr stdout       = GetStandardOutput ();
		[CLSCompliant (false)]
		public static readonly int    TMP_MAX      = GetTmpMax ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int remove (
				[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
				string filename);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int rename (
				[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
				string oldpath, 
				[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
				string newpath);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr tmpfile ();

		private static object tmpnam_lock = new object ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="tmpnam")]
		private static extern IntPtr sys_tmpnam (StringBuilder s);

		[Obsolete ("Syscall.mkstemp() should be preferred.")]
		public static string tmpnam (StringBuilder s)
		{
			if (s != null && s.Capacity < L_tmpnam)
				throw new ArgumentOutOfRangeException ("s", "s.Capacity < L_tmpnam");
			lock (tmpnam_lock) {
				IntPtr r = sys_tmpnam (s);
				return UnixMarshal.PtrToString (r);
			}
		}

		[Obsolete ("Syscall.mkstemp() should be preferred.")]
		public static string tmpnam ()
		{
			lock (tmpnam_lock) {
				IntPtr r = sys_tmpnam (null);
				return UnixMarshal.PtrToString (r);
			}
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fclose (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fflush (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr fopen (
				[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
				string path, string mode);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr freopen (
				[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
				string path, string mode, IntPtr stream);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl, 
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_setbuf")]
		public static extern int setbuf (IntPtr stream, IntPtr buf);

		[CLSCompliant (false)]
		public static unsafe int setbuf (IntPtr stream, byte* buf)
		{
			return setbuf (stream, (IntPtr) buf);
		}

		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_setvbuf")]
		public static extern int setvbuf (IntPtr stream, IntPtr buf, int mode, ulong size);

		[CLSCompliant (false)]
		public static unsafe int setvbuf (IntPtr stream, byte* buf, int mode, ulong size)
		{
			return setvbuf (stream, (IntPtr) buf, mode, size);
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="fprintf")]
		private static extern int sys_fprintf (IntPtr stream, string format, string message);

		public static int fprintf (IntPtr stream, string message)
		{
			return sys_fprintf (stream, "%s", message);
		}

		[Obsolete ("Not necessarily portable due to cdecl restrictions.\n" +
				"Use fprintf (IntPtr, string) instead.")]
		public static int fprintf (IntPtr stream, string format, params object[] parameters)
		{
			object[] _parameters = new object[checked(parameters.Length+2)];
			_parameters [0] = stream;
			_parameters [1] = format;
			Array.Copy (parameters, 0, _parameters, 2, parameters.Length);
			return (int) XPrintfFunctions.fprintf (_parameters);
		}

		/* SKIP: fscanf(3) */

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="printf")]
		private static extern int sys_printf (string format, string message);

		public static int printf (string message)
		{
			return sys_printf ("%s", message);
		}

		[Obsolete ("Not necessarily portable due to cdecl restrictions.\n" +
				"Use printf (string) instead.")]
		public static int printf (string format, params object[] parameters)
		{
			object[] _parameters = new object[checked(parameters.Length+1)];
			_parameters [0] = format;
			Array.Copy (parameters, 0, _parameters, 1, parameters.Length);
			return (int) XPrintfFunctions.printf (_parameters);
		}

		/* SKIP: scanf(3) */

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_snprintf")]
		private static extern int sys_snprintf (StringBuilder s, ulong n, 
				string format, string message);

		[CLSCompliant (false)]
		public static int snprintf (StringBuilder s, ulong n, string message)
		{
			if (n > (ulong) s.Capacity)
				throw new ArgumentOutOfRangeException ("n", "n must be <= s.Capacity");
			return sys_snprintf (s, n, "%s", message);
		}

		public static int snprintf (StringBuilder s, string message)
		{
			return sys_snprintf (s, (ulong) s.Capacity, "%s", message);
		}

		[CLSCompliant (false)]
		[Obsolete ("Not necessarily portable due to cdecl restrictions.\n" +
				"Use snprintf (StringBuilder, string) instead.")]
		public static int snprintf (StringBuilder s, ulong n, 
				string format, params object[] parameters)
		{
			if (n > (ulong) s.Capacity)
				throw new ArgumentOutOfRangeException ("n", "n must be <= s.Capacity");

			object[] _parameters = new object[checked(parameters.Length+3)];
			_parameters [0] = s;
			_parameters [1] = n;
			_parameters [2] = format;
			Array.Copy (parameters, 0, _parameters, 3, parameters.Length);
			return (int) XPrintfFunctions.snprintf (_parameters);
		}

		[CLSCompliant (false)]
		[Obsolete ("Not necessarily portable due to cdecl restrictions.\n" +
				"Use snprintf (StringBuilder, string) instead.")]
		public static int snprintf (StringBuilder s,
				string format, params object[] parameters)
		{
			object[] _parameters = new object[checked(parameters.Length+3)];
			_parameters [0] = s;
			_parameters [1] = (ulong) s.Capacity;
			_parameters [2] = format;
			Array.Copy (parameters, 0, _parameters, 3, parameters.Length);
			return (int) XPrintfFunctions.snprintf (_parameters);
		}

		/*
		 * SKIP:
		 *    sprintf(3)
		 *    sscanf(3)
		 *    vfprintf(3)
		 *    vfscanf(3)
		 *    vprintf(3)
		 *    vscanf(3)
		 *    vsnprintf(3)
		 *    vsprint(3)
		 *    vsscanf(3)
		 */

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fgetc (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="fgets")]
		private static extern IntPtr sys_fgets (StringBuilder sb, int size, IntPtr stream);

		public static StringBuilder fgets (StringBuilder sb, int size, IntPtr stream)
		{
			IntPtr r = sys_fgets (sb, size, stream);
			if (r == IntPtr.Zero)
				return null;
			return sb;
		}

		public static StringBuilder fgets (StringBuilder sb, IntPtr stream)
		{
			return fgets (sb, sb.Capacity, stream);
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fputc (int c, IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fputs (string s, IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int getc (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int getchar ();

		/* SKIP: gets(3) */

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int putc (int c, IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int putchar (int c);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int puts (string s);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int ungetc (int c, IntPtr stream);

		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		public static extern ulong fread (IntPtr ptr, ulong size, ulong nmemb, IntPtr stream);

		[CLSCompliant (false)]
		public static unsafe ulong fread (void* ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			return fread ((IntPtr) ptr, size, nmemb, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		private static extern ulong sys_fread ([Out] byte[] ptr, 
				ulong size, ulong nmemb, IntPtr stream);

		[CLSCompliant (false)]
		public static ulong fread (byte[] ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			if ((size * nmemb) > (ulong) ptr.Length)
				throw new ArgumentOutOfRangeException ("nmemb");
			return sys_fread (ptr, size, nmemb, stream);
		}

		[CLSCompliant (false)]
		public static ulong fread (byte[] ptr, IntPtr stream)
		{
			return fread (ptr, 1, (ulong) ptr.Length, stream);
		}

		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		public static extern ulong fwrite (IntPtr ptr, ulong size, ulong nmemb, IntPtr stream);

		[CLSCompliant (false)]
		public static unsafe ulong fwrite (void* ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			return fwrite ((IntPtr) ptr, size, nmemb, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		private static extern ulong sys_fwrite (byte[] ptr, 
				ulong size, ulong nmemb, IntPtr stream);

		[CLSCompliant (false)]
		public static ulong fwrite (byte[] ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			if ((size * nmemb) > (ulong) ptr.Length)
				throw new ArgumentOutOfRangeException ("nmemb");
			return sys_fwrite (ptr, size, nmemb, stream);
		}

		[CLSCompliant (false)]
		public static ulong fwrite (byte[] ptr, IntPtr stream)
		{
			return fwrite (ptr, 1, (ulong) ptr.Length, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fgetpos")]
		private static extern int sys_fgetpos (IntPtr stream, HandleRef pos);

		public static int fgetpos (IntPtr stream, FilePosition pos)
		{
			return sys_fgetpos (stream, pos.Handle);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fseek")]
		private static extern int sys_fseek (IntPtr stream, long offset, int origin);

		[CLSCompliant (false)]
		public static int fseek (IntPtr stream, long offset, SeekFlags origin)
		{
			int _origin = NativeConvert.FromSeekFlags (origin);
			return sys_fseek (stream, offset, _origin);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fsetpos")]
		private static extern int sys_fsetpos (IntPtr stream, HandleRef pos);

		public static int fsetpos (IntPtr stream, FilePosition pos)
		{
			return sys_fsetpos (stream, pos.Handle);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_ftell")]
		public static extern long ftell (IntPtr stream);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_rewind")]
		public static extern int rewind (IntPtr stream);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_clearerr")]
		public static extern int clearerr (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int feof (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int ferror (IntPtr stream);

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl, 
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_perror")]
		private static extern int perror (string s, int err);

		public static int perror (string s)
		{
			return perror (s, Marshal.GetLastWin32Error ());
		}

		//
		// <stdlib.h>
		//
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_EXIT_FAILURE")]
		private static extern int GetExitFailure();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_EXIT_SUCCESS")]
		private static extern int GetExitSuccess ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_MB_CUR_MAX")]
		private static extern int GetMbCurMax ();

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Stdlib_RAND_MAX")]
		private static extern int GetRandMax ();

		[CLSCompliant (false)]
		public static readonly int  EXIT_FAILURE = GetExitFailure ();
		[CLSCompliant (false)]
		public static readonly int  EXIT_SUCCESS = GetExitSuccess ();
		[CLSCompliant (false)]
		public static readonly int  MB_CUR_MAX   = GetMbCurMax ();
		[CLSCompliant (false)]
		public static readonly int  RAND_MAX     = GetRandMax ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int rand ();

		[CLSCompliant (false)]
		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void srand (uint seed);

		// calloc(3):
		//    void *calloc (size_t nmemb, size_t size);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_calloc")]
		public static extern IntPtr calloc (ulong nmemb, ulong size);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void free (IntPtr ptr);

		// malloc(3):
		//    void *malloc(size_t size);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_malloc")]
		public static extern IntPtr malloc (ulong size);

		// realloc(3):
		//    void *realloc(void *ptr, size_t size);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_realloc")]
		public static extern IntPtr realloc (IntPtr ptr, ulong size);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void abort ();

		/* SKIP: atexit(3) -- the GC should have collected most references by the
		 * time this runs, so no delegates should exist, making it pointless. */

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void exit (int status);

		[CLSCompliant (false)]
		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void _Exit (int status);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, EntryPoint="getenv")]
		private static extern IntPtr sys_getenv (string name);

		public static string getenv (string name)
		{
			IntPtr r = sys_getenv (name);
			return UnixMarshal.PtrToString (r);
		}

		[CLSCompliant (false)]
		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int system (string @string);

		//
		// <string.h>
		//

		private static object strerror_lock = new object ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="strerror")]
		private static extern IntPtr sys_strerror (int errnum);

		[CLSCompliant (false)]
		public static string strerror (Errno errnum)
		{
			int e = NativeConvert.FromErrno (errnum);
			lock (strerror_lock) {
				IntPtr r = sys_strerror (e);
				return UnixMarshal.PtrToString (r);
			}
		}

		// strlen(3):
		//    size_t strlen(const char *s);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_strlen")]
		public static extern ulong strlen (IntPtr s);
	}

	#endregion // Classes
}

// vim: noexpandtab
