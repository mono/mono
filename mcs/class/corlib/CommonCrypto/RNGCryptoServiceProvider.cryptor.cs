//
// RngCryptoServiceProvider.cryptor.cs: based on Mono's System.Security.Cryptography.RNGCryptoServiceProvider
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Sebastien Pouliot (sebastien@xamarun.com)
//
// (C) 2002
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012-2014 Xamarin Inc.
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

using Crimson.CommonCrypto;

// http://developer.apple.com/library/ios/#DOCUMENTATION/Security/Reference/RandomizationReference/Reference/reference.html
// we need to use the CommonCrypto implementation instead of the runtime-supported RNGCryptoServiceProvider
// since we have no guarantee (on iOS) about /dev/[u]random availability or quality
#if MONOTOUCH || XAMMAC
namespace System.Security.Cryptography {
	public class RNGCryptoServiceProvider : RandomNumberGenerator {
		public RNGCryptoServiceProvider ()
		{
		}
		
		public RNGCryptoServiceProvider (byte[] rgb)
		{
		}

		public RNGCryptoServiceProvider (CspParameters cspParams)
		{
		}

		public RNGCryptoServiceProvider (string str) 
		{
		}

		~RNGCryptoServiceProvider () 
		{
		}
		
		public override void GetBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");
					
			Cryptor.GetRandom (data);
		}

		unsafe internal void GetBytes (byte* data, IntPtr data_length)
		{
			Cryptor.GetRandom (data, data_length);
		}
		
		public override void GetNonZeroBytes (byte[] data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			byte[] random = new byte [data.Length * 2];
			int i = 0;
			// one pass should be enough but hey this is random ;-)
			while (i < data.Length) {
				Cryptor.GetRandom (random);
				for (int j=0; j < random.Length; j++) {
					if (i == data.Length)
						break;
					if (random [j] != 0)
						data [i++] = random [j];
				}
			}
		}
	}
}
#endif
