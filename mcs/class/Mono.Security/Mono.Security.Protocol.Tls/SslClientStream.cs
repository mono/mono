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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;

namespace Mono.Security.Protocol.Tls
{
	#region Delegates

	public delegate bool CertificateValidationCallback(
		X509Certificate certificate, 
		int[]			certificateErrors);
	
	public delegate X509Certificate CertificateSelectionCallback(
		X509CertificateCollection	clientCertificates, 
		X509Certificate				serverCertificate, 
		string						targetHost, 
		X509CertificateCollection	serverRequestedCertificates);
	
	public delegate AsymmetricAlgorithm PrivateKeySelectionCallback(
		X509Certificate				clientCertificate, 
		string						targetHost);

	#endregion

	public class SslClientStream : Stream, IDisposable
	{
		#region Events

		public event TlsWarningAlertEventHandler WarningAlert;

		#endregion

		#region Internal Events
		
		internal event CertificateValidationCallback	ServerCertValidation;
		internal event CertificateSelectionCallback		ClientCertSelection;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Fields

		private CertificateValidationCallback	serverCertValidationDelegate;
		private CertificateSelectionCallback	clientCertSelectionDelegate;
		private PrivateKeySelectionCallback		privateKeySelectionDelegate;
		private Stream							innerStream;
		private BufferedStream					inputBuffer;
		private TlsContext						context;
		private bool							ownsStream;
		private bool							disposed;
		private string							read;
		private string							write;

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return this.innerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return this.innerStream.CanWrite; }
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
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public CipherAlgorithmType CipherAlgorithm 
		{
			get { return this.context.Cipher.CipherAlgorithmType;}
		}
		
		public int CipherStrength 
		{
			get { return this.context.Cipher.EffectiveKeyBits;}
		}
		
		public X509CertificateCollection ClientCertificates 
		{
			get { return this.context.ClientSettings.Certificates;}
		}
		
		public HashAlgorithmType HashAlgorithm 
		{
			get { return this.context.Cipher.HashAlgorithmType; }
		}
		
		public int HashStrength
		{
			get { return this.context.Cipher.HashSize * 8; }
		}
		
		public int KeyExchangeStrength 
		{
			get 
			{ 
				return this.context.ServerSettings.Certificates[0].RSA.KeySize;
			}
		}
		
		public ExchangeAlgorithmType KeyExchangeAlgorithm 
		{
			get { return this.context.Cipher.ExchangeAlgorithmType; }
		}
		
		public SecurityProtocolType SecurityProtocol 
		{
			get { return this.context.Protocol; }
		}
		
		public X509Certificate SelectedClientCertificate 
		{
			get { throw new NotImplementedException(); }
		}

		public X509Certificate ServerCertificate 
		{
			get { throw new NotImplementedException(); }
		} 

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ServerCertValidationDelegate
		{
			get { return this.serverCertValidationDelegate; }
			set 
			{ 
				if (this.ServerCertValidation != null)
				{
					this.ServerCertValidation -= this.serverCertValidationDelegate;
				}
				this.serverCertValidationDelegate	= value;
				this.ServerCertValidation			+= this.serverCertValidationDelegate;
			}
		}

