//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Runtime;

    class DefaultDiscoveryService : DiscoveryService
    {
        readonly ReadOnlyCollection<EndpointDiscoveryMetadata> publishedEndpoints;

        public DefaultDiscoveryService(
            DiscoveryServiceExtension discoveryServiceExtension,
            DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator,
            int duplicateMessageHistoryLength)
            : base(discoveryMessageSequenceGenerator, duplicateMessageHistoryLength)

        {
            Fx.Assert(discoveryServiceExtension != null, "The discoveryServiceExtension must be non null.");
            this.publishedEndpoints = discoveryServiceExtension.PublishedEndpoints;
        }

        protected override IAsyncResult OnBeginFind(
            FindRequestContext findRequestContext, 
            AsyncCallback callback, 
            object state)
        {
            this.Match(findRequestContext);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndFind(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<EndpointDiscoveryMetadata>(
                this.Match(resolveCriteria),
                callback,
                state);
        }

        protected override EndpointDiscoveryMetadata OnEndResolve(IAsyncResult result)
        {
            return CompletedAsyncResult<EndpointDiscoveryMetadata>.End(result);
        }

        EndpointDiscoveryMetadata Match(ResolveCriteria criteria)
        {
            for (int i = 0; i < this.publishedEndpoints.Count; i++)
            {
                if (this.publishedEndpoints[i].Address.Equals(criteria.Address))
                {
                    return this.publishedEndpoints[i];
                }
            }

            return null;
        }

        void Match(FindRequestContext findRequestContext)
        {
            FindCriteria criteria = findRequestContext.Criteria;

            if (!ScopeCompiler.IsSupportedMatchingRule(criteria.ScopeMatchBy))
            {
                return;
            }

            CompiledScopeCriteria[] compiledScopeCriterias = ScopeCompiler.CompileMatchCriteria(
                criteria.InternalScopes, 
                criteria.ScopeMatchBy);

            int matchingEndpointCount = 0;
            for (int i = 0; i < this.publishedEndpoints.Count; i++)
            {
                if (criteria.IsMatch(this.publishedEndpoints[i], compiledScopeCriterias))
                {
                    findRequestContext.AddMatchingEndpoint(this.publishedEndpoints[i]);
                    matchingEndpointCount++;

                    if (matchingEndpointCount == criteria.MaxResults)
                    {
                        break;
                    }
                }
            }
        }
    }
}
