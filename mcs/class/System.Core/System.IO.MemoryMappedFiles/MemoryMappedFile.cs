//
// MemoryMappedFile.cs
//
// Authors:
//	Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009, Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;


#if !MOBILE
using Mono.Unix.Native;
using Mono.Unix;
#else
using System.Runtime.CompilerServices;
#endif

namespace System.IO.MemoryMappedFiles
{
#if !MOBILE
	internal static class MemoryMapImpl {
		//
		// Turns the FileMode into the first half of open(2) flags
		//
		static OpenFlags ToUnixMode (FileMode mode)
		{
			switch (mode){
			case FileMode.CreateNew:
				return OpenFlags.O_CREAT | OpenFlags.O_EXCL;
				
			case FileMode.Create:
				return OpenFlags.O_CREAT | OpenFlags.O_TRUNC;
				
			case FileMode.OpenOrCreate:
				return OpenFlags.O_CREAT;
				
			case FileMode.Truncate:
				return OpenFlags.O_TRUNC;
				
			case FileMode.Append:
				return OpenFlags.O_APPEND;
			default:
			case FileMode.Open:
				return 0;
			}
		}

		//
		// Turns the MemoryMappedFileAccess into the second half of open(2) flags
		//
		static OpenFlags ToUnixMode (MemoryMappedFileAccess access)
		{
			switch (access){
			case MemoryMappedFileAccess.CopyOnWrite:
			case MemoryMappedFileAccess.ReadWriteExecute:
			case MemoryMappedFileAccess.ReadWrite:
				return OpenFlags.O_RDWR;
				
			case MemoryMappedFileAccess.Write:
				return OpenFlags.O_WRONLY;

			case MemoryMappedFileAccess.ReadExecute:
			case MemoryMappedFileAccess.Read:
			default:
				return OpenFlags.O_RDONLY;
			}
		}

		static MmapProts ToUnixProts (MemoryMappedFileAccess access)
		{
			switch (access){
			case MemoryMappedFileAccess.ReadWrite:
				return MmapProts.PROT_WRITE | MmapProts.PROT_READ;
				
			case MemoryMappedFileAccess.Write:
				return MmapProts.PROT_WRITE;
				
			case MemoryMappedFileAccess.CopyOnWrite:
				return MmapProts.PROT_WRITE | MmapProts.PROT_READ;
				
			case MemoryMappedFileAccess.ReadExecute:
				return MmapProts.PROT_EXEC;
				
			case MemoryMappedFileAccess.ReadWriteExecute:
				return MmapProts.PROT_WRITE | MmapProts.PROT_READ | MmapProts.PROT_EXEC;
				
			case MemoryMappedFileAccess.Read:
			default:
				return MmapProts.PROT_READ;
			}
		}

		internal static int Open (string path, FileMode mode, long capacity, MemoryMappedFileAccess access)
		{
			if (MonoUtil.IsUnix){
				Stat buf;
				if (Syscall.stat (path, out buf) == -1)
					UnixMarshal.ThrowExceptionForLastError ();

				if ((capacity == 0 && buf.st_size == 0) || (capacity > buf.st_size))
					throw new ArgumentException ("capacity");

				int fd = Syscall.open (path, ToUnixMode (mode) | ToUnixMode (access), FilePermissions.DEFFILEMODE);

				if (fd == -1)
					UnixMarshal.ThrowExceptionForLastError ();
				return fd;
			} else
				throw new NotImplementedException ();
		}
		
		internal static void CloseFD (int fd) {
			Syscall.close (fd);
		}

		internal static void Flush (int fd) {
			if (MonoUtil.IsUnix)
				Syscall.fsync (fd);
			else
				throw new NotImplementedException ("Not implemented on Windows");
			
		}

		static int pagesize;

