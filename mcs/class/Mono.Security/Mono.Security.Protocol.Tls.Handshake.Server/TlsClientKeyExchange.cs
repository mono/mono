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
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls.Handshake.Server
{
    internal class TlsClientKeyExchange : HandshakeMessage
    {
        #region Constructors

        public TlsClientKeyExchange(Context context, byte[] buffer) : 
			base(context,
				HandshakeType.ClientKeyExchange, 
				buffer)
        {
        }

        #endregion

        #region Protected Methods

        protected override void ProcessAsSsl3()
        {
            AsymmetricAlgorithm privKey = null;
            ServerContext context = (ServerContext)this.Context;

            // Select the private key information
            privKey = context.SslStream.RaisePrivateKeySelection(
                new X509Certificate(context.ServerSettings.Certificates[0].RawData),
                null);

            if (privKey == null)
            {
                throw new TlsException(AlertDescription.UserCancelled, "Server certificate Private Key unavailable.");
            }

            // Read client premaster secret
            byte[] clientSecret = this.ReadBytes((int)this.Length);

            // Decrypt premaster secret
            RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(privKey);

            byte[] preMasterSecret = deformatter.DecryptKeyExchange(clientSecret);

            // Create master secret
            this.Context.Cipher.ComputeMasterSecret(preMasterSecret);

            // Create keys
            this.Context.Cipher.ComputeKeys();

            // Initialize Cipher Suite
            this.Context.Cipher.InitializeCipher();
        }

        protected override void ProcessAsTls1()
        {
            AsymmetricAlgorithm privKey = null;
            ServerContext context = (ServerContext)this.Context;

            // Select the private key information
            // Select the private key information
            privKey = context.SslStream.RaisePrivateKeySelection(
                new X509Certificate(context.ServerSettings.Certificates[0].RawData),
                null);

            if (privKey == null)
            {
                throw new TlsException(AlertDescription.UserCancelled, "Server certificate Private Key unavailable.");
            }

            // Read client premaster secret
            byte[] clientSecret = this.ReadBytes(this.ReadInt16());

            // Decrypt premaster secret
            RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(privKey);

            byte[] preMasterSecret = deformatter.DecryptKeyExchange(clientSecret);

            // Create master secret
            this.Context.Cipher.ComputeMasterSecret(preMasterSecret);

            // Create keys
            this.Context.Cipher.ComputeKeys();

            // Initialize Cipher Suite
            this.Context.Cipher.InitializeCipher();
        }

        #endregion
    }
}
