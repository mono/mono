//
// System.Net.NetworkInformation.NetworkInterface
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation
{


	class Win32NetworkInterface {
				// Can't have unresolvable pinvokes on ios
#if WIN_PLATFORM
		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetNetworkParams (IntPtr ptr, ref int size);
#endif

		static Win32_FIXED_INFO fixedInfo;
		static bool initialized = false;

		public static Win32_FIXED_INFO FixedInfo {
			get {
				if (!initialized) {
#if WIN_PLATFORM
					int len = 0;
					IntPtr ptr = IntPtr.Zero;
					GetNetworkParams (ptr, ref len);
					ptr = Marshal.AllocHGlobal(len);
					GetNetworkParams (ptr, ref len);
					fixedInfo = Marshal.PtrToStructure<Win32_FIXED_INFO> (ptr);
#else
					throw new NotImplementedException ();
#endif
					initialized = true;
				}
				return fixedInfo;
			}
		}
	}

	// They are mostly defined in iptypes.h (included by iphlpapi.h).
	// grep around /usr/include/w32api/* for identifiers you are curious.

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_FIXED_INFO
	{

		const int MAX_HOSTNAME_LEN = 128;
		const int MAX_DOMAIN_NAME_LEN = 128;
		const int MAX_SCOPE_ID_LEN = 256;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = MAX_HOSTNAME_LEN + 4)]
		public string HostName;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = MAX_DOMAIN_NAME_LEN + 4)]
		public string DomainName;
		public IntPtr CurrentDnsServer; // to Win32IP_ADDR_STRING
		public Win32_IP_ADDR_STRING DnsServerList;
		public NetBiosNodeType NodeType;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = MAX_SCOPE_ID_LEN + 4)]
		public string ScopeId;
		public uint EnableRouting;
		public uint EnableProxy;
		public uint EnableDns;
	}

	[StructLayout (LayoutKind.Explicit)]
	struct AlignmentUnion
	{
		[FieldOffset (0)] // 1
		public ulong Alignment;
		[FieldOffset (0)] // 2-1
		public int Length;
		[FieldOffset (4)] // 2-2
		public int IfIndex;
	}

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct Win32_IP_ADAPTER_ADDRESSES {
		public AlignmentUnion Alignment;
		public IntPtr Next; // to Win32_IP_ADAPTER_ADDRESSES
		[MarshalAs (UnmanagedType.LPStr)]
		public string AdapterName; // PCHAR
		public IntPtr FirstUnicastAddress; //to IP_ADAPTER_UNICAST_ADDRESS
		public IntPtr FirstAnycastAddress; // to IP_ADAPTER_ANYCAST_ADDRESS
		public IntPtr FirstMulticastAddress; // to IP_ADAPTER_MULTICAST_ADDRESS
		public IntPtr FirstDnsServerAddress; // to IP_ADAPTER_DNS_SERVER_ADDRESS
		public string DnsSuffix;
		public string Description;
		public string FriendlyName;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
		public byte [] PhysicalAddress;
		public uint PhysicalAddressLength;
		public uint Flags;
		public uint Mtu;
		public NetworkInterfaceType IfType;
		public OperationalStatus OperStatus;
		public int Ipv6IfIndex;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 16)]
		public uint [] ZoneIndices;
		public IntPtr FirstPrefix; // to PIP_ADAPTER_PREFIX
		public UInt64 TransmitLinkSpeed;
		public UInt64 ReceiveLinkSpeed;
		public IntPtr FirstWinsServerAddress; // to PIP_ADAPTER_WINS_SERVER_ADDRESS_LH
		public IntPtr FirstGatewayAddress; // to PIP_ADAPTER_GATEWAY_ADDRESS_LH
		public uint Ipv4Metric;
		public uint Ipv6Metric;
		public UInt64 Luid;
		public Win32_SOCKET_ADDRESS Dhcpv4Server;
		public uint CompartmentId;
		public UInt64 NetworkGuid;
		public int ConnectionType;
		public int TunnelType;
		public Win32_SOCKET_ADDRESS Dhcpv6Server;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAX_DHCPV6_DUID_LENGTH)]
		public byte [] Dhcpv6ClientDuid;
		public ulong Dhcpv6ClientDuidLength;
		public ulong Dhcpv6Iaid;
		public IntPtr FirstDnsSuffix; // to PIP_ADAPTER_DNS_SUFFIX

		//Flags For GetAdapterAddresses
		public const int GAA_FLAG_INCLUDE_WINS_INFO = 0x0040;
		public const int GAA_FLAG_INCLUDE_GATEWAYS = 0x0080;

		const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
		const int MAX_DHCPV6_DUID_LENGTH = 130;

		const int IP_ADAPTER_DDNS_ENABLED = 1;
		const int IP_ADAPTER_DHCP_ENABLED = 4;
		const int IP_ADAPTER_RECEIVE_ONLY = 8;
		const int IP_ADAPTER_NO_MULTICAST = 0x10;

		public bool DdnsEnabled {
			get { return (Flags & IP_ADAPTER_DDNS_ENABLED) != 0; }
		}

		public bool DhcpEnabled {
			get { return (Flags & IP_ADAPTER_DHCP_ENABLED) != 0; }
		}

		public bool IsReceiveOnly {
			get { return (Flags & IP_ADAPTER_RECEIVE_ONLY) != 0; }
		}

		public bool NoMulticast {
			get { return (Flags & IP_ADAPTER_NO_MULTICAST) != 0; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_INFO
	{
		const int MAX_ADAPTER_NAME_LENGTH = 256;
		const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
		const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

		public IntPtr Next; // to Win32_IP_ADAPTER_INFO
		public int ComboIndex;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
		public string AdapterName;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
		public string Description;
		public uint AddressLength;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
		public byte [] Address;
		public uint Index;
		public uint Type;
		public uint DhcpEnabled;
		public IntPtr CurrentIpAddress; // Win32_IP_ADDR_STRING
		public Win32_IP_ADDR_STRING IpAddressList;
		public Win32_IP_ADDR_STRING GatewayList;
		public Win32_IP_ADDR_STRING DhcpServer;
		public bool HaveWins;
		public Win32_IP_ADDR_STRING PrimaryWinsServer;
		public Win32_IP_ADDR_STRING SecondaryWinsServer;
		public long LeaseObtained;
		public long LeaseExpires;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_MIB_IFROW
	{
		const int MAX_INTERFACE_NAME_LEN = 256;
		const int MAXLEN_PHYSADDR = 8;
		const int MAXLEN_IFDESCR = 256;

		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAX_INTERFACE_NAME_LEN * 2)]
		public char [] Name;
		public int Index;
		public NetworkInterfaceType Type;
		public int Mtu;
		public uint Speed;
		public int PhysAddrLen;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAXLEN_PHYSADDR)]
		public byte [] PhysAddr;
		public uint AdminStatus;
		public uint OperStatus;
		public uint LastChange;
		public int InOctets;
		public int InUcastPkts;
		public int InNUcastPkts;
		public int InDiscards;
		public int InErrors;
		public int InUnknownProtos;
		public int OutOctets;
		public int OutUcastPkts;
		public int OutNUcastPkts;
		public int OutDiscards;
		public int OutErrors;
		public int OutQLen;
		public int DescrLen;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = MAXLEN_IFDESCR)]
		public byte [] Descr;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADDR_STRING
	{
		public IntPtr Next; // to Win32_IP_ADDR_STRING
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 16)]
		public string IpAddress;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 16)]
		public string IpMask;
		public uint Context;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32LengthFlagsUnion
	{
		const int IP_ADAPTER_ADDRESS_DNS_ELIGIBLE = 1;
		const int IP_ADAPTER_ADDRESS_TRANSIENT = 2;

		// union { struct {
		public uint Length;
		public uint Flags;
		// }; };

		public bool IsDnsEligible {
			get { return (Flags & IP_ADAPTER_ADDRESS_DNS_ELIGIBLE) != 0; }
		}

		public bool IsTransient {
			get { return (Flags & IP_ADAPTER_ADDRESS_TRANSIENT) != 0; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_ANYCAST_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_ANYCAST_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_DNS_SERVER_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_DNS_SERVER_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_MULTICAST_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_MULTICAST_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_GATEWAY_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_GATEWAY_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_WINS_SERVER_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_WINS_SERVER_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_IP_ADAPTER_UNICAST_ADDRESS
	{
		public Win32LengthFlagsUnion LengthFlags;
		public IntPtr Next; // to Win32_IP_ADAPTER_UNICAST_ADDRESS
		public Win32_SOCKET_ADDRESS Address;
		public PrefixOrigin PrefixOrigin;
		public SuffixOrigin SuffixOrigin;
		public DuplicateAddressDetectionState DadState;
		public uint ValidLifetime;
		public uint PreferredLifetime;
		public uint LeaseLifetime;
		public byte OnLinkPrefixLength;

	}

	[StructLayout (LayoutKind.Sequential)]
	struct Win32_SOCKADDR
	{
		public ushort AddressFamily;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 14 * 2)]
		public byte [] AddressData;
	}

	// FIXME: it somehow fails to marshal.
	[StructLayout (LayoutKind.Sequential)]
	struct Win32_SOCKET_ADDRESS
	{
		public IntPtr Sockaddr; // to Win32_SOCKADDR
		public int SockaddrLength;

		public IPAddress GetIPAddress ()
		{
			Win32_SOCKADDR sa = (Win32_SOCKADDR) Marshal.PtrToStructure (Sockaddr, typeof (Win32_SOCKADDR));
//foreach (byte b in sa.AddressData) Console.Write ("{0:X02}", b); Console.WriteLine ();
			byte [] arr;
			if (sa.AddressFamily == AF_INET6) {
				arr = new byte [16];
				Array.Copy (sa.AddressData, 6, arr, 0, 16);
			} else {
				arr = new byte [4];
				Array.Copy (sa.AddressData, 2, arr, 0, 4);
			}
//foreach (byte b in arr) Console.Write ("{0:X02}", b); Console.WriteLine ();
			return new IPAddress (arr);
		}

		const int AF_INET6 = 23;
	}
}

