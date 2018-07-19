//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMessageApril2005
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveElement, Namespace = ProtocolStrings.VersionApril2005.Namespace)]
        public ResolveCriteriaApril2005 Resolve
        {
            get;
            set;
        }
    }
}

