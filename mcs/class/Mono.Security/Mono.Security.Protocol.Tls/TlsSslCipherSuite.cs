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
	internal class TlsSslCipherSuite : CipherSuite
	{
		#region FIELDS

		private byte[] pad1;
		private byte[] pad2;

		#endregion

		#region CONSTRUCTORS
		
		public TlsSslCipherSuite(short code, string name, string algName, 
			string hashName, bool exportable, bool blockMode, 
			byte keyMaterialSize, byte expandedKeyMaterialSize, 
			short effectiveKeyBytes, byte ivSize, byte blockSize) 
			: base (code, name, algName, hashName, exportable, blockMode,
			keyMaterialSize, expandedKeyMaterialSize, effectiveKeyBytes,
			ivSize, blockSize)
		{
			int padLength = (hashName == "MD5") ? 48 : 40;

			// Fill pad arrays
			this.pad1 = new byte[padLength];
			this.pad2 = new byte[padLength];

			/* Pad the key for inner and outer digest */
			for (int i = 0; i < padLength; ++i) 
			{
				pad1[i] = 0x36;
				pad2[i] = 0x5C;
			}
		}

		#endregion

		#region MAC_GENERATION_METHOD

		public override byte[] ComputeServerRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			HashAlgorithm	hash	= HashAlgorithm.Create(this.HashName);
			TlsStream		block	= new TlsStream();

			block.Write(this.Context.ServerWriteMAC);
			block.Write(this.pad1);
			block.Write(this.Context.ReadSequenceNumber);
			block.Write((byte)contentType);
			block.Write((short)fragment.Length);
			block.Write(fragment);
			
			hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			byte[] blockHash = hash.Hash;

			block.Reset();

			block.Write(this.Context.ServerWriteMAC);
			block.Write(this.pad2);
			block.Write(blockHash);

			hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			block.Reset();

			return hash.Hash;
		}

		public override byte[] ComputeClientRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			HashAlgorithm	hash	= HashAlgorithm.Create(this.HashName);
			TlsStream		block	= new TlsStream();

			block.Write(this.Context.ClientWriteMAC);
			block.Write(this.pad1);
			block.Write(this.Context.WriteSequenceNumber);
			block.Write((byte)contentType);
			block.Write((short)fragment.Length);
			block.Write(fragment);
			
			hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			byte[] blockHash = hash.Hash;

			block.Reset();

			block.Write(this.Context.ClientWriteMAC);
			block.Write(this.pad2);
			block.Write(blockHash);

			hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			block.Reset();

			return hash.Hash;
		}

		#endregion

		#region KEY_GENERATION_METODS

		public override void ComputeMasterSecret(byte[] preMasterSecret)
		{
			TlsStream masterSecret = new TlsStream();

			masterSecret.Write(this.prf(preMasterSecret, "A", this.Context.RandomCS));
			masterSecret.Write(this.prf(preMasterSecret, "BB", this.Context.RandomCS));
			masterSecret.Write(this.prf(preMasterSecret, "CCC", this.Context.RandomCS));

			this.Context.MasterSecret = masterSecret.ToArray();
		}

		public override void ComputeKeys()
		{
			// Compute KeyBlock
			TlsStream tmp = new TlsStream();
			
			char	labelChar	= 'A';
			int		count		= 1;
			while (tmp.Length < this.KeyBlockSize)
			{
				string label = String.Empty;

				for (int i = 0; i < count; i++)
				{
					label += labelChar.ToString();
				}
						
				byte[] block = this.prf(this.Context.MasterSecret, label.ToString(), this.Context.RandomSC);

				int size = (tmp.Length + block.Length) > this.KeyBlockSize ? (this.KeyBlockSize - (int)tmp.Length) : block.Length;
				
				tmp.Write(block, 0, size);

				labelChar++;
				count++;
			}
			
			// Create keyblock
			TlsStream keyBlock = new TlsStream(tmp.ToArray());

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
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

				// Generate final write keys
				byte[] finalClientWriteKey	= new byte[md5.HashSize];
				md5.TransformBlock(this.Context.ClientWriteKey, 0, this.Context.ClientWriteKey.Length, finalClientWriteKey, 0);
				finalClientWriteKey = md5.TransformFinalBlock(this.Context.RandomCS, 0, this.Context.RandomCS.Length);

				byte[] finalServerWriteKey	= new byte[md5.HashSize];
				md5.TransformBlock(this.Context.ServerWriteKey, 0, this.Context.ServerWriteKey.Length, finalServerWriteKey, 0);
				finalClientWriteKey = md5.TransformFinalBlock(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
				
				this.Context.ClientWriteKey	= finalClientWriteKey;
				this.Context.ServerWriteKey	= finalServerWriteKey;

				// Generate IV keys
				this.Context.ClientWriteIV = md5.TransformFinalBlock(this.Context.RandomCS, 0, this.Context.RandomCS.Length);
				this.Context.ServerWriteIV = md5.TransformFinalBlock(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
			}

			// Clear no more needed data
			keyBlock.Reset();
			tmp.Reset();
		}

		#endregion

		#region PRIVATE_METHODS

		private byte[] prf(byte[] secret, string label, byte[] random)
		{
			HashAlgorithm md5 = new MD5CryptoServiceProvider();
			SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();

			// Compute SHA hash
			TlsStream block = new TlsStream();
			block.Write(Encoding.ASCII.GetBytes(label));
			block.Write(secret);
			block.Write(random);
						
			byte[] shaHash = sha.ComputeHash(block.ToArray(), 0, (int)block.Length);

			block.Reset();

			// Compute MD5 hash
			block.Write(secret);
			block.Write(shaHash);

			byte[] result = md5.ComputeHash(block.ToArray(), 0, (int)block.Length);

			// Free resources
			block.Reset();

			return result;
		}

		#endregion
	}
}