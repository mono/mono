//
// System.IO/FileStream.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.PAL;
using System.Runtime.InteropServices;

// fixme: I do not know how to handle errno when calling PInvoke functions
// fixme: emit the correct exceptions everywhere

namespace System.IO
{

	public class FileStream : Stream
	{
		private OpSys _os = Platform.OS;
		private IntPtr fd;
		private FileAccess acc;
		private bool owner;
		
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
			//acc = access;
			//owner = ownsHandle;
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
			if ((int)(fd = _os.OpenFile (name, mode, access, share)) == -1)
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
				return _os.FileLength (fd);
			}
		}

		public override long Position
		{
			get {
				return _os.SeekFile (fd, 0,  SeekOrigin.Current);
			}
			set {
				_os.SeekFile (fd, value, SeekOrigin.Begin);
			}
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
			if (owner) {
				_os.CloseFile (fd);
			}
		}

		public unsafe override int Read (byte[] buffer,
					  int offset,
					  int count)
		{
			return _os.ReadFile (fd, buffer, offset, count);
		}

		public unsafe override int ReadByte ()
		{
			byte[] val = new byte[1];
			
			if (Read (val, 0, 1) != 1)
				throw new IOException();
			
			return val[0];
		}

		public override long Seek (long offset,
					   SeekOrigin origin)
		{
			return _os.SeekFile (fd, offset, origin);
		}

		public override void SetLength (long value)
		{
			int res;

			if ((res = _os.SetLengthFile (fd, value)) == -1)
				throw new IOException();

			
		}

		public unsafe override void Write (byte[] buffer,
						   int offset,
						   int count)
		{
			int res = _os.WriteFile (fd, buffer, offset, count);
			
			if (res != count)
				throw new IOException();
		}

		public unsafe override void WriteByte (byte value)
		{
			byte[] buf = new byte[1];

			buf[0] = value;

			Write (buf, 0, 1);
		}

	}
}
