//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.ServiceModel;

    [MessageContract(IsWrapped = false)]
    class ResolveMessageCD1
    {
        [MessageBodyMember(Name = ProtocolStrings.SchemaNames.ResolveElement, Namespace = ProtocolStrings.VersionCD1.Namespace)]
        public ResolveCriteriaCD1 Resolve
        {
            get;
            set;
        }
    }
}