		public CertificateSelectionCallback ClientCertSelectionDelegate 
		{
			get { return this.clientCertSelectionDelegate; }
			set 
			{ 
				if (this.ClientCertSelection != null)
				{
					this.ClientCertSelection -= this.clientCertSelectionDelegate;
				}
				this.clientCertSelectionDelegate	= value;
				this.ClientCertSelection			+= this.clientCertSelectionDelegate;
			}
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate 
		{
			get { return this.privateKeySelectionDelegate; }
			set 
			{ 
				if (this.PrivateKeySelection != null)
				{
					this.PrivateKeySelection -= this.privateKeySelectionDelegate;
				}
				this.privateKeySelectionDelegate	= value;
				this.PrivateKeySelection			+= this.privateKeySelectionDelegate;
			}
		}

		#endregion

		#region Constructors
		
		public SslClientStream(
			Stream stream, 
			string targetHost, 
			bool ownsStream) 
			: this(stream, targetHost, 
			ownsStream, SecurityProtocolType.Default, null)
		{
		}
		
		public SslClientStream(
			Stream stream, string targetHost, X509Certificate clientCertificate) : 
			this(
			stream, targetHost, 
			false, SecurityProtocolType.Default, 
			new X509CertificateCollection(new X509Certificate[]{clientCertificate}))
		{
		}

		public SslClientStream(
			Stream stream,
			string targetHost, X509CertificateCollection clientCertificates) : 
			this(stream, targetHost, false, 
			SecurityProtocolType.Default, clientCertificates)
		{
		}

		public SslClientStream(
			Stream stream,
			string targetHost,
			bool ownsStream,
			SecurityProtocolType securityProtocolType) : 
			this(stream, targetHost, ownsStream, securityProtocolType,
			new X509CertificateCollection())
		{
		}

		public SslClientStream(
			Stream stream,
			string targetHost,
			bool ownsStream,
			SecurityProtocolType securityProtocolType,
			X509CertificateCollection clientCertificates)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream is null.");
			}
			if (!stream.CanRead || !stream.CanWrite)
			{
				throw new ArgumentNullException("stream is not both readable and writable.");
			}
			if (targetHost == null || targetHost.Length == 0)
			{
				throw new ArgumentNullException("targetHost is null or an empty string.");
			}

