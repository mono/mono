//-----------------------------------------------------------------------
// <copyright file="IdentityModelCaches.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System.IdentityModel.Tokens;

    /// <summary>
    /// Defines caches supported by IdentityModel for TokenReplay and SecuritySessionTokens
    /// </summary>
    public sealed class IdentityModelCaches
    {
        private TokenReplayCache tokenReplayCache = new DefaultTokenReplayCache();
        private SessionSecurityTokenCache sessionSecurityTokenCache = new MruSessionSecurityTokenCache();

        /// <summary>
        /// Gets or sets the TokenReplayCache that is used to determine replayed token.
        /// </summary>
        public TokenReplayCache TokenReplayCache
        {
            get
            {
                return this.tokenReplayCache;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.tokenReplayCache = value;
            }
        }

        /// <summary>
        /// Gets or sets the SessionSecurityTokenCache that is used to cache the <see cref="SessionSecurityToken"/>
        /// </summary>
        public SessionSecurityTokenCache SessionSecurityTokenCache
        {
            get
            {
                return this.sessionSecurityTokenCache;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.sessionSecurityTokenCache = value;
            }
        }
    }
}
