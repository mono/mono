using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;

#if NET_2_0

namespace Mono.System.IO.Ports
{
	public class WinSerialStream : Stream, IDisposable
	{
		// Windows API Constants
		const uint GenericRead = 0x80000000;
		const uint GenericWrite = 0x40000000;
		const uint OpenExisting = 3;
		const uint FileFlagOverlapped = 0x40000000;
		const uint PurgeRxClear = 0x0004;
		const uint PurgeTxClear = 0x0008;

		int handle;
		bool disposed;

		[DllImport("kernel32", SetLastError=true)]
		static extern int CreateFile(string port_name, uint desired_access,
			uint share_mode, uint security_attrs, uint creation, uint flags,
			uint template);

		[DllImport("kernel32", SetLastError=true)]
		static extern bool SetupComm(int handle, int read_buffer_size, int write_buffer_size);

		[DllImport("kernel32", SetLastError = true)]
		static extern bool PurgeComm(int handle, uint flags);

		public unsafe WinSerialStream(string port_name, int read_buffer_size, int write_buffer_size)
		{
			handle = CreateFile(port_name, GenericRead | GenericWrite, 0, 0, OpenExisting,
					FileFlagOverlapped, 0);

			if (handle == -1)
				throw new Win32Exception ();

			// Clean buffers
			if (!PurgeComm(handle, PurgeRxClear | PurgeTxClear))
				throw new Win32Exception();

			// Set buffers size
			if (!SetupComm(handle, read_buffer_size, write_buffer_size))
				throw new Win32Exception();
		}

		public override bool CanRead
		{
			get {
				return true;
			}
		}

		public override bool CanSeek
		{
			get {
				return false;
			}
		}

		public override bool CanTimeout
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get {
				return true;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		[DllImport("kernel32", SetLastError=true)]
		static extern bool CloseHandle(int handle);

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return; 

			disposed = true;
			CloseHandle(handle);
		}

		void IDisposable.Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override void Close()
		{
			((IDisposable)this).Dispose();
		}

		~WinSerialStream()
		{
			Dispose(false);
		}

		public override void Flush()
		{
			CheckDisposed();
			// No dothing by now
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		[DllImport("kernel32", SetLastError=true)]
		static extern unsafe bool ReadFile(int handle, byte *buffer, int bytes_to_read,
			int *read_bytes, int overlapped);

		public override unsafe int Read(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException();

			int bytes_read = 0;
			bool success;

			fixed (byte *ptr = buffer)
			{
				success = ReadFile(handle, ptr + offset, count, &bytes_read, 0);
			}

			if (!success)
				throw new Win32Exception();

			return bytes_read;
		}

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool WriteFile(int handle, byte* buffer, int bytes_to_write,
			int* bytes_written, int overlapped);

		public override unsafe void Write(byte[] buffer, int offset, int count)
		{
			CheckDisposed();
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException("count > buffer.Length - offset");

			int bytes_written = 0;
			bool success;
			fixed (byte* ptr = buffer)
			{
				success = WriteFile(handle, ptr + offset, count, &bytes_written, 0);
			}

			if (!success)
				throw new Win32Exception();
		}

		void CheckDisposed()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}

	}
}

#endif

