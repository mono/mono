//
// MD4.cs - Message Digest 4 Abstract class
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

public abstract class MD4 : HashAlgorithm {

	protected MD4 () 
	{
		// MD4 hash length are 128 bits long
		HashSizeValue = 128; 
	}

	public static new MD4 Create () 
	{
		// for this to work we must register ourself with CryptoConfig
		return Create ("MD4");
	}

	public static new MD4 Create (string hashName) 
	{
		object o = CryptoConfig.CreateFromName (hashName);
		// in case machine.config isn't configured to use any MD4 implementation
		if (o == null) {
			o = new MD4CryptoServiceProvider ();
		}
		return (MD4) o;
	}
}

}
