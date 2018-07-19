//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.IdentityModel.Tokens;

namespace System.IdentityModel.Metadata
{
    /// <summary>
    /// Defines the Saml metadata base class.
    /// </summary>
    public abstract class MetadataBase
    {
        SigningCredentials _signingCredentials;

        /// <summary>
        /// Gets or sets the <see cref="SigningCredentials"/>.
        /// </summary>
        public SigningCredentials SigningCredentials
        {
            get { return _signingCredentials; }
            set { _signingCredentials = value; }
        }
    }
}
