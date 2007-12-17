//
// Mono.Unix/UnixPath.cs
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
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixPath
	{
		private UnixPath () {}

		public static readonly char DirectorySeparatorChar = '/';
		public static readonly char AltDirectorySeparatorChar = '/';
		public static readonly char PathSeparator = ':';
		public static readonly char VolumeSeparatorChar = '/';

		private static readonly char[] _InvalidPathChars = new char[]{};

		public static char[] GetInvalidPathChars ()
		{
			return (char[]) _InvalidPathChars.Clone ();
		}

		public static string Combine (string path1, params string[] paths)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");
			if (paths == null)
				throw new ArgumentNullException ("paths");
			if (path1.IndexOfAny (_InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path1");

			int len = path1.Length;
			int start = -1;
			for (int i = 0; i < paths.Length; ++i) {
				if (paths [i] == null)
					throw new ArgumentNullException ("paths[" + i + "]");
				if (paths [i].IndexOfAny (_InvalidPathChars) != -1)
					throw new ArgumentException ("Illegal characters in path", "paths[" + i + "]");
				if (IsPathRooted (paths [i])) {
					len = 0;
					start = i;
				}
				len += paths [i].Length + 1;
			}

			StringBuilder sb = new StringBuilder (len);
			if (start == -1) {
				sb.Append (path1);
				start = 0;
			}
			for (int i = start; i < paths.Length; ++i)
				Combine (sb, paths [i]);
			return sb.ToString ();
		}

		private static void Combine (StringBuilder path, string part)
		{
			if (path.Length > 0 && part.Length > 0) {
				char end = path [path.Length-1];
				if (end != DirectorySeparatorChar && 
						end != AltDirectorySeparatorChar && 
						end != VolumeSeparatorChar)
					path.Append (DirectorySeparatorChar);
			}
			path.Append (part);
		}

		public static string GetDirectoryName (string path)
		{
			CheckPath (path);

			int lastDir = path.LastIndexOf (DirectorySeparatorChar);
			if (lastDir > 0)
				return path.Substring (0, lastDir);
			if (lastDir == 0)
				return "/";
			return "";
		}

		public static string GetFileName (string path)
		{
			if (path == null || path.Length == 0)
				return path;

			int lastDir = path.LastIndexOf (DirectorySeparatorChar);
			if (lastDir >= 0)
				return path.Substring (lastDir+1);

			return path;
		}

		public static string GetFullPath (string path)
		{
			path = _GetFullPath (path);
			return GetCanonicalPath (path);
		}

		private static string _GetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (!IsPathRooted (path))
				path = UnixDirectoryInfo.GetCurrentDirectory() + DirectorySeparatorChar + path;

			return path;
		}

		public static string GetCanonicalPath (string path)
		{
			string [] dirs;
			int lastIndex;
			GetPathComponents (path, out dirs, out lastIndex);
			string end = string.Join ("/", dirs, 0, lastIndex);
			return IsPathRooted (path) ? "/" + end : end;
		}

		private static void GetPathComponents (string path, 
			out string[] components, out int lastIndex)
		{
			string [] dirs = path.Split (DirectorySeparatorChar);
			int target = 0;
			for (int i = 0; i < dirs.Length; ++i) {
				if (dirs [i] == "." || dirs [i] == string.Empty) continue;
				else if (dirs [i] == "..") {
					if (target != 0) --target;
					else ++target;
				}
				else
					dirs [target++] = dirs [i];
			}
			components = dirs;
			lastIndex = target;
		}

		public static string GetPathRoot (string path)
		{
			if (path == null)
				return null;
			if (!IsPathRooted (path))
				return "";
			return "/";
		}

		public static string GetCompleteRealPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			string [] dirs;
			int lastIndex;
			GetPathComponents (path, out dirs, out lastIndex);
			StringBuilder realPath = new StringBuilder ();
			if (dirs.Length > 0) {
				string dir = IsPathRooted (path) ? "/" : "";
				dir += dirs [0];
				realPath.Append (GetRealPath (dir));
			}
			for (int i = 1; i < lastIndex; ++i) {
				realPath.Append ("/").Append (dirs [i]);
				string p = GetRealPath (realPath.ToString());
				realPath.Remove (0, realPath.Length);
				realPath.Append (p);
			}
			return realPath.ToString ();
		}

		public static string GetRealPath (string path)
		{
			do {
				string name = ReadSymbolicLink (path);
				if (name == null)
					return path;
				if (IsPathRooted (name))
					path = name;
				else {
					path = GetDirectoryName (path) + DirectorySeparatorChar + name;
					path = GetCanonicalPath (path);
				}
			} while (true);
		}

		// Read the specified symbolic link.  If the file isn't a symbolic link,
		// return null; otherwise, return the contents of the symbolic link.
		//
		// readlink(2) is horribly evil, as there is no way to query how big the
		// symlink contents are.  Consequently, it's trial and error...
		internal static string ReadSymbolicLink (string path)
		{
			StringBuilder buf = new StringBuilder (256);
			do {
				int r = Native.Syscall.readlink (path, buf);
				if (r < 0) {
					Native.Errno e;
					switch (e = Native.Stdlib.GetLastError()) {
					case Native.Errno.EINVAL:
						// path isn't a symbolic link
						return null;
					default:
						UnixMarshal.ThrowExceptionForError (e);
						break;
					}
				}
				else if (r == buf.Capacity) {
					buf.Capacity *= 2;
				}
				else
					return buf.ToString (0, r);
			} while (true);
		}

		// Read the specified symbolic link.  If the file isn't a symbolic link,
		// return null; otherwise, return the contents of the symbolic link.
		//
		// readlink(2) is horribly evil, as there is no way to query how big the
		// symlink contents are.  Consequently, it's trial and error...
		private static string ReadSymbolicLink (string path, out Native.Errno errno)
		{
			errno = (Native.Errno) 0;
			StringBuilder buf = new StringBuilder (256);
			do {
				int r = Native.Syscall.readlink (path, buf);
				if (r < 0) {
					errno = Native.Stdlib.GetLastError ();
					return null;
				}
				else if (r == buf.Capacity) {
					buf.Capacity *= 2;
				}
				else
					return buf.ToString (0, r);
			} while (true);
		}

		public static string TryReadLink (string path)
		{
			Native.Errno errno;
			return ReadSymbolicLink (path, out errno);
		}

		public static string ReadLink (string path)
		{
			Native.Errno errno;
			path = ReadSymbolicLink (path, out errno);
			if (errno != 0)
				UnixMarshal.ThrowExceptionForError (errno);
			return path;
		}

		public static bool IsPathRooted (string path)
		{
			if (path == null || path.Length == 0)
				return false;
			return path [0] == DirectorySeparatorChar;
		}

		internal static void CheckPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.Length == 0)
				throw new ArgumentException ("Path cannot contain a zero-length string", "path");
			if (path.IndexOfAny (_InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path.", "path");
		}
	}
}

// vim: noexpandtab
