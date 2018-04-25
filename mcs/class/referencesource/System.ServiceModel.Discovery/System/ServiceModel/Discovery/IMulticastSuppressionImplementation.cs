//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;

    interface IMulticastSuppressionImplementation
    {
        IAsyncResult BeginShouldRedirectFind(FindCriteria findCriteria, AsyncCallback callback, object state);
        bool EndShouldRedirectFind(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints);

        IAsyncResult BeginShouldRedirectResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state);
        bool EndShouldRedirectResolve(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints);
    }
}
