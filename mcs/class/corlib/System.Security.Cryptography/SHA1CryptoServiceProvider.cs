//
// System.Security.Cryptography SHA1CryptoServiceProvider Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
//

// Note:
// The MS Framework includes two (almost) identical class for SHA1.
//	SHA1Managed is a 100% managed implementation.
//	SHA1CryptoServiceProvider (this file) is a wrapper on CryptoAPI.
// Mono must provide those two class for binayry compatibility.
// In our case both class are wrappers around a managed internal class SHA1Internal.

namespace System.Security.Cryptography {

	/// <summary>
	/// C# implementation of the SHA1 cryptographic hash function.
	/// LAMESPEC?: Basically the same thing as SHA1Managed except for how its implemented.
	/// </summary>
	internal class SHA1Internal {
		private const int BLOCK_SIZE_BYTES =  64;
		private const int HASH_SIZE_BYTES  =  20;
		private const int HASH_SIZE_BITS   = 160;
		[CLSCompliant(false)] private uint[] _H;  // these are my chaining variables
		[CLSCompliant(false)] private uint count;
		private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
		private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.

		/// <summary>
		/// Creates a new SHA1CryptoServiceProvider.
		/// </summary>
		public SHA1Internal () 
		{
			_H = new uint[5];
			_ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];
			
			Initialize();
		}

