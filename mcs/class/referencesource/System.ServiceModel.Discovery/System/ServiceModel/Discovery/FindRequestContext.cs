//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.Collections.ObjectModel;

    [Fx.Tag.XamlVisible(false)]
    public class FindRequestContext
    {
        readonly FindCriteria criteria;

        protected FindRequestContext(FindCriteria criteria)
        {
            Fx.Assert(criteria != null, "The criteria must be non null.");

            this.criteria = criteria;
        }

        public FindCriteria Criteria 
        {
            get
            {
                return this.criteria;
            }
        }

        public void AddMatchingEndpoint(EndpointDiscoveryMetadata matchingEndpoint) 
        {
            if (matchingEndpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("matchingEndpoint");
            }

            this.OnAddMatchingEndpoint(matchingEndpoint);
        }

        protected virtual void OnAddMatchingEndpoint(EndpointDiscoveryMetadata matchingEndpoint)
        {
        }
    }
}
