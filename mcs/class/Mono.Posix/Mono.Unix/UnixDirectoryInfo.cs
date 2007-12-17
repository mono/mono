//
// Mono.Unix/UnixDirectoryInfo.cs
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
using System.Text;
using System.Text.RegularExpressions;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixDirectoryInfo : UnixFileSystemInfo
	{
		public UnixDirectoryInfo (string path)
			: base (path)
		{
		}

		internal UnixDirectoryInfo (string path, Native.Stat stat)
			: base (path, stat)
		{
		}

		public override string Name {
			get {
				string r = UnixPath.GetFileName (FullPath);
				if (r == null || r.Length == 0)
					return FullPath;
				return r;
			}
		}

		public UnixDirectoryInfo Parent {
			get {
				if (FullPath == "/")
					return this;
				string dirname = UnixPath.GetDirectoryName (FullPath);
				if (dirname == "")
					throw new InvalidOperationException ("Do not know parent directory for path `" + FullPath + "'");
				return new UnixDirectoryInfo (dirname);
			}
		}

		public UnixDirectoryInfo Root {
			get {
				string root = UnixPath.GetPathRoot (FullPath);
				if (root == null)
					return null;
				return new UnixDirectoryInfo (root);
			}
		}

		[CLSCompliant (false)]
		public void Create (Mono.Unix.Native.FilePermissions mode)
		{
			int r = Mono.Unix.Native.Syscall.mkdir (FullPath, mode);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public void Create (FileAccessPermissions mode)
		{
			Create ((Native.FilePermissions) mode);
		}

		public void Create ()
		{
			Mono.Unix.Native.FilePermissions mode = 
				Mono.Unix.Native.FilePermissions.ACCESSPERMS;
			Create (mode);
		}

		public override void Delete ()
		{
			Delete (false);
		}

		public void Delete (bool recursive)
		{
			if (recursive) {
				foreach (UnixFileSystemInfo e in GetFileSystemEntries ()) {
					UnixDirectoryInfo d = e as UnixDirectoryInfo;
					if (d != null)
						d.Delete (true);
					else
						e.Delete ();
				}
			}
			int r = Native.Syscall.rmdir (FullPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public Native.Dirent[] GetEntries ()
		{
			IntPtr dirp = Native.Syscall.opendir (FullPath);
			if (dirp == IntPtr.Zero)
				UnixMarshal.ThrowExceptionForLastError ();

			bool complete = false;
			try {
				Native.Dirent[] entries = GetEntries (dirp);
				complete = true;
				return entries;
			}
			finally {
				int r = Native.Syscall.closedir (dirp);
				// don't throw an exception if an exception is in progress
				if (complete)
					UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		private static Native.Dirent[] GetEntries (IntPtr dirp)
		{
			ArrayList entries = new ArrayList ();

			int r;
			IntPtr result;
			do {
				Native.Dirent d = new Native.Dirent ();
				r = Native.Syscall.readdir_r (dirp, d, out result);
				if (r == 0 && result != IntPtr.Zero)
					// don't include current & parent dirs
					if (d.d_name != "." && d.d_name != "..")
						entries.Add (d);
			} while  (r == 0 && result != IntPtr.Zero);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

			return (Native.Dirent[]) entries.ToArray (typeof(Native.Dirent));
		}

		public Native.Dirent[] GetEntries (Regex regex)
		{
			IntPtr dirp = Native.Syscall.opendir (FullPath);
			if (dirp == IntPtr.Zero)
				UnixMarshal.ThrowExceptionForLastError ();

			try {
				return GetEntries (dirp, regex);
			}
			finally {
				int r = Native.Syscall.closedir (dirp);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		private static Native.Dirent[] GetEntries (IntPtr dirp, Regex regex)
		{
			ArrayList entries = new ArrayList ();

			int r;
			IntPtr result;
			do {
				Native.Dirent d = new Native.Dirent ();
				r = Native.Syscall.readdir_r (dirp, d, out result);
				if (r == 0 && result != IntPtr.Zero && regex.Match (d.d_name).Success) {
					// don't include current & parent dirs
					if (d.d_name != "." && d.d_name != "..")
						entries.Add (d);
				}
			} while  (r == 0 && result != IntPtr.Zero);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();

			return (Native.Dirent[]) entries.ToArray (typeof(Native.Dirent));
		}

		public Native.Dirent[] GetEntries (string regex)
		{
			Regex re = new Regex (regex);
			return GetEntries (re);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries ()
		{
			Native.Dirent[] dentries = GetEntries ();
			return GetFileSystemEntries (dentries);
		}

		private UnixFileSystemInfo[] GetFileSystemEntries (Native.Dirent[] dentries)
		{
			UnixFileSystemInfo[] entries = new UnixFileSystemInfo[dentries.Length];
			for (int i = 0; i != entries.Length; ++i)
				entries [i] = UnixFileSystemInfo.GetFileSystemEntry (
						UnixPath.Combine (FullPath, dentries[i].d_name));
			return entries;
		}

		public UnixFileSystemInfo[] GetFileSystemEntries (Regex regex)
		{
			Native.Dirent[] dentries = GetEntries (regex);
			return GetFileSystemEntries (dentries);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries (string regex)
		{
			Regex re = new Regex (regex);
			return GetFileSystemEntries (re);
		}

		public static string GetCurrentDirectory ()
		{
			StringBuilder buf = new StringBuilder (16);
			IntPtr r = IntPtr.Zero;
			do {
				buf.Capacity *= 2;
				r = Native.Syscall.getcwd (buf, (ulong) buf.Capacity);
			} while (r == IntPtr.Zero && Native.Syscall.GetLastError() == Native.Errno.ERANGE);
			if (r == IntPtr.Zero)
				UnixMarshal.ThrowExceptionForLastError ();
			return buf.ToString ();
		}

		public static void SetCurrentDirectory (string path)
		{
			int r = Native.Syscall.chdir (path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}
	}
}

// vim: noexpandtab
