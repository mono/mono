// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// SHA1Managed.cs
//

namespace System.Security.Cryptography {
    using System;
    using System.Security;
    using System.Diagnostics.Contracts;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class SHA1Managed : SHA1
    {
        private byte[] _buffer;
        private long   _count; // Number of bytes in the hashed message
        private uint[] _stateSHA1;
        private uint[] _expandedBuffer;

        //
        // public constructors
        //

        public SHA1Managed()
        {
#if FEATURE_CRYPTO
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            Contract.EndContractBlock();
#endif // FEATURE_CRYPTO

            _stateSHA1 = new uint[5];
            _buffer = new byte[64];
            _expandedBuffer = new uint[80];

            InitializeState();
        }

        //
        // public methods
        //

        public override void Initialize() {
            InitializeState();

            // Zeroize potentially sensitive information.
            Array.Clear(_buffer, 0, _buffer.Length);
            Array.Clear(_expandedBuffer, 0, _expandedBuffer.Length);
        }

        protected override void HashCore(byte[] rgb, int ibStart, int cbSize) {
            _HashData(rgb, ibStart, cbSize);
        }

        protected override byte[] HashFinal() {
            return _EndHash();
        }

        //
        // private methods
        //

        private void InitializeState() {
            _count = 0;

            _stateSHA1[0] =  0x67452301;
            _stateSHA1[1] =  0xefcdab89;
            _stateSHA1[2] =  0x98badcfe;
            _stateSHA1[3] =  0x10325476;
            _stateSHA1[4] =  0xc3d2e1f0;
        }

        /* Copyright (C) RSA Data Security, Inc. created 1993.  This is an
           unpublished work protected as such under copyright law.  This work
           contains proprietary, confidential, and trade secret information of
           RSA Data Security, Inc.  Use, disclosure or reproduction without the
           express written authorization of RSA Data Security, Inc. is
           prohibited.
           */

        /* SHA block update operation. Continues an SHA message-digest
           operation, processing another message block, and updating the
           context.
           */
        [System.Security.SecuritySafeCritical]  // auto-generated
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int bufferLen;
            int partInLen = cbSize;
            int partInBase = ibStart;

            /* Compute length of buffer */
            bufferLen = (int) (_count & 0x3f);

            /* Update number of bytes */
            _count += partInLen;

            fixed (uint* stateSHA1 = _stateSHA1) {
                fixed (byte* buffer = _buffer) {
                    fixed (uint* expandedBuffer = _expandedBuffer) {
                        if ((bufferLen > 0) && (bufferLen + partInLen >= 64)) {
                            Buffer.InternalBlockCopy(partIn, partInBase, _buffer, bufferLen, 64 - bufferLen);
                            partInBase += (64 - bufferLen);
                            partInLen -= (64 - bufferLen);
                            SHATransform(expandedBuffer, stateSHA1, buffer);
                            bufferLen = 0;
                        }

                        /* Copy input to temporary buffer and hash */
                        while (partInLen >= 64) {
                            Buffer.InternalBlockCopy(partIn, partInBase, _buffer, 0, 64);
                            partInBase += 64;
                            partInLen -= 64;
                            SHATransform(expandedBuffer, stateSHA1, buffer);
                        }

                        if (partInLen > 0) {
                            Buffer.InternalBlockCopy(partIn, partInBase, _buffer, bufferLen, partInLen);
                        }
                    }
                }
            }
        }

        /* SHA finalization. Ends an SHA message-digest operation, writing
           the message digest.
            */

