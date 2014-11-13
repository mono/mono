//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    static class DiscoveryDefaults
    {
        public static readonly TimeSpan DiscoveryOperationDuration = TimeSpan.Parse(DiscoveryOperationDurationString, CultureInfo.InvariantCulture);

        public static readonly Uri ScopeMatchBy = FindCriteria.ScopeMatchByPrefix;
        public const string DiscoveryOperationDurationString = "00:00:20";
        public const int DuplicateMessageHistoryLength = 2 * 1028;

        public static class Udp
        {
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
            public static readonly Uri IPv4MulticastAddress = new Uri(ProtocolStrings.Udp.MulticastIPv4Address);

            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
            public static readonly Uri IPv6MulticastAddress = new Uri(ProtocolStrings.Udp.MulticastIPv6Address);
            public static readonly TimeSpan AppMaxDelay = TimeSpan.Parse(AppMaxDelayString, CultureInfo.InvariantCulture);

            public const string AppMaxDelayString = "00:00:00.500";

            public const int DuplicateMessageHistoryLength = 4 * 1028;
            public const int MaxUnicastRetransmitCount = 1;
            public const int MaxMulticastRetransmitCount = 2;

            public static UdpTransportBindingElement CreateUdpTransportBindingElement()
            {
                UdpTransportBindingElement udpBE = new UdpTransportBindingElement();
                udpBE.RetransmissionSettings.MaxUnicastRetransmitCount = MaxUnicastRetransmitCount;
                udpBE.RetransmissionSettings.MaxMulticastRetransmitCount = MaxMulticastRetransmitCount;
                udpBE.RetransmissionSettings.DelayLowerBound = TimeSpan.FromMilliseconds(50);
                udpBE.RetransmissionSettings.DelayUpperBound = TimeSpan.FromMilliseconds(250);
                udpBE.RetransmissionSettings.MaxDelayPerRetransmission = TimeSpan.FromMilliseconds(500);
                udpBE.DuplicateMessageHistoryLength = DuplicateMessageHistoryLength;

                // The default value of ManualAddressing on UDP transport is false. 
                // In discovery case, the discovery endpoints will receive all kinds of discovery messages, which sometimes
                // don't match the service contract. In this case, we want the discovery endpoint to ---- the errors
                // instead of sending fault messages back. So we need to disable auto addressing for the discovery scenario.
                udpBE.ManualAddressing = true;

                return udpBE;
            }
        }
    }
}
