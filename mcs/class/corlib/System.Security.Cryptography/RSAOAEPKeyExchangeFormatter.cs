//
// RSAOAEPKeyExchangeFormatter.cs - Handles OAEP keyex encryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter {

	private RSA rsa;
	private RandomNumberGenerator random;

	public RSAOAEPKeyExchangeFormatter () 
	{
		rsa = null;
	}

	public RSAOAEPKeyExchangeFormatter (AsymmetricAlgorithm key) 
	{
		SetKey (key);
	}

	public byte[] Parameter {
		get { return null; }
		set { ; }
	}

	public override string Parameters {
		get { return null; }
	}

	public RandomNumberGenerator Rng {
		get { return random; }
		set { random = value; }
	}

	public override byte[] CreateKeyExchange (byte[] rgbData) 
	{
		if (rsa == null)
			throw new CryptographicException ();
		if (random == null)
			random = RandomNumberGenerator.Create ();  // create default

		SHA1 sha1 = SHA1.Create ();
		return PKCS1.Encrypt_OAEP (rsa, sha1, random, rgbData);
	}

	public override byte[] CreateKeyExchange (byte[] rgbData, Type symAlgType) 
	{
		// documentation says that symAlgType is not used !?!
		// FIXME: must be the same as previous method ?
		return CreateKeyExchange (rgbData);
	}

	public override void SetKey (AsymmetricAlgorithm key) 
	{
		if (key is RSA) {
			rsa = (RSA) key;
		}
		else
			throw new CryptographicException ();
	}
}

}
