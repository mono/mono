//
// Mono.Unix/Stdlib.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public delegate void SignalHandler (int signal);

	public sealed class FilePosition : IDisposable {

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
			GC.SuppressFinalize (this);
			Cleanup ();
		}

		private void Cleanup ()
		{
			if (pos.Handle != IntPtr.Zero) {
				Stdlib.free (pos.Handle);
				pos = new HandleRef (this, IntPtr.Zero);
			}
		}

		~FilePosition ()
		{
			Cleanup ();
		}
	}

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

			CdeclFunction _syslog = new CdeclFunction (Syscall.LIBC, "syslog", typeof(void));
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
	public class Stdlib
	{
		internal const string LIBC = "msvcrt";
		internal const string MPH  = "MonoPosixHelper";

		internal Stdlib () {}

		//
		// <signal.h>
		//
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Posix_Syscall_InvokeSignalHandler")]
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

		public static readonly SignalHandler SIG_DFL = new SignalHandler (_DefaultHandler);
		public static readonly SignalHandler SIG_ERR = new SignalHandler (_ErrorHandler);
		public static readonly SignalHandler SIG_IGN = new SignalHandler (_IgnoreHandler);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, SignalHandler handler);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, IntPtr handler);

		public static SignalHandler signal (Signum signum, SignalHandler handler)
		{
			int _sig = UnixConvert.FromSignum (signum);
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
			return new SignalHandler (new SignalWrapper (handler).InvokeSignalHandler);
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, EntryPoint="raise")]
		private static extern int sys_raise (int sig);

		public static int raise (Signum sig)
		{
			int _sig = UnixConvert.FromSignum (sig);
			return sys_raise (_sig);
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

		public static readonly int    _IOFBF       = GetFullyBuffered ();
		public static readonly int    _IOLBF       = GetLineBuffered ();
		public static readonly int    _IONBF       = GetNonBuffered ();
		public static readonly int    BUFSIZ       = GetBufferSize ();
		public static readonly int    EOF          = GetEOF ();
		public static readonly int    FOPEN_MAX    = GetFopenMax ();
		public static readonly int    FILENAME_MAX = GetFilenameMax ();
		public static readonly int    L_tmpnam     = GetTmpnamLength ();
		public static readonly IntPtr stderr       = GetStandardError ();
		public static readonly IntPtr stdin        = GetStandardInput ();
		public static readonly IntPtr stdout       = GetStandardOutput ();
		public static readonly int    TMP_MAX      = GetTmpMax ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int remove (string filename);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int rename (string oldpath, string newpath);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr tmpfile ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="tmpnam")]
		private static extern IntPtr sys_tmpnam (StringBuilder s);

		[Obsolete ("Syscall.mkstemp() should be preferred.")]
		public static string tmpnam (StringBuilder s)
		{
			if (s != null && s.Capacity < L_tmpnam)
				throw new ArgumentOutOfRangeException ("s", "s.Capacity < L_tmpnam");
			IntPtr r = sys_tmpnam (s);
			return UnixMarshal.PtrToString (r);
		}

		[Obsolete ("Syscall.mkstemp() should be preferred.")]
		public static string tmpnam ()
		{
			IntPtr r = sys_tmpnam (null);
			return UnixMarshal.PtrToString (r);
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fclose (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int fflush (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr fopen (string path, string mode);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern IntPtr freopen (string path, string mode, IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern void setbuf (IntPtr stream, IntPtr buf);

		public static unsafe void setbuf (IntPtr stream, byte* buf)
		{
			setbuf (stream, (IntPtr) buf);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_setvbuf")]
		public static extern int setvbuf (IntPtr stream, IntPtr buf, int mode, ulong size);

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

		public static int snprintf (StringBuilder s, ulong n, string message)
		{
			return sys_snprintf (s, n, "%s", message);
		}

		public static int snprintf (StringBuilder s, string message)
		{
			return sys_snprintf (s, (ulong) s.Capacity, "%s", message);
		}

		[Obsolete ("Not necessarily portable due to cdecl restrictions.\n" +
				"Use snprintf (StringBuilder, string) instead.")]
		public static int snprintf (StringBuilder s, ulong n, 
				string format, params object[] parameters)
		{
			object[] _parameters = new object[checked(parameters.Length+3)];
			_parameters [0] = s;
			_parameters [1] = n;
			_parameters [2] = format;
			Array.Copy (parameters, 0, _parameters, 3, parameters.Length);
			return (int) XPrintfFunctions.snprintf (_parameters);
		}

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

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		public static extern ulong fread (IntPtr ptr, ulong size, ulong nmemb, IntPtr stream);

		public static unsafe ulong fread (void* ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			return fread ((IntPtr) ptr, size, nmemb, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		private static extern ulong sys_fread ([Out] byte[] ptr, 
				ulong size, ulong nmemb, IntPtr stream);

		public static ulong fread (byte[] ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			if ((size * nmemb) > (ulong) ptr.Length)
				throw new ArgumentOutOfRangeException ("nmemb");
			return sys_fread (ptr, size, nmemb, stream);
		}

		public static ulong fread (byte[] ptr, IntPtr stream)
		{
			return fread (ptr, 1, (ulong) ptr.Length, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		public static extern ulong fwrite (IntPtr ptr, ulong size, ulong nmemb, IntPtr stream);

		public static unsafe ulong fwrite (void* ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			return fwrite ((IntPtr) ptr, size, nmemb, stream);
		}

		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		private static extern ulong sys_fwrite (byte[] ptr, 
				ulong size, ulong nmemb, IntPtr stream);

		public static ulong fwrite (byte[] ptr, ulong size, ulong nmemb, IntPtr stream)
		{
			if ((size * nmemb) > (ulong) ptr.Length)
				throw new ArgumentOutOfRangeException ("nmemb");
			return sys_fwrite (ptr, size, nmemb, stream);
		}

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

		public static int fseek (IntPtr stream, long offset, SeekFlags origin)
		{
			int _origin = UnixConvert.FromSeekFlags (origin);
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

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void rewind (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void clearerr (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int feof (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int ferror (IntPtr stream);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern void perror (string s);

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

		public static readonly int  EXIT_FAILURE = GetExitFailure ();
		public static readonly int  EXIT_SUCCESS = GetExitSuccess ();
		public static readonly int  MB_CUR_MAX   = GetMbCurMax ();
		public static readonly int  RAND_MAX     = GetRandMax ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern int rand ();

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void srand (uint seed);

		// calloc(3):
		//    void *calloc (size_t nmemb, size_t size);
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_calloc")]
		public static extern IntPtr calloc (ulong nmemb, ulong size);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void free (IntPtr ptr);

		// malloc(3):
		//    void *malloc(size_t size);
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_malloc")]
		public static extern IntPtr malloc (ulong size);

		// realloc(3):
		//    void *realloc(void *ptr, size_t size);
		[DllImport (MPH, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="Mono_Posix_Stdlib_realloc")]
		public static extern IntPtr realloc (IntPtr ptr, ulong size);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void abort ();

		/* SKIP: atexit(3) */

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void exit (int status);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl)]
		public static extern void _Exit (int status);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, EntryPoint="getenv")]
		private static extern IntPtr sys_getenv (string name);

		public static string getenv (string name)
		{
			IntPtr r = sys_getenv (name);
			return UnixMarshal.PtrToString (r);
		}

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		public static extern int system (string @string);

		//
		// <string.h>
		//

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl,
				SetLastError=true, EntryPoint="strerror")]
		private static extern IntPtr sys_strerror (int errnum);

		public static string strerror (Error errnum)
		{
			int e = UnixConvert.FromError (errnum);
			IntPtr r = sys_strerror (e);
			return UnixMarshal.PtrToString (r);
		}
	}
}

// vim: noexpandtab
