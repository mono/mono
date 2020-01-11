/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// DeflateStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//  Marek Safar (marek.safar@gmail.com)
//
// (c) Copyright 2004,2009 Novell, Inc. <http://www.novell.com>
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Compression
{
	public class DeflateStream : Stream
	{
		delegate int ReadMethod (byte[] array, int offset, int count);
		delegate void WriteMethod (byte[] array, int offset, int count);

		Stream base_stream;
		CompressionMode mode;
		bool leaveOpen;
		bool disposed;
		DeflateStreamNative native;

		public DeflateStream (Stream stream, CompressionMode mode) :
			this (stream, mode, false, false)
		{
		}

		public DeflateStream (Stream stream, CompressionMode mode, bool leaveOpen) :
			this (stream, mode, leaveOpen, false)
		{
		}

		internal DeflateStream (Stream stream, CompressionMode mode, bool leaveOpen, int windowsBits) :
			this (stream, mode, leaveOpen, true)
		{
		}

		internal DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen, bool gzip)
		{
			if (compressedStream == null)
				throw new ArgumentNullException ("compressedStream");

			if (mode != CompressionMode.Compress && mode != CompressionMode.Decompress)
				throw new ArgumentException ("mode");

			this.base_stream = compressedStream;

			this.native = DeflateStreamNative.Create (compressedStream, mode, gzip);
			if (this.native == null) {
				throw new NotImplementedException ("Failed to initialize zlib. You probably have an old zlib installed. Version 1.2.0.4 or later is required.");
			}
			this.mode = mode;
			this.leaveOpen = leaveOpen;
		}
		
		public DeflateStream (Stream stream, CompressionLevel compressionLevel)
			: this (stream, compressionLevel, false, false)
		{
		}
		
		public DeflateStream (Stream stream, CompressionLevel compressionLevel, bool leaveOpen)
			: this (stream, compressionLevel, leaveOpen, false)
		{
		}

		internal DeflateStream (Stream stream, CompressionLevel compressionLevel, bool leaveOpen, int windowsBits)
			: this (stream, compressionLevel, leaveOpen, true)
		{
		}

		internal DeflateStream (Stream stream, CompressionLevel compressionLevel, bool leaveOpen, bool gzip)
			: this (stream, CompressionMode.Compress, leaveOpen, gzip)
		{
		}		

		~DeflateStream () => Dispose (false);

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				GC.SuppressFinalize (this);

			native?.Dispose (disposing);

			if (disposing && !disposed) {
				disposed = true;

				if (!leaveOpen) {
					Stream st = base_stream;
					if (st != null)
						st.Close ();
					base_stream = null;
				}
			}

			base.Dispose (disposing);
		}

		unsafe int ReadInternal (byte[] array, int offset, int count)
		{
			if (count == 0)
				return 0;

			fixed (byte *b = array) {
				IntPtr ptr = new IntPtr (b + offset);
				return native.ReadZStream (ptr, count);
			}
		}

		internal ValueTask<int> ReadAsyncMemory (Memory<byte> destination, CancellationToken cancellationToken)
		{
			return base.ReadAsync(destination, cancellationToken);
		}

		internal int ReadCore (Span<byte> destination)
		{
			var buffer = new byte [destination.Length];
			int count = Read(buffer, 0, buffer.Length);
			buffer.AsSpan(0, count).CopyTo(destination);
			return count;
		}

		public override int Read (byte[] array, int offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
			if (array == null)
				throw new ArgumentNullException ("Destination array is null.");
			if (!CanRead)
				throw new InvalidOperationException ("Stream does not support reading.");
			int len = array.Length;
			if (offset < 0 || count < 0)
				throw new ArgumentException ("Dest or count is negative.");
			if (offset > len)
				throw new ArgumentException ("destination offset is beyond array size");
			if ((offset + count) > len)
				throw new ArgumentException ("Reading would overrun buffer");

			return ReadInternal (array, offset, count);
		}

		unsafe void WriteInternal (byte[] array, int offset, int count)
		{
			if (count == 0)
				return;

			fixed (byte *b = array) {
				IntPtr ptr = new IntPtr (b + offset);
				native.WriteZStream (ptr, count);
			}
		}

		internal ValueTask WriteAsyncMemory (ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
		{
			return base.WriteAsync(source, cancellationToken);
		}

		internal void WriteCore (ReadOnlySpan<byte> source)
		{
			Write (source.ToArray (), 0, source.Length);
		}

		public override void Write (byte[] array, int offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (array == null)
				throw new ArgumentNullException ("array");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if (!CanWrite)
				throw new NotSupportedException ("Stream does not support writing");

			if (offset > array.Length - count)
				throw new ArgumentException ("Buffer too small. count/offset wrong.");

			WriteInternal (array, offset, count);
		}

		public override void Flush ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (CanWrite) {
				native.Flush ();
			}
		}

		public override IAsyncResult BeginRead (byte [] array, int offset, int count,
							AsyncCallback asyncCallback, object asyncState)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			if (array == null)
				throw new ArgumentNullException ("array");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			if (count + offset > array.Length)
				throw new ArgumentException ("Buffer too small. count/offset wrong.");

			ReadMethod r = new ReadMethod (ReadInternal);
			return r.BeginInvoke (array, offset, count, asyncCallback, asyncState);
		}

		public override IAsyncResult BeginWrite (byte [] array, int offset, int count,
							AsyncCallback asyncCallback, object asyncState)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);

			if (!CanWrite)
				throw new InvalidOperationException ("This stream does not support writing");

			if (array == null)
				throw new ArgumentNullException ("array");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			if (count + offset > array.Length)
				throw new ArgumentException ("Buffer too small. count/offset wrong.");

			WriteMethod w = new WriteMethod (WriteInternal);
			return w.BeginInvoke (array, offset, count, asyncCallback, asyncState);			
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ares = asyncResult as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			ReadMethod r = ares.AsyncDelegate as ReadMethod;
			if (r == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			return r.EndInvoke (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ares = asyncResult as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			WriteMethod w = ares.AsyncDelegate as WriteMethod;
			if (w == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			w.EndInvoke (asyncResult);
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
	}

	class DeflateStreamNative
	{
		const int BufferSize = 4096;

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int UnmanagedReadOrWrite (IntPtr buffer, int length, IntPtr data);

		UnmanagedReadOrWrite feeder; // This will be passed to unmanaged code and used there

		Stream base_stream;
		SafeDeflateStreamHandle z_stream;
		GCHandle data;
		bool disposed;
		byte [] io_buffer;
		Exception last_error;

		private DeflateStreamNative ()
		{
		}

		public static DeflateStreamNative Create (Stream compressedStream, CompressionMode mode, bool gzip)
		{
			var dsn = new DeflateStreamNative ();
			dsn.data = GCHandle.Alloc (dsn);
			dsn.feeder = mode == CompressionMode.Compress ? new UnmanagedReadOrWrite (UnmanagedWrite) : new UnmanagedReadOrWrite (UnmanagedRead);
			dsn.z_stream = CreateZStream (mode, gzip, dsn.feeder, GCHandle.ToIntPtr (dsn.data));
			if (dsn.z_stream.IsInvalid) {
				dsn.Dispose (true);
				return null;
			}

			dsn.base_stream = compressedStream;
			return dsn;
		}

		~DeflateStreamNative ()
		{
			Dispose (false);
		}

		public void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;
				GC.SuppressFinalize (this);
			} else {
				// When we are in the finalizer we don't want to access the underlying stream anymore
				base_stream = Stream.Null;
			}
			
			io_buffer = null;

			if (z_stream != null && !z_stream.IsInvalid) {
				z_stream.Dispose();
			}

			if (data != null && data.IsAllocated) {
				data.Free ();
			}
		}

		public void Flush ()
		{
			var res = Flush (z_stream);
			CheckResult (res, "Flush");
		}

		public int ReadZStream (IntPtr buffer, int length)
		{
			var res = ReadZStream (z_stream, buffer, length);
			CheckResult (res, "ReadInternal");
			return res;
		}

		public void WriteZStream (IntPtr buffer, int length)
		{
			var res = WriteZStream (z_stream, buffer, length);
			CheckResult (res, "WriteInternal");
		}

		[Mono.Util.MonoPInvokeCallback (typeof (UnmanagedReadOrWrite))]
		static int UnmanagedRead (IntPtr buffer, int length, IntPtr data)
		{
			GCHandle s = GCHandle.FromIntPtr (data);
			var self = s.Target as DeflateStreamNative;
			if (self == null)
				return -1;
			return self.UnmanagedRead (buffer, length);
		}

		int UnmanagedRead (IntPtr buffer, int length)
		{
			if (io_buffer == null)
				io_buffer = new byte [BufferSize];

			int count = Math.Min (length, io_buffer.Length);
			int n;

			try {
				n = base_stream.Read (io_buffer, 0, count);
			} catch (Exception ex) {
				last_error = ex;
				return -12;
			}

			if (n > 0)
				Marshal.Copy (io_buffer, 0, buffer, n);

			return n;
		}

		[Mono.Util.MonoPInvokeCallback (typeof (UnmanagedReadOrWrite))]
		static int UnmanagedWrite (IntPtr buffer, int length, IntPtr data)
		{
			GCHandle s = GCHandle.FromIntPtr (data);
			var self = s.Target as DeflateStreamNative;
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
				try {
					base_stream.Write (io_buffer, 0, count);
				} catch (Exception ex) {
					last_error = ex;
					return -12;
				}
				unsafe {
					buffer = new IntPtr ((byte *) buffer.ToPointer () + count);
				}
				length -= count;
				total += count;
			}
			return total;
		}

		void CheckResult (int result, string where)
		{
			if (result >= 0)
				return;

			var throw_me = Interlocked.Exchange (ref last_error, null);
			if (throw_me != null)
				throw throw_me;

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

#if ORBIS
		static SafeDeflateStreamHandle CreateZStream (CompressionMode compress, bool gzip, UnmanagedReadOrWrite feeder, IntPtr data)
		{
			throw new PlatformNotSupportedException ();
		}

		static int CloseZStream (IntPtr stream)
		{
			throw new PlatformNotSupportedException ();
		}

		static int Flush (SafeDeflateStreamHandle stream)
		{
			throw new PlatformNotSupportedException ();
		}

		static int ReadZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length)
		{
			throw new PlatformNotSupportedException ();
		}

		static int WriteZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length)
		{
			throw new PlatformNotSupportedException ();
		}
