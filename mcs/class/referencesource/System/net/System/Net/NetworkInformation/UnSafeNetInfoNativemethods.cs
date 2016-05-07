
namespace System.Net.NetworkInformation {
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using Microsoft.Win32.SafeHandles;

    internal class IpHelperErrors {

        internal const uint Success = 0;
        internal const uint ErrorInvalidFunction = 1;
        internal const uint ErrorNoSuchDevice = 2;
        internal const uint ErrorInvalidData= 13;
        internal const uint ErrorInvalidParameter = 87;
        internal const uint ErrorBufferOverflow = 111;
        internal const uint ErrorInsufficientBuffer = 122;
        internal const uint ErrorNoData= 232;
        internal const uint Pending = 997;
        internal const uint ErrorNotFound = 1168;
    }

    //
    // Per-adapter Flags
    //

    [Flags]
    internal enum AdapterFlags {
        DnsEnabled=               0x01,
        RegisterAdapterSuffix=    0x02,
        DhcpEnabled =                0x04,
        ReceiveOnly =               0x08,
        NoMulticast=               0x10,
        Ipv6OtherStatefulConfig= 0x20,
        // Vista+
        NetBiosOverTcp =        0x40,
        IPv4Enabled =           0x80,
        IPv6Enabled =           0x100,
        IPv6ManagedAddressConfigurationSupported = 0x200,
    };

    [Flags]
    internal enum AdapterAddressFlags{
        DnsEligible = 0x1,
        Transient = 0x2
    }
    internal enum OldOperationalStatus{
        NonOperational      =0,
        Unreachable          =1,
        Disconnected         =2,
        Connecting           =3,
        Connected            =4,
        Operational          =5
    }

    [Flags]
    internal enum GetAdaptersAddressesFlags
    {
        SkipUnicast                 = 0x0001,
        SkipAnycast                 = 0x0002,
        SkipMulticast               = 0x0004,
        SkipDnsServer               = 0x0008,
        IncludePrefix               = 0x0010,
        SkipFriendlyName            = 0x0020,
        IncludeWins                 = 0x0040,
        IncludeGateways             = 0x0080,
        IncludeAllInterfaces        = 0x0100,
        IncludeAllCompartments      = 0x0200,
        IncludeTunnelBindingOrder   = 0x0400,
    }

