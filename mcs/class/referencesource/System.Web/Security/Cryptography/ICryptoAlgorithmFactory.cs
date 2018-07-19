//------------------------------------------------------------------------------
// <copyright file="ICryptoAlgorithmFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;
    using System.Security.Cryptography;

    // Represents an object that can provide encryption + validation algorithm instances

    internal interface ICryptoAlgorithmFactory {

        // Gets a SymmetricAlgorithm instance that can be used for encryption / decryption
         SymmetricAlgorithm GetEncryptionAlgorithm();

        // Gets a KeyedHashAlgorithm instance that can be used for signing / validation
         KeyedHashAlgorithm GetValidationAlgorithm();
    
    }
}
