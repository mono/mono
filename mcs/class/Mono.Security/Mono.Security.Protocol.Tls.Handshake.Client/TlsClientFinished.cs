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
using System.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientFinished : TlsHandshakeMessage
	{
		#region CONSTRUCTORS

		public TlsClientFinished(TlsSession session) 
			: base(session, TlsHandshakeType.Finished,  TlsContentType.Handshake)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Reset();
		}

		#endregion

		#region PROTECTED_METHODS

		private byte[] computeSslHash(string hashName, byte[] hashes, int sender)
		{
			HashAlgorithm		hash	= HashAlgorithm.Create(hashName);
			TlsStream			block	= new TlsStream();
			TlsSslCipherSuite	cipher	= (TlsSslCipherSuite)this.Session.Context.Cipher;
			byte[]				pad1	= null;
			byte[]				pad2	= null;

			cipher.GeneratePad(hashName, ref pad1, ref pad2);

			block.Write(hashes);
			block.Write(sender);
			block.Write(this.Session.Context.MasterSecret);
			block.Write(cipher.Pad1);

			byte[] blockHash = hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			block.Reset();

			block.Write(this.Session.Context.MasterSecret);
			block.Write(cipher.Pad2);
			block.Write(blockHash);

			blockHash = hash.ComputeHash(block.ToArray(), 0, (int)block.Length);

			block.Reset();

			return blockHash;
		}

		protected override void ProcessAsSsl3()
		{
			this.Write(computeSslHash("MD5", Session.Context.HandshakeHashes.Messages, 0x434C4E54));
			this.Write(computeSslHash("SHA1", Session.Context.HandshakeHashes.Messages, 0x434C4E54));
			
			Session.Context.HandshakeHashes.Reset();
		}

		protected override void ProcessAsTls1()
		{
			// Get hashes of handshake messages
			TlsStream hashes = new TlsStream();

			hashes.Write(Session.Context.HandshakeHashes.GetMD5Hash());
			hashes.Write(Session.Context.HandshakeHashes.GetSHAHash());

			// Write message contents
			Write(Session.Context.Cipher.PRF(Session.Context.MasterSecret, "client finished", hashes.ToArray(), 12));

			// Reset data
			hashes.Reset();
			Session.Context.HandshakeHashes.Reset();
		}

		#endregion
	}
}
