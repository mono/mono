//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Runtime
{
    static class MD5HashHelper
    {
        // "derived from the RSA Data Security, Inc. MD5 Message-Digest Algorithm"
        // DO NOT USE FOR SECURITY PURPOSES.
        public static byte[] ComputeHash(byte[] buffer)
        {
            int[] shifts = new int[] { 7, 12, 17, 22, 5, 9, 14, 20, 4, 11, 16, 23, 6, 10, 15, 21 };
            uint[] sines = new uint[] {
                0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
                0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,

                0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
                0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,

                0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
                0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,

                0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
                0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391 };

            int blocks = (buffer.Length + 8) / 64 + 1;

            uint aa = 0x67452301;
            uint bb = 0xefcdab89;
            uint cc = 0x98badcfe;
            uint dd = 0x10325476;

            for (int i = 0; i < blocks; i++)
            {
                byte[] block = buffer;
                int offset = i * 64;

                if (offset + 64 > buffer.Length)
                {
                    block = new byte[64];

                    for (int j = offset; j < buffer.Length; j++)
                    {
                        block[j - offset] = buffer[j];
                    }
                    if (offset <= buffer.Length)
                    {
                        block[buffer.Length - offset] = 0x80;
                    }
                    if (i == blocks - 1)
                    {
                        block[56] = (byte)(buffer.Length << 3);
                        block[57] = (byte)(buffer.Length >> 5);
                        block[58] = (byte)(buffer.Length >> 13);
                        block[59] = (byte)(buffer.Length >> 21);
                    }

                    offset = 0;
                }

                uint a = aa;
                uint b = bb;
                uint c = cc;
                uint d = dd;

                uint f;
                int g;

                for (int j = 0; j < 64; j++)
                {
                    if (j < 16)
                    {
                        f = b & c | ~b & d;
                        g = j;
                    }
                    else if (j < 32)
                    {
                        f = b & d | c & ~d;
                        g = 5 * j + 1;
                    }
                    else if (j < 48)
                    {
                        f = b ^ c ^ d;
                        g = 3 * j + 5;
                    }
                    else
                    {
                        f = c ^ (b | ~d);
                        g = 7 * j;
                    }

                    g = (g & 0x0f) * 4 + offset;

                    uint hold = d;
                    d = c;
                    c = b;

                    b = a + f + sines[j] + (uint)(block[g] + (block[g + 1] << 8) + (block[g + 2] << 16) + (block[g + 3] << 24));
                    b = b << shifts[j & 3 | j >> 2 & ~3] | b >> 32 - shifts[j & 3 | j >> 2 & ~3];
                    b += c;

                    a = hold;
                }

                aa += a;
                bb += b;
                cc += c;
                dd += d;
            }

            return new byte[] {
                (byte)aa, (byte)(aa >> 8), (byte)(aa >> 16), (byte)(aa >> 24),
                (byte)bb, (byte)(bb >> 8), (byte)(bb >> 16), (byte)(bb >> 24),
                (byte)cc, (byte)(cc >> 8), (byte)(cc >> 16), (byte)(cc >> 24),
                (byte)dd, (byte)(dd >> 8), (byte)(dd >> 16), (byte)(dd >> 24) };
        }
    }
}
