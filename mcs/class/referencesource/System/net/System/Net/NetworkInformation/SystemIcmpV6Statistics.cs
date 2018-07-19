

    /// <summary><para>
    ///    Provides support for ip configuation information and statistics.
    ///</para></summary>     
    ///
namespace System.Net.NetworkInformation {
    using System.Net.Sockets;
    using System;
    using System.ComponentModel;


    internal enum IcmpV6StatType {
        DestinationUnreachable          =   1,
        PacketTooBig       =   2,
        TimeExceeded        =   3,
        ParameterProblem           =   4,
        EchoRequest         = 128,
        EchoReply           = 129,
        MembershipQuery     = 130,
        MembershipReport    = 131,
        MembershipReduction = 132,
        RouterSolicit          = 133,
        RouterAdvertisement           = 134,
        NeighborSolict        = 135,
        NeighborAdvertisement         = 136,
        Redirect                = 137,
    };


       
    /// <summary>Icmp statistics for Ipv6.</summary>
    internal class SystemIcmpV6Statistics:IcmpV6Statistics {

        MibIcmpInfoEx stats;

        internal SystemIcmpV6Statistics(){

            uint result = UnsafeNetInfoNativeMethods.GetIcmpStatisticsEx(out stats,AddressFamily.InterNetworkV6);

            if (result != IpHelperErrors.Success) {
                throw new NetworkInformationException((int)result);
            }
        }

        public override long MessagesSent{get {return (long)stats.outStats.dwMsgs;}}
        public override long MessagesReceived{get {return (long)stats.inStats.dwMsgs;}}
        public override long ErrorsSent{get {return (long)stats.outStats.dwErrors;}}
        public override long ErrorsReceived{get {return (long)stats.inStats.dwErrors;}}
        public override long DestinationUnreachableMessagesSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.DestinationUnreachable];
            }
        }
        public override long DestinationUnreachableMessagesReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.DestinationUnreachable];
            }
        }
        public override long PacketTooBigMessagesSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.PacketTooBig];
            }
        }
        public override long PacketTooBigMessagesReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.PacketTooBig];
            }
        }
        public override long TimeExceededMessagesSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.TimeExceeded];
            }
        }
        public override long TimeExceededMessagesReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.TimeExceeded];
            }
        }
        public override long ParameterProblemsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.ParameterProblem];
            }
        }
        public override long ParameterProblemsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.ParameterProblem];
            }
        }
        public override long EchoRequestsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.EchoRequest];
            }
        }
        public override long EchoRequestsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.EchoRequest];
            }
        }
        public override long EchoRepliesSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.EchoReply];
            }
        }
        public override long EchoRepliesReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.EchoReply];
            }
        }
        public override long MembershipQueriesSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipQuery];
            }
        }
        public override long MembershipQueriesReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipQuery];
            }
        }
        public override long MembershipReportsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipReport];
            }
        }
        public override long MembershipReportsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipReport];
            }
        }
        public override long MembershipReductionsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipReduction];
            }
        }
        public override long MembershipReductionsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.MembershipReduction];
            }
        }
        public override long RouterAdvertisementsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.RouterAdvertisement];
            }
        }
        public override long RouterAdvertisementsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.RouterAdvertisement];
            }
        }
        public override long RouterSolicitsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.RouterSolicit];
            }
        }
        public override long RouterSolicitsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.RouterSolicit];
            }
        }
        public override long NeighborAdvertisementsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.NeighborAdvertisement];
            }
        }
        public override long NeighborAdvertisementsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.NeighborAdvertisement];
            }
        }
        public override long NeighborSolicitsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.NeighborSolict];
            }
        }
        public override long NeighborSolicitsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.NeighborSolict];
            }
        }
        public override long RedirectsSent{
            get {
                return stats.outStats.rgdwTypeCount[(long)IcmpV6StatType.Redirect];
            }
        }
        public override long RedirectsReceived{
            get {
                return stats.inStats.rgdwTypeCount[(long)IcmpV6StatType.Redirect];
            }
        }
    }
}


