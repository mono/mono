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
		int stream_length; // -1 when CL not present
		long contentLength;
		long totalRead;
		internal long totalWritten;
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
		AsyncCallback cb_wrapper; // Calls to ReadCallbackWrapper or WriteCallbacWrapper
		internal bool IgnoreIOErrors;

		public WebConnectionStream (WebConnection cnc, WebConnectionData data)
		{          
			if (data == null)
				throw new InvalidOperationException ("data was not initialized");
			if (data.Headers == null)
				throw new InvalidOperationException ("data.Headers was not initialized");
			if (data.request == null)
				throw new InvalidOperationException ("data.request was not initialized");
			isRead = true;
			cb_wrapper = new AsyncCallback (ReadCallbackWrapper);
			pending = new ManualResetEvent (true);
			this.request = data.request;
			read_timeout = request.ReadWriteTimeout;
			write_timeout = read_timeout;
			this.cnc = cnc;
			string contentType = data.Headers ["Transfer-Encoding"];
			bool chunkedRead = (contentType != null && contentType.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);
			string clength = data.Headers ["Content-Length"];
			if (!chunkedRead && clength != null && clength != "") {
				try {
					contentLength = Int32.Parse (clength);
					if (contentLength == 0 && !IsNtlmAuth ()) {
						ReadAll ();
					}
				} catch {
					contentLength = Int64.MaxValue;
				}
			} else {
				contentLength = Int64.MaxValue;
			}

			// Negative numbers?
			if (!Int32.TryParse (clength, out stream_length))
				stream_length = -1;
		}

		public WebConnectionStream (WebConnection cnc, HttpWebRequest request)
		{
			read_timeout = request.ReadWriteTimeout;
			write_timeout = read_timeout;
			isRead = false;
			cb_wrapper = new AsyncCallback (WriteCallbackWrapper);
			this.cnc = cnc;
			this.request = request;
			allowBuffering = request.InternalAllowBuffering;
			sendChunked = request.SendChunked;
			if (sendChunked)
				pending = new ManualResetEvent (true);
			else if (allowBuffering)
				writeBuffer = new MemoryStream ();
		}

		bool CheckAuthHeader (string headerName)
		{
			var authHeader = cnc.Data.Headers [headerName];
			return (authHeader != null && authHeader.IndexOf ("NTLM", StringComparison.Ordinal) != -1);
		}

		bool IsNtlmAuth ()
		{
			bool isProxy = (request.Proxy != null && !request.Proxy.IsBypassed (request.Address));
			if (isProxy && CheckAuthHeader ("Proxy-Authenticate"))
				return true;
			return CheckAuthHeader ("WWW-Authenticate");
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
		public override bool CanTimeout {
			get { return true; }
		}

		public override int ReadTimeout {
			get {
				return read_timeout;
			}

			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				read_timeout = value;
			}
		}

		public override int WriteTimeout {
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
			set { readBufferOffset = value; }
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
				if (contentLength == Int64.MaxValue)
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

			if (!pending.WaitOne (ReadTimeout))
				throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
			lock (locker) {
				if (totalRead >= contentLength)
					return;
				
				byte [] b = null;
				int diff = readBufferSize - readBufferOffset;
				int new_size;

				if (contentLength == Int64.MaxValue) {
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
					new_size = (int) (contentLength - totalRead);
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
				try {
					EndWrite (r);
				} catch {
				}
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
			AsyncCallback cb = cb_wrapper;
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
				cb = cb_wrapper;

			if (contentLength != Int64.MaxValue && contentLength - totalRead < size)
				size = (int)(contentLength - totalRead);

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

	   	void WriteAsyncCB (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r.AsyncState;
			result.InnerAsyncResult = null;

			try {
				cnc.EndWrite (request, true, r);
				result.SetCompleted (false, 0);
				if (!initRead) {
					initRead = true;
					cnc.InitRead ();
				}
			} catch (Exception e) {
				KillBuffer ();
				nextReadCalled = true;
				cnc.Close (true);
				if (e is System.Net.Sockets.SocketException)
					e = new IOException ("Error writing request", e);
				result.SetCompleted (false, e);
			}

			if (allowBuffering && !sendChunked && request.ContentLength > 0 && totalWritten == request.ContentLength)
				complete_request_written = true;

			result.DoCallback ();
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (request.Aborted)
				throw new WebException ("The request was canceled.", WebExceptionStatus.RequestCanceled);

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
			AsyncCallback callback = new AsyncCallback (WriteAsyncCB);

			if (sendChunked) {
				requestWritten = true;

				string cSize = String.Format ("{0:X}\r\n", size);
				byte[] head = Encoding.ASCII.GetBytes (cSize);
				int chunkSize = 2 + size + head.Length;
				byte[] newBuffer = new byte [chunkSize];
				Buffer.BlockCopy (head, 0, newBuffer, 0, head.Length);
				Buffer.BlockCopy (buffer, offset, newBuffer, head.Length, size);
				Buffer.BlockCopy (crlf, 0, newBuffer, head.Length + size, crlf.Length);

				if (allowBuffering) {
					if (writeBuffer == null)
						writeBuffer = new MemoryStream ();
					writeBuffer.Write (buffer, offset, size);
					totalWritten += size;
				}

				buffer = newBuffer;
				offset = 0;
				size = chunkSize;
			} else {
				CheckWriteOverflow (request.ContentLength, totalWritten, size);

				if (allowBuffering) {
					if (writeBuffer == null)
						writeBuffer = new MemoryStream ();
					writeBuffer.Write (buffer, offset, size);
					totalWritten += size;

					if (request.ContentLength <= 0 || totalWritten < request.ContentLength) {
						result.SetCompleted (true, 0);
						result.DoCallback ();
						return result;
					}

					result.AsyncWriteAll = true;
					requestWritten = true;
					buffer = writeBuffer.GetBuffer ();
					offset = 0;
					size = (int)totalWritten;
				}
			}

			try {
				result.InnerAsyncResult = cnc.BeginWrite (request, buffer, offset, size, callback, result);
				if (result.InnerAsyncResult == null) {
					if (!result.IsCompleted)
						result.SetCompleted (true, 0);
					result.DoCallback ();
				}
			} catch (Exception) {
				if (!IgnoreIOErrors)
					throw;
				result.SetCompleted (true, 0);
				result.DoCallback ();
			}
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

			if (sendChunked) {
				lock (locker) {
					pendingWrites--;
					if (pendingWrites <= 0)
						pending.Set ();
				}
			}

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
		}
		
		public override void Write (byte [] buffer, int offset, int size)
		{
			AsyncCallback cb = cb_wrapper;
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

		internal void SetHeadersAsync (bool setInternalLength, SimpleAsyncCallback callback)
		{
			SimpleAsyncResult.Run (r => SetHeadersAsync (r, setInternalLength), callback);
		}

		bool SetHeadersAsync (SimpleAsyncResult result, bool setInternalLength)
		{
			if (headersSent)
				return false;

			string method = request.Method;
			bool no_writestream = (method == "GET" || method == "CONNECT" || method == "HEAD" ||
			                      method == "TRACE");
			bool webdav = (method == "PROPFIND" || method == "PROPPATCH" || method == "MKCOL" ||
			              method == "COPY" || method == "MOVE" || method == "LOCK" ||
			              method == "UNLOCK");

			if (setInternalLength && !no_writestream && writeBuffer != null)
				request.InternalContentLength = writeBuffer.Length;

			bool has_content = !no_writestream && (writeBuffer == null || request.ContentLength > -1);
			if (!(sendChunked || has_content || no_writestream || webdav))
				return false;

			headersSent = true;
			headers = request.GetRequestHeaders ();

			var innerResult = cnc.BeginWrite (request, headers, 0, headers.Length, r => {
				try {
					cnc.EndWrite (request, true, r);
					if (!initRead) {
						initRead = true;
						cnc.InitRead ();
					}
					var cl = request.ContentLength;
					if (!sendChunked && cl == 0)
						requestWritten = true;
					result.SetCompleted (false);
				} catch (WebException e) {
					result.SetCompleted (false, e);
				} catch (Exception e) {
					result.SetCompleted (false, new WebException ("Error writing headers", WebExceptionStatus.SendFailure, WebExceptionInternalStatus.RequestFatal, e));
				}
			}, null);

			return innerResult != null;
		}

		internal bool RequestWritten {
			get { return requestWritten; }
		}

		internal SimpleAsyncResult WriteRequestAsync (SimpleAsyncCallback callback)
		{
			var result = WriteRequestAsync (callback);
			try {
				if (!WriteRequestAsync (result))
					result.SetCompleted (true);
			} catch (Exception ex) {
				result.SetCompleted (true, ex);
			}
			return result;
		}

		internal bool WriteRequestAsync (SimpleAsyncResult result)
		{
			if (requestWritten)
				return false;

			requestWritten = true;
			if (sendChunked || !allowBuffering || writeBuffer == null)
				return false;

			// Keep the call for a potential side-effect of GetBuffer
			var bytes = writeBuffer.GetBuffer ();
			var length = (int)writeBuffer.Length;
			if (request.ContentLength != -1 && request.ContentLength < length) {
				nextReadCalled = true;
				cnc.Close (true);
				throw new WebException ("Specified Content-Length is less than the number of bytes to write", null,
					WebExceptionStatus.ServerProtocolViolation, null);
			}

			SetHeadersAsync (true, inner => {
				if (inner.GotException) {
					result.SetCompleted (inner.CompletedSynchronouslyPeek, inner.Exception);
					return;
				}

				if (cnc.Data.StatusCode != 0 && cnc.Data.StatusCode != 100) {
					result.SetCompleted (inner.CompletedSynchronouslyPeek);
					return;
				}

				if (!initRead) {
					initRead = true;
					cnc.InitRead ();
				}

				if (length == 0) {
					complete_request_written = true;
					result.SetCompleted (inner.CompletedSynchronouslyPeek);
					return;
				}

				cnc.BeginWrite (request, bytes, 0, length, r => {
					try {
						complete_request_written = cnc.EndWrite (request, false, r);
						result.SetCompleted (false);
					} catch (Exception exc) {
						result.SetCompleted (false, exc);
					}
				}, null);
			});

			return true;
		}

		internal void InternalClose ()
		{
			disposed = true;
		}

		internal bool GetResponseOnClose {
			get; set;
		}

		public override void Close ()
		{
			if (GetResponseOnClose) {
				if (disposed)
					return;
				disposed = true;
				var response = (HttpWebResponse)request.GetResponse ();
				response.ReadAll ();
				response.Close ();
				return;
			}

			if (sendChunked) {
				if (disposed)
					return;
				disposed = true;
				if (!pending.WaitOne (WriteTimeout)) {
					throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
				}
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
					cnc.InitRead ();
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
				throw new WebException ("Request was cancelled.", WebExceptionStatus.RequestCanceled, WebExceptionInternalStatus.RequestFatal, io);
			}

			// Commented out the next line to fix xamarin bug #1512
			//WriteRequest ();
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
			get {
				if (!isRead)
					throw new NotSupportedException ();
				return stream_length;
			}
		}

		public override long Position {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
}

