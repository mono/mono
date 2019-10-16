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
		WebCompletionSource pendingWrite;
		long totalWritten;
		byte[] headers;
		bool headersSent;
		int completeRequestWritten;
		int chunkTrailerWritten;

		internal readonly string ME;

		public WebRequestStream (WebConnection connection, WebOperation operation,
					 Stream stream, WebConnectionTunnel tunnel)
			: base (connection, operation)
		{
			InnerStream = stream;

			allowBuffering = operation.Request.InternalAllowBuffering;
			sendChunked = operation.Request.SendChunked && operation.WriteBuffer == null;
			if (!sendChunked && allowBuffering && operation.WriteBuffer == null)
				writeBuffer = new MemoryStream ();

			KeepAlive = Request.KeepAlive;
			if (tunnel?.ProxyVersion != null && tunnel?.ProxyVersion != HttpVersion.Version11)
				KeepAlive = false;

#if MONO_WEB_DEBUG
			ME = $"WRQ(Cnc={Connection.ID}, Op={Operation.ID})";
#endif
		}

		internal Stream InnerStream {
			get;
		}

		public bool KeepAlive {
			get;
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

		async Task FinishWriting (CancellationToken cancellationToken)
		{
			if (Interlocked.CompareExchange (ref completeRequestWritten, 1, 0) != 0)
				return;

			WebConnection.Debug ($"{ME} FINISH WRITING: {sendChunked}");
			try {
				Operation.ThrowIfClosedOrDisposed (cancellationToken);
				if (sendChunked)
					await WriteChunkTrailer_inner (cancellationToken).ConfigureAwait (false);
			} catch (Exception ex) {
				Operation.CompleteRequestWritten (this, ex);
				throw;
			} finally {
				WebConnection.Debug ($"{ME} FINISH WRITING DONE");
			}

			Operation.CompleteRequestWritten (this);
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			WebConnection.Debug ($"{ME} WRITE ASYNC: {buffer.Length}/{offset}/{count}");

			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled (cancellationToken);

			Operation.ThrowIfClosedOrDisposed (cancellationToken);

			if (Operation.WriteBuffer != null)
				throw new InvalidOperationException ();

			var completion = new WebCompletionSource ();
			if (Interlocked.CompareExchange (ref pendingWrite, completion, null) != null)
				throw new InvalidOperationException (SR.GetString (SR.net_repcall));

			return WriteAsyncInner (buffer, offset, count, completion, cancellationToken);
		}

		async Task WriteAsyncInner (byte[] buffer, int offset, int size,
		                            WebCompletionSource completion,
		                            CancellationToken cancellationToken)
		{
			try {
				await ProcessWrite (buffer, offset, size, cancellationToken).ConfigureAwait (false);

				WebConnection.Debug ($"{ME} WRITE ASYNC #1: {allowBuffering} {sendChunked} {Request.ContentLength} {totalWritten}");

				if (Request.ContentLength > 0 && totalWritten == Request.ContentLength)
					await FinishWriting (cancellationToken);

				pendingWrite = null;
				completion.TrySetCompleted ();
			} catch (Exception ex) {
				KillBuffer ();
				closed = true;

				WebConnection.Debug ($"{ME} WRITE ASYNC EX: {ex.Message}");

				var oldError = Operation.CheckDisposed (cancellationToken);
				if (oldError != null)
					ex = oldError.SourceException;
				else if (ex is SocketException)
					ex = new IOException ("Error writing request", ex);

				Operation.CompleteRequestWritten (this, ex);

				pendingWrite = null;
				completion.TrySetException (ex);

				if (oldError != null)
					oldError.Throw ();
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
				}

				totalWritten += size;

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
				} else {
					totalWritten += size;
				}
			}

			await InnerStream.WriteAsync (buffer, offset, size, cancellationToken).ConfigureAwait (false);
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
			if (sendChunked || !HasWriteBuffer)
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

			Operation.ThrowIfClosedOrDisposed (cancellationToken);
			if (buffer != null && buffer.Size > 0)
				await InnerStream.WriteAsync (buffer.Buffer, 0, buffer.Size, cancellationToken);

			await FinishWriting (cancellationToken);
		}

		async Task WriteChunkTrailer_inner (CancellationToken cancellationToken)
		{
			if (Interlocked.CompareExchange (ref chunkTrailerWritten, 1, 0) != 0)
				return;
			Operation.ThrowIfClosedOrDisposed (cancellationToken);
			byte[] chunk = Encoding.ASCII.GetBytes ("0\r\n\r\n");
			await InnerStream.WriteAsync (chunk, 0, chunk.Length, cancellationToken).ConfigureAwait (false);
		}

		async Task WriteChunkTrailer ()
		{
			var cts = new CancellationTokenSource ();
			try {
				cts.CancelAfter (WriteTimeout);
				var timeoutTask = Task.Delay (WriteTimeout, cts.Token);
				while (true) {
					var completion = new WebCompletionSource ();
					var oldCompletion = Interlocked.CompareExchange (ref pendingWrite, completion, null);
					if (oldCompletion == null)
						break;
					var oldWriteTask = oldCompletion.WaitForCompletion ();
					var ret = await Task.WhenAny (timeoutTask, oldWriteTask).ConfigureAwait (false);
					if (ret == timeoutTask)
						throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
				}

				await WriteChunkTrailer_inner (cts.Token).ConfigureAwait (false);
			} catch {
				// Intentionally eating exceptions.
			} finally {
				pendingWrite = null;
				cts.Cancel ();
				cts.Dispose ();
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

		protected override bool TryReadFromBufferedContent (byte[] buffer, int offset, int count, out int result) => throw new InvalidOperationException ();

		protected override void Close_internal (ref bool disposed)
		{
			WebConnection.Debug ($"{ME} CLOSE: {disposed} {requestWritten} {allowBuffering}");

			if (disposed)
				return;
			disposed = true;

			if (sendChunked) {
				// Don't use FinishWriting() here, we need to block on 'pendingWrite' to ensure that
				// any pending WriteAsync() has been completed.
				//
				// FIXME: I belive .NET simply aborts if you call Close() or Dispose() while writing,
				//        need to check this.  2017/07/17 Martin.
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
