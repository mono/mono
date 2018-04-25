// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// SHA384.cs
//
// This abstract class represents the SHA-384 hash algorithm.
//

namespace System.Security.Cryptography {
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class SHA384 : HashAlgorithm
    {
        //
        // protected constructors
        //

        protected SHA384() {
            HashSizeValue = 384;
        }

        //
        // public methods
        //

        new static public SHA384 Create() {
#if FULL_AOT_RUNTIME
            return new System.Security.Cryptography.SHA384Managed ();
#else
            return Create("System.Security.Cryptography.SHA384");
#endif
        }

        new static public SHA384 Create(String hashName) {
            return (SHA384) CryptoConfig.CreateFromName(hashName);
        }
    }
}

