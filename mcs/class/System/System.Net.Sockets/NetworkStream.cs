//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System.IO;

namespace System.Net.Sockets
{
	public class NetworkStream : Stream, IDisposable {
		FileAccess access;
		Socket socket;
		bool owns_socket;
		bool readable, writeable;
		bool disposed = false;
		
		public NetworkStream (Socket socket)
			: this (socket, FileAccess.ReadWrite, false)
		{
		}

		public NetworkStream (Socket socket, bool owns_socket)
			: this (socket, FileAccess.ReadWrite, owns_socket)
		{
		}

		public NetworkStream (Socket socket, FileAccess access)
			: this (socket, access, false)
		{
		}
		
		public NetworkStream (Socket socket, FileAccess access, bool owns_socket)
		{
			if (socket == null)
				throw new ArgumentNullException ("socket is null");
			if (!socket.Connected)
				throw new ArgumentException ("Not connected", "socket");
			if (socket.SocketType != SocketType.Stream)
				throw new ArgumentException ("Socket is not of type Stream", "socket");
			if (!socket.Blocking)
				throw new IOException ();
			
			this.socket = socket;
			this.owns_socket = owns_socket;
			this.access = access;

			readable = CanRead;
			writeable = CanWrite;
		}

		public override bool CanRead {
			get {
				return access == FileAccess.ReadWrite || access == FileAccess.Read;
			}
		}

		public override bool CanSeek {
			get {
				// network sockets cant seek.
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return access == FileAccess.ReadWrite || access == FileAccess.Write;
			}
		}

		public virtual bool DataAvailable {
			get {
				try {
					return socket.Available > 0;
				} finally {
					CheckDisposed ();
				}
			}
		}

		public override long Length {
			get {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
			
			set {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
		}

		protected bool Readable {
			get {
				return readable;
			}

			set {
				readable = value;
			}
		}

		protected Socket Socket {
			get {
				return socket;
			}
		}

		protected bool Writeable {
			get {
				return writeable;
			}

			set {
				writeable = value;
			}
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
							AsyncCallback callback, object state)
		{
			try {
				IAsyncResult retval;

				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");
				int len = buffer.Length;
				if(offset<0 || offset>=len) {
					throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
				}
				if(offset+size<0 || offset+size>len) {
					throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");
				}

				try {
					retval = socket.BeginReceive (buffer, offset, size, 0, callback, state);
				} catch {
					throw new IOException ("BeginReceive failure");
				}

				return retval;
			} finally {		
				CheckDisposed ();				
			}
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback callback, object state)
		{
			try {
				IAsyncResult retval;

				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");

				int len = buffer.Length;
				if(offset<0 || offset>=len) {
					throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
				}
				if(offset+size<0 || offset+size>len) {
					throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");
				}

				try {
					retval = socket.BeginSend (buffer, offset, size, 0, callback, state);
				} catch {
					throw new IOException ("BeginWrite failure");
				}

				return retval;
			} finally {
				CheckDisposed ();
			}
		}

		~NetworkStream ()
		{
			Dispose (false);
		}
		
		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed) 
				return;
			disposed = true;
			
			if (owns_socket) {
				Socket s = socket;
				if (s != null)
					s.Close ();
			}
			socket = null;
		}

		public override int EndRead (IAsyncResult ar)
		{
			try {
				int res;

				if (ar == null)
					throw new ArgumentNullException ("async result is null");

				try {
					res = socket.EndReceive (ar);
				} catch {
					throw new IOException ("EndRead failure");
				}
				return res;
			} finally {
				CheckDisposed ();
			}
		}

		public override void EndWrite (IAsyncResult ar)
		{
			try {			
				if (ar == null)
					throw new ArgumentNullException ("async result is null");

				try {
					socket.EndSend (ar);
				} catch {
					throw new IOException ("EndWrite failure");
				}
			} finally {
				CheckDisposed ();
			}
		}

		public override void Flush ()
		{
			// network streams are non-buffered, this is a no-op
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public override int Read (byte [] buffer, int offset, int size)
		{
			try {
				int res;

				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");
				if(offset<0 || offset>=buffer.Length) {
					throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
				}
				if(offset+size < 0 || offset+size>buffer.Length) {
					throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");
				}

				try {
					res = socket.Receive (buffer, offset, size, 0);
				} catch {
					throw new IOException ("Read failure");
				}
				return res;
			} finally { 
				CheckDisposed ();
			}
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			// NetworkStream objects do not support seeking.
			
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			// NetworkStream objects do not support SetLength
			
			throw new NotSupportedException ();
		}

		public override void Write (byte [] buffer, int offset, int size)
		{
			try {
				if (buffer == null)
					throw new ArgumentNullException ("buffer is null");
				if(offset<0 || offset>=buffer.Length) {
					throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
				}
				if(offset+size<0 || offset+size>buffer.Length) {
					throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");
				}

				try {
					socket.Send (buffer, offset, size, 0);
				} catch {
					throw new IOException ("Write failure"); 
				}
			} finally {
				CheckDisposed ();
			}
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
	     
	}
}
