//
// RC4.cs: RC4(tm) symmetric stream cipher
//	RC4 is a trademark of RSA Security
//
// Author:
//	Sebastien Pouliot (sebastien@xamarin.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
//

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

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

#if !INSIDE_CORLIB
	public
#endif
	abstract class RC4 : SymmetricAlgorithm {

	private static KeySizes[] s_legalBlockSizes = {
		new KeySizes (64, 64, 0)
	};

	private static KeySizes[] s_legalKeySizes = {
		new KeySizes (40, 2048, 8)  
	};

	public RC4() 
	{
		KeySizeValue = 128;
		BlockSizeValue = 64;
		FeedbackSizeValue = BlockSizeValue;
		LegalBlockSizesValue = s_legalBlockSizes;
		LegalKeySizesValue = s_legalKeySizes;
	}

	// required for compatibility with .NET 2.0
	public override byte[] IV {
		get { return new byte [0]; }
		set { ; }
	}

	new static public RC4 Create() 
	{
#if FULL_AOT_RUNTIME
		return new ARC4Managed ();
#else
		return Create ("RC4");
#endif
	}

	new static public RC4 Create (string algName) 
	{
		object o = CryptoConfig.CreateFromName (algName);
		// in case machine.config isn't configured to use 
		// any RC4 implementation
		if (o == null) {
			o = new ARC4Managed ();
		}
		return (RC4) o;
	}
}

}
