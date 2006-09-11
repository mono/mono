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

namespace Mono.Security.Protocol.Tls
{
	internal class TlsCipherSuite : CipherSuite
	{
		private const int MacHeaderLength = 13;
		private byte[] header;

		#region Constructors
		
		public TlsCipherSuite(
			short code, string name, CipherAlgorithmType cipherAlgorithmType, 
			HashAlgorithmType hashAlgorithmType, ExchangeAlgorithmType exchangeAlgorithmType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize) 
			:base(code, name, cipherAlgorithmType, hashAlgorithmType, 
			exchangeAlgorithmType, exportable, blockMode, keyMaterialSize, 
			expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize)
		{
		}

		#endregion

		#region MAC Generation Methods

		public override byte[] ComputeServerRecordMAC(ContentType contentType, byte[] fragment)
		{
			if (header == null)
				header = new byte [MacHeaderLength];

			ulong seqnum = (Context is ClientContext) ? Context.ReadSequenceNumber : Context.WriteSequenceNumber;
			Write (header, 0, seqnum);
			header [8] = (byte) contentType;
			Write (header, 9, this.Context.Protocol);
			Write (header, 11, (short)fragment.Length);

			HashAlgorithm mac = this.ServerHMAC;
			mac.TransformBlock (header, 0, header.Length, header, 0);
			mac.TransformBlock (fragment, 0, fragment.Length, fragment, 0);
			// hack, else the method will allocate a new buffer of the same length (negative half the optimization)
			mac.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);
			return mac.Hash;
		}

		public override byte[] ComputeClientRecordMAC(ContentType contentType, byte[] fragment)
		{
			if (header == null)
				header = new byte [MacHeaderLength];

			ulong seqnum = (Context is ClientContext) ? Context.WriteSequenceNumber : Context.ReadSequenceNumber;
			Write (header, 0, seqnum);
			header [8] = (byte) contentType;
			Write (header, 9, this.Context.Protocol);
			Write (header, 11, (short)fragment.Length);

			HashAlgorithm mac = this.ClientHMAC;
			mac.TransformBlock (header, 0, header.Length, header, 0);
			mac.TransformBlock (fragment, 0, fragment.Length, fragment, 0);
			// hack, else the method will allocate a new buffer of the same length (negative half the optimization)
			mac.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);
			return mac.Hash;
		}

		#endregion

		#region Key Generation Methods

		public override void ComputeMasterSecret(byte[] preMasterSecret)
		{
			// Create master secret
			this.Context.MasterSecret = new byte[preMasterSecret.Length];
			this.Context.MasterSecret = this.PRF(
				preMasterSecret, "master secret", this.Context.RandomCS, 48);

			DebugHelper.WriteLine(">>>> MasterSecret", this.Context.MasterSecret);
		}

		public override void ComputeKeys()
		{
			// Create keyblock
			TlsStream keyBlock = new TlsStream(
				this.PRF(
				this.Context.MasterSecret, 
				"key expansion",
				this.Context.RandomSC,
				this.KeyBlockSize));

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
				// Generate final write keys
				byte[] finalClientWriteKey	= PRF(this.Context.ClientWriteKey, "client write key", this.Context.RandomCS, this.ExpandedKeyMaterialSize);
				byte[] finalServerWriteKey	= PRF(this.Context.ServerWriteKey, "server write key", this.Context.RandomCS, this.ExpandedKeyMaterialSize);
				
				this.Context.ClientWriteKey	= finalClientWriteKey;
				this.Context.ServerWriteKey	= finalServerWriteKey;

				if (this.IvSize > 0) 
				{
					// Generate IV block
					byte[] ivBlock = PRF(CipherSuite.EmptyArray, "IV block", this.Context.RandomCS, this.IvSize*2);

					// Generate IV keys
					this.Context.ClientWriteIV = new byte[this.IvSize];				
					Buffer.BlockCopy(ivBlock, 0, this.Context.ClientWriteIV, 0, this.Context.ClientWriteIV.Length);

					this.Context.ServerWriteIV = new byte[this.IvSize];
					Buffer.BlockCopy(ivBlock, this.IvSize, this.Context.ServerWriteIV, 0, this.Context.ServerWriteIV.Length);
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
		}

		#endregion
	}
}
