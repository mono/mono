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

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientCertificateVerify : HandshakeMessage
	{
		#region Constructors

		public TlsClientCertificateVerify(Context context) 
			: base(context, HandshakeType.CertificateVerify)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Reset();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			AsymmetricAlgorithm privKey = null;
			ClientContext		context = (ClientContext)this.Context;
			
			privKey = context.SslStream.RaisePrivateKeySelection(
				context.ClientSettings.ClientCertificate,
				context.ClientSettings.TargetHost);

			if (privKey == null)
			{
				throw new TlsException(AlertDescription.UserCancelled, "Client certificate Private Key unavailable.");
			}
			else
			{
				SslHandshakeHash hash = new SslHandshakeHash(context.MasterSecret);			
				hash.TransformFinalBlock(
					context.HandshakeMessages.ToArray(), 
					0, 
					(int)context.HandshakeMessages.Length);

				// CreateSignature uses ((RSA)privKey).DecryptValue which is not implemented
				// in RSACryptoServiceProvider. Other implementations likely implement DecryptValue
				// so we will try the CreateSignature method.
				byte[] signature = null;
#if !MOONLIGHT
				if (!(privKey is RSACryptoServiceProvider))
#endif
				{
					try
					{
						signature = hash.CreateSignature((RSA)privKey);
					}
					catch (NotImplementedException)
					{ }
				}
				// If DecryptValue is not implemented, then try to export the private
				// key and let the RSAManaged class do the DecryptValue
				if (signature == null)
				{
					// RSAManaged of the selected ClientCertificate 
					// (at this moment the first one)
					RSA rsa = this.getClientCertRSA((RSA)privKey);

					// Write message
					signature = hash.CreateSignature(rsa);
				}
				this.Write((short)signature.Length);
				this.Write(signature, 0, signature.Length);
			}
		}

		protected override void ProcessAsTls1()
		{
			AsymmetricAlgorithm privKey = null;
			ClientContext		context = (ClientContext)this.Context;
			
			privKey = context.SslStream.RaisePrivateKeySelection(
				context.ClientSettings.ClientCertificate,
				context.ClientSettings.TargetHost);

			if (privKey == null)
			{
				throw new TlsException(AlertDescription.UserCancelled, "Client certificate Private Key unavailable.");
			}
			else
			{
				// Compute handshake messages hash
				MD5SHA1 hash = new MD5SHA1();
				hash.ComputeHash(
					context.HandshakeMessages.ToArray(),
					0,
					(int)context.HandshakeMessages.Length);

				// CreateSignature uses ((RSA)privKey).DecryptValue which is not implemented
				// in RSACryptoServiceProvider. Other implementations likely implement DecryptValue
				// so we will try the CreateSignature method.
				byte[] signature = null;
#if !MOONLIGHT
				if (!(privKey is RSACryptoServiceProvider))
#endif
				{
					try
					{
						signature = hash.CreateSignature((RSA)privKey);
					}
					catch (NotImplementedException)
					{ }
				}
				// If DecryptValue is not implemented, then try to export the private
				// key and let the RSAManaged class do the DecryptValue
				if (signature == null)
				{
					// RSAManaged of the selected ClientCertificate 
					// (at this moment the first one)
					RSA rsa = this.getClientCertRSA((RSA)privKey);

					// Write message
					signature = hash.CreateSignature(rsa);
				}
				this.Write((short)signature.Length);
				this.Write(signature, 0, signature.Length);
			}
		}

		#endregion

		#region Private methods

		private RSA getClientCertRSA(RSA privKey)
		{
			RSAParameters rsaParams		= new RSAParameters();
			RSAParameters privateParams = privKey.ExportParameters(true);

			// for RSA m_publickey contains 2 ASN.1 integers
			// the modulus and the public exponent
			ASN1 pubkey = new ASN1 (this.Context.ClientSettings.Certificates[0].GetPublicKey());
			ASN1 modulus = pubkey [0];
			if ((modulus == null) || (modulus.Tag != 0x02))
			{
				return null;
			}
			ASN1 exponent = pubkey [1];
			if (exponent.Tag != 0x02)
			{
				return null;
			}

			rsaParams.Modulus = this.getUnsignedBigInteger(modulus.Value);
			rsaParams.Exponent = exponent.Value;

			// Set private key parameters
			rsaParams.D			= privateParams.D;
			rsaParams.DP		= privateParams.DP;
			rsaParams.DQ		= privateParams.DQ;
			rsaParams.InverseQ	= privateParams.InverseQ;
			rsaParams.P			= privateParams.P;
			rsaParams.Q			= privateParams.Q;			

			// BUG: MS BCL 1.0 can't import a key which 
			// isn't the same size as the one present in
			// the container.
			int keySize = (rsaParams.Modulus.Length << 3);
			RSAManaged rsa = new RSAManaged(keySize);
			rsa.ImportParameters (rsaParams);

			return (RSA)rsa;
		}

		private byte[] getUnsignedBigInteger(byte[] integer) 
		{
			if (integer [0] == 0x00) 
			{
				// this first byte is added so we're sure it's an unsigned integer
				// however we can't feed it into RSAParameters or DSAParameters
				int length = integer.Length - 1;
				byte[] uinteger = new byte [length];
				Buffer.BlockCopy (integer, 1, uinteger, 0, length);
				return uinteger;
			}
			else
			{
				return integer;
			}
		}

		#endregion
	}
}
