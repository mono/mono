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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal abstract class RecordProtocol
	{
		#region Fields

		protected Stream	innerStream;
		protected Context	context;

		#endregion

		#region Properties

		public Stream InnerStream
		{
			get { return this.innerStream; }
			set { this.innerStream = value; }
		}

		public Context Context
		{
			get { return this.context; }
			set { this.context = value; }
		}

		#endregion

		#region Constructors

		public RecordProtocol(Stream innerStream, Context context)
		{
			this.innerStream	= innerStream;
			this.context		= context;
		}

		#endregion

		#region Abstract Methods

		public abstract void SendRecord(HandshakeType type);
		protected abstract void ProcessHandshakeMessage(TlsStream handMsg);
				
		#endregion

		#region Reveive Record Methods

		public byte[] ReceiveRecord()
		{
			if (this.context.ConnectionEnd)
			{
				throw this.context.CreateException("The session is finished and it's no longer valid.");
			}
			
			// Try to read the Record Content Type
			int type = this.innerStream.ReadByte();

			// There are no more data for read
			if (type == -1)
			{
				return null;
			}

			ContentType	contentType	= (ContentType)type;
			short		protocol	= this.readShort();
			short		length		= this.readShort();
			
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
			if (protocol != this.context.Protocol && 
				this.context.ProtocolNegotiated)
			{
				throw this.context.CreateException("Invalid protocol version on message received from server");
			}

			// Decrypt message contents if needed
			if (contentType == ContentType.Alert && length == 2)
			{
			}
			else
			{
				if (this.context.IsActual &&
					contentType != ContentType.ChangeCipherSpec)
				{
					message = this.decryptRecordFragment(
						contentType, 
						message.ToArray());
				}
			}

			// Set last handshake message received to None
			this.context.LastHandshakeMsg = HandshakeType.None;
			
			// Process record
			byte[] result = message.ToArray();

			switch (contentType)
			{
				case ContentType.Alert:
					this.processAlert(
						(AlertLevel)message.ReadByte(),
						(AlertDescription)message.ReadByte());
					break;

				case ContentType.ChangeCipherSpec:
					// Reset sequence numbers
					this.context.ReadSequenceNumber = 0;
					break;

				case ContentType.ApplicationData:
					break;

				case ContentType.Handshake:
					while (!message.EOF)
					{
						this.ProcessHandshakeMessage(message);
					}

					// Update handshakes of current messages
					this.context.HandshakeMessages.Write(message.ToArray());
					break;

				default:
					throw this.context.CreateException("Unknown record received from server.");
			}

			return result;
		}

		private short readShort()
		{
			byte[] b = new byte[2];
			this.innerStream.Read(b, 0, b.Length);

			short val = BitConverter.ToInt16(b, 0);

			return System.Net.IPAddress.HostToNetworkOrder(val);
		}

		private void processAlert(
			AlertLevel			alertLevel, 
			AlertDescription	alertDesc)
		{
			switch (alertLevel)
			{
				case AlertLevel.Fatal:
					throw this.context.CreateException(alertLevel, alertDesc);					

				case AlertLevel.Warning:
				default:
				switch (alertDesc)
				{
					case AlertDescription.CloseNotify:
						this.context.ConnectionEnd = true;
						break;
				}
				break;
			}
		}

		#endregion

		#region Send Alert Methods

		public void SendAlert(AlertDescription description)
		{
			this.SendAlert(new Alert(this.Context, description));
		}

		public void SendAlert(
			AlertLevel			level, 
			AlertDescription	description)
		{
			this.SendAlert(new Alert(this.Context, level, description));
		}

		public void SendAlert(Alert alert)
		{			
			// Write record
			this.SendRecord(ContentType.Alert, alert.ToArray());

			// Update session
			alert.Update();

			// Reset message contents
			alert.Reset();
		}

		#endregion

		#region Send Record Methods

		public void SendChangeCipherSpec()
		{
			// Send Change Cipher Spec message
			this.SendRecord(ContentType.ChangeCipherSpec, new byte[] {1});

			// Reset sequence numbers
			this.context.WriteSequenceNumber = 0;

			// Make the pending state to be the current state
			this.context.IsActual = true;

			// Send Finished message
			this.SendRecord(HandshakeType.Finished);			
		}

		public void SendRecord(ContentType contentType, byte[] recordData)
		{
			if (this.context.ConnectionEnd)
			{
				throw this.context.CreateException("The session is finished and it's no longer valid.");
			}

			byte[] record = this.EncodeRecord(contentType, recordData);

			this.innerStream.Write(record, 0, record.Length);
		}

		public byte[] EncodeRecord(ContentType contentType, byte[] recordData)
		{
			return this.EncodeRecord(
				contentType,
				recordData,
				0,
				recordData.Length);
		}

		public byte[] EncodeRecord(
			ContentType	contentType, 
			byte[]		recordData,
			int			offset,
			int			count)
		{
			if (this.context.ConnectionEnd)
			{
				throw this.context.CreateException("The session is finished and it's no longer valid.");
			}

			TlsStream record = new TlsStream();

			int	position = offset;

			while (position < ( offset + count ))
			{
				short	fragmentLength = 0;
				byte[]	fragment;

				if ((count - position) > Context.MAX_FRAGMENT_SIZE)
				{
					fragmentLength = Context.MAX_FRAGMENT_SIZE;
				}
				else
				{
					fragmentLength = (short)(count - position);
				}

				// Fill the fragment data
				fragment = new byte[fragmentLength];
				Buffer.BlockCopy(recordData, position, fragment, 0, fragmentLength);

				if (this.context.IsActual)
				{
					// Encrypt fragment
					fragment = this.encryptRecordFragment(contentType, fragment);
				}

				// Write tls message
				record.Write((byte)contentType);
				record.Write(this.context.Protocol);
				record.Write((short)fragment.Length);
				record.Write(fragment);

				// Update buffer position
				position += fragmentLength;
			}

			return record.ToArray();
		}
		
		#endregion

		#region Cryptography Methods

		private byte[] encryptRecordFragment(
			ContentType	contentType, 
			byte[]		fragment)
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

		private TlsStream decryptRecordFragment(
			ContentType	contentType, 
			byte[]		fragment)
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
	}
}
