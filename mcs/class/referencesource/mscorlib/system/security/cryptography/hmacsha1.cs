// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// HMACSHA1.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class HMACSHA1 : HMAC {
        //
        // public constructors
        //

        public HMACSHA1 () : this (Utils.GenerateRandom(64)) {}

        public HMACSHA1 (byte[] key) : this (key, false) {}

        public HMACSHA1 (byte[] key, bool useManagedSha1) {
            m_hashName = "SHA1";
#if FEATURE_CRYPTO && !FULL_AOT_RUNTIME
            if (useManagedSha1) {
#endif // FEATURE_CRYPTO
                m_hash1 = new SHA1Managed();
                m_hash2 = new SHA1Managed();
#if FEATURE_CRYPTO && !FULL_AOT_RUNTIME
            } else {
                m_hash1 = new SHA1CryptoServiceProvider();
                m_hash2 = new SHA1CryptoServiceProvider();
            }
#endif // FEATURE_CRYPTO

            HashSizeValue = 160;
            base.InitializeKey(key);
        }
    }
}
