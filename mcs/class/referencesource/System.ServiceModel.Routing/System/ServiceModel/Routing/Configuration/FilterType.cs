//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing.Configuration
{
    using System;

    public enum FilterType
    {
        Action,
        EndpointAddress,
        PrefixEndpointAddress,
        And,
        Custom,
        EndpointName,
        MatchAll,
        XPath
    }
}
