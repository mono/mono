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
using System.IO.Compression;
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
		Stream innerStreamWrapper;
		MonoChunkStream innerChunkStream;
		bool nextReadCalled;
		bool bufferedEntireContent;
		TaskCompletionSource<object> readTcs;
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

#if MONO_WEB_DEBUG
			ME = $"WRP(Cnc={Connection.ID}, Op={Operation.ID})";
#endif
		}

		public override bool CanRead => true;

		public override bool CanWrite => false;

		bool ChunkedRead {
			get; set;
		}

		public override async Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} READ ASYNC");

			cancellationToken.ThrowIfCancellationRequested ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			if (Interlocked.CompareExchange (ref nestedRead, 1, 0) != 0)
				throw new InvalidOperationException ("Invalid nested call.");

			var myReadTcs = new TaskCompletionSource<object> ();
			while (!cancellationToken.IsCancellationRequested) {
				/*
				 * 'readTcs' is set by ReadAllAsync().
				 */
				var oldReadTcs = Interlocked.CompareExchange (ref readTcs, myReadTcs, null);
				WebConnection.Debug ($"{ME} READ ASYNC #1: {oldReadTcs != null}");
				if (oldReadTcs == null)
					break;
				await oldReadTcs.Task.ConfigureAwait (false);
			}

			WebConnection.Debug ($"{ME} READ ASYNC #2");

			int nbytes = 0;
			Exception throwMe = null;

			try {
				// FIXME: NetworkStream.ReadAsync() does not support cancellation.
				nbytes = await ProcessRead (buffer, offset, count, cancellationToken).ConfigureAwait (false);
			} catch (Exception e) {
				throwMe = GetReadException (WebExceptionStatus.ReceiveFailure, e, "ReadAsync");
			}

			WebConnection.Debug ($"{ME} READ ASYNC #3: {nbytes} {throwMe?.Message}");

			if (throwMe != null) {
				lock (locker) {
					myReadTcs.TrySetException (throwMe);
					readTcs = null;
					nestedRead = 0;
				}

				closed = true;
				Operation.CompleteResponseRead (false, throwMe);
				throw throwMe;
			}

			if (nbytes <= 0 && !read_eof) {
				read_eof = true;

				if (!nextReadCalled) {
					WebConnection.Debug ($"{ME} READ ASYNC - READ COMPLETE: {nbytes} - {nextReadCalled}");
					if (!nextReadCalled) {
						nextReadCalled = true;
						Operation.CompleteResponseRead (true);
					}
				}
			}

			lock (locker) {
				readTcs.TrySetResult (null);
				readTcs = null;
				nestedRead = 0;
			}

			return nbytes;
		}

		Task<int> ProcessRead (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			if (read_eof)
				return Task.FromResult (0);
			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<int> (cancellationToken);
			return HttpWebRequest.RunWithTimeout (
				async ct => {
					var ret = await ProcessReadInner (buffer, offset, size, cancellationToken).ConfigureAwait (false);
					if (ret != 0)
						return ret;

					await ProcessReadFinished (cancellationToken).ConfigureAwait (false);
					return ret;
				},
				ReadTimeout,
				() => {
					Operation.Abort ();
					innerStreamWrapper.Dispose ();
				}, cancellationToken);
		}

		async Task ProcessReadFinished (CancellationToken cancellationToken)
		{
			if (innerChunkStream == null || innerChunkStream == innerStreamWrapper)
				return;

			/*
			 * We only get here when using GZip/Deflate decompression with
			 * chunked encoding.  Since the GZipStream/DeflateStream knows
			 * about the size of the content, it may not have read the
			 * chunk trailer.
			 */

			WebConnection.Debug ($"{ME} INNER READ - READ CHUNK TRAILER");
			await innerChunkStream.ReadChunkTrailer (cancellationToken).ConfigureAwait (false);
			WebConnection.Debug ($"{ME} INNER READ - READ CHUNK TRAILER DONE");
		}

		async Task<int> ProcessReadInner (byte[] buffer, int offset, int size, CancellationToken cancellationToken)
		{
			Operation.ThrowIfDisposed (cancellationToken);

			WebConnection.Debug ($"{ME} INNER READ ASYNC: stream={innerStreamWrapper}");

			try {
				var ret = await innerStreamWrapper.ReadAsync (
					buffer, offset, size, cancellationToken).ConfigureAwait (false);
				WebConnection.Debug ($"{ME} INNER READ ASYNC DONE: {ret}");
				return ret;
			} catch (Exception ex) {
				WebConnection.Debug ($"{ME} INNER READ ASYNC EX: {ex.Message}");
				throw;
			}
		}

		bool CheckAuthHeader (string headerName)
		{
			var authHeader = Headers[headerName];
			return (authHeader != null && authHeader.IndexOf ("NTLM", StringComparison.Ordinal) != -1);
		}

		bool ExpectContent {
			get {
				if (Request.Method == "HEAD")
					return false;
				return ((int)StatusCode >= 200 && (int)StatusCode != 204 && (int)StatusCode != 304);
			}
		}

		void Initialize (BufferOffsetSize buffer)
		{
			WebConnection.Debug ($"{ME} INIT: status={(int)StatusCode} bos={buffer.Offset}/{buffer.Size}");

			long contentLength;
			string contentType = Headers["Transfer-Encoding"];
			bool chunkedRead = (contentType != null && contentType.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);
			string clength = Headers["Content-Length"];
			if (!chunkedRead && !string.IsNullOrEmpty (clength)) {
				if (!long.TryParse (clength, out contentLength))
					contentLength = Int64.MaxValue;
			} else {
				contentLength = Int64.MaxValue;
			}

			string tencoding = null;
			if (ExpectContent)
				tencoding = Headers["Transfer-Encoding"];

			ChunkedRead = (tencoding != null && tencoding.IndexOf ("chunked", StringComparison.OrdinalIgnoreCase) != -1);

			if (Version == HttpVersion.Version11 && RequestStream.KeepAlive) {
				KeepAlive = true;
				var cncHeader = Headers[ServicePoint.UsesProxy ? "Proxy-Connection" : "Connection"];
				if (cncHeader != null) {
					cncHeader = cncHeader.ToLower ();
					KeepAlive = cncHeader.IndexOf ("keep-alive", StringComparison.Ordinal) != -1;
					if (cncHeader.IndexOf ("close", StringComparison.Ordinal) != -1)
						KeepAlive = false;
				}

				if (!ChunkedRead && contentLength == Int64.MaxValue) {
					/*
					 * This is a violation of the HTTP Spec - the server neither send a
					 * "Content-Length:" nor a "Transfer-Encoding: chunked" header.
					 * The only way to recover from this is to keep reading until the
					 * remote closes the connection, so we can't reuse it.
					 */
					KeepAlive = false;
				}
			}

			/*
			 * Inner layer:
			 * We may have read a few extra bytes while parsing the headers, these will be
			 * passed to us in the @buffer parameter and we need to read these before
			 * reading from the `InnerStream`.
			 */
			if (!ExpectContent || (!ChunkedRead && buffer.Size >= contentLength)) {
				bufferedEntireContent = true;
				innerStreamWrapper = new BufferedReadStream (Operation, null, buffer);
			} else if (buffer.Size > 0) {
				innerStreamWrapper = new BufferedReadStream (Operation, InnerStream, buffer);
			} else {
				innerStreamWrapper = InnerStream;
			}

			/*
			 * Intermediate layer:
			 * - Wrap with MonoChunkStream when using chunked encoding.
			 * - Otherwise, we should have a Content-Length, wrap with
			 *   FixedSizeReadStream to read exactly that many bytes.
			 */
			if (ChunkedRead) {
				innerStreamWrapper = innerChunkStream = new MonoChunkStream (
					Operation, innerStreamWrapper, Headers);
			} else if (!bufferedEntireContent && contentLength != Int64.MaxValue) {
				innerStreamWrapper = new FixedSizeReadStream (Operation, innerStreamWrapper, contentLength);
			}

			/*
			 * Outer layer:
			 * - Decode gzip/deflate if requested.
			 */
			string content_encoding = Headers["Content-Encoding"];
			if (content_encoding == "gzip" && (Request.AutomaticDecompression & DecompressionMethods.GZip) != 0) {
				innerStreamWrapper = new GZipStream (innerStreamWrapper, CompressionMode.Decompress);
				Headers.Remove (HttpRequestHeader.ContentEncoding);
			} else if (content_encoding == "deflate" && (Request.AutomaticDecompression & DecompressionMethods.Deflate) != 0) {
				innerStreamWrapper = new DeflateStream (innerStreamWrapper, CompressionMode.Decompress);
				Headers.Remove (HttpRequestHeader.ContentEncoding);
			}

			WebConnection.Debug ($"{ME} INIT #1: - {ExpectContent} {closed} {nextReadCalled}");

			if (!ExpectContent) {
				nextReadCalled = true;
				Operation.CompleteResponseRead (true);
			}
		}

		Stream CreateStreamWrapper (BufferOffsetSize buffer)
		{
			if (buffer == null || buffer.Size == 0)
				return InnerStream;
			return new BufferedReadStream (Operation, InnerStream, buffer);
		}

		async Task<byte[]> ReadAllAsyncInner (CancellationToken cancellationToken)
		{
			var maximumSize = (long)HttpWebRequest.DefaultMaximumErrorResponseLength << 16;
			using (var ms = new MemoryStream ()) {
				while (ms.Position < maximumSize) {
					cancellationToken.ThrowIfCancellationRequested ();
					var buffer = new byte[16384];
					var ret = await ProcessRead (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
					if (ret < 0)
						throw new IOException ();
					if (ret == 0)
						break;
					ms.Write (buffer, 0, ret);
				}
				return ms.GetBuffer ();
			}
		}

		internal async Task ReadAllAsync (bool resending, CancellationToken cancellationToken)
		{
			WebConnection.Debug ($"{ME} READ ALL ASYNC: resending={resending} eof={read_eof} " +
					     "nextReadCalled={nextReadCalled}");
			if (read_eof || bufferedEntireContent || nextReadCalled) {
				if (!nextReadCalled) {
					nextReadCalled = true;
					Operation.CompleteResponseRead (true);
				}
				return;
			}

			var timeoutTask = Task.Delay (ReadTimeout);
			var myReadTcs = new TaskCompletionSource<object> ();
			while (true) {
				/*
				 * 'readTcs' is set by ReadAsync().
				 */
				cancellationToken.ThrowIfCancellationRequested ();
				var oldReadTcs = Interlocked.CompareExchange (ref readTcs, myReadTcs, null);
				if (oldReadTcs == null)
					break;

				// ReadAsync() is in progress.
				var anyTask = await Task.WhenAny (oldReadTcs.Task, timeoutTask).ConfigureAwait (false);
				if (anyTask == timeoutTask)
					throw new WebException ("The operation has timed out.", WebExceptionStatus.Timeout);
			}

			WebConnection.Debug ($"{ME} READ ALL ASYNC #1");

			try {
				cancellationToken.ThrowIfCancellationRequested ();

				/*
				 * We may have awaited on the 'readTcs', so check
				 * for eof again as ReadAsync() may have set it.
				 */
				if (read_eof || bufferedEntireContent)
					return;
				/*
				 * Simplify: if we're resending on a new connection,
				 * then we can simply close the connection here.
				 */
				if (resending && !KeepAlive) {
					Close ();
					return;
				}

				var buffer = await ReadAllAsyncInner (cancellationToken).ConfigureAwait (false);
				var bos = new BufferOffsetSize (buffer, 0, buffer.Length, false);
				innerStreamWrapper = new BufferedReadStream (Operation, null, bos);

				nextReadCalled = true;
				myReadTcs.TrySetResult (null);
			} catch (Exception ex) {
				WebConnection.Debug ($"{ME} READ ALL ASYNC EX: {ex.Message}");
				myReadTcs.TrySetException (ex);
				throw;
			} finally {
				WebConnection.Debug ($"{ME} READ ALL ASYNC #2");
				readTcs = null;
			}

			Operation.CompleteResponseRead (true);
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return Task.FromException (new NotSupportedException (SR.net_readonlystream));
		}

		protected override void Close_internal (ref bool disposed)
		{
			WebConnection.Debug ($"{ME} CLOSE: disposed={disposed} closed={closed} nextReadCalled={nextReadCalled}");
			if (!closed && !nextReadCalled) {
				nextReadCalled = true;
				WebConnection.Debug ($"{ME} CLOSE #1: read_eof={read_eof} bufferedEntireContent={bufferedEntireContent}");
				if (read_eof || bufferedEntireContent) {
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
				Initialize (buffer);
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

					if (pos >= buffer.Size)
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