		/// <summary>
		/// Drives the hashing function.
		/// </summary>
		/// <param name="rgb">Byte array containing the data to hash.</param>
		/// <param name="start">Where in the input buffer to start.</param>
		/// <param name="size">Size in bytes of the data in the buffer to hash.</param>
		public void HashCore (byte[] rgb, int start, int size) 
		{
			int i;

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
	
		/// <summary>
		/// This finalizes the hash.  Takes the data from the chaining variables and returns it.
		/// </summary>
		public byte[] HashFinal () 
		{
			byte[] hash = new byte[20];
			int i, j;

			ProcessFinalBlock(_ProcessingBuffer, 0, _ProcessingBufferCount);

			for (i=0; i<5; i++) {
				for (j=0; j<4; j++) {
					hash[i*4+j] = (byte)(_H[i] >> (8*(3-j)));
				}
			}

			return hash;
		}

		
		/// <summary>
		/// Resets the class after use.  Called automatically after hashing is done.
		/// </summary>
		public void Initialize () 
		{
			count = 0;
			_ProcessingBufferCount = 0;

			_H[0] = 0x67452301;
			_H[1] = 0xefcdab89;
			_H[2] = 0x98badcfe;
			_H[3] = 0x10325476;
			_H[4] = 0xC3D2E1F0;
		}

		/// <summary>
		/// This is the meat of the hash function.  It is what processes each block one at a time.
		/// </summary>
		/// <param name="inputBuffer">Byte array to process data from.</param>
		/// <param name="inputOffset">Where in the byte array to start processing.</param>
		public void ProcessBlock(byte[] inputBuffer, int inputOffset) 
		{
			uint[] buff = new uint[80];
			uint a, b, c, d, e;
			int i;

			count += BLOCK_SIZE_BYTES;
		
			for (i=0; i<16; i++) {
				buff[i] = ((uint)(inputBuffer[inputOffset+4*i]) << 24)
					| ((uint)(inputBuffer[inputOffset+4*i+1]) << 16)
					| ((uint)(inputBuffer[inputOffset+4*i+2]) <<  8)
					| ((uint)(inputBuffer[inputOffset+4*i+3]));
			}

			for (i=16; i<80; i++) {
				buff[i] = ((buff[i-3] ^ buff[i-8] ^ buff[i-14] ^ buff[i-16]) << 1)
					| ((buff[i-3] ^ buff[i-8] ^ buff[i-14] ^ buff[i-16]) >> 31);
			}
		
			a = _H[0];
			b = _H[1];
			c = _H[2];
			d = _H[3];
			e = _H[4];


			// This function was unrolled because it seems to be doubling our performance with current compiler/VM.
			// Possibly roll up if this changes.
	
			// ---- Round 1 --------
  
			e += ((a << 5) | (a >> 27)) + (((c ^ d) & b) ^ d) + 0x5A827999 + buff[0];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (((b ^ c) & a) ^ c) + 0x5A827999 + buff[1];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (((a ^ b) & e) ^ b) + 0x5A827999 + buff[2];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (((e ^ a) & d) ^ a) + 0x5A827999 + buff[3];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (((d ^ e) & c) ^ e) + 0x5A827999 + buff[4];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (((c ^ d) & b) ^ d) + 0x5A827999 + buff[5];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (((b ^ c) & a) ^ c) + 0x5A827999 + buff[6];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (((a ^ b) & e) ^ b) + 0x5A827999 + buff[7];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (((e ^ a) & d) ^ a) + 0x5A827999 + buff[8];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (((d ^ e) & c) ^ e) + 0x5A827999 + buff[9];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (((c ^ d) & b) ^ d) + 0x5A827999 + buff[10];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (((b ^ c) & a) ^ c) + 0x5A827999 + buff[11];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (((a ^ b) & e) ^ b) + 0x5A827999 + buff[12];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (((e ^ a) & d) ^ a) + 0x5A827999 + buff[13];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (((d ^ e) & c) ^ e) + 0x5A827999 + buff[14];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (((c ^ d) & b) ^ d) + 0x5A827999 + buff[15];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (((b ^ c) & a) ^ c) + 0x5A827999 + buff[16];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (((a ^ b) & e) ^ b) + 0x5A827999 + buff[17];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (((e ^ a) & d) ^ a) + 0x5A827999 + buff[18];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (((d ^ e) & c) ^ e) + 0x5A827999 + buff[19];
			c = (c << 30) | (c >> 2);



			// ---- Round 2 --------
  
			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0x6ED9EBA1 + buff[20];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0x6ED9EBA1 + buff[21];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0x6ED9EBA1 + buff[22];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0x6ED9EBA1 + buff[23];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0x6ED9EBA1 + buff[24];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0x6ED9EBA1 + buff[25];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0x6ED9EBA1 + buff[26];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0x6ED9EBA1 + buff[27];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0x6ED9EBA1 + buff[28];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0x6ED9EBA1 + buff[29];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0x6ED9EBA1 + buff[30];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0x6ED9EBA1 + buff[31];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0x6ED9EBA1 + buff[32];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0x6ED9EBA1 + buff[33];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0x6ED9EBA1 + buff[34];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0x6ED9EBA1 + buff[35];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0x6ED9EBA1 + buff[36];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0x6ED9EBA1 + buff[37];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0x6ED9EBA1 + buff[38];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0x6ED9EBA1 + buff[39];
			c = (c << 30) | (c >> 2);



			// ---- Round 3 --------
  
			e += ((a << 5) | (a >> 27)) + ((b&c) | (b&d) | (c&d)) + 0x8F1BBCDC + buff[40];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + ((a&b) | (a&c) | (b&c)) + 0x8F1BBCDC + buff[41];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + ((e&a) | (e&b) | (a&b)) + 0x8F1BBCDC + buff[42];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + ((d&e) | (d&a) | (e&a)) + 0x8F1BBCDC + buff[43];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + ((c&d) | (c&e) | (d&e)) + 0x8F1BBCDC + buff[44];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + ((b&c) | (b&d) | (c&d)) + 0x8F1BBCDC + buff[45];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + ((a&b) | (a&c) | (b&c)) + 0x8F1BBCDC + buff[46];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + ((e&a) | (e&b) | (a&b)) + 0x8F1BBCDC + buff[47];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + ((d&e) | (d&a) | (e&a)) + 0x8F1BBCDC + buff[48];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + ((c&d) | (c&e) | (d&e)) + 0x8F1BBCDC + buff[49];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + ((b&c) | (b&d) | (c&d)) + 0x8F1BBCDC + buff[50];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + ((a&b) | (a&c) | (b&c)) + 0x8F1BBCDC + buff[51];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + ((e&a) | (e&b) | (a&b)) + 0x8F1BBCDC + buff[52];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + ((d&e) | (d&a) | (e&a)) + 0x8F1BBCDC + buff[53];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + ((c&d) | (c&e) | (d&e)) + 0x8F1BBCDC + buff[54];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + ((b&c) | (b&d) | (c&d)) + 0x8F1BBCDC + buff[55];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + ((a&b) | (a&c) | (b&c)) + 0x8F1BBCDC + buff[56];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + ((e&a) | (e&b) | (a&b)) + 0x8F1BBCDC + buff[57];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + ((d&e) | (d&a) | (e&a)) + 0x8F1BBCDC + buff[58];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + ((c&d) | (c&e) | (d&e)) + 0x8F1BBCDC + buff[59];
			c = (c << 30) | (c >> 2);



			// ---- Round 4 --------
  
			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0xCA62C1D6 + buff[60];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0xCA62C1D6 + buff[61];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0xCA62C1D6 + buff[62];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0xCA62C1D6 + buff[63];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0xCA62C1D6 + buff[64];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0xCA62C1D6 + buff[65];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0xCA62C1D6 + buff[66];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0xCA62C1D6 + buff[67];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0xCA62C1D6 + buff[68];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0xCA62C1D6 + buff[69];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0xCA62C1D6 + buff[70];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0xCA62C1D6 + buff[71];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0xCA62C1D6 + buff[72];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0xCA62C1D6 + buff[73];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0xCA62C1D6 + buff[74];
			c = (c << 30) | (c >> 2);

			e += ((a << 5) | (a >> 27)) + (b ^ c ^ d) + 0xCA62C1D6 + buff[75];
			b = (b << 30) | (b >> 2);

			d += ((e << 5) | (e >> 27)) + (a ^ b ^ c) + 0xCA62C1D6 + buff[76];
			a = (a << 30) | (a >> 2);

			c += ((d << 5) | (d >> 27)) + (e ^ a ^ b) + 0xCA62C1D6 + buff[77];
			e = (e << 30) | (e >> 2);

			b += ((c << 5) | (c >> 27)) + (d ^ e ^ a) + 0xCA62C1D6 + buff[78];
			d = (d << 30) | (d >> 2);

			a += ((b << 5) | (b >> 27)) + (c ^ d ^ e) + 0xCA62C1D6 + buff[79];
			c = (c << 30) | (c >> 2);


			_H[0] += a;
			_H[1] += b;
			_H[2] += c;
			_H[3] += d;
			_H[4] += e;
		}
	
		/// <summary>
		/// Pads and then processes the final block.
		/// Non-standard.
		/// </summary>
		/// <param name="inputBuffer">Buffer to grab data from.</param>
		/// <param name="inputOffset">Position in buffer in bytes to get data from.</param>
		/// <param name="inputCount">How much data in bytes in the buffer to use.</param>
		public void ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) 
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
			size *= 8;  // I deal in bytes.  They algorythm deals in bits.

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

	public sealed class SHA1CryptoServiceProvider : SHA1 {

		private SHA1Internal sha;

		public SHA1CryptoServiceProvider () 
		{
			sha = new SHA1Internal ();
		}

		~SHA1CryptoServiceProvider () 
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing) 
		{
			// nothing new to do (managed implementation)
			base.Dispose (disposing);
		}

		protected override void HashCore (byte[] rgb, int start, int size) 
		{
			State = 1;
			sha.HashCore (rgb, start, size);
		}

		protected override byte[] HashFinal () 
		{
			State = 0;
			return sha.HashFinal ();
		}

		public override void Initialize () 
		{
			sha.Initialize ();
		}

		private void ProcessBlock (byte[] inputBuffer, int inputOffset) 
		{
			sha.ProcessBlock (inputBuffer, inputOffset);
		}

		private void ProcessFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount) 
		{
			sha.ProcessFinalBlock (inputBuffer, inputOffset, inputCount);
		}
	}

}

