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
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography;

using Mono.Security.Protocol.Tls;
using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.Protocol.Tls.Handshake;
using Mono.Security.Protocol.Tls.Handshake.Client;

namespace Mono.Security.Protocol.Tls
{
	public sealed class TlsSocket : Socket
	{
		#region FIELDS

		private TlsSession		session;
		private BufferedStream	inputBuffer;

		#endregion

		#region PROPERTIES

		internal TlsSession Session
		{
			get { return this.session; }
		}

		internal BufferedStream InputBuffer
		{
			get { return inputBuffer; }
		}

		#endregion

		#region CONSTRUCTORS

		private TlsSocket(
				AddressFamily	addressFamily,
				SocketType		socketType,
				ProtocolType	protocolType
				) : base(addressFamily, socketType, protocolType)
		{
			this.inputBuffer = new BufferedStream(new MemoryStream());
		}

		public TlsSocket(
			TlsSession		session,
			AddressFamily	addressFamily,
			SocketType		socketType,
			ProtocolType	protocolType
			) : this(addressFamily, socketType, protocolType)
		{
			this.session = session;
		}

		#endregion

		#region REPLACED_METHODS

		public new void Close()
		{
			this.resetBuffer();
			base.Close();
			if (this.session.State != TlsSessionState.Closing &&
				this.session.State != TlsSessionState.Closed)
			{
				this.session.Close();
			}
		}

		public new int Receive(byte[] buffer)
		{
			return this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
		}
						
		public new int Receive(byte[] buffer, SocketFlags socketFlags)
		{
			return this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
		}

		public new int Receive(byte[] buffer, int size, SocketFlags socketFlags)
		{
			return this.Receive(buffer, 0, size, socketFlags);
		}

		public new int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
		{
			if (!session.IsSecure)
			{
				return base.Receive(buffer, offset, size, socketFlags);
			}
			
			// If actual buffer is full readed reset it
			if (inputBuffer.Position == inputBuffer.Length)
			{
				this.resetBuffer();
			}

			// Check if we have space in the middle buffer
			// if not Read next TLS record and update the inputBuffer
			while ((inputBuffer.Length - inputBuffer.Position) < size)
			{
				// Read next record and write it into the inputBuffer
				long	position	= inputBuffer.Position;					
				byte[]	record		= this.receiveRecord();

				if (record.Length > 0)
				{
					// Write new data to the inputBuffer
					inputBuffer.Seek(0, SeekOrigin.End);
					inputBuffer.Write(record, 0, record.Length);

					// Restore buffer position
					inputBuffer.Seek(position, SeekOrigin.Begin);
				}

				if (base.Available == 0)
				{
					break;
				}
			}

			return inputBuffer.Read(buffer, offset, size);
		}

		public new int Send(byte[] buffer)
		{
			return this.Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
		}
						
		public new int Send(byte[] buffer, SocketFlags socketFlags)
		{
			return this.Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
		}

		public new int Send(byte[] buffer, int size, SocketFlags socketFlags)
		{
			return this.Send(buffer, 0, size, socketFlags);
		}

		public new int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
		{
			if (!session.IsSecure)
			{
				return base.Send(buffer, offset, size, socketFlags);
			}

			// Send the buffer as a TLS record
			byte[] recordData = new byte[size];
			System.Array.Copy(buffer, offset, recordData, 0, size);

			return this.sendRecord(TlsContentType.ApplicationData, recordData);
		}

		#endregion

		#region TLS_RECORD_METHODS

