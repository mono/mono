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

		// Misc
		private bool				isActual;
		private bool				connectionEnd;
		private TlsCipherSuite		cipher;

		// Sequence numbers
		private long				writeSequenceNumber;
		private long				readSequenceNumber;

		// Random data
		private byte[]				clientRandom;
		private byte[]				serverRandom;

		// Key information
		private byte[]				masterSecret;
		private byte[]				clientWriteMAC;
		private byte[]				serverWriteMAC;
		private byte[]				clientWriteKey;
		private byte[]				serverWriteKey;
		private byte[]				clientWriteIV;
		private byte[]				serverWriteIV;
		
		// Handshake hashes
		private TlsHandshakeHashes	handshakeHashes;
		
		#endregion

		#region INTERNAL_CONSTANTS

		internal const short MAX_FRAGMENT_SIZE = 16384; // 2^14

		#endregion

		#region PROPERTIES

		public TlsProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
		}

		public TlsCompressionMethod CompressionMethod
		{
			get { return compressionMethod; }
			set { compressionMethod = value; }
		}

		public TlsServerSettings ServerSettings
		{
			get { return serverSettings; }
			set { serverSettings = value; }
		}

		public bool	IsActual
		{
			get { return isActual; }
			set { isActual = value; }
		}

		public bool ConnectionEnd
		{
			get { return connectionEnd; }
			set { connectionEnd = value; }
		}

		public TlsCipherSuite Cipher
		{
			get { return cipher; }
			set { cipher = value; }
		}

		public TlsHandshakeHashes HandshakeHashes
		{
			get { return handshakeHashes; }
		}

		public long WriteSequenceNumber
		{
			get { return writeSequenceNumber; }
			set { writeSequenceNumber = value; }
		}

		public long ReadSequenceNumber
		{
			get { return readSequenceNumber; }
			set { readSequenceNumber = value; }
		}

		public byte[] ClientRandom
		{
			get { return clientRandom; }
			set { clientRandom = value; }
		}

		public byte[] ServerRandom
		{
			get { return serverRandom; }
			set { serverRandom = value; }
		}

		public byte[] MasterSecret
		{
			get { return masterSecret; }
			set { masterSecret = value; }
		}

		public byte[] ClientWriteMAC
		{
			get { return clientWriteMAC; }
			set { clientWriteMAC = value; }
		}

		public byte[] ServerWriteMAC
		{
			get { return serverWriteMAC; }
			set { serverWriteMAC = value; }
		}

		public byte[] ClientWriteKey
		{
			get { return clientWriteKey; }
			set { clientWriteKey = value; }
		}

		public byte[] ServerWriteKey
		{
			get { return serverWriteKey; }
			set { serverWriteKey = value; }
		}

		public byte[] ClientWriteIV
		{
			get { return clientWriteIV; }
			set { clientWriteIV = value; }
		}

		public byte[] ServerWriteIV
		{
			get { return serverWriteIV; }
			set { serverWriteIV = value; }
		}

		#endregion

		#region CONSTRUCTORS

		public TlsSessionContext()
		{
			this.protocol			= TlsProtocol.Tls1;
			this.compressionMethod	= TlsCompressionMethod.None;
			this.serverSettings		= new TlsServerSettings();
			this.handshakeHashes	= new TlsHandshakeHashes();
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
			masterSecret	= null;

			// Clear client and server random
			clientRandom	= null;
			serverRandom	= null;

			// Clear client keys
			clientWriteKey	= null;
			clientWriteIV	= null;
			clientWriteMAC	= null;

			// Clear server keys
			serverWriteKey	= null;
			serverWriteIV	= null;
			serverWriteMAC	= null;

			// Force the GC to recollect the memory ??
		}

		#endregion
	}
}
