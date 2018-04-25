//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Xml;

    interface IDiscoveryServiceImplementation
    {
        bool IsDuplicate(UniqueId messageId);

        DiscoveryMessageSequence GetNextMessageSequence();

        IAsyncResult BeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state);
        void EndFind(IAsyncResult result);

        IAsyncResult BeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state);
        EndpointDiscoveryMetadata EndResolve(IAsyncResult result);
    }
}
