//
// RSAPKCS1KeyExchangeDeformatter.cs - Handles PKCS#1 v.1.5 keyex decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography { 

	public class RSAPKCS1KeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {
	
		private RSA rsa;
		private string param;
		private RandomNumberGenerator random;
	
		public RSAPKCS1KeyExchangeDeformatter () 
		{
			rsa = null;
		}
	
		public RSAPKCS1KeyExchangeDeformatter (AsymmetricAlgorithm key) 
		{
			SetKey (key);
		}
	
		public override string Parameters {
			get { return param; }
			set { param = value; }
		}
	
		public RandomNumberGenerator RNG {
			get { return random; }
			set { random = value; }
		}
	
		public override byte[] DecryptKeyExchange (byte[] rgbData) 
		{
			if (rsa == null)
				throw new CryptographicException ();
			return PKCS1.Decrypt_v15 (rsa, rgbData);
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
