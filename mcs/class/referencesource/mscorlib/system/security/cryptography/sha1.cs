// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// SHA1.cs
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class SHA1 : HashAlgorithm
    {
        protected SHA1() {
            HashSizeValue = 160;
        }

        //
        // public methods
        //

        new static public SHA1 Create() {
#if FULL_AOT_RUNTIME
            return new System.Security.Cryptography.SHA1CryptoServiceProvider ();
#else
            return Create("System.Security.Cryptography.SHA1");
#endif
        }

        new static public SHA1 Create(String hashName) {
            return (SHA1) CryptoConfig.CreateFromName(hashName);
        }
    }
}

