//
// Mono.Security.Cryptography.CapiRandomNumberGenerator
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

class CapiRandomNumberGenerator : CapiContext {

	public CapiRandomNumberGenerator () : base () {}

	public CapiRandomNumberGenerator (CspParameters cspParams) : base (cspParams) {}

	public void GenRandom (byte[] data) 
	{
		uint l = (uint) data.Length;
		InternalResult = CryptoAPI.CryptGenRandom (Handle, l, data);
	}
}

}
