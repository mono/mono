//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Claims;
    using System.Security.Principal;

    class KerberosRequestorSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        public KerberosRequestorSecurityTokenAuthenticator()
            : base()
        { }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is KerberosRequestorSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            KerberosRequestorSecurityToken kerbToken = (KerberosRequestorSecurityToken) token;
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            ClaimSet claimSet = new DefaultClaimSet(ClaimSet.System, new Claim(ClaimTypes.Spn, kerbToken.ServicePrincipalName, Rights.PossessProperty));
            policies.Add(new UnconditionalPolicy(SecurityUtils.CreateIdentity(kerbToken.ServicePrincipalName, SecurityUtils.AuthTypeKerberos), claimSet));
            return policies.AsReadOnly();
        }
    }
}
