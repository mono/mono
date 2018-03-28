//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Serialization;

    [KnownType(typeof(List<ServiceInfo>))]
    internal sealed class ServiceInfoCollection : Collection<ServiceInfo>
    {
        internal ServiceInfoCollection(IEnumerable<ServiceHostBase> services)
        {
            foreach (ServiceHostBase service in services)
            {
                base.Add(new ServiceInfo(service));
            }
        }
    }
}
