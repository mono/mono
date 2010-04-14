/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// DeflateStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (c) Copyright 2004,2009 Novell, Inc. <http://www.novell.com>
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

#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

#if MONOTOUCH
using MonoTouch;
#endif

namespace System.IO.Compression {
	public class DeflateStream : Stream
	{
		const int BufferSize = 4096;
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int UnmanagedReadOrWrite (IntPtr buffer, int length, IntPtr data);
		delegate int ReadMethod (byte[] array, int offset, int count);
		delegate void WriteMethod (byte[] array, int offset, int count);

		Stream base_stream;
		CompressionMode mode;
		bool leaveOpen;
		bool disposed;
		UnmanagedReadOrWrite feeder; // This will be passed to unmanaged code and used there
		IntPtr z_stream;
		byte [] io_buffer;

		GCHandle data;

		public DeflateStream (Stream compressedStream, CompressionMode mode) :
			this (compressedStream, mode, false, false)
		{
		}

		public DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen) :
			this (compressedStream, mode, leaveOpen, false)
		{
		}

		internal DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen, bool gzip)
		{
			if (compressedStream == null)
				throw new ArgumentNullException ("compressedStream");

			if (mode != CompressionMode.Compress && mode != CompressionMode.Decompress)
				throw new ArgumentException ("mode");

			this.data = GCHandle.Alloc (this);
			this.base_stream = compressedStream;
			this.feeder = (mode == CompressionMode.Compress) ? new UnmanagedReadOrWrite (UnmanagedWrite) :
									   new UnmanagedReadOrWrite (UnmanagedRead);
			this.z_stream = CreateZStream (mode, gzip, feeder, GCHandle.ToIntPtr (data));
			if (z_stream == IntPtr.Zero) {
				this.base_stream = null;
				this.feeder = null;
				throw new NotImplementedException ("Failed to initialize zlib. You probably have an old zlib installed. Version 1.2.0.4 or later is required.");
			}
			this.mode = mode;
			this.leaveOpen = leaveOpen;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;
				IntPtr zz = z_stream;
				z_stream = IntPtr.Zero;
				int res = 0;
				if (zz != IntPtr.Zero)
					res = CloseZStream (zz); // This will Flush() the remaining output if any

				io_buffer = null;
				if (!leaveOpen) {
					Stream st = base_stream;
					if (st != null)
						st.Close ();
					base_stream = null;
				}
				CheckResult (res, "Dispose");
			}

			if (data.IsAllocated) {
				data.Free ();
				data = new GCHandle ();
			}

			base.Dispose (disposing);
		}

#if MONOTOUCH
		[MonoPInvokeCallback (typeof (UnmanagedReadOrWrite))]
#endif
		static int UnmanagedRead (IntPtr buffer, int length, IntPtr data)
		{
			GCHandle s = GCHandle.FromIntPtr (data);
			var self = s.Target as DeflateStream;
			if (self == null)
				return -1;
			return self.UnmanagedRead (buffer, length);
		}

		int UnmanagedRead (IntPtr buffer, int length)
		{
			int total = 0;
			int n = 1;
			while (length > 0 && n > 0) {
				if (io_buffer == null)
					io_buffer = new byte [BufferSize];

				int count = Math.Min (length, io_buffer.Length);
				n = base_stream.Read (io_buffer, 0, count);
				if (n > 0) {
					Marshal.Copy (io_buffer, 0, buffer, n);
					unsafe {
						buffer = new IntPtr ((byte *) buffer.ToPointer () + n);
					}
					length -= n;
					total += n;
				}
			}
			return total;
		}

#if MONOTOUCH
		[MonoPInvokeCallback (typeof (UnmanagedReadOrWrite))]
