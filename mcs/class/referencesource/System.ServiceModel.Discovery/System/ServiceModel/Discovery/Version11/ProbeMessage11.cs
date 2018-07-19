//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMessage11
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public FindCriteria11 Probe
        {
            get;
            set;
        }
    }
}
