//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "Instantiated by ASP.NET")]
    class TcpProcessProtocolHandler : BaseProcessProtocolHandler
    {
        public TcpProcessProtocolHandler()
            : base(Uri.UriSchemeNetTcp)
        { }
    }
}

