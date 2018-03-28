// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel
{
    internal static class StringUtil
    {
        private static readonly bool randomizedStringHashingEnabled;

        static StringUtil()
        {
            // StringComparer.InvariantCultureIgnoreCase.GetHashCode is a stable hash between 32 and 64 bits. 
            // Test the result of this GetHashCode against a known test vector to see if randomized hashing is enabled.
            randomizedStringHashingEnabled = StringComparer.InvariantCultureIgnoreCase.GetHashCode("The quick brown fox jumps over the lazy dog.") != 0x703e662e;
        }

        // This should be used instead of String.GetHashCode if the value should be
        // stable even if UseRandomizedStringHashing is enabled.
        internal static int GetNonRandomizedHashCode(string str)
        {
            if (!randomizedStringHashingEnabled)
            {
                return str.GetHashCode();
            }

            return GetStableHashCode(str);
        }

        // This is copied from the 32 bit implementation from String.GetHashCode.
        // Since ServiceModel is compiled for MSIL, we can't have different functionality
        // for 32 and 64 bits.
        [System.Security.SecuritySafeCritical]
        private static int GetStableHashCode(string str)
        {
            unsafe 
            {
                fixed (char* src = str) 
                {
                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    // 32 bit machines.
                    int* pint = (int*)src;
                    int len = str.Length;
                    while (len > 2)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len  -= 4;
                    }

                    if (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
    }
}
