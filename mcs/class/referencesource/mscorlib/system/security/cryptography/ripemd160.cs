// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// RIPEMD160.cs
//

namespace System.Security.Cryptography {
    using System;
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class RIPEMD160 : HashAlgorithm
    {
        //
        // public constructors
        //

        protected RIPEMD160()
        {
            HashSizeValue = 160;
        }

        //
        // public methods
        //

        new static public RIPEMD160 Create() {
#if FULL_AOT_RUNTIME
            return new System.Security.Cryptography.RIPEMD160Managed ();
#else
            return Create("System.Security.Cryptography.RIPEMD160");
#endif
        }

        new static public RIPEMD160 Create(String hashName) {
            return (RIPEMD160) CryptoConfig.CreateFromName(hashName);
        }
    }
}

