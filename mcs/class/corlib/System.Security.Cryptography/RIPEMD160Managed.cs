//
// RIPEMD160Managed.cs: Implements the RIPEMD-160 hash algorithm
//
// Author:
//	Pieter Philippaerts (Pieter@mentalis.org)
//
//   References:
//     - http://www.esat.kuleuven.ac.be/~cosicart/ps/AB-9601/
//
// (C) 2003 The Mentalis.org Team (http://www.mentalis.org/)
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

namespace System.Security.Cryptography {
	/// <summary>
	/// Computes the <see cref="RIPEMD160"/> hash for the input data.
	/// </summary>
	[ComVisible (true)]
	public class RIPEMD160Managed : RIPEMD160 { // not 'sealed' according to preliminary docs; this may change though
		/// <summary>
		/// Initializes a new instance of the <see cref="RIPEMD160Managed"/> class. This class cannot be inherited.
		/// </summary>
		public RIPEMD160Managed() {
			_X = new uint[16];
			_HashValue = new uint[5];
			_ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];
			Initialize();
		}
		/// <summary>
		/// Initializes an instance of <see cref="RIPEMD160Managed"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The RIPEMD160Managed instance has been disposed.</exception>
		public override void Initialize() {
			_HashValue[0] = 0x67452301;
			_HashValue[1] = 0xefcdab89;
			_HashValue[2] = 0x98badcfe;
			_HashValue[3] = 0x10325476;
			_HashValue[4] = 0xc3d2e1f0;
			_Length = 0;
			_ProcessingBufferCount = 0;
			Array.Clear (_X, 0, _X.Length);
			Array.Clear (_ProcessingBuffer, 0, _ProcessingBuffer.Length);
		}
		/// <summary>
		/// Routes data written to the object into the <see cref="RIPEMD160"/> hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The array of data bytes.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the array to use as data.</param>
		/// <exception cref="ObjectDisposedException">The <see cref="RIPEMD160Managed"/> instance has been disposed.</exception>
		protected override void HashCore(byte[] rgb, int ibStart, int cbSize) {
			int i;
			State = 1;

			_Length += (uint)cbSize; // global length

			if (_ProcessingBufferCount != 0) {
				if (cbSize < (BLOCK_SIZE_BYTES - _ProcessingBufferCount)) {
					System.Buffer.BlockCopy (rgb, ibStart, _ProcessingBuffer, _ProcessingBufferCount, cbSize);
					_ProcessingBufferCount += cbSize;
					return;
				} else {
					i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
					System.Buffer.BlockCopy (rgb, ibStart, _ProcessingBuffer, _ProcessingBufferCount, i);
					ProcessBlock (_ProcessingBuffer, 0);
					_ProcessingBufferCount = 0;
					ibStart += i;
					cbSize -= i;
				}
			}

			for (i = 0; i < cbSize - cbSize % BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES) {
				ProcessBlock (rgb, ibStart + i);
			}

			if (cbSize % BLOCK_SIZE_BYTES != 0) {
				System.Buffer.BlockCopy (rgb, cbSize - cbSize % BLOCK_SIZE_BYTES + ibStart, _ProcessingBuffer, 0, cbSize % BLOCK_SIZE_BYTES);
				_ProcessingBufferCount = cbSize % BLOCK_SIZE_BYTES;
			}
		}
		/// <summary>
		/// Returns the computed <see cref="RIPEMD160"/> hash as an array of bytes after all data has been written to the object.
		/// </summary>
		/// <returns>The computed hash value.</returns>
		/// <exception cref="ObjectDisposedException">The <see cref="RIPEMD160Managed"/> instance has been disposed.</exception>
		protected override byte[] HashFinal() {
			CompressFinal(_Length);
			byte[] hash = new byte[20];
			if (!BitConverter.IsLittleEndian) {
				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < 4; j++) {
						hash [i*4+j] = (byte)(_HashValue [i] >> j*8);
					}
				}
			} else {
				Buffer.BlockCopy (_HashValue, 0, hash, 0, 20);
			}
			return hash;
		}

		/// <summary>
		/// Processes one block of data.
		/// </summary>
		/// <param name="buffer">The buffer with the data.</param>
		/// <param name="offset">The offset in the buffer.</param>
		private void ProcessBlock (byte[] buffer, int offset)
		{
			if (!BitConverter.IsLittleEndian) {
				for (int i=0; i < _X.Length; i++) {
					_X [i] = (uint)(buffer [offset])
						| (((uint)(buffer [offset+1])) <<  8)
						| (((uint)(buffer [offset+2])) << 16)
						| (((uint)(buffer [offset+3])) << 24);
					offset += 4;
				}
			} else {
				Buffer.BlockCopy (buffer, offset, _X, 0, 64);
			}
			Compress();
		}

		private void Compress() {
			uint aa = _HashValue[0],  bb = _HashValue[1],  cc = _HashValue[2],  dd = _HashValue[3],  ee = _HashValue[4];
			uint aaa = _HashValue[0], bbb = _HashValue[1], ccc = _HashValue[2], ddd = _HashValue[3], eee = _HashValue[4];
			/* round 1 */
			FF(ref aa, bb, ref cc, dd, ee, _X[ 0], 11);
			FF(ref ee, aa, ref bb, cc, dd, _X[ 1], 14);
			FF(ref dd, ee, ref aa, bb, cc, _X[ 2], 15);
			FF(ref cc, dd, ref ee, aa, bb, _X[ 3], 12);
			FF(ref bb, cc, ref dd, ee, aa, _X[ 4],  5);
			FF(ref aa, bb, ref cc, dd, ee, _X[ 5],  8);
			FF(ref ee, aa, ref bb, cc, dd, _X[ 6],  7);
			FF(ref dd, ee, ref aa, bb, cc, _X[ 7],  9);
			FF(ref cc, dd, ref ee, aa, bb, _X[ 8], 11);
			FF(ref bb, cc, ref dd, ee, aa, _X[ 9], 13);
			FF(ref aa, bb, ref cc, dd, ee, _X[10], 14);
			FF(ref ee, aa, ref bb, cc, dd, _X[11], 15);
			FF(ref dd, ee, ref aa, bb, cc, _X[12],  6);
			FF(ref cc, dd, ref ee, aa, bb, _X[13],  7);
			FF(ref bb, cc, ref dd, ee, aa, _X[14],  9);
			FF(ref aa, bb, ref cc, dd, ee, _X[15],  8);
			/* round 2 */
			GG(ref ee, aa, ref bb, cc, dd, _X[ 7],  7);
			GG(ref dd, ee, ref aa, bb, cc, _X[ 4],  6);
			GG(ref cc, dd, ref ee, aa, bb, _X[13],  8);
			GG(ref bb, cc, ref dd, ee, aa, _X[ 1], 13);
			GG(ref aa, bb, ref cc, dd, ee, _X[10], 11);
			GG(ref ee, aa, ref bb, cc, dd, _X[ 6],  9);
			GG(ref dd, ee, ref aa, bb, cc, _X[15],  7);
			GG(ref cc, dd, ref ee, aa, bb, _X[ 3], 15);
			GG(ref bb, cc, ref dd, ee, aa, _X[12],  7);
			GG(ref aa, bb, ref cc, dd, ee, _X[ 0], 12);
			GG(ref ee, aa, ref bb, cc, dd, _X[ 9], 15);
			GG(ref dd, ee, ref aa, bb, cc, _X[ 5],  9);
			GG(ref cc, dd, ref ee, aa, bb, _X[ 2], 11);
			GG(ref bb, cc, ref dd, ee, aa, _X[14],  7);
			GG(ref aa, bb, ref cc, dd, ee, _X[11], 13);
			GG(ref ee, aa, ref bb, cc, dd, _X[ 8], 12);
			/* round 3 */
			HH(ref dd, ee, ref aa, bb, cc, _X[ 3], 11);
			HH(ref cc, dd, ref ee, aa, bb, _X[10], 13);
			HH(ref bb, cc, ref dd, ee, aa, _X[14],  6);
			HH(ref aa, bb, ref cc, dd, ee, _X[ 4],  7);
			HH(ref ee, aa, ref bb, cc, dd, _X[ 9], 14);
			HH(ref dd, ee, ref aa, bb, cc, _X[15],  9);
			HH(ref cc, dd, ref ee, aa, bb, _X[ 8], 13);
			HH(ref bb, cc, ref dd, ee, aa, _X[ 1], 15);
			HH(ref aa, bb, ref cc, dd, ee, _X[ 2], 14);
			HH(ref ee, aa, ref bb, cc, dd, _X[ 7],  8);
			HH(ref dd, ee, ref aa, bb, cc, _X[ 0], 13);
			HH(ref cc, dd, ref ee, aa, bb, _X[ 6],  6);
			HH(ref bb, cc, ref dd, ee, aa, _X[13],  5);
			HH(ref aa, bb, ref cc, dd, ee, _X[11], 12);
			HH(ref ee, aa, ref bb, cc, dd, _X[ 5],  7);
			HH(ref dd, ee, ref aa, bb, cc, _X[12],  5);
			/* round 4 */
			II(ref cc, dd, ref ee, aa, bb, _X[ 1], 11);
			II(ref bb, cc, ref dd, ee, aa, _X[ 9], 12);
			II(ref aa, bb, ref cc, dd, ee, _X[11], 14);
			II(ref ee, aa, ref bb, cc, dd, _X[10], 15);
			II(ref dd, ee, ref aa, bb, cc, _X[ 0], 14);
			II(ref cc, dd, ref ee, aa, bb, _X[ 8], 15);
			II(ref bb, cc, ref dd, ee, aa, _X[12],  9);
			II(ref aa, bb, ref cc, dd, ee, _X[ 4],  8);
			II(ref ee, aa, ref bb, cc, dd, _X[13],  9);
			II(ref dd, ee, ref aa, bb, cc, _X[ 3], 14);
			II(ref cc, dd, ref ee, aa, bb, _X[ 7],  5);
			II(ref bb, cc, ref dd, ee, aa, _X[15],  6);
			II(ref aa, bb, ref cc, dd, ee, _X[14],  8);
			II(ref ee, aa, ref bb, cc, dd, _X[ 5],  6);
			II(ref dd, ee, ref aa, bb, cc, _X[ 6],  5);
			II(ref cc, dd, ref ee, aa, bb, _X[ 2], 12);
			/* round 5 */
			JJ(ref bb, cc, ref dd, ee, aa, _X[ 4],  9);
			JJ(ref aa, bb, ref cc, dd, ee, _X[ 0], 15);
			JJ(ref ee, aa, ref bb, cc, dd, _X[ 5],  5);
			JJ(ref dd, ee, ref aa, bb, cc, _X[ 9], 11);
			JJ(ref cc, dd, ref ee, aa, bb, _X[ 7],  6);
			JJ(ref bb, cc, ref dd, ee, aa, _X[12],  8);
			JJ(ref aa, bb, ref cc, dd, ee, _X[ 2], 13);
			JJ(ref ee, aa, ref bb, cc, dd, _X[10], 12);
			JJ(ref dd, ee, ref aa, bb, cc, _X[14],  5);
			JJ(ref cc, dd, ref ee, aa, bb, _X[ 1], 12);
			JJ(ref bb, cc, ref dd, ee, aa, _X[ 3], 13);
			JJ(ref aa, bb, ref cc, dd, ee, _X[ 8], 14);
			JJ(ref ee, aa, ref bb, cc, dd, _X[11], 11);
			JJ(ref dd, ee, ref aa, bb, cc, _X[ 6],  8);
			JJ(ref cc, dd, ref ee, aa, bb, _X[15],  5);
			JJ(ref bb, cc, ref dd, ee, aa, _X[13],  6);
			/* parallel round 1 */
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, _X[ 5],  8);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, _X[14],  9);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, _X[ 7],  9);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, _X[ 0], 11);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, _X[ 9], 13);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, _X[ 2], 15);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, _X[11], 15);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, _X[ 4],  5);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, _X[13],  7);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, _X[ 6],  7);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, _X[15],  8);
			JJJ(ref eee, aaa, ref bbb, ccc, ddd, _X[ 8], 11);
			JJJ(ref ddd, eee, ref aaa, bbb, ccc, _X[ 1], 14);
			JJJ(ref ccc, ddd, ref eee, aaa, bbb, _X[10], 14);
			JJJ(ref bbb, ccc, ref ddd, eee, aaa, _X[ 3], 12);
			JJJ(ref aaa, bbb, ref ccc, ddd, eee, _X[12],  6);
			/* parallel round 2 */
			III(ref eee, aaa, ref bbb, ccc, ddd, _X[ 6],  9); 
			III(ref ddd, eee, ref aaa, bbb, ccc, _X[11], 13);
			III(ref ccc, ddd, ref eee, aaa, bbb, _X[ 3], 15);
			III(ref bbb, ccc, ref ddd, eee, aaa, _X[ 7],  7);
			III(ref aaa, bbb, ref ccc, ddd, eee, _X[ 0], 12);
			III(ref eee, aaa, ref bbb, ccc, ddd, _X[13],  8);
			III(ref ddd, eee, ref aaa, bbb, ccc, _X[ 5],  9);
			III(ref ccc, ddd, ref eee, aaa, bbb, _X[10], 11);
			III(ref bbb, ccc, ref ddd, eee, aaa, _X[14],  7);
			III(ref aaa, bbb, ref ccc, ddd, eee, _X[15],  7);
			III(ref eee, aaa, ref bbb, ccc, ddd, _X[ 8], 12);
			III(ref ddd, eee, ref aaa, bbb, ccc, _X[12],  7);
			III(ref ccc, ddd, ref eee, aaa, bbb, _X[ 4],  6);
			III(ref bbb, ccc, ref ddd, eee, aaa, _X[ 9], 15);
			III(ref aaa, bbb, ref ccc, ddd, eee, _X[ 1], 13);
			III(ref eee, aaa, ref bbb, ccc, ddd, _X[ 2], 11);
			/* parallel round 3 */
			HHH(ref ddd, eee, ref aaa, bbb, ccc, _X[15],  9);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, _X[ 5],  7);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, _X[ 1], 15);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, _X[ 3], 11);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, _X[ 7],  8);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, _X[14],  6);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, _X[ 6],  6);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, _X[ 9], 14);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, _X[11], 12);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, _X[ 8], 13);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, _X[12],  5);
			HHH(ref ccc, ddd, ref eee, aaa, bbb, _X[ 2], 14);
			HHH(ref bbb, ccc, ref ddd, eee, aaa, _X[10], 13);
			HHH(ref aaa, bbb, ref ccc, ddd, eee, _X[ 0], 13);
			HHH(ref eee, aaa, ref bbb, ccc, ddd, _X[ 4],  7);
			HHH(ref ddd, eee, ref aaa, bbb, ccc, _X[13],  5);
			/* parallel round 4 */   
			GGG(ref ccc, ddd, ref eee, aaa, bbb, _X[ 8], 15);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, _X[ 6],  5);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, _X[ 4],  8);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, _X[ 1], 11);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, _X[ 3], 14);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, _X[11], 14);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, _X[15],  6);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, _X[ 0], 14);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, _X[ 5],  6);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, _X[12],  9);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, _X[ 2], 12);
			GGG(ref bbb, ccc, ref ddd, eee, aaa, _X[13],  9);
			GGG(ref aaa, bbb, ref ccc, ddd, eee, _X[ 9], 12);
			GGG(ref eee, aaa, ref bbb, ccc, ddd, _X[ 7],  5);
			GGG(ref ddd, eee, ref aaa, bbb, ccc, _X[10], 15);
			GGG(ref ccc, ddd, ref eee, aaa, bbb, _X[14],  8);
			/* parallel round 5 */
			FFF(ref bbb, ccc, ref ddd, eee, aaa, _X[12],  8);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, _X[15],  5);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, _X[10], 12);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, _X[ 4],  9);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, _X[ 1], 12);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, _X[ 5],  5);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, _X[ 8], 14);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, _X[ 7],  6);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, _X[ 6],  8);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, _X[ 2], 13);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, _X[13],  6);
			FFF(ref aaa, bbb, ref ccc, ddd, eee, _X[14],  5);
			FFF(ref eee, aaa, ref bbb, ccc, ddd, _X[ 0], 15);
			FFF(ref ddd, eee, ref aaa, bbb, ccc, _X[ 3], 13);
			FFF(ref ccc, ddd, ref eee, aaa, bbb, _X[ 9], 11);
			FFF(ref bbb, ccc, ref ddd, eee, aaa, _X[11], 11);
			/* combine results */
			ddd += cc + _HashValue[1];               /* final result for _HashValue[0] */
			_HashValue[1] = _HashValue[2] + dd + eee;
			_HashValue[2] = _HashValue[3] + ee + aaa;
			_HashValue[3] = _HashValue[4] + aa + bbb;
			_HashValue[4] = _HashValue[0] + bb + ccc;
			_HashValue[0] = ddd;
		}
		private void CompressFinal(ulong length) {
			uint lswlen = (uint)(length & 0xFFFFFFFF);
			uint mswlen = (uint)(length >> 32);
			// clear _X
			Array.Clear(_X, 0, _X.Length);
			// put bytes from _ProcessingBuffer into _X
			int ptr = 0;
			for (uint i = 0; i < (lswlen & 63); i++) {
				// byte i goes into word X[i div 4] at pos.  8*(i mod 4)
				_X[i >> 2] ^= ((uint)_ProcessingBuffer[ptr++]) << (int)(8 * (i & 3));
			}
			// append the bit m_n == 1
			_X[(lswlen >> 2) & 15] ^= (uint)1 << (int)(8 * (lswlen & 3) + 7);
			if ((lswlen & 63) > 55) {
				// length goes to next block
				Compress();
				Array.Clear(_X, 0, _X.Length);
			}
			// append length in bits
			_X[14] = lswlen << 3;
			_X[15] = (lswlen >> 29) | (mswlen << 3);
			Compress();
		}

		// the following methods should be inlined by the compiler
		private uint ROL(uint x, int n) {
			return (((x) << (n)) | ((x) >> (32-(n))));
		}
		private uint F(uint x, uint y, uint z) {
			return ((x) ^ (y) ^ (z)) ;
		}
		private uint G(uint x, uint y, uint z) {
			return (((x) & (y)) | (~(x) & (z)));
		}
		private uint H(uint x, uint y, uint z) {
			return (((x) | ~(y)) ^ (z));
		}
		private uint I(uint x, uint y, uint z) {
			return (((x) & (z)) | ((y) & ~(z)));
		}
		private uint J(uint x, uint y, uint z) {
			return ((x) ^ ((y) | ~(z)));
		}
		private void FF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += F(b, c, d) + x;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void GG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += G(b, c, d) + x + 0x5a827999;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void HH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += H(b, c, d) + x + 0x6ed9eba1;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void II(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += I(b, c, d) + x + 0x8f1bbcdc;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void JJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += J(b, c, d) + x + 0xa953fd4e;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void FFF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += F(b, c, d) + x;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void GGG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += G(b, c, d) + x + 0x7a6d76e9;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void HHH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += H(b, c, d) + x + 0x6d703ef3;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void III(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += I(b, c, d) + x + 0x5c4dd124;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}
		private void JJJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s) {
			a += J(b, c, d) + x + 0x50a28be6;
			a = ROL(a, s) + e;
			c = ROL(c, 10);
		}

		/// <summary>
		/// A buffer that holds the extra data.
		/// </summary>
		private byte[] _ProcessingBuffer;
		/// <summary>
		/// The X vectors.
		/// </summary>
		private uint[] _X;
		/// <summary>
		/// The current value of the hash.
		/// </summary>
		private uint[] _HashValue;
		/// <summary>
		/// The number of bytes hashed.
		/// </summary>
		private ulong _Length;

		private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.

		private const int BLOCK_SIZE_BYTES =  64;
	}
}
