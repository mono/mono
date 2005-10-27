//
// Mono.Unix/UnixStream.cs
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
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixStream : Stream, IDisposable
	{
		public const int InvalidFileDescriptor = -1;
		public const int StandardInputFileDescriptor = 0;
		public const int StandardOutputFileDescriptor = 1;
		public const int StandardErrorFileDescriptor = 2;

		public UnixStream (int fileDescriptor)
			: this (fileDescriptor, true) {}

		public UnixStream (int fileDescriptor, bool ownsHandle)
		{
			if (InvalidFileDescriptor == fileDescriptor)
				throw new ArgumentException (Locale.GetText ("Invalid file descriptor"), "fileDescriptor");
			
			this.fileDescriptor = fileDescriptor;
			this.owner = ownsHandle;
			
			long offset = Syscall.lseek (fileDescriptor, 0, SeekFlags.SEEK_CUR);
			if (offset != -1)
				canSeek = true;
			long read = Syscall.read (fileDescriptor, IntPtr.Zero, 0);
			if (read != -1)
				canRead = true;
			long write = Syscall.write (fileDescriptor, IntPtr.Zero, 0);
			if (write != -1)
				canWrite = true;  
		}

		private void AssertNotDisposed ()
		{
			if (fileDescriptor == InvalidFileDescriptor)
				throw new ObjectDisposedException ("Invalid File Descriptor");
		}

		public int Handle {
			get {return fileDescriptor;}
		}

		public override bool CanRead {
			get {return canRead;}
		}

		public override bool CanSeek {
			get {return canSeek;}
		}

		public override bool CanWrite {
			get {return canWrite;}
		}

		public override long Length {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("File descriptor doesn't support seeking");
				RefreshStat ();
				return stat.st_size;
			}
		}

		public override long Position {
			get {
				AssertNotDisposed ();
				if (!CanSeek)
					throw new NotSupportedException ("The stream does not support seeking");
				long pos = Syscall.lseek (fileDescriptor, 0, SeekFlags.SEEK_CUR);
				if (pos == -1)
					UnixMarshal.ThrowExceptionForLastError ();
				return (long) pos;
			}
			set {
				Seek (value, SeekOrigin.Begin);
			}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use Protection")]
		public FilePermissions Permissions {
			get {
				RefreshStat ();
				return (FilePermissions) stat.st_mode;
			}
			set {
				int r = Syscall.fchmod (fileDescriptor, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		[CLSCompliant (false)]
		public Native.FilePermissions Protection {
			get {
				RefreshStat ();
				return stat.st_mode;
			}
			set {
				// we can't change file type with fchmod, so clear out that portion
				value &= ~Native.FilePermissions.S_IFMT;
				int r = Native.Syscall.fchmod (fileDescriptor, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public FileTypes FileType {
			get {
				int type = (int) Protection;
				return (FileTypes) (type & (int) FileTypes.AllTypes);
			}
			// no set as fchmod(2) won't accept changing the file type.
		}

		public FileAccessPermissions FileAccessPermissions {
			get {
				int perms = (int) Protection;
				return (FileAccessPermissions) (perms & (int) FileAccessPermissions.AllPermissions);
			}
			set {
				int perms = (int) Protection;
				perms &= (int) ~FileAccessPermissions.AllPermissions;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public FileSpecialAttributes FileSpecialAttributes {
			get {
				int attrs = (int) Protection;
				return (FileSpecialAttributes) (attrs & (int) FileSpecialAttributes.AllAttributes);
			}
			set {
				int perms = (int) Protection;
				perms &= (int) ~FileSpecialAttributes.AllAttributes;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public UnixUserInfo OwnerUser {
			get {RefreshStat (); return new UnixUserInfo (stat.st_uid);}
		}
                                                                                                
		public long OwnerUserId {
			get {RefreshStat (); return stat.st_uid;}
		}
                                                                                                
		public UnixGroupInfo OwnerGroup {
			get {RefreshStat (); return new UnixGroupInfo (stat.st_gid);}
		}
                                                                                                
		public long OwnerGroupId {
			get {RefreshStat (); return stat.st_gid;}
		}

		private void RefreshStat ()
		{
			int r = Native.Syscall.fstat (fileDescriptor, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void AdviseFileAccessPattern (FileAccessPattern pattern, long offset, long len)
		{
			UnixFile.AdviseFileAccessPattern (fileDescriptor, pattern, offset, len);
		}

		public void AdviseFileAccessPattern (FileAccessPattern pattern)
		{
			AdviseFileAccessPattern (pattern, 0, 0);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Normal, offset, len)")]
		public void AdviseNormalAccess (long offset, long len)
		{
			UnixFile.AdviseNormalAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Normal)")]
		public void AdviseNormalAccess ()
		{
			UnixFile.AdviseNormalAccess (fileDescriptor);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Sequential, offset, len)")]
		public void AdviseSequentialAccess (long offset, long len)
		{
			UnixFile.AdviseSequentialAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Sequential)")]
		public void AdviseSequentialAccess ()
		{
			UnixFile.AdviseSequentialAccess (fileDescriptor);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Random, offset, len)")]
		public void AdviseRandomAccess (long offset, long len)
		{
			UnixFile.AdviseRandomAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.Random)")]
		public void AdviseRandomAccess ()
		{
			UnixFile.AdviseRandomAccess (fileDescriptor);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.UseSoon, offset, len)")]
		public void AdviseNeedAccess (long offset, long len)
		{
			UnixFile.AdviseNeedAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.UseSoon)")]
		public void AdviseNeedAccess ()
		{
			UnixFile.AdviseNeedAccess (fileDescriptor);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.WillNotUse, offset, len)")]
		public void AdviseNoAccess (long offset, long len)
		{
			UnixFile.AdviseNoAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.WillNotUse)")]
		public void AdviseNoAccess ()
		{
			UnixFile.AdviseNoAccess (fileDescriptor);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.UseOnce, offset, len)")]
		public void AdviseOnceAccess (long offset, long len)
		{
			UnixFile.AdviseOnceAccess (fileDescriptor, offset, len);
		}

		[Obsolete ("Use AdviseFileAccessPattern (FileAccessPattern.UseOnce)")]
		public void AdviseOnceAccess ()
		{
			UnixFile.AdviseOnceAccess (fileDescriptor);
		}

		public override void Flush ()
		{
			int r = Native.Syscall.fsync (fileDescriptor);

			if (r == -1) {
				Native.Errno e = Native.Stdlib.GetLastError ();

				// From the man page:
				//  EROFS, EINVAL:
				//    fd is bound to a special file which does not support
				//    synchronization.
				// Sockets are such a file, and since Close() calls Flush(), and we
				// want to support manually opened sockets, we shouldn't generate an
				// exception for these errors.
				if (e == Native.Errno.EROFS || e == Native.Errno.EINVAL) {
					return;
				}

				UnixMarshal.ThrowExceptionForError (e);
			}
		}

		public override unsafe int Read ([In, Out] byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
				 
			long r = 0;
			fixed (byte* buf = &buffer[offset]) {
				do {
					r = Syscall.read (fileDescriptor, buf, (ulong) count);
				} while (UnixMarshal.ShouldRetrySyscall ((int) r));
			}
			if (r == -1)
				UnixMarshal.ThrowExceptionForLastError ();
			return (int) r;
		}

		private void AssertValidBuffer (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			if (offset > buffer.Length)
				throw new ArgumentException ("destination offset is beyond array size");
			if (offset > (buffer.Length - count))
				throw new ArgumentException ("would overrun buffer");
		}

		public unsafe int ReadAtOffset ([In, Out] byte[] buffer, 
			int offset, int count, long fileOffset)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading");
				 
			long r = 0;
			fixed (byte* buf = &buffer[offset]) {
				do {
					r = Syscall.pread (fileDescriptor, buf, (ulong) count, fileOffset);
				} while (UnixMarshal.ShouldRetrySyscall ((int) r));
			}
			if (r == -1)
				UnixMarshal.ThrowExceptionForLastError ();
			return (int) r;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			AssertNotDisposed ();
			if (!CanSeek)
				throw new NotSupportedException ("The File Descriptor does not support seeking");
			if (offset > int.MaxValue)
				throw new ArgumentOutOfRangeException ("offset", "too large");
					
			SeekFlags sf = SeekFlags.SEEK_CUR;
			switch (origin) {
				case SeekOrigin.Begin:   sf = SeekFlags.SEEK_SET; break;
				case SeekOrigin.Current: sf = SeekFlags.SEEK_CUR; break;
				case SeekOrigin.End:     sf = SeekFlags.SEEK_END; break;
			}

			long pos = Syscall.lseek (fileDescriptor, offset, sf);
			if (pos == -1)
				UnixMarshal.ThrowExceptionForLastError ();
			return (long) pos;
		}

		public override void SetLength (long value)
		{
			AssertNotDisposed ();
			if (value < 0)
				throw new ArgumentOutOfRangeException ("value", "< 0");
			if (!CanSeek && !CanWrite)
				throw new NotSupportedException ("You can't truncating the current file descriptor");
			
			int r;
			do {
				r = Syscall.ftruncate (fileDescriptor, value);
			} while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public override unsafe void Write (byte[] buffer, int offset, int count)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanWrite)
				throw new NotSupportedException ("File Descriptor does not support writing");

			long r = 0;
			fixed (byte* buf = &buffer[offset]) {
				do {
					r = Syscall.write (fileDescriptor, buf, (ulong) count);
				} while (UnixMarshal.ShouldRetrySyscall ((int) r));
			}
			if (r == -1)
				UnixMarshal.ThrowExceptionForLastError ();
		}
		
		public unsafe void WriteAtOffset (byte[] buffer, 
			int offset, int count, long fileOffset)
		{
			AssertNotDisposed ();
			AssertValidBuffer (buffer, offset, count);
			if (!CanWrite)
				throw new NotSupportedException ("File Descriptor does not support writing");

			long r = 0;
			fixed (byte* buf = &buffer[offset]) {
				do {
					r = Syscall.pwrite (fileDescriptor, buf, (ulong) count, fileOffset);
				} while (UnixMarshal.ShouldRetrySyscall ((int) r));
			}
			if (r == -1)
				UnixMarshal.ThrowExceptionForLastError ();
		}

		public void SendTo (UnixStream output)
		{
			SendTo (output, (ulong) output.Length);
		}

		[CLSCompliant (false)]
		public void SendTo (UnixStream output, ulong count)
		{
			SendTo (output.Handle, count);
		}

		[CLSCompliant (false)]
		public void SendTo (int out_fd, ulong count)
		{
			if (!CanWrite)
				throw new NotSupportedException ("Unable to write to the current file descriptor");
			long offset = Position;
			long r = Syscall.sendfile (out_fd, fileDescriptor, ref offset, count);
			if (r == -1)
				UnixMarshal.ThrowExceptionForLastError ();
		}
		
		[CLSCompliant (false)]
		[Obsolete ("Use SetOwner (long, long)")]
		public void SetOwner (uint user, uint group)
		{
			AssertNotDisposed ();

			int r = Syscall.fchown (fileDescriptor, user, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetOwner (long user, long group)
		{
			AssertNotDisposed ();

			int r = Syscall.fchown (fileDescriptor, 
					Convert.ToUInt32 (user), Convert.ToUInt32 (group));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetOwner (string user, string group)
		{
			AssertNotDisposed ();

			uint uid = UnixUser.GetUserId (user);
			uint gid = UnixGroup.GetGroupId (group);
			SetOwner (uid, gid);
		}

		public void SetOwner (string user)
		{
			AssertNotDisposed ();

			Passwd pw = Syscall.getpwnam (user);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "user");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner (uid, gid);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetConfigurationValue (Mono.Unix.Native.PathconfName")]
		public long GetConfigurationValue (PathConf name)
		{
			AssertNotDisposed ();
			long r = Syscall.fpathconf (fileDescriptor, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		public long GetConfigurationValue (Native.PathconfName name)
		{
			AssertNotDisposed ();
			long r = Native.Syscall.fpathconf (fileDescriptor, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		~UnixStream ()
		{
			Close ();
		}

		public override void Close ()
		{
			if (fileDescriptor == InvalidFileDescriptor)
				return;
				
			Flush ();
			int r;
			do {
				r = Syscall.close (fileDescriptor);
			} while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			fileDescriptor = InvalidFileDescriptor;
			GC.SuppressFinalize (this);
		}
		
		void IDisposable.Dispose ()
		{
			AssertNotDisposed ();
			if (owner) {
				Close ();
			}
			GC.SuppressFinalize (this);
		}

		private bool canSeek = false;
		private bool canRead = false;
		private bool canWrite = false;
		private bool owner = true;
		private int fileDescriptor = InvalidFileDescriptor;
		private Native.Stat stat;
	}
}

// vim: noexpandtab
