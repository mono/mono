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

		public Context Context
		{
			get { return this.context; }
			set { this.context = value; }
		}

		#endregion

		#region Constructors

		public RecordProtocol(Stream innerStream, Context context)
		{
			this.innerStream			= innerStream;
			this.context				= context;
			this.context.RecordProtocol = this;
		}

		#endregion

		#region Abstract Methods

		public abstract void SendRecord(HandshakeType type);
		protected abstract void ProcessHandshakeMessage(TlsStream handMsg);
		protected abstract void ProcessChangeCipherSpec();
				
		#endregion

		#region Reveive Record Methods

		public byte[] ReceiveRecord()
		{
			if (this.context.ConnectionEnd)
			{
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
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

			DebugHelper.WriteLine(
				">>>> Read record ({0}|{1})", 
				this.context.DecodeProtocolCode(protocol),
				contentType);
			DebugHelper.WriteLine("Record data", buffer);

			TlsStream message = new TlsStream(buffer);
		
			// Check that the message has a valid protocol version
			if (protocol != this.context.Protocol && 
				this.context.ProtocolNegotiated)
			{
				throw new TlsException(
					AlertDescription.ProtocolVersion,
					"Invalid protocol version on message received from server");
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

					DebugHelper.WriteLine("Decrypted record data", message.ToArray());
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
					// don't include alert data with application data
					// anyway the data is already processed
					result = null;
					break;

				case ContentType.ChangeCipherSpec:
					this.ProcessChangeCipherSpec();
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
					throw new TlsException(
						AlertDescription.UnexpectedMessage,
						"Unknown record received from server.");
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
					throw new TlsException(alertLevel, alertDesc);

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
			this.SendAlert(new Alert(description));
		}

		public void SendAlert(
			AlertLevel			level, 
			AlertDescription	description)
		{
			this.SendAlert(new Alert(level, description));
		}

		public void SendAlert(Alert alert)
		{
			DebugHelper.WriteLine(">>>> Write Alert ({0}|{1})", alert.Description, alert.Message);

			// Write record
			this.SendRecord(
				ContentType.Alert, 
				new byte[]{(byte)alert.Level, (byte)alert.Description});

			if (alert.IsCloseNotify)
			{
				this.context.ConnectionEnd = true;
			}
		}

		#endregion

		#region Send Record Methods

		public void SendChangeCipherSpec()
		{
			DebugHelper.WriteLine(">>>> Write Change Cipher Spec");

			// Send Change Cipher Spec message as a plain message
			this.context.IsActual = false;

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
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
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
				throw new TlsException(
					AlertDescription.InternalError,
					"The session is finished and it's no longer valid.");
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

				DebugHelper.WriteLine("Record data", fragment);

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
			byte[] mac	= null;

			// Calculate message MAC
			if (this.Context is ClientContext)
			{
				mac	= this.context.Cipher.ComputeClientRecordMAC(contentType, fragment);
			}	
			else
			{
				mac	= this.context.Cipher.ComputeServerRecordMAC(contentType, fragment);
			}

			DebugHelper.WriteLine(">>>> Record MAC", mac);

			// Encrypt the message
			byte[] ecr = this.context.Cipher.EncryptRecord(fragment, mac);

			// Set new Client Cipher IV
			if (this.context.Cipher.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[this.context.Cipher.IvSize];
				Buffer.BlockCopy(ecr, ecr.Length - iv.Length, iv, 0, iv.Length);

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
			byte[]	dcrFragment		= null;
			byte[]	dcrMAC			= null;
			bool	badRecordMac	= false;

			try
			{
				this.context.Cipher.DecryptRecord(fragment, ref dcrFragment, ref dcrMAC);
			}
			catch
			{
				if (this.context is ServerContext)
				{
					this.Context.RecordProtocol.SendAlert(AlertDescription.DecryptionFailed);
				}

				throw;
			}
			
			// Generate record MAC
			byte[] mac = null;

			if (this.Context is ClientContext)
			{
				mac = this.context.Cipher.ComputeServerRecordMAC(contentType, dcrFragment);
			}
			else
			{
				mac = this.context.Cipher.ComputeClientRecordMAC(contentType, dcrFragment);
			}

			DebugHelper.WriteLine(">>>> Record MAC", mac);

			// Check record MAC
			if (mac.Length != dcrMAC.Length)
			{
				badRecordMac = true;
			}
			else
			{
				for (int i = 0; i < mac.Length; i++)
				{
					if (mac[i] != dcrMAC[i])
					{
						badRecordMac = true;
						break;
					}
				}
			}

			if (badRecordMac)
			{
				throw new TlsException(AlertDescription.BadRecordMAC, "Bad record MAC");
			}

			// Update sequence number
			this.context.ReadSequenceNumber++;

			return new TlsStream(dcrFragment);
		}

		#endregion
	}
}
