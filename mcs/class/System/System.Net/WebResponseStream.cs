//
// WebResponseStream.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Net.Sockets;

namespace System.Net
{
	class WebResponseStream : WebConnectionStream
	{
		BufferOffsetSize readBuffer;
		long contentLength;
		long totalRead;
		bool nextReadCalled;
		int stream_length; // -1 when CL not present
		WebCompletionSource pendingRead;
		object locker = new object ();
		int nestedRead;
		bool read_eof;

		public WebRequestStream RequestStream {
			get;
		}

		public WebHeaderCollection Headers {
			get;
			private set;
		}

		public HttpStatusCode StatusCode {
			get;
			private set;
		}

		public string StatusDescription {
			get;
			private set;
		}

		public Version Version {
			get;
			private set;
		}

		public bool KeepAlive {
			get;
			private set;
		}

		internal readonly string ME;

		public WebResponseStream (WebRequestStream request)
			: base (request.Connection, request.Operation, request.InnerStream)
		{
			RequestStream = request;
			request.InnerStream.ReadTimeout = ReadTimeout;

#if MONO_WEB_DEBUG
			ME = $"WRP(Cnc={Connection.ID}, Op={Operation.ID})";
#endif
		}

		public override long Length {
			get {
				return stream_length;
			}
		}

		public override bool CanRead => true;

		public override bool CanWrite => false;

		protected bool ChunkedRead {
			get;
			private set;
		}

		protected MonoChunkStream ChunkStream {
			get;
			private set;
		}

		public override async Task<int> ReadAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} READ ASYNC");

			cancellationToken.ThrowIfCancellationRequested ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			if (Interlocked.CompareExchange (ref nestedRead, 1, 0) != 0)
				throw new InvalidOperationException ("Invalid nested call.");

			var completion = new WebCompletionSource ();
			while (!cancellationToken.IsCancellationRequested) {
				/*
				 * 'currentRead' is set by ReadAllAsync().
				 */
				var oldCompletion = Interlocked.CompareExchange (ref pendingRead, completion, null);
				WebConnection.Debug ($"{ME} READ ASYNC #1: {oldCompletion != null}");
				if (oldCompletion == null)
					break;
				await oldCompletion.WaitForCompletion (true).ConfigureAwait (false);
			}

			WebConnection.Debug ($"{ME} READ ASYNC #2: {totalRead} {contentLength}");

			int oldBytes = 0, nbytes = 0;
			Exception throwMe = null;

			try {
				// FIXME: NetworkStream.ReadAsync() does not support cancellation.
				(oldBytes, nbytes) = await HttpWebRequest.RunWithTimeout (
					ct => ProcessRead (buffer, offset, size, ct),
					ReadTimeout, () => {
						Operation.Abort ();
						InnerStream.Dispose ();
					}).ConfigureAwait (false);
			} catch (Exception e) {
				throwMe = GetReadException (WebExceptionStatus.ReceiveFailure, e, "ReadAsync");
			}

			WebConnection.Debug ($"{ME} READ ASYNC #3: {totalRead} {contentLength} - {oldBytes} {nbytes} {throwMe?.Message}");

			if (throwMe != null) {
				lock (locker) {
					completion.TrySetException (throwMe);
					pendingRead = null;
					nestedRead = 0;
				}

				closed = true;
				Operation.CompleteResponseRead (false, throwMe);
				throw throwMe;
			}

			lock (locker) {
				pendingRead.TrySetCompleted ();
				pendingRead = null;
				nestedRead = 0;
			}

			if (totalRead >= contentLength && !nextReadCalled) {
				WebConnection.Debug ($"{ME} READ ASYNC - READ COMPLETE: {oldBytes} {nbytes} - {totalRead} {contentLength} {nextReadCalled}");
				if (!nextReadCalled) {
					nextReadCalled = true;
					Operation.CompleteResponseRead (true);
				}
			}