#endif
		static int UnmanagedWrite (IntPtr buffer, int length, IntPtr data)
		{
			GCHandle s = GCHandle.FromIntPtr (data);
			var self = s.Target as DeflateStream;
			if (self == null)
				return -1;
			return self.UnmanagedWrite (buffer, length);
		}

		int UnmanagedWrite (IntPtr buffer, int length)
		{
			int total = 0;
			while (length > 0) {
				if (io_buffer == null)
					io_buffer = new byte [BufferSize];

				int count = Math.Min (length, io_buffer.Length);
				Marshal.Copy (buffer, io_buffer, 0, count);
				base_stream.Write (io_buffer, 0, count);
				unsafe {
					buffer = new IntPtr ((byte *) buffer.ToPointer () + count);
				}
				length -= count;
				total += count;
			}
			return total;
		}

		unsafe int ReadInternal (byte[] array, int offset, int count)
		{
			if (count == 0)
				return 0;

			int result = 0;
			fixed (byte *b = array) {
				IntPtr ptr = new IntPtr (b + offset);
				result = ReadZStream (z_stream, ptr, count);
			}
			CheckResult (result, "ReadInternal");
			return result;
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
			if (dest == null)
				throw new ArgumentNullException ("Destination array is null.");
			if (!CanRead)
				throw new InvalidOperationException ("Stream does not support reading.");
			int len = dest.Length;
			if (dest_offset < 0 || count < 0)
				throw new ArgumentException ("Dest or count is negative.");
			if (dest_offset > len)
				throw new ArgumentException ("destination offset is beyond array size");
			if ((dest_offset + count) > len)
				throw new ArgumentException ("Reading would overrun buffer");

			return ReadInternal (dest, dest_offset, count);
		}

		unsafe void WriteInternal (byte[] array, int offset, int count)
		{
			if (count == 0)
				return;

			int result = 0;
			fixed (byte *b = array) {
				IntPtr ptr = new IntPtr (b + offset);
				result = WriteZStream (z_stream, ptr, count);
			}
			CheckResult (result, "WriteInternal");
		}

		public override void Write (byte[] src, int src_offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (src == null)
				throw new ArgumentNullException ("src");

			if (src_offset < 0)
				throw new ArgumentOutOfRangeException ("src_offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			WriteInternal (src, src_offset, count);
		}

		static void CheckResult (int result, string where)
		{
			if (result >= 0)
				return;

			string error;
			switch (result) {
			case -1: // Z_ERRNO
				error = "Unknown error"; // Marshal.GetLastWin32() ?
				break;
			case -2: // Z_STREAM_ERROR
				error = "Internal error";
				break;
			case -3: // Z_DATA_ERROR
				error = "Corrupted data";
				break;
			case -4: // Z_MEM_ERROR
				error = "Not enough memory";
				break;
			case -5: // Z_BUF_ERROR
				error = "Internal error (no progress possible)";
				break;
			case -6: // Z_VERSION_ERROR
				error = "Invalid version";
				break;
			case -10:
				error = "Invalid argument(s)";
				break;
			case -11:
				error = "IO error";
				break;
			default:
				error = "Unknown error";
				break;
			}

			throw new IOException (error + " " + where);
		}

		public override void Flush ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (CanWrite) {
				int result = Flush (z_stream);
				CheckResult (result, "Flush");
			}
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			if (count + offset > buffer.Length)
				throw new ArgumentException ("Buffer too small. count/offset wrong.");

			ReadMethod r = new ReadMethod (ReadInternal);
			return r.BeginInvoke (buffer, offset, count, cback, state);
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (!CanWrite)
				throw new InvalidOperationException ("This stream does not support writing");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			if (count + offset > buffer.Length)
				throw new ArgumentException ("Buffer too small. count/offset wrong.");

			WriteMethod w = new WriteMethod (WriteInternal);
			return w.BeginInvoke (buffer, offset, count, cback, state);			
		}

		public override int EndRead(IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			AsyncResult ares = async_result as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			ReadMethod r = ares.AsyncDelegate as ReadMethod;
			if (r == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			return r.EndInvoke (async_result);
		}

		public override void EndWrite (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			AsyncResult ares = async_result as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			WriteMethod w = ares.AsyncDelegate as WriteMethod;
			if (w == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			w.EndInvoke (async_result);
			return;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException();
		}

		public Stream BaseStream {
			get { return base_stream; }
		}

		public override bool CanRead {
			get { return !disposed && mode == CompressionMode.Decompress && base_stream.CanRead; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return !disposed && mode == CompressionMode.Compress && base_stream.CanWrite; }
		}

		public override long Length {
			get { throw new NotSupportedException(); }
		}

		public override long Position {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

#if MONOTOUCH
		const string LIBNAME = "__Internal";
#else
		const string LIBNAME = "MonoPosixHelper";
#endif

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr CreateZStream (CompressionMode compress, bool gzip, UnmanagedReadOrWrite feeder, IntPtr data);

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern int CloseZStream (IntPtr stream);

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern int Flush (IntPtr stream);

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern int ReadZStream (IntPtr stream, IntPtr buffer, int length);

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern int WriteZStream (IntPtr stream, IntPtr buffer, int length);
	}
}
#endif

