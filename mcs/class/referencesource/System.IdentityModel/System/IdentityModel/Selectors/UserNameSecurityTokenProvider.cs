//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.IdentityModel.Tokens;

    public class UserNameSecurityTokenProvider : SecurityTokenProvider
    {
        UserNameSecurityToken userNameToken;

        public UserNameSecurityTokenProvider(string userName, string password)
        {
            if (userName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
            }
            
            this.userNameToken = new UserNameSecurityToken(userName, password);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.userNameToken;
        }
    }
}
