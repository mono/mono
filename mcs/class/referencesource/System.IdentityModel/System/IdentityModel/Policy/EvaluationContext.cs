//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;

    public abstract class EvaluationContext
    {
        public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }
        public abstract IDictionary<string, object> Properties { get; }
        public abstract int Generation { get; }
        public abstract void AddClaimSet(IAuthorizationPolicy policy, ClaimSet claimSet);
        public abstract void RecordExpirationTime(DateTime expirationTime);
    }
}
