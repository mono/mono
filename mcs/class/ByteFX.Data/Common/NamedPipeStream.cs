// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.Runtime.InteropServices;


namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for API.
	/// </summary>
	internal class NamedPipeStream : Stream
	{
		[DllImport("kernel32.dll", EntryPoint="CreateFile", SetLastError=true)]
		private static extern IntPtr CreateFile(String lpFileName, 
			UInt32 dwDesiredAccess, UInt32 dwShareMode,
			IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, 
			UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile);
		[DllImport("kernel32.dll", EntryPoint="PeekNamedPipe", SetLastError=true)]
		private static extern bool PeekNamedPipe( IntPtr handle,
			byte[] buffer, uint nBufferSize, ref uint bytesRead,
			ref uint bytesAvail, ref uint BytesLeftThisMessage);
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern bool ReadFile( IntPtr handle,
			byte[] buffer, uint toRead, ref uint read, IntPtr lpOverLapped);
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern bool WriteFile( IntPtr handle,
			byte[] buffer, uint count, ref uint written, IntPtr lpOverlapped );
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern bool CloseHandle( IntPtr handle );
		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern bool FlushFileBuffers( IntPtr handle );

		//Constants for dwDesiredAccess:
		private const UInt32 GENERIC_READ = 0x80000000;
		private const UInt32 GENERIC_WRITE = 0x40000000;

		//Constants for return value:
		private const Int32 INVALID_HANDLE_VALUE = -1;

		//Constants for dwFlagsAndAttributes:
		private const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
		private const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

		//Constants for dwCreationDisposition:
		private const UInt32 OPEN_EXISTING = 3;

		IntPtr		_handle;
		FileAccess	_mode;

		public NamedPipeStream(string host, FileAccess mode)
		{
			_handle = IntPtr.Zero;
			Open(host, mode);
		}

		public void Open( string host, FileAccess mode )
		{
			_mode = mode;
			uint pipemode = 0;

			if ((mode & FileAccess.Read) > 0)
				pipemode |= GENERIC_READ;
			if ((mode & FileAccess.Write) > 0)
				pipemode |= GENERIC_WRITE;
			_handle = CreateFile( host, pipemode,
				0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero );
		}

		public bool DataAvailable
		{
			get 
			{
				uint bytesRead=0, avail=0, thismsg=0;

				bool result = PeekNamedPipe( _handle, 
					null, 0, ref bytesRead, ref avail, ref thismsg );
				return (result == true && avail > 0);
			}
	}

		public override bool CanRead
		{
			get { return (_mode & FileAccess.Read) > 0; }
		}

		public override bool CanWrite
		{
			get { return (_mode & FileAccess.Write) > 0; }
		}

		public override bool CanSeek
		{
			get { throw new NotSupportedException("NamedPipeStream does not support seeking"); }
		}

		public override long Length
		{
			get { throw new NotSupportedException("NamedPipeStream does not support seeking"); }
		}

		public override long Position 
		{
			get { throw new NotSupportedException("NamedPipeStream does not support seeking"); }
			set { }
		}

		public override void Flush() 
		{
			if (_handle == IntPtr.Zero)
				throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");
			FlushFileBuffers(_handle);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null) 
				throw new ArgumentNullException("buffer", "The buffer to read into cannot be null");
			if (buffer.Length < (offset + count))
				throw new ArgumentException("Buffer is not large enough to hold requested data", "buffer");
			if (offset < 0) 
				throw new ArgumentOutOfRangeException("offset", offset, "Offset cannot be negative");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Count cannot be negative");
			if (! CanRead)
				throw new NotSupportedException("The stream does not support reading");
			if (_handle == IntPtr.Zero)
				throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");

			// first read the data into an internal buffer since ReadFile cannot read into a buf at
			// a specified offset
			uint read=0;
			byte[] buf = new Byte[count];
			ReadFile( _handle, buf, (uint)count, ref read, IntPtr.Zero );
			
			for (int x=0; x < read; x++) 
			{
				buffer[offset+x] = buf[x];
			}
			return (int)read;
		}

		public override void Close()
		{
			CloseHandle(_handle);
			_handle = IntPtr.Zero;
		}

		public override void SetLength(long length)
		{
			throw new NotSupportedException("NamedPipeStream doesn't support SetLength");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null) 
				throw new ArgumentNullException("buffer", "The buffer to write into cannot be null");
			if (buffer.Length < (offset + count))
				throw new ArgumentException("Buffer does not contain amount of requested data", "buffer");
			if (offset < 0) 
				throw new ArgumentOutOfRangeException("offset", offset, "Offset cannot be negative");
			if (count < 0)
				throw new ArgumentOutOfRangeException("count", count, "Count cannot be negative");
			if (! CanWrite)
				throw new NotSupportedException("The stream does not support writing");
			if (_handle == IntPtr.Zero)
				throw new ObjectDisposedException("NamedPipeStream", "The stream has already been closed");
			
			// copy data to internal buffer to allow writing from a specified offset
			byte[] buf = new Byte[count];
			for (int x=0; x < count; x++) 
			{
				buf[x] = buffer[offset+x];
			}
			uint written=0;
			bool result = WriteFile( _handle, buf, (uint)count, ref written, IntPtr.Zero );

			if (! result)
				throw new IOException("Writing to the stream failed");
			if (written < count)
				throw new IOException("Unable to write entire buffer to stream");
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotSupportedException("NamedPipeStream doesn't support seeking");
		}
	}
}


