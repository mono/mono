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
using System.Security.Cryptography.X509Certificates;

using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
	internal class TlsClientCertificateVerify : HandshakeMessage
	{
		#region Constructors

		public TlsClientCertificateVerify(Context context, byte[] buffer)
			: base(context, HandshakeType.CertificateVerify, buffer)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			ServerContext	context		= (ServerContext)this.Context;
			byte[]			signature	= this.ReadBytes((int)this.Length);

			// Verify signature
			SslHandshakeHash hash = new SslHandshakeHash(context.MasterSecret);			
			hash.TransformFinalBlock(
				context.HandshakeMessages.ToArray(), 
				0, 
				(int)context.HandshakeMessages.Length);

			if (!hash.VerifySignature(context.ClientSettings.CertificateRSA, signature))
			{
				throw new TlsException(AlertDescription.HandshakeFailiure, "Handshake Failiure.");
			}
		}

		protected override void ProcessAsTls1()
		{
			ServerContext	context		= (ServerContext)this.Context;
			byte[]			signature	= this.ReadBytes((int)this.Length);			

			// Verify signature
			MD5SHA1 hash = new MD5SHA1();
			hash.ComputeHash(
				context.HandshakeMessages.ToArray(),
				0,
				(int)context.HandshakeMessages.Length);

			if (!hash.VerifySignature(context.ClientSettings.CertificateRSA, signature))
			{
				throw new TlsException(
					AlertDescription.HandshakeFailiure,
					"Handshake Failiure.");
			}
		}

		#endregion
	}
}
