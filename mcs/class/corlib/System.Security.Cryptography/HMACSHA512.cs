//
// HMACSHA512.cs: HMAC implementation using SHA512
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

	public class HMACSHA512 : HMAC {

		public HMACSHA512 () : this (KeyBuilder.Key (8)) {}

		public HMACSHA512 (byte[] rgbKey) : base () 
		{
			HashName = "SHA512";
			HashSizeValue = 512;
			Key = rgbKey;
		}

		~HMACSHA512 () 
		{
			Dispose (false);
		}
	}
}

#endif