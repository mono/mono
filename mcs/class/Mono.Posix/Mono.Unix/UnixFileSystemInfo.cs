//
// Mono.Unix/UnixFileSystemInfo.cs
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

	public abstract class UnixFileSystemInfo
	{
		private Stat stat;
		private string path;
		private bool valid = false;

		protected UnixFileSystemInfo (string path)
		{
			this.path = path;
			Refresh (true);
		}

		internal UnixFileSystemInfo (String path, Stat stat)
		{
			this.path = path;
			this.stat = stat;
			this.valid = true;
		}

		protected string Path {
			get {return path;}
			set {path = value;}
		}

		private void AssertValid ()
		{
			Refresh (false);
			if (!valid)
				throw new InvalidOperationException ("Path doesn't exist!");
		}

		public bool Exists {
			get {
				int r = Syscall.access (path, AccessMode.F_OK);
				if (r == 0)
					return true;
				return false;
			}
		}

		public ulong Device {
			get {AssertValid (); return stat.st_dev;}
		}

		public ulong Inode {
			get {AssertValid (); return stat.st_ino;}
		}

		public FilePermissions Mode {
			get {AssertValid (); return stat.st_mode;}
		}

		public FilePermissions Permissions {
			get {AssertValid (); return stat.st_mode & ~FilePermissions.S_IFMT;}
		}

		public FilePermissions FileType {
			get {AssertValid (); return stat.st_mode & FilePermissions.S_IFMT;}
		}

		public ulong LinkCount {
			get {AssertValid (); return (ulong) stat.st_nlink;}
		}

		public uint OwnerUser {
			get {AssertValid (); return stat.st_uid;}
		}

		public uint OwnerGroup {
			get {AssertValid (); return stat.st_gid;}
		}

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

		public bool IsFIFO {
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

		public bool CanAccess (AccessMode mode)
		{
			int r = Syscall.access (path, mode);
			return r == 0;
		}

		public abstract void Delete ();

		public long GetConfigurationValue (PathConf name)
		{
			Syscall.SetLastError ((Error) 0);
			long r = Syscall.pathconf (Path, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		// TODO: Should ReadLink be in UnixSymbolicLinkInfo?
		public string ReadLink ()
		{
			string r = TryReadLink ();
			if (r == null)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public string TryReadLink ()
		{
			// Who came up with readlink(2)?  There doesn't seem to be a way to
			// properly handle it.
			StringBuilder sb = new StringBuilder (512);
			int r = Syscall.readlink (path, sb);
			if (r == -1)
				return null;
			return sb.ToString().Substring (0, r);
		}

		public new void Refresh ()
		{
			Refresh (true);
		}

		internal void Refresh (bool force)
		{
			if (valid && !force)
				return;
			int r = Syscall.stat (path, out this.stat);
			valid = r == 0;
		}

		public void SetLength (long length)
		{
			int r;
			do {
				r = Syscall.truncate (path, length);
			}	while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetPermissions (FilePermissions perms)
		{
			int r = Syscall.chmod (path, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public virtual void SetOwner (uint owner, uint group)
		{
			int r = Syscall.chown (path, owner, group);
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
			return path;
		}

		internal static UnixFileSystemInfo Create (string path)
		{
			Stat stat = UnixFile.GetFileStatus (path);
			if (IsType (stat.st_mode, FilePermissions.S_IFDIR))
				return new UnixDirectoryInfo (path, stat);
			else if (IsType (stat.st_mode, FilePermissions.S_IFLNK))
				return new UnixSymbolicLinkInfo (path, stat);
			return new UnixFileInfo (path, stat);
		}
	}
}

// vim: noexpandtab
