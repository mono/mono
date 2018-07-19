// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Security.Cryptography.X509Certificates {
    /// <summary>
    ///     Level of trustworthiness assigned to a manifest's signature
    /// </summary>
    public enum TrustStatus {
        /// <summary>
        ///     The signature is by an explicitly distrusted publisher
        /// </summary>
        Untrusted = 0,

        /// <summary>
        ///     The signature itself is not valid
        /// </summary>
        UnknownIdentity,

        /// <summary>
        ///     The signature is valid
        /// </summary>
        KnownIdentity,

        /// <summary>
        ///     The signature is valid and was created by an explicitly trusted publisher
        /// </summary>
        Trusted
    }
}
