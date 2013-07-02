//
// System.Security.Cryptography.Rijndael.cs
//
// Authors: Dan Lewis (dihlewis@yahoo.co.uk)
//          Andrew Birkett (andy@nobugs.org)
//
// (C) 2002
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 197: Advanced Encryption Standard
	//	http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf

	[ComVisible (true)]
	public abstract class Rijndael : SymmetricAlgorithm {

		public static new Rijndael Create () 
		{
#if FULL_AOT_RUNTIME
			return new System.Security.Cryptography.RijndaelManaged ();
#else
			return Create ("System.Security.Cryptography.Rijndael");
#endif
		}

		public static new Rijndael Create (string algName) 
		{
			return (Rijndael) CryptoConfig.CreateFromName (algName);
		}

		protected Rijndael ()
		{
			KeySizeValue = 256;
			BlockSizeValue = 128;
			FeedbackSizeValue = 128;
	
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (128, 256, 64);

			LegalBlockSizesValue = new KeySizes [1];
			LegalBlockSizesValue [0] = new KeySizes (128, 256, 64);
		}
	}
}
