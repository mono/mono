//
// KeyedHashAlgorithm.cs: Handles keyed hash and MAC classes.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace System.Security.Cryptography {

public abstract class KeyedHashAlgorithm : HashAlgorithm {
	
	protected byte[] KeyValue;

	protected KeyedHashAlgorithm () : base () 
	{
		// create a random 64 bits key
	}

	~KeyedHashAlgorithm () 
	{
		Dispose (false);
	}

	public virtual byte[] Key {
		get { 
			return (byte[]) KeyValue.Clone (); 
		}
		set { 
			// can't change the key during a hashing ops
			if (State != 0)
				throw new CryptographicException ();
			// zeroize current key material for security
			ZeroizeKey ();
			// copy new key
			KeyValue = (byte[]) value.Clone (); 
		}
	}

	protected override void Dispose (bool disposing)
	{
                // zeroize key material for security
		ZeroizeKey();
		// dispose managed resources
                // none so far
		// dispose unmanaged resources 
                // none so far
		// calling base class HashAlgorithm
		base.Dispose (disposing);
	}

	private void ZeroizeKey() 
	{
		if (KeyValue != null)
			Array.Clear (KeyValue, 0, KeyValue.Length);
	}

	public static new KeyedHashAlgorithm Create ()
	{
		return Create ("System.Security.Cryptography.KeyedHashAlgorithm");
	}

	public static new KeyedHashAlgorithm Create (string algName)
	{
		return (KeyedHashAlgorithm) CryptoConfig.CreateFromName (algName);
	}
}

}