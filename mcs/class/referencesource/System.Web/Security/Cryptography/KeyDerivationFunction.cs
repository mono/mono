//------------------------------------------------------------------------------
// <copyright file="KeyDerivationFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;

    // A delegate that represents a cryptographic key derivation function (KDF).
    // A KDF takes a master key (the key derivation key) and a purpose string,
    // producing a derived key in the process.
    internal delegate CryptographicKey KeyDerivationFunction(CryptographicKey keyDerivationKey, Purpose purpose);

}
