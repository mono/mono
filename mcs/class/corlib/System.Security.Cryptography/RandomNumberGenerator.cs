//
// System.Security.Cryptography.RandomNumberGenerator
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using System.Globalization;

namespace System.Security.Cryptography {
	
public abstract class RandomNumberGenerator  {
	
	public RandomNumberGenerator () {}

	public static RandomNumberGenerator Create () 
        {
		// create the default random number generator
		return Create ("System.Security.Cryptography.RandomNumberGenerator");
	}

	public static RandomNumberGenerator Create (string rngName) 
        {
		return (RandomNumberGenerator) (CryptoConfig.CreateFromName (rngName));
	}

	public abstract void GetBytes (byte[] data);

	public abstract void GetNonZeroBytes (byte[] data);
	
}

}
