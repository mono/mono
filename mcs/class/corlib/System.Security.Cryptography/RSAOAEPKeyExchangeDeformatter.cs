//
// RSAOAEPKeyExchangeDeformatter.cs - Handles OAEP keyex decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {
	
		private RSA rsa;
		private string param;
	
		public RSAOAEPKeyExchangeDeformatter ()
		{
			rsa = null;
		}
	
		public RSAOAEPKeyExchangeDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override string Parameters {
			get { return param; }
			set { param = value; }
		}
	
		public override byte[] DecryptKeyExchange (byte[] rgbData) 
		{
			if (rsa == null)
				throw new CryptographicException ();
	
			SHA1 sha1 = SHA1.Create ();
			return PKCS1.Decrypt_OAEP (rsa, sha1, rgbData);
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			if (key is RSA) {
				rsa = (RSA)key;
			}
			else
				throw new CryptographicException ();
		}
	}
}
