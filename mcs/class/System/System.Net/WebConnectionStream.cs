//
// System.Net.WebConnectionStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc (http://www.novell.com)
//

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

using System.IO;
using System.Text;
using System.Threading;

namespace System.Net
{
	class WebConnectionStream : Stream
	{
		static byte [] crlf = new byte [] { 13, 10 };
		bool isRead;
		WebConnection cnc;
		HttpWebRequest request;
		byte [] readBuffer;
		int readBufferOffset;
		int readBufferSize;
		int contentLength;
		int totalRead;
		long totalWritten;
		bool nextReadCalled;
		int pendingReads;
		int pendingWrites;
		ManualResetEvent pending;
		bool allowBuffering;
		bool sendChunked;
		MemoryStream writeBuffer;
		bool requestWritten;
		byte [] headers;
		bool disposed;
		bool headersSent;
		object locker = new object ();
		bool initRead;
		bool read_eof;
		bool complete_request_written;
		int read_timeout;
		int write_timeout;

		public WebConnectionStream (WebConnection cnc)
		{
			isRead = true;
			pending = new ManualResetEvent (true);
			this.request = cnc.Data.request;
			read_timeout = request.ReadWriteTimeout;
			write_timeout = read_timeout;
			this.cnc = cnc;
			string contentType = cnc.Data.Headers ["Transfer-Encoding"];
			bool chunkedRead = (contentType != null && contentType.ToLower ().IndexOf ("chunked") != -1);
			string clength = cnc.Data.Headers ["Content-Length"];
			if (!chunkedRead && clength != null && clength != "") {
				try {
					contentLength = Int32.Parse (clength);
					if (contentLength == 0 && !IsNtlmAuth ()) {
						ReadAll ();
					}
				} catch {
					contentLength = Int32.MaxValue;
				}
			} else {
				contentLength = Int32.MaxValue;
			}
		}

		public WebConnectionStream (WebConnection cnc, HttpWebRequest request)
		{
			read_timeout = request.ReadWriteTimeout;
			write_timeout = read_timeout;
			isRead = false;
			this.cnc = cnc;
			this.request = request;
			allowBuffering = request.InternalAllowBuffering;
			sendChunked = request.SendChunked;
			if (sendChunked)
				pending = new ManualResetEvent (true);
			else if (allowBuffering)
				writeBuffer = new MemoryStream ();
		}

		bool IsNtlmAuth ()
		{
			bool isProxy = (request.Proxy != null && !request.Proxy.IsBypassed (request.Address));
			string header_name = (isProxy) ? "Proxy-Authenticate" : "WWW-Authenticate";
			string authHeader = cnc.Data.Headers [header_name];
			return (authHeader != null && authHeader.IndexOf ("NTLM") != -1);
		}

		internal void CheckResponseInBuffer ()
		{
			if (contentLength > 0 && (readBufferSize - readBufferOffset) >= contentLength) {
				if (!IsNtlmAuth ())
					ReadAll ();
			}
		}

		internal HttpWebRequest Request {
			get { return request; }
		}

