//
// System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//	Eric Butler (eric@extremeboredom.net)
//
// Copyright (c) 2008 Eric Butler
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
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	[StructLayout(LayoutKind.Explicit)]
	struct ifa_ifu
	{ 		
		[FieldOffset (0)]
		public IntPtr ifu_broadaddr; 

		[FieldOffset (0)]
		public IntPtr ifu_dstaddr; 
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ifaddrs
	{
		public IntPtr  ifa_next;
		public string  ifa_name;
		public uint    ifa_flags;
		public IntPtr  ifa_addr;
		public IntPtr  ifa_netmask;
		public ifa_ifu ifa_ifu;
		public IntPtr  ifa_data;
	}

	[StructLayout(LayoutKind.Sequential)]	
	struct sockaddr_in
	{
		public ushort sin_family;
		public ushort sin_port;
		public uint   sin_addr;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct sockaddr_in6
	{
		public ushort   sin6_family;   /* AF_INET6 */
		public ushort   sin6_port;     /* Transport layer port # */
		public uint     sin6_flowinfo; /* IPv6 flow information */
		public in6_addr sin6_addr;     /* IPv6 address */
		public uint     sin6_scope_id; /* scope id (new in RFC2553) */
	}

	[StructLayout(LayoutKind.Sequential)]
	struct in6_addr
	{
		[MarshalAs (UnmanagedType.ByValArray, SizeConst=16)]
		public byte[] u6_addr8;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct sockaddr_ll
	{
		public ushort sll_family;
		public ushort sll_protocol;
		public int    sll_ifindex;
		public ushort sll_hatype;
		public byte   sll_pkttype;
		public byte   sll_halen;
#if MONODROID
		// In MonoDroid the structure has larger space allocated for the address part since there exist
		// addresses (Infiniband, ipv6 tunnels) which exceed the standard 8 bytes. In fact, glibc's
		// getifaddrs implementation also uses the bigger size, but for compatibility with other libc
		// implementations we use the standard address size
		[MarshalAs (UnmanagedType.ByValArray, SizeConst=24)]
#else
		[MarshalAs (UnmanagedType.ByValArray, SizeConst=8)]
#endif
		public byte[] sll_addr;
	}

	enum LinuxArpHardware {
		ETHER = 1,
		EETHER = 2,
		PRONET = 4,
		ATM = 19,
		SLIP = 256,
		CSLIP = 257,
		SLIP6 = 258,
		CSLIP6 = 259,
		PPP = 512,
		LOOPBACK = 772,
		FDDI = 774,
		TUNNEL = 768,
		TUNNEL6 = 769,
		SIT = 776, // IPv6-in-IPv4 tunnel
		IPDDP = 777, // IP over DDP tunnel
		IPGRE = 778, // GRE over IP
		IP6GRE = 823 // GRE over IPv6
	}
}

