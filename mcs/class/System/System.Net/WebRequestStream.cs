//
// WebRequestStream.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Net.Sockets;

namespace System.Net
{
	class WebRequestStream : WebConnectionStream
	{
		static byte[] crlf = new byte[] { 13, 10 };
		MemoryStream writeBuffer;
		bool requestWritten;
		bool allowBuffering;
		bool sendChunked;
		TaskCompletionSource<int> pendingWrite;
		long totalWritten;
		byte[] headers;
		bool headersSent;

		internal string ME {
			get;
		}

		public WebRequestStream (WebConnection connection, WebOperation operation,
		                         Stream stream, WebConnectionTunnel tunnel)
			: base (connection, operation, stream)
		{
			allowBuffering = operation.Request.InternalAllowBuffering;
			sendChunked = operation.Request.SendChunked && operation.WriteBuffer == null;
			if (!sendChunked && allowBuffering && operation.WriteBuffer == null)
				writeBuffer = new MemoryStream ();

			KeepAlive = Request.KeepAlive;
			if (tunnel?.ProxyVersion != null && tunnel?.ProxyVersion != HttpVersion.Version11)
				KeepAlive = false;

			ME = $"WRQ(Cnc={Connection.ID}, Op={Operation.ID})";
		}

		public bool KeepAlive {
			get;
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override bool CanRead => false;

		public override bool CanWrite => true;

		internal bool SendChunked {
			get { return sendChunked; }
			set { sendChunked = value; }
		}

		internal bool HasWriteBuffer {
			get {
				return Operation.WriteBuffer != null || writeBuffer != null;
			}
		}

		internal int WriteBufferLength {
			get {
				if (Operation.WriteBuffer != null)
					return Operation.WriteBuffer.Size;
				if (writeBuffer != null)
					return (int)writeBuffer.Length;
				return -1;
			}
		}

		internal BufferOffsetSize GetWriteBuffer ()
		{
			if (Operation.WriteBuffer != null)
				return Operation.WriteBuffer;
			if (writeBuffer == null || writeBuffer.Length == 0)
				return null;
			var buffer = writeBuffer.GetBuffer ();
			return new BufferOffsetSize (buffer, 0, (int)writeBuffer.Length, false);
		}

		public override async Task WriteAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} WRITE ASYNC");

			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			if (Operation.WriteBuffer != null)
				throw new InvalidOperationException ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			var myWriteTcs = new TaskCompletionSource<int> ();
			if (Interlocked.CompareExchange (ref pendingWrite, myWriteTcs, null) != null)
				throw new InvalidOperationException (SR.GetString (SR.net_repcall));

			try {
				await ProcessWrite (buffer, offset, size, cancellationToken).ConfigureAwait (false);

				if (allowBuffering && !sendChunked && Request.ContentLength > 0 && totalWritten == Request.ContentLength)
					Operation.CompleteRequestWritten (this);

				pendingWrite = null;
				myWriteTcs.TrySetResult (0);
			} catch (Exception ex) {
				KillBuffer ();
				closed = true;

				if (ex is SocketException)
					ex = new IOException ("Error writing request", ex);

				Operation.CompleteRequestWritten (this, ex);

				pendingWrite = null;
				myWriteTcs.TrySetException (ex);
				throw;
			}

		}

		async Task ProcessWrite (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			if (sendChunked) {
				requestWritten = true;

				string cSize = String.Format ("{0:X}\r\n", size);
				byte[] head = Encoding.ASCII.GetBytes (cSize);
				int chunkSize = 2 + size + head.Length;
				byte[] newBuffer = new byte[chunkSize];
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
				CheckWriteOverflow (Request.ContentLength, totalWritten, size);

				if (allowBuffering) {
					if (writeBuffer == null)
						writeBuffer = new MemoryStream ();
					writeBuffer.Write (buffer, offset, size);
					totalWritten += size;

					if (Request.ContentLength <= 0 || totalWritten < Request.ContentLength)
						return;

					requestWritten = true;
					buffer = writeBuffer.GetBuffer ();
					offset = 0;
					size = (int)totalWritten;
				}
			}

			try {
				await InnerStream.WriteAsync (buffer, offset, size, cancellationToken).ConfigureAwait (false);
			} catch {
				if (!IgnoreIOErrors)
					throw;
			}
			totalWritten += size;
		}

		void CheckWriteOverflow (long contentLength, long totalWritten, long size)
		{
			if (contentLength == -1)
				return;

			long avail = contentLength - totalWritten;
			if (size > avail) {
				KillBuffer ();
				closed = true;
				var throwMe = new ProtocolViolationException (
					"The number of bytes to be written is greater than " +
					"the specified ContentLength.");
				Operation.CompleteRequestWritten (this, throwMe);
				throw throwMe;
			}
		}

		internal async Task Initialize (CancellationToken cancellationToken)
		{
			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			WebConnection.Debug ($"{ME} INIT: {Operation.WriteBuffer != null}");

			if (Operation.WriteBuffer != null) {
				if (Operation.IsNtlmChallenge)
					Request.InternalContentLength = 0;
				else
					Request.InternalContentLength = Operation.WriteBuffer.Size;
			}

			await SetHeadersAsync (false, cancellationToken).ConfigureAwait (false);

			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			if (Operation.WriteBuffer != null && !Operation.IsNtlmChallenge) {
				await WriteRequestAsync (cancellationToken);
				Close ();
			}
		}

