//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------


namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Diagnostics;

    sealed class TcpHostedTransportConfiguration : HostedTransportConfigurationBase
    {
        HostedTcpTransportManager uniqueManager;

        public TcpHostedTransportConfiguration()
            : base(Uri.UriSchemeNetTcp)
        {
            string[] bindings = HostedTransportConfigurationManager.MetabaseSettings.GetBindings(Uri.UriSchemeNetTcp);
            for (int i = 0; i < bindings.Length; i++)
            {
                BaseUriWithWildcard listenAddress = BaseUriWithWildcard.CreateHostedUri(Uri.UriSchemeNetTcp, bindings[i], HostingEnvironmentWrapper.ApplicationVirtualPath);
                if (i == 0)
                {
                    Debug.Print("TcpHostedTransportConfiguration.ctor() Creating the unique TcpTransportManager with ListenUri:" + listenAddress.BaseAddress);
                    uniqueManager = new HostedTcpTransportManager(listenAddress);
                }

                this.ListenAddresses.Add(listenAddress);
                Debug.Print("Registering the unique TcpTransportManager with ListenUri:" + listenAddress.BaseAddress);
                TcpChannelListener.StaticTransportManagerTable.RegisterUri(listenAddress.BaseAddress, listenAddress.HostNameComparisonMode, uniqueManager);
            }
        }

        internal TcpTransportManager TransportManager { get { return uniqueManager; } }
    }
}
