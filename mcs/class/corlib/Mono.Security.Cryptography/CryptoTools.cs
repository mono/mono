//
// System.Security.Cryptography.CryptoTools
//	Shared class for common cryptographic functionalities
//
// Authors:
//   Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

internal class KeyBuilder {

	static private RandomNumberGenerator rng;

	static KeyBuilder () 
	{
		rng = RandomNumberGenerator.Create ();
	}

	static public byte[] Key (int size) 
	{
		byte[] key = new byte [size];
		rng.GetBytes (key);
		return key;
	}

	static public byte[] IV (int size) 
	{
		byte[] iv = new byte [size];
		rng.GetBytes (iv);
		return iv;
	}
}

}
