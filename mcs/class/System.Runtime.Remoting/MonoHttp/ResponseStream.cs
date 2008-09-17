#define EMBEDDED_IN_1_0

//
// System.Net.ResponseStream
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

#if EMBEDDED_IN_1_0

using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System; using System.Net; namespace MonoHttp {
	// FIXME: Does this buffer the response until Close?
	// What happens when we set content-length to X and write X-1 bytes then close?
	// what if we don't set content-length at all?
	class ResponseStream : Stream
	{
		HttpListenerResponse response;
		bool ignore_errors;
		bool disposed;
		bool trailer_sent;
		Stream stream;

		internal ResponseStream (Stream stream, HttpListenerResponse response, bool ignore_errors)
		{
			this.response = response;
			this.ignore_errors = ignore_errors;
			this.stream = stream;
		}

		public override bool CanRead {
			get { return false; }
		}

		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanWrite {
			get { return true; }
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
			if (disposed == false) {
				disposed = true;
				if (response.HeadersSent == false)
					response.SendHeaders (true);

				if (response.SendChunked && !trailer_sent) {
					WriteChunkSize (0, true);
					trailer_sent = true;
				}
				response.Close ();
			}
		}

		public override void Flush ()
		{
		}

		static byte [] crlf = new byte [] { 13, 10 };
		void WriteChunkSize (int size, bool final)
		{
			string str = String.Format ("{0:x}\r\n{1}", size, final ? "\r\n" : "");
			byte [] b = Encoding.ASCII.GetBytes (str);
			stream.Write (b, 0, b.Length);
		}

		internal void InternalWrite (byte [] buffer, int offset, int count)
		{
			if (ignore_errors) {
				try {
					stream.Write (buffer, offset, count);
				} catch { }
			} else {
				stream.Write (buffer, offset, count);
			}
		}

		public override void Write (byte [] buffer, int offset, int count)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (response.HeadersSent == false)
				response.SendHeaders (false);

			bool chunked = response.SendChunked;
			try {
				if (chunked)
					WriteChunkSize (count, false);
			} catch { }
			InternalWrite (buffer, offset, count);
			try {
				if (chunked)
					stream.Write (crlf, 0, 2);
			} catch { }
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (response.HeadersSent == false)
				response.SendHeaders (false);

			try {
				if (response.SendChunked)
					WriteChunkSize (count, false);
			} catch { }
			return stream.BeginWrite (buffer, offset, count, cback, state);
		}

		public override void EndWrite (IAsyncResult ares)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (ignore_errors) {
				try {
					stream.EndWrite (ares);
					if (response.SendChunked)
						stream.Write (crlf, 0, 2);
				} catch { }
			} else {
				stream.EndWrite (ares);
				if (response.SendChunked)
					stream.Write (crlf, 0, 2);
			}
		}

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			throw new NotSupportedException ();
		}

		public override int EndRead (IAsyncResult ares)
		{
			throw new NotSupportedException ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif

