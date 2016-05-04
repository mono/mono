//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;

    static class UdpConstants
    {
        // max is 64K - 20 (IP header) - 8(UDP header) - 1 (wraparound)
        public const int MaxMessageSizeOverIPv4 = 64 * 1024 - 20 - 8 - 1;
        public const int MaxTimeToLive = 255;
        public const long MinPendingMessagesTotalSize = 0;
        public const int MinReceiveBufferSize = 1;
        public const int MinTimeToLive = 0; //localhost traffic only
        public const int PendingReceiveCountPerProcessor = 2;
        public const string Scheme = "soap.udp";
        public const string TimeSpanZero = "00:00:00";
        public const string WsdlSoapUdpTransportUri = "http://schemas.microsoft.com/soap/udp";
        public const string WsdlSoapUdpTransportNamespace = "http://schemas.microsoft.com/ws/06/2010/policy/soap/udp";
        public const string WsdlSoapUdpTransportPrefix = "sud";
        public const string RetransmissionEnabled = "RetransmissionEnabled";

        internal static class Defaults
        {
            public static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMinutes(1);
            public static readonly TimeSpan SendTimeout = TimeSpan.FromMinutes(1);
            public const string EncodingString = "utf-8";
            public const string DelayLowerBound = "00:00:00.050";
            public const string DelayUpperBound = "00:00:00.250";
            public const int DuplicateMessageHistoryLength = 0;
            public const int DuplicateMessageHistoryLengthWithRetransmission = 4096;
            public const int InterfaceIndex = -1;
            public const string MaxDelayPerRetransmission = "00:00:00.500";
            public const int MaxRetransmitCount = 0;
            public const int MaxUnicastRetransmitCount = 0;
            public const int MaxMulticastRetransmitCount = 0;
            public const long DefaultMaxPendingMessagesTotalSize = 0;
            public static readonly long MaxPendingMessagesTotalSize = 1024 * 1024 * Environment.ProcessorCount;  // 512 * 2K messages per processor
            public const long MaxReceivedMessageSize = SocketReceiveBufferSize;
            public const string MulticastInterfaceId = null;
            public const int SocketReceiveBufferSize = 64 * 1024;
            public const int TimeToLive = 1;
            public static MessageEncoderFactory MessageEncoderFactory = new TextMessageEncodingBindingElement().CreateMessageEncoderFactory();

            public static readonly TimeSpan DelayLowerBoundTimeSpan = TimeSpan.Parse(DelayLowerBound, CultureInfo.InvariantCulture);
            public static readonly TimeSpan DelayUpperBoundTimeSpan = TimeSpan.Parse(DelayUpperBound, CultureInfo.InvariantCulture);
            public static readonly TimeSpan MaxDelayPerRetransmissionTimeSpan = TimeSpan.Parse(MaxDelayPerRetransmission, CultureInfo.InvariantCulture);
        }
    }
}
