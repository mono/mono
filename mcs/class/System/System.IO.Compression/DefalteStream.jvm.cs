// 
// DeflateStream.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//
// (c) 2008 Mainsoft corp. <http://www.mainsoft.com>
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

//#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using java.io;
using java.util.zip;
using vmw.common;

namespace System.IO.Compression
{
	public class DeflateStream : Stream
	{
		readonly Stream _baseStream;
		readonly InflaterInputStream _reader;
		readonly DeflaterOutputStream _writer;

		readonly bool _leaveOpen;
		bool _open;

		delegate int ReadMethod (byte [] array, int offset, int count);
		delegate void WriteMethod (byte [] array, int offset, int count);

		internal DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen, bool gzip) {
			if (compressedStream == null)
				throw new ArgumentNullException ("compressedStream");

			switch (mode) {
			case CompressionMode.Compress:
				if (!compressedStream.CanWrite)
					throw new ArgumentException ("The base stream is not writeable.");
				OutputStream outStream = new OutputStreamImpl(compressedStream);
				_writer = gzip ? new GZIPOutputStream (outStream) : new DeflaterOutputStream (outStream, new Deflater (Deflater.DEFAULT_COMPRESSION, true));
				break;
			case CompressionMode.Decompress:
				if (!compressedStream.CanRead)
					throw new ArgumentException ("The base stream is not readable.");
				InputStream inStream = new InputStreamImpl (compressedStream);
				_reader = gzip ? new GZIPInputStream (inStream) : new InflaterInputStream (inStream, new Inflater (true));
				break;
			default:
				throw new ArgumentException ("mode");
			}

			_baseStream = compressedStream;
			_leaveOpen = leaveOpen;
			_open = true;
		}

		public DeflateStream (Stream compressedStream, CompressionMode mode)
			:
			this (compressedStream, mode, false, false) { }

		public DeflateStream (Stream compressedStream, CompressionMode mode, bool leaveOpen)
			:
			this (compressedStream, mode, leaveOpen, false) { }

		protected override void Dispose (bool disposing) {
			if (!_open) {
				base.Dispose (disposing);
				return;
			}

			try {
				FlushInternal (true);
				base.Dispose (disposing);
			}
			finally {
				_open = false;
				if (!_leaveOpen)
					_baseStream.Close ();
			}
		}

		private int ReadInternal (byte [] array, int offset, int count) {
			int r = _reader.read (TypeUtils.ToSByteArray (array), offset, count);
			return r < 0 ? 0 : r;
		}

		public override int Read (byte [] dest, int dest_offset, int count) {
			if (!_open)
				throw new ObjectDisposedException ("DeflateStream");
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

		private void WriteInternal (byte [] array, int offset, int count) {
			_writer.write (TypeUtils.ToSByteArray (array), offset, count);
		}

		public override void Write (byte [] src, int src_offset, int count) {
			if (!_open)
				throw new ObjectDisposedException ("DeflateStream");

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
			if (!_open)
				throw new ObjectDisposedException ("DeflateStream");

			if (_writer != null) {
				_writer.flush ();

				if (finish)
					_writer.finish ();
			}
		}

		public override void Flush () {
			FlushInternal (false);
		}

		public override long Seek (long offset, SeekOrigin origin) {
			throw new System.NotSupportedException ();
		}

		public override void SetLength (long value) {
			throw new System.NotSupportedException ();
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state) {
			if (!_open)
				throw new ObjectDisposedException ("DeflateStream");

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
							AsyncCallback cback, object state) {
			if (!_open)
				throw new ObjectDisposedException ("DeflateStream");

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

		public override int EndRead (IAsyncResult async_result) {
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

		public override void EndWrite (IAsyncResult async_result) {
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			AsyncResult ares = async_result as AsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			WriteMethod w = ares.AsyncDelegate as WriteMethod;
			if (w == null)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			w.EndInvoke (async_result);
		}

		public Stream BaseStream {
			get {
				return _baseStream;
			}
		}
		public override bool CanRead {
			get {
				return _open && _reader != null;
			}
		}
		public override bool CanSeek {
			get {
				return false;
			}
		}
		public override bool CanWrite {
			get {
				return _open && _writer != null;
			}
		}
		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}
		public override long Position {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		#region InputStreamImpl

		sealed class InputStreamImpl : InputStream
		{
			readonly Stream _stream;

			public InputStreamImpl (Stream stream) {
				_stream = stream;
			}

			public override void close () {
				BaseStream.Close ();
			}

			public override int read () {
				return BaseStream.ReadByte ();
			}

			public override int read (sbyte [] b, int off, int len) {
				int r = BaseStream.Read ((byte []) TypeUtils.ToByteArray (b), off, len);
				return r == 0 ? -1 : r;
			}

			public override long skip (long n) {
				return BaseStream.Seek (n, SeekOrigin.Current);
			}

			public override bool Equals (object obj) {
				return (obj is InputStreamImpl) &&
					BaseStream.Equals (((InputStreamImpl) obj).BaseStream);
			}

			public override int GetHashCode () {
				return _stream.GetHashCode ();
			}

			public Stream BaseStream {
				get { return _stream; }
			}
		}

		#endregion

		#region OutputStreamImpl

		sealed class OutputStreamImpl : OutputStream
		{
			readonly Stream _stream;

			public OutputStreamImpl (Stream stream) {
				_stream = stream;
			}

			public override void close () {
				BaseStream.Close ();
			}

			public override void flush () {
				BaseStream.Flush ();
			}

			public override void write (int b) {
				BaseStream.WriteByte ((byte) (b & 0xFF));
			}

			public override void write (sbyte [] b, int off, int len) {
				BaseStream.Write ((byte []) TypeUtils.ToByteArray (b), off, len);
			}

			public override bool Equals (object obj) {
				return (obj is OutputStreamImpl) &&
					BaseStream.Equals (((OutputStreamImpl) obj).BaseStream);
			}

			public override int GetHashCode () {
				return _stream.GetHashCode ();
			}

			public Stream BaseStream {
				get { return _stream; }
			}
		}

		#endregion
	}
}
//#endif
