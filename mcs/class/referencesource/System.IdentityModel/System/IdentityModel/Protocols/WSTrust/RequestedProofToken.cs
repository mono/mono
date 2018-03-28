//-----------------------------------------------------------------------
// <copyright file="RequestedProofToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// The content of a RequestedProofToken element could be EncryptedSecurityToken which means that EncryptedKey is used 
    /// under the RequestedProofToken. If the security token is a regular token, such as a SCT,
    /// then its session key will be the material which gets encrypted.  Another possibility is where
    /// we use combined entropy, then RequestedProofToken will only contain a ComputedKey element.
    /// </summary>
    public class RequestedProofToken
    {
        string _computedKeyAlgorithm;
        ProtectedKey _keys;

        /// <summary>
        /// In case of combined entropy, construct a requestedprooftoken 
        /// instance with computed key algorithm to specify the algorithm used to 
        /// calculate the session key.
        /// </summary>
        /// <param name="computedKeyAlgorithm">The algorithm used to computed the session key in 
        /// the combined entropy case.</param>
        public RequestedProofToken(string computedKeyAlgorithm)
            : base()
        {
            if (string.IsNullOrEmpty(computedKeyAlgorithm))
            {
                DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("computedKeyAlgorithm");
            }

            _computedKeyAlgorithm = computedKeyAlgorithm;
        }

        /// <summary>
        /// When the requested proof token contains real key in plain text.
        /// </summary>
        /// <param name="secret">The key material.</param>
        public RequestedProofToken(byte[] secret)
        {
            _keys = new ProtectedKey(secret);
        }

        /// <summary>
        /// When the requested proof token contains real key encrypted.
        /// </summary>
        /// <param name="secret">The key material.</param>
        /// <param name="wrappingCredentials">The encrypting credentials to encrypt the key material.</param>
        public RequestedProofToken(byte[] secret, EncryptingCredentials wrappingCredentials)
        {
            _keys = new ProtectedKey(secret, wrappingCredentials);
        }

        /// <summary>
        /// Constructs a requested proof token instance with the protected key.
        /// </summary>
        /// <param name="protectedKey">The protected key which can be either binary secret or encrypted key.</param>
        public RequestedProofToken(ProtectedKey protectedKey)
        {
            if (protectedKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("protectedKey");
            }

            _keys = protectedKey;
        }

        /// <summary>
        /// Gets the computed key algorithm used to calculate the session key in the combined 
        /// entropy case.
        /// </summary>
        public string ComputedKeyAlgorithm
        {
            get
            {
                return _computedKeyAlgorithm;
            }
        }

        /// <summary>
        /// In the case when the requested proof token contains the real key, 
        /// ProtectedKey getter will returns the real key bytes either encrypted
        /// or plaintext.
        /// </summary>
        public ProtectedKey ProtectedKey
        {
            get
            {
                return _keys;
            }
        }
    }
}
