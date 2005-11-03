//
// Mono.Unix/UnixFileSystemInfo.cs
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

	public abstract class UnixFileSystemInfo
	{
		private Stat stat;
		private string fullPath;
		private string originalPath;
		private bool valid = false;

		protected UnixFileSystemInfo (string path)
		{
			UnixPath.CheckPath (path);
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			Refresh (true);
		}

		[Obsolete ("Use UnixFileSystemInfo (string, Mono.Unix.Native.Stat)")]
		internal UnixFileSystemInfo (String path, Stat stat)
		{
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			this.stat = stat;
			this.valid = true;
		}

		internal UnixFileSystemInfo (String path, Native.Stat stat)
		{
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			// this.stat = stat;
			this.valid = true;
		}

		protected string FullPath {
			get {return fullPath;}
			set {fullPath = value;}
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

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release.")]
		public ulong Device {
			get {AssertValid (); return stat.st_dev;}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release.")]
		public ulong Inode {
			get {AssertValid (); return stat.st_ino;}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use Protection.")]
		public FilePermissions Mode {
			get {AssertValid (); return stat.st_mode;}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use FileAccessPermissions.")]
		public FilePermissions Permissions {
			get {AssertValid (); return stat.st_mode & ~FilePermissions.S_IFMT;}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to FileTypes in the next release.")]
		public FilePermissions FileType {
			get {AssertValid (); return stat.st_mode & FilePermissions.S_IFMT;}
		}

		[CLSCompliant (false)]
		public Native.FilePermissions Protection {
			get {AssertValid (); return (Native.FilePermissions) stat.st_mode;}
			set {
				int r = Native.Syscall.chmod (FullPath, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

#if false
		public FileTypes FileType {
			get {
				AssertValid ();
				int type = (int) stat.st_mode;
				return (FileTypes) (type & (int) FileTypes.AllTypes);
			}
			// no set as chmod(2) won't accept changing the file type.
		}
#endif

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
				return (FileSpecialAttributes) (attrs & (int) FileSpecialAttributes.AllAttributes);
			}
			set {
				AssertValid ();
				int perms = (int) stat.st_mode;
				perms &= (int) ~FileSpecialAttributes.AllAttributes;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release.")]
		public ulong LinkCount {
			get {AssertValid (); return (ulong) stat.st_nlink;}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use OwnerUserId.  " +
				"The type of this property will change to UnixUserInfo in the next release.")]
		public uint OwnerUser {
			get {AssertValid (); return stat.st_uid;}
		}

		public long OwnerUserId {
			get {AssertValid (); return stat.st_uid;}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use OwnerGroupId.  " +
				"The type of this property will change to UnixGroupInfo in the next release.")]
		public uint OwnerGroup {
			get {AssertValid (); return stat.st_gid;}
		}

		public long OwnerGroupId {
			get {AssertValid (); return stat.st_gid;}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change to Int64 in the next release.")]
		public ulong DeviceType {
			get {AssertValid (); return stat.st_rdev;}
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
			get {AssertValid (); return UnixConvert.ToDateTime (stat.st_atime);}
		}

		public DateTime LastAccessTimeUtc {
			get {return LastAccessTime.ToUniversalTime ();}
		}

		public DateTime LastWriteTime {
			get {AssertValid (); return UnixConvert.ToDateTime (stat.st_mtime);}
		}

		public DateTime LastWriteTimeUtc {
			get {return LastWriteTime.ToUniversalTime ();}
		}

		public DateTime LastStatusChangeTime {
			get {AssertValid (); return UnixConvert.ToDateTime (stat.st_ctime);}
		}

		public DateTime LastStatusChangeTimeUtc {
			get {return LastStatusChangeTime.ToUniversalTime ();}
		}

		public bool IsDirectory {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFDIR);}
		}

		public bool IsCharacterDevice {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFCHR);}
		}

		public bool IsBlockDevice {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFBLK);}
		}

		public bool IsFile {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFREG);}
		}

		[Obsolete ("Use IsFifo")]
		[CLSCompliant (false)]
		public bool IsFIFO {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFIFO);}
		}

		public bool IsFifo {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFIFO);}
		}

		public bool IsSymbolicLink {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFLNK);}
		}

		public bool IsSocket {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_IFSOCK);}
		}

		public bool IsSetUser {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_ISUID);}
		}

		public bool IsSetGroup {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_ISGID);}
		}

		public bool IsSticky {
			get {AssertValid (); return IsType (stat.st_mode, FilePermissions.S_ISVTX);}
		}

		internal static bool IsType (FilePermissions mode, FilePermissions type)
		{
			return (mode & type) == type;
		}

		internal static bool IsType (Native.FilePermissions mode, Native.FilePermissions type)
		{
			return (mode & type) == type;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use CanAccess (Mono.Unix.Native.AccessModes)")]
		public bool CanAccess (AccessMode mode)
		{
			int r = Syscall.access (FullPath, mode);
			return r == 0;
		}

		[CLSCompliant (false)]
		public bool CanAccess (Native.AccessModes mode)
		{
			int r = Native.Syscall.access (FullPath, mode);
			return r == 0;
		}

		public UnixFileSystemInfo CreateLink (string path)
		{
			int r = Syscall.link (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return Create (path);
		}

		public UnixSymbolicLinkInfo CreateSymbolicLink (string path)
		{
			int r = Syscall.symlink (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixSymbolicLinkInfo (path);
		}

		public abstract void Delete ();

		[CLSCompliant (false)]
		[Obsolete ("Use GetConfigurationValue (Mono.Unix.Native.PathconfName)")]
		public long GetConfigurationValue (PathConf name)
		{
			long r = Syscall.pathconf (FullPath, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		public long GetConfigurationValue (Native.PathconfName name)
		{
			long r = Native.Syscall.pathconf (FullPath, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
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
			int r = GetFileStatus (FullPath, out this.stat);
			valid = r == 0;
		}

		[Obsolete ("Use GetFileStatus (string, Mono.Unix.Native.Stat)")]
		protected virtual int GetFileStatus (string path, out Stat stat)
		{
			return Syscall.stat (path, out stat);
		}

		protected virtual int GetFileStatus (string path, out Native.Stat stat)
		{
			return Native.Syscall.stat (path, out stat);
		}

		public void SetLength (long length)
		{
			int r;
			do {
				r = Syscall.truncate (FullPath, length);
			}	while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use Protection setter")]
		public void SetPermissions (FilePermissions perms)
		{
			int r = Syscall.chmod (FullPath, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use SetOwner (long, long)")]
		public virtual void SetOwner (uint owner, uint group)
		{
			int r = Syscall.chown (FullPath, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public virtual void SetOwner (long owner, long group)
		{
			uint _owner = Convert.ToUInt32 (owner);
			uint _group = Convert.ToUInt32 (group);
			int r = Syscall.chown (FullPath, _owner, _group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetOwner (string owner)
		{
			Passwd pw = Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner (uid, gid);
		}

		public void SetOwner (string owner, string group)
		{
			uint uid = UnixUser.GetUserId (owner);
			uint gid = UnixGroup.GetGroupId (group);

			SetOwner (uid, gid);
		}

		public override string ToString ()
		{
			return FullPath;
		}

		public Native.Stat ToStat ()
		{
			Native.Stat stat = new Native.Stat ();
			stat.st_dev     = this.stat.st_dev;
			stat.st_ino     = this.stat.st_ino;
			stat.st_mode    = (Native.FilePermissions) this.stat.st_mode;
			stat.st_nlink   = this.stat.st_nlink;
			stat.st_uid     = this.stat.st_uid;
			stat.st_gid     = this.stat.st_gid;
			stat.st_rdev    = this.stat.st_rdev;
			stat.st_size    = this.stat.st_size;
			stat.st_blksize = this.stat.st_blksize;
			stat.st_blocks  = this.stat.st_blocks;
			stat.st_atime   = this.stat.st_atime;
			stat.st_mtime   = this.stat.st_mtime;
			stat.st_ctime   = this.stat.st_ctime;
			return stat;
		}

		internal static UnixFileSystemInfo Create (string path)
		{
			Stat stat;
			int r = Syscall.lstat (path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);

			if (IsType (stat.st_mode, FilePermissions.S_IFDIR))
				return new UnixDirectoryInfo (path, stat);
			else if (IsType (stat.st_mode, FilePermissions.S_IFLNK))
				return new UnixSymbolicLinkInfo (path, stat);
			return new UnixFileInfo (path, stat);
		}
	}
}

// vim: noexpandtab
