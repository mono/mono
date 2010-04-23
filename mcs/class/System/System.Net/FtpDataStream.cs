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
using System.Net;

#if NET_2_0

namespace System.Net
{
	class FtpDataStream : Stream, IDisposable
	{
		FtpWebRequest request;
		Stream networkStream;
		bool disposed;
		bool isRead;
		int totalRead;

		internal FtpDataStream (FtpWebRequest request, Stream stream, bool isRead)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			this.request = request;
			this.networkStream = stream;
			this.isRead = isRead;
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

		internal Stream NetworkStream {
			get {
				CheckDisposed ();
				return networkStream;
			}
		}

		public override void Close ()
		{
			Dispose (true);
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
			int nbytes = 0;
			request.CheckIfAborted ();

			try {
				// Probably it would be better to have the socket here
				nbytes = networkStream.Read (buffer, offset, size);
			} catch (IOException) {
				throw new ProtocolViolationException ("Server commited a protocol violation");
			}

			totalRead += nbytes;
			if (nbytes == 0) {
				networkStream.Close ();
				request.SetTransferCompleted ();
			}

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
			request.CheckIfAborted ();
			IAsyncResult ar = BeginRead (buffer, offset, size, null, null);
			if (!ar.IsCompleted && !ar.AsyncWaitHandle.WaitOne (request.ReadWriteTimeout, false))
				throw new WebException ("Read timed out.", WebExceptionStatus.Timeout);
			return EndRead (ar);
		}


		delegate void WriteDelegate (byte [] buffer, int offset, int size);
		
		void WriteInternal (byte [] buffer, int offset, int size)
		{
			request.CheckIfAborted ();
			
			try {
				networkStream.Write (buffer, offset, size);
			} catch (IOException) {
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
			request.CheckIfAborted ();
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
			if (networkStream != null)  {
				try {
 					networkStream.Close ();
				} catch {
				}

				request.SetTransferCompleted ();
				request = null;
				networkStream = null;
			}
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		delegate int ReadDelegate (byte [] buffer, int offset, int size);
	}
}

#endif

