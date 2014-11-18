//-----------------------------------------------------------------------
// <copyright file="ProtectedKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// This class are used in defining Entropy and RequestProofToken element inside the 
    /// RequestSecurityToken and RequestSecurityTokenResponse.
    /// </summary>
    public class ProtectedKey
    {
        byte[] _secret;
        EncryptingCredentials _wrappingCredentials;
        
        /// <summary>
        /// Use this constructor if we want to send the key material in clear text.
        /// </summary>
        /// <param name="secret">The key material that needs to be protected.</param>
        public ProtectedKey(byte[] secret)
        {
            _secret = secret;
        }

        /// <summary>
        /// Use this constructor if we want to send the key material encrypted.
        /// </summary>
        /// <param name="secret">The key material that needs to be protected.</param>
        /// <param name="wrappingCredentials">The encrypting credentials used to encrypt the key material.</param>
        public ProtectedKey(byte[] secret, EncryptingCredentials wrappingCredentials)
        {
            _secret = secret;
            _wrappingCredentials = wrappingCredentials;
        }

        /// <summary>
        /// Gets the key material.
        /// </summary>
        public byte[] GetKeyBytes()
        {
            return _secret;
        }

        /// <summary>
        /// Gets the encrypting credentials. Null means that the keys are not encrypted.
        /// </summary>
        public EncryptingCredentials WrappingCredentials
        {
            get 
            { 
                return _wrappingCredentials; 
            }
        }
    }
}

