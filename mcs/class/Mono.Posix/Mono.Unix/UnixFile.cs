//
// Mono.Unix/UnixFile.cs
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

	public struct UnixPipes
	{
		public UnixPipes (UnixStream reading, UnixStream writing)
		{
			Reading = reading;
			Writing = writing;
		}

		public UnixStream Reading;
		public UnixStream Writing;
	}

	public sealed /* static */ class UnixFile
	{
		private UnixFile () {}

		public static bool CanAccess (string path, AccessMode mode)
		{
			int r = Syscall.access (path, mode);
			return r == 0;
		}

		public static void Delete (string path)
		{
			int r = Syscall.unlink (path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static bool Exists (string path)
		{
			int r = Syscall.access (path, AccessMode.F_OK);
			if (r == 0)
				return true;
			return false;
		}

		public static long GetConfigurationValue (string path, PathConf name)
		{
			Syscall.SetLastError ((Error) 0);
			long r = Syscall.pathconf (path, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public static DateTime GetLastAccessTime (string path)
		{
			return new UnixFileInfo (path).LastAccessTime;
		}

		public static Stat GetFileStatus (string path)
		{
			Stat stat;
			int r = Syscall.stat (path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return stat;
		}

		public static DateTime GetLastWriteTime (string path)
		{
			return new UnixFileInfo(path).LastWriteTime;
		}

		public static DateTime GetLastStatusChangeTime (string path)
		{
			return new UnixFileInfo (path).LastStatusChangeTime;
		}

		public static FilePermissions GetPermissions (string path)
		{
			return new UnixFileInfo (path).Permissions;
		}

		public static string ReadLink (string path)
		{
			string r = TryReadLink (path);
			if (r == null)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public static string TryReadLink (string path)
		{
			// Who came up with readlink(2)?  There doesn't seem to be a way to
			// properly handle it.
			StringBuilder sb = new StringBuilder (512);
			int r = Syscall.readlink (path, sb);
			if (r == -1)
				return null;
			return sb.ToString (0, r);
		}

		public static void SetPermissions (string path, FilePermissions perms)
		{
			int r = Syscall.chmod (path, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static UnixStream Create (string path)
		{
			FilePermissions mode = // 0644
				FilePermissions.S_IRUSR | FilePermissions.S_IWUSR |
				FilePermissions.S_IRGRP | FilePermissions.S_IROTH; 
			return Create (path, mode);
		}

		public static UnixStream Create (string path, FilePermissions mode)
		{
			int fd = Syscall.creat (path, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixPipes CreatePipes ()
		{
			int reading, writing;
			int r = Syscall.pipe (out reading, out writing);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixPipes (new UnixStream (reading), new UnixStream (writing));
		}

		public static UnixStream Open (string path, OpenFlags flags)
		{
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixStream Open (string path, OpenFlags flags, FilePermissions mode)
		{
			int fd = Syscall.open (path, flags, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixStream Open (string path, FileMode mode)
		{
			OpenFlags flags = ToOpenFlags (mode, FileAccess.ReadWrite);
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixStream Open (string path, FileMode mode, FileAccess access)
		{
			OpenFlags flags = ToOpenFlags (mode, access);
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixStream Open (string path, FileMode mode, FileAccess access, FilePermissions perms)
		{
			OpenFlags flags = ToOpenFlags (mode, access);
			int fd = Syscall.open (path, flags, perms);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		public static UnixStream OpenRead (string path)
		{
			return Open (path, FileMode.Open, FileAccess.Read);
		}

		public static UnixStream OpenWrite (string path)
		{
			return Open (path, FileMode.OpenOrCreate, FileAccess.Write);
		}

		public static void SetOwner (string path, uint owner, uint group)
		{
			int r = Syscall.chown (path, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void SetOwner (string path, string owner)
		{
			Passwd pw = Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner (path, uid, gid);
		}

		public static void SetOwner (string path, string owner, string group)
		{
			uint uid = UnixUser.GetUserId (owner);
			uint gid = UnixGroup.GetGroupId (group);

			SetOwner (path, uid, gid);
		}

		public static void SetLinkOwner (string path, uint owner, uint group)
		{
			int r = Syscall.lchown (path, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void SetLinkOwner (string path, string owner)
		{
			Passwd pw = Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetLinkOwner (path, uid, gid);
		}

		public static void SetLinkOwner (string path, string owner, string group)
		{
			uint uid = UnixUser.GetUserId (owner);
			uint gid = UnixGroup.GetGroupId (group);

			SetLinkOwner (path, uid, gid);
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
				throw new ArgumentException (Locale.GetText ("Unsupported mode value"), "mode");
			}

			// Is O_LARGEFILE supported?
			int _v;
			if (UnixConvert.TryFromOpenFlags (OpenFlags.O_LARGEFILE, out _v))
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
				throw new ArgumentException (Locale.GetText ("Unsupported access value"), "access");
			}

			return flags;
		}

		public static void AdviseNormalAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_NORMAL);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseNormalAccess (int fd)
		{
			AdviseNormalAccess (fd, 0, 0);
		}

		public static void AdviseNormalAccess (FileStream file, long offset, long len)
		{
			AdviseNormalAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseNormalAccess (FileStream file)
		{
			AdviseNormalAccess (file.Handle.ToInt32());
		}

		public static void AdviseNormalAccess (UnixStream stream, long offset, long len)
		{
			AdviseNormalAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseNormalAccess (UnixStream stream)
		{
			AdviseNormalAccess (stream.FileDescriptor);
		}

		public static void AdviseSequentialAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_SEQUENTIAL);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseSequentialAccess (int fd)
		{
			AdviseSequentialAccess (fd, 0, 0);
		}

		public static void AdviseSequentialAccess (FileStream file, long offset, long len)
		{
			AdviseSequentialAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseSequentialAccess (FileStream file)
		{
			AdviseSequentialAccess (file.Handle.ToInt32());
		}

		public static void AdviseSequentialAccess (UnixStream stream, long offset, long len)
		{
			AdviseSequentialAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseSequentialAccess (UnixStream stream)
		{
			AdviseSequentialAccess (stream.FileDescriptor);
		}

		public static void AdviseRandomAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_RANDOM);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseRandomAccess (int fd)
		{
			AdviseRandomAccess (fd, 0, 0);
		}

		public static void AdviseRandomAccess (FileStream file, long offset, long len)
		{
			AdviseRandomAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseRandomAccess (FileStream file)
		{
			AdviseRandomAccess (file.Handle.ToInt32());
		}

		public static void AdviseRandomAccess (UnixStream stream, long offset, long len)
		{
			AdviseRandomAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseRandomAccess (UnixStream stream)
		{
			AdviseRandomAccess (stream.FileDescriptor);
		}

		public static void AdviseNeedAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_WILLNEED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseNeedAccess (int fd)
		{
			AdviseNeedAccess (fd, 0, 0);
		}

		public static void AdviseNeedAccess (FileStream file, long offset, long len)
		{
			AdviseNeedAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseNeedAccess (FileStream file)
		{
			AdviseNeedAccess (file.Handle.ToInt32());
		}

		public static void AdviseNeedAccess (UnixStream stream, long offset, long len)
		{
			AdviseNeedAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseNeedAccess (UnixStream stream)
		{
			AdviseNeedAccess (stream.FileDescriptor);
		}

		public static void AdviseNoAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_DONTNEED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseNoAccess (int fd)
		{
			AdviseNoAccess (fd, 0, 0);
		}

		public static void AdviseNoAccess (FileStream file, long offset, long len)
		{
			AdviseNoAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseNoAccess (FileStream file)
		{
			AdviseNoAccess (file.Handle.ToInt32());
		}

		public static void AdviseNoAccess (UnixStream stream, long offset, long len)
		{
			AdviseNoAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseNoAccess (UnixStream stream)
		{
			AdviseNoAccess (stream.FileDescriptor);
		}

		public static void AdviseOnceAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_NOREUSE);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public static void AdviseOnceAccess (int fd)
		{
			AdviseOnceAccess (fd, 0, 0);
		}

		public static void AdviseOnceAccess (FileStream file, long offset, long len)
		{
			AdviseOnceAccess (file.Handle.ToInt32(), offset, len);
		}

		public static void AdviseOnceAccess (FileStream file)
		{
			AdviseOnceAccess (file.Handle.ToInt32());
		}

		public static void AdviseOnceAccess (UnixStream stream, long offset, long len)
		{
			AdviseOnceAccess (stream.FileDescriptor, offset, len);
		}

		public static void AdviseOnceAccess (UnixStream stream)
		{
			AdviseOnceAccess (stream.FileDescriptor);
		}
	}
}

// vim: noexpandtab
