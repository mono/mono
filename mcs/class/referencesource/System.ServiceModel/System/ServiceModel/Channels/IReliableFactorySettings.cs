//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//--------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    interface IReliableFactorySettings
    {
        TimeSpan AcknowledgementInterval { get; }

        bool FlowControlEnabled { get; }

        TimeSpan InactivityTimeout { get; }

        int MaxPendingChannels { get; }

        int MaxRetryCount { get; }

        int MaxTransferWindowSize { get; }

        MessageVersion MessageVersion { get; }

        bool Ordered { get; }

        ReliableMessagingVersion ReliableMessagingVersion { get; }

        TimeSpan SendTimeout { get; }
    }
}
