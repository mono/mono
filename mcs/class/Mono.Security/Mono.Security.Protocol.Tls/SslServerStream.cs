// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	public class SslServerStream : Stream, IDisposable
	{
		#region Internal Events
		
		internal event CertificateValidationCallback	ClientCertValidation;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Fields

		private ServerRecordProtocol	protocol;
		private BufferedStream			inputBuffer;
		private ServerContext			context;
		private Stream					innerStream;
		private bool					disposed;
		private bool					ownsStream;
		private bool					checkCertRevocationStatus;
		private object					read;
		private object					write;		

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
			get { return this.checkCertRevocationStatus ; }
			set { this.checkCertRevocationStatus = value; }
		}

		public CipherAlgorithmType CipherAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.CipherAlgorithmType;
				}

				return CipherAlgorithmType.None;
			}
		}

		public int CipherStrength 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.EffectiveKeyBits;
				}

				return 0;
			}
		}
		
		public X509Certificate ClientCertificate
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.ClientSettings.ClientCertificate;
				}

				return null;
			}
		}		
		
		public HashAlgorithmType HashAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.HashAlgorithmType; 
				}

				return HashAlgorithmType.None;
			}
		}
		
		public int HashStrength
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.HashSize * 8; 
				}

				return 0;
			}
		}
		
		public int KeyExchangeStrength 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.ServerSettings.Certificates[0].RSA.KeySize;
				}

				return 0;
			}
		}
		
		public ExchangeAlgorithmType KeyExchangeAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.ExchangeAlgorithmType; 
				}

				return ExchangeAlgorithmType.None;
			}
		}
		
		public SecurityProtocolType SecurityProtocol 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.SecurityProtocol; 
				}

				return 0;
			}
		}

		public X509Certificate ServerCertificate 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					if (this.context.ServerSettings.Certificates != null &&
						this.context.ServerSettings.Certificates.Count > 0)
					{
						return new X509Certificate(this.context.ServerSettings.Certificates[0].RawData);
					}
				}

				return null;
			}
		} 

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ClientCertValidationDelegate 
		{
			get { return this.ClientCertValidation; }
			set { this.ClientCertValidation = value; }
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate 
		{
			get { return this.PrivateKeySelection; }
			set { this.PrivateKeySelection = value; }
		}

		#endregion

		#region Constructors

		public SslServerStream(
			Stream			stream, 
			X509Certificate serverCertificate) : this(
			stream, 
			serverCertificate, 
			false, 
			false, 
			SecurityProtocolType.Default)
		{
		}

		public SslServerStream(
			Stream			stream,
			X509Certificate serverCertificate,
			bool			clientCertificateRequired,
			bool			ownsStream): this(
			stream, 
			serverCertificate, 
			clientCertificateRequired, 
			ownsStream, 
			SecurityProtocolType.Default)
		{
		}

		public SslServerStream(
			Stream					stream,
			X509Certificate			serverCertificate,
			bool					clientCertificateRequired,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream is null.");
			}
			if (!stream.CanRead || !stream.CanWrite)
			{
				throw new ArgumentNullException("stream is not both readable and writable.");
			}

			this.context = new ServerContext(
				this,
				securityProtocolType,
				serverCertificate,
				clientCertificateRequired);

			this.inputBuffer	= new BufferedStream(new MemoryStream());
			this.innerStream	= stream;
			this.ownsStream		= ownsStream;
			this.read			= new object ();
			this.write			= new object ();
			this.protocol		= new ServerRecordProtocol(innerStream, context);
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
						if (this.context.HandshakeState == HandshakeState.Finished)
						{
							// Write close notify
							this.protocol.SendAlert(AlertDescription.CloseNotify);
						}

						if (this.ownsStream)
						{
							// Close inner stream
							this.innerStream.Close();
						}
					}
					this.ownsStream				= false;
					this.innerStream			= null;
					this.ClientCertValidation	= null;
					this.PrivateKeySelection	= null;
				}

				this.disposed = true;
			}
		}

		#endregion

		#region Methods

		public override IAsyncResult BeginRead(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			this.checkDisposed();
			
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
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			lock (this)
			{
				if (this.context.HandshakeState == HandshakeState.None)
				{
					this.doHandshake();	// Handshake negotiation
				}
			}

			IAsyncResult asyncResult;

			lock (this.read)
			{
				try
				{
					// If actual buffer is full readed reset it
					if (this.inputBuffer.Position == this.inputBuffer.Length &&
						this.inputBuffer.Length > 0)
					{
						this.resetBuffer();
					}

					if (!this.context.ConnectionEnd)
					{
						// Check if we have space in the middle buffer
						// if not Read next TLS record and update the inputBuffer
						while ((this.inputBuffer.Length - this.inputBuffer.Position) < count)
						{
							// Read next record and write it into the inputBuffer
							long	position	= this.inputBuffer.Position;					
							byte[]	record		= this.protocol.ReceiveRecord();
					
							if (record != null && record.Length > 0)
							{
								// Write new data to the inputBuffer
								this.inputBuffer.Seek(0, SeekOrigin.End);
								this.inputBuffer.Write(record, 0, record.Length);

								// Restore buffer position
								this.inputBuffer.Seek(position, SeekOrigin.Begin);
							}
							else
							{
								if (record == null)
								{
									break;
								}
							}

							// TODO: Review if we need to check the Length
							// property of the innerStream for other types
							// of streams, to check that there are data available
							// for read
							if (this.innerStream is NetworkStream &&
								!((NetworkStream)this.innerStream).DataAvailable)
							{
								break;
							}
						}
					}

					asyncResult = this.inputBuffer.BeginRead(
						buffer, offset, count, callback, state);
				}
				catch (TlsException ex)
				{
					this.protocol.SendAlert(ex.Alert);
					this.Close();

					throw new IOException("The authentication or decryption has failed.");
				}
				catch (Exception)
				{
					throw new IOException("IO exception during read.");
				}
			}

			return asyncResult;
		}

		public override IAsyncResult BeginWrite(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			this.checkDisposed();

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
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			lock (this)
			{
				if (this.context.HandshakeState == HandshakeState.None)
				{
					// Start handshake negotiation
					this.doHandshake();
				}
			}

			IAsyncResult asyncResult;

			lock (this.write)
			{
				try
				{
					// Send the buffer as a TLS record					
					byte[] record = this.protocol.EncodeRecord(
						ContentType.ApplicationData, buffer, offset, count);
				
					asyncResult = this.innerStream.BeginWrite(
						record, 0, record.Length, callback, state);
				}
				catch (TlsException ex)
				{
					this.protocol.SendAlert(ex.Alert);
					this.Close();

					throw new IOException("The authentication or decryption has failed.");
				}
				catch (Exception)
				{
					throw new IOException("IO exception during Write.");
				}
			}

			return asyncResult;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

			return this.inputBuffer.EndRead(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

			this.innerStream.EndWrite (asyncResult);
		}

		public override void Close()
		{
			((IDisposable)this).Dispose();
		}

		public override void Flush()
		{
			this.checkDisposed();

			this.innerStream.Flush();
		}

		public int Read(byte[] buffer)
		{
			return this.Read(buffer, 0, buffer.Length);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			IAsyncResult res = this.BeginRead(buffer, offset, count, null, null);

			return this.EndRead(res);
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
			this.Write(buffer, 0, buffer.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			IAsyncResult res = this.BeginWrite (buffer, offset, count, null, null);

			this.EndWrite(res);
		}

		#endregion

		#region Misc Methods

		private void resetBuffer()
		{
			this.inputBuffer.SetLength(0);
			this.inputBuffer.Position = 0;
		}

		private void checkDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}
		}

		#endregion

		#region Handsake Methods

		/*
			Client											Server

			ClientHello                 -------->
															ServerHello
															Certificate*
															ServerKeyExchange*
															CertificateRequest*
										<--------			ServerHelloDone
			Certificate*
			ClientKeyExchange
			CertificateVerify*
			[ChangeCipherSpec]
			Finished                    -------->
															[ChangeCipherSpec]
										<--------           Finished
			Application Data            <------->			Application Data

					Fig. 1 - Message flow for a full handshake		
		*/

		private void doHandshake()
		{
			try
			{
				// Reset the context if needed
				if (this.context.HandshakeState != HandshakeState.None)
				{
					this.context.Clear();
				}

				// Obtain supported cipher suites
				this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);

				// Set handshake state
				this.context.HandshakeState = HandshakeState.Started;

				// Receive Client Hello message
				this.protocol.ReceiveRecord();

				// If received message is not an ClientHello send a
				// Fatal Alert
				if (this.context.LastHandshakeMsg != HandshakeType.ClientHello)
				{
					this.protocol.SendAlert(AlertDescription.UnexpectedMessage);
				}

				// Send ServerHello message
				this.protocol.SendRecord(HandshakeType.ServerHello);

				// Send ServerCertificate message
				this.protocol.SendRecord(HandshakeType.Certificate);

				// If the negotiated cipher is a KeyEx cipher send ServerKeyExchange
				if (this.context.Cipher.ExchangeAlgorithmType == ExchangeAlgorithmType.RsaKeyX)
				{
					this.protocol.SendRecord(HandshakeType.ServerKeyExchange);
				}

				// If the negotiated cipher is a KeyEx cipher or
				// the client certificate is required send the CertificateRequest message
				if (this.context.Cipher.ExchangeAlgorithmType == ExchangeAlgorithmType.RsaKeyX ||
					this.context.ClientCertificateRequired)
				{
					this.protocol.SendRecord(HandshakeType.CertificateRequest);
				}

				// Send ServerHelloDone message
				this.protocol.SendRecord(HandshakeType.ServerHelloDone);

				// Receive client response, until the Client Finished message
				// is received
				while (this.context.LastHandshakeMsg != HandshakeType.Finished)
				{
					this.protocol.ReceiveRecord();
				}
				
				// Send ChangeCipherSpec and ServerFinished messages
				this.protocol.SendChangeCipherSpec();

				// The handshake is finished
				this.context.HandshakeState = HandshakeState.Finished;

				// Clear Key Info
				this.context.ClearKeyInfo();
			}
			catch (TlsException ex)
			{
				this.protocol.SendAlert(ex.Alert);
				this.Close();

				throw new IOException("The authentication or decryption has failed.");
			}
			catch (Exception)
			{
				this.protocol.SendAlert(AlertDescription.InternalError);
				this.Close();

				throw new IOException("The authentication or decryption has failed.");
			}
		}

		#endregion

		#region Event Methods

		internal bool RaiseClientCertificateValidation(
			X509Certificate certificate, 
			int[]			certificateErrors)
		{
			if (this.ClientCertValidation != null)
			{
				return this.ClientCertValidation(certificate, certificateErrors);
			}

			return (certificateErrors != null && certificateErrors.Length == 0);
		}

		internal AsymmetricAlgorithm RaisePrivateKeySelection(
			X509Certificate certificate, 
			string			targetHost)
		{
			if (this.PrivateKeySelection != null)
			{
				return this.PrivateKeySelection(certificate, targetHost);
			}

			return null;
		}

		#endregion
	}
}
