//
// Mono.Unix/UnixDirectoryInfo.cs
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
using Mono.Unix;

namespace Mono.Unix {

	public class UnixDirectoryInfo : UnixFileSystemInfo
	{
		public UnixDirectoryInfo (string path)
			: base (path)
		{
		}

		internal UnixDirectoryInfo (string path, Stat stat)
			: base (path, stat)
		{
		}

		public void Create (FilePermissions mode)
		{
			int r = Syscall.mkdir (Path, mode);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public void Create ()
		{
			FilePermissions mode = FilePermissions.ACCESSPERMS;
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
			int r = Syscall.rmdir (Path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public Dirent[] GetEntries ()
		{
			IntPtr dirp = Syscall.opendir (Path);
			if (dirp == IntPtr.Zero)
				UnixMarshal.ThrowExceptionForLastError ();

			bool complete = false;
			try {
				Dirent[] entries = GetEntries (dirp);
				complete = true;
				return entries;
			}
			finally {
				int r = Syscall.closedir (dirp);
				// don't throw an exception if an exception is in progress
				if (complete)
					UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		private static Dirent[] GetEntries (IntPtr dirp)
		{
			ArrayList entries = new ArrayList ();

			int r;
			IntPtr result;
			do {
				Dirent d = new Dirent ();
				r = Syscall.readdir_r (dirp, d, out result);
				if (r == 0 && result != IntPtr.Zero)
					// don't include current & parent dirs
					if (d.d_name != "." && d.d_name != "..")
						entries.Add (d);
			} while  (r == 0 && result != IntPtr.Zero);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastErrorIf (r);

			return (Dirent[]) entries.ToArray (typeof(Dirent));
		}

		public Dirent[] GetEntries (Regex regex)
		{
			IntPtr dirp = Syscall.opendir (Path);
			if (dirp == IntPtr.Zero)
				UnixMarshal.ThrowExceptionForLastError ();

			try {
				return GetEntries (dirp, regex);
			}
			finally {
				int r = Syscall.closedir (dirp);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		private static Dirent[] GetEntries (IntPtr dirp, Regex regex)
		{
			ArrayList entries = new ArrayList ();

			int r;
			IntPtr result;
			do {
				Dirent d = new Dirent ();
				r = Syscall.readdir_r (dirp, d, out result);
				if (r == 0 && result != IntPtr.Zero && regex.Match (d.d_name).Success) {
					// don't include current & parent dirs
					if (d.d_name != "." && d.d_name != "..")
						entries.Add (d);
				}
			} while  (r == 0 && result != IntPtr.Zero);
			if (r != 0)
				UnixMarshal.ThrowExceptionForLastError ();

			return (Dirent[]) entries.ToArray (typeof(Dirent));
		}

		public Dirent[] GetEntries (string regex)
		{
			Regex re = new Regex (regex);
			return GetEntries (re);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries ()
		{
			Dirent[] dentries = GetEntries ();
			return GetFileSystemEntries (dentries);
		}

		private UnixFileSystemInfo[] GetFileSystemEntries (Dirent[] dentries)
		{
			UnixFileSystemInfo[] entries = new UnixFileSystemInfo[dentries.Length];
			for (int i = 0; i != entries.Length; ++i)
				entries [i] = UnixFileSystemInfo.Create (dentries[i].d_name);
			return entries;
		}

		public UnixFileSystemInfo[] GetFileSystemEntries (Regex regex)
		{
			Dirent[] dentries = GetEntries (regex);
			return GetFileSystemEntries (dentries);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries (string regex)
		{
			Regex re = new Regex (regex);
			return GetFileSystemEntries (re);
		}
	}
}

// vim: noexpandtab
