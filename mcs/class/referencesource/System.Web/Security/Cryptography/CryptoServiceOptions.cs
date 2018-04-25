//------------------------------------------------------------------------------
// <copyright file="CryptoServiceOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Security.Cryptography {
    using System;

    // Describes options that can configure an ICryptoService.

    internal enum CryptoServiceOptions {

        // [default] no special behavior needed
        None = 0,

        // the output of the Protect method will be cached, so the same plaintext should lead to the same ciphertext (no randomness)
        CacheableOutput,

    }
}