#elif MONOTOUCH || MONODROID || WASM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr CreateZStream (int compress, bool gzip, IntPtr feeder, IntPtr data);

		static SafeDeflateStreamHandle CreateZStream (CompressionMode compress, bool gzip, UnmanagedReadOrWrite feeder, IntPtr data)
		{
			SafeDeflateStreamHandle res;
			try {} finally {
				res = new SafeDeflateStreamHandle (CreateZStream ((int) compress, gzip, Marshal.GetFunctionPointerForDelegate<UnmanagedReadOrWrite> (feeder), data));
			}

			return res;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int CloseZStream (IntPtr stream);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int Flush (IntPtr stream);

		static int Flush (SafeDeflateStreamHandle stream)
		{
			bool release = false;
			try {
				stream.DangerousAddRef (ref release);
				return Flush (stream.DangerousGetHandle ());
			} finally {
				if (release)
					stream.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int ReadZStream (IntPtr stream, IntPtr buffer, int length);

		static int ReadZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length)
		{
			bool release = false;
			try {
				stream.DangerousAddRef (ref release);
				return ReadZStream (stream.DangerousGetHandle (), buffer, length);
			} finally {
				if (release)
					stream.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int WriteZStream (IntPtr stream, IntPtr buffer, int length);

		static int WriteZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length)
		{
			bool release = false;
			try {
				stream.DangerousAddRef (ref release);
				return WriteZStream (stream.DangerousGetHandle (), buffer, length);
			} finally {
				if (release)
					stream.DangerousRelease ();
			}
		}
#else
		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern SafeDeflateStreamHandle CreateZStream (CompressionMode compress, bool gzip, UnmanagedReadOrWrite feeder, IntPtr data);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int CloseZStream (IntPtr stream);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int Flush (SafeDeflateStreamHandle stream);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int ReadZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern int WriteZStream (SafeDeflateStreamHandle stream, IntPtr buffer, int length);
#endif

		sealed class SafeDeflateStreamHandle : SafeHandle
		{
			public override bool IsInvalid
			{
				get { return handle == IntPtr.Zero; }
			}

			private SafeDeflateStreamHandle() : base(IntPtr.Zero, true)
			{
			}

			internal SafeDeflateStreamHandle(IntPtr handle) : base (handle, true)
			{
			}

			override protected bool ReleaseHandle()
			{
				try {
					DeflateStreamNative.CloseZStream(handle);
				} catch {
					;
				}
				return true;
			}
		}
	}
}

