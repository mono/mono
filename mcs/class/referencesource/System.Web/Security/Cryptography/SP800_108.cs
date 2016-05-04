//------------------------------------------------------------------------------
// <copyright file="SP800_108.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;

    /******************************************************************
     * !! WARNING !!                                                  *
     * This class contains cryptographic code. If you make changes to *
     * this class, please have it reviewed by the appropriate people. *
     ******************************************************************/

    // Implements the NIST SP800-108 key derivation routine in counter mode with an HMAC PRF (HMACSHA512).
    // See: http://csrc.nist.gov/publications/nistpubs/800-108/sp800-108.pdf
    //
    // The algorithm is defined as follows:
    //
    // INPUTS:
    // PRF = The pseudo-random function used for key derivation; in our case, an HMAC.
    // KI = The key derivation key (master key) from which keys will be derived.
    // Label = The purpose of the derived key.
    // Context = Information related to the derived key, such as consuming party identities or a nonce.
    // L = The desired length (in bits) of the derived key.
    //
    // ALGORITHM:
    // Let n = ceil(L / HMAC-output-size)
    // For i = 1 to n,
    //   K_i = PRF(KI, [i]_2 || Label || 0x00 || Context || [L]_2)
    // where [x]_2 = the big-endian representation of 'x'
    //
    // OUTPUT:
    // Result := K_1 || K_2 || ... || K_n, truncated to be L bits in length

    internal static class SP800_108 {

        // Implements the KeyDerivationFunction delegate signature; public entry point to the API.
        public static CryptographicKey DeriveKey(CryptographicKey keyDerivationKey, Purpose purpose) {
            // After consultation with the crypto board, we have decided to use HMACSHA512 as the PRF
            // to our KDF. The reason for this is that our PRF is an HMAC, so the total entropy of the
            // PRF is given by MIN(key derivation key length, HMAC block size). It is conceivable that
            // a developer might specify a key greater than 256 bits in length, at which point using
            // a shorter PRF like HMACSHA256 starts discarding entropy. But from the crypto team's
            // perspective it is unreasonable for a developer to supply a key greater than 512 bits,
            // so there's no real harm in us limiting our PRF entropy to 512 bits (HMACSHA512).
            //
            // On 64-bit platforms, HMACSHA512 matches or outperforms HMACSHA256 in our perf testing.
            // On 32-bit platforms, HMACSHA512 is around 1/3 the speed of HMACSHA256. In both cases, we
            // try to cache the derived CryptographicKey wherever we can, so this shouldn't be a
            // bottleneck regardless.

            using (HMACSHA512 hmac = CryptoAlgorithms.CreateHMACSHA512(keyDerivationKey.GetKeyMaterial())) {
                byte[] label, context;
                purpose.GetKeyDerivationParameters(out label, out context);

                byte[] derivedKey = DeriveKeyImpl(hmac, label, context, keyDerivationKey.KeyLength);
                return new CryptographicKey(derivedKey);
            }
        }

        // NOTE: This method also exists in Win8 (as BCryptKeyDerivation) and QTD (as DeriveKeySP800_108).
        // However, the QTD implementation is currently incorrect, so we can't depend on it here. The below
        // is a correct implementation. When we take a Win8 dependency, we can call into BCryptKeyDerivation.
        private static byte[] DeriveKeyImpl(HMAC hmac, byte[] label, byte[] context, int keyLengthInBits) {
            // This entire method is checked because according to SP800-108 it is an error
            // for any single operation to result in overflow.
            checked {

                // Make a buffer which is ____ || label || 0x00 || context || [l]_2.
                // We can reuse this buffer during each round.

                int labelLength = (label != null) ? label.Length : 0;
                int contextLength = (context != null) ? context.Length : 0;
                byte[] buffer = new byte[4 /* [i]_2 */ + labelLength /* label */ + 1 /* 0x00 */ + contextLength /* context */ + 4 /* [L]_2 */];

                if (labelLength != 0) {
                    Buffer.BlockCopy(label, 0, buffer, 4, labelLength); // the 4 accounts for the [i]_2 length
                }
                if (contextLength != 0) {
                    Buffer.BlockCopy(context, 0, buffer, 5 + labelLength, contextLength); // the '5 +' accounts for the [i]_2 length, the label, and the 0x00 byte
                }
                WriteUInt32ToByteArrayBigEndian((uint)keyLengthInBits, buffer, 5 + labelLength + contextLength); // the '5 +' accounts for the [i]_2 length, the label, the 0x00 byte, and the context

                // Initialization

                int numBytesWritten = 0;
                int numBytesRemaining = keyLengthInBits / 8;
                byte[] output = new byte[numBytesRemaining];

                // Calculate each K_i value and copy the leftmost bits to the output buffer as appropriate.

                for (uint i = 1; numBytesRemaining > 0; i++) {
                    WriteUInt32ToByteArrayBigEndian(i, buffer, 0); // set the first 32 bits of the buffer to be the current iteration value
                    byte[] K_i = hmac.ComputeHash(buffer);

                    // copy the leftmost bits of K_i into the output buffer
                    int numBytesToCopy = Math.Min(numBytesRemaining, K_i.Length);
                    Buffer.BlockCopy(K_i, 0, output, numBytesWritten, numBytesToCopy);
                    numBytesWritten += numBytesToCopy;
                    numBytesRemaining -= numBytesToCopy;
                }

                // finished
                return output;
            }
        }

        private static void WriteUInt32ToByteArrayBigEndian(uint value, byte[] buffer, int offset) {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value);
        }

    }
}
