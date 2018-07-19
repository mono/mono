// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// AsymmetricSignatureFormatter.cs
//

namespace System.Security.Cryptography {
    using System;
    using System.Diagnostics.Contracts;

[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class AsymmetricSignatureFormatter {
        //
        // protected constructors
        //
    
        protected AsymmetricSignatureFormatter() {
        }
    
        //
        // public methods
        //
    
        abstract public void SetKey(AsymmetricAlgorithm key);
        abstract public void SetHashAlgorithm(String strName);
    
        public virtual byte[] CreateSignature(HashAlgorithm hash) {
            if (hash == null) throw new ArgumentNullException("hash");
            Contract.EndContractBlock();
            SetHashAlgorithm(hash.ToString());
            return CreateSignature(hash.Hash);
        }
        
        abstract public byte[] CreateSignature(byte[] rgbHash);    
    }    
}    
