//
// Mono.Unix/UnixFileInfo.cs
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
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixFileInfo : UnixFileSystemInfo
	{
		public UnixFileInfo (string path)
			: base (path)
		{
		}

		internal UnixFileInfo (string path, Stat stat)
			: base (path, stat)
		{
		}

		public override string Name {
			get {return UnixPath.GetFileName (FullPath);}
		}

		public string DirectoryName {
			get {return UnixPath.GetDirectoryName (FullPath);}
		}

		public UnixDirectoryInfo Directory {
			get {return new UnixDirectoryInfo (DirectoryName);}
		}

		public override void Delete ()
		{
			int r = Syscall.unlink (FullPath);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			base.Refresh ();
		}

		public UnixStream Create ()
		{
			FilePermissions mode = // 0644
				FilePermissions.S_IRUSR | FilePermissions.S_IWUSR |
				FilePermissions.S_IRGRP | FilePermissions.S_IROTH; 
			return Create (mode);
		}

		public UnixStream Create (FilePermissions mode)
		{
			int fd = Syscall.creat (FullPath, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			base.Refresh ();
			return new UnixStream (fd);
		}

		public UnixStream Open (OpenFlags flags)
		{
			int fd = Syscall.open (FullPath, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public UnixStream Open (OpenFlags flags, FilePermissions mode)
		{
			int fd = Syscall.open (FullPath, flags, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public UnixStream Open (FileMode mode)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, FileAccess.ReadWrite);
			int fd = Syscall.open (FullPath, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public UnixStream Open (FileMode mode, FileAccess access)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, access);
			int fd = Syscall.open (FullPath, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public UnixStream Open (FileMode mode, FileAccess access, FilePermissions perms)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, access);
			int fd = Syscall.open (FullPath, flags, perms);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public UnixStream OpenRead ()
		{
			return Open (FileMode.Open, FileAccess.Read);
		}

		public UnixStream OpenWrite ()
		{
			return Open (FileMode.OpenOrCreate, FileAccess.Write);
		}
	}
}

// vim: noexpandtab
