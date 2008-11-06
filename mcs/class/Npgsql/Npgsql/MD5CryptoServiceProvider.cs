//
// System.Security.Cryptography MD5CryptoServiceProvider Class implementation
//
// Authors:
//	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
//
// Comment: Adapted to the Project from Mono CVS as Sebastien Pouliot suggested to enable
// support of Npgsql MD5 authentication in platforms which don't have support for MD5 algorithm.
//


using System;


namespace Npgsql
{
    /// <summary>
    /// C# implementation of the MD5 cryptographic hash function.
    /// </summary>
#if USE_VERSION_1_0
    internal class MD5CryptoServiceProvider : MD5
    {
#else
    internal sealed class MD5CryptoServiceProvider : MD5
    {
#endif
        private const int BLOCK_SIZE_BYTES =  64;
        private const int HASH_SIZE_BYTES  =  16;
        private const int HASH_SIZE_BITS   = 128;
        [CLSCompliant(false)] private uint[] _H;
        [CLSCompliant(false)] private uint count;
        private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
        private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.

        /// <summary>
        /// Creates a new MD5CryptoServiceProvider.
        /// </summary>
        public MD5CryptoServiceProvider ()
        {
            _H = new uint[4];
            HashSizeValue = HASH_SIZE_BITS;
            _ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];

            Initialize();
        }

        ~MD5CryptoServiceProvider ()
        {
            Dispose (false);
        }

        protected override void Dispose (bool disposing)
        {
            // nothing to do (managed implementation)
        }

        /// <summary>
        /// Drives the hashing function.
        /// </summary>
        /// <param name="rgb">Byte array containing the data to hash.</param>
        /// <param name="start">Where in the input buffer to start.</param>
        /// <param name="size">Size in bytes of the data in the buffer to hash.</param>
        protected override void HashCore (byte[] rgb, int start, int size)
        {
            int i;
            State = 1;

            if (_ProcessingBufferCount != 0)
            {
                if (size < (BLOCK_SIZE_BYTES - _ProcessingBufferCount))
                {
                    System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, size);
                    _ProcessingBufferCount += size;
                    return;
                }
                else
                {
                    i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
                    System.Buffer.BlockCopy (rgb, start, _ProcessingBuffer, _ProcessingBufferCount, i);
                    ProcessBlock (_ProcessingBuffer, 0);
                    _ProcessingBufferCount = 0;
                    start += i;
                    size -= i;
                }
            }

            for (i=0; i<size-size%BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES)
            {
                ProcessBlock (rgb, start+i);
            }

            if (size%BLOCK_SIZE_BYTES != 0)
            {
                System.Buffer.BlockCopy (rgb, size-size%BLOCK_SIZE_BYTES+start, _ProcessingBuffer, 0, size%BLOCK_SIZE_BYTES);
                _ProcessingBufferCount = size%BLOCK_SIZE_BYTES;
            }
        }

        /// <summary>
        /// This finalizes the hash.  Takes the data from the chaining variables and returns it.
        /// </summary>
        protected override byte[] HashFinal ()
        {
            byte[] hash = new byte[16];
            int i, j;

            ProcessFinalBlock(_ProcessingBuffer, 0, _ProcessingBufferCount);

            for (i=0; i<4; i++)
            {
                for (j=0; j<4; j++)
                {
                    hash[i*4+j] = (byte)(_H[i] >> j*8);
                }
            }

            return hash;
        }

        /// <summary>
        /// Resets the class after use.  Called automatically after hashing is done.
        /// </summary>
        public override void Initialize ()
        {
            count = 0;
            _ProcessingBufferCount = 0;

            _H[0] = 0x67452301;
            _H[1] = 0xefcdab89;
            _H[2] = 0x98badcfe;
            _H[3] = 0x10325476;
        }

