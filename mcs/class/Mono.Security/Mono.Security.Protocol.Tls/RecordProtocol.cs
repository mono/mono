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
			if (type == -1)
			{
				return null;
			}

			ContentType	contentType	= (ContentType)type;
			byte[] buffer = this.ReadRecordBuffer(type);

			TlsStream message = new TlsStream(buffer);
		
			// Decrypt message contents if needed
			if (contentType == ContentType.Alert && buffer.Length == 2)
			{
			}
			else
			{
				if (this.context.IsActual && contentType != ContentType.ChangeCipherSpec)
				{
					message = this.decryptRecordFragment(contentType, message.ToArray());

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
					this.ProcessAlert((AlertLevel)message.ReadByte(), (AlertDescription)message.ReadByte());
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

// FIXME / MCS bug - http://bugzilla.ximian.com/show_bug.cgi?id=67711
//				case (ContentType)0x80:
//					this.context.HandshakeMessages.Write (result);
//					break;

				default:
					if (contentType != (ContentType)0x80)
					{
						throw new TlsException(
							AlertDescription.UnexpectedMessage,
							"Unknown record received from server.");
					}
					this.context.HandshakeMessages.Write (result);
					break;
			}

			return result;
		}

		private byte[] ReadRecordBuffer(int contentType)
		{
			switch (contentType)
			{
				case 0x80:
					return this.ReadClientHelloV2();

				default:
					if (!Enum.IsDefined(typeof(ContentType), (ContentType)contentType))
					{
						throw new TlsException(AlertDescription.DecodeError);
					}
					return this.ReadStandardRecordBuffer();
			}
		}

		private byte[] ReadClientHelloV2()
		{
			int msgLength			= this.innerStream.ReadByte();
			byte[] message = new byte [msgLength];
			this.innerStream.Read (message, 0, msgLength);

			int msgType		= message [0];
			if (msgType != 1)
			{
				throw new TlsException(AlertDescription.DecodeError);
			}
			int protocol = (message [1] << 8 | message [2]);
			int cipherSpecLength = (message [3] << 8 | message [4]);
			int sessionIdLength = (message [5] << 8 | message [6]);
			int challengeLength = (message [7] << 8 | message [8]);
			int length = (challengeLength > 32) ? 32 : challengeLength;

			// Read CipherSpecs
			byte[] cipherSpecV2 = new byte[cipherSpecLength];
			Buffer.BlockCopy (message, 9, cipherSpecV2, 0, cipherSpecLength);

			// Read session ID
			byte[] sessionId = new byte[sessionIdLength];
			Buffer.BlockCopy (message, 9 + cipherSpecLength, sessionId, 0, sessionIdLength);

			// Read challenge ID
			byte[] challenge = new byte[challengeLength];
			Buffer.BlockCopy (message, 9 + cipherSpecLength + sessionIdLength, challenge, 0, challengeLength);
		
			if (challengeLength < 16 || cipherSpecLength == 0 || (cipherSpecLength % 3) != 0)
			{
				throw new TlsException(AlertDescription.DecodeError);
			}

			// Updated the Session ID
			if (sessionId.Length > 0)
			{
				this.context.SessionId = sessionId;
			}

			// Update the protocol version
			this.Context.ChangeProtocol((short)protocol);

			// Select the Cipher suite
			this.ProcessCipherSpecV2Buffer(this.Context.SecurityProtocol, cipherSpecV2);

			// Updated the Client Random
			this.context.ClientRandom = new byte [32]; // Always 32
			// 1. if challenge is bigger than 32 bytes only use the last 32 bytes
			// 2. right justify (0) challenge in ClientRandom if less than 32
			Buffer.BlockCopy (challenge, challenge.Length - length, this.context.ClientRandom, 32 - length, length);

			// Set 
			this.context.LastHandshakeMsg = HandshakeType.ClientHello;
			this.context.ProtocolNegotiated = true;

			return message;
		}

		private byte[] ReadStandardRecordBuffer()
		{
			short protocol	= this.ReadShort();
			short length	= this.ReadShort();
			
			// Read Record data
			int		received	= 0;
			byte[]	buffer		= new byte[length];
			while (received != length)
			{
				received += this.innerStream.Read(buffer, received, buffer.Length - received);
			}

			// Check that the message has a valid protocol version
			if (protocol != this.context.Protocol && this.context.ProtocolNegotiated)
			{
				throw new TlsException(
					AlertDescription.ProtocolVersion, "Invalid protocol version on message received");
			}

			DebugHelper.WriteLine("Record data", buffer);

			return buffer;
		}

		private short ReadShort()
		{
			byte[] b = new byte[2];
			this.innerStream.Read(b, 0, b.Length);

			short val = BitConverter.ToInt16(b, 0);

			return System.Net.IPAddress.HostToNetworkOrder(val);
		}

		private void ProcessAlert(AlertLevel alertLevel, AlertDescription alertDesc)
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

		#region CipherSpecV2 processing

		private void ProcessCipherSpecV2Buffer(SecurityProtocolType protocol, byte[] buffer)
		{
			TlsStream codes = new TlsStream(buffer);

			string prefix = (protocol == SecurityProtocolType.Ssl3) ? "SSL_" : "TLS_";

			while (codes.Position < codes.Length)
			{
				byte check = codes.ReadByte();

				if (check == 0)
				{
					// SSL/TLS cipher spec
					int index = 0;
					short code = codes.ReadInt16();					
					if ((index = this.Context.SupportedCiphers.IndexOf(code)) != -1)
					{
						this.Context.Cipher	= this.Context.SupportedCiphers[index];
						break;
					}
				}
				else
				{
					byte[] tmp = new byte[2];
					codes.Read(tmp, 0, tmp.Length);

					int tmpCode = ((check & 0xff) << 16) | ((tmp[0] & 0xff) << 8) | (tmp[1] & 0xff);
					CipherSuite cipher = this.MapV2CipherCode(prefix, tmpCode);

					if (cipher != null)
					{
						this.Context.Cipher = cipher;
						break;
					}
				}
			}

			if (this.Context.Cipher == null)
			{
				throw new TlsException(AlertDescription.InsuficientSecurity, "Insuficient Security");
			}
		}

		private CipherSuite MapV2CipherCode(string prefix, int code)
		{
			try
			{
				switch (code)
				{
					case 65664:
						// TLS_RC4_128_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_WITH_RC4_128_MD5"];
					
					case 131200:
						// TLS_RC4_128_EXPORT40_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC4_40_MD5"];
					
					case 196736:
						// TLS_RC2_CBC_128_CBC_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
					
					case 262272:
						// TLS_RC2_CBC_128_CBC_EXPORT40_WITH_MD5
						return this.Context.SupportedCiphers[prefix + "RSA_EXPORT_WITH_RC2_CBC_40_MD5"];
					
					case 327808:
						// TLS_IDEA_128_CBC_WITH_MD5
						return null;
					
					case 393280:
						// TLS_DES_64_CBC_WITH_MD5
						return null;

					case 458944:
						// TLS_DES_192_EDE3_CBC_WITH_MD5
						return null;

					default:
						return null;
				}
			}
			catch
			{
				return null;
			}
		}

		#endregion
	}
}
