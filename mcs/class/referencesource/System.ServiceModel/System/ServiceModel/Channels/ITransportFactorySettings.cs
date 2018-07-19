//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Diagnostics;

    interface IConnectionOrientedConnectionSettings
    {
        int ConnectionBufferSize { get; }
        TimeSpan MaxOutputDelay { get; }
        TimeSpan IdleTimeout { get; }
    }

    interface IConnectionOrientedListenerSettings : IConnectionOrientedConnectionSettings
    {
        TimeSpan ChannelInitializationTimeout { get; }
        int MaxPendingConnections { get; }
        int MaxPendingAccepts { get; }
        int MaxPooledConnections { get; }
    }

    interface ITransportFactorySettings : IDefaultCommunicationTimeouts
    {
        bool ManualAddressing { get; }
        BufferManager BufferManager { get; }
        long MaxReceivedMessageSize { get; }
        MessageEncoderFactory MessageEncoderFactory { get; }
        MessageVersion MessageVersion { get; }
    }

    interface IConnectionOrientedTransportFactorySettings : ITransportFactorySettings, IConnectionOrientedConnectionSettings
    {
        int MaxBufferSize { get; }
        StreamUpgradeProvider Upgrade { get; }
        TransferMode TransferMode { get; }
        // Audit
        ServiceSecurityAuditBehavior AuditBehavior { get; }
    }

    interface IConnectionOrientedTransportChannelFactorySettings : IConnectionOrientedTransportFactorySettings
    {
        string ConnectionPoolGroupName { get; }
        int MaxOutboundConnectionsPerEndpoint { get; }
    }

    interface ITcpChannelFactorySettings : IConnectionOrientedTransportChannelFactorySettings
    {
        TimeSpan LeaseTimeout { get; }
    }

    interface IHttpTransportFactorySettings : ITransportFactorySettings
    {
        int MaxBufferSize { get; }
        TransferMode TransferMode { get; }
    }

    interface IPipeTransportFactorySettings : IConnectionOrientedTransportChannelFactorySettings
    {
        NamedPipeSettings PipeSettings { get; }
    }
}