        private byte[] _EndHash()
        {
            byte[]          pad;
            int             padLen;
            long            bitCount;
            byte[]          hash = new byte[20];

            /* Compute padding: 80 00 00 ... 00 00 <bit count>
             */

            padLen = 64 - (int)(_count & 0x3f);
            if (padLen <= 8)
                padLen += 64;

            pad = new byte[padLen];
            pad[0] = 0x80;

            //  Convert count to bit count
            bitCount = _count * 8;

            pad[padLen-8] = (byte) ((bitCount >> 56) & 0xff);
            pad[padLen-7] = (byte) ((bitCount >> 48) & 0xff);
            pad[padLen-6] = (byte) ((bitCount >> 40) & 0xff);
            pad[padLen-5] = (byte) ((bitCount >> 32) & 0xff);
            pad[padLen-4] = (byte) ((bitCount >> 24) & 0xff);
            pad[padLen-3] = (byte) ((bitCount >> 16) & 0xff);
            pad[padLen-2] = (byte) ((bitCount >> 8) & 0xff);
            pad[padLen-1] = (byte) ((bitCount >> 0) & 0xff);

            /* Digest padding */
            _HashData(pad, 0, pad.Length);

            /* Store digest */
            Utils.DWORDToBigEndian (hash, _stateSHA1, 5);

            HashValue = hash;
            return hash;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static unsafe void SHATransform (uint* expandedBuffer, uint* state, byte* block)
        {
            uint a = state[0];
            uint b = state[1];
            uint c = state[2];
            uint d = state[3];
            uint e = state[4];

            int i;

            Utils.DWORDFromBigEndian(expandedBuffer, 16, block);
            SHAExpand(expandedBuffer);

            /* Round 1 */
            for (i=0; i<20; i+= 5) {
                { (e) +=  (((((a)) << (5)) | (((a)) >> (32-(5)))) + ( (d) ^ ( (b) & ( (c) ^ (d) ) ) ) + (expandedBuffer[i]) + 0x5a827999); (b) =  ((((b)) << (30)) | (((b)) >> (32-(30)))); }
                { (d) +=  (((((e)) << (5)) | (((e)) >> (32-(5)))) + ( (c) ^ ( (a) & ( (b) ^ (c) ) ) ) + (expandedBuffer[i+1]) + 0x5a827999); (a) =  ((((a)) << (30)) | (((a)) >> (32-(30)))); }
                { (c) +=  (((((d)) << (5)) | (((d)) >> (32-(5)))) + ( (b) ^ ( (e) & ( (a) ^ (b) ) ) ) + (expandedBuffer[i+2]) + 0x5a827999); (e) =  ((((e)) << (30)) | (((e)) >> (32-(30)))); };;
                { (b) +=  (((((c)) << (5)) | (((c)) >> (32-(5)))) + ( (a) ^ ( (d) & ( (e) ^ (a) ) ) ) + (expandedBuffer[i+3]) + 0x5a827999); (d) =  ((((d)) << (30)) | (((d)) >> (32-(30)))); };;
                { (a) +=  (((((b)) << (5)) | (((b)) >> (32-(5)))) + ( (e) ^ ( (c) & ( (d) ^ (e) ) ) ) + (expandedBuffer[i+4]) + 0x5a827999); (c) =  ((((c)) << (30)) | (((c)) >> (32-(30)))); };;
            }

            /* Round 2 */
            for (; i<40; i+= 5) {
                { (e) +=  (((((a)) << (5)) | (((a)) >> (32-(5)))) + ((b) ^ (c) ^ (d)) + (expandedBuffer[i]) + 0x6ed9eba1); (b) =  ((((b)) << (30)) | (((b)) >> (32-(30)))); };;
                { (d) +=  (((((e)) << (5)) | (((e)) >> (32-(5)))) + ((a) ^ (b) ^ (c)) + (expandedBuffer[i+1]) + 0x6ed9eba1); (a) =  ((((a)) << (30)) | (((a)) >> (32-(30)))); };;
                { (c) +=  (((((d)) << (5)) | (((d)) >> (32-(5)))) + ((e) ^ (a) ^ (b)) + (expandedBuffer[i+2]) + 0x6ed9eba1); (e) =  ((((e)) << (30)) | (((e)) >> (32-(30)))); };;
                { (b) +=  (((((c)) << (5)) | (((c)) >> (32-(5)))) + ((d) ^ (e) ^ (a)) + (expandedBuffer[i+3]) + 0x6ed9eba1); (d) =  ((((d)) << (30)) | (((d)) >> (32-(30)))); };;
                { (a) +=  (((((b)) << (5)) | (((b)) >> (32-(5)))) + ((c) ^ (d) ^ (e)) + (expandedBuffer[i+4]) + 0x6ed9eba1); (c) =  ((((c)) << (30)) | (((c)) >> (32-(30)))); };;
            }

            /* Round 3 */
            for (; i<60; i+=5) {
                { (e) +=  (((((a)) << (5)) | (((a)) >> (32-(5)))) + ( ( (b) & (c) ) | ( (d) & ( (b) | (c) ) ) ) + (expandedBuffer[i]) + 0x8f1bbcdc); (b) =  ((((b)) << (30)) | (((b)) >> (32-(30)))); };;
                { (d) +=  (((((e)) << (5)) | (((e)) >> (32-(5)))) + ( ( (a) & (b) ) | ( (c) & ( (a) | (b) ) ) ) + (expandedBuffer[i+1]) + 0x8f1bbcdc); (a) =  ((((a)) << (30)) | (((a)) >> (32-(30)))); };;
                { (c) +=  (((((d)) << (5)) | (((d)) >> (32-(5)))) + ( ( (e) & (a) ) | ( (b) & ( (e) | (a) ) ) ) + (expandedBuffer[i+2]) + 0x8f1bbcdc); (e) =  ((((e)) << (30)) | (((e)) >> (32-(30)))); };;
                { (b) +=  (((((c)) << (5)) | (((c)) >> (32-(5)))) + ( ( (d) & (e) ) | ( (a) & ( (d) | (e) ) ) ) + (expandedBuffer[i+3]) + 0x8f1bbcdc); (d) =  ((((d)) << (30)) | (((d)) >> (32-(30)))); };;
                { (a) +=  (((((b)) << (5)) | (((b)) >> (32-(5)))) + ( ( (c) & (d) ) | ( (e) & ( (c) | (d) ) ) ) + (expandedBuffer[i+4]) + 0x8f1bbcdc); (c) =  ((((c)) << (30)) | (((c)) >> (32-(30)))); };;
            }

            /* Round 4 */
            for (; i<80; i+=5) {
                { (e) +=  (((((a)) << (5)) | (((a)) >> (32-(5)))) + ((b) ^ (c) ^ (d)) + (expandedBuffer[i]) + 0xca62c1d6); (b) =  ((((b)) << (30)) | (((b)) >> (32-(30)))); };;
                { (d) +=  (((((e)) << (5)) | (((e)) >> (32-(5)))) + ((a) ^ (b) ^ (c)) + (expandedBuffer[i+1]) + 0xca62c1d6); (a) =  ((((a)) << (30)) | (((a)) >> (32-(30)))); };;
                { (c) +=  (((((d)) << (5)) | (((d)) >> (32-(5)))) + ((e) ^ (a) ^ (b)) + (expandedBuffer[i+2]) + 0xca62c1d6); (e) =  ((((e)) << (30)) | (((e)) >> (32-(30)))); };;
                { (b) +=  (((((c)) << (5)) | (((c)) >> (32-(5)))) + ((d) ^ (e) ^ (a)) + (expandedBuffer[i+3]) + 0xca62c1d6); (d) =  ((((d)) << (30)) | (((d)) >> (32-(30)))); };;
                { (a) +=  (((((b)) << (5)) | (((b)) >> (32-(5)))) + ((c) ^ (d) ^ (e)) + (expandedBuffer[i+4]) + 0xca62c1d6); (c) =  ((((c)) << (30)) | (((c)) >> (32-(30)))); };;
            }

            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
            state[4] += e;
        }

        /* Expands x[0..15] into x[16..79], according to the recurrence
           x[i] = x[i-3] ^ x[i-8] ^ x[i-14] ^ x[i-16].
           */

        [System.Security.SecurityCritical]  // auto-generated
        private static unsafe void SHAExpand (uint* x)
        {
            int  i;
            uint tmp;

            for (i = 16; i < 80; i++) {
                tmp =  (x[i-3] ^ x[i-8] ^ x[i-14] ^ x[i-16]);
                x[i] =  ((tmp << 1) | (tmp >> 31));
            }
        }
    }
}
