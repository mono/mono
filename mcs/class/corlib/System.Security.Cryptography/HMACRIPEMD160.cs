//
// HMACRIPEMD160.cs: HMAC implementation using RIPEMD160
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	public class HMACRIPEMD160 : HMAC {

		public HMACRIPEMD160 () : this (KeyBuilder.Key (8)) {}

		public HMACRIPEMD160 (byte[] rgbKey) : base () 
		{
			HashName = "RIPEMD160";
			HashSizeValue = 160;
			Key = rgbKey;
		}

		~HMACRIPEMD160 () 
		{
			Dispose (false);
		}
	}
}

#endif