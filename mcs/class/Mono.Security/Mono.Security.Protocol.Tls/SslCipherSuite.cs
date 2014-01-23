// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Protocol.Tls
{
	internal class SslCipherSuite : CipherSuite
	{
		#region Fields

		private byte[] pad1;
		private byte[] pad2;

		private const int MacHeaderLength = 11;
		private byte[] header;

		#endregion

		#region Constructors
		
		public SslCipherSuite(
			short code, string name, CipherAlgorithmType cipherAlgorithmType, 
			HashAlgorithmType hashAlgorithmType, ExchangeAlgorithmType exchangeAlgorithmType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize) :
			base(code, name, cipherAlgorithmType, hashAlgorithmType, 
			exchangeAlgorithmType, exportable, blockMode, keyMaterialSize, 
			expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize)

		{
			int padLength = (hashAlgorithmType == HashAlgorithmType.Md5) ? 48 : 40;

			// Fill pad arrays
			this.pad1 = new byte[padLength];
			this.pad2 = new byte[padLength];

			/* Pad the key for inner and outer digest */
			for (int i = 0; i < padLength; ++i) 
			{
				this.pad1[i] = 0x36;
				this.pad2[i] = 0x5C;
			}
		}

		#endregion

		#region MAC Generation Methods

		public override byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment)
		{
			HashAlgorithm hash = CreateHashAlgorithm ();

			byte[] smac = this.Context.Read.ServerWriteMAC;
			hash.TransformBlock (smac, 0, smac.Length, smac, 0);
			hash.TransformBlock (pad1, 0, pad1.Length, pad1, 0);

			if (header == null)
				header = new byte [MacHeaderLength];

			ulong seqnum = (Context is ClientContext) ? Context.ReadSequenceNumber : Context.WriteSequenceNumber;
			Write (header, 0, seqnum);
			header [8] = (byte) contentType;
			Write (header, 9, (short)fragment.Length);
			hash.TransformBlock (header, 0, header.Length, header, 0);
			hash.TransformBlock (fragment, 0, fragment.Length, fragment, 0);
			// hack, else the method will allocate a new buffer of the same length (negative half the optimization)
			hash.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);

			byte[] blockHash = hash.Hash;

			hash.Initialize ();

			hash.TransformBlock (smac, 0, smac.Length, smac, 0);
			hash.TransformBlock (pad2, 0, pad2.Length, pad2, 0);
			hash.TransformBlock (blockHash, 0, blockHash.Length, blockHash, 0);
			// hack again
			hash.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);

			return hash.Hash;
		}

		public override byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment)
		{
			HashAlgorithm hash = CreateHashAlgorithm ();

			byte[] cmac = this.Context.Current.ClientWriteMAC;
			hash.TransformBlock (cmac, 0, cmac.Length, cmac, 0);
			hash.TransformBlock (pad1, 0, pad1.Length, pad1, 0);

			if (header == null)
				header = new byte [MacHeaderLength];

			ulong seqnum = (Context is ClientContext) ? Context.WriteSequenceNumber : Context.ReadSequenceNumber;
			Write (header, 0, seqnum);
			header [8] = (byte) contentType;
			Write (header, 9, (short)fragment.Length);
			hash.TransformBlock (header, 0, header.Length, header, 0);
			hash.TransformBlock (fragment, 0, fragment.Length, fragment, 0);
			// hack, else the method will allocate a new buffer of the same length (negative half the optimization)
			hash.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);

			byte[] blockHash = hash.Hash;

			hash.Initialize ();

			hash.TransformBlock (cmac, 0, cmac.Length, cmac, 0);
			hash.TransformBlock (pad2, 0, pad2.Length, pad2, 0);
			hash.TransformBlock (blockHash, 0, blockHash.Length, blockHash, 0);
			// hack again
			hash.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);

			return hash.Hash;
		}

		#endregion

		#region Key Generation Methods

		public override void ComputeMasterSecret(byte[] preMasterSecret)
		{
			TlsStream masterSecret = new TlsStream();

			masterSecret.Write(this.prf(preMasterSecret, "A", this.Context.RandomCS));
			masterSecret.Write(this.prf(preMasterSecret, "BB", this.Context.RandomCS));
			masterSecret.Write(this.prf(preMasterSecret, "CCC", this.Context.RandomCS));

			this.Context.MasterSecret = masterSecret.ToArray();

			DebugHelper.WriteLine(">>>> MasterSecret", this.Context.MasterSecret);
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
			
			this.Context.Negotiating.ClientWriteMAC = keyBlock.ReadBytes(this.HashSize);
			this.Context.Negotiating.ServerWriteMAC = keyBlock.ReadBytes(this.HashSize);
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
					this.Context.ClientWriteIV = CipherSuite.EmptyArray;
					this.Context.ServerWriteIV = CipherSuite.EmptyArray;
				}
			}
			else
			{
				HashAlgorithm md5 = MD5.Create();

				int keySize = (md5.HashSize >> 3); //in bytes not bits
				byte[] temp = new byte [keySize];

				// Generate final write keys
				md5.TransformBlock(this.Context.ClientWriteKey, 0, this.Context.ClientWriteKey.Length, temp, 0);
				md5.TransformFinalBlock(this.Context.RandomCS, 0, this.Context.RandomCS.Length);
				byte[] finalClientWriteKey = new byte[this.ExpandedKeyMaterialSize];
				Buffer.BlockCopy(md5.Hash, 0, finalClientWriteKey, 0, this.ExpandedKeyMaterialSize);

				md5.Initialize();
				md5.TransformBlock(this.Context.ServerWriteKey, 0, this.Context.ServerWriteKey.Length, temp, 0);
				md5.TransformFinalBlock(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
				byte[] finalServerWriteKey = new byte[this.ExpandedKeyMaterialSize];
				Buffer.BlockCopy(md5.Hash, 0, finalServerWriteKey, 0, this.ExpandedKeyMaterialSize);
				
				this.Context.ClientWriteKey = finalClientWriteKey;
				this.Context.ServerWriteKey = finalServerWriteKey;

				// Generate IV keys
				if (this.IvSize > 0) 
				{
					md5.Initialize();
					temp = md5.ComputeHash(this.Context.RandomCS, 0, this.Context.RandomCS.Length);
					this.Context.ClientWriteIV = new byte[this.IvSize];
					Buffer.BlockCopy(temp, 0, this.Context.ClientWriteIV, 0, this.IvSize);

					md5.Initialize();
					temp = md5.ComputeHash(this.Context.RandomSC, 0, this.Context.RandomSC.Length);
					this.Context.ServerWriteIV = new byte[this.IvSize];
					Buffer.BlockCopy(temp, 0, this.Context.ServerWriteIV, 0, this.IvSize);
				}
				else 
				{
					this.Context.ClientWriteIV = CipherSuite.EmptyArray;
					this.Context.ServerWriteIV = CipherSuite.EmptyArray;
				}
			}

			DebugHelper.WriteLine(">>>> KeyBlock", keyBlock.ToArray());
			DebugHelper.WriteLine(">>>> ClientWriteKey", this.Context.ClientWriteKey);
			DebugHelper.WriteLine(">>>> ClientWriteIV", this.Context.ClientWriteIV);
			DebugHelper.WriteLine(">>>> ClientWriteMAC", this.Context.Negotiating.ClientWriteMAC);
			DebugHelper.WriteLine(">>>> ServerWriteKey", this.Context.ServerWriteKey);
			DebugHelper.WriteLine(">>>> ServerWriteIV", this.Context.ServerWriteIV);
			DebugHelper.WriteLine(">>>> ServerWriteMAC", this.Context.Negotiating.ServerWriteMAC);

			ClientSessionCache.SetContextInCache (this.Context);
			// Clear no more needed data
			keyBlock.Reset();
			tmp.Reset();
		}

		#endregion

		#region Private Methods

		private byte[] prf(byte[] secret, string label, byte[] random)
		{
			HashAlgorithm md5 = MD5.Create();
			HashAlgorithm sha = SHA1.Create();

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
