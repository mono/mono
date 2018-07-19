//------------------------------------------------------------------------------
// <copyright file="IMasterKeyProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;

    // Represents an object that can provide master encryption / validation keys

    internal interface IMasterKeyProvider {

        // encryption + decryption key
        CryptographicKey GetEncryptionKey();

        // signing + validation key
        CryptographicKey GetValidationKey();

    }
}
