//
// Mono.Unix/UnixMarshal.cs
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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	// Scenario:  We want to be able to translate an Error to a string.
	//  Problem:  Thread-safety.  Strerror(3) isn't thread safe (unless
	//            thread-local-variables are used, which is probably only 
	//            true on Windows).
	// Solution:  Use strerror_r().
	//  Problem:  strerror_r() isn't portable. 
	//            (Apparently Solaris doesn't provide it.)
	// Solution:  Cry.  Then introduce an intermediary, ErrorMarshal.
	//            ErrorMarshal exposes a single public delegate, Translator,
	//            which will convert an Error to a string.  It's static
	//            constructor first tries using strerror_r().  If it works,
	//            great; use it in the future.  If it doesn't work, fallback to
	//            using strerror(3).
	//            This should be thread safe, since the check is done within the
	//            class constructor lock.
	//  Problem:  But if strerror_r() isn't present, we're not thread safe!
	// Solution:  Tough.  It's still better than nothing.  I think.  
	//            Check UnixMarshal.IsErrorDescriptionThreadSafe if you need to
	//            know which error translator is being used.
	internal class ErrorMarshal
	{
		public delegate string ErrorTranslator (Error errno);

		public static readonly ErrorTranslator Translate;
		public static readonly bool HaveStrerror_r;

		static ErrorMarshal ()
		{
			try {
				Translate = new ErrorTranslator (strerror_r);
				string ignore = Translate (Error.EPERM);
				HaveStrerror_r = true;
			}
			catch (MissingMethodException e) {
				Translate = new ErrorTranslator (strerror);
				HaveStrerror_r = false;
			}
		}

		private static string strerror (Error errno)
		{
			return Syscall.strerror (errno);
		}

		private static string strerror_r (Error errno)
		{
			StringBuilder buf = new StringBuilder (16);
			int r = 0;
			do {
				buf.Capacity *= 2;
				r = Syscall.strerror_r (errno, buf);
			} while (r == -1 && Syscall.GetLastError() == Error.ERANGE);

			if (r == -1)
				return "** Unknown error code: " + ((int) errno) + "**";
			return buf.ToString();
		}
	}

	public sealed /* static */ class UnixMarshal
	{
		private UnixMarshal () {}

		public static string GetErrorDescription (Error errno)
		{
			return ErrorMarshal.Translate (errno);
		}

		public static bool IsErrorDescriptionThreadSafe {
			get {return ErrorMarshal.HaveStrerror_r;}
		}

		public static IntPtr Alloc (long size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException ("size", "< 0");
			return Stdlib.malloc ((ulong) size);
		}

		public static IntPtr ReAlloc (IntPtr ptr, long size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException ("size", "< 0");
			return Stdlib.realloc (ptr, (ulong) size);
		}

		public static void Free (IntPtr ptr)
		{
			Stdlib.free (ptr);
		}

		public static string PtrToString (IntPtr p)
		{
			// TODO: deal with character set issues.  Will PtrToStringAnsi always
			// "Do The Right Thing"?
			if (p == IntPtr.Zero)
				return null;
			return Marshal.PtrToStringAnsi (p);
		}

		/*
		 * Marshal a C `char **'.  ANSI C `main' requirements are assumed:
		 *
		 *   stringArray is an array of pointers to C strings
		 *   stringArray has a terminating NULL string.
		 *
		 * For example:
		 *   stringArray[0] = "string 1";
		 *   stringArray[1] = "string 2";
		 *   stringArray[2] = NULL
		 *
		 * The terminating NULL is required so that we know when to stop looking
		 * for strings.
		 */
		public static string[] PtrToStringArray (IntPtr stringArray)
		{
			if (stringArray == IntPtr.Zero)
				return new string[]{};

			int argc = CountStrings (stringArray);
			return PtrToStringArray (argc, stringArray);
		}

		private static int CountStrings (IntPtr stringArray)
		{
			int count = -1;
			IntPtr item = Marshal.ReadIntPtr (stringArray, count * IntPtr.Size);
			do {
				++count;
			} while (Marshal.ReadIntPtr (stringArray, count * IntPtr.Size) != IntPtr.Zero);
			return count;
		}

		/*
		 * Like PtrToStringArray(IntPtr), but it allows the user to specify how
		 * many strings to look for in the array.  As such, the requirement for a
		 * terminating NULL element is not required.
		 *
		 * Usage is similar to ANSI C `main': count is argc, stringArray is argv.
		 * stringArray[count] is NOT accessed (though ANSI C requires that 
		 * argv[argc] = NULL, which PtrToStringArray(IntPtr) requires).
		 */
		public static string[] PtrToStringArray (int count, IntPtr stringArray)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			if (stringArray == IntPtr.Zero)
				return new string[count];

			string[] members = new string[count];
			for (int i = 0; i < count; ++i) {
				IntPtr s = Marshal.ReadIntPtr (stringArray, i * IntPtr.Size);
				members[i] = PtrToString (s);
			}

			return members;
		}

		public static bool ShouldRetrySyscall (int r)
		{
			if (r == -1 && Syscall.GetLastError () == Error.EINTR)
				return true;
			return false;
		}

		public static bool ShouldRetrySyscall (int r, out Error error)
		{
			error = (Error) 0;
			if (r == -1 && (error = Syscall.GetLastError ()) == Error.EINTR)
				return true;
			return false;
		}

		private static Exception CreateExceptionForError (Error errno)
		{
			string message = GetErrorDescription (errno);
			UnixIOException p = new UnixIOException (errno);
			switch (errno) {
				case Error.EFAULT:        return new NullReferenceException (message, p);
				case Error.EINVAL:        return new ArgumentException (message, p);
				case Error.EIO:
				  case Error.ENOSPC:
				  case Error.EROFS:
				  case Error.ESPIPE:
					return new IOException (message, p);
				case Error.ENAMETOOLONG:  return new PathTooLongException (message, p);
				case Error.ENOENT:        return new FileNotFoundException (message, p);
				case Error.ENOEXEC:       return new InvalidProgramException (message, p);
				case Error.EOVERFLOW:     return new OverflowException (message, p);
				case Error.ERANGE:        return new ArgumentOutOfRangeException (message);
				default: /* ignore */     break;
			}
			return p;
		}

		public static void ThrowExceptionForError (Error errno)
		{
			throw CreateExceptionForError (errno);
		}

		public static void ThrowExceptionForLastError ()
		{
			throw CreateExceptionForError (Syscall.GetLastError());
		}

		public static void ThrowExceptionForErrorIf (int retval, Error errno)
		{
			if (retval == -1)
				ThrowExceptionForError (errno);
		}

		public static void ThrowExceptionForLastErrorIf (int retval)
		{
			if (retval == -1)
				ThrowExceptionForLastError ();
		}
	}
}

// vim: noexpandtab