		internal static unsafe void Map (int file_handle, long offset, ref long size, MemoryMappedFileAccess access, out IntPtr map_addr, out int offset_diff)
		{
			if (!MonoUtil.IsUnix)
				throw new NotImplementedException ("Not implemented on windows.");

			if (pagesize == 0)
				pagesize = Syscall.getpagesize ();

			Stat buf;
			Syscall.fstat (file_handle, out buf);
			long fsize = buf.st_size;

			if (size == 0 || size > fsize)
				size = fsize;
			
			// Align offset
			long real_offset = offset & ~(pagesize - 1);

			offset_diff = (int)(offset - real_offset);

			// FIXME: Need to determine the unix fd for the file, Handle is only
			// equal to it by accident
			//
			// The new API no longer uses FileStream everywhere, but exposes instead
			// the filename (with one exception), we could move this API to use
			// file descriptors instead of the FileStream plus its Handle.
			//
			map_addr = Syscall.mmap (IntPtr.Zero, (ulong) size,
						 ToUnixProts (access),
						 access == MemoryMappedFileAccess.CopyOnWrite ? MmapFlags.MAP_PRIVATE : MmapFlags.MAP_SHARED,
						 file_handle, real_offset);

			if (map_addr == (IntPtr)(-1))
				throw new IOException ("mmap failed for fd#" + file_handle + "(" + offset + ", " + size + ")");
		}

		internal static bool Unmap (IntPtr map_addr, ulong map_size)
		{
			if (!MonoUtil.IsUnix)
				return false;
			return Syscall.munmap (map_addr, map_size) == 0;
		}

		static void ConfigureUnixFD (IntPtr handle, HandleInheritability h)
		{
			// TODO: Mono.Posix is lacking O_CLOEXEC definitions for fcntl.
		}


		[DllImport("kernel32", SetLastError = true)]
		static extern bool SetHandleInformation (IntPtr hObject, int dwMask, int dwFlags);
		static void ConfigureWindowsFD (IntPtr handle, HandleInheritability h)
		{
			SetHandleInformation (handle, 1 /* FLAG_INHERIT */, h == HandleInheritability.None ? 0 : 1);
		}
		
		internal static void ConfigureFD (IntPtr handle, HandleInheritability inheritability)
		{
			if (MonoUtil.IsUnix)
				ConfigureUnixFD (handle, inheritability);
			else
				ConfigureWindowsFD (handle, inheritability);
		}

	}
#else
	internal static class MemoryMapImpl {
		[DllImport ("libc")]
		static extern int fsync (int fd);

		[DllImport ("libc")]
		static extern int close (int fd);

		[DllImport ("libc")]
		static extern int fcntl (int fd, int cmd, int arg0);

		//XXX check if android off_t is 64bits or not. on iOS / darwin it is.
		[DllImport ("libc")]
		static extern IntPtr mmap (IntPtr addr, IntPtr len, int prot, int flags, int fd, long offset);

		[DllImport ("libc")]
		static extern int munmap (IntPtr addr, IntPtr size);

		[DllImport ("libc", SetLastError=true)]
		static extern int open (string path, int flags, int access);

		[DllImport ("libc")]
		static extern int getpagesize ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern long mono_filesize_from_path (string str);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern long mono_filesize_from_fd (int fd);

		//Values valid on iOS/OSX and android ndk r6
		const int F_GETFD = 1;
		const int F_SETFD = 2;
		const int FD_CLOEXEC = 1;
		const int DEFFILEMODE = 0x666;

		const int O_RDONLY = 0x0;
		const int O_WRONLY = 0x1;
		const int O_RDWR   = 0x2;

		const int PROT_READ  = 0x1;
		const int PROT_WRITE = 0x2;
		const int PROT_EXEC  = 0x4;

		const int MAP_PRIVATE = 0x2;
		const int MAP_SHARED  = 0x1;

		const int EINVAL = 22;

#if MONODROID
		const int O_CREAT = 0x040;
		const int O_TRUNC = 0x080;
		const int O_EXCL  = 0x200;

		const int ENAMETOOLONG = 63;
#else
		/* MONOTOUCH */
		const int O_CREAT = 0x0200;
		const int O_TRUNC = 0x0400;
		const int O_EXCL  = 0x0800;

		const int ENAMETOOLONG = 36;
#endif

		static int ToUnixMode (FileMode mode)
		{
			switch (mode) {
			case FileMode.CreateNew:
				return O_CREAT | O_EXCL;
				
			case FileMode.Create:
				return O_CREAT | O_TRUNC;
				
			case FileMode.OpenOrCreate:
				return O_CREAT;
				
			case FileMode.Truncate:
				return O_TRUNC;
			default:
			case FileMode.Open:
				return 0;
			}
		}

