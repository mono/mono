//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMessageApril2005
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public FindCriteriaApril2005 Probe
        {
            get;
            set;
        }
    }
}

