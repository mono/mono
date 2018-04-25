//-----------------------------------------------------------------------
// <copyright file="UseKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// This optional element enables the client to request the Identity provider to issue a token
    /// containing his public key which is specified under the 'UseKey' element. However, the client
    /// has to prove possesion of the key. In a WS-Security based SOAP message the client can add
    /// his certificate as an endorsing token to the Security header to prove possession of the key.
    /// </summary>
    public class UseKey
    {
        SecurityToken _token;
        SecurityKeyIdentifier _ski;

        /// <summary>
        /// Constructor for extensibility point
        /// </summary>
        public UseKey()
            : base()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="UseKey"/>.
        /// </summary>
        /// <param name="ski">A security key identifier which represents the existing key that should be used. </param>
        public UseKey(SecurityKeyIdentifier ski)
            : this(ski, null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="UseKey"/>.
        /// </summary>
        /// <param name="token">A token which represents existing key that should be used.</param>
        public UseKey(SecurityToken token)
            : this(null, token)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="UseKey"/>.
        /// </summary>
        /// <param name="ski">A security key identifier which represents the existing key that should be used. </param>
        /// <param name="token">A token which represents existing key that should be used.</param>
        public UseKey(SecurityKeyIdentifier ski, SecurityToken token)
        {
            _ski = ski;
            _token = token;
        }

        /// <summary>
        /// Gets the security token.
        /// </summary>
        public SecurityToken Token
        {
            get { return _token; }
        }

        /// <summary>
        /// Gets the security key identifier.
        /// </summary>
        public SecurityKeyIdentifier SecurityKeyIdentifier
        {
            get { return _ski; }
        }
    }
}