		//
		// Turns the MemoryMappedFileAccess into the second half of open(2) flags
		//
		static int ToUnixMode (MemoryMappedFileAccess access)
		{
			switch (access) {
			case MemoryMappedFileAccess.CopyOnWrite:
			case MemoryMappedFileAccess.ReadWriteExecute:
			case MemoryMappedFileAccess.ReadWrite:
				return O_RDWR;
				
			case MemoryMappedFileAccess.Write:
				return O_WRONLY;

			case MemoryMappedFileAccess.ReadExecute:
			case MemoryMappedFileAccess.Read:
			default:
				return O_RDONLY;
			}
		}

		static int ToUnixProts (MemoryMappedFileAccess access)
		{
			switch (access){
			case MemoryMappedFileAccess.ReadWrite:
				return PROT_WRITE | PROT_READ;
				
			case MemoryMappedFileAccess.Write:
				return PROT_WRITE;
				
			case MemoryMappedFileAccess.CopyOnWrite:
				return PROT_WRITE | PROT_READ;
				
			case MemoryMappedFileAccess.ReadExecute:
				return PROT_EXEC;
				
			case MemoryMappedFileAccess.ReadWriteExecute:
				return PROT_WRITE | PROT_READ | PROT_EXEC;
				
			case MemoryMappedFileAccess.Read:
			default:
				return PROT_READ;
			}
		}

		static void ThrowErrorFromErrno (int errno) 
		{
			switch (errno) {
			case EINVAL:		throw new ArgumentException ();
			case ENAMETOOLONG:	throw new PathTooLongException ();
			default: throw new IOException ("Failed with errno " + errno);
			}
		}

		internal static int Open (string path, FileMode mode, long capacity, MemoryMappedFileAccess access)
		{
			long file_size = mono_filesize_from_path (path);
			if (file_size < 0)
				throw new FileNotFoundException (path);

			if ((capacity == 0 && file_size == 0) || (capacity > file_size))
				throw new ArgumentException ("capacity");

			int fd = open (path, ToUnixMode (mode) | ToUnixMode (access), DEFFILEMODE);

			if (fd == -1)
				ThrowErrorFromErrno (Marshal.GetLastWin32Error ());
			return fd;
		}

		internal static void CloseFD (int fd)
		{
			close (fd);
		}

		internal static void Flush (int fd)
		{
			fsync (fd);
		}

		internal static bool Unmap (IntPtr map_addr, ulong map_size)
		{
			return munmap (map_addr, (IntPtr)map_size) == 0;
		}

		static int pagesize;

		internal static unsafe void Map (int file_handle, long offset, ref long size, MemoryMappedFileAccess access, out IntPtr map_addr, out int offset_diff)
		{
			if (pagesize == 0)
				pagesize = getpagesize ();

			long fsize = mono_filesize_from_fd (file_handle);
			if (fsize < 0)
				throw new FileNotFoundException ();

			if (size == 0 || size > fsize)
				size = fsize;
			
			// Align offset
			long real_offset = offset & ~(pagesize - 1);

			offset_diff = (int)(offset - real_offset);

			map_addr = mmap (IntPtr.Zero, (IntPtr) size,
						 ToUnixProts (access),
						 access == MemoryMappedFileAccess.CopyOnWrite ? MAP_PRIVATE : MAP_SHARED,
						 file_handle, real_offset);

			if (map_addr == (IntPtr)(-1))
				throw new IOException ("mmap failed for fd#" + file_handle + "(" + offset + ", " + size + ")");
		}

		internal static void ConfigureFD (IntPtr handle, HandleInheritability inheritability)
		{
			int fd = (int)handle;
			int flags = fcntl (fd, F_GETFD, 0);
			if (inheritability == HandleInheritability.None)
				flags &= ~FD_CLOEXEC;
			else
				flags |= FD_CLOEXEC;
			fcntl (fd, F_SETFD, flags);
		}

	}
#endif

	public class MemoryMappedFile : IDisposable {
		MemoryMappedFileAccess fileAccess;
		string name;
		long fileCapacity;

		//
		// We allow the use of either the FileStream/keepOpen combo
		// or a Unix file descriptor.  This way we avoid the dependency on
		// Mono's io-layer having the Unix file descriptors mapped to
		// the same io-layer handle
		//
		FileStream stream;
		bool keepOpen;
		int unix_fd;

