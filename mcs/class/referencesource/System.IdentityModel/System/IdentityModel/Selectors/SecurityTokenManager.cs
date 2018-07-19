//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    /// <summary>
    /// Uber class that will create SecurityTokenProvider, SecurityTokenAuthenticator and SecurityTokenSerializer objects
    /// </summary>
    public abstract class SecurityTokenManager
    {
        public abstract SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement);
        public abstract SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version);
        public abstract SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver);
    }
}
