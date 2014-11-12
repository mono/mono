// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// MD5.cs
//

namespace System.Security.Cryptography {
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class MD5 : HashAlgorithm
    {      
        protected MD5() {
            HashSizeValue = 128;
        }
    
        //
        // public methods
        //

        new static public MD5 Create() {
            return Create("System.Security.Cryptography.MD5");
        }

        new static public MD5 Create(String algName) {
            return (MD5) CryptoConfig.CreateFromName(algName);
        }
    }
}
