//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel.Channels;

    static class ListenerConstants
    {
        // Default constants for configurable settings
        public const int DefaultListenBacklog = TcpTransportDefaults.ListenBacklogConst;
        public const int DefaultMaxPendingAccepts = 0;
        public const int DefaultMaxPendingConnections = 100;
        public const string DefaultReceiveTimeoutString = "00:00:30";
        public const bool DefaultTeredoEnabled = false;
        public const bool DefaultPerformanceCountersEnabled = true;

        // Registration service binding settings
        public const int RegistrationMaxConcurrentSessions = int.MaxValue;
        // based on empirical observations, I've never seen it go over 9018 (seems to be ~8k plus soap goo)
        // we can be safer here, since we don't actually increase the memeory usage
        public const int RegistrationMaxReceivedMessageSize = 10000;
        public static readonly TimeSpan RegistrationCloseTimeout = TimeSpan.FromSeconds(2);

        // Shared connection settings
        // we shouldn't be needing to read more than 2115 bytes to dipatch a session
        public const int SharedConnectionBufferSize = 2500;
        public const int SharedMaxDrainSize = TransportDefaults.MaxDrainSize;
        public static readonly TimeSpan SharedSendTimeout = ServiceDefaults.SendTimeout;
        public const int SharedMaxContentTypeSize = ConnectionOrientedTransportDefaults.MaxContentTypeSize;

        // Internal listener global settings
        public const int MaxRetries = 5;
        public const int MaxUriSize = ConnectionOrientedTransportDefaults.MaxViaSize;
        public static readonly TimeSpan ServiceStartTimeout = TimeSpan.FromSeconds(10);
        public const int ServiceStopTimeout = 30000;
        public static readonly TimeSpan WasConnectTimeout = TimeSpan.FromSeconds(120);

        // Constant strings
        public const string GlobalPrefix = "Global\\";
        public const string MsmqActivationServiceName = "NetMsmqActivator";
        public const string NamedPipeActivationServiceName = "NetPipeActivator";
        public const string NamedPipeSharedMemoryName = NamedPipeActivationServiceName + "/endpoint";
        public const string TcpActivationServiceName = "NetTcpActivator";
        public const string TcpPortSharingServiceName = "NetTcpPortSharing";
        public const string TcpSharedMemoryName = TcpPortSharingServiceName + "/endpoint";
    }
}