		internal WebConnection Connection {
			get { return cnc; }
		}
#if NET_2_0
		public override bool CanTimeout {
			get { return true; }
		}
#endif

#if NET_2_0
		public override
#endif
		int ReadTimeout {
			get {
				return read_timeout;
			}

			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				read_timeout = value;
			}
		}

#if NET_2_0
		public override
#endif
		int WriteTimeout {
			get {
				return write_timeout;
			}

			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				write_timeout = value;
			}
		}

		internal bool CompleteRequestWritten {
			get { return complete_request_written; }
		}

		internal bool SendChunked {
			set { sendChunked = value; }
		}

		internal byte [] ReadBuffer {
			set { readBuffer = value; }
		}

		internal int ReadBufferOffset {
			set { readBufferOffset = value;}
		}
		
		internal int ReadBufferSize {
			set { readBufferSize = value; }
		}
		
		internal byte[] WriteBuffer {
			get { return writeBuffer.GetBuffer (); }
		}

		internal int WriteBufferLength {
			get { return writeBuffer != null ? (int) writeBuffer.Length : (-1); }
		}

		internal void ForceCompletion ()
		{
			if (!nextReadCalled) {
				if (contentLength == Int32.MaxValue)
					contentLength = 0;
				nextReadCalled = true;
				cnc.NextRead ();
			}
		}
		
		internal void CheckComplete ()
		{
			bool nrc = nextReadCalled;
			if (!nrc && readBufferSize - readBufferOffset == contentLength) {
				nextReadCalled = true;
				cnc.NextRead ();
			}
		}

		internal void ReadAll ()
		{
			if (!isRead || read_eof || totalRead >= contentLength || nextReadCalled) {
				if (isRead && !nextReadCalled) {
					nextReadCalled = true;
					cnc.NextRead ();
				}
				return;
			}

			pending.WaitOne ();
			lock (locker) {
				if (totalRead >= contentLength)
					return;
				
				byte [] b = null;
				int diff = readBufferSize - readBufferOffset;
				int new_size;

				if (contentLength == Int32.MaxValue) {
					MemoryStream ms = new MemoryStream ();
					byte [] buffer = null;
					if (readBuffer != null && diff > 0) {
						ms.Write (readBuffer, readBufferOffset, diff);
						if (readBufferSize >= 8192)
							buffer = readBuffer;
					}

					if (buffer == null)
						buffer = new byte [8192];

					int read;
					while ((read = cnc.Read (request, buffer, 0, buffer.Length)) != 0)
						ms.Write (buffer, 0, read);

					b = ms.GetBuffer ();
					new_size = (int) ms.Length;
					contentLength = new_size;
				} else {
					new_size = contentLength - totalRead;
					b = new byte [new_size];
					if (readBuffer != null && diff > 0) {
						if (diff > new_size)
							diff = new_size;

						Buffer.BlockCopy (readBuffer, readBufferOffset, b, 0, diff);
					}
					
					int remaining = new_size - diff;
					int r = -1;
					while (remaining > 0 && r != 0) {
						r = cnc.Read (request, b, diff, remaining);
						remaining -= r;
						diff += r;
					}
				}

				readBuffer = b;
				readBufferOffset = 0;
				readBufferSize = new_size;
				totalRead = 0;
				nextReadCalled = true;
			}

			cnc.NextRead ();
		}

	   	void WriteCallbackWrapper (IAsyncResult r)
		{
			WebAsyncResult result = r as WebAsyncResult;
			if (result != null && result.AsyncWriteAll)
				return;

			if (r.AsyncState != null) {
				result = (WebAsyncResult) r.AsyncState;
				result.InnerAsyncResult = r;
				result.DoCallback ();
			} else {
				EndWrite (r);
			}
		}

	   	void ReadCallbackWrapper (IAsyncResult r)
		{
			WebAsyncResult result;
			if (r.AsyncState != null) {
				result = (WebAsyncResult) r.AsyncState;
				result.InnerAsyncResult = r;
				result.DoCallback ();
			} else {
				try {
					EndRead (r);
				} catch {
				}
			}
		}

		public override int Read (byte [] buffer, int offset, int size)
		{
			AsyncCallback cb = new AsyncCallback (ReadCallbackWrapper);
			WebAsyncResult res = (WebAsyncResult) BeginRead (buffer, offset, size, cb, null);
			if (!res.IsCompleted && !res.WaitUntilComplete (ReadTimeout, false)) {
				nextReadCalled = true;
				cnc.Close (true);
				throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
			}

			return EndRead (res);
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (!isRead)
				throw new NotSupportedException ("this stream does not allow reading");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException ("offset");
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException ("size");

			lock (locker) {
				pendingReads++;
				pending.Reset ();
			}

			WebAsyncResult result = new WebAsyncResult (cb, state, buffer, offset, size);
			if (totalRead >= contentLength) {
				result.SetCompleted (true, -1);
				result.DoCallback ();
				return result;
			}
			
			int remaining = readBufferSize - readBufferOffset;
			if (remaining > 0) {
				int copy = (remaining > size) ? size : remaining;
				Buffer.BlockCopy (readBuffer, readBufferOffset, buffer, offset, copy);
				readBufferOffset += copy;
				offset += copy;
				size -= copy;
				totalRead += copy;
				if (size == 0 || totalRead >= contentLength) {
					result.SetCompleted (true, copy);
					result.DoCallback ();
					return result;
				}
				result.NBytes = copy;
			}

			if (cb != null)
				cb = new AsyncCallback (ReadCallbackWrapper);

			if (contentLength != Int32.MaxValue && contentLength - totalRead < size)
				size = contentLength - totalRead;

			if (!read_eof) {
				result.InnerAsyncResult = cnc.BeginRead (request, buffer, offset, size, cb, result);
			} else {
				result.SetCompleted (true, result.NBytes);
				result.DoCallback ();
			}
			return result;
		}

		public override int EndRead (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r;
			if (result.EndCalled) {
				int xx = result.NBytes;
				return (xx >= 0) ? xx : 0;
			}

			result.EndCalled = true;

			if (!result.IsCompleted) {
				int nbytes = -1;
				try {
					nbytes = cnc.EndRead (request, result);
				} catch (Exception exc) {
					lock (locker) {
						pendingReads--;
						if (pendingReads == 0)
							pending.Set ();
					}

					nextReadCalled = true;
					cnc.Close (true);
					result.SetCompleted (false, exc);
					result.DoCallback ();
					throw;
				}

				if (nbytes < 0) {
					nbytes = 0;
					read_eof = true;
				}

				totalRead += nbytes;
				result.SetCompleted (false, nbytes + result.NBytes);
				result.DoCallback ();
				if (nbytes == 0)
					contentLength = totalRead;
			}

			lock (locker) {
				pendingReads--;
				if (pendingReads == 0)
					pending.Set ();
			}

			if (totalRead >= contentLength && !nextReadCalled)
				ReadAll ();

			int nb = result.NBytes;
			return (nb >= 0) ? nb : 0;
		}

	   	void WriteRequestAsyncCB (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r.AsyncState;
			try {
				cnc.EndWrite2 (request, r);
				result.SetCompleted (false, 0);
				if (!initRead) {
					initRead = true;
					WebConnection.InitRead (cnc);
				}
			} catch (Exception e) {
				KillBuffer ();
				nextReadCalled = true;
				cnc.Close (true);
				if (e is System.Net.Sockets.SocketException)
					e = new IOException ("Error writing request", e);
				result.SetCompleted (false, e);
			}
			complete_request_written = true;
			result.DoCallback ();
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (request.Aborted)
				throw new WebException ("The request was canceled.", null, WebExceptionStatus.RequestCanceled);

			if (isRead)
				throw new NotSupportedException ("this stream does not allow writing");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException ("offset");
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException ("size");

			if (sendChunked) {
				lock (locker) {
					pendingWrites++;
					pending.Reset ();
				}
			}

			WebAsyncResult result = new WebAsyncResult (cb, state);
			if (!sendChunked)
				CheckWriteOverflow (request.ContentLength, totalWritten, size);
			if (allowBuffering && !sendChunked) {
				if (writeBuffer == null)
					writeBuffer = new MemoryStream ();
				writeBuffer.Write (buffer, offset, size);
				totalWritten += size;
				if (request.ContentLength > 0 && totalWritten == request.ContentLength) {
					try {
						result.AsyncWriteAll = true;
						result.InnerAsyncResult = WriteRequestAsync (new AsyncCallback (WriteRequestAsyncCB), result);
						if (result.InnerAsyncResult == null) {
							if (!result.IsCompleted)
								result.SetCompleted (true, 0);
							result.DoCallback ();
						}
					} catch (Exception exc) {
						result.SetCompleted (true, exc);
						result.DoCallback ();
					}
				} else {
					result.SetCompleted (true, 0);
					result.DoCallback ();
				}
				return result;
			}

			AsyncCallback callback = null;
			if (cb != null)
				callback = new AsyncCallback (WriteCallbackWrapper);

			if (sendChunked) {
				WriteRequest ();

				string cSize = String.Format ("{0:X}\r\n", size);
				byte [] head = Encoding.ASCII.GetBytes (cSize);
				int chunkSize = 2 + size + head.Length;
				byte [] newBuffer = new byte [chunkSize];
				Buffer.BlockCopy (head, 0, newBuffer, 0, head.Length);
				Buffer.BlockCopy (buffer, offset, newBuffer, head.Length, size);
				Buffer.BlockCopy (crlf, 0, newBuffer, head.Length + size, crlf.Length);

				buffer = newBuffer;
				offset = 0;
				size = chunkSize;
			}

			result.InnerAsyncResult = cnc.BeginWrite (request, buffer, offset, size, callback, result);
			totalWritten += size;
			return result;
		}

		void CheckWriteOverflow (long contentLength, long totalWritten, long size)
		{
			if (contentLength == -1)
				return;

			long avail = contentLength - totalWritten;
			if (size > avail) {
				KillBuffer ();
				nextReadCalled = true;
				cnc.Close (true);
				throw new ProtocolViolationException (
					"The number of bytes to be written is greater than " +
					"the specified ContentLength.");
			}
		}

		public override void EndWrite (IAsyncResult r)
		{
			if (r == null)
				throw new ArgumentNullException ("r");

			WebAsyncResult result = r as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			if (result.EndCalled)
				return;

			result.EndCalled = true;
			if (result.AsyncWriteAll) {
				result.WaitUntilComplete ();
				if (result.GotException)
					throw result.Exception;
				return;
			}

			if (allowBuffering && !sendChunked)
				return;

			if (result.GotException)
				throw result.Exception;

			try {
				cnc.EndWrite2 (request, result.InnerAsyncResult);
				result.SetCompleted (false, 0);
				result.DoCallback ();
			} catch (Exception e) {
				result.SetCompleted (false, e);
				result.DoCallback ();
				throw;
			} finally {
				if (sendChunked) {
					lock (locker) {
						pendingWrites--;
						if (pendingWrites == 0)
							pending.Set ();
					}
				}
			}
		}
		
		public override void Write (byte [] buffer, int offset, int size)
		{
			AsyncCallback cb = new AsyncCallback (WriteCallbackWrapper);
			WebAsyncResult res = (WebAsyncResult) BeginWrite (buffer, offset, size, cb, null);
			if (!res.IsCompleted && !res.WaitUntilComplete (WriteTimeout, false)) {
				KillBuffer ();
				nextReadCalled = true;
				cnc.Close (true);
				throw new IOException ("Write timed out.");
			}

			EndWrite (res);
		}

		public override void Flush ()
		{
		}

		internal void SetHeaders (byte [] buffer)
		{
			if (headersSent)
				return;

			headers = buffer;
			long cl = request.ContentLength;
			string method = request.Method;
			bool no_writestream = (method == "GET" || method == "CONNECT" || method == "HEAD" ||
						method == "TRACE" || method == "DELETE");
			if (sendChunked || cl > -1 || no_writestream) {
				WriteHeaders ();
				if (!initRead) {
					initRead = true;
					WebConnection.InitRead (cnc);
				}
				if (!sendChunked && cl == 0)
					requestWritten = true;
			}
		}

		internal bool RequestWritten {
			get { return requestWritten; }
		}

		IAsyncResult WriteRequestAsync (AsyncCallback cb, object state)
		{
			requestWritten = true;
			byte [] bytes = writeBuffer.GetBuffer ();
			int length = (int) writeBuffer.Length;
			// Headers already written to the stream
			return (length > 0) ? cnc.BeginWrite (request, bytes, 0, length, cb, state) : null;
		}

		void WriteHeaders ()
		{
			if (headersSent)
				return;

			headersSent = true;
			string err_msg = null;
			if (!cnc.Write (request, headers, 0, headers.Length, ref err_msg))
				throw new WebException ("Error writing request: " + err_msg, null, WebExceptionStatus.SendFailure, null);
		}

		internal void WriteRequest ()
		{
			if (requestWritten)
				return;

			requestWritten = true;
			if (sendChunked)
				return;

			if (!allowBuffering || writeBuffer == null)
				return;

			byte [] bytes = writeBuffer.GetBuffer ();
			int length = (int) writeBuffer.Length;
			if (request.ContentLength != -1 && request.ContentLength < length) {
				nextReadCalled = true;
				cnc.Close (true);
				throw new WebException ("Specified Content-Length is less than the number of bytes to write", null,
							WebExceptionStatus.ServerProtocolViolation, null);
			}

			if (!headersSent) {
				string method = request.Method;
				bool no_writestream = (method == "GET" || method == "CONNECT" || method == "HEAD" ||
							method == "TRACE" || method == "DELETE");
				if (!no_writestream)
					request.InternalContentLength = length;
				request.SendRequestHeaders (true);
			}
			WriteHeaders ();
			if (cnc.Data.StatusCode != 0 && cnc.Data.StatusCode != 100)
				return;
				
			IAsyncResult result = null;
			if (length > 0)
				result = cnc.BeginWrite (request, bytes, 0, length, null, null);
			
			if (!initRead) {
				initRead = true;
				WebConnection.InitRead (cnc);
			}

			if (length > 0) 
				complete_request_written = cnc.EndWrite (request, result);
			else
				complete_request_written = true;
		}

		internal void InternalClose ()
		{
			disposed = true;
		}

		public override void Close ()
		{
			if (sendChunked) {
				if (disposed)
					return;
				disposed = true;
				pending.WaitOne ();
				byte [] chunk = Encoding.ASCII.GetBytes ("0\r\n\r\n");
				string err_msg = null;
				cnc.Write (request, chunk, 0, chunk.Length, ref err_msg);
				return;
			}

			if (isRead) {
				if (!nextReadCalled) {
					CheckComplete ();
					// If we have not read all the contents
					if (!nextReadCalled) {
						nextReadCalled = true;
						cnc.Close (true);
					}
				}
				return;
			} else if (!allowBuffering) {
				complete_request_written = true;
				if (!initRead) {
					initRead = true;
					WebConnection.InitRead (cnc);
				}
				return;
			}

			if (disposed || requestWritten)
				return;

			long length = request.ContentLength;

			if (!sendChunked && length != -1 && totalWritten != length) {
				IOException io = new IOException ("Cannot close the stream until all bytes are written");
				nextReadCalled = true;
				cnc.Close (true);
				throw new WebException ("Request was cancelled.", io, WebExceptionStatus.RequestCanceled);
			}

			WriteRequest ();
			disposed = true;
		}

		internal void KillBuffer ()
		{
			writeBuffer = null;
		}

		public override long Seek (long a, SeekOrigin b)
		{
			throw new NotSupportedException ();
		}
		
		public override void SetLength (long a)
		{
			throw new NotSupportedException ();
		}
		
		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanRead {
			get { return !disposed && isRead; }
		}

		public override bool CanWrite {
			get { return !disposed && !isRead; }
		}

		public override long Length {
			get { throw new NotSupportedException (); }
		}

		public override long Position {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
}

