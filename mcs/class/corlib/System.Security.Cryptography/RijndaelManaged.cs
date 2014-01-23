//
// System.Security.Cryptography.RijndaelManaged.cs
//
// Authors:
//	Mark Crichton (crichton@gimp.org)
//	Andrew Birkett (andy@nobugs.org)
//	Sebastien Pouliot (sebastien@ximian.com)
//	Kazuki Oikawa (kazuki@panicode.com)
//
// (C) 2002
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {
	
	// References:
	// a.	FIPS PUB 197: Advanced Encryption Standard
	//	http://csrc.nist.gov/publications/fips/fips197/fips-197.pdf
	// b.	Rijndael Specification
	//	http://csrc.nist.gov/CryptoToolkit/aes/rijndael/Rijndael-ammended.pdf
	
	[ComVisible (true)]
	public sealed class RijndaelManaged : Rijndael {
		
		public RijndaelManaged ()
		{
		}
		
		public override void GenerateIV ()
		{
			IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
		}
		
		public override void GenerateKey ()
		{
			KeyValue = KeyBuilder.Key (KeySizeValue >> 3);
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new RijndaelManagedTransform (this, false, rgbKey, rgbIV);
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new RijndaelManagedTransform (this, true, rgbKey, rgbIV);
		}
	}
}
