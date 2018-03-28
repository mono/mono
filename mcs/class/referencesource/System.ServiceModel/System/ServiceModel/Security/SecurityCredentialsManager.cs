//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;

    public abstract class SecurityCredentialsManager
    {
        protected SecurityCredentialsManager() { }

        public abstract SecurityTokenManager CreateSecurityTokenManager();
    }
}
