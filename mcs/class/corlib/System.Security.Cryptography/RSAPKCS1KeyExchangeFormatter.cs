//
// RSAPKCS1KeyExchangeFormatter.cs: Handles PKCS#1 v.1.5 keyex encryption.
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
	
	// LAMESPEC: There seems no way to select a hash algorithm. The default 
	// algorithm, is SHA1 because the class use the PKCS1MaskGenerationMethod -
	// which default to SHA1.
	public class RSAPKCS1KeyExchangeFormatter: AsymmetricKeyExchangeFormatter
	{
		private RSA rsa;
		private RandomNumberGenerator random;
	
		public RSAPKCS1KeyExchangeFormatter ()
		{
		}
	
		public RSAPKCS1KeyExchangeFormatter (AsymmetricAlgorithm key)
		{
			SetKey (key);
		}
	
		public RandomNumberGenerator Rng 
		{
			get { return random; }
			set { random = value; }
		}
	
		public override string Parameters 
		{
			get { return "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />"; }
		}
	
		public override byte[] CreateKeyExchange (byte[] rgbData)
		{
			if (random == null)
				random = RandomNumberGenerator.Create ();  // create default
			return PKCS1.Encrypt_v15 (rsa, random, rgbData);
		}
	
		public override byte[] CreateKeyExchange (byte[] rgbData, Type symAlgType)
		{
			// documentation says that symAlgType is not used !?!
			return CreateKeyExchange (rgbData);
		}
	
		public override void SetKey (AsymmetricAlgorithm key)
		{
			rsa = (RSA) key;
		}
	}
}
