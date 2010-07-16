//
// MD2Managed.cs - Message Digest 2 Managed Implementation
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2001-2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005,2010 Novell, Inc (http://www.novell.com)
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

namespace Mono.Security.Cryptography { 

	// References:
	// a.	RFC1319: The MD2 Message-Digest Algorithm
	//	http://www.ietf.org/rfc/rfc1319.txt

	public class MD2Managed : MD2 {

		private byte[] state;
		private byte[] checksum;
		private byte[] buffer;
		private int count;
		private byte[] x;

		/// <summary>
		/// Permutation of 0..255 constructed from the digits of pi. It gives a
		/// "random" nonlinear byte substitution operation.
		/// </summary>
		private static readonly byte[] PI_SUBST = {
			41, 46, 67, 201, 162, 216, 124, 1, 61, 54, 84, 161, 236, 240, 6,
			19, 98, 167, 5, 243, 192, 199, 115, 140, 152, 147, 43, 217, 188,
			76, 130, 202, 30, 155, 87, 60, 253, 212, 224, 22, 103, 66, 111, 24,
			138, 23, 229, 18, 190, 78, 196, 214, 218, 158, 222, 73, 160, 251,
			245, 142, 187, 47, 238, 122, 169, 104, 121, 145, 21, 178, 7, 63,
			148, 194, 16, 137, 11, 34, 95, 33, 128, 127, 93, 154, 90, 144, 50,
			39, 53, 62, 204, 231, 191, 247, 151, 3, 255, 25, 48, 179, 72, 165,
			181, 209, 215, 94, 146, 42, 172, 86, 170, 198, 79, 184, 56, 210,
			150, 164, 125, 182, 118, 252, 107, 226, 156, 116, 4, 241, 69, 157,
			112, 89, 100, 113, 135, 32, 134, 91, 207, 101, 230, 45, 168, 2, 27,
			96, 37, 173, 174, 176, 185, 246, 28, 70, 97, 105, 52, 64, 126, 15,
			85, 71, 163, 35, 221, 81, 175, 58, 195, 92, 249, 206, 186, 197,
			234, 38, 44, 83, 13, 110, 133, 40, 132, 9, 211, 223, 205, 244, 65,
			129, 77, 82, 106, 220, 55, 200, 108, 193, 171, 250, 36, 225, 123,
			8, 12, 189, 177, 74, 120, 136, 149, 139, 227, 99, 232, 109, 233,
			203, 213, 254, 59, 0, 29, 57, 242, 239, 183, 14, 102, 88, 208, 228,
			166, 119, 114, 248, 235, 117, 75, 10, 49, 68, 80, 180, 143, 237,
			31, 26, 219, 153, 141, 51, 159, 17, 131, 20 };

		private byte[] Padding (int nLength)
		{
			if (nLength > 0) {
				byte[] padding = new byte [nLength];
				for (int i = 0; i < padding.Length; i++)
					padding[i] = (byte) nLength;
				return padding;
			}
			return null;
		}

		//--- constructor -----------------------------------------------------------
		
		public MD2Managed () : base ()
		{
			// we allocate the context memory
			state = new byte [16];
			checksum = new byte [16];
			buffer = new byte [16];
			x = new byte [48];
			// the initialize our context
			Initialize ();
		}

		public override void Initialize ()
		{
			count = 0;
			Array.Clear (state, 0, 16);
			Array.Clear (checksum, 0, 16);
			Array.Clear (buffer, 0, 16);
			// Zeroize sensitive information
			Array.Clear (x, 0, 48);
		}

		protected override void HashCore (byte[] array, int ibStart, int cbSize)
		{
			int i;

			/* Update number of bytes mod 16 */
			int index = count;
			count = (int) (index + cbSize) & 0xf;

			int partLen = 16 - index;

			/* Transform as many times as possible. */
			if (cbSize >= partLen) {
				// MD2_memcpy((POINTER)&context->buffer[index], (POINTER)input, partLen);
				Buffer.BlockCopy (array, ibStart, buffer, index, partLen);
				// MD2Transform (context->state, context->checksum, context->buffer);
				MD2Transform (state, checksum, buffer, 0);

				for (i = partLen; i + 15 < cbSize; i += 16) {
					// MD2Transform (context->state, context->checksum, &input[i]);
					MD2Transform (state, checksum, array, ibStart + i);
				}

				index = 0;
			}
			else
				i = 0;

			/* Buffer remaining input */
			// MD2_memcpy((POINTER)&context->buffer[index], (POINTER)&input[i], inputLen-i);
			Buffer.BlockCopy (array, ibStart + i, buffer, index, (cbSize - i));
		}

		protected override byte[] HashFinal ()
		{
			// Pad out to multiple of 16. 
			int index = count;
			int padLen = 16 - index;

			// is padding needed ? required if length not a multiple of 16.
			if (padLen > 0)
				HashCore (Padding (padLen), 0, padLen);

			// Extend with checksum 
			HashCore (checksum, 0, 16);

			// Store state in digest
			byte[] digest = (byte[]) state.Clone ();

			// Zeroize sensitive information.
			Initialize ();

			return digest;
		}

		//--- private methods ---------------------------------------------------

		/// <summary>
		/// MD2 basic transformation. Transforms state and updates checksum
		/// based on block. 
		/// </summary>
		private void MD2Transform (byte[] state, byte[] checksum, byte[] block, int index)
		{
			/* Form encryption block from state, block, state ^ block. */
			// MD2_memcpy ((POINTER)x, (POINTER)state, 16);
			Buffer.BlockCopy (state, 0, x, 0, 16);
			// MD2_memcpy ((POINTER)x+16, (POINTER)block, 16);
			Buffer.BlockCopy (block, index, x, 16, 16);

			// for (i = 0; i < 16; i++) x[i+32] = state[i] ^ block[i];
			for (int i = 0; i < 16; i++)
				x [i+32] = (byte) ((byte) state [i] ^ (byte) block [index + i]);

			/* Encrypt block (18 rounds). */
			int t = 0;
			for (int i = 0; i < 18; i++) {
				for (int j = 0; j < 48; j++ ) 
					t = x [j] ^= PI_SUBST [t];
				t = (t + i) & 0xff;
			}

			/* Save new state */
			// MD2_memcpy ((POINTER)state, (POINTER)x, 16);
			Buffer.BlockCopy (x, 0, state, 0, 16);

			/* Update checksum. */
			t = checksum [15];
			for (int i = 0; i < 16; i++)
				t = checksum [i] ^= PI_SUBST [block [index + i] ^ t];
		}
	}
}
