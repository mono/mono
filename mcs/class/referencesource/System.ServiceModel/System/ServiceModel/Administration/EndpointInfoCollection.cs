//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    internal sealed class EndpointInfoCollection : Collection<EndpointInfo>
    {
        internal EndpointInfoCollection(ServiceEndpointCollection endpoints, string serviceName)
        {
            for (int i = 0; i < endpoints.Count; ++i)
            {
                base.Add(new EndpointInfo(endpoints[i], serviceName));
            }
        }
    }
}
