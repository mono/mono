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
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsSessionContext
	{
		#region FIELDS

		// Protocol version
		private TlsProtocol			protocol;

		// Compression method
		private TlsCompressionMethod compressionMethod;

		// Information sent and request by the server in the Handshake protocol
		private TlsServerSettings	serverSettings;

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

		public TlsProtocol Protocol
		{
			get { return this.protocol; }
			set { this.protocol = value; }
		}

		public TlsCompressionMethod CompressionMethod
		{
			get { return this.compressionMethod; }
			set { this.compressionMethod = value; }
		}

		public TlsServerSettings ServerSettings
		{
			get { return this.serverSettings; }
			set { this.serverSettings = value; }
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

		public TlsSessionContext()
		{
			this.protocol			= TlsProtocol.Tls1;
			this.compressionMethod	= TlsCompressionMethod.None;
			this.serverSettings		= new TlsServerSettings();
			this.handshakeMessages	= new TlsStream();
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
			if (this.protocol != TlsProtocol.Ssl3)
			{
				this.clientWriteMAC = null;
				this.serverWriteMAC = null;
			}
		}

		#endregion
	}
}
