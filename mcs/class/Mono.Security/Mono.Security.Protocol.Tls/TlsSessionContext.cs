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

		// Information sent and request by the server in the Handshake protocol
		private TlsServerSettings	serverSettings;

		// Misc
		private bool				isActual;
		private bool				connectionEnd;
		private TlsCipherSuite		cipher;
		private int					compressionMethod;

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

		#region PROPERTIES

		public TlsProtocol Protocol
		{
			get { return protocol; }
			set { protocol = value; }
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

		public int CompressionMethod
		{
			get { return compressionMethod; }
			set { compressionMethod = value; }
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
			this.serverSettings		= new TlsServerSettings();
			this.handshakeHashes	= new TlsHandshakeHashes();
		}

		#endregion

		#region KEY_GENERATION_METODS

		public byte[] GetSecureRandomBytes(int count)
		{
			byte[] secureBytes = new byte[count];

			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetNonZeroBytes(secureBytes);
			
			return secureBytes;
		}

		public int GetUnixTime()
		{
			DateTime now		= DateTime.Now.ToUniversalTime();
			TimeSpan unixTime	= now.Subtract(new DateTime(1970, 1, 1));

			return (int)unixTime.TotalSeconds;
		}

		public byte[] CreatePremasterSecret()
		{
			TlsStream stream = new TlsStream();

			// Write protocol version
			stream.Write((short)protocol);

			// Generate random bytes
			stream.Write(GetSecureRandomBytes(46));

			byte[] preMasterSecret = stream.ToArray();

			stream.Reset();

			return preMasterSecret;
		}

		public void CreateMasterSecret(byte[] preMasterSecret)
		{
			TlsCipherSuite	cipherSuite	= cipher;			
			TlsStream		seed		= new TlsStream();

			// Seed
			seed.Write(clientRandom);
			seed.Write(serverRandom);

			// Create master secret
			masterSecret = new byte[preMasterSecret.Length];
			masterSecret = PRF(preMasterSecret, "master secret", seed.ToArray(), 48);

			seed.Reset();
		}

		public void CreateKeys()
		{
			TlsStream seed = new TlsStream();

			// Seed
			seed.Write(serverRandom);
			seed.Write(clientRandom);

			// Create keyblock
			TlsStream keyBlock = new TlsStream(
				PRF(masterSecret, 
				"key expansion",
				seed.ToArray(), 
				cipher.GetKeyBlockSize()));

			clientWriteMAC = keyBlock.ReadBytes(cipher.HashSize);
			serverWriteMAC = keyBlock.ReadBytes(cipher.HashSize);
			clientWriteKey = keyBlock.ReadBytes(cipher.KeyMaterialSize);
			serverWriteKey = keyBlock.ReadBytes(cipher.KeyMaterialSize);

			if (!cipher.IsExportable)
			{
				if (cipher.IvSize != 0)
				{
					clientWriteIV = keyBlock.ReadBytes(cipher.IvSize);
					serverWriteIV = keyBlock.ReadBytes(cipher.IvSize);
				}
				else
				{
					clientWriteIV = new byte[0];
					serverWriteIV = new byte[0];
				}
			}
			else
			{
				// Seed
				seed.Reset();
				seed.Write(clientRandom);
				seed.Write(serverRandom);

				// Generate final write keys
				byte[] finalClientWriteKey	= PRF(clientWriteKey, "client write key", seed.ToArray(), cipher.KeyMaterialSize);
				byte[] finalServerWriteKey	= PRF(serverWriteKey, "server write key", seed.ToArray(), cipher.KeyMaterialSize);
				
				clientWriteKey	= finalClientWriteKey;
				serverWriteKey	= finalServerWriteKey;

				// Generate IV block
				byte[] ivBlock = PRF(new byte[]{}, "IV block", seed.ToArray(), cipher.IvSize*2);

				// Generate IV keys
				clientWriteIV = new byte[cipher.IvSize];				
				System.Array.Copy(ivBlock, 0, clientWriteIV, 0, clientWriteIV.Length);
				serverWriteIV = new byte[cipher.IvSize];
				System.Array.Copy(ivBlock, cipher.IvSize, serverWriteIV, 0, serverWriteIV.Length);
			}

			// Clear no more needed data
			seed.Reset();
			keyBlock.Reset();
		}

		public byte[] PRF(byte[] secret, string label, byte[] data, int length)
		{
			MD5CryptoServiceProvider	md5	= new MD5CryptoServiceProvider();
			SHA1CryptoServiceProvider	sha1 = new SHA1CryptoServiceProvider();

			int secretLen = secret.Length / 2;

			// Seed
			TlsStream seedStream = new TlsStream();
			seedStream.Write(Encoding.ASCII.GetBytes(label));
			seedStream.Write(data);
			byte[] seed = seedStream.ToArray();
			seedStream.Reset();

			// Secret 1
			byte[] secret1 = new byte[secretLen];
			System.Array.Copy(secret, 0, secret1, 0, secretLen);

			// Secret2
			byte[] secret2 = new byte[secretLen];
			System.Array.Copy(secret, secretLen, secret2, 0, secretLen);

			// Secret 1 processing
			byte[] p_md5 = Expand("MD5", secret1, seed, length);

			// Secret 2 processing
			byte[] p_sha = Expand("SHA1", secret2, seed, length);

			// Perfor XOR of both results
			byte[] masterSecret = new byte[length];
			for (int i = 0; i < masterSecret.Length; i++)
			{
				masterSecret[i] = (byte)(p_md5[i] ^ p_sha[i]);
			}

			return masterSecret;
		}
		
		public byte[] Expand(string hashName, byte[] secret, byte[] seed, int length)
		{
			int hashLength	= hashName == "MD5" ? 16 : 20;
			int	iterations	= (int)(length / hashLength);
			if ((length % hashLength) > 0)
			{
				iterations++;
			}
			
			HMAC		hmac	= new HMAC(hashName, secret);
			TlsStream	resMacs	= new TlsStream();
			
			byte[][] hmacs = new byte[iterations + 1][];
			hmacs[0] = seed;
			for (int i = 1; i <= iterations; i++)
			{				
				TlsStream hcseed = new TlsStream();
				hmac.TransformFinalBlock(hmacs[i-1], 0, hmacs[i-1].Length);
				hmacs[i] = hmac.Hash;
				hcseed.Write(hmacs[i]);
				hcseed.Write(seed);
				hmac.TransformFinalBlock(hcseed.ToArray(), 0, (int)hcseed.Length);
				resMacs.Write(hmac.Hash);
				hcseed.Reset();
			}

			byte[] res = new byte[length];
			
			System.Array.Copy(resMacs.ToArray(), 0, res, 0, res.Length);

			resMacs.Reset();

			return res;
		}

		#endregion

		#region METHODS
		
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