			return oldBytes + nbytes;
		}

		async Task<(int, int)> ProcessRead (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} PROCESS READ: {totalRead} {contentLength}");

			cancellationToken.ThrowIfCancellationRequested ();
			if (totalRead >= contentLength) {
				read_eof = true;
				contentLength = totalRead;
				return (0, 0);
			}

			int oldBytes = 0;
			int remaining = readBuffer?.Size ?? 0;
			if (remaining > 0) {
				int copy = (remaining > size) ? size : remaining;
				Buffer.BlockCopy (readBuffer.Buffer, readBuffer.Offset, buffer, offset, copy);
				readBuffer.Offset += copy;
				readBuffer.Size -= copy;
				offset += copy;
				size -= copy;
				totalRead += copy;
				if (totalRead >= contentLength) {
					contentLength = totalRead;
					read_eof = true;
				}
				if (size == 0 || totalRead >= contentLength)
					return (0, copy);
				oldBytes = copy;
			}

			if (contentLength != Int64.MaxValue && contentLength - totalRead < size)
				size = (int)(contentLength - totalRead);

			WebConnection.Debug ($"{ME} PROCESS READ #1: {oldBytes} {size} {read_eof}");

			if (read_eof) {
				contentLength = totalRead;
				return (oldBytes, 0);
			}

			var ret = await InnerReadAsync (buffer, offset, size, cancellationToken).ConfigureAwait (false);

			if (ret <= 0) {
				read_eof = true;
				contentLength = totalRead;
				return (oldBytes, 0);
			}

			totalRead += ret;
			return (oldBytes, ret);
		}

		internal async Task<int> InnerReadAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} INNER READ ASYNC");

			Operation.ThrowIfDisposed (cancellationToken);

			int nbytes = 0;
			bool done = false;

			if (!ChunkedRead || (!ChunkStream.DataAvailable && ChunkStream.WantMore)) {
				nbytes = await InnerStream.ReadAsync (buffer, offset, size, cancellationToken).ConfigureAwait (false);
				WebConnection.Debug ($"{ME} INNER READ ASYNC #1: {nbytes} {ChunkedRead}");
				if (!ChunkedRead)
					return nbytes;
				done = nbytes == 0;
			}

			try {
				ChunkStream.WriteAndReadBack (buffer, offset, size, ref nbytes);
				WebConnection.Debug ($"{ME} INNER READ ASYNC #1: {done} {nbytes} {ChunkStream.WantMore}");
				if (!done && nbytes == 0 && ChunkStream.WantMore)
					nbytes = await EnsureReadAsync (buffer, offset, size, cancellationToken).ConfigureAwait (false);
			} catch (Exception e) {
				if (e is WebException || e is OperationCanceledException)
					throw;
				throw new WebException ("Invalid chunked data.", e, WebExceptionStatus.ServerProtocolViolation, null);
			}

			if ((done || nbytes == 0) && ChunkStream.ChunkLeft != 0) {
				// HandleError (WebExceptionStatus.ReceiveFailure, null, "chunked EndRead");
				throw new WebException ("Read error", null, WebExceptionStatus.ReceiveFailure, null);
			}

			return nbytes;
		}

		async Task<int> EnsureReadAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			byte[] morebytes = null;
			int nbytes = 0;
			while (nbytes == 0 && ChunkStream.WantMore && !cancellationToken.IsCancellationRequested) {
				int localsize = ChunkStream.ChunkLeft;
				if (localsize <= 0) // not read chunk size yet
					localsize = 1024;
				else if (localsize > 16384)
					localsize = 16384;

				if (morebytes == null || morebytes.Length < localsize)
					morebytes = new byte[localsize];

				int nread = await InnerStream.ReadAsync (morebytes, 0, localsize, cancellationToken).ConfigureAwait (false);
				if (nread <= 0)
					return 0; // Error

				ChunkStream.Write (morebytes, 0, nread);
				nbytes += ChunkStream.Read (buffer, offset + nbytes, size - nbytes);
			}

			return nbytes;
		}

		bool CheckAuthHeader (string headerName)
		{
			var authHeader = Headers[headerName];
			return (authHeader != null && authHeader.IndexOf ("NTLM", StringComparison.Ordinal) != -1);
		}

		bool IsNtlmAuth ()
		{
			bool isProxy = (Request.Proxy != null && !Request.Proxy.IsBypassed (Request.Address));
			if (isProxy && CheckAuthHeader ("Proxy-Authenticate"))
				return true;
			return CheckAuthHeader ("WWW-Authenticate");
		}

		bool ExpectContent {
			get {
				if (Request.Method == "HEAD")
					return false;
				return ((int)StatusCode >= 200 && (int)StatusCode != 204 && (int)StatusCode != 304);
			}
		}

		async Task Initialize (BufferOffsetSize buffer, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} INIT: status={(int)StatusCode} bos={buffer.Offset}/{buffer.Size}");

			string contentType = Headers["Transfer-Encoding"];
			bool chunkedRead = (contentType != null && contentType.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);
			string clength = Headers["Content-Length"];
			if (!chunkedRead && !string.IsNullOrEmpty (clength)) {
				if (!long.TryParse (clength, out contentLength))
					contentLength = Int64.MaxValue;
			} else {
				contentLength = Int64.MaxValue;
			}

			if (Version == HttpVersion.Version11 && RequestStream.KeepAlive) {
				KeepAlive = true;
				var cncHeader = Headers[ServicePoint.UsesProxy ? "Proxy-Connection" : "Connection"];
				if (cncHeader != null) {
					cncHeader = cncHeader.ToLower ();
					KeepAlive = cncHeader.IndexOf ("keep-alive", StringComparison.Ordinal) != -1;
					if (cncHeader.IndexOf ("close", StringComparison.Ordinal) != -1)
						KeepAlive = false;
				}
			}

			// Negative numbers?
			if (!Int32.TryParse (clength, out stream_length))
				stream_length = -1;

			string me = "WebResponseStream.Initialize()";
			string tencoding = null;
			if (ExpectContent)
				tencoding = Headers["Transfer-Encoding"];

			ChunkedRead = (tencoding != null && tencoding.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);
			if (!ChunkedRead) {
				readBuffer = buffer;
				try {
					if (contentLength > 0 && readBuffer.Size >= contentLength) {
						if (!IsNtlmAuth ())
							await ReadAllAsync (false, cancellationToken).ConfigureAwait (false);
					}
				} catch (Exception e) {
					throw GetReadException (WebExceptionStatus.ReceiveFailure, e, me);
				}
			} else if (ChunkStream == null) {
				try {
					ChunkStream = new MonoChunkStream (buffer.Buffer, buffer.Offset, buffer.Offset + buffer.Size, Headers);
				} catch (Exception e) {
					throw GetReadException (WebExceptionStatus.ServerProtocolViolation, e, me);
				}
			} else {
				ChunkStream.ResetBuffer ();
				try {
					ChunkStream.Write (buffer.Buffer, buffer.Offset, buffer.Size);
				} catch (Exception e) {
					throw GetReadException (WebExceptionStatus.ServerProtocolViolation, e, me);
				}
			}

			WebConnection.Debug ($"{ME} INIT #1: - {ExpectContent} {closed} {nextReadCalled}");

			if (!ExpectContent) {
				if (!closed && !nextReadCalled) {
					if (contentLength == Int64.MaxValue)
						contentLength = 0;
					nextReadCalled = true;
				}
				Operation.CompleteResponseRead (true);
			}
		}

		internal async Task ReadAllAsync (bool resending, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} READ ALL ASYNC: resending={resending} eof={read_eof} total={totalRead} " +
			                     "length={contentLength} nextReadCalled={nextReadCalled}");
			if (read_eof || totalRead >= contentLength || nextReadCalled) {
				if (!nextReadCalled) {
					nextReadCalled = true;
					Operation.CompleteResponseRead (true);
				}
				return;
			}

			var timeoutTask = Task.Delay (ReadTimeout);
			var completion = new WebCompletionSource ();
			while (true) {
				/*
				 * 'currentRead' is set by ReadAsync().
				 */
				cancellationToken.ThrowIfCancellationRequested ();
				var oldCompletion = Interlocked.CompareExchange (ref pendingRead, completion, null);
				if (oldCompletion == null)
					break;

				// ReadAsync() is in progress.
				var oldReadTask = oldCompletion.WaitForCompletion (true);
				var anyTask = await Task.WhenAny (oldReadTask, timeoutTask).ConfigureAwait (false);
				if (anyTask == timeoutTask)
					throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
			}

			WebConnection.Debug ($"{ME} READ ALL ASYNC #1");

			cancellationToken.ThrowIfCancellationRequested ();

			try {
				if (totalRead >= contentLength)
					return;

				byte[] b = null;
				int new_size;

				if (contentLength == Int64.MaxValue && !ChunkedRead) {
					WebConnection.Debug ($"{ME} READ ALL ASYNC - NEITHER LENGTH NOR CHUNKED");
					/*
					 * This is a violation of the HTTP Spec - the server neither send a
					 * "Content-Length:" nor a "Transfer-Encoding: chunked" header.
					 *
					 * When we're redirecting or resending for NTLM, then we can simply close
					 * the connection here.
					 *
					 * However, if it's the final reply, then we need to try our best to read it.
					 */
					if (resending) {
						Close ();
						return;
					}
					KeepAlive = false;
				}

				if (contentLength == Int64.MaxValue) {
					MemoryStream ms = new MemoryStream ();
					BufferOffsetSize buffer = null;
					if (readBuffer != null && readBuffer.Size > 0) {
						ms.Write (readBuffer.Buffer, readBuffer.Offset, readBuffer.Size);
						readBuffer.Offset = 0;
						readBuffer.Size = readBuffer.Buffer.Length;
						if (readBuffer.Buffer.Length >= 8192)
							buffer = readBuffer;
					}

					if (buffer == null)
						buffer = new BufferOffsetSize (new byte[8192], false);

					int read;
					while ((read = await InnerReadAsync (buffer.Buffer, buffer.Offset, buffer.Size, cancellationToken)) != 0)
						ms.Write (buffer.Buffer, buffer.Offset, read);

					new_size = (int)ms.Length;
					contentLength = new_size;
					readBuffer = new BufferOffsetSize (ms.GetBuffer (), 0, new_size, false);
				} else {
					new_size = (int)(contentLength - totalRead);
					b = new byte[new_size];
					int readSize = 0;
					if (readBuffer != null && readBuffer.Size > 0) {
						readSize = readBuffer.Size;
						if (readSize > new_size)
							readSize = new_size;

						Buffer.BlockCopy (readBuffer.Buffer, readBuffer.Offset, b, 0, readSize);
					}

					int remaining = new_size - readSize;
					int r = -1;
					while (remaining > 0 && r != 0) {
						r = await InnerReadAsync (b, readSize, remaining, cancellationToken);
						remaining -= r;
						readSize += r;
					}
				}

				readBuffer = new BufferOffsetSize (b, 0, new_size, false);
				totalRead = 0;
				nextReadCalled = true;
				completion.TrySetCompleted ();
			} catch (Exception ex) {
				WebConnection.Debug ($"{ME} READ ALL ASYNC EX: {ex.Message}");
				completion.TrySetException (ex);
				throw;
			} finally {
				WebConnection.Debug ($"{ME} READ ALL ASYNC #2");
				pendingRead = null;
			}

			Operation.CompleteResponseRead (true);
		}

		public override Task WriteAsync (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			return Task.FromException (new NotSupportedException (SR.net_readonlystream));
		}

		protected override void Close_internal (ref bool disposed)
		{
			WebConnection.Debug ($"{ME} CLOSE: {disposed} {closed} {nextReadCalled}");
			if (!closed && !nextReadCalled) {
				nextReadCalled = true;
				if (totalRead >= contentLength) {
					disposed = true;
					Operation.CompleteResponseRead (true);
				} else {
					// If we have not read all the contents
					closed = true;
					disposed = true;
					Operation.CompleteResponseRead (false);
				}
			}
		}

		WebException GetReadException (WebExceptionStatus status, Exception error, string where)
		{
			error = GetException (error);
			string msg = $"Error getting response stream ({where}): {status}";
			if (error == null)
				return new WebException ($"Error getting response stream ({where}): {status}", status);
			if (error is WebException wexc)
				return wexc;
			if (Operation.Aborted || error is OperationCanceledException || error is ObjectDisposedException)
				return HttpWebRequest.CreateRequestAbortedException ();
			return new WebException ($"Error getting response stream ({where}): {status} {error.Message}", status,
						 WebExceptionInternalStatus.RequestFatal, error);
		}

		internal async Task InitReadAsync (CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} INIT READ ASYNC");

			var buffer = new BufferOffsetSize (new byte[4096], false);
			var state = ReadState.None;
			int position = 0;

			while (true) {
				Operation.ThrowIfClosedOrDisposed (cancellationToken);

				WebConnection.Debug ($"{ME} INIT READ ASYNC LOOP: {state} {position} - {buffer.Offset}/{buffer.Size}");

				var nread = await InnerStream.ReadAsync (
					buffer.Buffer, buffer.Offset, buffer.Size, cancellationToken).ConfigureAwait (false);

				WebConnection.Debug ($"{ME} INIT READ ASYNC LOOP #1: {state} {position} - {buffer.Offset}/{buffer.Size} - {nread}");

				if (nread == 0)
					throw GetReadException (WebExceptionStatus.ReceiveFailure, null, "ReadDoneAsync2");

				if (nread < 0)
					throw GetReadException (WebExceptionStatus.ServerProtocolViolation, null, "ReadDoneAsync3");

				buffer.Offset += nread;
				buffer.Size -= nread;

				if (state == ReadState.None) {
					try {
						var oldPos = position;
						if (!GetResponse (buffer, ref position, ref state))
							position = oldPos;
					} catch (Exception e) {
						WebConnection.Debug ($"{ME} INIT READ ASYNC FAILED: {e.Message}\n{e}");
						throw GetReadException (WebExceptionStatus.ServerProtocolViolation, e, "ReadDoneAsync4");
					}
				}

				if (state == ReadState.Aborted)
					throw GetReadException (WebExceptionStatus.RequestCanceled, null, "ReadDoneAsync5");

				if (state == ReadState.Content) {
					buffer.Size = buffer.Offset - position;
					buffer.Offset = position;
					break;
				}

				int est = nread * 2;
				if (est > buffer.Size) {
					var newBuffer = new byte [buffer.Buffer.Length + est];
					Buffer.BlockCopy (buffer.Buffer, 0, newBuffer, 0, buffer.Offset);
					buffer = new BufferOffsetSize (newBuffer, buffer.Offset, newBuffer.Length - buffer.Offset, false);
				}
				state = ReadState.None;
				position = 0;
			}

			WebConnection.Debug ($"{ME} INIT READ ASYNC LOOP DONE: {buffer.Offset} {buffer.Size}");

			try {
				Operation.ThrowIfDisposed (cancellationToken);
				await Initialize (buffer, cancellationToken).ConfigureAwait (false);
			} catch (Exception e) {
				throw GetReadException (WebExceptionStatus.ReceiveFailure, e, "ReadDoneAsync6");
			}
		}

		bool GetResponse (BufferOffsetSize buffer, ref int pos, ref ReadState state)
		{
			string line = null;
			bool lineok = false;
			bool isContinue = false;
			bool emptyFirstLine = false;
			do {
				if (state == ReadState.Aborted)
					throw GetReadException (WebExceptionStatus.RequestCanceled, null, "GetResponse");

				if (state == ReadState.None) {
					lineok = WebConnection.ReadLine (buffer.Buffer, ref pos, buffer.Offset, ref line);
					if (!lineok)
						return false;

					if (line == null) {
						emptyFirstLine = true;
						continue;
					}
					emptyFirstLine = false;
					state = ReadState.Status;

					string[] parts = line.Split (' ');
					if (parts.Length < 2)
						throw GetReadException (WebExceptionStatus.ServerProtocolViolation, null, "GetResponse");

					if (String.Compare (parts[0], "HTTP/1.1", true) == 0) {
						Version = HttpVersion.Version11;
						ServicePoint.SetVersion (HttpVersion.Version11);
					} else {
						Version = HttpVersion.Version10;
						ServicePoint.SetVersion (HttpVersion.Version10);
					}

					StatusCode = (HttpStatusCode)UInt32.Parse (parts[1]);
					if (parts.Length >= 3)
						StatusDescription = String.Join (" ", parts, 2, parts.Length - 2);
					else
						StatusDescription = string.Empty;

					if (pos >= buffer.Offset)
						return true;
				}

				emptyFirstLine = false;
				if (state == ReadState.Status) {
					state = ReadState.Headers;
					Headers = new WebHeaderCollection ();
					var headerList = new List<string> ();
					bool finished = false;
					while (!finished) {
						if (WebConnection.ReadLine (buffer.Buffer, ref pos, buffer.Offset, ref line) == false)
							break;

						if (line == null) {
							// Empty line: end of headers
							finished = true;
							continue;
						}

						if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t')) {
							int count = headerList.Count - 1;
							if (count < 0)
								break;

							string prev = headerList[count] + line;
							headerList[count] = prev;
						} else {
							headerList.Add (line);
						}
					}

					if (!finished)
						return false;

					// .NET uses ParseHeaders or ParseHeadersStrict which is much better
					foreach (string s in headerList) {

						int pos_s = s.IndexOf (':');
						if (pos_s == -1)
							throw new ArgumentException ("no colon found", "header");

						var header = s.Substring (0, pos_s);
						var value = s.Substring (pos_s + 1).Trim ();

						if (WebHeaderCollection.AllowMultiValues (header)) {
							Headers.AddInternal (header, value);
						} else {
							Headers.SetInternal (header, value);
						}
					}

					if (StatusCode == HttpStatusCode.Continue) {
						ServicePoint.SendContinue = true;
						if (pos >= buffer.Offset)
							return true;

						if (Request.ExpectContinue) {
							Request.DoContinueDelegate ((int)StatusCode, Headers);
							// Prevent double calls when getting the
							// headers in several packets.
							Request.ExpectContinue = false;
						}

						state = ReadState.None;
						isContinue = true;
					} else {
						state = ReadState.Content;
						return true;
					}
				}
			} while (emptyFirstLine || isContinue);

			throw GetReadException (WebExceptionStatus.ServerProtocolViolation, null, "GetResponse");
		}


	}
}
