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
	internal class TlsCipherSuite : TlsAbstractCipherSuite
	{
		#region CONSTRUCTORS
		
		public TlsCipherSuite(short code, string name, string algName, 
			string hashName, bool exportable, bool blockMode, 
			byte keyMaterialSize, byte expandedKeyMaterialSize, 
			short effectiveKeyBytes, byte ivSize, byte blockSize) 
			: base (code, name, algName, hashName, exportable, blockMode,
			keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes,
			ivSize, blockSize)
		{
		}

		#endregion

		#region METHODS

		public override byte[] EncryptRecord(byte[] fragment, byte[] mac)
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
				int paddingLength	= (((fragmentLength/blockSize)*blockSize) + blockSize) - fragmentLength;

				// Write padding length byte
				cs.WriteByte((byte)paddingLength);
			}
			//cs.FlushFinalBlock();
			cs.Close();			

			return ms.ToArray();
		}

		public override void DecryptRecord(byte[] fragment, ref byte[] dcrFragment, ref byte[] dcrMAC)
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

		#endregion

		#region KEY_GENERATION_METODS

		public override void CreateMasterSecret(byte[] preMasterSecret)
		{
			TlsStream seed = new TlsStream();

			// Seed
			seed.Write(context.ClientRandom);
			seed.Write(context.ServerRandom);

			// Create master secret
			context.MasterSecret = new byte[preMasterSecret.Length];
			context.MasterSecret = PRF(preMasterSecret, "master secret", seed.ToArray(), 48);

			seed.Reset();
		}

		public override void CreateKeys()
		{
			TlsStream seed = new TlsStream();

			// Seed
			seed.Write(context.ServerRandom);
			seed.Write(context.ClientRandom);

			// Create keyblock
			TlsStream keyBlock = new TlsStream(
				PRF(this.Context.MasterSecret, 
				"key expansion",
				seed.ToArray(),
				this.KeyBlockSize));

			this.Context.ClientWriteMAC = keyBlock.ReadBytes(this.HashSize);
			this.Context.ServerWriteMAC = keyBlock.ReadBytes(this.HashSize);
			this.Context.ClientWriteKey = keyBlock.ReadBytes(this.KeyMaterialSize);
			this.Context.ServerWriteKey = keyBlock.ReadBytes(this.KeyMaterialSize);

			if (!this.IsExportable)
			{
				if (this.IvSize != 0)
				{
					this.Context.ClientWriteIV = keyBlock.ReadBytes(this.IvSize);
					this.Context.ServerWriteIV = keyBlock.ReadBytes(this.IvSize);
				}
				else
				{
					this.Context.ClientWriteIV = new byte[0];
					this.Context.ServerWriteIV = new byte[0];
				}
			}
			else
			{
				// Seed
				seed.Reset();
				seed.Write(this.Context.ClientRandom);
				seed.Write(this.Context.ServerRandom);

				// Generate final write keys
				byte[] finalClientWriteKey	= PRF(this.Context.ClientWriteKey, "client write key", seed.ToArray(), this.KeyMaterialSize);
				byte[] finalServerWriteKey	= PRF(this.Context.ServerWriteKey, "server write key", seed.ToArray(), this.KeyMaterialSize);
				
				this.Context.ClientWriteKey	= finalClientWriteKey;
				this.Context.ServerWriteKey	= finalServerWriteKey;

				// Generate IV block
				byte[] ivBlock = PRF(new byte[]{}, "IV block", seed.ToArray(), this.IvSize*2);

				// Generate IV keys
				this.Context.ClientWriteIV = new byte[this.IvSize];				
				System.Array.Copy(ivBlock, 0, this.Context.ClientWriteIV, 0, this.Context.ClientWriteIV.Length);
				this.Context.ServerWriteIV = new byte[this.IvSize];
				System.Array.Copy(ivBlock, this.IvSize, this.Context.ServerWriteIV, 0, this.Context.ServerWriteIV.Length);
			}

			// Clear no more needed data
			seed.Reset();
			keyBlock.Reset();
		}

		#endregion
	}
}