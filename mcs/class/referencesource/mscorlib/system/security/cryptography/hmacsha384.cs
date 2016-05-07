// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// HMACSHA384.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class HMACSHA384 : HMAC {

        private bool m_useLegacyBlockSize = Utils._ProduceLegacyHmacValues();

        //
        // public constructors
        //

        public HMACSHA384 () : this (Utils.GenerateRandom(128)) {}

        [System.Security.SecuritySafeCritical]  // auto-generated
        public HMACSHA384 (byte[] key) {
            m_hashName = "SHA384";
#if FULL_AOT_RUNTIME
            m_hash1 = new SHA384Managed();
            m_hash2 = new SHA384Managed();
#else
            m_hash1 = GetHashAlgorithmWithFipsFallback(() => new SHA384Managed(), () => HashAlgorithm.Create("System.Security.Cryptography.SHA384CryptoServiceProvider"));
            m_hash2 = GetHashAlgorithmWithFipsFallback(() => new SHA384Managed(), () => HashAlgorithm.Create("System.Security.Cryptography.SHA384CryptoServiceProvider"));
#endif
            HashSizeValue = 384;
            BlockSizeValue = BlockSize;
            base.InitializeKey(key);
        }

        private int BlockSize {
            get { return m_useLegacyBlockSize ? 64 : 128; }
        }

        // See code:System.Security.Cryptography.HMACSHA512.ProduceLegacyHmacValues
        public bool ProduceLegacyHmacValues {
            get { return m_useLegacyBlockSize; }

            set {
                m_useLegacyBlockSize = value;

                BlockSizeValue = BlockSize;
                InitializeKey(KeyValue);
            }
        }
    }
}
