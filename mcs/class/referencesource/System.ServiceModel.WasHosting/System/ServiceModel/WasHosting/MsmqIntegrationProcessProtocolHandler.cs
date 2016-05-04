//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Activation;
    using System.Web.Hosting;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "Instantiated by ASP.NET")]
    class MsmqIntegrationProcessProtocolHandler : BaseProcessProtocolHandler
    {
        public MsmqIntegrationProcessProtocolHandler()
            : base(MsmqUri.FormatNameAddressTranslator.Scheme)
        { }

        internal override void HandleStartListenerChannelError(IListenerChannelCallback listenerChannelCallback, Exception ex)
        {
            listenerChannelCallback.ReportStarted();
            listenerChannelCallback.ReportStopped(Marshal.GetHRForException(ex));
        } 
    }
}

