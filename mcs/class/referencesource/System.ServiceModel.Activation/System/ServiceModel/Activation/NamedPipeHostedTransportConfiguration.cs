//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Diagnostics;

    sealed class NamedPipeHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        HostedNamedPipeTransportManager uniqueManager;

        public NamedPipeHostedTransportConfiguration()
            : base(Uri.UriSchemeNetPipe)
        {
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(Uri.UriSchemeNetPipe);
            for (int i = 0; i < bindings.Length; i++)
            {
                BaseUriWithWildcard listenAddress = BaseUriWithWildcard.CreateHostedPipeUri(bindings[i], HostingEnvironmentWrapper.ApplicationVirtualPath);
                if (i == 0)
                {
                    Debug.Print("NamedPipeHostedTransportConfiguration.ctor() Creating the unique NamedPipeTransportManager with ListenUri:" + listenAddress.BaseAddress);
                    uniqueManager = new HostedNamedPipeTransportManager(listenAddress);
                }

                this.ListenAddresses.Add(listenAddress);
                Debug.Print("Registering the unique NamedPipeTransportManager with ListenUri:" + listenAddress.BaseAddress);
                NamedPipeChannelListener.StaticTransportManagerTable.RegisterUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, uniqueManager);
            }
        }

        internal NamedPipeTransportManager TransportManager { get { return uniqueManager; } }
    }
}

