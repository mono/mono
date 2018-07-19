//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Policy;

    class NonValidatingSecurityTokenAuthenticator<TTokenType> : SecurityTokenAuthenticator
    {
        public NonValidatingSecurityTokenAuthenticator()
            : base()
        { }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is TTokenType);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
        }
    }
}
