//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Security.Principal;

    public class CustomUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        UserNamePasswordValidator validator;

        public CustomUserNameSecurityTokenAuthenticator(UserNamePasswordValidator validator)
        {
            if (validator == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("validator");
            this.validator = validator;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            this.validator.Validate(userName, password);
            return SecurityUtils.CreateAuthorizationPolicies(new UserNameClaimSet(userName, validator.GetType().Name));
        }

        class UserNameClaimSet : DefaultClaimSet, IIdentityInfo
        {
            IIdentity identity;

            public UserNameClaimSet(string userName, string authType)
            {
                if (userName == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");

                this.identity = SecurityUtils.CreateIdentity(userName, authType);

                List<Claim> claims = new List<Claim>(2);
                claims.Add(new Claim(ClaimTypes.Name, userName, Rights.Identity));
                claims.Add(Claim.CreateNameClaim(userName));
                Initialize(ClaimSet.System, claims);
            }

            public IIdentity Identity
            {
                get { return this.identity; }
            }
        }
    }
}
