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
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Cryptography;
using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsContext
	{
		#region FIELDS
		
		// SslClientStream that owns the context
		private SslClientStream	sslStream;

		// Protocol version
		private SecurityProtocolType protocol;

		// Sesison ID
		private byte[] sessionId;

		// Compression method
		private SecurityCompressionType compressionMethod;

		// Information sent and request by the server in the Handshake protocol
		private TlsServerSettings	serverSettings;

		// Client configuration
		private TlsClientSettings	clientSettings;

		// Cipher suite information
		private CipherSuite					cipher;
		private TlsCipherSuiteCollection	supportedCiphers;

		// Misc
		private bool				isActual;
		private bool				helloDone;
		private	bool				handshakeFinished;
		private bool				connectionEnd;
		
		// Sequence numbers
		private long				writeSequenceNumber;
		private long				readSequenceNumber;

		// Random data
		private byte[]				clientRandom;
		private byte[]				serverRandom;
		private byte[]				randomCS;
		private byte[]				randomSC;

		// Key information
		private byte[]				masterSecret;
		private byte[]				clientWriteMAC;
		private byte[]				serverWriteMAC;
		private byte[]				clientWriteKey;
		private byte[]				serverWriteKey;
		private byte[]				clientWriteIV;
		private byte[]				serverWriteIV;
		
		// Handshake hashes
		private TlsStream			handshakeMessages;
		
		#endregion

		#region INTERNAL_CONSTANTS

		internal const short MAX_FRAGMENT_SIZE = 16384; // 2^14

		#endregion

		#region PROPERTIES

		public SslClientStream SslStream
		{
			get { return sslStream; }
		}

		public SecurityProtocolType Protocol
		{
			get { return this.protocol; }
			set { this.protocol = value; }
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
			set { this.serverSettings = value; }
		}

		public TlsClientSettings ClientSettings
		{
			get { return this.clientSettings; }
			set { this.clientSettings = value; }
		}

		public bool	IsActual
		{
			get { return this.isActual; }
			set { this.isActual = value; }
		}

		public bool HelloDone
		{
			get { return helloDone; }
			set { helloDone = value; }
		}

		public bool HandshakeFinished
		{
			get { return handshakeFinished; }
			set { handshakeFinished = value; }
		}

		public bool ConnectionEnd
		{
			get { return this.connectionEnd; }
			set { this.connectionEnd = value; }
		}

		public CipherSuite Cipher
		{
			get { return this.cipher; }
			set { this.cipher = value; }
		}

		public TlsCipherSuiteCollection SupportedCiphers
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

		#endregion

		#region CONSTRUCTORS

		public TlsContext(
			SslClientStream sslStream,
			SecurityProtocolType securityProtocolType,
			string targetHost,
			X509CertificateCollection clientCertificates)
		{
			this.sslStream			= sslStream;
			this.protocol			= securityProtocolType;
			this.compressionMethod	= SecurityCompressionType.None;
			this.serverSettings		= new TlsServerSettings();
			this.clientSettings		= new TlsClientSettings();
			this.handshakeMessages	= new TlsStream();
			this.sessionId			= null;

			// Set client settings
			this.ClientSettings.TargetHost		= targetHost;
			this.ClientSettings.Certificates	= clientCertificates;
		}

		#endregion

		#region METHODS
		
		public int GetUnixTime()
		{
			DateTime now		= DateTime.Now.ToUniversalTime();
			TimeSpan unixTime	= now.Subtract(new DateTime(1970, 1, 1));

			return (int)unixTime.TotalSeconds;
		}

		public byte[] GetSecureRandomBytes(int count)
		{
			byte[] secureBytes = new byte[count];

			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetNonZeroBytes(secureBytes);
			
			return secureBytes;
		}

		public void ClearKeyInfo()
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

			// Clear MAC keys if protocol is different than Ssl3
			if (this.protocol != SecurityProtocolType.Ssl3)
			{
				this.clientWriteMAC = null;
				this.serverWriteMAC = null;
			}
		}

		#endregion

		#region EXCEPTION_METHODS

		internal TlsException CreateException(TlsAlertLevel alertLevel, TlsAlertDescription alertDesc)
		{
			return CreateException(TlsAlert.GetAlertMessage(alertDesc));
		}

		internal TlsException CreateException(string format, params object[] args)
		{
			StringBuilder message = new StringBuilder();
			message.AppendFormat(format, args);

			return CreateException(message.ToString());
		}

		internal TlsException CreateException(string message)
		{
			return new TlsException(message);
		}

		#endregion
	}
}