        /// <summary>
        /// This is the meat of the hash function.  It is what processes each block one at a time.
        /// </summary>
        /// <param name="inputBuffer">Byte array to process data from.</param>
        /// <param name="inputOffset">Where in the byte array to start processing.</param>
        private void ProcessBlock (byte[] inputBuffer, int inputOffset)
        {
            uint[] buff = new uint[16];
            uint a, b, c, d;
            int i;

            count += BLOCK_SIZE_BYTES;

            for (i=0; i<16; i++)
            {
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

            a += (((c ^ d) & b) ^ d) + (uint) Constants.C0 + buff [0];
            a = (a << 7) | (a >> 25);
            a += b;

            d += (((b ^ c) & a) ^ c) + (uint) Constants.C1 + buff [1];
            d = (d << 12) | (d >> 20);
            d += a;

            c += (((a ^ b) & d) ^ b) + (uint) Constants.C2 + buff [2];
            c = (c << 17) | (c >> 15);
            c += d;

            b += (((d ^ a) & c) ^ a) + (uint) Constants.C3 + buff [3];
            b = (b << 22) | (b >> 10);
            b += c;

            a += (((c ^ d) & b) ^ d) + (uint) Constants.C4 + buff [4];
            a = (a << 7) | (a >> 25);
            a += b;

            d += (((b ^ c) & a) ^ c) + (uint) Constants.C5 + buff [5];
            d = (d << 12) | (d >> 20);
            d += a;

            c += (((a ^ b) & d) ^ b) + (uint) Constants.C6 + buff [6];
            c = (c << 17) | (c >> 15);
            c += d;

            b += (((d ^ a) & c) ^ a) + (uint) Constants.C7 + buff [7];
            b = (b << 22) | (b >> 10);
            b += c;

            a += (((c ^ d) & b) ^ d) + (uint) Constants.C8 + buff [8];
            a = (a << 7) | (a >> 25);
            a += b;

            d += (((b ^ c) & a) ^ c) + (uint) Constants.C9 + buff [9];
            d = (d << 12) | (d >> 20);
            d += a;

            c += (((a ^ b) & d) ^ b) + (uint) Constants.C10 + buff [10];
            c = (c << 17) | (c >> 15);
            c += d;

            b += (((d ^ a) & c) ^ a) + (uint) Constants.C11 + buff [11];
            b = (b << 22) | (b >> 10);
            b += c;

            a += (((c ^ d) & b) ^ d) + (uint) Constants.C12 + buff [12];
            a = (a << 7) | (a >> 25);
            a += b;

            d += (((b ^ c) & a) ^ c) + (uint) Constants.C13 + buff [13];
            d = (d << 12) | (d >> 20);
            d += a;

            c += (((a ^ b) & d) ^ b) + (uint) Constants.C14 + buff [14];
            c = (c << 17) | (c >> 15);
            c += d;

            b += (((d ^ a) & c) ^ a) + (uint) Constants.C15 + buff [15];
            b = (b << 22) | (b >> 10);
            b += c;


            // ---- Round 2 --------

            a += (((b ^ c) & d) ^ c) + (uint) Constants.C16 + buff [1];
            a = (a << 5) | (a >> 27);
            a += b;

            d += (((a ^ b) & c) ^ b) + (uint) Constants.C17 + buff [6];
            d = (d << 9) | (d >> 23);
            d += a;

            c += (((d ^ a) & b) ^ a) + (uint) Constants.C18 + buff [11];
            c = (c << 14) | (c >> 18);
            c += d;

            b += (((c ^ d) & a) ^ d) + (uint) Constants.C19 + buff [0];
            b = (b << 20) | (b >> 12);
            b += c;

            a += (((b ^ c) & d) ^ c) + (uint) Constants.C20 + buff [5];
            a = (a << 5) | (a >> 27);
            a += b;

            d += (((a ^ b) & c) ^ b) + (uint) Constants.C21 + buff [10];
            d = (d << 9) | (d >> 23);
            d += a;

            c += (((d ^ a) & b) ^ a) + (uint) Constants.C22 + buff [15];
            c = (c << 14) | (c >> 18);
            c += d;

            b += (((c ^ d) & a) ^ d) + (uint) Constants.C23 + buff [4];
            b = (b << 20) | (b >> 12);
            b += c;

            a += (((b ^ c) & d) ^ c) + (uint) Constants.C24 + buff [9];
            a = (a << 5) | (a >> 27);
            a += b;

            d += (((a ^ b) & c) ^ b) + (uint) Constants.C25 + buff [14];
            d = (d << 9) | (d >> 23);
            d += a;

            c += (((d ^ a) & b) ^ a) + (uint) Constants.C26 + buff [3];
            c = (c << 14) | (c >> 18);
            c += d;

            b += (((c ^ d) & a) ^ d) + (uint) Constants.C27 + buff [8];
            b = (b << 20) | (b >> 12);
            b += c;

            a += (((b ^ c) & d) ^ c) + (uint) Constants.C28 + buff [13];
            a = (a << 5) | (a >> 27);
            a += b;

            d += (((a ^ b) & c) ^ b) + (uint) Constants.C29 + buff [2];
            d = (d << 9) | (d >> 23);
            d += a;

            c += (((d ^ a) & b) ^ a) + (uint) Constants.C30 + buff [7];
            c = (c << 14) | (c >> 18);
            c += d;

            b += (((c ^ d) & a) ^ d) + (uint) Constants.C31 + buff [12];
            b = (b << 20) | (b >> 12);
            b += c;


            // ---- Round 3 --------

            a += (b ^ c ^ d) + (uint) Constants.C32 + buff [5];
            a = (a << 4) | (a >> 28);
            a += b;

            d += (a ^ b ^ c) + (uint) Constants.C33 + buff [8];
            d = (d << 11) | (d >> 21);
            d += a;

            c += (d ^ a ^ b) + (uint) Constants.C34 + buff [11];
            c = (c << 16) | (c >> 16);
            c += d;

            b += (c ^ d ^ a) + (uint) Constants.C35 + buff [14];
            b = (b << 23) | (b >> 9);
            b += c;

            a += (b ^ c ^ d) + (uint) Constants.C36 + buff [1];
            a = (a << 4) | (a >> 28);
            a += b;

            d += (a ^ b ^ c) + (uint) Constants.C37 + buff [4];
            d = (d << 11) | (d >> 21);
            d += a;

            c += (d ^ a ^ b) + (uint) Constants.C38 + buff [7];
            c = (c << 16) | (c >> 16);
            c += d;

            b += (c ^ d ^ a) + (uint) Constants.C39 + buff [10];
            b = (b << 23) | (b >> 9);
            b += c;

            a += (b ^ c ^ d) + (uint) Constants.C40 + buff [13];
            a = (a << 4) | (a >> 28);
            a += b;

            d += (a ^ b ^ c) + (uint) Constants.C41 + buff [0];
            d = (d << 11) | (d >> 21);
            d += a;

            c += (d ^ a ^ b) + (uint) Constants.C42 + buff [3];
            c = (c << 16) | (c >> 16);
            c += d;

            b += (c ^ d ^ a) + (uint) Constants.C43 + buff [6];
            b = (b << 23) | (b >> 9);
            b += c;

            a += (b ^ c ^ d) + (uint) Constants.C44 + buff [9];
            a = (a << 4) | (a >> 28);
            a += b;

            d += (a ^ b ^ c) + (uint) Constants.C45 + buff [12];
            d = (d << 11) | (d >> 21);
            d += a;

            c += (d ^ a ^ b) + (uint) Constants.C46 + buff [15];
            c = (c << 16) | (c >> 16);
            c += d;

            b += (c ^ d ^ a) + (uint) Constants.C47 + buff [2];
            b = (b << 23) | (b >> 9);
            b += c;


            // ---- Round 4 --------

            a += (((~d) | b) ^ c) + (uint) Constants.C48 + buff [0];
            a = (a << 6) | (a >> 26);
            a += b;

            d += (((~c) | a) ^ b) + (uint) Constants.C49 + buff [7];
            d = (d << 10) | (d >> 22);
            d += a;

            c += (((~b) | d) ^ a) + (uint) Constants.C50 + buff [14];
            c = (c << 15) | (c >> 17);
            c += d;

            b += (((~a) | c) ^ d) + (uint) Constants.C51 + buff [5];
            b = (b << 21) | (b >> 11);
            b += c;

            a += (((~d) | b) ^ c) + (uint) Constants.C52 + buff [12];
            a = (a << 6) | (a >> 26);
            a += b;

            d += (((~c) | a) ^ b) + (uint) Constants.C53 + buff [3];
            d = (d << 10) | (d >> 22);
            d += a;

            c += (((~b) | d) ^ a) + (uint) Constants.C54 + buff [10];
            c = (c << 15) | (c >> 17);
            c += d;

            b += (((~a) | c) ^ d) + (uint) Constants.C55 + buff [1];
            b = (b << 21) | (b >> 11);
            b += c;

            a += (((~d) | b) ^ c) + (uint) Constants.C56 + buff [8];
            a = (a << 6) | (a >> 26);
            a += b;

            d += (((~c) | a) ^ b) + (uint) Constants.C57 + buff [15];
            d = (d << 10) | (d >> 22);
            d += a;

            c += (((~b) | d) ^ a) + (uint) Constants.C58 + buff [6];
            c = (c << 15) | (c >> 17);
            c += d;

            b += (((~a) | c) ^ d) + (uint) Constants.C59 + buff [13];
            b = (b << 21) | (b >> 11);
            b += c;

            a += (((~d) | b) ^ c) + (uint) Constants.C60 + buff [4];
            a = (a << 6) | (a >> 26);
            a += b;

            d += (((~c) | a) ^ b) + (uint) Constants.C61 + buff [11];
            d = (d << 10) | (d >> 22);
            d += a;

            c += (((~b) | d) ^ a) + (uint) Constants.C62 + buff [2];
            c = (c << 15) | (c >> 17);
            c += d;

            b += (((~a) | c) ^ d) + (uint) Constants.C63 + buff [9];
            b = (b << 21) | (b >> 11);
            b += c;


            _H[0] += a;
            _H[1] += b;
            _H[2] += c;
            _H[3] += d;
        }

        /// <summary>
        /// Pads and then processes the final block.
        /// </summary>
        /// <param name="inputBuffer">Buffer to grab data from.</param>
        /// <param name="inputOffset">Position in buffer in bytes to get data from.</param>
        /// <param name="inputCount">How much data in bytes in the buffer to use.</param>
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

            for (i=0; i<inputCount; i++)
            {
                fooBuffer[i] = inputBuffer[i+inputOffset];
            }

            fooBuffer[inputCount] = 0x80;
            for (i=inputCount+1; i<inputCount+paddingSize; i++)
            {
                fooBuffer[i] = 0x00;
            }

            size = (uint)(count+inputCount);
            size *= 8;
            fooBuffer[inputCount+paddingSize] = (byte)((size) >>  0);
            fooBuffer[inputCount+paddingSize+1] = (byte)((size) >>  8);
            fooBuffer[inputCount+paddingSize+2] = (byte)((size) >> 16);
            fooBuffer[inputCount+paddingSize+3] = (byte)((size) >> 24);

            fooBuffer[inputCount+paddingSize+4]   = 0x00;
            fooBuffer[inputCount+paddingSize+5] = 0x00;
            fooBuffer[inputCount+paddingSize+6] = 0x00;
            fooBuffer[inputCount+paddingSize+7] = 0x00;

            ProcessBlock(fooBuffer, 0);

            if (inputCount+paddingSize+8 == 128)
            {
                ProcessBlock(fooBuffer, 64);
            }
        }