    /// <summary>
    ///   IpAddressList - store an IP address with its corresponding subnet mask,
    ///   both as dotted decimal strings
    /// </summary>
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    internal struct IpAddrString {
        internal IntPtr               Next;      /* struct _IpAddressList* */
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=16)]
        internal string IpAddress;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=16)]
        internal string    IpMask;
        internal uint              Context;
    };

    /// <summary>
    ///   Core network information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    internal struct FIXED_INFO {
        internal const int MAX_HOSTNAME_LEN               = 128;
        internal const int MAX_DOMAIN_NAME_LEN            = 128;
        internal const int MAX_SCOPE_ID_LEN               = 256;

        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_HOSTNAME_LEN + 4)]
        internal string         hostName;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_DOMAIN_NAME_LEN + 4)]
        internal string         domainName;
        internal uint           currentDnsServer; /* IpAddressList* */
        internal IpAddrString DnsServerList;
        internal NetBiosNodeType           nodeType;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=MAX_SCOPE_ID_LEN + 4)]
        internal string         scopeId;
        internal bool           enableRouting;
        internal bool           enableProxy;
        internal bool           enableDns;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpSocketAddress {
        internal IntPtr address;
        internal int addressLength;

        internal IPAddress MarshalIPAddress() {
            // Determine the address family used to create the IPAddress
            AddressFamily family = (addressLength > SocketAddress.IPv4AddressSize) 
                ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            SocketAddress sockAddress = new SocketAddress(family, addressLength);
            Marshal.Copy(address, sockAddress.m_Buffer, 0, addressLength);

            return sockAddress.GetIPAddress();
        }
    }

    // IP_ADAPTER_ANYCAST_ADDRESS
    // IP_ADAPTER_MULTICAST_ADDRESS
    // IP_ADAPTER_DNS_SERVER_ADDRESS
    // IP_ADAPTER_WINS_SERVER_ADDRESS
    // IP_ADAPTER_GATEWAY_ADDRESS
    [StructLayout(LayoutKind.Sequential)]
    internal struct IpAdapterAddress {
        internal uint length;
        internal AdapterAddressFlags flags;
        internal IntPtr next;
        internal IpSocketAddress address;

        internal static IPAddressCollection MarshalIpAddressCollection(IntPtr ptr) {
            IPAddressCollection addressList = new IPAddressCollection();

            while (ptr != IntPtr.Zero) {
                IpAdapterAddress addressStructure =
                    (IpAdapterAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
                IPAddress address = addressStructure.address.MarshalIPAddress();
                addressList.InternalAdd(address);
                ptr = addressStructure.next;
            }

            return addressList;
        }
        
        internal static IPAddressInformationCollection MarshalIpAddressInformationCollection(IntPtr ptr) {
            IPAddressInformationCollection addressList = new IPAddressInformationCollection();

            while (ptr != IntPtr.Zero) {
                IpAdapterAddress addressStructure = 
                    (IpAdapterAddress)Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
                IPAddress address = addressStructure.address.MarshalIPAddress();
                addressList.InternalAdd(new SystemIPAddressInformation(address, addressStructure.flags));

                ptr = addressStructure.next;
            }

            return addressList;
        }
    }

    // Vista+
    [StructLayout(LayoutKind.Sequential)]
    internal struct IpAdapterUnicastAddress {
        internal uint length;
        internal AdapterAddressFlags flags;
        internal IntPtr next;
        internal IpSocketAddress address;
        internal PrefixOrigin prefixOrigin;
        internal SuffixOrigin suffixOrigin;
        internal DuplicateAddressDetectionState dadState;
        internal uint validLifetime;
        internal uint preferredLifetime;
        internal uint leaseLifetime;
        internal byte prefixLength;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct IpAdapterAddresses {
        internal const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

        internal uint length;
        internal uint index;
        internal IntPtr next;

        // Needs to be ANSI
        [MarshalAs(UnmanagedType.LPStr)]
        internal string AdapterName;

        internal IntPtr firstUnicastAddress;
        internal IntPtr firstAnycastAddress;
        internal IntPtr firstMulticastAddress;
        internal IntPtr firstDnsServerAddress;

        internal string dnsSuffix;
        internal string description;
        internal string friendlyName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
        internal byte[] address;
        internal uint addressLength;
        internal AdapterFlags flags;
        internal uint mtu;
        internal NetworkInterfaceType type;
        internal OperationalStatus operStatus;
        internal uint ipv6Index;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal uint[] zoneIndices;
        internal IntPtr firstPrefix;

        /* Vista+ */
        internal UInt64 transmitLinkSpeed;
        internal UInt64 receiveLinkSpeed;
        internal IntPtr firstWinsServerAddress;
        internal IntPtr firstGatewayAddress;
        internal UInt32 ipv4Metric;
        internal UInt32 ipv6Metric;
        internal UInt64 luid;
        internal IpSocketAddress dhcpv4Server;
        internal UInt32 compartmentId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] networkGuid;
        internal InterfaceConnectionType connectionType;
        internal InterfaceTunnelType tunnelType;
        internal IpSocketAddress dhcpv6Server; // Never available in Windows.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 130)]
        internal byte[] dhcpv6ClientDuid;
        internal UInt32 dhcpv6ClientDuidLength;
        internal UInt32 dhcpV6Iaid;

        /* Windows 2008 +
              PIP_ADAPTER_DNS_SUFFIX             FirstDnsSuffix; 
         * */
    }

    internal enum InterfaceConnectionType : int {
        Dedicated = 1,
        Passive = 2,
        Demand = 3,
        Maximum = 4,
    }

    internal enum InterfaceTunnelType : int {
        None = 0,
        Other = 1,
        Direct = 2,
        SixToFour = 11,
        Isatap = 13,
        Teredo = 14,
        IpHttps = 15,
    }

    /// <summary>
    ///   IP_PER_ADAPTER_INFO - per-adapter IP information such as DNS server list.
    /// </summary>
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Ansi)]
    internal struct IpPerAdapterInfo {
        internal bool           autoconfigEnabled;
        internal bool           autoconfigActive;
        internal IntPtr         currentDnsServer; /* IpAddressList* */
        internal IpAddrString dnsServerList;
    };

    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
    internal struct MibIfRow2 // MIB_IF_ROW2
    {
        private const int GuidLength = 16;
        private const int IfMaxStringSize = 256;
        private const int IfMaxPhysAddressLength = 32;

        internal UInt64 interfaceLuid;
        internal UInt32 interfaceIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GuidLength)]
        internal byte[] interfaceGuid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IfMaxStringSize + 1)]
        internal char[] alias; // Null terminated string
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IfMaxStringSize + 1)]
        internal char[] description; // Null terminated string
        internal UInt32 physicalAddressLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IfMaxPhysAddressLength)]
        internal byte[] physicalAddress; // ANSI
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IfMaxPhysAddressLength)]
        internal byte[] permanentPhysicalAddress; // ANSI
        internal UInt32 mtu;
        internal NetworkInterfaceType type;
        internal InterfaceTunnelType tunnelType;
        internal UInt32 mediaType; // Enum
        internal UInt32 physicalMediumType; // Enum
        internal UInt32 accessType; // Enum
        internal UInt32 directionType; // Enum
        internal byte interfaceAndOperStatusFlags; // Flags Enum
        internal OperationalStatus operStatus;
        internal UInt32 adminStatus; // Enum
        internal UInt32 mediaConnectState; // Enum
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GuidLength)]
        internal byte[] networkGuid;
        internal InterfaceConnectionType connectionType;
        internal UInt64 transmitLinkSpeed;
        internal UInt64 receiveLinkSpeed;
        internal UInt64 inOctets;
        internal UInt64 inUcastPkts;
        internal UInt64 inNUcastPkts;
        internal UInt64 inDiscards;
        internal UInt64 inErrors;
        internal UInt64 inUnknownProtos;
        internal UInt64 inUcastOctets;
        internal UInt64 inMulticastOctets;
        internal UInt64 inBroadcastOctets;
        internal UInt64 outOctets;
        internal UInt64 outUcastPkts;
        internal UInt64 outNUcastPkts;
        internal UInt64 outDiscards;
        internal UInt64 outErrors;
        internal UInt64 outUcastOctets;
        internal UInt64 outMulticastOctets;
        internal UInt64 outBroadcastOctets;
        internal UInt64 outQLen;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdpStats {
        internal uint datagramsReceived;
        internal uint incomingDatagramsDiscarded;
        internal uint incomingDatagramsWithErrors;
        internal uint datagramsSent;
        internal uint udpListeners;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpStats {
        internal uint reTransmissionAlgorithm;
        internal uint minimumRetransmissionTimeOut;
        internal uint maximumRetransmissionTimeOut;
        internal uint maximumConnections;
        internal uint activeOpens;
        internal uint passiveOpens;
        internal uint failedConnectionAttempts;
        internal uint resetConnections;
        internal uint currentConnections;
        internal uint segmentsReceived;
        internal uint segmentsSent;
        internal uint segmentsResent;
        internal uint errorsReceived;
        internal uint segmentsSentWithReset;
        internal uint cumulativeConnections;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIpStats {
        internal bool forwardingEnabled;
        internal uint defaultTtl;
        internal uint packetsReceived;
        internal uint receivedPacketsWithHeaderErrors;
        internal uint receivedPacketsWithAddressErrors;
        internal uint packetsForwarded;
        internal uint receivedPacketsWithUnknownProtocols;
        internal uint receivedPacketsDiscarded;
        internal uint receivedPacketsDelivered;
        internal uint packetOutputRequests;
        internal uint outputPacketRoutingDiscards;
        internal uint outputPacketsDiscarded;
        internal uint outputPacketsWithNoRoute;
        internal uint packetReassemblyTimeout;
        internal uint packetsReassemblyRequired;
        internal uint packetsReassembled;
        internal uint packetsReassemblyFailed;
        internal uint packetsFragmented;
        internal uint packetsFragmentFailed;
        internal uint packetsFragmentCreated;
        internal uint interfaces;
        internal uint ipAddresses;
        internal uint routes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpInfo {
        internal MibIcmpStats inStats;
        internal MibIcmpStats outStats;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpStats {
        internal uint messages;
        internal uint errors;
        internal uint destinationUnreachables;
        internal uint timeExceeds;
        internal uint parameterProblems;
        internal uint sourceQuenches;
        internal uint redirects;
        internal uint echoRequests;
        internal uint echoReplies;
        internal uint timestampRequests;
        internal uint timestampReplies;
        internal uint addressMaskRequests;
        internal uint addressMaskReplies;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpInfoEx {
        internal MibIcmpStatsEx inStats;
        internal MibIcmpStatsEx outStats;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibIcmpStatsEx {
        internal uint       dwMsgs;
        internal uint       dwErrors;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=256)]
        internal uint[]      rgdwTypeCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpTable {
        internal uint numberOfEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpRow {
        internal TcpState  state;
        internal uint  localAddr;
        internal byte  localPort1;
        internal byte  localPort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte  ignoreLocalPort3;
        internal byte  ignoreLocalPort4;
        internal uint  remoteAddr;
        internal byte  remotePort1;
        internal byte  remotePort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte  ignoreRemotePort3;
        internal byte  ignoreRemotePort4;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcp6TableOwnerPid {
        internal uint numberOfEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcp6RowOwnerPid {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] localAddr;
        internal uint localScopeId;
        internal byte localPort1;
        internal byte localPort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte ignoreLocalPort3;
        internal byte ignoreLocalPort4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] remoteAddr;
        internal uint remoteScopeId;
        internal byte remotePort1;
        internal byte remotePort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte ignoreRemotePort3;
        internal byte ignoreRemotePort4;
        internal TcpState state;
        internal uint owningPid;
    }

    internal enum TcpTableClass {
        TcpTableBasicListener = 0,
        TcpTableBasicConnections = 1,
        TcpTableBasicAll = 2,
        TcpTableOwnerPidListener = 3,
        TcpTableOwnerPidConnections = 4,
        TcpTableOwnerPidAll = 5,
        TcpTableOwnerModuleListener = 6,
        TcpTableOwnerModuleConnections = 7,
        TcpTableOwnerModuleAll = 8
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdpTable {
        internal uint numberOfEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdpRow {
        internal uint  localAddr;
        internal byte  localPort1;
        internal byte  localPort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte  ignoreLocalPort3;
        internal byte  ignoreLocalPort4;
    }

    internal enum UdpTableClass {
        UdpTableBasic = 0,
        UdpTableOwnerPid = 1,
        UdpTableOwnerModule = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdp6TableOwnerPid {
        internal uint numberOfEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibUdp6RowOwnerPid {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] localAddr;
        internal uint localScopeId;
        internal byte localPort1;
        internal byte localPort2;
        // Ports are only 16 bit values (in network WORD order, 3,4,1,2).
        // There are reports where the high order bytes have garbage in them.
        internal byte ignoreLocalPort3;
        internal byte ignoreLocalPort4;
        internal uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IPOptions {
        internal byte  ttl;
        internal byte  tos;
        internal byte  flags;
        internal byte  optionsSize;
        internal IntPtr optionsData;

        internal IPOptions (PingOptions options)
        {
            ttl = 128;
            tos = 0;
            flags = 0;
            optionsSize = 0;
            optionsData = IntPtr.Zero;

            if (options != null) {
                this.ttl = (byte)options.Ttl;

                if (options.DontFragment){
                    flags = 2;
                }
            }
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct IcmpEchoReply {
        internal uint address;
        internal uint status;
        internal uint  roundTripTime;
        internal ushort dataSize;
        internal ushort reserved;
        internal IntPtr data;
        internal IPOptions options;
        }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct Ipv6Address {
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]
        internal byte[] Goo;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst=16)]
        internal byte[] Address;    // Replying address.
        internal uint ScopeID;
    }
		
    [StructLayout(LayoutKind.Sequential)]
     internal struct Icmp6EchoReply {
        internal Ipv6Address Address;
        internal uint Status;               // Reply IP_STATUS.
        internal uint RoundTripTime; // RTT in milliseconds.
        internal IntPtr data;
        // internal IPOptions options;
        // internal IntPtr data; data os after tjos
     }

    internal delegate void StableUnicastIpAddressTableDelegate(IntPtr context, IntPtr table);

    /// <summary>
    ///   Wrapper for API's in iphlpapi.dll
    /// </summary>

    [
    System.Security.SuppressUnmanagedCodeSecurityAttribute()
    ]
    internal static class UnsafeNetInfoNativeMethods {

        private const string IPHLPAPI = "iphlpapi.dll";

        [DllImport(IPHLPAPI)]
        internal extern static uint GetAdaptersAddresses(
            AddressFamily family,
            uint flags,
            IntPtr pReserved,
            SafeLocalFree adapterAddresses,
            ref uint outBufLen);
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", 
            "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", 
            Justification = "This operation is Read-Only and does not access restricted information")]
        [DllImport(IPHLPAPI)]
        internal extern static uint GetBestInterfaceEx(byte[] ipAddress, out int index);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetIfEntry2(ref MibIfRow2 pIfRow);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetIpStatisticsEx(out MibIpStats statistics, AddressFamily family);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetTcpStatisticsEx(out MibTcpStats statistics, AddressFamily family);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetUdpStatisticsEx(out MibUdpStats statistics, AddressFamily family);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetIcmpStatistics(out MibIcmpInfo statistics);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetIcmpStatisticsEx(out MibIcmpInfoEx statistics,AddressFamily family);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetTcpTable(SafeLocalFree pTcpTable, ref uint dwOutBufLen, bool order);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetExtendedTcpTable(SafeLocalFree pTcpTable, ref uint dwOutBufLen, bool order,
                                                        uint IPVersion, TcpTableClass tableClass, uint reserved);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetUdpTable(SafeLocalFree pUdpTable, ref uint dwOutBufLen, bool order);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetExtendedUdpTable(SafeLocalFree pUdpTable, ref uint dwOutBufLen, bool order,
                                                        uint IPVersion, UdpTableClass tableClass, uint reserved);
        [DllImport(IPHLPAPI)]
        internal extern static uint GetNetworkParams(SafeLocalFree pFixedInfo,ref uint pOutBufLen);

        [DllImport(IPHLPAPI)]
        internal extern static uint GetPerAdapterInfo(uint IfIndex,SafeLocalFree pPerAdapterInfo,ref uint pOutBufLen);

        [DllImport(IPHLPAPI, SetLastError=true)]
        internal extern static SafeCloseIcmpHandle IcmpCreateFile();

        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static SafeCloseIcmpHandle Icmp6CreateFile ();

        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static bool IcmpCloseHandle(IntPtr handle);

        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static uint IcmpSendEcho2 (SafeCloseIcmpHandle icmpHandle, SafeWaitHandle Event, IntPtr apcRoutine, IntPtr apcContext,
            uint ipAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);

        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static uint IcmpSendEcho2 (SafeCloseIcmpHandle icmpHandle, IntPtr Event, IntPtr apcRoutine, IntPtr apcContext,
            uint ipAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        
        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static uint Icmp6SendEcho2 (SafeCloseIcmpHandle icmpHandle, SafeWaitHandle Event, IntPtr apcRoutine, IntPtr apcContext,
            byte[] sourceSocketAddress, byte[] destSocketAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);

        [DllImport (IPHLPAPI, SetLastError=true)]
        internal extern static uint Icmp6SendEcho2 (SafeCloseIcmpHandle icmpHandle, IntPtr Event, IntPtr apcRoutine, IntPtr apcContext,
            byte[] sourceSocketAddress, byte[] destSocketAddress, [In] SafeLocalFree data, ushort dataSize, ref IPOptions options, SafeLocalFree replyBuffer, uint replySize, uint timeout);
        
        [DllImport(IPHLPAPI)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern void FreeMibTable(IntPtr handle);

        [DllImport(IPHLPAPI)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern uint CancelMibChangeNotify2(IntPtr notificationHandle);

        [DllImport(IPHLPAPI)]
        internal static extern uint NotifyStableUnicastIpAddressTable(
            [In] AddressFamily addressFamily,
            [Out] out SafeFreeMibTable table,
            [MarshalAs(UnmanagedType.FunctionPtr)][In] StableUnicastIpAddressTableDelegate callback,
            [In] IntPtr context,
            [Out] out SafeCancelMibChangeNotify notificationHandle);
    }
}



