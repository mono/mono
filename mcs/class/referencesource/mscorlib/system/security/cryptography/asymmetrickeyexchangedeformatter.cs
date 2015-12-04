// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// AsymmetricKeyExchangeDeformatter.cs
//

namespace System.Security.Cryptography {
    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class AsymmetricKeyExchangeDeformatter {
        //
        // protected constructors
        //
    
        protected AsymmetricKeyExchangeDeformatter() {
        }
    
        //
        // public properties
        //

        public abstract String Parameters {
            get;
            set;
        }

        //
        // public methods
        //

        abstract public void SetKey(AsymmetricAlgorithm key);
        abstract public byte[] DecryptKeyExchange(byte[] rgb);
    }
}    
