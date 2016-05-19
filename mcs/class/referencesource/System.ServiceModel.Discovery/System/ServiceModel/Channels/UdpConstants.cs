//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;

    static class UdpConstants
    {
        // max is 64K - 20 (IP header) - 8(UDP header) - 1 (wraparound)
        public const int MaxMessageSizeOverIPv4 = 64 * 1024 - 20 - 8 - 1;
        public const int MaxTimeToLive = 255;
        public const int MinReceiveBufferSize = 1;
        public const int MinTimeToLive = 0; //localhost traffic only
        public const int PendingReceiveCountPerProcessor = 2;
        public const string Scheme = "soap.udp";

        internal static class Defaults
        {
            public static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMinutes(1);
            public static readonly TimeSpan SendTimeout = TimeSpan.FromMinutes(1);
            public const int DuplicateMessageHistoryLength = 0;
            public const int InterfaceIndex = -1;
            public const int MaxPendingMessageCount = 32;
            public const long MaxReceivedMessageSize = SocketReceiveBufferSize;
            public const int SocketReceiveBufferSize = 64 * 1024;
            public const int TimeToLive = 1;
            public static MessageEncoderFactory MessageEncoderFactory = new TextMessageEncodingBindingElement().CreateMessageEncoderFactory();
        }

    }
}
