//
// RSAPKCS1KeyExchangeDeformatter.cs - Handles PKCS#1 v.1.5 keyex decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

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
		byte[] mask = rsa.DecryptValue (rgbData);
		// it's normal if length = expected length - 1
		// because the first byte is 0x00 and, as such, is ignored
		// as a BigInteger
		// First byte will be 0x02 (because 0x00 was ignored by BigInteger)
		if (mask[0] != 0x02)
			return null;
		// Next will be nonzero random octets(at least 8 bytes)
		// 0x00 marker
		int i = 1;
		for (; i < mask.Length; i++) {
			if (mask[i] == 0x00)
				break;
		}
		byte[] secret = null;
		// Last bytes will be the secret
		if (mask[i] == 0x00) {
			int len = mask.Length - i;
			secret = new byte [len];
			Array.Copy (mask, i, secret, 0, len);
		}
		return secret;
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
