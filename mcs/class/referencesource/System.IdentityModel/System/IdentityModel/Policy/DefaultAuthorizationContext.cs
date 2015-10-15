//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Policy
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;

    class DefaultAuthorizationContext : AuthorizationContext
    {
        static DefaultAuthorizationContext empty;
        SecurityUniqueId id;
        ReadOnlyCollection<ClaimSet> claimSets;
        DateTime expirationTime;
        IDictionary<string, object> properties;

        public DefaultAuthorizationContext(DefaultEvaluationContext evaluationContext)
        {
            this.claimSets = evaluationContext.ClaimSets;
            this.expirationTime = evaluationContext.ExpirationTime;
            this.properties = evaluationContext.Properties;
        }

        public static DefaultAuthorizationContext Empty
        {
            get
            {
                if (LocalAppContextSwitches.EnableCachedEmptyDefaultAuthorizationContext)
                {
                    if (empty == null)
                        empty = new DefaultAuthorizationContext(new DefaultEvaluationContext());
                    return empty;
                }
                else
                {
                    return new DefaultAuthorizationContext(new DefaultEvaluationContext());
                }
            }
        }

        public override string Id
        {
            get
            {
                if (this.id == null)
                    this.id = SecurityUniqueId.Create();
                return this.id.Value;
            }
        }

        public override ReadOnlyCollection<ClaimSet> ClaimSets
        {
            get { return this.claimSets; }
        }

        public override DateTime ExpirationTime
        {
            get { return this.expirationTime; }
        }

        public override IDictionary<string, object> Properties
        {
            get { return this.properties; }
        }
    }
}
