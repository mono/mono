//
// Mono.Unix/UnixPath.cs
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
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixPath
	{
		private UnixPath () {}

		public static readonly char DirectorySeparatorChar = '/';
		public static readonly char AltDirectorySeparatorChar = '/';
		public static readonly char[] InvalidPathChars = new char[]{'\0'};
		public static readonly char PathSeparator = ':';
		public static readonly char VolumeSeparatorChar = '/';

		public static string Combine (string path1, params string[] paths)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");
			if (paths == null)
				throw new ArgumentNullException ("paths");
			if (path1.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path1");

			int len = path1.Length + 1;
			for (int i = 0; i < paths.Length; ++i) {
				if (paths [i] == null)
					throw new ArgumentNullException ("paths");
				len += paths [i].Length + 1;
			}

			StringBuilder sb = new StringBuilder (len);
			sb.Append (path1);
			for (int i = 0; i < paths.Length; ++i)
				Combine (sb, paths [i]);
			return sb.ToString ();
		}

		private static void Combine (StringBuilder path, string part)
		{
			if (part.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path1");
			char end = path [path.Length-1];
			if (end != DirectorySeparatorChar && 
					end != AltDirectorySeparatorChar && 
					end != VolumeSeparatorChar)
				path.Append (DirectorySeparatorChar);
			path.Append (part);
		}

		public static string GetDirectoryName (string path)
		{
			CheckPath (path);

			int lastDir = path.LastIndexOf (DirectorySeparatorChar);
			if (lastDir > 0)
				return path.Substring (0, lastDir);
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
			string [] dirs;
			int lastIndex;
			GetPathComponents (path, out dirs, out lastIndex);
			return string.Join ("/", dirs, 0, lastIndex);
		}

		private static string _GetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (!IsPathRooted (path))
				path = UnixDirectory.GetCurrentDirectory() + DirectorySeparatorChar + path;

			return path;
		}

		private static void GetPathComponents (string path, 
			out string[] components, out int lastIndex)
		{
			string [] dirs = path.Split (DirectorySeparatorChar);
			int target = 0;
			for (int i = 0; i < dirs.Length; ++i) {
				if (dirs [i] == "." || (i != 0 && dirs [i] == string.Empty)) continue;
				else if (dirs [i] == "..") {
					if (target != 0) --target;
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

		public static string GetRealPath (string path)
		{
			path = _GetFullPath (path);
			string [] dirs;
			int lastIndex;
			GetPathComponents (path, out dirs, out lastIndex);
			StringBuilder realPath = new StringBuilder ();
			for (int i = 0; i < dirs.Length; ++i) {
				realPath.Append ("/").Append (dirs [i]);
				string p = _GetRealPath (realPath.ToString());
				realPath.Remove (0, realPath.Length);
				realPath.Append (p);
			}
			return realPath.ToString ();
		}

		private static string _GetRealPath (string path)
		{
			StringBuilder buf = new StringBuilder (1024);
			int r;
			do {
				r = Syscall.readlink (path, buf);
				if (r == -1) {
					Error e;
					switch (e = Syscall.GetLastError()) {
					case Error.EINVAL:
						// path isn't a symbolic link
						return path;
					default:
						UnixMarshal.ThrowExceptionForError (e);
						break;
					}
				}
				path = buf.ToString ();
			} while (r == 0);

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
			if (path.IndexOfAny (UnixPath.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path.");
		}
	}
}

// vim: noexpandtab
