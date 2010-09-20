//
// Mono.Security.Cryptography.CryptoTools
//	Shared class for common cryptographic functionalities
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2008 Novell, Inc (http://www.novell.com)
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

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class KeyBuilder {
	
		static private RandomNumberGenerator rng;

		private KeyBuilder ()
		{
		}

		static RandomNumberGenerator Rng {
			get {
#if MOONLIGHT
				if (rng == null)
					rng = new RNGCryptoServiceProvider ();
#else
				if (rng == null)
					rng = RandomNumberGenerator.Create ();
#endif
				return rng;
			}
		}
	
		static public byte[] Key (int size) 
		{
			byte[] key = new byte [size];
			Rng.GetBytes (key);
			return key;
		}
	
		static public byte[] IV (int size) 
		{
			byte[] iv = new byte [size];
			Rng.GetBytes (iv);
			return iv;
		}
	}
	
	// Process an array as a sequence of blocks
#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class BlockProcessor {
		private ICryptoTransform transform;
		private byte[] block;
		private int blockSize;	// in bytes (not in bits)
		private int blockCount;
	
		public BlockProcessor (ICryptoTransform transform) 
			: this (transform, transform.InputBlockSize) {} 
	
		// some Transforms (like HashAlgorithm descendant) return 1 for
		// block size (which isn't their real internal block size)
		public BlockProcessor (ICryptoTransform transform, int blockSize)
		{
			this.transform = transform;
			this.blockSize = blockSize;
			block = new byte [blockSize];
		}
	
		~BlockProcessor () 
		{
			// zeroize our block (so we don't retain any information)
			Array.Clear (block, 0, blockSize);
		}
	
		public void Initialize ()
		{
			Array.Clear (block, 0, blockSize);
			blockCount = 0;
		}
	
		public void Core (byte[] rgb) 
		{
			Core (rgb, 0, rgb.Length);
		}
	
		public void Core (byte[] rgb, int ib, int cb) 
		{
			// 1. fill the rest of the "block"
			int n = System.Math.Min (blockSize - blockCount, cb);
			Buffer.BlockCopy (rgb, ib, block, blockCount, n); 
			blockCount += n;
	
			// 2. if block is full then transform it
			if (blockCount == blockSize) {
				transform.TransformBlock (block, 0, blockSize, block, 0);
	
				// 3. transform any other full block in specified buffer
				int b = (int) ((cb - n) / blockSize);
				for (int i=0; i < b; i++) {
					transform.TransformBlock (rgb, n + ib, blockSize, block, 0);
					n += blockSize;
				}
	
				// 4. if data is still present fill the "block" with the remainder
				blockCount = cb - n;
				if (blockCount > 0)
					Buffer.BlockCopy (rgb, n + ib, block, 0, blockCount);
			}
		}
	
		public byte[] Final () 
		{
			return transform.TransformFinalBlock (block, 0, blockCount);
		}
	}
}