		public static MemoryMappedFile CreateFromFile (string path)
		{
			return CreateFromFile (path, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode)
		{
			return CreateFromFile (path, mode, null, 0, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName)
		{
			return CreateFromFile (path, mode, mapName, 0, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName, long capacity)
		{
			return CreateFromFile (path, mode, mapName, capacity, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName, long capacity, MemoryMappedFileAccess access)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if (mode == FileMode.Append)
				throw new ArgumentException ("mode");			

			int fd = MemoryMapImpl.Open (path, mode, capacity, access);
			
			return new MemoryMappedFile () {
				unix_fd = fd,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity
			};
		}

#if MOBILE
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access,
							       HandleInheritability inheritability,
							       bool leaveOpen)
#else
		[MonoLimitation ("memoryMappedFileSecurity is currently ignored")]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access,
							       MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability,
							       bool leaveOpen)
#endif
		{
			if (fileStream == null)
				throw new ArgumentNullException ("fileStream");
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if ((capacity == 0 && fileStream.Length == 0) || (capacity > fileStream.Length))
				throw new ArgumentException ("capacity");

			MemoryMapImpl.ConfigureFD (fileStream.Handle, inheritability);
				
			return new MemoryMappedFile () {
				stream = fileStream,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity,
				keepOpen = leaveOpen
			};
		}

		[MonoLimitation ("CreateNew requires that mapName be a file name on Unix")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity)
		{
#if MOBILE
			return CreateNew (mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, 0);
#else
			return CreateNew (mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, null, 0);
#endif
		}

		[MonoLimitation ("CreateNew requires that mapName be a file name on Unix")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access) 
		{
#if MOBILE
			return CreateNew (mapName, capacity, access, MemoryMappedFileOptions.DelayAllocatePages, 0);
#else
			return CreateNew (mapName, capacity, access, MemoryMappedFileOptions.DelayAllocatePages, null, 0);
#endif
		}

#if MOBILE
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access,
							  MemoryMappedFileOptions options, 
							  HandleInheritability handleInheritability)
#else
		[MonoLimitation ("CreateNew requires that mapName be a file name on Unix; options and memoryMappedFileSecurity are ignored")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access,
							  MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity,
							  HandleInheritability inheritability)
#endif
		{
			return CreateFromFile (mapName, FileMode.CreateNew, mapName, capacity, access);
		}

		[MonoLimitation ("CreateOrOpen requires that mapName be a file name on Unix")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity)
		{
			return CreateOrOpen (mapName, capacity, MemoryMappedFileAccess.ReadWrite);
		}

		[MonoLimitation ("CreateOrOpen requires that mapName be a file name on Unix")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access)
		{
			return CreateFromFile (mapName, FileMode.OpenOrCreate, mapName, capacity, access); 
		}

		[MonoTODO]
#if MOBILE
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, HandleInheritability inheritability)
#else
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
#endif
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemoryMappedFile OpenExisting (string mapName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemoryMappedFile OpenExisting (string mapName, MemoryMappedFileRights desiredAccessRights)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemoryMappedFile OpenExisting (string mapName, MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability)
		{
			throw new NotImplementedException ();
		}

		public MemoryMappedViewStream CreateViewStream ()
		{
			return CreateViewStream (0, 0);
		}

		public MemoryMappedViewStream CreateViewStream (long offset, long size)
		{
			return CreateViewStream (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		public MemoryMappedViewStream CreateViewStream (long offset, long size, MemoryMappedFileAccess access)
		{
			return new MemoryMappedViewStream (stream != null ? (int)stream.Handle : unix_fd, offset, size, access);
		}

		public MemoryMappedViewAccessor CreateViewAccessor ()
		{
			return CreateViewAccessor (0, 0);
		}

		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size)
		{
			return CreateViewAccessor (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size, MemoryMappedFileAccess access)
		{
			int file_handle = stream != null ? (int) stream.Handle : unix_fd;
			
			return new MemoryMappedViewAccessor (file_handle, offset, size, access);
		}

		MemoryMappedFile ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				if (stream != null){
					if (keepOpen == false)
						stream.Close ();
					unix_fd = -1;
					stream = null;
				}
				if (unix_fd != -1) {
					MemoryMapImpl.CloseFD (unix_fd);
					unix_fd = -1;
				}
			}
		}

#if !MOBILE
		[MonoTODO]
		public MemoryMappedFileSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetAccessControl (MemoryMappedFileSecurity memoryMappedFileSecurity)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
