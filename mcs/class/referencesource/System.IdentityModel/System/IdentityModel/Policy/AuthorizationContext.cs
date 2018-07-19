//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;

    public abstract class AuthorizationContext : IAuthorizationComponent
    {
        public abstract string Id { get; }
        public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }
        public abstract DateTime ExpirationTime { get; }
        public abstract IDictionary<string, object> Properties { get; }

        public static AuthorizationContext CreateDefaultAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
        {
            return SecurityUtils.CreateDefaultAuthorizationContext(authorizationPolicies);
        }
    }
}
