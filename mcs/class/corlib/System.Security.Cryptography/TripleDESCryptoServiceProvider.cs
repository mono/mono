//
// TripleDESCryptoServiceProvider.cs: Default TripleDES implementation
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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
	// a.	FIPS PUB 46-3: TripleDES
	//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf
	// b.	ANSI X9.52
	//	not free :-(
	//	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E52%2D1998
	
	[ComVisible (true)]
	public sealed class TripleDESCryptoServiceProvider : TripleDES {
	
		public TripleDESCryptoServiceProvider ()
		{
		}
	
		public override void GenerateIV () 
		{
			IVValue = KeyBuilder.IV (BlockSizeValue >> 3);
		}
		
		public override void GenerateKey () 
		{
			KeyValue = TripleDESTransform.GetStrongKey ();
		}
		
		public override ICryptoTransform CreateDecryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new TripleDESTransform (this, false, rgbKey, rgbIV);
		}
		
		public override ICryptoTransform CreateEncryptor (byte[] rgbKey, byte[] rgbIV) 
		{
			return new TripleDESTransform (this, true, rgbKey, rgbIV);
		}
	}
	
	// TripleDES is just DES-EDE
	internal class TripleDESTransform : SymmetricTransform {
	
		// for encryption
		private DESTransform E1;
		private DESTransform D2;
		private DESTransform E3;
	
		// for decryption
		private DESTransform D1;
		private DESTransform E2;
		private DESTransform D3;
		
		public TripleDESTransform (TripleDES algo, bool encryption, byte[] key, byte[] iv) : base (algo, encryption, iv) 
		{
			if (key == null) {
				key = GetStrongKey ();
			}
			// note: checking weak keys also checks valid key length
			if (TripleDES.IsWeakKey (key)) {
				string msg = Locale.GetText ("This is a known weak key.");
				throw new CryptographicException (msg);
			}

			byte[] key1 = new byte [8];
			byte[] key2 = new byte [8];
			byte[] key3 = new byte [8];
			DES des = DES.Create ();
			Buffer.BlockCopy (key, 0, key1, 0, 8);
			Buffer.BlockCopy (key, 8, key2, 0, 8);
			if (key.Length == 16)
				Buffer.BlockCopy (key, 0, key3, 0, 8);
			else
				Buffer.BlockCopy (key, 16, key3, 0, 8);
	
			// note: some modes (like CFB) requires encryption when decrypting
			if ((encryption) || (algo.Mode == CipherMode.CFB)) {
				E1 = new DESTransform (des, true, key1, iv);
				D2 = new DESTransform (des, false, key2, iv);
				E3 = new DESTransform (des, true, key3, iv);
			}
			else {
				D1 = new DESTransform (des, false, key3, iv);
				E2 = new DESTransform (des, true, key2, iv);
				D3 = new DESTransform (des, false, key1, iv);
			}
		}
	
		// note: this method is garanteed to be called with a valid blocksize
		// for both input and output
		protected override void ECB (byte[] input, byte[] output) 
		{
			DESTransform.Permutation (input, output, DESTransform.ipTab, false);
			if (encrypt) {
				E1.ProcessBlock (output, output);
				D2.ProcessBlock (output, output);
				E3.ProcessBlock (output, output);
			}
			else {
				D1.ProcessBlock (output, output);
				E2.ProcessBlock (output, output);
				D3.ProcessBlock (output, output);
			}
			DESTransform.Permutation (output, output, DESTransform.fpTab, true);
		}

		static internal byte[] GetStrongKey ()
		{
			int size = DESTransform.BLOCK_BYTE_SIZE * 3;
			byte[] key = KeyBuilder.Key (size);
			while (TripleDES.IsWeakKey (key))
				key = KeyBuilder.Key (size);
			return key;
		}
	}
}

