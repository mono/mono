//
// MD2.cs - Message Digest 2 Abstract class
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2001-2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

public abstract class MD2 : HashAlgorithm {

	protected MD2 () 
	{
		// MD2 hash length are 128 bits long
		HashSizeValue = 128; 
	}

	public static new MD2 Create ()
	{
		// for this to work we must register ourself with CryptoConfig
		return Create ("MD2");
	}

	public static new MD2 Create (string hashName)
	{
		object o = CryptoConfig.CreateFromName (hashName);
		// in case machine.config isn't configured to use any MD2 implementation
		if (o == null) {
			o = new MD2CryptoServiceProvider ();
		}
		return (MD2) o;
	}
}

}
