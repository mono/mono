//
// HMACSHA384.cs: HMAC implementation using SHA384
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

	public class HMACSHA384 : HMAC {

		public HMACSHA384 () : this (KeyBuilder.Key (8)) {}

		public HMACSHA384 (byte[] rgbKey) : base () 
		{
			HashName = "SHA384";
			HashSizeValue = 384;
			Key = rgbKey;
		}

		~HMACSHA384 () 
		{
			Dispose (false);
		}
	}
}

#endif