			this.context		= new TlsContext(
				this,
				securityProtocolType, 
				targetHost, 
				clientCertificates);
			this.inputBuffer	= new BufferedStream(new MemoryStream());
			this.innerStream	= stream;
			this.ownsStream		= ownsStream;
			this.read			= String.Empty;
			this.write			= String.Empty;
		}

		#endregion

		#region Finalizer

		~SslClientStream()
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
						// Write close notify
						TlsCloseNotifyAlert alert = new TlsCloseNotifyAlert(this.context);
						this.SendAlert(alert);

						if (this.ownsStream)
						{
							// Close inner stream
							this.innerStream.Close();
						}
					}
					this.ownsStream						= false;
					this.innerStream					= null;
					if (this.ClientCertSelection != null)
					{
						this.ClientCertSelection -= this.clientCertSelectionDelegate;
					}
					if (this.ServerCertValidation != null)
					{
						this.ServerCertValidation -= this.serverCertValidationDelegate;
					}
					this.serverCertValidationDelegate	= null;
					this.clientCertSelectionDelegate	= null;
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
			AsyncCallback callback,
			object state)
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}
			
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
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

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
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}

			this.innerStream.Flush();
		}

		public int Read(byte[] buffer)
		{
			return this.Read(buffer, 0, buffer.Length);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}
			
			if (!this.context.HandshakeFinished)
			{
				this.doHandshake(); // Handshake negotiation
			}

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

			if (!Monitor.TryEnter(this.read))
			{
				throw new InvalidOperationException("A read operation is already in progress.");
			}
			try
			{
				System.Threading.Monitor.Enter(this.read);

				// If actual buffer is full readed reset it
				if (this.inputBuffer.Position == this.inputBuffer.Length &&
					this.inputBuffer.Length > 0)
				{
					this.resetBuffer();
				}

				// Check if we have space in the middle buffer
				// if not Read next TLS record and update the inputBuffer
				while ((this.inputBuffer.Length - this.inputBuffer.Position) < count)
				{
					// Read next record and write it into the inputBuffer
					long	position	= this.inputBuffer.Position;					
					byte[]	record		= this.receiveRecord();
					
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

				return this.inputBuffer.Read(buffer, offset, count);
			}
			catch (TlsException ex)
			{
				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during read.", ex);
			}
			finally
			{
				System.Threading.Monitor.Exit(this.read);
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

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}

			if (!this.context.HandshakeFinished)
			{
				// Start handshake negotiation
				this.doHandshake();
			}

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

			if (!Monitor.TryEnter(this.write))
			{
				throw new InvalidOperationException("A write operation is already in progress.");
			}
			try
			{
				Monitor.Enter(this.write);

				// Send the buffer as a TLS record
				byte[] recordData = new byte[count];
				System.Array.Copy(buffer, offset, recordData, 0, count);

				this.sendRecord(TlsContentType.ApplicationData, recordData);
			}
			catch (TlsException ex)
			{
				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				throw new IOException("IO exception during Write.", ex);
			}
			finally
			{
				Monitor.Exit(this.write);
			}			
		}

		#endregion

		#region Reveive Record Methods

		private byte[] receiveRecord()
		{
			if (this.context.ConnectionEnd)
			{
				throw this.context.CreateException("The session is finished and it's no longer valid.");
			}
			
			// Try to read the Record Content Type
			int type = innerStream.ReadByte();

			// There are no more data for read
			if (type == -1)
			{
				return null;
			}

			TlsContentType			contentType	= (TlsContentType)type;
			SecurityProtocolType	protocol	= (SecurityProtocolType)this.ReadShort();
			short					length		= this.ReadShort();
			
			// Read Record data
			int		received	= 0;
			byte[]	buffer		= new byte[length];
			while (received != length)
			{
				received += this.innerStream.Read(
					buffer, received, buffer.Length - received);
			}

			TlsStream message = new TlsStream(buffer);
		
			// Check that the message has a valid protocol version
			if (protocol != this.context.Protocol)
			{
				throw this.context.CreateException("Invalid protocol version on message received from server");
			}

			// Decrypt message contents if needed
			if (contentType == TlsContentType.Alert && length == 2)
			{
			}
			else
			{
				if (this.context.IsActual &&
					contentType != TlsContentType.ChangeCipherSpec)
				{
					message = this.decryptRecordFragment(
						contentType, 
						protocol,
						message.ToArray());
				}
			}

			byte[] result = message.ToArray();

			// Process record
			switch (contentType)
			{
				case TlsContentType.Alert:
					this.processAlert((TlsAlertLevel)message.ReadByte(),
						(TlsAlertDescription)message.ReadByte());
					break;

				case TlsContentType.ChangeCipherSpec:
					// Reset sequence numbers
					this.context.ReadSequenceNumber = 0;
					break;

				case TlsContentType.ApplicationData:
					break;

				case TlsContentType.Handshake:
					while (!message.EOF)
					{
						this.processHandshakeMessage(message);
					}
					// Update handshakes of current messages
					this.context.HandshakeMessages.Write(message.ToArray());
					break;

				default:
					throw this.context.CreateException("Unknown record received from server.");
			}

			return result;
		}

		#endregion

		#region Send Record Methods

		internal void SendAlert(TlsAlert alert)
		{			
			// Write record
			this.sendRecord(TlsContentType.Alert, alert.ToArray());

			// Update session
			alert.Update();

			// Reset message contents
			alert.Reset();
		}

		private void sendRecord(TlsHandshakeType type)
		{
			TlsHandshakeMessage msg = createClientHandshakeMessage(type);
			
			// Write record
			this.sendRecord(msg.ContentType, msg.EncodeMessage());

			// Update session
			msg.Update();

			// Reset message contents
			msg.Reset();
		}

		private void sendChangeCipherSpec()
		{
			// Send Change Cipher Spec message
			this.sendRecord(TlsContentType.ChangeCipherSpec, new byte[] {1});

			// Reset sequence numbers
			this.context.WriteSequenceNumber = 0;

			// Make the pending state to be the current state
			this.context.IsActual = true;

			// Send Finished message
			this.sendRecord(TlsHandshakeType.Finished);			
		}
		
		private void sendRecord(TlsContentType contentType, byte[] recordData)
		{
			if (this.context.ConnectionEnd)
			{
				throw this.context.CreateException("The session is finished and it's no longer valid.");
			}

			byte[][] fragments = this.fragmentData(recordData);
			for (int i = 0; i < fragments.Length; i++)
			{
				byte[] fragment = fragments[i];

				if (this.context.IsActual)
				{
					// Encrypt fragment
					fragment = this.encryptRecordFragment(contentType, fragment);
				}

				// Write tls message
				TlsStream record = new TlsStream();
				record.Write((byte)contentType);
				record.Write((short)this.context.Protocol);
				record.Write((short)fragment.Length);
				record.Write(fragment);

				// Write record
				this.innerStream.Write(record.ToArray(), 0, (int)record.Length);

				// Reset record data
				record.Reset();
			}
		}

		private byte[][] fragmentData(byte[] messageData)
		{
			ArrayList d = new ArrayList();
			
			int	position = 0;

			while (position < messageData.Length)
			{
				short	fragmentLength = 0;
				byte[]	fragmentData;
				if ((messageData.Length - position) > TlsContext.MAX_FRAGMENT_SIZE)
				{
					fragmentLength = TlsContext.MAX_FRAGMENT_SIZE;
				}
				else
				{
					fragmentLength = (short)(messageData.Length - position);
				}
				fragmentData = new byte[fragmentLength];

				System.Array.Copy(messageData, position, fragmentData, 0, fragmentLength);

				d.Add(fragmentData);

				position += fragmentLength;
			}

			byte[][] result = new byte[d.Count][];
			for (int i = 0; i < d.Count; i++)
			{
				result[i] = (byte[])d[i];
			}

			return result;
		}

		#endregion

		#region Cryptography Methods

		private byte[] encryptRecordFragment(TlsContentType contentType, byte[] fragment)
		{
			// Calculate message MAC
			byte[] mac	= this.context.Cipher.ComputeClientRecordMAC(contentType, fragment);

			// Encrypt the message
			byte[] ecr = this.context.Cipher.EncryptRecord(fragment, mac);

			// Set new IV
			if (this.context.Cipher.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[this.context.Cipher.IvSize];
				System.Array.Copy(ecr, ecr.Length - iv.Length, iv, 0, iv.Length);
				this.context.Cipher.UpdateClientCipherIV(iv);
			}

			// Update sequence number
			this.context.WriteSequenceNumber++;

			return ecr;
		}

		private TlsStream decryptRecordFragment(TlsContentType contentType, 
			SecurityProtocolType protocol,
			byte[] fragment)
		{
			byte[]	dcrFragment	= null;
			byte[]	dcrMAC		= null;

			// Decrypt message
			this.context.Cipher.DecryptRecord(fragment, ref dcrFragment, ref dcrMAC);

			// Set new IV
			if (this.context.Cipher.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[this.context.Cipher.IvSize];
				System.Array.Copy(fragment, fragment.Length - iv.Length, iv, 0, iv.Length);
				this.context.Cipher.UpdateServerCipherIV(iv);
			}
			
			// Check MAC code
			byte[] mac = this.context.Cipher.ComputeServerRecordMAC(contentType, dcrFragment);

			// Check that the mac is correct
			if (mac.Length != dcrMAC.Length)
			{
				throw new TlsException("Invalid MAC received from server.");
			}
			for (int i = 0; i < mac.Length; i++)
			{
				if (mac[i] != dcrMAC[i])
				{
					throw new TlsException("Invalid MAC received from server.");
				}
			}

			// Update sequence number
			this.context.ReadSequenceNumber++;

			return new TlsStream(dcrFragment);
		}

		#endregion

		#region Handshake Processing Methods

		private void processHandshakeMessage(TlsStream handMsg)
		{
			TlsHandshakeType	handshakeType	= (TlsHandshakeType)handMsg.ReadByte();
			TlsHandshakeMessage	message			= null;

			// Read message length
			int length = handMsg.ReadInt24();

			// Read message data
			byte[] data = new byte[length];
			handMsg.Read(data, 0, length);

			// Create and process the server message
			message = this.createServerHandshakeMessage(handshakeType, data);

			// Update session
			if (message != null)
			{
				message.Update();
			}
		}

		private void processAlert(TlsAlertLevel alertLevel, TlsAlertDescription alertDesc)
		{
			switch (alertLevel)
			{
				case TlsAlertLevel.Fatal:
					throw this.context.CreateException(alertLevel, alertDesc);					

				case TlsAlertLevel.Warning:
				default:
				switch (alertDesc)
				{
					case TlsAlertDescription.CloseNotify:
						this.context.ConnectionEnd = true;
						break;

					default:
						this.RaiseWarningAlert(alertLevel, alertDesc);
						break;
				}
				break;
			}
		}

		#endregion

		#region Misc Methods

		private void resetBuffer()
		{
			this.inputBuffer.SetLength(0);
			this.inputBuffer.Position = 0;
		}

		private short ReadShort()
		{
			byte[] b = new byte[2];
			this.innerStream.Read(b, 0, b.Length);

			short val = BitConverter.ToInt16(b, 0);

			return System.Net.IPAddress.HostToNetworkOrder(val);
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
			// Obtain supported cipher suite collection
			this.context.SupportedCiphers = TlsCipherSuiteFactory.GetSupportedCiphers(context.Protocol);

			// Send client hello
			this.sendRecord(TlsHandshakeType.ClientHello);

			// Read server response
			while (!this.context.HelloDone)
			{
				// Read next record
				this.receiveRecord();
			}
			
			// Send client certificate if requested
			if (this.context.ServerSettings.CertificateRequest)
			{
				this.sendRecord(TlsHandshakeType.Certificate);
			}

			// Send Client Key Exchange
			this.sendRecord(TlsHandshakeType.ClientKeyExchange);

			// Now initialize session cipher with the generated keys
			this.context.Cipher.InitializeCipher();

			// Send certificate verify if requested
			if (this.context.ServerSettings.CertificateRequest)
			{
				this.sendRecord(TlsHandshakeType.CertificateVerify);
			}

			// Send Cipher Spec protocol
			this.sendChangeCipherSpec();			
			
			// Read record until server finished is received
			while (!this.context.HandshakeFinished)
			{
				// If all goes well this will process messages:
				// 		Change Cipher Spec
				//		Server finished
				this.receiveRecord();
			}

			// Clear Key Info
			this.context.ClearKeyInfo();
		}
		
		private TlsHandshakeMessage createClientHandshakeMessage(TlsHandshakeType type)
		{
			switch (type)
			{
				case TlsHandshakeType.ClientHello:
					return new TlsClientHello(this.context);

				case TlsHandshakeType.Certificate:
					return new TlsClientCertificate(this.context);

				case TlsHandshakeType.ClientKeyExchange:
					return new TlsClientKeyExchange(this.context);

				case TlsHandshakeType.CertificateVerify:
					return new TlsClientCertificateVerify(this.context);

				case TlsHandshakeType.Finished:
					return new TlsClientFinished(this.context);

				default:
					throw new InvalidOperationException("Unknown client handshake message type: " + type.ToString() );
			}
		}

		private TlsHandshakeMessage createServerHandshakeMessage(TlsHandshakeType type, byte[] buffer)
		{
			switch (type)
			{
				case TlsHandshakeType.HelloRequest:
					this.sendRecord(TlsHandshakeType.ClientHello);
					return null;

				case TlsHandshakeType.ServerHello:
					return new TlsServerHello(this.context, buffer);

				case TlsHandshakeType.Certificate:
					return new TlsServerCertificate(this.context, buffer);

				case TlsHandshakeType.ServerKeyExchange:
					return new TlsServerKeyExchange(this.context, buffer);

				case TlsHandshakeType.CertificateRequest:
					return new TlsServerCertificateRequest(this.context, buffer);

				case TlsHandshakeType.ServerHelloDone:
					return new TlsServerHelloDone(this.context, buffer);

				case TlsHandshakeType.Finished:
					return new TlsServerFinished(this.context, buffer);

				default:
					throw this.context.CreateException("Unknown server handshake message received ({0})", type.ToString());
			}
		}

		#endregion

		#region Event Methods

		internal void RaiseWarningAlert(TlsAlertLevel level, TlsAlertDescription description)
		{
			if (WarningAlert != null)
			{
				WarningAlert(this, new TlsWarningAlertEventArgs(level, description));
			}
		}

		internal bool RaiseServerCertificateValidation(
			X509Certificate certificate, int[] certificateErrors)
		{
			if (this.ServerCertValidation != null)
			{
				return this.ServerCertValidation(certificate, certificateErrors);
			}

			return false;
		}

		internal X509Certificate RaiseClientCertificateSelection(
			X509CertificateCollection clientCertificates, 
			X509Certificate serverCertificate, 
			string targetHost, 
			X509CertificateCollection serverRequestedCertificates)
		{
			if (this.ClientCertSelection != null)
			{
				return this.ClientCertSelection(
					clientCertificates,
					serverCertificate,
					targetHost,
					serverRequestedCertificates);
			}

			return null;
		}

		internal AsymmetricAlgorithm RaisePrivateKeySelection(
			X509Certificate clientCertificate, 
			string			targetHost)
		{
			if (this.PrivateKeySelection != null)
			{
				return this.PrivateKeySelection(
					clientCertificate,
					targetHost);
			}

			return null;
		}

		#endregion
	}
}
