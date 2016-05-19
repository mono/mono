//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// Encapsulates the properties that impact the FederatedSecurityTokenProvider's token
    /// retrieval logic.
    /// </summary>
    internal class FederatedClientCredentialsParameters
    {
        SecurityToken _actAs;
        SecurityToken _onBehalfOf;
        SecurityToken _issuedSecurityToken;

        /// <summary>
        /// Gets or sets the SecurityToken sent to the SecurityTokenService as the ActAs element.
        /// </summary>
        public SecurityToken ActAs
        {
            get { return _actAs; }
            set { _actAs = value; }
        }

        /// <summary>
        /// Gets or sets the SecurityToken sent to the SecurityTokenService as the OnBehalfOf element.
        /// </summary>
        public SecurityToken OnBehalfOf
        {
            get { return _onBehalfOf; }
            set { _onBehalfOf = value; }
        }

        /// <summary>
        /// Gets or sets the SecurityToken returned when the FederatedSecurityTokenProvider.GetTokenCore is called.
        /// If this property is not set, a SecurityToken is retrieved from the configured SecurityTokenService as normal.
        /// </summary>
        public SecurityToken IssuedSecurityToken
        {
            get { return _issuedSecurityToken; }
            set { _issuedSecurityToken = value; }
        }
    }
}
