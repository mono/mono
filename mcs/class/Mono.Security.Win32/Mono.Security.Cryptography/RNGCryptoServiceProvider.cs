//
// Mono.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security.Cryptography {
	
public class RNGCryptoServiceProvider : RandomNumberGenerator {

	private CapiRandomNumberGenerator rng;
	private byte[] seed;

	public RNGCryptoServiceProvider () 
	{
		rng = new CapiRandomNumberGenerator ();
		seed = null;
	}

	public RNGCryptoServiceProvider (byte[] rgb) 
	{
		rng = new CapiRandomNumberGenerator ();
		seed = rgb;
	}

	public RNGCryptoServiceProvider (CspParameters cspParams) 
	{
		rng = new CapiRandomNumberGenerator (cspParams);
		seed = null;
	}

	public RNGCryptoServiceProvider (string str) 
	{
		rng = new CapiRandomNumberGenerator ();
		seed = Encoding.Default.GetBytes (str);
	}

	~RNGCryptoServiceProvider () 
	{
		// zeroize seed
		if (seed != null)
			Array.Clear (seed, 0, seed.Length);
		// release unmanaged resources
		rng.Dispose ();
	}

	public override void GetBytes (byte[] data) 
	{
		if (data == null)
			throw new ArgumentNullException ("data");

		// send the seed
		if (seed != null)
			rng.GenRandom (seed);	
		// note: by doing this seed is modified each time

		rng.GenRandom (data);

		// generate random
		if (!rng.Result)
			throw new CryptographicException (rng.Error);
	}

	public override void GetNonZeroBytes (byte[] data) 
	{
		byte[] random = new byte [data.Length * 2];
		int i = 0;
		// one pass should be enough but hey this is random ;-)
		while (i < data.Length) {
			GetBytes (random);
			for (int j=0; j < random.Length; j++) {
				if (i == data.Length)
					break;
				if (random [j] != 0)
					data [i++] = random [j];
			}
		}
	}
}

}
