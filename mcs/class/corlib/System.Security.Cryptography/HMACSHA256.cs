//
// HMACSHA256.cs: HMAC implementation using SHA256
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

	public class HMACSHA256 : HMAC {

		public HMACSHA256 () : this (KeyBuilder.Key (8)) {}

		public HMACSHA256 (byte[] rgbKey) : base () 
		{
			HashName = "SHA256";
			HashSizeValue = 256;
			Key = rgbKey;
		}

		~HMACSHA256 () 
		{
			Dispose (false);
		}
	}
}

#endif