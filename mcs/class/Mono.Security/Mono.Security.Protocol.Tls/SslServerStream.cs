/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
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
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls
{
	public class SslServerStream : Stream, IDisposable
	{
		#region Fields

		private Stream	innerStream;
		private bool	disposed;
		private bool	ownsStream;

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return this.innerStream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return this.innerStream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return this.innerStream.CanSeek; }
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

		#endregion

		#region Security Properties

		public bool CheckCertRevocationStatus 
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public CipherAlgorithmType CipherAlgorithm 
		{
			get { throw new NotSupportedException(); }
		}
		
		public int CipherStrength 
		{
			get { throw new NotSupportedException(); }
		}
		
		public X509Certificate ClientCertificate
		{
			get { throw new NotSupportedException(); }
		}

		public CertificateValidationCallback ClientCertValidationDelegate 
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public HashAlgorithmType HashAlgorithm 
		{
			get { throw new NotSupportedException(); }
		}
		
		public int HashStrength
		{
			get { throw new NotSupportedException(); }
		}
		
		public int KeyExchangeStrength 
		{
			get { throw new NotSupportedException(); }
		}
		
		public ExchangeAlgorithmType KeyExchangeAlgorithm 
		{
			get { throw new NotSupportedException(); }
		}
		
		public SecurityProtocolType SecurityProtocol 
		{
			get { throw new NotSupportedException(); }
		}

		public X509Certificate ServerCertificate 
		{
			get { throw new NotSupportedException(); }
		}

		#endregion

		#region Constructors

		public SslServerStream(Stream stream, X509Certificate serverCertificate)
		{
			throw new NotSupportedException();
		}

		public SslServerStream(
			Stream stream,
			X509Certificate serverCertificate,
			bool clientCertificateRequired,
			bool ownsStream)
		{
			throw new NotSupportedException();
		}

		public SslServerStream(
			Stream stream,
			X509Certificate serverCertificate,
			bool clientCertificateRequired,
			bool ownsStream,
			SecurityProtocolType securityProtocolType)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Finalizer

		~SslServerStream()
		{
			this.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		void IDisposable.Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.innerStream != null)
					{
						if (this.ownsStream)
						{
							// Close inner stream
							this.innerStream.Close();
						}
					}
					this.ownsStream		= false;
					this.innerStream	= null;
				}

				this.disposed = true;
			}
		}

		#endregion

		#region Methods

		public override IAsyncResult BeginRead(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback asyncCallback, 
			object asyncState)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginWrite(
			byte[] buffer, 
			int offset, 
			int count, 
			AsyncCallback asyncCallback, 
			object asyncState)
		{
			throw new NotSupportedException();
		}

		public override void Close()
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

		public override void Flush()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The NetworkStream is closed.");
			}
		}

		public int Read(byte[] buffer)
		{
			throw new NotSupportedException();
		}

		public override int Read(
			byte[] buffer, 
			int offset, 
			int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public void Write(byte[] buffer)
		{
			throw new NotSupportedException();
		}

		public override void Write(
			byte[] buffer, 
			int offset, 
			int count)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
