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
				CheckDisposed ();
				return socket.Available > 0;
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
			CheckDisposed ();				
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
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback callback, object state)
		{
			CheckDisposed ();
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
			CheckDisposed ();
			int res;

			if (ar == null)
				throw new ArgumentNullException ("async result is null");

			try {
				res = socket.EndReceive (ar);
			} catch (Exception e) {
				throw new IOException ("EndRead failure", e);
			}
			return res;
		}

		public override void EndWrite (IAsyncResult ar)
		{
			CheckDisposed ();
			if (ar == null)
				throw new ArgumentNullException ("async result is null");

			try {
				socket.EndSend (ar);
			} catch (Exception e) {
				throw new IOException ("EndWrite failure", e);
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
			CheckDisposed ();
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
			} catch (Exception e) {
				throw new IOException ("Read failure", e);
			}

			return res;
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
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || offset > buffer.Length)
				throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");

			if (size < 0 || size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");

			try {
				socket.Send (buffer, offset, size, 0);
			} catch (Exception e) {
				throw new IOException ("Write failure", e); 
			}
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
	     
	}
}
