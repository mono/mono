//
// RSAOAEPKeyExchangeDeformatter.cs - Handles OAEP keyex decryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter {

	protected RSA rsa;
	protected string param;

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
		byte[] mask = rsa.DecryptValue (rgbData);
		byte[] secret = null;
		// TODO retreive key from mask
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
