// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// HMACMD5.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class HMACMD5 : HMAC {
        //
        // public constructors
        //

        public HMACMD5 () : this (Utils.GenerateRandom(64)) {}

        public HMACMD5 (byte[] key) {
            m_hashName = "MD5";
            m_hash1 = new MD5CryptoServiceProvider();
            m_hash2 = new MD5CryptoServiceProvider();
            HashSizeValue = 128;
            base.InitializeKey(key);
        }
    }
}
