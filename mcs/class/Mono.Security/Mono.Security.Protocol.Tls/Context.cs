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
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Cryptography;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal abstract class Context
	{
		#region Internal Constants

		internal const short MAX_FRAGMENT_SIZE	= 16384; // 2^14
		internal const short TLS1_PROTOCOL_CODE = (0x03 << 8) | 0x01;
		internal const short SSL3_PROTOCOL_CODE = (0x03 << 8) | 0x00;
		internal const long  UNIX_BASE_TICKS	= 621355968000000000;

		#endregion

		#region Fields
		
		// Protocol version
		private SecurityProtocolType securityProtocol;
		
		// Sesison ID
		private byte[] sessionId;

		// Compression method
		private SecurityCompressionType compressionMethod;

		// Information sent and request by the server in the Handshake protocol
		private TlsServerSettings serverSettings;

		// Client configuration
		private TlsClientSettings clientSettings;

		// Cipher suite information
		private CipherSuite				cipher;
		private CipherSuiteCollection	supportedCiphers;

		// Last handshake message received
		private HandshakeType lastHandshakeMsg;

		// Handshake negotiation state
		private	HandshakeState handshakeState;

		// Misc
		private bool	isActual;
		private bool	connectionEnd;
		private bool	protocolNegotiated;
		
		// Sequence numbers
		private long	writeSequenceNumber;
		private long	readSequenceNumber;

		// Random data
		private byte[]	clientRandom;
		private byte[]	serverRandom;
		private byte[]	randomCS;
		private byte[]	randomSC;

		// Key information
		private byte[]	masterSecret;
		private byte[]	clientWriteMAC;
		private byte[]	serverWriteMAC;
		private byte[]	clientWriteKey;
		private byte[]	serverWriteKey;
		private byte[]	clientWriteIV;
		private byte[]	serverWriteIV;
		
		// Handshake hashes
		private TlsStream handshakeMessages;
		
		// Secure Random generator		
		private RandomNumberGenerator random;

		// Record protocol
		private RecordProtocol recordProtocol;

		#endregion

		#region Properties

		public bool	ProtocolNegotiated
		{
			get { return this.protocolNegotiated; }
			set { this.protocolNegotiated = value; }
		}

		public SecurityProtocolType SecurityProtocol
		{
			get 
			{
				if ((this.securityProtocol & SecurityProtocolType.Tls) == SecurityProtocolType.Tls ||	
					(this.securityProtocol & SecurityProtocolType.Default) == SecurityProtocolType.Default)
				{
					return SecurityProtocolType.Tls;
				}
				else
				{
					if ((this.securityProtocol & SecurityProtocolType.Ssl3) == SecurityProtocolType.Ssl3)
					{
						return SecurityProtocolType.Ssl3;
					}
				}

				throw new NotSupportedException("Unsupported security protocol type");
			}

			set { this.securityProtocol = value; }
		}

		public SecurityProtocolType SecurityProtocolFlags
		{
			get { return this.securityProtocol; }
		}

		public short Protocol
		{
			get 
			{ 
				switch (this.SecurityProtocol)
				{
					case SecurityProtocolType.Tls:
					case SecurityProtocolType.Default:
						return Context.TLS1_PROTOCOL_CODE;

					case SecurityProtocolType.Ssl3:
						return Context.SSL3_PROTOCOL_CODE;

					case SecurityProtocolType.Ssl2:
					default:
						throw new NotSupportedException("Unsupported security protocol type");
				}
			}
		}

		public byte[] SessionId
		{
			get { return this.sessionId; }
			set { this.sessionId = value; }
		}

		public SecurityCompressionType CompressionMethod
		{
			get { return this.compressionMethod; }
			set { this.compressionMethod = value; }
		}

		public TlsServerSettings ServerSettings
		{
			get { return this.serverSettings; }
		}

		public TlsClientSettings ClientSettings
		{
			get { return this.clientSettings; }
		}

		public bool	IsActual
		{
			get { return this.isActual; }
			set { this.isActual = value; }
		}

		public HandshakeType LastHandshakeMsg
		{
			get { return this.lastHandshakeMsg; }
			set { this.lastHandshakeMsg = value; }
		}

		public	HandshakeState HandshakeState
		{
			get { return this.handshakeState; }
			set { this.handshakeState = value; }
		}

		public bool ConnectionEnd
		{
			get { return this.connectionEnd; }
			set { this.connectionEnd = value; }
		}

		public CipherSuite Cipher
		{
			get { return this.cipher; }
			set 
			{ 
				this.cipher			= value; 
				this.cipher.Context = this;
			}
		}

		public CipherSuiteCollection SupportedCiphers
		{
			get { return supportedCiphers; }
			set { supportedCiphers = value; }
		}

		public TlsStream HandshakeMessages
		{
			get { return this.handshakeMessages; }
		}

		public long WriteSequenceNumber
		{
			get { return this.writeSequenceNumber; }
			set { this.writeSequenceNumber = value; }
		}

		public long ReadSequenceNumber
		{
			get { return this.readSequenceNumber; }
			set { this.readSequenceNumber = value; }
		}

		public byte[] ClientRandom
		{
			get { return this.clientRandom; }
			set { this.clientRandom = value; }
		}

		public byte[] ServerRandom
		{
			get { return this.serverRandom; }
			set { this.serverRandom = value; }
		}

		public byte[] RandomCS
		{
			get { return this.randomCS; }
			set { this.randomCS = value; }
		}

		public byte[] RandomSC
		{
			get { return this.randomSC; }
			set { this.randomSC = value; }
		}

		public byte[] MasterSecret
		{
			get { return this.masterSecret; }
			set { this.masterSecret = value; }
		}

		public byte[] ClientWriteMAC
		{
			get { return this.clientWriteMAC; }
			set { this.clientWriteMAC = value; }
		}

		public byte[] ServerWriteMAC
		{
			get { return this.serverWriteMAC; }
			set { this.serverWriteMAC = value; }
		}

		public byte[] ClientWriteKey
		{
			get { return this.clientWriteKey; }
			set { this.clientWriteKey = value; }
		}

		public byte[] ServerWriteKey
		{
			get { return this.serverWriteKey; }
			set { this.serverWriteKey = value; }
		}

		public byte[] ClientWriteIV
		{
			get { return this.clientWriteIV; }
			set { this.clientWriteIV = value; }
		}

		public byte[] ServerWriteIV
		{
			get { return this.serverWriteIV; }
			set { this.serverWriteIV = value; }
		}

		public RecordProtocol RecordProtocol
		{
			get { return this.recordProtocol; }
			set { this.recordProtocol = value; }
		}

		#endregion

		#region Constructors

		public Context(SecurityProtocolType securityProtocolType)
		{
			this.SecurityProtocol	= securityProtocolType;
			this.compressionMethod	= SecurityCompressionType.None;
			this.serverSettings		= new TlsServerSettings();
			this.clientSettings		= new TlsClientSettings();
			this.handshakeMessages	= new TlsStream();
			this.sessionId			= null;
			this.handshakeState		= HandshakeState.None;
			this.random				= RandomNumberGenerator.Create();
		}

		#endregion

		#region Methods
		
		public int GetUnixTime()
		{
			DateTime now = DateTime.UtcNow;
																		     
			return (int)(now.Ticks - UNIX_BASE_TICKS / TimeSpan.TicksPerSecond);
		}

		public byte[] GetSecureRandomBytes(int count)
		{
			byte[] secureBytes = new byte[count];

			this.random.GetNonZeroBytes(secureBytes);
			
			return secureBytes;
		}

		public virtual void Clear()
		{
			this.compressionMethod	= SecurityCompressionType.None;
			this.serverSettings		= new TlsServerSettings();
			this.clientSettings		= new TlsClientSettings();
			this.handshakeMessages	= new TlsStream();
			this.sessionId			= null;
			this.handshakeState		= HandshakeState.None;

			this.ClearKeyInfo();
		}

		public virtual void ClearKeyInfo()
		{
			// Clear Master Secret
			this.masterSecret	= null;

			// Clear client and server random
			this.clientRandom	= null;
			this.serverRandom	= null;
			this.randomCS		= null;
			this.randomSC		= null;

			// Clear client keys
			this.clientWriteKey	= null;
			this.clientWriteIV	= null;
			
			// Clear server keys
			this.serverWriteKey	= null;
			this.serverWriteIV	= null;

			// Reset handshake messages
			this.handshakeMessages.Reset();

			// Clear MAC keys if protocol is different than Ssl3
			if (this.securityProtocol != SecurityProtocolType.Ssl3)
			{
				this.clientWriteMAC = null;
				this.serverWriteMAC = null;
			}
		}

		public SecurityProtocolType DecodeProtocolCode(short code)
		{
			switch (code)
			{
				case Context.TLS1_PROTOCOL_CODE:
					return SecurityProtocolType.Tls;

				case Context.SSL3_PROTOCOL_CODE:
					return SecurityProtocolType.Ssl3;

				default:
					throw new NotSupportedException("Unsupported security protocol type");
			}
		}

		public void ChangeProtocol(short protocol)
		{
			SecurityProtocolType protocolType = this.DecodeProtocolCode(protocol);

			if ((protocolType & this.SecurityProtocolFlags) == protocolType ||
				(this.SecurityProtocolFlags & SecurityProtocolType.Default) == SecurityProtocolType.Default)
			{
				this.SecurityProtocol = protocolType;
				this.SupportedCiphers.Clear();
				this.SupportedCiphers = null;
				this.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(protocolType);
			}
			else
			{
				throw new TlsException(AlertDescription.ProtocolVersion, "Incorrect protocol version received from server");
			}
		}

		#endregion
	}
}
