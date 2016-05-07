//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;

    public abstract class UserNameSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected UserNameSecurityTokenAuthenticator()
        { 
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is UserNameSecurityToken;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            UserNameSecurityToken userNameToken = (UserNameSecurityToken) token;
            return ValidateUserNamePasswordCore(userNameToken.UserName, userNameToken.Password);
        }

        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password);
    }
}
