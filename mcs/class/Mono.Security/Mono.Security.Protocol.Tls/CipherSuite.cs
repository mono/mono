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
using System.Text;
using System.Security.Cryptography;

using Mono.Security;
using Mono.Security.Cryptography;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls
{
	internal abstract class CipherSuite
	{
		#region FIELDS

		private short				code;
		private string				name;
		private string				algName;
		private string				hashName;
		private bool				isExportable;
		private CipherMode			cipherMode;
		private byte				keyMaterialSize;
		private byte				expandedKeyMaterialSize;
		private short				effectiveKeyBits;
		private byte				ivSize;
		private byte				blockSize;
		private TlsSessionContext	context;
		private SymmetricAlgorithm	encryptionAlgorithm;
		private ICryptoTransform	encryptionCipher;
		private SymmetricAlgorithm	decryptionAlgorithm;
		private ICryptoTransform	decryptionCipher;
		private KeyedHashAlgorithm	clientHMAC;
		private KeyedHashAlgorithm	serverHMAC;
			
		#endregion

		#region PROTECTED_PROPERTIES

		protected ICryptoTransform EncryptionCipher
		{
			get { return encryptionCipher; }
		}

		protected ICryptoTransform DecryptionCipher
		{
			get { return decryptionCipher; }
		}

		protected KeyedHashAlgorithm ClientHMAC
		{
			get { return clientHMAC; }
		}
		
		protected KeyedHashAlgorithm ServerHMAC
		{
			get { return serverHMAC; }
		}

		#endregion

		#region PROPERTIES

		public short Code
		{
			get { return code; }
		}

		public string Name
		{
			get { return name; }
		}

		public bool IsExportable
		{
			get { return isExportable; }
		}

		public CipherMode CipherMode
		{
			get { return cipherMode; }
		}

		public int HashSize
		{
			get { return (int)(hashName == "MD5" ? 16 : 20); }
		}

		public byte	KeyMaterialSize
		{
			get { return keyMaterialSize; }
		}

		public int KeyBlockSize
		{
			get 
			{
				return keyMaterialSize*2 + HashSize*2 + ivSize*2;
			}
		}

		public byte	ExpandedKeyMaterialSize
		{
			get { return expandedKeyMaterialSize; }
		}

		public byte	EffectiveKeyBits
		{
			get { return EffectiveKeyBits; }
		}
		
		public byte IvSize
		{
			get { return ivSize; }
		}

		public byte	BlockSize
		{
			get { return blockSize; }
		}

		public string HashName
		{
			get { return hashName; }
		}

		public TlsSessionContext Context
		{
			get { return context; }
			set { context = value; }
		}

		#endregion

		#region CONSTRUCTORS
		
		public CipherSuite(short code, string name, string algName, string hashName, bool exportable, bool blockMode, byte keyMaterialSize, byte expandedKeyMaterialSize, short effectiveKeyBytes, byte ivSize, byte blockSize)
		{
			this.code						= code;
			this.name						= name;
			this.algName					= algName;
			this.hashName					= hashName;
			this.isExportable				= exportable;
			if (blockMode)
			{
				this.cipherMode				= CipherMode.CBC;
			}
			this.keyMaterialSize			= keyMaterialSize;
			this.expandedKeyMaterialSize	= expandedKeyMaterialSize;
			this.effectiveKeyBits			= effectiveKeyBits;
			this.ivSize						= ivSize;
			this.blockSize					= blockSize;
		}

		#endregion

		#region METHODS

		public void InitializeCipher()
		{
			createEncryptionCipher();
			createDecryptionCipher();
		}

		public RSA CreateRSA()
		{
			RSA rsa;
			if (this.Context.ServerSettings.ServerKeyExchange)
			{
				rsa = new RSACryptoServiceProvider();
				rsa.ImportParameters(this.Context.ServerSettings.RsaParameters);
			}
			else
			{
				rsa = this.Context.ServerSettings.ServerCertificates[0].RSA;
			}
	
			return rsa;
		}

		public RSACryptoServiceProvider CreateRSA(RSAParameters rsaParams)
		{			
			// BUG: MS BCL 1.0 can't import a key which 
			// isn't the same size as the one present in
			// the container.
			int keySize = (rsaParams.Modulus.Length << 3);
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize);
			rsa.ImportParameters(rsaParams);

			return rsa;
		}

		public void UpdateClientCipherIV(byte[] iv)
		{
			if (cipherMode == CipherMode.CBC)
			{
				// Set the new IV
				encryptionAlgorithm.IV	= iv;
			
				// Create encryption cipher with the new IV
				encryptionCipher = encryptionAlgorithm.CreateEncryptor();
			}
		}

		public void UpdateServerCipherIV(byte[] iv)
		{
			if (cipherMode == CipherMode.CBC)
			{
				// Set the new IV
				decryptionAlgorithm.IV	= iv;
			
				// Create encryption cipher with the new IV
				decryptionCipher = decryptionAlgorithm.CreateDecryptor();
			}
		}

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
				int fragmentLength	= fragment.Length + mac.Length + 1;
				int paddingLength	= this.blockSize - fragmentLength % this.blockSize;
				if (paddingLength == this.blockSize)
				{
					paddingLength = 0;
				}

				// Write padding length byte
				for (int i = 0; i < (paddingLength + 1); i++)
				{
					cs.WriteByte((byte)paddingLength);
				}
			}
			// cs.FlushFinalBlock();
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
				paddingLength = buffer[buffer.Length - 1];

				/* Review this that is valid way for TLS1 but not for SSL3
				for (int i = (buffer.Length - 1); i > (buffer.Length - (paddingLength + 1)); i--)
				{
					if (buffer[i] != paddingLength)
					{
						paddingLength = 0;
						break;
					}
				}
				*/

				fragmentSize = (buffer.Length - (paddingLength + 1)) - HashSize;
			}
			else
			{
				fragmentSize = buffer.Length - HashSize;
			}

			dcrFragment = new byte[fragmentSize];
			dcrMAC		= new byte[HashSize];

			Buffer.BlockCopy(buffer, 0, dcrFragment, 0, dcrFragment.Length);
			Buffer.BlockCopy(buffer, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
		}

		#endregion

		#region ABSTRACT_METHODS

		public abstract byte[] ComputeClientRecordMAC(TlsContentType contentType, byte[] fragment);

		public abstract byte[] ComputeServerRecordMAC(TlsContentType contentType, byte[] fragment);

		public abstract void ComputeMasterSecret(byte[] preMasterSecret);

		public abstract void ComputeKeys();

		#endregion

		#region KEY_GENERATION_METODS

		public byte[] CreatePremasterSecret()
		{
			TlsStream stream = new TlsStream();

			// Write protocol version
			stream.Write((short)this.Context.Protocol);

			// Generate random bytes
			stream.Write(this.context.GetSecureRandomBytes(46));

			byte[] preMasterSecret = stream.ToArray();

			stream.Reset();

			return preMasterSecret;
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

		#region PRIVATE_METHODS

		// This code is from Mono.Security.X509Certificate class.
		private byte[] getUnsignedBigInteger(byte[] integer) 
		{
			if (integer[0] == 0x00) 
			{
				// this first byte is added so we're sure it's an unsigned integer
				// however we can't feed it into RSAParameters or DSAParameters
				int		length	 = integer.Length - 1;
				byte[]	uinteger = new byte[length];				
				Array.Copy(integer, 1, uinteger, 0, length);

				return uinteger;
			}
			else
			{
				return integer;
			}
		}

		private void createEncryptionCipher()
		{
			// Create and configure the symmetric algorithm
			switch (this.algName)
			{
				case "RC4":
					encryptionAlgorithm = new ARC4Managed();
					break;

				default:
					encryptionAlgorithm = SymmetricAlgorithm.Create(algName);
					break;
			}

			// If it's a block cipher
			if (cipherMode == CipherMode.CBC)
			{
				// Configure encrypt algorithm
				encryptionAlgorithm.Mode		= this.cipherMode;
				encryptionAlgorithm.Padding		= PaddingMode.None;
				encryptionAlgorithm.KeySize		= this.keyMaterialSize * 8;
				encryptionAlgorithm.BlockSize	= this.blockSize * 8;
			}

			// Set the key and IV for the algorithm
			encryptionAlgorithm.Key = context.ClientWriteKey;
			encryptionAlgorithm.IV	= context.ClientWriteIV;
			
			// Create encryption cipher
			encryptionCipher = encryptionAlgorithm.CreateEncryptor();

			// Create the HMAC algorithm for the client
			clientHMAC = new HMAC(hashName, context.ClientWriteMAC);
		}

		private void createDecryptionCipher()
		{
			// Create and configure the symmetric algorithm
			switch (this.algName)
			{
				case "RC4":
					decryptionAlgorithm = new ARC4Managed();
					break;

				default:
					decryptionAlgorithm = SymmetricAlgorithm.Create(algName);
					break;
			}

			// If it's a block cipher
			if (cipherMode == CipherMode.CBC)
			{
				// Configure encrypt algorithm
				decryptionAlgorithm.Mode		= this.cipherMode;
				decryptionAlgorithm.Padding		= PaddingMode.None;
				decryptionAlgorithm.KeySize		= this.keyMaterialSize * 8;
				decryptionAlgorithm.BlockSize	= this.blockSize * 8;
			}

			// Set the key and IV for the algorithm
			decryptionAlgorithm.Key = context.ServerWriteKey;
			decryptionAlgorithm.IV	= context.ServerWriteIV;

			// Create decryption cipher			
			decryptionCipher = decryptionAlgorithm.CreateDecryptor();

			// Create the HMAC algorithm for the server
			serverHMAC = new HMAC(hashName, context.ServerWriteMAC);
		}

		#endregion
	}
}