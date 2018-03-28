//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "Instantiated by ASP.NET")]
    class TcpAppDomainProtocolHandler : BaseAppDomainProtocolHandler
    {
        HostedTcpTransportManager transportManager;
        public TcpAppDomainProtocolHandler()
            : base(Uri.UriSchemeNetTcp)
        {
        }

        protected override void OnStart()
        {
            TcpHostedTransportConfiguration configuration = HostedTransportConfigurationManager.GetConfiguration(Uri.UriSchemeNetTcp) as TcpHostedTransportConfiguration;
            transportManager = configuration.TransportManager as HostedTcpTransportManager;
            transportManager.Start(listenerChannelContext.ListenerChannelId, listenerChannelContext.Token, OnMessageReceived);
        }

        protected override void OnStop()
        {
            if (transportManager != null)
            {
                transportManager.Stop(DefaultStopTimeout);
            }
        }
    }
}

