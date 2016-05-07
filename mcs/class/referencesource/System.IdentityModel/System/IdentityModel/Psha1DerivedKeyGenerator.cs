//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.ServiceModel.Diagnostics;

    sealed class Psha1DerivedKeyGenerator
    {
        byte[] key;

        public Psha1DerivedKeyGenerator(byte[] key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            this.key = key;
        }

        public byte[] GenerateDerivedKey(byte[] label, byte[] nonce, int derivedKeySize, int position)
        {
            if (label == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("label");
            }
            if (nonce == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
            }
            ManagedPsha1 dkcp = new ManagedPsha1(key, label, nonce);
            return dkcp.GetDerivedKey(derivedKeySize, position);
        }

        // private class to do the real work
        // Note: Though named ManagedPsha1, this works for both fips and non-fips compliance
        sealed class ManagedPsha1
        {
            byte[] aValue;
            byte[] buffer;
            byte[] chunk;
            KeyedHashAlgorithm hmac;
            int index;
            int position;
            byte[] secret;
            byte[] seed;

            // assume arguments are already validated
            public ManagedPsha1(byte[] secret, byte[] label, byte[] seed)
            {
                this.secret = secret;
                this.seed = DiagnosticUtility.Utility.AllocateByteArray(checked(label.Length + seed.Length));
                label.CopyTo(this.seed, 0);
                seed.CopyTo(this.seed, label.Length);

                this.aValue = this.seed;
                this.chunk = new byte[0];
                this.index = 0;
                this.position = 0;
                this.hmac = CryptoHelper.NewHmacSha1KeyedHashAlgorithm(secret);

                this.buffer = DiagnosticUtility.Utility.AllocateByteArray(checked(this.hmac.HashSize / 8 + this.seed.Length));
            }

            public byte[] GetDerivedKey(int derivedKeySize, int position)
            {
                if (derivedKeySize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("derivedKeySize", SR.GetString(SR.ValueMustBeNonNegative)));
                }
                if (this.position > position)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("position", SR.GetString(SR.ValueMustBeInRange, 0, this.position)));
                }

                // Seek to the desired position in the pseudo-random stream.
                while (this.position < position)
                {
                    GetByte();
                }
                int sizeInBytes = derivedKeySize / 8;
                byte[] derivedKey = new byte[sizeInBytes];
                for (int i = 0; i < sizeInBytes; i++)
                {
                    derivedKey[i] = GetByte();
                }
                return derivedKey;
            }

            byte GetByte()
            {
                if (index >= chunk.Length)
                {
                    // Calculate A(i) = HMAC_SHA1(secret, A(i-1)).
                    hmac.Initialize();
                    this.aValue = hmac.ComputeHash(this.aValue);
                    // Calculate P_SHA1(secret, seed)[j] = HMAC_SHA1(secret, A(j+1) || seed).
                    this.aValue.CopyTo(buffer, 0);
                    this.seed.CopyTo(buffer, this.aValue.Length);
                    hmac.Initialize();
                    this.chunk = hmac.ComputeHash(buffer);
                    index = 0;
                }
                position++;
                return chunk[index++];
            }
        }
    }
}
