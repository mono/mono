//
// Mono.Unix/UnixFileSystemInfo.cs
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
using System.IO;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public abstract class UnixFileSystemInfo
	{
		private Native.Stat stat;
		private string fullPath;
		private string originalPath;
		private bool valid = false;

		internal const FileSpecialAttributes AllSpecialAttributes = 
			FileSpecialAttributes.SetUserId | FileSpecialAttributes.SetGroupId |
			FileSpecialAttributes.Sticky;
		internal const FileTypes AllFileTypes = 
			FileTypes.Directory | FileTypes.CharacterDevice | FileTypes.BlockDevice |
			FileTypes.RegularFile | FileTypes.Fifo | FileTypes.SymbolicLink | 
			FileTypes.Socket;

		protected UnixFileSystemInfo (string path)
		{
			UnixPath.CheckPath (path);
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			Refresh (true);
		}

		internal UnixFileSystemInfo (String path, Native.Stat stat)
		{
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			this.stat = stat;
			this.valid = true;
		}

		protected string FullPath {
			get {return fullPath;}
			set {
				if (fullPath != value) {
					UnixPath.CheckPath (value);
					valid = false;
					fullPath = value;
				}
			}
		}

		protected string OriginalPath {
			get {return originalPath;}
			set {originalPath = value;}
		}

		private void AssertValid ()
		{
			Refresh (false);
			if (!valid)
				throw new InvalidOperationException ("Path doesn't exist!");
		}

		public virtual string FullName {
			get {return FullPath;}
		}

		public abstract string Name {get;}

		public bool Exists {
			get {
				Refresh (true);
				return valid;
			}
		}

		public long Device {
			get {AssertValid (); return Convert.ToInt64 (stat.st_dev);}
		}

		public long Inode {
			get {AssertValid (); return Convert.ToInt64 (stat.st_ino);}
		}

		[CLSCompliant (false)]
		public Native.FilePermissions Protection {
			get {AssertValid (); return (Native.FilePermissions) stat.st_mode;}
			set {
				int r = Native.Syscall.chmod (FullPath, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public FileTypes FileType {
			get {
				AssertValid ();
				return (FileTypes) (stat.st_mode & Native.FilePermissions.S_IFMT);
			}
			// no set as chmod(2) won't accept changing the file type.
		}

		public FileAccessPermissions FileAccessPermissions {
			get {
				AssertValid (); 
				int perms = (int) stat.st_mode;
				return (FileAccessPermissions) (perms & (int) FileAccessPermissions.AllPermissions);
			}
			set {
				AssertValid ();
				int perms = (int) stat.st_mode;
				perms &= (int) ~FileAccessPermissions.AllPermissions;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public FileSpecialAttributes FileSpecialAttributes {
			get {
				AssertValid ();
				int attrs = (int) stat.st_mode;
				return (FileSpecialAttributes) (attrs & (int) AllSpecialAttributes);
			}
			set {
				AssertValid ();
				int perms = (int) stat.st_mode;
				perms &= (int) ~AllSpecialAttributes;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public long LinkCount {
			get {AssertValid (); return Convert.ToInt64 (stat.st_nlink);}
		}

		public UnixUserInfo OwnerUser {
			get {AssertValid (); return new UnixUserInfo (stat.st_uid);}
		}

		public long OwnerUserId {
			get {AssertValid (); return stat.st_uid;}
		}

		public UnixGroupInfo OwnerGroup {
			get {AssertValid (); return new UnixGroupInfo (stat.st_gid);}
		}

		public long OwnerGroupId {
			get {AssertValid (); return stat.st_gid;}
		}

		public long DeviceType {
			get {AssertValid (); return Convert.ToInt64 (stat.st_rdev);}
		}

		public long Length {
			get {AssertValid (); return (long) stat.st_size;}
		}

		public long BlockSize {
			get {AssertValid (); return (long) stat.st_blksize;}
		}

		public long BlocksAllocated {
			get {AssertValid (); return (long) stat.st_blocks;}
		}

		public DateTime LastAccessTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_atime);}
		}

		public DateTime LastAccessTimeUtc {
			get {return LastAccessTime.ToUniversalTime ();}
		}

		public DateTime LastWriteTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_mtime);}
		}

		public DateTime LastWriteTimeUtc {
			get {return LastWriteTime.ToUniversalTime ();}
		}

		public DateTime LastStatusChangeTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_ctime);}
		}

		public DateTime LastStatusChangeTimeUtc {
			get {return LastStatusChangeTime.ToUniversalTime ();}
		}

		public bool IsDirectory {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFDIR);}
		}

		public bool IsCharacterDevice {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFCHR);}
		}

		public bool IsBlockDevice {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFBLK);}
		}

		public bool IsRegularFile {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFREG);}
		}

		public bool IsFifo {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFIFO);}
		}

		public bool IsSymbolicLink {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFLNK);}
		}

		public bool IsSocket {
			get {AssertValid (); return IsFileType (stat.st_mode, Native.FilePermissions.S_IFSOCK);}
		}

		public bool IsSetUser {
			get {AssertValid (); return IsSet (stat.st_mode, Native.FilePermissions.S_ISUID);}
		}

		public bool IsSetGroup {
			get {AssertValid (); return IsSet (stat.st_mode, Native.FilePermissions.S_ISGID);}
		}

		public bool IsSticky {
			get {AssertValid (); return IsSet (stat.st_mode, Native.FilePermissions.S_ISVTX);}
		}

		internal static bool IsFileType (Native.FilePermissions mode, Native.FilePermissions type)
		{
			return (mode & Native.FilePermissions.S_IFMT) == type;
		}

		internal static bool IsSet (Native.FilePermissions mode, Native.FilePermissions type)
		{
			return (mode & type) == type;
		}

		[CLSCompliant (false)]
		public bool CanAccess (Native.AccessModes mode)
		{
			int r = Native.Syscall.access (FullPath, mode);
			return r == 0;
		}

		public UnixFileSystemInfo CreateLink (string path)
		{
			int r = Native.Syscall.link (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return GetFileSystemEntry (path);
		}

		public UnixSymbolicLinkInfo CreateSymbolicLink (string path)
		{
			int r = Native.Syscall.symlink (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixSymbolicLinkInfo (path);
		}

		public abstract void Delete ();

		[CLSCompliant (false)]
		public long GetConfigurationValue (Native.PathconfName name)
		{
			long r = Native.Syscall.pathconf (FullPath, name);
			if (r == -1 && Native.Stdlib.GetLastError() != (Native.Errno) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public void Refresh ()
		{
			Refresh (true);
		}

		internal void Refresh (bool force)
		{
			if (valid && !force)
				return;
			valid = GetFileStatus (FullPath, out this.stat);
		}

		protected virtual bool GetFileStatus (string path, out Native.Stat stat)
		{
			return Native.Syscall.stat (path, out stat) == 0;
		}

		public void SetLength (long length)
		{
			int r;
			do {
				r = Native.Syscall.truncate (FullPath, length);
			}	while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public virtual void SetOwner (long owner, long group)
		{
			uint _owner = Convert.ToUInt32 (owner);
			uint _group = Convert.ToUInt32 (group);
			int r = Native.Syscall.chown (FullPath, _owner, _group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetOwner (string owner)
		{
			Native.Passwd pw = Native.Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner ((long) uid, (long) gid);
		}

		public void SetOwner (string owner, string group)
		{
			long uid = -1;
			if (owner != null)
				uid = new UnixUserInfo (owner).UserId;
			long gid = -1;
			if (group != null)
				gid = new UnixGroupInfo (group).GroupId;

			SetOwner (uid, gid);
		}

		public void SetOwner (UnixUserInfo owner)
		{
			long uid, gid;
			uid = gid = -1;
			if (owner != null) {
				uid = owner.UserId;
				gid = owner.GroupId;
			}
			SetOwner (uid, gid);
		}

		public void SetOwner (UnixUserInfo owner, UnixGroupInfo group)
		{
			long uid, gid;
			uid = gid = -1;
			if (owner != null)
				uid = owner.UserId;
			if (group != null)
				gid = owner.GroupId;
			SetOwner (uid, gid);
		}

		public override string ToString ()
		{
			return FullPath;
		}

		public Native.Stat ToStat ()
		{
			AssertValid ();
			return stat;
		}

		public static UnixFileSystemInfo GetFileSystemEntry (string path)
		{
			UnixFileSystemInfo info;
			if (TryGetFileSystemEntry (path, out info))
				return info;

			UnixMarshal.ThrowExceptionForLastError ();

			// Throw DirectoryNotFoundException because lstat(2) probably failed
			// because of ENOTDIR (e.g. "/path/to/file/wtf"), so
			// DirectoryNotFoundException is what would have been thrown anyway.
			throw new DirectoryNotFoundException ("UnixMarshal.ThrowExceptionForLastError didn't throw?!");
		}

		public static bool TryGetFileSystemEntry (string path, out UnixFileSystemInfo entry)
		{
			Native.Stat stat;
			int r = Native.Syscall.lstat (path, out stat);
			if (r == -1) {
				if (Native.Stdlib.GetLastError() == Native.Errno.ENOENT) {
					entry = new UnixFileInfo (path);
					return true;
				}
				entry = null;
				return false;
			}

			if (IsFileType (stat.st_mode, Native.FilePermissions.S_IFDIR))
				entry = new UnixDirectoryInfo (path, stat);
			else if (IsFileType (stat.st_mode, Native.FilePermissions.S_IFLNK))
				entry = new UnixSymbolicLinkInfo (path, stat);
			else
				entry = new UnixFileInfo (path, stat);

			return true;
		}
	}
}

// vim: noexpandtab
