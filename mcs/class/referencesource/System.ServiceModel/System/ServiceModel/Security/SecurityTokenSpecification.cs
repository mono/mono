//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security.Tokens;

    public class SecurityTokenSpecification
    {
        SecurityToken token;
        ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies;

        public SecurityTokenSpecification(SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies)
        {
            this.token = token;
            if (tokenPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenPolicies");
            }
            this.tokenPolicies = tokenPolicies;
        }

        public SecurityToken SecurityToken 
        {
            get { return this.token; }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> SecurityTokenPolicies
        {
            get { return this.tokenPolicies; }
        }
    }
}
