/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzmán Álvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Mono.Security.Protocol.Tls
{
	public class TlsNetworkStream : Stream, IDisposable
	{
		#region PROPERTIES

		private TlsSocket	socket;
		private bool		ownsSocket;
		private bool		canRead;
		private bool		canWrite;
		private bool		disposed;

		#endregion

		#region PROPERTIES

		public override bool CanRead
		{
			get { return canRead; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return canWrite; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }

		}

		public bool DataAvailable
		{
			get
			{
				if (socket == null)
				{
					throw new IOException();
				}

				if (this.socket.Session.IsSecure)
				{
					if ((this.socket.InputBuffer.Length - this.socket.InputBuffer.Position) > 0)
					{
						return true;
					}
				}

				// If there are bytes in the socket buffer return true
				// otherwise false
				return this.socket.Available != 0;
			}
		}

		#endregion

		#region DESTRUCTOR

		~TlsNetworkStream()
		{
			this.Dispose(false);
		}

		#endregion

        #region IDISPOSABLE

		void IDisposable.Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (this.socket != null)
					{
						if (this.ownsSocket)
						{
							try
							{
								this.socket.Shutdown(SocketShutdown.Both);
							}
							catch{}
							finally
							{
								this.socket.Close();
							}
						}
					}
					this.ownsSocket	= false;
					this.canRead	= false;
					this.canWrite	= false;
					this.socket		= null;
				}

				disposed = true;
			}
		}

		#endregion

		#region PROTECTED_PROPERTIES

		protected bool Readable 
		{
			get { return this.canRead; }
			set { this.canRead = value;}
		}

		protected TlsSocket Socket 
		{
			get { return socket; }
		}

		protected bool Writeable 
		{
			get { return this.canWrite; }
			set { this.canWrite = value; }
		}

		#endregion

		#region CONSTRUCTORS
		
		public TlsNetworkStream(TlsSocket socket) 
			: this(socket, FileAccess.ReadWrite, false)
		{
		}
		
		public TlsNetworkStream(TlsSocket socket, bool ownsSocket) 
			: this(socket, FileAccess.ReadWrite, ownsSocket)
		{
		}
		
		public TlsNetworkStream(TlsSocket socket, FileAccess access)
			: this(socket, FileAccess.ReadWrite, false)
		{
		}
		
		public TlsNetworkStream(TlsSocket socket, FileAccess access, bool ownsSocket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket is a null reference.");
			}
			if (!socket.Blocking)
			{
				throw new IOException("socket is in a nonblocking state.");
			}
			if (socket.SocketType != SocketType.Stream)
			{
				throw new IOException("The SocketType property of socket is not SocketType.Stream.");
			}
			if (!socket.Connected)
			{
				throw new IOException("socket is not connected.");
			}

			this.socket		= socket;
			this.ownsSocket	= ownsSocket;
			switch (access)
			{
				case FileAccess.Read:
					this.canRead	= true;
					break;

				case FileAccess.ReadWrite:
					this.canRead	= true;
					this.canWrite	= true;
					break;

				case FileAccess.Write:
					this.canWrite	= true;
					break;
			}
		}

		#endregion

		#region METHODS

		public override IAsyncResult BeginRead(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginWrite(
			byte[] buffer,
			int offset,
			int count,
			AsyncCallback callback,
			object state)
		{
			throw new NotSupportedException();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			throw new NotSupportedException();
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			throw new NotSupportedException();
		}

		public override void Close()
		{
			((IDisposable)this).Dispose();
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int size)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size is less than 0.");
			}
			if (size > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("size is less than the length of buffer minus the value of the offset parameter.");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("The NetworkStream is closed.");
			}
			if (!socket.Connected)
			{
				throw new IOException("The underlying Socket is closed.");
			}
	
			try
			{
				return socket.Receive(buffer, offset, size, SocketFlags.None);
			}
			catch (TlsException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during read.", ex);
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException("size is less than 0.");
			}
			if (size > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("size is less than the length of buffer minus the value of the offset parameter.");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("The NetworkStream is closed.");
			}
			if (!socket.Connected)
			{
				throw new IOException("The underlying Socket is closed.");
			}

			try
			{
				socket.Send(buffer, offset, size, SocketFlags.None);
			}
			catch (TlsException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during Write.", ex);
			}
		}

		#endregion
	}
}