    private enum Constants :
        uint {
            C0 = 0xd76aa478, C1 = 0xe8c7b756, C2 = 0x242070db,
            C3 = 0xc1bdceee, C4 = 0xf57c0faf, C5 = 0x4787c62a,
            C6 = 0xa8304613, C7 = 0xfd469501, C8 = 0x698098d8,
            C9 = 0x8b44f7af,C10 = 0xffff5bb1,C11 = 0x895cd7be,
            C12 = 0x6b901122,C13 = 0xfd987193,C14 = 0xa679438e,
            C15 = 0x49b40821,C16 = 0xf61e2562,C17 = 0xc040b340,
            C18 = 0x265e5a51,C19 = 0xe9b6c7aa,C20 = 0xd62f105d,
            C21 = 0x02441453,C22 = 0xd8a1e681,C23 = 0xe7d3fbc8,
            C24 = 0x21e1cde6,C25 = 0xc33707d6,C26 = 0xf4d50d87,
            C27 = 0x455a14ed,C28 = 0xa9e3e905,C29 = 0xfcefa3f8,
            C30 = 0x676f02d9,C31 = 0x8d2a4c8a,C32 = 0xfffa3942,
            C33 = 0x8771f681,C34 = 0x6d9d6122,C35 = 0xfde5380c,
            C36 = 0xa4beea44,C37 = 0x4bdecfa9,C38 = 0xf6bb4b60,
            C39 = 0xbebfbc70,C40 = 0x289b7ec6,C41 = 0xeaa127fa,
            C42 = 0xd4ef3085,C43 = 0x04881d05,C44 = 0xd9d4d039,
            C45 = 0xe6db99e5,C46 = 0x1fa27cf8,C47 = 0xc4ac5665,
            C48 = 0xf4292244,C49 = 0x432aff97,C50 = 0xab9423a7,
            C51 = 0xfc93a039,C52 = 0x655b59c3,C53 = 0x8f0ccc92,
            C54 = 0xffeff47d,C55 = 0x85845dd1,C56 = 0x6fa87e4f,
            C57 = 0xfe2ce6e0,C58 = 0xa3014314,C59 = 0x4e0811a1,
            C60 = 0xf7537e82,C61 = 0xbd3af235,C62 = 0x2ad7d2bb,
            C63 = 0xeb86d391
        }

    }
}

