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
using System.Text;
using System.Security.Cryptography;

using Mono.Security;
using Mono.Security.Cryptography;
using Mono.Security.X509;
using M = Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal abstract class CipherSuite
	{
		#region Static Fields

		public static byte[] EmptyArray = new byte[0];

		#endregion

		#region Fields

		private short					code;
		private string					name;
		private CipherAlgorithmType		cipherAlgorithmType;
		private HashAlgorithmType		hashAlgorithmType;
		private ExchangeAlgorithmType	exchangeAlgorithmType;
		private bool					isExportable;
		private CipherMode				cipherMode;
		private byte					keyMaterialSize;
		private int						keyBlockSize;
		private byte					expandedKeyMaterialSize;
		private short					effectiveKeyBits;
		private byte					ivSize;
		private byte					blockSize;
		private Context					context;
		private SymmetricAlgorithm		encryptionAlgorithm;
		private ICryptoTransform		encryptionCipher;
		private SymmetricAlgorithm		decryptionAlgorithm;
		private ICryptoTransform		decryptionCipher;
		private KeyedHashAlgorithm		clientHMAC;
		private KeyedHashAlgorithm		serverHMAC;
			
		#endregion

		#region Protected Properties

		protected ICryptoTransform EncryptionCipher
		{
			get { return this.encryptionCipher; }
		}

		protected ICryptoTransform DecryptionCipher
		{
			get { return this.decryptionCipher; }
		}

		protected KeyedHashAlgorithm ClientHMAC
		{
			get { return this.clientHMAC; }
		}
		
		protected KeyedHashAlgorithm ServerHMAC
		{
			get { return this.serverHMAC; }
		}

		#endregion

		#region Properties

		public CipherAlgorithmType CipherAlgorithmType
		{
			get { return this.cipherAlgorithmType; }
		}

		public string HashAlgorithmName
		{
			get 
			{  
				switch (this.hashAlgorithmType)
				{
					case HashAlgorithmType.Md5:
						return "MD5";

					case HashAlgorithmType.Sha1:
						return "SHA1";

					default:
						return "None";
				}
			}
		}

		public HashAlgorithmType HashAlgorithmType
		{
			get { return this.hashAlgorithmType; }
		}

		public int HashSize
		{
			get 
			{ 
				switch (this.hashAlgorithmType)
				{
					case HashAlgorithmType.Md5:
						return 16;

					case HashAlgorithmType.Sha1:
						return 20;

					default:
						return 0;
				}
			}	
		}
		
		public ExchangeAlgorithmType ExchangeAlgorithmType
		{
			get { return this.exchangeAlgorithmType; }
		}

		public CipherMode CipherMode
		{
			get { return this.cipherMode; }
		}

		public short Code
		{
			get { return this.code; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public bool IsExportable
		{
			get { return this.isExportable; }
		}

		public byte	KeyMaterialSize
		{
			get { return this.keyMaterialSize; }
		}

		public int KeyBlockSize
		{
			get { return this.keyBlockSize; }
		}

		public byte	ExpandedKeyMaterialSize
		{
			get { return this.expandedKeyMaterialSize; }
		}

		public byte	EffectiveKeyBits
		{
			get { return this.EffectiveKeyBits; }
		}
		
		public byte IvSize
		{
			get { return this.ivSize; }
		}

		/*
		public byte	BlockSize
		{
			get { return this.blockSize; }
		}
		*/

		public Context Context
		{
			get { return this.context; }
			set 
			{ 
				this.context = value; 
			}
		}

		#endregion

		#region Constructors
		
		public CipherSuite(
			short code, string name, CipherAlgorithmType cipherAlgorithmType, 
			HashAlgorithmType hashAlgorithmType, ExchangeAlgorithmType exchangeAlgorithmType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize)
		{
			this.code					= code;
			this.name					= name;
			this.cipherAlgorithmType	= cipherAlgorithmType;
			this.hashAlgorithmType		= hashAlgorithmType;
			this.exchangeAlgorithmType	= exchangeAlgorithmType;
			this.isExportable			= exportable;
			if (blockMode)
			{
				this.cipherMode			= CipherMode.CBC;
			}
			this.keyMaterialSize		= keyMaterialSize;
			this.expandedKeyMaterialSize= expandedKeyMaterialSize;
			this.effectiveKeyBits		= effectiveKeyBits;
			this.ivSize					= ivSize;
			this.blockSize				= blockSize;
			this.keyBlockSize			= (this.keyMaterialSize + this.HashSize + this.ivSize) << 1;
		}

		#endregion

		#region Methods

		public void InitializeCipher()
		{
			this.createEncryptionCipher();
			this.createDecryptionCipher();
		}

		public void UpdateClientCipherIV(byte[] iv)
		{
			if (this.cipherMode == CipherMode.CBC)
			{
				// Set the new IV
				this.encryptionAlgorithm.IV	= iv;
			
				// Create encryption cipher with the new IV
				this.encryptionCipher = this.encryptionAlgorithm.CreateEncryptor();
			}
		}

		/*
		public void UpdateServerCipherIV(byte[] iv)
		{
			if (this.cipherMode == CipherMode.CBC)
			{
				// Set the new IV
				this.decryptionAlgorithm.IV	= iv;
			
				// Create encryption cipher with the new IV
				this.decryptionCipher = this.decryptionAlgorithm.CreateDecryptor();
			}
		}
		*/

		public byte[] EncryptRecord(byte[] fragment, byte[] mac)
		{
			// Encryption ( fragment + mac [+ padding + padding_length] )
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, this.EncryptionCipher, CryptoStreamMode.Write);

			cs.Write(fragment, 0, fragment.Length);
			cs.Write(mac, 0, mac.Length);
			if (this.CipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				byte fragmentLength	= (byte)(fragment.Length + mac.Length + 1);
				byte paddingLength	= (byte)(this.blockSize - fragmentLength % this.blockSize);
				if (paddingLength == this.blockSize)
				{
					paddingLength = 0;
				}

				// Write padding length byte
				byte[] padding = new byte[(paddingLength + 1)];				
				for (int i = 0; i < (paddingLength + 1); i++)
				{
					padding[i] = paddingLength;
				}

				cs.Write(padding, 0, padding.Length);
			}
			cs.FlushFinalBlock();
			cs.Close();

			return ms.ToArray();
		}

		public void DecryptRecord(byte[] fragment, ref byte[] dcrFragment, ref byte[] dcrMAC)
		{
			int	fragmentSize	= 0;
			int paddingLength	= 0;

			// Decrypt message fragment ( fragment + mac [+ padding + padding_length] )
			byte[] buffer = new byte[fragment.Length];
			this.DecryptionCipher.TransformBlock(fragment, 0, fragment.Length, buffer, 0);

			// Calculate fragment size
			if (this.CipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				paddingLength	= buffer[buffer.Length - 1];
				fragmentSize	= (buffer.Length - (paddingLength + 1)) - this.HashSize;
			}
			else
			{
				fragmentSize = buffer.Length - this.HashSize;
			}

			dcrFragment = new byte[fragmentSize];
			dcrMAC		= new byte[HashSize];

			Buffer.BlockCopy(buffer, 0, dcrFragment, 0, dcrFragment.Length);
			Buffer.BlockCopy(buffer, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
		}

		#endregion

		#region Abstract Methods

		public abstract byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment);

		public abstract byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment);

		public abstract void ComputeMasterSecret(byte[] preMasterSecret);

		public abstract void ComputeKeys();

		#endregion

		#region Key Generation Methods

		public byte[] CreatePremasterSecret()
		{
			TlsStream		stream	= new TlsStream();
			ClientContext	context = (ClientContext)this.context;
			
			// Write protocol version
			// We need to send here the protocol version used in 
			// the ClientHello message, that can be different than the actual
			// protocol version
			stream.Write(context.ClientHelloProtocol);

			// Generate random bytes
			stream.Write(this.context.GetSecureRandomBytes(46));

			byte[] preMasterSecret = stream.ToArray();

			stream.Reset();

			return preMasterSecret;
		}

		public byte[] PRF(byte[] secret, string label, byte[] data, int length)
		{
			HashAlgorithm md5	= MD5.Create();
			HashAlgorithm sha1	= SHA1.Create();

			/* Secret Length calc exmplain from the RFC2246. Section 5
			 * 
			 * S1 and S2 are the two halves of the secret and each is the same
			 * length. S1 is taken from the first half of the secret, S2 from the
			 * second half. Their length is created by rounding up the length of the
			 * overall secret divided by two; thus, if the original secret is an odd
			 * number of bytes long, the last byte of S1 will be the same as the
			 * first byte of S2.
			 */

			// split secret in 2
			int secretLen = secret.Length >> 1;
			// rounding up
			if ((secret.Length & 0x1) == 0x1)
				secretLen++;

			// Seed
			TlsStream seedStream = new TlsStream();
			seedStream.Write(Encoding.ASCII.GetBytes(label));
			seedStream.Write(data);
			byte[] seed = seedStream.ToArray();
			seedStream.Reset();

			// Secret 1
			byte[] secret1 = new byte[secretLen];
			Buffer.BlockCopy(secret, 0, secret1, 0, secretLen);

			// Secret2
			byte[] secret2 = new byte[secretLen];
			Buffer.BlockCopy(secret, (secret.Length - secretLen), secret2, 0, secretLen);

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
			
			M.HMAC		hmac	= new M.HMAC(hashName, secret);
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
			
			Buffer.BlockCopy(resMacs.ToArray(), 0, res, 0, res.Length);

			resMacs.Reset();

			return res;
		}

		#endregion

		#region Private Methods

		private void createEncryptionCipher()
		{
			// Create and configure the symmetric algorithm
			switch (this.cipherAlgorithmType)
			{
				case CipherAlgorithmType.Des:
					this.encryptionAlgorithm = DES.Create();
					break;

				case CipherAlgorithmType.Rc2:
					this.encryptionAlgorithm = RC2.Create();
					break;

				case CipherAlgorithmType.Rc4:
					this.encryptionAlgorithm = new ARC4Managed();
					break;

				case CipherAlgorithmType.TripleDes:
					this.encryptionAlgorithm = TripleDES.Create();
					break;

				case CipherAlgorithmType.Rijndael:
					this.encryptionAlgorithm = Rijndael.Create();
					break;
			}

			// If it's a block cipher
			if (this.cipherMode == CipherMode.CBC)
			{
				// Configure encrypt algorithm
				this.encryptionAlgorithm.Mode		= this.cipherMode;
				this.encryptionAlgorithm.Padding	= PaddingMode.None;
				this.encryptionAlgorithm.KeySize	= this.expandedKeyMaterialSize * 8;
				this.encryptionAlgorithm.BlockSize	= this.blockSize * 8;
			}

			// Set the key and IV for the algorithm
			if (this.context is ClientContext)
			{
				this.encryptionAlgorithm.Key	= this.context.ClientWriteKey;
				this.encryptionAlgorithm.IV		= this.context.ClientWriteIV;
			}
			else
			{
				this.encryptionAlgorithm.Key	= this.context.ServerWriteKey;
				this.encryptionAlgorithm.IV		= this.context.ServerWriteIV;
			}
			
			// Create encryption cipher
			this.encryptionCipher = this.encryptionAlgorithm.CreateEncryptor();

			// Create the HMAC algorithm
			if (this.context is ClientContext)
			{
				this.clientHMAC = new M.HMAC(
					this.HashAlgorithmName,
					this.context.ClientWriteMAC);
			}
			else
			{
				this.serverHMAC = new M.HMAC(
					this.HashAlgorithmName,
					this.context.ServerWriteMAC);
			}
		}

		private void createDecryptionCipher()
		{
			// Create and configure the symmetric algorithm
			switch (this.cipherAlgorithmType)
			{
				case CipherAlgorithmType.Des:
					this.decryptionAlgorithm = DES.Create();
					break;

				case CipherAlgorithmType.Rc2:
					this.decryptionAlgorithm = RC2.Create();
					break;

				case CipherAlgorithmType.Rc4:
					this.decryptionAlgorithm = new ARC4Managed();
					break;

				case CipherAlgorithmType.TripleDes:
					this.decryptionAlgorithm = TripleDES.Create();
					break;

				case CipherAlgorithmType.Rijndael:
					this.decryptionAlgorithm = Rijndael.Create();
					break;
			}

			// If it's a block cipher
			if (this.cipherMode == CipherMode.CBC)
			{
				// Configure encrypt algorithm
				this.decryptionAlgorithm.Mode		= this.cipherMode;
				this.decryptionAlgorithm.Padding	= PaddingMode.None;
				this.decryptionAlgorithm.KeySize	= this.expandedKeyMaterialSize * 8;
				this.decryptionAlgorithm.BlockSize	= this.blockSize * 8;
			}

			// Set the key and IV for the algorithm
			if (this.context is ClientContext)
			{
				this.decryptionAlgorithm.Key	= this.context.ServerWriteKey;
				this.decryptionAlgorithm.IV		= this.context.ServerWriteIV;
			}
			else
			{
				this.decryptionAlgorithm.Key	= this.context.ClientWriteKey;
				this.decryptionAlgorithm.IV		= this.context.ClientWriteIV;
			}

			// Create decryption cipher			
			this.decryptionCipher = this.decryptionAlgorithm.CreateDecryptor();

			// Create the HMAC
			if (this.context is ClientContext)
			{
				this.serverHMAC = new M.HMAC(
					this.HashAlgorithmName,
					this.context.ServerWriteMAC);
			}
			else
			{
				this.clientHMAC = new M.HMAC(
					this.HashAlgorithmName,
					this.context.ClientWriteMAC);
			}
		}

		#endregion
	}
}