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
using System.Security.Cryptography.X509Certificates;

using Mono.Security;
using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsCipherSuite
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
		private byte				effectiveKeyBits;
		private byte				ivSize;
		private byte				blockSize;
		private TlsSessionContext	sessionContext;
		private SymmetricAlgorithm	encryptionAlgorithm;
		private ICryptoTransform	encryptionCipher;
		private SymmetricAlgorithm	decryptionAlgorithm;
		private ICryptoTransform	decryptionCipher;
		private KeyedHashAlgorithm	clientHMAC;
		private KeyedHashAlgorithm	serverHMAC;
			
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

		public TlsSessionContext SessionState
		{
			get { return sessionContext; }
			set { sessionContext = value; }
		}

		public KeyedHashAlgorithm ClientHMAC
		{
			get { return clientHMAC; }
		}

		public KeyedHashAlgorithm ServerHMAC
		{
			get { return serverHMAC; }
		}

		#endregion

		#region CONSTRUCTORS
		
		public TlsCipherSuite(short code, string name, string algName, string hashName, bool exportable, bool blockMode, byte keyMaterialSize, byte expandedKeyMaterialSize, byte effectiveKeyBytes, byte ivSize, byte blockSize)
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

		public RSACryptoServiceProvider CreateRSA(X509Certificate certificate)
		{
			RSAParameters rsaParams = new RSAParameters();

			// for RSA m_publickey contains 2 ASN.1 integers
			// the modulus and the public exponent
			ASN1 pubkey	= new ASN1(certificate.GetPublicKey());
			ASN1 modulus = pubkey [0];
			if ((modulus == null) || (modulus.Tag != 0x02))
			{
				return null;
			}
			ASN1 exponent = pubkey [1];
			if (exponent.Tag != 0x02)
			{
				return null;
			}

			rsaParams.Modulus	= getUnsignedBigInteger(modulus.Value);
			rsaParams.Exponent	= exponent.Value;
			
			return CreateRSA(rsaParams);
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

		public void InitializeCipher()
		{
			createEncryptionCipher();
			createDecryptionCipher();
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
			CryptoStream cs = new CryptoStream(ms, encryptionCipher, CryptoStreamMode.Write);

			cs.Write(fragment, 0, fragment.Length);
			cs.Write(mac, 0, mac.Length);
			if (cipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				int fragmentLength	= fragment.Length + mac.Length + 1;
				int paddingLength	= (((fragmentLength/blockSize)*8) + blockSize) - fragmentLength;

				// Write padding length byte
				cs.WriteByte((byte)paddingLength);
			}
			//cs.FlushFinalBlock();
			cs.Close();			

			return ms.ToArray();
		}

		public void DecryptRecord(byte[] fragment, ref byte[] dcrFragment, ref byte[] dcrMAC)
		{
			int	fragmentSize	= 0;
			int paddingLength	= 0;

			// Decrypt message fragment ( fragment + mac [+ padding + padding_length] )
			byte[] buffer = new byte[fragment.Length];
			decryptionCipher.TransformBlock(fragment, 0, fragment.Length, buffer, 0);

			// Calculate fragment size
			if (cipherMode == CipherMode.CBC)
			{
				// Calculate padding_length
				paddingLength = buffer[buffer.Length - 1];
				for (int i = (buffer.Length - 1); i > (buffer.Length - (paddingLength + 1)); i--)
				{
					if (buffer[i] != paddingLength)
					{
						paddingLength = 0;
						break;
					}
				}

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

		public int GetKeyBlockSize()
		{
			return keyMaterialSize*2 + HashSize*2 + ivSize*2;
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
				encryptionAlgorithm.Padding		= PaddingMode.PKCS7;
				encryptionAlgorithm.KeySize		= this.keyMaterialSize * 8;
				encryptionAlgorithm.BlockSize	= this.blockSize * 8;
			}

			// Set the key and IV for the algorithm
			encryptionAlgorithm.Key = sessionContext.ClientWriteKey;
			encryptionAlgorithm.IV	= sessionContext.ClientWriteIV;
			
			// Create encryption cipher
			encryptionCipher = encryptionAlgorithm.CreateEncryptor();

			// Create the HMAC algorithm for the client
			clientHMAC = new HMAC(hashName, sessionContext.ClientWriteMAC);
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
			decryptionAlgorithm.Key = sessionContext.ServerWriteKey;
			decryptionAlgorithm.IV	= sessionContext.ServerWriteIV;

			// Create decryption cipher			
			decryptionCipher = decryptionAlgorithm.CreateDecryptor();

			// Create the HMAC algorithm for the server
			serverHMAC = new HMAC(hashName, sessionContext.ServerWriteMAC);
		}

		#endregion
	}
}