// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// SHA256.cs
//
// This abstract class represents the SHA-256 hash algorithm.
//

namespace System.Security.Cryptography {
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class SHA256 : HashAlgorithm
    {
        //
        // protected constructors
        //

        protected SHA256() {
            HashSizeValue = 256;
        }

        //
        // public methods
        //

        new static public SHA256 Create() {
            return Create("System.Security.Cryptography.SHA256");
        }

        new static public SHA256 Create(String hashName) {
            return (SHA256) CryptoConfig.CreateFromName(hashName);
        }
    }
}

