//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;

    class DefaultEvaluationContext : EvaluationContext
    {
        List<ClaimSet> claimSets;
        Dictionary<string, object> properties;
        DateTime expirationTime = SecurityUtils.MaxUtcDateTime;
        int generation;

        ReadOnlyCollection<ClaimSet> readOnlyClaimSets;

        public DefaultEvaluationContext()
        {
            this.properties = new Dictionary<string, object>();
            this.generation = 0;
        }

        public override int Generation
        {
            get { return this.generation; }
        }

        public override ReadOnlyCollection<ClaimSet> ClaimSets
        {
            get 
            {
                if (this.claimSets == null)
                    return EmptyReadOnlyCollection<ClaimSet>.Instance;

                if (this.readOnlyClaimSets == null)
                    this.readOnlyClaimSets = new ReadOnlyCollection<ClaimSet>(this.claimSets);

                return this.readOnlyClaimSets; 
            }
        }

        public override IDictionary<string, object> Properties
        {
            get { return this.properties; }
        }

        public DateTime ExpirationTime
        {
            get { return this.expirationTime; }
        }

        public override void AddClaimSet(IAuthorizationPolicy policy, ClaimSet claimSet)
        {
            if (claimSet == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimSet");

            if (this.claimSets == null)
                this.claimSets = new List<ClaimSet>();

            this.claimSets.Add(claimSet);
            ++this.generation;
        }

        public override void RecordExpirationTime(DateTime expirationTime)
        {
            if (this.expirationTime > expirationTime)
                this.expirationTime = expirationTime;
        }
    }
}
