//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    static class UdpTransportConfigurationStrings
    {      
        // UDP transport settings
        internal const string DuplicateMessageHistoryLength = "duplicateMessageHistoryLength";
        internal const string MaxPendingMessagesTotalSize = "maxPendingMessagesTotalSize";
        internal const string MaxReceivedMessageSize = "maxReceivedMessageSize";
        internal const string MaxBufferPoolSize = "maxBufferPoolSize";
        internal const string MulticastInterfaceId = "multicastInterfaceId";
        internal const string SocketReceiveBufferSize = "socketReceiveBufferSize";
        internal const string TimeToLive = "timeToLive";

        // UDP retransmission settings
        internal const string RetransmissionSettings = "retransmissionSettings";
        internal const string DelayLowerBound = "delayLowerBound";
        internal const string DelayUpperBound = "delayUpperBound";
        internal const string MaxDelayPerRetransmission = "maxDelayPerRetransmission";
        internal const string MaxMulticastRetransmitCount = "maxMulticastRetransmitCount";
        internal const string MaxUnicastRetransmitCount = "maxUnicastRetransmitCount";

        // BasicUdpBinding strings
        internal const string UdpBindingElementName = "udpBinding";
        internal const string MaxRetransmitCount = "maxRetransmitCount";
        internal const string ReaderQuotas = "readerQuotas";
        internal const string TextEncoding = "textEncoding";
    }
}
