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
using Mono.Unix;

namespace Mono.Unix {

	public delegate void sighandler_t (int value);

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
		[DllImport (LIBC, SetLastError=true, EntryPoint="signal")]
		private static extern IntPtr sys_signal (int signum, sighandler_t handler);

		// FIXME: signal returns sighandler_t.  What should we do?
		public static int signal (Signum signum, sighandler_t handler)
		{
			int _sig = UnixConvert.FromSignum (signum);
			IntPtr r = sys_signal (_sig, handler);
			// handle `r'
			return 0;
		}

		// TODO: Need access to SIG_IGN, SIG_DFL, and SIG_ERR values.

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
		[DllImport (MPH, SetLastError=true, EntryPoint="Mono_Posix_Stdlib_calloc")]
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
