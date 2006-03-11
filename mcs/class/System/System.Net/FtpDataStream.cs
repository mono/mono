//
// System.Net.FtpDataStream.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;

#if NET_2_0

namespace System.Net
{
	class FtpDataStream : Stream, IDisposable
	{
		FtpWebRequest request;
		NetworkStream networkStream;
		bool disposed;
		bool isRead;
		int totalRead;
		int contentLength;

		ManualResetEvent closewh;

		internal FtpDataStream (FtpWebRequest request, Socket socket, bool isRead)
		{
			if (request == null)
				throw new ArgumentNullException ("request");
			if (socket == null)
				throw new ArgumentNullException ("socket");
			if (!socket.Connected)
				throw new ArgumentException ("socket");

			this.request = request;
			this.contentLength = socket.Available;
			this.networkStream = new NetworkStream (socket, true);
			this.isRead = isRead;

			closewh = new ManualResetEvent (false);
		}

		public override bool CanRead {
			get {
				return isRead;
			}
		}

		public override bool CanWrite {
			get {
				return !isRead;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		internal NetworkStream NetworkStream {
			get {
				CheckDisposed ();
				return networkStream;
			}
		}

		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}

		public override void Flush ()
		{
			// Do nothing.
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		int ReadInternal (byte [] buffer, int offset, int size)
		{
			int nbytes;
			try {
				// Probably it would be better to have the socket here
				nbytes = networkStream.Read (buffer, offset, size);
			} catch (IOException exc) {
				throw new ProtocolViolationException ("Server commited a protocol violation");
			}

			totalRead += nbytes;
			if (nbytes == 0)
				contentLength = totalRead;
			if (totalRead >= contentLength)
				request.SetTransferCompleted ();
			
			return nbytes;
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			CheckDisposed ();

			if (!isRead)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException ("offset+size");

			ReadDelegate del = ReadInternal;
			return del.BeginInvoke (buffer, offset, size, cb, state);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ar = asyncResult as AsyncResult;
			if (ar == null)
				throw new ArgumentException ("Invalid asyncResult", "asyncResult");
			
			ReadDelegate del = ar.AsyncDelegate as ReadDelegate;
			if (del == null)
				throw new ArgumentException ("Invalid asyncResult", "asyncResult");

			return del.EndInvoke (asyncResult);
		}

		public override int Read (byte [] buffer, int offset, int size)
		{
			IAsyncResult ar = BeginRead (buffer, offset, size, null, null);
			if (!ar.IsCompleted && !ar.AsyncWaitHandle.WaitOne (request.ReadWriteTimeout, false))
				throw new WebException ("Read timed out.", WebExceptionStatus.Timeout);

			return EndRead (ar);
		}


		delegate void WriteDelegate (byte [] buffer, int offset, int size);
		
		void WriteInternal (byte [] buffer, int offset, int size)
		{
			try {
				networkStream.Write (buffer, offset, size);
			} catch (IOException exc) {
				throw new ProtocolViolationException ();
			}
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size, AsyncCallback cb, object state)
		{
			CheckDisposed ();
			if (isRead)
				throw new NotSupportedException ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException ("offset+size");

			WriteDelegate del = WriteInternal;
			return del.BeginInvoke (buffer, offset, size, cb, state);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ar = asyncResult as AsyncResult;
			if (ar == null)
				throw new ArgumentException ("Invalid asyncResult.", "asyncResult");

			WriteDelegate del = ar.AsyncDelegate as WriteDelegate;
			if (del == null)
				throw new ArgumentException ("Invalid asyncResult.", "asyncResult");

			del.EndInvoke (asyncResult);
		}

		public override void Write (byte [] buffer, int offset, int size)
		{
			IAsyncResult ar = BeginWrite (buffer, offset, size, null, null);
			if (!ar.IsCompleted && !ar.AsyncWaitHandle.WaitOne (request.ReadWriteTimeout, false))
				throw new WebException ("Read timed out.", WebExceptionStatus.Timeout);

			EndWrite (ar);
		}

		~FtpDataStream ()
		{
			Dispose (false);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;

			disposed = true;

			networkStream.Close ();
			networkStream = null;

			closewh.Set ();
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		delegate int ReadDelegate (byte [] buffer, int offset, int size);

		// We need to know whether the stream has been closed
		internal ManualResetEvent CloseWaitHandle {
			get {
				return closewh;
			}
		}
	}
}

#endif