		async Task SetHeadersAsync (bool setInternalLength, CancellationToken cancellationToken)
		{
			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			if (headersSent)
				return;

			string method = Request.Method;
			bool no_writestream = (method == "GET" || method == "CONNECT" || method == "HEAD" ||
					      method == "TRACE");
			bool webdav = (method == "PROPFIND" || method == "PROPPATCH" || method == "MKCOL" ||
				      method == "COPY" || method == "MOVE" || method == "LOCK" ||
				      method == "UNLOCK");

			if (Operation.IsNtlmChallenge)
				no_writestream = true;

			if (setInternalLength && !no_writestream && HasWriteBuffer)
				Request.InternalContentLength = WriteBufferLength;

			bool has_content = !no_writestream && (!HasWriteBuffer || Request.ContentLength > -1);
			if (!(sendChunked || has_content || no_writestream || webdav))
				return;

			headersSent = true;
			headers = Request.GetRequestHeaders ();

			WebConnection.Debug ($"{ME} SET HEADERS: {Request.ContentLength}");

			try {
				await InnerStream.WriteAsync (headers, 0, headers.Length, cancellationToken).ConfigureAwait (false);
				var cl = Request.ContentLength;
				if (!sendChunked && cl == 0)
					requestWritten = true;
			} catch (Exception e) {
				if (e is WebException || e is OperationCanceledException)
					throw;
				throw new WebException ("Error writing headers", WebExceptionStatus.SendFailure, WebExceptionInternalStatus.RequestFatal, e);
			}
		}

		internal async Task WriteRequestAsync (CancellationToken cancellationToken)
		{
			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			WebConnection.Debug ($"{ME} WRITE REQUEST: {requestWritten} {sendChunked} {allowBuffering} {HasWriteBuffer}");

			if (requestWritten)
				return;

			requestWritten = true;
			if (sendChunked || !allowBuffering || !HasWriteBuffer)
				return;

			BufferOffsetSize buffer = GetWriteBuffer ();
			if (buffer != null && !Operation.IsNtlmChallenge && Request.ContentLength != -1 && Request.ContentLength < buffer.Size) {
				closed = true;
				var throwMe = new WebException ("Specified Content-Length is less than the number of bytes to write", null,
					WebExceptionStatus.ServerProtocolViolation, null);
				Operation.CompleteRequestWritten (this, throwMe);
				throw throwMe;
			}

			await SetHeadersAsync (true, cancellationToken).ConfigureAwait (false);

			WebConnection.Debug ($"{ME} WRITE REQUEST #1: {buffer != null}");

			if (buffer != null && buffer.Size > 0)
				await InnerStream.WriteAsync (buffer.Buffer, 0, buffer.Size, cancellationToken).ConfigureAwait (false);

			Operation.CompleteRequestWritten (this);
		}

		async Task WriteChunkTrailer ()
		{
			using (var cts = new CancellationTokenSource ()) {
				cts.CancelAfter (WriteTimeout);
				var timeoutTask = Task.Delay (WriteTimeout);
				while (true) {
					var myWriteTcs = new TaskCompletionSource<int> ();
					var oldTcs = Interlocked.CompareExchange (ref pendingWrite, myWriteTcs, null);
					if (oldTcs == null)
						break;
					var ret = await Task.WhenAny (timeoutTask, oldTcs.Task).ConfigureAwait (false);
					if (ret == timeoutTask)
						throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
				}

				try {
					Operation.ThrowIfClosedOrDisposed (cts.Token);
					byte[] chunk = Encoding.ASCII.GetBytes ("0\r\n\r\n");
					await InnerStream.WriteAsync (chunk, 0, chunk.Length, cts.Token).ConfigureAwait (false);
				} catch {
					;
				} finally {
					pendingWrite = null;
				}
			}
		}

		internal void KillBuffer ()
		{
			writeBuffer = null;
		}

		public override Task<int> ReadAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			return Task.FromException<int> (new NotSupportedException (SR.net_writeonlystream));
		}

		protected override void Close_internal (ref bool disposed)
		{
			WebConnection.Debug ($"{ME} CLOSE: {disposed} {requestWritten} {allowBuffering}");

			if (disposed)
				return;
			disposed = true;

			if (sendChunked) {
				WriteChunkTrailer ().Wait ();
				return;
			}

			if (!allowBuffering || requestWritten) {
				Operation.CompleteRequestWritten (this);
				return;
			}

			long length = Request.ContentLength;

			if (!sendChunked && !Operation.IsNtlmChallenge && length != -1 && totalWritten != length) {
				IOException io = new IOException ("Cannot close the stream until all bytes are written");
				closed = true;
				disposed = true;
				var throwMe = new WebException ("Request was cancelled.", WebExceptionStatus.RequestCanceled, WebExceptionInternalStatus.RequestFatal, io);
				Operation.CompleteRequestWritten (this, throwMe);
				throw throwMe;
			}

			// Commented out the next line to fix xamarin bug #1512
			//WriteRequest ();
			disposed = true;
			Operation.CompleteRequestWritten (this);
		}
	}
}
