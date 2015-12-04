// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// HMACRIPEMD160.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public class HMACRIPEMD160 : HMAC {
        //
        // public constructors
        //

        public HMACRIPEMD160 () : this (Utils.GenerateRandom(64)) {}

        public HMACRIPEMD160 (byte[] key) {
            m_hashName = "RIPEMD160";
            m_hash1 = new RIPEMD160Managed();
            m_hash2 = new RIPEMD160Managed();
            HashSizeValue = 160;
            base.InitializeKey(key);
        }
    }
}
