/* -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// 
// DeflateStream.cs
//
// Authors:
//	Christopher James Lahey <clahey@ximian.com>
//
// (c) 2004 Novell, Inc. <http://www.novell.com>
//

#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace System.IO.Compression {
	class DeflateStream : Stream
	{
		private Stream compressedStream;
		private CompressionMode mode;
		private bool leaveOpen;
		private bool open;
		private IntPtr z_stream;

		private const int BUFFER_SIZE = 4096;
		private IntPtr sized_buffer;
		static int bytes_read = 0;
		private bool finished = false;
		private enum ZReturnConsts {
			Z_OK = 0,
			Z_STREAM_END = 1,
			Z_NEED_DICT = 2,
			Z_STREAM_ERROR = -2,
			Z_DATA_ERROR = -3,
			Z_MEM_ERROR = -4,
			Z_BUF_ERROR = -5,
		}
		private enum ZFlushConsts {
			Z_NO_FLUSH     = 0,
			Z_PARTIAL_FLUSH = 1, /* will be removed, use Z_SYNC_FLUSH instead */
			Z_SYNC_FLUSH    = 2,
			Z_FULL_FLUSH    = 3,
			Z_FINISH        = 4,
			Z_BLOCK         = 5,
		};


		[DllImport("MonoPosixHelper")]
		static extern IntPtr create_z_stream(CompressionMode compress, bool gzip);
		[DllImport("MonoPosixHelper")]
		static extern void free_z_stream(IntPtr z_stream);
		[DllImport("MonoPosixHelper")]
		static extern void z_stream_set_next_in(IntPtr z_stream, IntPtr next_in);
		[DllImport("MonoPosixHelper")]
		static extern void z_stream_set_avail_in(IntPtr z_stream, int avail_in);
		[DllImport("MonoPosixHelper")]
		static extern int z_stream_get_avail_in(IntPtr z_stream);
		[DllImport("MonoPosixHelper")]
		static extern void z_stream_set_next_out(IntPtr z_stream, IntPtr next_out);
		[DllImport("MonoPosixHelper")]
		static extern void z_stream_set_avail_out(IntPtr z_stream, int avail_out);
		[DllImport("MonoPosixHelper")]
		static extern ZReturnConsts z_stream_inflate(IntPtr z_stream, ref int avail_out);
		[DllImport("MonoPosixHelper")]
		static extern ZReturnConsts z_stream_deflate(IntPtr z_stream, ZFlushConsts flush, IntPtr next_out, ref int avail_out);

		delegate int  ReadMethod (byte[] array, int offset, int count);
		delegate void WriteMethod(byte[] array, int offset, int count);

		internal DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen, bool gzip) {
			this.compressedStream = compressedStream;
			this.mode = mode;
			this.leaveOpen = leaveOpen;
			this.sized_buffer = Marshal.AllocHGlobal (BUFFER_SIZE);
			this.z_stream = create_z_stream (mode, gzip);
			if (z_stream == IntPtr.Zero) {
				throw new OutOfMemoryException ();
			}
			this.open = true;
			if (mode == CompressionMode.Compress) {
				Flush();
			}
		}

		public DeflateStream (Stream compressedStream, CompressionMode mode) :
			this (compressedStream, mode, false, false) { }

		public DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen) :
			this (compressedStream, mode, leaveOpen, false) { }

		~DeflateStream () {
			Marshal.FreeHGlobal (sized_buffer);
		}

		public override void Close () {
			FlushInternal (true);

			if (mode == CompressionMode.Decompress && compressedStream.CanSeek) {
				int avail_in = z_stream_get_avail_in (z_stream);
				if (avail_in != 0) {
					compressedStream.Seek (- avail_in, SeekOrigin.Current);
					z_stream_set_avail_in (z_stream, 0);
				}
			}

			free_z_stream (z_stream);
			z_stream = IntPtr.Zero;

			if (!leaveOpen) {
				compressedStream.Close();
			}

			open = false;
		}

		private int ReadInternal(byte[] array, int offset, int count) {

			int buffer_size;
			if (finished)
				return 0;

			if (compressedStream.CanSeek)
				buffer_size = BUFFER_SIZE;
			else
				buffer_size = 1;

			IntPtr buffer = Marshal.AllocHGlobal(count);
			try {
				int avail_out;

				avail_out = count;
				z_stream_set_next_out (z_stream, buffer);

				while (avail_out != 0 && !finished) {
					if (z_stream_get_avail_in (z_stream) == 0) {
						byte[] read_buf = new byte[buffer_size];
						int length_read = compressedStream.Read (read_buf, 0, buffer_size);
						bytes_read += length_read;
						if (length_read == 0) {
							break;
						}
						Marshal.Copy (read_buf, 0, sized_buffer, length_read);
						z_stream_set_next_in (z_stream, sized_buffer);
						z_stream_set_avail_in (z_stream, length_read);
					}
					ZReturnConsts ret_val = z_stream_inflate(z_stream, ref avail_out);
					switch (ret_val) {
					case ZReturnConsts.Z_OK:
						break;
					case ZReturnConsts.Z_STREAM_END:
						finished = true;
						break;
					case ZReturnConsts.Z_NEED_DICT:
						throw new InvalidDataException ("ZLib stream requires a dictionary.");
					case ZReturnConsts.Z_DATA_ERROR:
						throw new InvalidDataException ("Invalid ZLib data.");
					case ZReturnConsts.Z_STREAM_ERROR:
						throw new InvalidOperationException ("Internal DeflateStream error.");
					case ZReturnConsts.Z_MEM_ERROR:
						throw new OutOfMemoryException ();
					case ZReturnConsts.Z_BUF_ERROR:
						throw new InvalidOperationException ("Internal DeflateStream error: Buf error.");
					}
				}
				if (count != avail_out)
					Marshal.Copy (buffer, array, offset, count - avail_out);
				return count - avail_out; 
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}

		public override int Read (byte[] dest, int dest_offset, int count)
		{
			if (dest == null)
				throw new ArgumentNullException ("Destination array is null.");
			if (!CanRead)
				throw new NotSupportedException ("Stream does not support reading.");
			int len = dest.Length;
			if (dest_offset < 0 || count < 0)
				throw new ArgumentException ("Dest or count is negative.");
			if (dest_offset > len)
				throw new ArgumentException ("destination offset is beyond array size");
			if ((dest_offset + count) > len)
				throw new ArgumentException ("Reading would overrun buffer");

			return ReadInternal (dest, dest_offset, count);
		}


		private ZReturnConsts do_deflate (ZFlushConsts flush, out int avail_out) {
			avail_out = BUFFER_SIZE;
			ZReturnConsts ret_val = z_stream_deflate (z_stream, flush, sized_buffer, ref avail_out);
			switch (ret_val) {
			case ZReturnConsts.Z_STREAM_ERROR:
				throw new InvalidOperationException ("Internal error.");
			case ZReturnConsts.Z_MEM_ERROR:
				throw new InvalidOperationException ("Memory error.");
			}
			return ret_val;
		}

		private void WriteInternal(byte[] array, int offset, int count) {
			IntPtr buffer = Marshal.AllocHGlobal(count);
			try {
				int avail_in;

				avail_in = count;

				Marshal.Copy (array, offset, buffer, count);
				z_stream_set_next_in (z_stream, buffer);
				z_stream_set_avail_in (z_stream, avail_in);
				while (avail_in != 0) {
					int avail_out;
				
					do_deflate (ZFlushConsts.Z_NO_FLUSH, out avail_out);

					if (avail_out != BUFFER_SIZE) {
						byte[] output = new byte[BUFFER_SIZE - avail_out];
						Marshal.Copy (sized_buffer, output, 0, BUFFER_SIZE - avail_out);
						compressedStream.Write(output, 0, BUFFER_SIZE - avail_out);
					}

					avail_in = z_stream_get_avail_in (z_stream);
				}
			} finally {
				Marshal.FreeHGlobal(buffer);
			}
		}

		public override void Write (byte[] src, int src_offset, int count)
		{
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

		private void FlushInternal (bool finish) {
			int avail_out;
			ZReturnConsts ret_val;

			if (!(open && mode == CompressionMode.Compress && compressedStream.CanWrite))
				return;

			z_stream_set_next_in (z_stream, IntPtr.Zero);
			z_stream_set_avail_in (z_stream, 0);

			while (true) {
				ret_val = do_deflate (finish ? ZFlushConsts.Z_FINISH : ZFlushConsts.Z_SYNC_FLUSH, out avail_out);

				if (BUFFER_SIZE != avail_out) {
					byte[] output = new byte[BUFFER_SIZE - avail_out];
					Marshal.Copy (sized_buffer, output, 0, BUFFER_SIZE - avail_out);
					compressedStream.Write(output, 0, BUFFER_SIZE - avail_out);
				} else {
					if (!finish)
						break;
				}
				if (ret_val == ZReturnConsts.Z_STREAM_END)
					break;
			}

			compressedStream.Flush();
		}

		public override void Flush () {
			FlushInternal (false);
		}

		public override long Seek (long offset, SeekOrigin origin) {
			throw new System.NotSupportedException();
		}

		public override void SetLength (long value) {
			throw new System.NotSupportedException();
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
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
			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support writing");

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

		public override int EndRead(IAsyncResult async_result) {
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

		public Stream BaseStream {
			get {
				return compressedStream;
			}
		}
		public override bool CanRead {
			get {
				return open && mode == CompressionMode.Decompress && compressedStream.CanRead;
			}
		}
		public override bool CanSeek {
			get {
				return false;
			}
		}
		public override bool CanWrite {
			get {
				return open && mode == CompressionMode.Compress && compressedStream.CanWrite;
			}
		}
		public override long Length {
			get {
				throw new System.NotSupportedException();
			}
		}
		public override long Position {
			get {
				throw new System.NotSupportedException();
			}
			set {
				throw new System.NotSupportedException();
			}
		}
	}
}
#endif
