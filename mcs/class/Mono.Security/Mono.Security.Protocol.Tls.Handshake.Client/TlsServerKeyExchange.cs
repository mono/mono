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
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Security.X509;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerKeyExchange : HandshakeMessage
	{
		#region Fields

		private RSAParameters	rsaParams;
		private byte[]			signedParams;

		#endregion

		#region Constructors

		public TlsServerKeyExchange(Context context, byte[] buffer)
			: base(context, HandshakeType.ServerKeyExchange, buffer)
		{
			this.verifySignature();
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			this.Context.ServerSettings.ServerKeyExchange	= true;
			this.Context.ServerSettings.RsaParameters		= this.rsaParams;
			this.Context.ServerSettings.SignedParams		= this.signedParams;
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			this.rsaParams = new RSAParameters();
			
			// Read modulus
			this.rsaParams.Modulus	= this.ReadBytes(this.ReadInt16());

			// Read exponent
			this.rsaParams.Exponent	= this.ReadBytes(this.ReadInt16());

			// Read signed params
			this.signedParams		= this.ReadBytes(this.ReadInt16());
		}

		#endregion

		#region Private Methods

		private void verifySignature()
		{
			MD5SHA1 hash = new MD5SHA1();

			// Calculate size of server params
			int size = rsaParams.Modulus.Length + rsaParams.Exponent.Length + 4;

			// Create server params array
			TlsStream stream = new TlsStream();

			stream.Write(this.Context.RandomCS);
			stream.Write(this.ToArray(), 0, size);

			hash.ComputeHash(stream.ToArray());

			stream.Reset();
			
			bool isValidSignature = hash.VerifySignature(
				this.Context.ServerSettings.CertificateRSA,
				this.signedParams);

			if (!isValidSignature)
			{
				throw new TlsException(
					AlertDescription.DecodeError,
					"Data was not signed with the server certificate.");
			}
		}

		#endregion
	}
}
