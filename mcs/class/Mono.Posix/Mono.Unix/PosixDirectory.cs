//
// Mono.Posix/PosixDirectory.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Posix;

namespace Mono.Posix {

	public sealed class PosixDirectory
	{
		private PosixDirectory () {}

		public static PosixDirectoryInfo CreateDirectory (string path, FilePermissions mode)
		{
			int r = Syscall.mkdir (path, mode);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
			return new PosixDirectoryInfo (path);
		}

		public static PosixDirectoryInfo CreateDirectory (string path)
		{
			FilePermissions mode = FilePermissions.ACCESSPERMS;
			return CreateDirectory (path, mode);
		}

		public static void Delete (string path)
		{
			Delete (path, false);
		}

		public static void Delete (string path, bool recursive)
		{
			new PosixDirectoryInfo (path).Delete (recursive);
		}

		public static bool Exists (string path)
		{
			int r = Syscall.access (path, AccessMode.F_OK);
			if (r == 0)
				return true;
			return false;
		}

		public static Dirent[] GetEntries (string path)
		{
			return new PosixDirectoryInfo(path).GetEntries ();
		}

		public static Dirent[] GetEntries (string path, Regex regex)
		{
			return new PosixDirectoryInfo(path).GetEntries (regex);
		}

		public static Dirent[] GetEntries (string path, string regex)
		{
			return new PosixDirectoryInfo(path).GetEntries (regex);
		}

		public static PosixFileSystemInfo[] GetFileSystemEntries (string path)
		{
			return new PosixDirectoryInfo(path).GetFileSystemEntries ();
		}

		public static PosixFileSystemInfo[] GetFileSystemEntries (string path, Regex regex)
		{
			return new PosixDirectoryInfo(path).GetFileSystemEntries (regex);
		}

		public static PosixFileSystemInfo[] GetFileSystemEntries (string path, string regex)
		{
			return new PosixDirectoryInfo(path).GetFileSystemEntries (regex);
		}

		public static Stat GetDirectoryStatus (string path)
		{
			return PosixFile.GetFileStatus (path);
		}

		public static string GetCurrentDirectory ()
		{
			StringBuilder buf = new StringBuilder (16);
			IntPtr r = IntPtr.Zero;
			do {
				buf.Capacity *= 2;
				r = Syscall.getcwd (buf, (ulong) buf.Capacity);
			} while (r == IntPtr.Zero && Syscall.GetLastError() == Error.ERANGE);
			if (r == IntPtr.Zero)
				PosixMarshal.ThrowExceptionForLastError ();
			return buf.ToString ();
		}

		public static void SetCurrentDirectory (string path)
		{
			int r = Syscall.chdir (path);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
		}
	}
}

// vim: noexpandtab
