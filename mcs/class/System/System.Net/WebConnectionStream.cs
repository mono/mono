//
// System.Net.WebConnectionStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Martin Baulig <mabaul@microsoft.com>
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Net.Sockets;

namespace System.Net
{
	abstract class WebConnectionStream : Stream
	{
		protected bool closed;
		bool disposed;
		object locker = new object ();
		int read_timeout;
		int write_timeout;
		internal bool IgnoreIOErrors;

		protected WebConnectionStream (WebConnection cnc, WebOperation operation)
		{
			Connection = cnc;
			Operation = operation;
			Request = operation.Request;

			read_timeout = Request.ReadWriteTimeout;
			write_timeout = read_timeout;
		}

		internal HttpWebRequest Request {
			get;
		}

		internal WebConnection Connection {
			get;
		}

		internal WebOperation Operation {
			get;
		}

		internal ServicePoint ServicePoint => Connection.ServicePoint;

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

		protected Exception GetException (Exception e)
		{
			e = HttpWebRequest.FlattenException (e);
			if (e is WebException)
				return e;
			if (Operation.Aborted || e is OperationCanceledException || e is ObjectDisposedException)
				return HttpWebRequest.CreateRequestAbortedException ();
			return e;
		}

		protected abstract bool TryReadFromBufferedContent (byte[] buffer, int offset, int count, out int result);

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (!CanRead)
				throw new NotSupportedException (SR.net_writeonlystream);

			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			if (TryReadFromBufferedContent (buffer, offset, count, out var result))
				return result;

			Operation.ThrowIfClosedOrDisposed ();

			try
			{
				return ReadAsync (buffer, offset, count, CancellationToken.None).Result;
			} catch (Exception e) {
				throw GetException (e);
			}
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count,
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
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			var task = ReadAsync (buffer, offset, count, CancellationToken.None);
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

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count,
							 AsyncCallback cb, object state)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			if (!CanWrite)
				throw new NotSupportedException (SR.net_readonlystream);
			Operation.ThrowIfClosedOrDisposed ();

			var task = WriteAsync (buffer, offset, count, CancellationToken.None);
			return TaskToApm.Begin (task, cb, state);
		}

		public override void EndWrite (IAsyncResult r)
		{
			if (r == null)
				throw new ArgumentNullException (nameof (r));

			try {
				TaskToApm.End (r);
			} catch (Exception e) {
				throw GetException (e);
			}
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));

			int length = buffer.Length;
			if (offset < 0 || length < offset)
				throw new ArgumentOutOfRangeException (nameof (offset));
			if (count < 0 || (length - offset) < count)
				throw new ArgumentOutOfRangeException (nameof (count));

			if (!CanWrite)
				throw new NotSupportedException (SR.net_readonlystream);
			Operation.ThrowIfClosedOrDisposed ();

			try {
				WriteAsync (buffer, offset, count).Wait ();
			} catch (Exception e) {
				throw GetException (e);
			}
		}

		public override void Flush ()
		{
		}

		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			return cancellationToken.IsCancellationRequested ?
			    Task.FromCancellation (cancellationToken) :
			    Task.CompletedTask;
		}

		internal void InternalClose ()
		{
			disposed = true;
		}

		protected abstract void Close_internal (ref bool disposed);

		public override void Close ()
		{
			Close_internal (ref disposed);
		}

		public override long Seek (long a, SeekOrigin b)
		{
			throw new NotSupportedException (SR.net_noseek);
		}

		public override void SetLength (long a)
		{
			throw new NotSupportedException (SR.net_noseek);
		}

		public override bool CanSeek => false;

		public override long Length => throw new NotSupportedException (SR.net_noseek);

		public override long Position {
			get { throw new NotSupportedException (SR.net_noseek); }
			set { throw new NotSupportedException (SR.net_noseek); }
		}
	}
}