		private byte[] receiveRecord()
		{
			if (session.Context.ConnectionEnd)
			{
				throw session.CreateException("The session is finished and it's no longer valid.");
			}
			
			TlsContentType	contentType	= (TlsContentType)this.ReadByte();
			TlsProtocol		protocol	= (TlsProtocol)this.ReadShort();
			int				length		= this.ReadShort();
			
			// Read Record data
			int		received	= 0;
			byte[]	buffer		= new byte[length];						
			while (received != length)
			{
				received += base.Receive(
					buffer, received, buffer.Length - received, SocketFlags.None);
			}

			TlsStream message	= new TlsStream(buffer);
		
			// Check that the message as a valid protocol version
			if (protocol != session.Context.Protocol)
			{
				throw session.CreateException("Invalid protocol version on message received from server");
			}

			// Decrypt message contents if needed
			if (contentType == TlsContentType.Alert &&
				length == 2)
			{
			}
			else
			{
				if (session.Context.IsActual &&
					contentType != TlsContentType.ChangeCipherSpec)
				{
					message = decryptRecordFragment(
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
					processAlert((TlsAlertLevel)message.ReadByte(),
						(TlsAlertDescription)message.ReadByte());
					break;

				case TlsContentType.ChangeCipherSpec:
					// Reset sequence numbers
					session.Context.ReadSequenceNumber = 0;
					break;

				case TlsContentType.ApplicationData:
					break;

				case TlsContentType.Handshake:
					while (!message.EOF)
					{
						processHandshakeMessage(message);
					}
					// Update handshakes of current messages
					this.session.Context.HandshakeHashes.Update(message.ToArray());
					break;

				default:
					throw session.CreateException("Unknown record received from server.");
			}

			return result;
		}

		#endregion

		#region TLS_CRYPTO_METHODS

		private byte[] encryptRecordFragment(TlsContentType contentType, byte[] fragment)
		{
			// Calculate message MAC
			byte[] mac	= encodeClientRecordMAC(contentType, fragment);

			// Encrypt the message
			byte[] ecr = session.Context.Cipher.EncryptRecord(fragment, mac);

			// Set new IV
			if (session.Context.Cipher.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[session.Context.Cipher.IvSize];
				System.Array.Copy(ecr, ecr.Length - iv.Length, iv, 0, iv.Length);
				session.Context.Cipher.UpdateClientCipherIV(iv);
			}

			// Update sequence number
			session.Context.WriteSequenceNumber++;

			return ecr;
		}

		private TlsStream decryptRecordFragment(TlsContentType contentType, 
			TlsProtocol protocol,
			byte[] fragment)
		{
			byte[]	dcrFragment	= null;
			byte[]	dcrMAC		= null;

			// Decrypt message
			session.Context.Cipher.DecryptRecord(fragment, ref dcrFragment, ref dcrMAC);

			// Set new IV
			if (session.Context.Cipher.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[session.Context.Cipher.IvSize];
				System.Array.Copy(fragment, fragment.Length - iv.Length, iv, 0, iv.Length);
				session.Context.Cipher.UpdateServerCipherIV(iv);
			}
			
			// Check MAC code
			byte[] mac = this.encodeServerRecordMAC(contentType, dcrFragment);

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
			session.Context.ReadSequenceNumber++;

			return new TlsStream(dcrFragment);
		}

		#endregion

		#region TLS_SEND_METHODS

		internal int SendAlert(TlsAlert alert)
		{			
			// Write record
			int bytesSent = this.sendRecord(TlsContentType.Alert, alert.ToArray());

			// Update session
			alert.UpdateSession();

			// Reset message contents
			alert.Reset();

			return bytesSent;
		}

		private int sendRecord(TlsHandshakeType type)
		{
			TlsHandshakeMessage msg = createClientHandshakeMessage(type);
			
			// Write record
			int bytesSent = this.sendRecord(msg.ContentType, msg.EncodeMessage());

			// Update session
			msg.UpdateSession();

			// Reset message contents
			msg.Reset();

			return bytesSent;
		}

		private int sendChangeCipherSpec()
		{
			// Send Change Cipher Spec message
			int bytesSent = this.sendRecord(TlsContentType.ChangeCipherSpec, new byte[] {1});

			// Reset sequence numbers
			session.Context.WriteSequenceNumber = 0;

			// Make the pending state to be the current state
			session.Context.IsActual = true;

			// Send Finished message
			bytesSent += this.sendRecord(TlsHandshakeType.Finished);

			return bytesSent;
		}
		
		private int sendRecord(TlsContentType contentType, byte[] recordData)
		{
			if (session.Context.ConnectionEnd)
			{
				throw session.CreateException("The session is finished and it's no longer valid.");
			}

			int			bytesSent = 0;
			byte[][]	fragments = fragmentData(recordData);
			for (int i = 0; i < fragments.Length; i++)
			{
				byte[] fragment = fragments[i];

				if (session.Context.IsActual)
				{
					// Encrypt fragment
					fragment = encryptRecordFragment(contentType, fragment);
				}

				// Write tls message
				TlsStream record = new TlsStream();
				record.Write((byte)contentType);
				record.Write((short)TlsProtocol.Tls1);
				record.Write((short)fragment.Length);
				record.Write(fragment);

				// Write record
				bytesSent += base.Send(record.ToArray());

				// Reset record data
				record.Reset();
			}

			return bytesSent;
		}

		private byte[][] fragmentData(byte[] messageData)
		{
			ArrayList d = new ArrayList();
			
			int	position = 0;

			while (position < messageData.Length)
			{
				short	fragmentLength = 0;
				byte[]	fragmentData;
				if ((messageData.Length - position) > session.MaxFragmentSize)
				{
					fragmentLength = session.MaxFragmentSize;
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

		#region MESSAGE_PROCESSING

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
			message = createServerHandshakeMessage(handshakeType, data);

			// Update session
			if (message != null)
			{
				message.UpdateSession();
			}
		}

		private void processAlert(TlsAlertLevel alertLevel, 
			TlsAlertDescription alertDesc)
		{
			switch (alertLevel)
			{
				case TlsAlertLevel.Fatal:
					throw session.CreateException(alertLevel, alertDesc);					

				case TlsAlertLevel.Warning:
				default:
				switch (alertDesc)
				{
					case TlsAlertDescription.CloseNotify:
						session.Context.ConnectionEnd = true;
						break;

					default:
						session.RaiseWarningAlert(alertLevel, alertDesc);
						break;
				}
					break;
			}
		}

		#endregion

		#region MISC_METHODS

		private void resetBuffer()
		{
			this.inputBuffer.SetLength(0);
			this.inputBuffer.Position = 0;
		}

		private byte[] encodeServerRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			TlsStream	data	= new TlsStream();
			byte[]		result	= null;

			data.Write(session.Context.ReadSequenceNumber);
			data.Write((byte)contentType);
			data.Write((short)TlsProtocol.Tls1);
			data.Write((short)fragment.Length);
			data.Write(fragment);

			result = session.Context.Cipher.ServerHMAC.ComputeHash(data.ToArray());

			data.Reset();

			return result;
		}

		private byte[] encodeClientRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			TlsStream	data	= new TlsStream();
			byte[]		result	= null;

			data.Write(session.Context.WriteSequenceNumber);
			data.Write((byte)contentType);
			data.Write((short)TlsProtocol.Tls1);
			data.Write((short)fragment.Length);
			data.Write(fragment);

			result = session.Context.Cipher.ClientHMAC.ComputeHash(data.ToArray());

			data.Reset();

			return result;
		}

		private byte ReadByte()
		{
			byte[] b = new byte[1];
			base.Receive(b);

			return b[0];
		}

		private short ReadShort()
		{
			byte[] b = new byte[2];
			base.Receive(b);

			short val = BitConverter.ToInt16(b, 0);

			return System.Net.IPAddress.HostToNetworkOrder(val);
		}

		#endregion

		#region HANDSHAKE_METHODS

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

		internal void DoHandshake()
		{
			// Reset isSecure field
			this.session.IsSecure = false;

			// Send client hello
			this.sendRecord(TlsHandshakeType.ClientHello);

			// Read server response
			while (!session.HelloDone)
			{
				// Read next record
				this.receiveRecord();
			}
			
			// Send client certificate if requested
			if (session.Context.ServerSettings.CertificateRequest)
			{
				this.sendRecord(TlsHandshakeType.Certificate);
			}

			// Send Client Key Exchange
			this.sendRecord(TlsHandshakeType.ClientKeyExchange);

			// Now initialize session cipher with the generated keys
			this.session.Context.Cipher.InitializeCipher();

			// Send certificate verify if requested
			if (session.Context.ServerSettings.CertificateRequest)
			{
				this.sendRecord(TlsHandshakeType.CertificateVerify);
			}

			// Send Cipher Spec protocol
			this.sendChangeCipherSpec();			
			
			// Read Cipher Spec protocol
			this.receiveRecord();

			// Read server finished
			if (!session.HandshakeFinished)
			{
				this.receiveRecord();
			}

			// Clear Key Info
			this.session.Context.ClearKeyInfo();

			// Set isSecure
			this.session.IsSecure = true;
		}
		
		private TlsHandshakeMessage createClientHandshakeMessage(TlsHandshakeType type)
		{
			switch (type)
			{
				case TlsHandshakeType.ClientHello:
					return new TlsClientHello(session);

				case TlsHandshakeType.Certificate:
					return new TlsClientCertificate(session);

				case TlsHandshakeType.ClientKeyExchange:
					return new TlsClientKeyExchange(session);

				case TlsHandshakeType.CertificateVerify:
					return new TlsClientCertificateVerify(session);

				case TlsHandshakeType.Finished:
					return new TlsClientFinished(session);

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
					return new TlsServerHello(session, buffer);

				case TlsHandshakeType.Certificate:
					return new TlsServerCertificate(session, buffer);

				case TlsHandshakeType.ServerKeyExchange:
					return new TlsServerKeyExchange(session, buffer);

				case TlsHandshakeType.CertificateRequest:
					return new TlsServerCertificateRequest(session, buffer);

				case TlsHandshakeType.ServerHelloDone:
					return new TlsServerHelloDone(session, buffer);

				case TlsHandshakeType.Finished:
					return new TlsServerFinished(session, buffer);

				default:
					throw session.CreateException("Unknown server handshake message received ({0})", type.ToString());
			}
		}

		#endregion
	}
}