//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ProbeMessageCD1
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ProbeElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public FindCriteriaCD1 Probe
        {
            get;
            set;
        }
    }
}
