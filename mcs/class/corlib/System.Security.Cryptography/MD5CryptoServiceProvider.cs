//
// System.Security.Cryptography.MD5CryptoServiceProvider.cs
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright 2001 by Matthew S. Ford.
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

#if NET_1_0
	public class MD5CryptoServiceProvider : MD5 {
#else
	#if NET_2_0
	[ComVisible (true)]
	#endif
	public sealed class MD5CryptoServiceProvider : MD5 {
#endif
		private const int BLOCK_SIZE_BYTES =  64;
		private const int HASH_SIZE_BYTES  =  16;
		private uint[] _H;
		private uint[] buff;
		private ulong count;
		private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
		private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.
	
		public MD5CryptoServiceProvider () 
		{
			_H = new uint[4];
			buff = new uint[16];
			_ProcessingBuffer = new byte [BLOCK_SIZE_BYTES];

			Initialize();
		}

		~MD5CryptoServiceProvider () 
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing) 
		{
			if (_ProcessingBuffer != null) {
				Array.Clear (_ProcessingBuffer, 0, _ProcessingBuffer.Length);
				_ProcessingBuffer = null;
			}
			if (_H != null) {
				Array.Clear (_H, 0, _H.Length);
				_H = null;
			}
			if (buff != null) {
				Array.Clear (buff, 0, buff.Length);
				buff = null;
			}
		}

		protected override void HashCore (byte[] rgb, int ibStart, int cbSize) 
		{
			int i;
			State = 1;

			if (_ProcessingBufferCount != 0) {
				if (cbSize < (BLOCK_SIZE_BYTES - _ProcessingBufferCount)) {
					System.Buffer.BlockCopy (rgb, ibStart, _ProcessingBuffer, _ProcessingBufferCount, cbSize);
					_ProcessingBufferCount += cbSize;
					return;
				}
				else {
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
	
		protected override byte[] HashFinal () 
		{
			byte[] hash = new byte[16];
			int i, j;

			ProcessFinalBlock (_ProcessingBuffer, 0, _ProcessingBufferCount);

			for (i=0; i<4; i++) {
				for (j=0; j<4; j++) {
					hash[i*4+j] = (byte)(_H[i] >> j*8);
				}
			}

			return hash;
		}

		public override void Initialize () 
		{
			count = 0;
			_ProcessingBufferCount = 0;

			_H[0] = 0x67452301;
			_H[1] = 0xefcdab89;
			_H[2] = 0x98badcfe;
			_H[3] = 0x10325476;
		}

		private void ProcessBlock (byte[] inputBuffer, int inputOffset) 
		{
			uint a, b, c, d;
			int i;
		
			count += BLOCK_SIZE_BYTES;
		
			for (i=0; i<16; i++) {
				buff[i] = (uint)(inputBuffer[inputOffset+4*i])
					| (((uint)(inputBuffer[inputOffset+4*i+1])) <<  8)
					| (((uint)(inputBuffer[inputOffset+4*i+2])) << 16)
					| (((uint)(inputBuffer[inputOffset+4*i+3])) << 24);
			}
		
			a = _H[0];
			b = _H[1];
			c = _H[2];
			d = _H[3];
		
			// This function was unrolled because it seems to be doubling our performance with current compiler/VM.
			// Possibly roll up if this changes.

			// ---- Round 1 --------

			a += (((c ^ d) & b) ^ d) + (uint) K [0] + buff [0];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K [1] + buff [1];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K [2] + buff [2];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K [3] + buff [3];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K [4] + buff [4];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K [5] + buff [5];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K [6] + buff [6];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K [7] + buff [7];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K [8] + buff [8];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K [9] + buff [9];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K [10] + buff [10];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K [11] + buff [11];
			b = (b << 22) | (b >> 10);
			b += c;

			a += (((c ^ d) & b) ^ d) + (uint) K [12] + buff [12];
			a = (a << 7) | (a >> 25);
			a += b;

			d += (((b ^ c) & a) ^ c) + (uint) K [13] + buff [13];
			d = (d << 12) | (d >> 20);
			d += a;

			c += (((a ^ b) & d) ^ b) + (uint) K [14] + buff [14];
			c = (c << 17) | (c >> 15);
			c += d;

			b += (((d ^ a) & c) ^ a) + (uint) K [15] + buff [15];
			b = (b << 22) | (b >> 10);
			b += c;


			// ---- Round 2 --------
  
			a += (((b ^ c) & d) ^ c) + (uint) K [16] + buff [1];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K [17] + buff [6];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K [18] + buff [11];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K [19] + buff [0];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K [20] + buff [5];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K [21] + buff [10];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K [22] + buff [15];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K [23] + buff [4];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K [24] + buff [9];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K [25] + buff [14];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K [26] + buff [3];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K [27] + buff [8];
			b = (b << 20) | (b >> 12);
			b += c;

			a += (((b ^ c) & d) ^ c) + (uint) K [28] + buff [13];
			a = (a << 5) | (a >> 27);
			a += b;

			d += (((a ^ b) & c) ^ b) + (uint) K [29] + buff [2];
			d = (d << 9) | (d >> 23);
			d += a;

			c += (((d ^ a) & b) ^ a) + (uint) K [30] + buff [7];
			c = (c << 14) | (c >> 18);
			c += d;

			b += (((c ^ d) & a) ^ d) + (uint) K [31] + buff [12];
			b = (b << 20) | (b >> 12);
			b += c;


			// ---- Round 3 --------
  
			a += (b ^ c ^ d) + (uint) K [32] + buff [5];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K [33] + buff [8];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K [34] + buff [11];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K [35] + buff [14];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K [36] + buff [1];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K [37] + buff [4];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K [38] + buff [7];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K [39] + buff [10];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K [40] + buff [13];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K [41] + buff [0];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K [42] + buff [3];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K [43] + buff [6];
			b = (b << 23) | (b >> 9);
			b += c;

			a += (b ^ c ^ d) + (uint) K [44] + buff [9];
			a = (a << 4) | (a >> 28);
			a += b;

			d += (a ^ b ^ c) + (uint) K [45] + buff [12];
			d = (d << 11) | (d >> 21);
			d += a;

			c += (d ^ a ^ b) + (uint) K [46] + buff [15];
			c = (c << 16) | (c >> 16);
			c += d;

			b += (c ^ d ^ a) + (uint) K [47] + buff [2];
			b = (b << 23) | (b >> 9);
			b += c;


			// ---- Round 4 --------
  
			a += (((~d) | b) ^ c) + (uint) K [48] + buff [0];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K [49] + buff [7];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K [50] + buff [14];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K [51] + buff [5];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K [52] + buff [12];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K [53] + buff [3];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K [54] + buff [10];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K [55] + buff [1];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K [56] + buff [8];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K [57] + buff [15];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K [58] + buff [6];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K [59] + buff [13];
			b = (b << 21) | (b >> 11);
			b += c;

			a += (((~d) | b) ^ c) + (uint) K [60] + buff [4];
			a = (a << 6) | (a >> 26);
			a += b;

			d += (((~c) | a) ^ b) + (uint) K [61] + buff [11];
			d = (d << 10) | (d >> 22);
			d += a;

			c += (((~b) | d) ^ a) + (uint) K [62] + buff [2];
			c = (c << 15) | (c >> 17);
			c += d;

			b += (((~a) | c) ^ d) + (uint) K [63] + buff [9];
			b = (b << 21) | (b >> 11);
			b += c;

			_H [0] += a;
			_H [1] += b;
			_H [2] += c;
			_H [3] += d;
		}
		
		private void ProcessFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			ulong total = count + (ulong)inputCount;
			int paddingSize = (int)(56 - total % BLOCK_SIZE_BYTES);

			if (paddingSize < 1)
				paddingSize += BLOCK_SIZE_BYTES;

			byte[] fooBuffer = new byte [inputCount+paddingSize+8];

			for (int i=0; i<inputCount; i++) {
				fooBuffer[i] = inputBuffer[i+inputOffset];
			}

			fooBuffer[inputCount] = 0x80;
			for (int i=inputCount+1; i<inputCount+paddingSize; i++) {
				fooBuffer[i] = 0x00;
			}

			// I deal in bytes. The algorithm deals in bits.
			ulong size = total << 3;
			AddLength (size, fooBuffer, inputCount+paddingSize);
			ProcessBlock (fooBuffer, 0);

			if (inputCount+paddingSize+8 == 128) {
				ProcessBlock(fooBuffer, 64);
			}
		}

		internal void AddLength (ulong length, byte[] buffer, int position)
		{
			buffer [position++] = (byte)(length);
			buffer [position++] = (byte)(length >>  8);
			buffer [position++] = (byte)(length >> 16);
			buffer [position++] = (byte)(length >> 24);
			buffer [position++] = (byte)(length >> 32);
			buffer [position++] = (byte)(length >> 40);
			buffer [position++] = (byte)(length >> 48);
			buffer [position]   = (byte)(length >> 56);
		}

		private readonly static uint[] K = {
			0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
			0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 
			0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
			0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
			0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
			0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
			0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
			0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
			0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
			0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
			0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
			0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
			0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
			0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
			0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
			0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
		};
	}
}

