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

namespace Mono.Security.Protocol.Tls.Handshake
{
	internal class TlsHandshakeHashes
	{
		#region FIELDS

		private MD5CryptoServiceProvider	md5;
		private SHA1CryptoServiceProvider	sha;
		private TlsStream					messages;

		#endregion

		#region CONSTRUCTORS

		public TlsHandshakeHashes()
		{
			this.messages	= new TlsStream();
			this.md5		= new MD5CryptoServiceProvider();
			this.sha		= new SHA1CryptoServiceProvider();
		}

		#endregion

		#region METHODS

		public void Update(byte[] message)
		{
			byte[] tmp = new byte[message.Length];

			md5.TransformBlock(message, 0, message.Length, tmp, 0);
			sha.TransformBlock(message, 0, message.Length, tmp, 0);

			this.messages.Write(message);
		}

		public byte[] GetMD5Hash()
		{
			md5.TransformFinalBlock(new byte[0], 0, 0);
		
			return md5.Hash;
		}

		public byte[] GetSHAHash()
		{
			sha.TransformFinalBlock(new byte[0], 0, 0);
		
			return sha.Hash;
		}

		public void Reset()
		{
			md5.Initialize();
			sha.Initialize();

			byte[] tmp = new byte[messages.Length];
			
			md5.TransformBlock(messages.ToArray(), 0, tmp.Length, tmp, 0);
			sha.TransformBlock(messages.ToArray(), 0, tmp.Length, tmp, 0);
		}

		public void Clear()
		{
			md5.Initialize();
			sha.Initialize();

			messages.Reset();
		}

		#endregion
	}
}
