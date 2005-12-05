//
// Mono.Unix/UnixFile.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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

	[Obsolete ("Use UnixFileInfo or FileHandleOperations", true)]
	public sealed /* static */ class UnixFile
	{
		private UnixFile () {}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).CanAccess (mode)")]
		public static bool CanAccess (string path, AccessMode mode)
		{
			int r = Syscall.access (path, mode);
			return r == 0;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).CanAccess (mode)")]
		public static bool CanAccess (string path, Native.AccessModes mode)
		{
			int r = Native.Syscall.access (path, mode);
			return r == 0;
		}

		[Obsolete ("Use new UnixFileInfo(path).Delete()")]
		public static void Delete (string path)
		{
			int r = Syscall.unlink (path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use new UnixFileInfo(path).Exists")]
		public static bool Exists (string path)
		{
			int r = Syscall.access (path, AccessMode.F_OK);
			if (r == 0)
				return true;
			return false;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).GetConfigurationValue(name)")]
		public static long GetConfigurationValue (string path, PathConf name)
		{
			long r = Syscall.pathconf (path, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).GetConfigurationValue(name)")]
		public static long GetConfigurationValue (string path, Native.PathconfName name)
		{
			long r = Native.Syscall.pathconf (path, name, (Native.Errno) 0);
			if (r == -1 && Native.Stdlib.GetLastError () != (Native.Errno) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[Obsolete ("Use new UnixFileInfo(path).LastAccessTime")]
		public static DateTime GetLastAccessTime (string path)
		{
			return new UnixFileInfo (path).LastAccessTime;
		}

		[Obsolete ("Use new UnixFileInfo(path).ToStat()")]
		public static Native.Stat GetFileStatus (string path)
		{
			Native.Stat stat;
			int r = Native.Syscall.stat (path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return stat;
		}

		[Obsolete ("Use new UnixFileInfo(path).LastWriteTime")]
		public static DateTime GetLastWriteTime (string path)
		{
			return new UnixFileInfo(path).LastWriteTime;
		}

		[Obsolete ("Use new UnixFileInfo(path).LastStatusChangeTime")]
		public static DateTime GetLastStatusChangeTime (string path)
		{
			return new UnixFileInfo (path).LastStatusChangeTime;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Protection")]
		public static FilePermissions GetPermissions (string path)
		{
			return new UnixFileInfo (path).Permissions;
		}

		[Obsolete ("Use new UnixSymbolicLinkInfo(path).ContentsPath")]
		public static string ReadLink (string path)
		{
			string r = TryReadLink (path);
			if (r == null)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[Obsolete ("Use new UnixSymbolicLinkInfo(path).TryReadLink")]
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

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Protection = perms")]
		public static void SetPermissions (string path, FilePermissions perms)
		{
			int r = Syscall.chmod (path, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Protection = perms")]
		public static void SetPermissions (string path, Native.FilePermissions perms)
		{
			int r = Native.Syscall.chmod (path, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use new UnixFileInfo(path).Create ()")]
		public static UnixStream Create (string path)
		{
			FilePermissions mode = // 0644
				FilePermissions.S_IRUSR | FilePermissions.S_IWUSR |
				FilePermissions.S_IRGRP | FilePermissions.S_IROTH; 
			return Create (path, mode);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Create (mode)")]
		public static UnixStream Create (string path, FilePermissions mode)
		{
			int fd = Syscall.creat (path, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Create (mode)")]
		public static UnixStream Create (string path, Native.FilePermissions mode)
		{
			int fd = Native.Syscall.creat (path, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[Obsolete ("Use UnixPipes.CreatePipes()")]
		public static UnixPipes CreatePipes ()
		{
			int reading, writing;
			int r = Syscall.pipe (out reading, out writing);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixPipes (new UnixStream (reading), new UnixStream (writing));
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (flags)")]
		public static UnixStream Open (string path, OpenFlags flags)
		{
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (flags)")]
		public static UnixStream Open (string path, Native.OpenFlags flags)
		{
			int fd = Native.Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (flags, mode)")]
		public static UnixStream Open (string path, OpenFlags flags, FilePermissions mode)
		{
			int fd = Syscall.open (path, flags, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (flags, mode)")]
		public static UnixStream Open (string path, Native.OpenFlags flags, Native.FilePermissions mode)
		{
			int fd = Native.Syscall.open (path, flags, mode);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[Obsolete ("Use new UnixFileInfo(path).Open (mode)")]
		public static UnixStream Open (string path, FileMode mode)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, FileAccess.ReadWrite);
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[Obsolete ("Use new UnixFileInfo(path).Open (mode, access)")]
		public static UnixStream Open (string path, FileMode mode, FileAccess access)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, access);
			int fd = Syscall.open (path, flags);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (mode, access, perms)")]
		public static UnixStream Open (string path, FileMode mode, FileAccess access, FilePermissions perms)
		{
			OpenFlags flags = UnixConvert.ToOpenFlags (mode, access);
			int fd = Syscall.open (path, flags, perms);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).Open (mode, access, perms)")]
		public static UnixStream Open (string path, FileMode mode, FileAccess access, Native.FilePermissions perms)
		{
			Native.OpenFlags flags = Native.NativeConvert.ToOpenFlags (mode, access);
			int fd = Native.Syscall.open (path, flags, perms);
			if (fd < 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return new UnixStream (fd);
		}

		[Obsolete ("Use new UnixFileInfo(path).OpenRead ()")]
		public static UnixStream OpenRead (string path)
		{
			return Open (path, FileMode.Open, FileAccess.Read);
		}

		[Obsolete ("Use new UnixFileInfo(path).OpenWrite ()")]
		public static UnixStream OpenWrite (string path)
		{
			return Open (path, FileMode.OpenOrCreate, FileAccess.Write);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixFileInfo(path).SetOwner (owner, group)")]
		public static void SetOwner (string path, uint owner, uint group)
		{
			int r = Syscall.chown (path, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use new UnixFileInfo(path).SetOwner (owner)")]
		public static void SetOwner (string path, string owner)
		{
			Passwd pw = Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner (path, uid, gid);
		}

		[Obsolete ("Use new UnixFileInfo(path).SetOwner (owner, group)")]
		public static void SetOwner (string path, string owner, string group)
		{
			uint uid = UnixUser.GetUserId (owner);
			uint gid = UnixGroup.GetGroupId (group);

			SetOwner (path, uid, gid);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use new UnixSymbolicLinkInfo(path).SetOwner (owner, group)")]
		public static void SetLinkOwner (string path, uint owner, uint group)
		{
			int r = Syscall.lchown (path, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use new UnixSymbolicLinkInfo(path).SetOwner (owner)")]
		public static void SetLinkOwner (string path, string owner)
		{
			Passwd pw = Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetLinkOwner (path, uid, gid);
		}

		[Obsolete ("Use new UnixSymbolicLinkInfo(path).SetOwner (owner, group)")]
		public static void SetLinkOwner (string path, string owner, string group)
		{
			uint uid = UnixUser.GetUserId (owner);
			uint gid = UnixGroup.GetGroupId (group);

			SetLinkOwner (path, uid, gid);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Normal, offset, len)")]
		public static void AdviseNormalAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_NORMAL);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Normal)")]
		public static void AdviseNormalAccess (int fd)
		{
			AdviseNormalAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Normal, offset, len)")]
		public static void AdviseNormalAccess (FileStream file, long offset, long len)
		{
			AdviseNormalAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Normal)")]
		public static void AdviseNormalAccess (FileStream file)
		{
			AdviseNormalAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Normal, offset, len)")]
		public static void AdviseNormalAccess (UnixStream stream, long offset, long len)
		{
			AdviseNormalAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Normal)")]
		public static void AdviseNormalAccess (UnixStream stream)
		{
			AdviseNormalAccess (stream.Handle);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Sequential, offset, len)")]
		public static void AdviseSequentialAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_SEQUENTIAL);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Sequential)")]
		public static void AdviseSequentialAccess (int fd)
		{
			AdviseSequentialAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Sequential, offset, len)")]
		public static void AdviseSequentialAccess (FileStream file, long offset, long len)
		{
			AdviseSequentialAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Sequential)")]
		public static void AdviseSequentialAccess (FileStream file)
		{
			AdviseSequentialAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Sequential, offset, len)")]
		public static void AdviseSequentialAccess (UnixStream stream, long offset, long len)
		{
			AdviseSequentialAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Sequential)")]
		public static void AdviseSequentialAccess (UnixStream stream)
		{
			AdviseSequentialAccess (stream.Handle);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Random, offset, len)")]
		public static void AdviseRandomAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_RANDOM);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.Random)")]
		public static void AdviseRandomAccess (int fd)
		{
			AdviseRandomAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Random, offset, len)")]
		public static void AdviseRandomAccess (FileStream file, long offset, long len)
		{
			AdviseRandomAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.Random)")]
		public static void AdviseRandomAccess (FileStream file)
		{
			AdviseRandomAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Random, offset, len)")]
		public static void AdviseRandomAccess (UnixStream stream, long offset, long len)
		{
			AdviseRandomAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.Random)")]
		public static void AdviseRandomAccess (UnixStream stream)
		{
			AdviseRandomAccess (stream.Handle);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.PreLoad, offset, len)")]
		public static void AdviseNeedAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_WILLNEED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.PreLoad)")]
		public static void AdviseNeedAccess (int fd)
		{
			AdviseNeedAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.PreLoad, offset, len)")]
		public static void AdviseNeedAccess (FileStream file, long offset, long len)
		{
			AdviseNeedAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.PreLoad)")]
		public static void AdviseNeedAccess (FileStream file)
		{
			AdviseNeedAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.PreLoad, offset, len)")]
		public static void AdviseNeedAccess (UnixStream stream, long offset, long len)
		{
			AdviseNeedAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.PreLoad)")]
		public static void AdviseNeedAccess (UnixStream stream)
		{
			AdviseNeedAccess (stream.Handle);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.FlushCache, offset, len)")]
		public static void AdviseNoAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_DONTNEED);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.FlushCache)")]
		public static void AdviseNoAccess (int fd)
		{
			AdviseNoAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.FlushCache, offset, len)")]
		public static void AdviseNoAccess (FileStream file, long offset, long len)
		{
			AdviseNoAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.FlushCache)")]
		public static void AdviseNoAccess (FileStream file)
		{
			AdviseNoAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.FlushCache, offset, len)")]
		public static void AdviseNoAccess (UnixStream stream, long offset, long len)
		{
			AdviseNoAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.FlushCache)")]
		public static void AdviseNoAccess (UnixStream stream)
		{
			AdviseNoAccess (stream.Handle);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.NoReuse, offset, len)")]
		public static void AdviseOnceAccess (int fd, long offset, long len)
		{
			int r = Syscall.posix_fadvise (fd, offset, len,
				PosixFadviseAdvice.POSIX_FADV_NOREUSE);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (fd, FileAccessPattern.NoReuse)")]
		public static void AdviseOnceAccess (int fd)
		{
			AdviseOnceAccess (fd, 0, 0);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.NoReuse, offset, len)")]
		public static void AdviseOnceAccess (FileStream file, long offset, long len)
		{
			AdviseOnceAccess (file.Handle.ToInt32(), offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (file, FileAccessPattern.NoReuse)")]
		public static void AdviseOnceAccess (FileStream file)
		{
			AdviseOnceAccess (file.Handle.ToInt32());
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.NoReuse, offset, len)")]
		public static void AdviseOnceAccess (UnixStream stream, long offset, long len)
		{
			AdviseOnceAccess (stream.Handle, offset, len);
		}

		[Obsolete ("Use FileHandleOperations.AdviseFileAccessPattern (stream, FileAccessPattern.NoReuse)")]
		public static void AdviseOnceAccess (UnixStream stream)
		{
			AdviseOnceAccess (stream.Handle);
		}
	}
}

// vim: noexpandtab
