//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
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
using System.Runtime.InteropServices;

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
			if(offset<0 || offset>len) {
				throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
			}
			if(size<0 || offset+size>len) {
				throw new ArgumentOutOfRangeException("offset+size exceeds the size of buffer");
			}

			try {
				retval = socket.BeginReceive (buffer, offset, size, 0, callback, state);
			} catch (Exception e) {
				throw new IOException ("BeginReceive failure", e);
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
			if(offset<0 || offset>len) {
				throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
			}
			if(size<0 || offset+size>len) {
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

		public override int Read ([In,Out] byte [] buffer, int offset, int size)
		{
			CheckDisposed ();
			int res;

			if (buffer == null)
				throw new ArgumentNullException ("buffer is null");
			if(offset<0 || offset>buffer.Length) {
				throw new ArgumentOutOfRangeException("offset exceeds the size of buffer");
			}
			if(size < 0 || offset+size>buffer.Length) {
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
				int count = 0;
				while (size - count > 0) {
					count += socket.Send (buffer, offset + count, size - count, 0);
				}
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
