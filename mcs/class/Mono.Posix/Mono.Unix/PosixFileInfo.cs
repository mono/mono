//
// Mono.Posix/PosixFileInfo.cs
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
using System.Text;
using Mono.Posix;

namespace Mono.Posix {

	public class PosixFileInfo : PosixFileSystemInfo
	{
		public PosixFileInfo (string path)
			: base (path)
		{
		}

		internal PosixFileInfo (string path, Stat stat)
			: base (path, stat)
		{
		}

		public override void Delete ()
		{
			int r = Syscall.unlink (Path);
			PosixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public PosixStream Create ()
		{
			FilePermissions mode = // 0644
				FilePermissions.S_IRUSR | FilePermissions.S_IWUSR |
				FilePermissions.S_IRGRP | FilePermissions.S_IROTH; 
			return Create (mode);
		}

		public PosixStream Create (FilePermissions mode)
		{
			int fd = Syscall.creat (Path, mode);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			base.Refresh ();
			return new PosixStream (fd);
		}

		public PosixStream Open (OpenFlags flags)
		{
			int fd = Syscall.open (Path, flags);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			return new PosixStream (fd);
		}

		public PosixStream Open (OpenFlags flags, FilePermissions mode)
		{
			int fd = Syscall.open (Path, flags, mode);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			return new PosixStream (fd);
		}

		public PosixStream Open (FileMode mode)
		{
			OpenFlags flags = ToOpenFlags (mode, FileAccess.ReadWrite);
			int fd = Syscall.open (Path, flags);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			return new PosixStream (fd);
		}

		public PosixStream Open (FileMode mode, FileAccess access)
		{
			OpenFlags flags = ToOpenFlags (mode, access);
			int fd = Syscall.open (Path, flags);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			return new PosixStream (fd);
		}

		public PosixStream Open (FileMode mode, FileAccess access, FilePermissions perms)
		{
			OpenFlags flags = ToOpenFlags (mode, access);
			int fd = Syscall.open (Path, flags, perms);
			if (fd < 0)
				PosixMarshal.ThrowExceptionForLastError ();
			return new PosixStream (fd);
		}

		public PosixStream OpenRead ()
		{
			return Open (FileMode.Open, FileAccess.Read);
		}

		public PosixStream OpenWrite ()
		{
			return Open (FileMode.OpenOrCreate, FileAccess.Write);
		}

		public static OpenFlags ToOpenFlags (FileMode mode, FileAccess access)
		{
			OpenFlags flags = 0;
			switch (mode) {
			case FileMode.CreateNew:
				flags = OpenFlags.O_CREAT | OpenFlags.O_EXCL;
				break;
			case FileMode.Create:
				flags = OpenFlags.O_CREAT | OpenFlags.O_TRUNC;
				break;
			case FileMode.Open:
				// do nothing
				break;
			case FileMode.OpenOrCreate:
				flags = OpenFlags.O_CREAT;
				break;
			case FileMode.Truncate:
				flags = OpenFlags.O_TRUNC;
				break;
			case FileMode.Append:
				flags = OpenFlags.O_APPEND;
				break;
			default:
				throw new ArgumentOutOfRangeException ("mode", mode, 
						Locale.GetText ("Unsupported mode value"));
			}

			int _ignored;
			if (PosixConvert.TryFromOpenFlags (OpenFlags.O_LARGEFILE, out _ignored))
				flags |= OpenFlags.O_LARGEFILE;

			switch (access) {
			case FileAccess.Read:
				flags |= OpenFlags.O_RDONLY;
				break;
			case FileAccess.Write:
				flags |= OpenFlags.O_WRONLY;
				break;
			case FileAccess.ReadWrite:
				flags |= OpenFlags.O_RDWR;
				break;
			default:
				throw new ArgumentOutOfRangeException ("access", access,
						Locale.GetText ("Unsupported access value"));
			}

			return flags;
		}
	}
}

// vim: noexpandtab
