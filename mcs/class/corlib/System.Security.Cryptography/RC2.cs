//
// System.Security.Cryptography.RC2.cs
//
// Authors: 
//	Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//
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
	// a.	IETF RFC2286: A Description of the RC2(r) Encryption Algorithm
	//	http://www.ietf.org/rfc/rfc2268.txt

	[ComVisible (true)]
	public abstract class RC2 : SymmetricAlgorithm {

		public static new RC2 Create () 
		{
			return Create ("System.Security.Cryptography.RC2");
		}
		
		public static new RC2 Create (string AlgName) 
		{
			return (RC2) CryptoConfig.CreateFromName (AlgName);
		}

		protected int EffectiveKeySizeValue;

		public virtual int EffectiveKeySize {
			get {
				if (EffectiveKeySizeValue == 0)
					return KeySizeValue;
				else
					return EffectiveKeySizeValue;
			}
			set {
				EffectiveKeySizeValue = value; 
			}
		}

		public override int KeySize {
			get { return base.KeySize; }
			set {
				base.KeySize = value;
				EffectiveKeySizeValue = value;
			}
		}

		protected RC2 ()
		{
			KeySizeValue = 128;
			BlockSizeValue = 64;
			FeedbackSizeValue = 8;

			// The RFC allows keys of 1 to 128 bytes, but MS impl only supports
			// 40 to 128 bits, sigh.
			LegalKeySizesValue = new KeySizes [1];
			LegalKeySizesValue [0] = new KeySizes (40, 128, 8);

			LegalBlockSizesValue = new KeySizes [1];
			LegalBlockSizesValue [0] = new KeySizes (64, 64, 0);
		}
	}
}

