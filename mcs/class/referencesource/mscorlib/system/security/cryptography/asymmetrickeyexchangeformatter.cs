// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// AsymmetricKeyExchangeFormatter.cs
//

namespace System.Security.Cryptography {
    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class AsymmetricKeyExchangeFormatter {
        //
        // protected constructors
        //
    
        protected AsymmetricKeyExchangeFormatter() {
        }

        //
        // public properties
        //

        public abstract String Parameters {
            get;
        }
    
        //
        // public methods
        //

        abstract public void SetKey(AsymmetricAlgorithm key);
        abstract public byte[] CreateKeyExchange(byte[] data);
        abstract public byte[] CreateKeyExchange(byte[] data, Type symAlgType);
    }    
}    
