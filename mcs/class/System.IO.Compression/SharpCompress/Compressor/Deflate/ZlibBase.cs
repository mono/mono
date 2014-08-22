// Zlib.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.  
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs): 
// Time-stamp: <2009-November-07 05:26:55>
//
// ------------------------------------------------------------------
//
// This module defines classes for ZLIB compression and
// decompression. This code is derived from the jzlib implementation of
// zlib, but significantly modified.  The object model is not the same,
// and many of the behaviors are new or different.  Nonetheless, in
// keeping with the license for jzlib, the copyright to that code is
// included below.
//
// ------------------------------------------------------------------
// 
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the distribution.
// 
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// -----------------------------------------------------------------------
//
// This program is based on zlib-1.1.3; credit to authors
// Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
// and contributors of zlib.
//
// -----------------------------------------------------------------------


namespace SharpCompress.Compressor.Deflate
{
    /// <summary>
    /// The compression level to be used when using a DeflateStream or ZlibStream with CompressionMode.Compress.
    /// </summary>
    internal enum CompressionLevel
    {
        /// <summary>
        /// None means that the data will be simply stored, with no change at all.
        /// If you are producing ZIPs for use on Mac OSX, be aware that archives produced with CompressionLevel.None
        /// cannot be opened with the default zip reader. Use a different CompressionLevel.
        /// </summary>
        None = 0,

        /// <summary>
        /// Same as None.
        /// </summary>
        Level0 = 0,

        /// <summary>
        /// The fastest but least effective compression.
        /// </summary>
        BestSpeed = 1,

        /// <summary>
        /// A synonym for BestSpeed.
        /// </summary>
        Level1 = 1,

        /// <summary>
        /// A little slower, but better, than level 1.
        /// </summary>
        Level2 = 2,

        /// <summary>
        /// A little slower, but better, than level 2.
        /// </summary>
        Level3 = 3,

        /// <summary>
        /// A little slower, but better, than level 3.
        /// </summary>
        Level4 = 4,

        /// <summary>
        /// A little slower than level 4, but with better compression.
        /// </summary>
        Level5 = 5,

        /// <summary>
        /// The default compression level, with a good balance of speed and compression efficiency.   
        /// </summary>
        Default = 6,

        /// <summary>
        /// A synonym for Default.
        /// </summary>
        Level6 = 6,

        /// <summary>
        /// Pretty good compression!
        /// </summary>
        Level7 = 7,

        /// <summary>
        ///  Better compression than Level7!
        /// </summary>
        Level8 = 8,

        /// <summary>
        /// The "best" compression, where best means greatest reduction in size of the input data stream. 
        /// This is also the slowest compression.
        /// </summary>
        BestCompression = 9,

        /// <summary>
        /// A synonym for BestCompression.
        /// </summary>
        Level9 = 9,
    }

    /// <summary>
    /// Describes options for how the compression algorithm is executed.  Different strategies
    /// work better on different sorts of data.  The strategy parameter can affect the compression
    /// ratio and the speed of compression but not the correctness of the compresssion.
    /// </summary>
    internal enum CompressionStrategy
    {
        /// <summary>
        /// The default strategy is probably the best for normal data. 
        /// </summary>
        Default = 0,

        /// <summary>
        /// The <c>Filtered</c> strategy is intended to be used most effectively with data produced by a
        /// filter or predictor.  By this definition, filtered data consists mostly of small
        /// values with a somewhat random distribution.  In this case, the compression algorithm
        /// is tuned to compress them better.  The effect of <c>Filtered</c> is to force more Huffman
        /// coding and less string matching; it is a half-step between <c>Default</c> and <c>HuffmanOnly</c>.
        /// </summary>
        Filtered = 1,

        /// <summary>
        /// Using <c>HuffmanOnly</c> will force the compressor to do Huffman encoding only, with no
        /// string matching.
        /// </summary>
        HuffmanOnly = 2,
    }


    /// <summary>
    /// A general purpose exception class for exceptions in the Zlib library.
    /// </summary>
    internal class ZlibException : System.Exception
    {
        /// <summary>
        /// The ZlibException class captures exception information generated
        /// by the Zlib library. 
        /// </summary>
        public ZlibException()
            : base()
        {
        }

        /// <summary>
        /// This ctor collects a message attached to the exception.
        /// </summary>
        /// <param name="s"></param>
        public ZlibException(System.String s)
            : base(s)
        {
        }
    }

    /// <summary>
    /// Computes an Adler-32 checksum. 
    /// </summary>
    /// <remarks>
    /// The Adler checksum is similar to a CRC checksum, but faster to compute, though less
    /// reliable.  It is used in producing RFC1950 compressed streams.  The Adler checksum
    /// is a required part of the "ZLIB" standard.  Applications will almost never need to
    /// use this class directly.
    /// </remarks>
    internal sealed class Adler
    {
        // largest prime smaller than 65536
        private static readonly int BASE = 65521;
        // NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1
        private static readonly int NMAX = 5552;

        internal static uint Adler32(uint adler, byte[] buf, int index, int len)
        {
            if (buf == null)
                return 1;

            int s1 = (int) (adler & 0xffff);
            int s2 = (int) ((adler >> 16) & 0xffff);

            while (len > 0)
            {
                int k = len < NMAX ? len : NMAX;
                len -= k;
                while (k >= 16)
                {
                    //s1 += (buf[index++] & 0xff); s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    s1 += buf[index++];
                    s2 += s1;
                    k -= 16;
                }
                if (k != 0)
                {
                    do
                    {
                        s1 += buf[index++];
                        s2 += s1;
                    } while (--k != 0);
                }
                s1 %= BASE;
                s2 %= BASE;
            }
            return (uint) ((s2 << 16) | s1);
        }
    }
}