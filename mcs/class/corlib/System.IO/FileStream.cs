//
// System.IO/FileStream.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Runtime.InteropServices;
using Unix;

// fixme: I do not know how to handle errno when calling PInvoke functions
// fixme: emit the correct exceptions everywhere

namespace System.IO
{

	class FileStream : Stream
	{
			
		private IntPtr fd;
		private FileAccess acc;
		private bool owner;
		
		private int getUnixFlags (FileMode mode, FileAccess access)
		{
			int flags = 0;

			switch (access) {
			case FileAccess.Read:
				flags = Wrapper.O_RDONLY;
				break;
			case FileAccess.Write:
				flags = Wrapper.O_WRONLY;
				break;
			case FileAccess.ReadWrite:
				flags = Wrapper.O_RDWR;
				break;
			}
			
			switch (mode) {
			case FileMode.Append:
				flags |= Wrapper.O_APPEND;
				break;
			case FileMode.Create:
				flags |= Wrapper.O_CREAT;
				break;
			case FileMode.CreateNew:
				flags |= Wrapper.O_CREAT |  Wrapper.O_EXCL;
				break;
			case FileMode.Open:
				break;
			case FileMode.OpenOrCreate:
				flags |= Wrapper.O_CREAT;				
				break;
			case FileMode.Truncate:
				flags |= Wrapper.O_TRUNC;
				break;
			}

			return flags;
		}
		
		public FileStream (IntPtr fd, FileAccess access)
			: this (fd, access, true, 0, false) {}

		public FileStream (IntPtr fd, FileAccess access, bool ownsHandle)
			: this (fd, access, ownsHandle, 0, false) {}
		
		public FileStream (IntPtr fd, FileAccess access, bool ownsHandle, int bufferSize)
			: this (fd, access, ownsHandle, bufferSize, false) {}
		
		public FileStream (IntPtr fd, FileAccess access, bool ownsHandle,
				   int bufferSize, bool isAsync)
		{
			fd = fd;
			acc = access;
			owner = ownsHandle;
		}
		
		public FileStream (string name, FileMode mode)
			: this (name, mode, FileAccess.ReadWrite, FileShare.ReadWrite, 0, false) {}

		public FileStream (string name, FileMode mode, FileAccess access)
			: this (name, mode, access, FileShare.ReadWrite, 0, false) {}

		public FileStream (string name, FileMode mode, FileAccess access, FileShare share)
			: this (name, mode, access, share, 0, false) {}
		
		public FileStream (string name, FileMode mode, FileAccess access,
				   FileShare share, int buferSize)
			: this (name, mode, access, share, 0, false) {}

		// fixme: implement all share, buffer, async
		public FileStream (string name, FileMode mode, FileAccess access, FileShare share,
				   int buferSize, bool useAsync)
		{
			int flags = getUnixFlags (mode, access);
			
			if ((int)(fd = Wrapper.open (name, flags, 0x1a4)) == -1)
				throw new IOException();

			acc = access;
			owner = true;
		}
		
		public override bool CanRead
		{
			get {
				switch (acc) {
				case FileAccess.Read:
				case FileAccess.ReadWrite:
					return true;
				case FileAccess.Write:
				default:
					return false;
				}
			}
		}

		public override bool CanSeek
		{
                        get {
				// fixme: not alway true
                                return true;
                        }
                }

                public override bool CanWrite
		{
                        get {
				switch (acc) {
				case FileAccess.Write:
				case FileAccess.ReadWrite:
					return true;
				default:
					return false;
				}
                        }
                }

		unsafe public override long Length
		{
			get {
				stat fs;

				Wrapper.fstat (fd, &fs);
				return fs.st_size;
			}
		}

		public override long Position
		{
			get {
				return Wrapper.seek (fd, 0,  Wrapper.SEEK_CUR);
			}
			set {
				Wrapper.seek (fd, value, Wrapper.SEEK_SET);
			}
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
			if (owner && Wrapper.close (fd) != 0)
				throw new IOException();
		}

		public unsafe override int Read (byte[] buffer,
					  int offset,
					  int count)
		{
			int res;

			fixed (void *p = &buffer [offset]) {
				res = Wrapper.read (fd, p, count);
			}
			
			return res;
		}

		public unsafe override int ReadByte ()
		{
			byte val;
			
			if (Wrapper.read (fd, &val, 1) != 1)
				throw new IOException();
			
			return val;
		}

		public override long Seek (long offset,
					   SeekOrigin origin)
		{
			int off = (int)offset;
			
			switch (origin) {
			case SeekOrigin.End:
				return Wrapper.seek (fd, Wrapper.SEEK_END, off);
			case SeekOrigin.Current:
				return Wrapper.seek (fd, Wrapper.SEEK_CUR, off);
			default:
				return Wrapper.seek (fd, Wrapper.SEEK_SET, off);
			}
		}

		public override void SetLength (long value)
		{
			int res;

			if ((res = Wrapper.ftruncate (fd, value)) == -1)
				throw new IOException();

			
		}

		public unsafe override void Write (byte[] buffer,
						   int offset,
						   int count)
		{
			int res;
			
			fixed (void *p = &buffer [offset]) {
				res = Wrapper.write (fd, p, count);
			}
			
			if (res != count)
				throw new IOException();
		}

		public unsafe override void WriteByte (byte value)
		{
			if (Wrapper.write (fd, &value, 1) != 1)
				throw new IOException();
		}

	}
}
