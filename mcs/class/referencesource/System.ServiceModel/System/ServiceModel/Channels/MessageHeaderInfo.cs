//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel;
    using System.Diagnostics;

    public abstract class MessageHeaderInfo
    {
        public abstract string Actor { get; }
        public abstract bool IsReferenceParameter { get; }
        public abstract string Name { get; }
        public abstract string Namespace { get; }
        public abstract bool MustUnderstand { get; }
        public abstract bool Relay { get; }
    }
}
