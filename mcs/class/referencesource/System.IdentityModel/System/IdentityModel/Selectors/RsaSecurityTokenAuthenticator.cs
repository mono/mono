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

    public class RsaSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        public RsaSecurityTokenAuthenticator()
        {
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is RsaSecurityToken;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            RsaSecurityToken rsaToken = (RsaSecurityToken)token;
            List<Claim> claims = new List<Claim>(2);
            claims.Add(new Claim(ClaimTypes.Rsa, rsaToken.Rsa, Rights.Identity));
            claims.Add(Claim.CreateRsaClaim(rsaToken.Rsa));

            DefaultClaimSet claimSet = new DefaultClaimSet(ClaimSet.Anonymous, claims);
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new UnconditionalPolicy(claimSet, rsaToken.ValidTo));
            return policies.AsReadOnly();
        }
    }
}
