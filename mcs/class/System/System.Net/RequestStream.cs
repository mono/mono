//
// System.Net.RequestStream
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace System.Net {
	class RequestStream : NetworkStream
	{
		byte [] buffer;
		int offset;
		int length;
		long available;
		bool disposed;

		internal RequestStream (Socket sock, byte [] buffer, int offset, int length) :
					base (sock, false)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.length = length;
			this.available = -1;
		}

		internal RequestStream (Socket sock, byte [] buffer, int offset, int length, long contentlength) :
					base (sock, false)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.length = length;
			this.available = contentlength;
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return false; }
		}

		public override long Length {
			get { throw new NotSupportedException (); }
		}

		public override long Position {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}


		public override void Close ()
		{
			// TODO: What do we close?
		}

		public override void Flush ()
		{
		}

		// true if the we have >= count bytes in this.buffer
		int FillFromBuffer (byte [] buffer, int off, int count)
		{
			int size = this.length - this.offset;
			if (size <= 0)
				return 0;

			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (off < 0)
				throw new ArgumentOutOfRangeException ("offset", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			int len = buffer.Length;
			if (off > len)
				throw new ArgumentException ("destination offset is beyond array size");
			if (off > len - count)
				throw new ArgumentException ("Reading would overrun buffer");

			size = Math.Min (size, count);
			Buffer.BlockCopy (this.buffer, this.offset, buffer, off, size);
			this.offset += size;
			return size;
		}

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (typeof (RequestStream).ToString ());

			if (available == 0)
				return 0;

			int nread = FillFromBuffer (buffer, offset, count);
			if (nread > 0)
				return nread;

			// Avoid reading past the end of the request to allow
			// for HTTP pipelining
			if (available != -1 && count > available)
				count = (int) available;

			nread = base.Read (buffer, offset, count);
			if (available != -1)
				available -= nread;
			return nread;
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (typeof (RequestStream).ToString ());

			int nread = FillFromBuffer (buffer, offset, count);
			if (nread > 0 || available == 0) {
				HttpStreamAsyncResult ares = new HttpStreamAsyncResult ();
				ares.Buffer = buffer;
				ares.Offset = offset;
				ares.Count = count;
				ares.Callback = cback;
				ares.State = state;
				ares.SynchRead = nread;
				ares.Complete ();
				return ares;
			}

			// Avoid reading past the end of the request to allow
			// for HTTP pipelining
			if (available != -1 && count > available)
				count = (int) Math.Min (Int32.MaxValue, available);
			return base.BeginRead (buffer, offset, count, cback, state);
		}

		public override int EndRead (IAsyncResult ares)
		{
			if (disposed)
				throw new ObjectDisposedException (typeof (RequestStream).ToString ());

			if (ares == null)
				throw new ArgumentNullException ("async_result");

			if (ares is HttpStreamAsyncResult) {
				HttpStreamAsyncResult r = (HttpStreamAsyncResult) ares;
				if (!ares.IsCompleted)
					ares.AsyncWaitHandle.WaitOne ();
				return r.SynchRead;
			}

			// Close on exception?
			int nread = base.EndRead (ares);
			if (available != -1)
				available -= nread;
			return nread;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			throw new NotSupportedException ();
		}

		public override void EndWrite (IAsyncResult async_result)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif

