//
// Mono.Security.Cryptography SHA224 class implementation
//	based on SHA256Managed class implementation (mscorlib.dll)
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2001 
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security.Cryptography;

namespace Mono.Security.Cryptography {
	
	public class SHA224Managed : SHA224 {

		private const int BLOCK_SIZE_BYTES =  64;
		private const int HASH_SIZE_BYTES  =  32;
		private uint[] _H;
		private uint[] K;
		private uint count;
		private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
		private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.

		public SHA224Managed ()
		{
			_H = new uint [8];
			_ProcessingBuffer = new byte [BLOCK_SIZE_BYTES];
			Initialize ();
		}

		private uint Ch (uint u, uint v, uint w) 
		{
			return (u&v) ^ (~u&w);
		}

		private uint Maj (uint u, uint v, uint w) 
		{
			return (u&v) ^ (u&w) ^ (v&w);
		}

		private uint Ro0 (uint x) 
		{
			return ((x >> 7) | (x << 25))
				^ ((x >> 18) | (x << 14))
				^ (x >> 3);
		}

		private uint Ro1 (uint x) 
		{
			return ((x >> 17) | (x << 15))
				^ ((x >> 19) | (x << 13))
				^ (x >> 10);
		}

		private uint Sig0 (uint x) 
		{
			return ((x >> 2) | (x << 30))
				^ ((x >> 13) | (x << 19))
				^ ((x >> 22) | (x << 10));
		}

		private uint Sig1 (uint x) 
		{
			return ((x >> 6) | (x << 26))
				^ ((x >> 11) | (x << 21))
				^ ((x >> 25) | (x << 7));
		}

		protected override void HashCore (byte[] rgb, int start, int size) 
		{
			int i;
			State = 1;

			if (_ProcessingBufferCount != 0) {
				if (size < (BLOCK_SIZE_BYTES - _ProcessingBufferCount)) {
					System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, size);
					_ProcessingBufferCount += size;
					return;
				}
				else {
					i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
					System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, i);
					ProcessBlock (_ProcessingBuffer, 0);
					_ProcessingBufferCount = 0;
					start += i;
					size -= i;
				}
			}

			for (i=0; i<size-size%BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES) {
				ProcessBlock (rgb, start+i);
			}

			if (size%BLOCK_SIZE_BYTES != 0) {
				System.Buffer.BlockCopy (rgb, size-size%BLOCK_SIZE_BYTES+start, _ProcessingBuffer, 0, size%BLOCK_SIZE_BYTES);
				_ProcessingBufferCount = size%BLOCK_SIZE_BYTES;
			}
		}
	
		protected override byte[] HashFinal () 
		{
			byte[] hash = new byte[28];
			int i, j;

			ProcessFinalBlock (_ProcessingBuffer, 0, _ProcessingBufferCount);

			for (i=0; i<7; i++) {
				for (j=0; j<4; j++) {
					hash[i*4+j] = (byte)(_H[i] >> (24-j*8));
				}
			}

			State = 0;
			return hash;
		}

		public override void Initialize () 
		{
			count = 0;
			_ProcessingBufferCount = 0;
		
			_H[0] = 0xC1059ED8;
			_H[1] = 0x367CD507;
			_H[2] = 0x3070DD17;
			_H[3] = 0xF70E5939;
			_H[4] = 0xFFC00B31;
			_H[5] = 0x68581511;
			_H[6] = 0x64F98FA7;
			_H[7] = 0xBEFA4FA4;
		}

		private void ProcessBlock (byte[] inputBuffer, int inputOffset) 
		{
			uint a, b, c, d, e, f, g, h;
			uint t1, t2;
			int i;
			uint[] buff;
		
			count += BLOCK_SIZE_BYTES;
		
			buff = new uint[64];

			for (i=0; i<16; i++) {
				buff[i] = ((uint)(inputBuffer[inputOffset+4*i]) << 24)
					| ((uint)(inputBuffer[inputOffset+4*i+1]) << 16)
					| ((uint)(inputBuffer[inputOffset+4*i+2]) <<  8)
					| ((uint)(inputBuffer[inputOffset+4*i+3]));
			}

		
			for (i=16; i<64; i++) {
				buff[i] = Ro1(buff[i-2]) + buff[i-7] + Ro0(buff[i-15]) + buff[i-16];
			}

			a = _H[0];
			b = _H[1];
			c = _H[2];
			d = _H[3];
			e = _H[4];
			f = _H[5];
			g = _H[6];
			h = _H[7];

			for (i=0; i<64; i++) {
				t1 = h + Sig1(e) + Ch(e,f,g) + SHAConstants.K1 [i] + buff[i];
				t2 = Sig0(a) + Maj(a,b,c);
				h = g;
				g = f;
				f = e;
				e = d + t1;
				d = c;
				c = b;
				b = a;
				a = t1 + t2;
			}

			_H[0] += a;
			_H[1] += b;
			_H[2] += c;
			_H[3] += d;
			_H[4] += e;
			_H[5] += f;
			_H[6] += g;
			_H[7] += h;
		}
	
		private void ProcessFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			byte[] fooBuffer;
			int paddingSize;
			int i;
			uint size;

			paddingSize = (int)(56 - (inputCount + count) % BLOCK_SIZE_BYTES);

			if (paddingSize < 1)
				paddingSize += BLOCK_SIZE_BYTES;

			fooBuffer = new byte[inputCount+paddingSize+8];

			for (i=0; i<inputCount; i++) {
				fooBuffer[i] = inputBuffer[i+inputOffset];
			}

			fooBuffer[inputCount] = 0x80;
			for (i=inputCount+1; i<inputCount+paddingSize; i++) {
				fooBuffer[i] = 0x00;
			}

			size = (uint)(count+inputCount);
			size *= 8;

			fooBuffer[inputCount+paddingSize]   = 0x00;
			fooBuffer[inputCount+paddingSize+1] = 0x00;
			fooBuffer[inputCount+paddingSize+2] = 0x00;
			fooBuffer[inputCount+paddingSize+3] = 0x00;

			fooBuffer[inputCount+paddingSize+4] = (byte)((size) >> 24);
			fooBuffer[inputCount+paddingSize+5] = (byte)((size) >> 16);
			fooBuffer[inputCount+paddingSize+6] = (byte)((size) >>  8);
			fooBuffer[inputCount+paddingSize+7] = (byte)((size) >>  0);

			ProcessBlock(fooBuffer, 0);

			if (inputCount+paddingSize+8 == 128) {
				ProcessBlock(fooBuffer, 64);
			}
		}
	}
}

