//
// Mono.Unix/Stdlib.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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

	public delegate void Sighandler_t (int value);

	internal class XPrintfFunctions
	{
		internal delegate object XPrintf (object[] parameters);

		internal static XPrintf printf;
		internal static XPrintf fprintf;
		internal static XPrintf syslog;

		static XPrintfFunctions ()
		{
			CdeclFunction _printf = new CdeclFunction ("libc", "printf", typeof(int));
			printf = new XPrintf (_printf.Invoke);

			CdeclFunction _fprintf = new CdeclFunction ("libc", "fprintf", typeof(int));
			fprintf = new XPrintf (_fprintf.Invoke);

			CdeclFunction _syslog = new CdeclFunction ("libc", "syslog", typeof(void));
			syslog = new XPrintf (_syslog.Invoke);
		}
	}

	internal sealed class SignalWrapper {
		private IntPtr handler;

		internal SignalWrapper (IntPtr handler)
		{
			this.handler = handler;
		}

		private const string MPH = "MonoPosixHelper";

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_InvokeSignalHandler")]
		private static extern void InvokeSignalHandler (int signum, IntPtr handler);

		public void InvokeSignalHandler (int signum)
		{
			InvokeSignalHandler (signum, handler);
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
	// We can also place logical "sibling" exports here -- exports which
	// strongly relate to an ANSI C function, either as an overload, or which
	// operates on the same datatype as an ANSI C function.  Examples include
	// fileno(3) and fdopen(3).
	//
	public class Stdlib
	{
		private const string LIBC = "libc";
		private const string MPH = "MonoPosixHelper";

		internal Stdlib () {}

		//
		// <signal.h>
		//
		[DllImport (MPH, EntryPoint="Mono_Posix_Stdlib_SIG_DFL")]
		private static extern IntPtr GetDefaultSignal ();

		[DllImport (MPH, EntryPoint="Mono_Posix_Stdlib_SIG_ERR")]
		private static extern IntPtr GetErrorSignal ();

		[DllImport (MPH, EntryPoint="Mono_Posix_Stdlib_SIG_IGN")]
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

		public static readonly Sighandler_t SIG_DFL = _DefaultHandler;
		public static readonly Sighandler_t SIG_ERR = _ErrorHandler;
		public static readonly Sighandler_t SIG_IGN = _IgnoreHandler;

		[DllImport (LIBC, SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, Sighandler_t handler);

		[DllImport (LIBC, SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, IntPtr handler);

		public static Sighandler_t signal (Signum signum, Sighandler_t handler)
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

		private static Sighandler_t TranslateHandler (IntPtr handler)
		{
			if (handler == _SIG_DFL)
				return SIG_DFL;
			if (handler == _SIG_ERR)
				return SIG_ERR;
			if (handler == _SIG_IGN)
				return SIG_IGN;
			return new Sighandler_t (new SignalWrapper (handler).InvokeSignalHandler);
		}

		//
		// <stdio.h>
		//
		[DllImport (LIBC, SetLastError=true)]
		public static extern void perror (string s);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int rename (string oldpath, string newpath);

		[DllImport (LIBC)]
		public static extern void clearerr (IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern IntPtr fopen (string path, string mode);

		[DllImport (LIBC, SetLastError=true)]
		public static extern IntPtr fdopen (int filedes, string mode);

		[DllImport (LIBC, SetLastError=true)]
		public static extern IntPtr freopen (string path, string mode, IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fclose (IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fflush (IntPtr stream);

		[DllImport (LIBC)]
		public static extern int feof (IntPtr stream);

		[DllImport (LIBC)]
		public static extern int ferror (IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fileno (IntPtr stream);

		[DllImport (LIBC, EntryPoint="printf")]
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

		[DllImport (LIBC, EntryPoint="fprintf")]
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

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fputc (int c, IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fputs (string s, IntPtr stream);

		// skip putc, as it may be a macro

		[DllImport (LIBC, SetLastError=true)]
		public static extern int putchar (int c);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int puts (string s);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fgetc (IntPtr stream);

		[DllImport (LIBC, SetLastError=true, EntryPoint="fgetc")]
		private static extern IntPtr sys_fgetc (StringBuilder sb, int size, IntPtr stream);

		public static StringBuilder fgetc (StringBuilder sb, int size, IntPtr stream)
		{
			sys_fgetc (sb, size, stream);
			return sb;
		}

		public static StringBuilder fgetc (StringBuilder sb, IntPtr stream)
		{
			sys_fgetc (sb, sb.Capacity, stream);
			return sb;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int getc (IntPtr stream);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int getchar ();

		// skip gets(3), it's evil.

		[DllImport (LIBC, SetLastError=true)]
		public static extern int ungetc (int c, IntPtr stream);

		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		public static extern unsafe ulong fread (void* ptr, ulong size, ulong nmemb, IntPtr stream);

		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fread")]
		public static extern ulong fread ([Out] byte[] ptr, ulong size, ulong nmemb, IntPtr stream);

		public static ulong fread (byte[] ptr, IntPtr stream)
		{
			return fread (ptr, 1, (ulong) ptr.Length, stream);
		}

		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		public static extern unsafe ulong fwrite (void* ptr, ulong size, ulong nmemb, IntPtr stream);

		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_fwrite")]
		public static extern ulong fwrite (byte[] ptr, ulong size, ulong nmemb, IntPtr stream);

		public static ulong fwrite (byte[] ptr, IntPtr stream)
		{
			return fwrite (ptr, 1, (ulong) ptr.Length, stream);
		}

		//
		// <stdlib.h>
		//

		// calloc(3):
		//    void *calloc (size_t nmemb, size_t size);
		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_calloc")]
		public static extern IntPtr calloc (ulong nmemb, ulong size);

		[DllImport (LIBC)]
		public static extern void exit (int status);

		[DllImport (LIBC)]
		public static extern void free (IntPtr ptr);

		// malloc(3):
		//    void *malloc(size_t size);
		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_malloc")]
		public static extern IntPtr malloc (ulong size);

		// realloc(3):
		//    void *realloc(void *ptr, size_t size);
		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_realloc")]
		public static extern IntPtr realloc (IntPtr ptr, ulong size);

		[DllImport (MPH, SetLastError=true)]
		public static extern int system (string @string);

		//
		// <string.h>
		//

		[DllImport ("libc", SetLastError=true, EntryPoint="strerror")]
		private static extern IntPtr sys_strerror (int errnum);

		public static IntPtr sys_strerror (Error errnum)
		{
			int e = UnixConvert.FromError (errnum);
			return sys_strerror (e);
		}

		public static string strerror (Error errnum)
		{
			IntPtr r = sys_strerror (errnum);
			return UnixMarshal.PtrToString (r);
		}
	}
}

// vim: noexpandtab
