//
// WebReadStream.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net
{
	abstract class WebReadStream : Stream
	{
		public WebOperation Operation {
			get;
		}

		protected Stream InnerStream {
			get;
		}

#if MONO_WEB_DEBUG
		internal string ME => $"WRS({GetType ().Name}:Op={Operation.ID})";
#else
		internal string ME => null;
#endif

		public WebReadStream (WebOperation operation, Stream innerStream)
		{
			Operation = operation;
			InnerStream = innerStream;
		}

		public override long Length => throw new NotSupportedException ();

		public override long Position {
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}

		public override bool CanSeek => false;

		public override bool CanRead => true;

		public override bool CanWrite => false;

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override void Flush ()
		{
			throw new NotSupportedException ();
		}

		protected Exception GetException (Exception e)
		{
			e = HttpWebRequest.FlattenException (e);
			if (e is WebException)
				return e;
			if (Operation.Aborted || e is OperationCanceledException || e is ObjectDisposedException)
				return HttpWebRequest.CreateRequestAbortedException ();
			return e;
		}

		public override int Read (byte[] buffer, int offset, int size)
		{
			if (!CanRead)
				throw new NotSupportedException (SR.net_writeonlystream);
			Operation.ThrowIfClosedOrDisposed ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			try {
				return ReadAsync (buffer, offset, size, CancellationToken.None).Result;
			} catch (Exception e) {
				throw GetException (e);
			}
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (!CanRead)
				throw new NotSupportedException (SR.net_writeonlystream);
			Operation.ThrowIfClosedOrDisposed ();

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			var task = ReadAsync (buffer, offset, size, CancellationToken.None);
			return TaskToApm.Begin (task, cb, state);
		}

		public override int EndRead (IAsyncResult r)
		{
			if (r == null)
				throw new ArgumentNullException (nameof (r));

			try {
				return TaskToApm.End<int> (r);
			} catch (Exception e) {
				throw GetException (e);
			}
		}

		public sealed override async Task<int> ReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken)
		{
			Operation.ThrowIfDisposed (cancellationToken);

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (size < 0 || (length - offset) < size)
				throw new ArgumentOutOfRangeException (nameof (size));

			WebConnection.Debug ($"{ME} READ");

			int nread;
			try {
				nread = await ProcessReadAsync (
					buffer, offset, size, cancellationToken).ConfigureAwait (false);
				WebConnection.Debug ($"{ME} READ DONE: nread={nread}");

				if (nread != 0)
					return nread;

				await FinishReading (cancellationToken).ConfigureAwait (false);

				return 0;
			} catch (OperationCanceledException) {
				WebConnection.Debug ($"{ME} READ CANCELED");
				throw;
			} catch (Exception ex) {
				WebConnection.Debug ($"{ME} READ ERROR: {ex.GetType ().Name}");
				throw;
			} finally {
				WebConnection.Debug ($"{ME} READ FINISHED");
			}
		}

		protected abstract Task<int> ProcessReadAsync (
			byte[] buffer, int offset, int size,
			CancellationToken cancellationToken);

		internal virtual Task FinishReading (CancellationToken cancellationToken)
		{
			Operation.ThrowIfDisposed (cancellationToken);

			WebConnection.Debug ($"{ME} FINISH READING: InnerStream={InnerStream?.GetType ()?.Name}!");

			if (InnerStream is WebReadStream innerReadStream)
				return innerReadStream.FinishReading (cancellationToken);
			return Task.CompletedTask;
		}

		bool disposed;

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;
				if (InnerStream != null)
					InnerStream.Dispose ();
			}
			base.Dispose (disposing);
		}
	}
}

