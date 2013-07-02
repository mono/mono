//
// System.Security.Cryptography.SHA384Managed.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002
// Implementation translated from Bouncy Castle JCE (http://www.bouncycastle.org/)
// See bouncycastle.txt for license.
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
	
[ComVisible (true)]
public class SHA384Managed : SHA384 {

	private byte[] xBuf;
	private int xBufOff;

	private ulong byteCount1;
	private ulong byteCount2;

	private ulong H1, H2, H3, H4, H5, H6, H7, H8;
	private ulong[] W;
	private int wOff;

	public SHA384Managed () 
	{
		xBuf = new byte [8];
		W = new ulong [80];
		Initialize (false); // limited initialization
	}

	private void Initialize (bool reuse) 
	{
		// SHA-384 initial hash value
		// The first 64 bits of the fractional parts of the square roots
		// of the 9th through 16th prime numbers
		H1 = 0xcbbb9d5dc1059ed8L;
		H2 = 0x629a292a367cd507L;
		H3 = 0x9159015a3070dd17L;
		H4 = 0x152fecd8f70e5939L;
		H5 = 0x67332667ffc00b31L;
		H6 = 0x8eb44a8768581511L;
		H7 = 0xdb0c2e0d64f98fa7L;
		H8 = 0x47b5481dbefa4fa4L;

		if (reuse) {
			byteCount1 = 0;
			byteCount2 = 0;

			xBufOff = 0;
			for (int i = 0; i < xBuf.Length; i++) 
				xBuf [i] = 0;

			wOff = 0;
			for (int i = 0; i != W.Length; i++)
				W [i] = 0;
		}
	}

	public override void Initialize () 
	{
		Initialize (true); // reuse instance
	}

	// protected

	protected override void HashCore (byte[] rgb, int ibStart, int cbSize) 
	{
		// fill the current word
		while ((xBufOff != 0) && (cbSize > 0)) {
			update (rgb [ibStart]);
			ibStart++;
			cbSize--;
		}

		// process whole words.
		while (cbSize > xBuf.Length) {
			processWord (rgb, ibStart);
			ibStart += xBuf.Length;
			cbSize -= xBuf.Length;
			byteCount1 += (ulong) xBuf.Length;
		}

		// load in the remainder.
		while (cbSize > 0) {
			update (rgb [ibStart]);
			ibStart++;
			cbSize--;
		}
	}

	protected override byte[] HashFinal () 
	{
		adjustByteCounts ();

		ulong lowBitLength = byteCount1 << 3;
		ulong hiBitLength = byteCount2;

		// add the pad bytes.
		update ( (byte) 128);
		while (xBufOff != 0)
			update ( (byte)0);

		processLength (lowBitLength, hiBitLength);
		processBlock ();
		
		byte[] output = new byte [48];
		unpackWord(H1, output, 0);
		unpackWord(H2, output, 8);
		unpackWord(H3, output, 16);
		unpackWord(H4, output, 24);
		unpackWord(H5, output, 32);
		unpackWord(H6, output, 40);

		Initialize ();
		return output;
	}

	private void update (byte input)
	{
		xBuf [xBufOff++] = input;
		if (xBufOff == xBuf.Length) {
			processWord(xBuf, 0);
			xBufOff = 0;
		}
		byteCount1++;
	}

	private void processWord (byte[] input, int inOff)
	{
		W [wOff++] = ( (ulong) input [inOff] << 56)
			| ( (ulong) input [inOff + 1] << 48)
			| ( (ulong) input [inOff + 2] << 40)
			| ( (ulong) input [inOff + 3] << 32)
        		| ( (ulong) input [inOff + 4] << 24)
			| ( (ulong) input [inOff + 5] << 16)
			| ( (ulong) input [inOff + 6] << 8)
			| ( (ulong) input [inOff + 7]); 
		if (wOff == 16)
			processBlock ();
	}

	private void unpackWord (ulong word, byte[] output, int outOff)
	{
		output[outOff]     = (byte) (word >> 56);
		output[outOff + 1] = (byte) (word >> 48);
		output[outOff + 2] = (byte) (word >> 40);
		output[outOff + 3] = (byte) (word >> 32);
		output[outOff + 4] = (byte) (word >> 24);
		output[outOff + 5] = (byte) (word >> 16);
		output[outOff + 6] = (byte) (word >> 8);
		output[outOff + 7] = (byte) word;
	}

	// adjust the byte counts so that byteCount2 represents the
	// upper long (less 3 bits) word of the byte count.
	private void adjustByteCounts()
	{
		if (byteCount1 > 0x1fffffffffffffffL) {
			byteCount2 += (byteCount1 >> 61);
			byteCount1 &= 0x1fffffffffffffffL;
		}
	}

	private void processLength (ulong lowW, ulong hiW)
	{
		if (wOff > 14)
			processBlock ();
		W[14] = hiW;
		W[15] = lowW;
	}

	private void processBlock ()
	{
		ulong a, b, c, d, e, f, g, h;

		// abcrem doesn't work on fields
		ulong[] W = this.W;
		ulong[] K2 = SHAConstants.K2;

		adjustByteCounts ();
		// expand 16 word block into 80 word blocks.
		for (int t = 16; t <= 79; t++)
		{
			a = W[t-15];
			a = ((a >> 1) | (a << 63)) ^ ((a >> 8) | (a << 56)) ^ (a >> 7);
			b = W[t - 2];
			b = ((b >> 19) | (b << 45)) ^ ((b >> 61) | (b << 3)) ^ (b >> 6);
			W[t] = b + W[t - 7] + a + W[t - 16];
		}
		// set up working variables.
		a = H1;
		b = H2;
		c = H3;
		d = H4;
		e = H5;
		f = H6;
		g = H7;
		h = H8;

		for (int t = 0; t <= 79; t++)
		{
			ulong T1 = ((e >> 14) | (e << 50)) ^ ((e >> 18) | (e << 46)) ^ ((e >> 41) | (e << 23));
			T1 += h + ((e & f) ^ ((~e) & g)) + K2[t] + W[t];

			ulong T2 = ((a >> 28) | (a << 36)) ^ ((a >> 34) | (a << 30)) ^ ((a >> 39) | (a << 25));
			T2 += ((a & b) ^ (a & c) ^ (b & c));

			h = g;
			g = f;
			f = e;
			e = d + T1;
			d = c;
			c = b;
			b = a;
			a = T1 + T2;
		}

		H1 += a;
		H2 += b;
		H3 += c;
		H4 += d;
		H5 += e;
		H6 += f;
		H7 += g;
		H8 += h;
		// reset the offset and clean out the word buffer.
		wOff = 0;
		for (int i = 0; i != W.Length; i++)
			W[i] = 0;
	}
}

}
