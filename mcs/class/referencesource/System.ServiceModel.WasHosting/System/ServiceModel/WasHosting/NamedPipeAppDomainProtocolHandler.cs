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
    class NamedPipeAppDomainProtocolHandler : BaseAppDomainProtocolHandler
    {
        HostedNamedPipeTransportManager transportManager;

        public NamedPipeAppDomainProtocolHandler()
            : base(Uri.UriSchemeNetPipe)
        { }

        protected override void OnStart()
        {
            NamedPipeHostedTransportConfiguration configuration = 
                HostedTransportConfigurationManager.GetConfiguration(Uri.UriSchemeNetPipe) as NamedPipeHostedTransportConfiguration;
            transportManager = configuration.TransportManager as HostedNamedPipeTransportManager;
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

