//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;

    /// <summary>
    /// Base class for SymmetricProofDescriptor and AsymmetricProofDescriptor.
    /// </summary>
    public abstract class ProofDescriptor
    {
        
        /// <summary>
        /// Sets the appropriate things, such as requested proof token, inside the RSTR 
        /// based on what is inside the proof descriptor instance. 
        /// </summary>
        /// <param name="response">The RSTR object that this proof descriptor needs to modify.</param>
        public abstract void ApplyTo(RSTR response);


        /// <summary>
        /// Gets the key identifier that can be used inside issued to define the key
        /// either symmetric or assymetric. If the key is symmetric, it is usually the binary secret
        /// or encrypted key; if it is asymmetric it is usually the key identifier from the RST/UseKey
        /// i.e. the public key.
        /// </summary>
        public abstract SecurityKeyIdentifier KeyIdentifier { get; }
    }
}
