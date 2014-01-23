//
// HMACSHA1.cs: Handles HMAC with SHA-1
//
// Author:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 198: The Keyed-Hash Message Authentication Code (HMAC), 2002 March.
	//	http://csrc.nist.gov/publications/fips/fips198/fips-198a.pdf
	// b.	Internet RFC 2104, HMAC, Keyed-Hashing for Message Authentication
	//	(include C source for HMAC-MD5)
	//	http://www.ietf.org/rfc/rfc2104.txt
	// c.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
	//	(include C source for HMAC-MD5 and HAMAC-SHA1)
	//	http://www.ietf.org/rfc/rfc2202.txt
	// d.	ANSI X9.71, Keyed Hash Message Authentication Code.
	//	not free :-(
	//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E71%2D2000

	[ComVisible (true)]
	public class HMACSHA1 : HMAC {

		public HMACSHA1 ()
			: this (KeyBuilder.Key (8))
		{
		}

		public HMACSHA1 (byte[] key)
		{
#if FULL_AOT_RUNTIME
			SetHash ("SHA1", new SHA1Managed ());
#else
			HashName = "SHA1";
#endif
			HashSizeValue = 160;
			Key = key;
		}

		public HMACSHA1 (byte[] key, bool useManagedSha1)
		{
			HashName = "System.Security.Cryptography.SHA1" + (useManagedSha1 ? "Managed" : "CryptoServiceProvider");
			HashSizeValue = 160;
			Key = key;
		}
	}
}
