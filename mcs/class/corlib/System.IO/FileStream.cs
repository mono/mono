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
using System.Threading;

// fixme: I do not know how to handle errno when calling PInvoke functions
// fixme: emit the correct exceptions everywhere

namespace System.IO
{

	public class FileStream : Stream
	{
		private OpSys _os = Platform.OS;
		private IntPtr fdhandle;
		private FileAccess acc;
		private bool owner;
		
		public FileStream (IntPtr handle, FileAccess access)
			: this (handle, access, true, 0, false) {}

		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle)
			: this (handle, access, ownsHandle, 0, false) {}
		
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
			: this (handle, access, ownsHandle, bufferSize, false) {}
		
		public FileStream (IntPtr handle, FileAccess access, bool ownsHandle,
				   int bufferSize, bool isAsync)
		{
			fdhandle = handle;
			
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
			fdhandle = _os.OpenFile (name, mode, access, share);
			
			/* Implement error checking, with some sort of access
			   to the errno error reason
			if(fdhandle == error) {
				throw new IOException();
			}
			*/

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
				return _os.FileLength (fdhandle);
			}
		}

		public override long Position
		{
			get {
				return _os.SeekFile (fdhandle, 0,  SeekOrigin.Current);
			}
			set {
				_os.SeekFile (fdhandle, value, SeekOrigin.Begin);
			}
		}

		public override void Flush ()
		{
		}

		public override void Close ()
		{
			if (owner) {
				_os.CloseFile (fdhandle);
			}
		}

		public unsafe override int Read (byte[] buffer,
					  int offset,
					  int count)
		{
			return _os.ReadFile (fdhandle, buffer, offset, count);
		}

		public unsafe override int ReadByte ()
		{
			byte[] val = new byte[1];
			int res = Read (val, 0, 1);
			
			if (res == -1)
				throw new IOException();
			if (res == 0)
				return -1;

			return val[0];
		}

		public override long Seek (long offset,
					   SeekOrigin origin)
		{
			return _os.SeekFile (fdhandle, offset, origin);
		}

		public override void SetLength (long value)
		{
			_os.SetLengthFile (fdhandle, value);
		}

		public unsafe override void Write (byte[] buffer,
						   int offset,
						   int count)
		{
			int res = _os.WriteFile (fdhandle, buffer, offset, count);
			
			if (res != count)
				throw new IOException();
		}

		public unsafe override void WriteByte (byte value)
		{
			byte[] buf = new byte[1];

			buf[0] = value;

			Write (buf, 0, 1);
		}

		public virtual IntPtr Handle
		{
			get {
				return(fdhandle);
			}
		}
	}
}
