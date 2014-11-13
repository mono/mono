//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMessage11
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveElement, Namespace = ProtocolStrings.Version11.Namespace)]
        public ResolveCriteria11 Resolve
        {
            get;
            set;
        }
    }
}
