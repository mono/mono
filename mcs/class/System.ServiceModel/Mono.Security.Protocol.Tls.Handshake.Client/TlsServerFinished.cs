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
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerFinished : HandshakeMessage
	{
		#region Constructors

		public TlsServerFinished(Context context, byte[] buffer) 
			: base(context, HandshakeType.Finished, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			// Hahdshake is finished
			this.Context.HandshakeState = HandshakeState.Finished;
		}

		#endregion

		#region Protected Methods

		static private byte[] Ssl3Marker = new byte [4] { 0x53, 0x52, 0x56, 0x52 };

		protected override void ProcessAsSsl3()
		{
			// Compute handshake messages hashes
			HashAlgorithm hash = new SslHandshakeHash(this.Context.MasterSecret);

			byte[] data = this.Context.HandshakeMessages.ToArray ();
			hash.TransformBlock (data, 0, data.Length, data, 0);
			hash.TransformBlock (Ssl3Marker, 0, Ssl3Marker.Length, Ssl3Marker, 0);
			// hack to avoid memory allocation
			hash.TransformFinalBlock (CipherSuite.EmptyArray, 0, 0);

			byte[] serverHash	= this.ReadBytes((int)Length);			
			byte[] clientHash	= hash.Hash;
			
			// Check server prf against client prf
			if (!Compare (clientHash, serverHash))
			{
#warning Review that selected alert is correct
				throw new TlsException(AlertDescription.InsuficientSecurity, "Invalid ServerFinished message received.");
			}
		}

		protected override void ProcessAsTls1()
		{
			byte[]			serverPRF	= this.ReadBytes((int)Length);
			HashAlgorithm	hash		= new MD5SHA1();

			// note: we could call HashAlgorithm.ComputeHash(Stream) but that would allocate (on Mono)
			// a 4096 bytes buffer to process the hash - which is bigger than HandshakeMessages
			byte[] data = this.Context.HandshakeMessages.ToArray ();
			byte[] digest = hash.ComputeHash (data, 0, data.Length);

			byte[] clientPRF = this.Context.Current.Cipher.PRF(this.Context.MasterSecret, "server finished", digest, 12);

			// Check server prf against client prf
			if (!Compare (clientPRF, serverPRF))
			{
				throw new TlsException("Invalid ServerFinished message received.");
			}
		}

		#endregion
	}
}
