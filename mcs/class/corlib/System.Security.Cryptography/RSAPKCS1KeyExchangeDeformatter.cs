//
// RSAPKCS1KeyExchangeDeformatter.cs - Handles PKCS#1 v.1.5 keyex decryption.
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
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
			get { return null; }
			set { ; }
		}
	
		public RandomNumberGenerator RNG {
			get { return random; }
			set { random = value; }
		}
	
		public override byte[] DecryptKeyExchange (byte[] rgbData) 
		{
			if (rsa == null) {
				throw new CryptographicUnexpectedOperationException (
					Locale.GetText ("No key pair available."));
			}
			return PKCS1.Decrypt_v15 (rsa, rgbData);
		}
	
		public override void SetKey (AsymmetricAlgorithm key) 
		{
			rsa = (RSA)key;
		}
	}
}
