//
// HMACMD5.cs: HMAC implementation using MD5
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

	// References:
	// a.	Internet RFC 2104, HMAC, Keyed-Hashing for Message Authentication
	//	(include C source for HMAC-MD5)
	//	http://www.ietf.org/rfc/rfc2104.txt
	// b.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
	//	(include C source for HMAC-MD5 and HAMAC-SHA1)
	//	http://www.ietf.org/rfc/rfc2202.txt

	public class HMACMD5 : HMAC {

		public HMACMD5 () : this (KeyBuilder.Key (8)) {}

		public HMACMD5 (byte[] rgbKey) : base ()
		{
			HashName = "MD5";
			HashSizeValue = 128;
			Key = rgbKey;
		}

		~HMACMD5 () 
		{
			Dispose (false);
		}
	}
}

#endif