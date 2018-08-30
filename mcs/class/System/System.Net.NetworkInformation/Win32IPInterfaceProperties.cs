//
// System.Net.NetworkInformation.IPInterfaceProperties
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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
#if WIN_PLATFORM
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	class Win32IPInterfaceProperties2 : IPInterfaceProperties
	{
		readonly Win32_IP_ADAPTER_ADDRESSES addr;
		readonly Win32_MIB_IFROW mib4, mib6;

		public Win32IPInterfaceProperties2 (Win32_IP_ADAPTER_ADDRESSES addr, Win32_MIB_IFROW mib4, Win32_MIB_IFROW mib6)
		{
			this.addr = addr;
			this.mib4 = mib4;
			this.mib6 = mib6;
		}

		public override IPv4InterfaceProperties GetIPv4Properties ()
		{
			return new Win32IPv4InterfaceProperties (addr, mib4);
		}

		public override IPv6InterfaceProperties GetIPv6Properties ()
		{
			return new Win32IPv6InterfaceProperties (mib6);
		}

		public override IPAddressInformationCollection AnycastAddresses {
			get { return Win32FromAnycast (addr.FirstAnycastAddress); }
		}

		static IPAddressInformationCollection Win32FromAnycast (IntPtr ptr)
		{
			var c = new IPAddressInformationCollection ();
			Win32_IP_ADAPTER_ANYCAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_ANYCAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_ANYCAST_ADDRESS));
				c.InternalAdd (new SystemIPAddressInformation (
				       a.Address.GetIPAddress (),
				       a.LengthFlags.IsDnsEligible,
				       a.LengthFlags.IsTransient));
			}
			return c;
		}

		public override IPAddressCollection DhcpServerAddresses {
			get {
				// FIXME: should ipv6 DhcpServer be considered?
				try {
					return Win32IPAddressCollection.FromSocketAddress (addr.Dhcpv4Server);
				} catch (IndexOutOfRangeException) {
					return Win32IPAddressCollection.Empty;
				}
			}
		}

		public override IPAddressCollection DnsAddresses {
			get { return Win32IPAddressCollection.FromDnsServer (addr.FirstDnsServerAddress); }
		}

		public override string DnsSuffix {
			get { return addr.DnsSuffix; }
		}

		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				var col = new GatewayIPAddressInformationCollection ();
				try {
					// FIXME: should ipv6 DhcpServer be considered?
					Win32_IP_ADAPTER_GATEWAY_ADDRESS a;
					for (IntPtr p = addr.FirstGatewayAddress; p != IntPtr.Zero; p = a.Next) {
						a = (Win32_IP_ADAPTER_GATEWAY_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_GATEWAY_ADDRESS));
						col.InternalAdd (new SystemGatewayIPAddressInformation (a.Address.GetIPAddress ()));
					}
				} catch (IndexOutOfRangeException) {}
				return col;
			}
		}

		public override bool IsDnsEnabled {
			get { return Win32NetworkInterface.FixedInfo.EnableDns != 0; }
		}

		public override bool IsDynamicDnsEnabled {
			get { return addr.DdnsEnabled; }
		}

		public override MulticastIPAddressInformationCollection MulticastAddresses {
			get { return Win32FromMulticast (addr.FirstMulticastAddress); }
		}

		static MulticastIPAddressInformationCollection Win32FromMulticast (IntPtr ptr)
		{
			var c = new MulticastIPAddressInformationCollection ();
			Win32_IP_ADAPTER_MULTICAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_MULTICAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_MULTICAST_ADDRESS));
				c.InternalAdd (new SystemMulticastIPAddressInformation (new SystemIPAddressInformation (
				       a.Address.GetIPAddress (),
				       a.LengthFlags.IsDnsEligible,
				       a.LengthFlags.IsTransient)));
			}
			return c;
		}

		public override UnicastIPAddressInformationCollection UnicastAddresses {
			get {
				try {
					// FIXME: should ipv6 DhcpServer be considered?
					return Win32FromUnicast (addr.FirstUnicastAddress);
				} catch (IndexOutOfRangeException) {
					return new UnicastIPAddressInformationCollection ();
				}
			}
		}

		static UnicastIPAddressInformationCollection Win32FromUnicast (IntPtr ptr)
		{
			UnicastIPAddressInformationCollection c = new UnicastIPAddressInformationCollection ();
			Win32_IP_ADAPTER_UNICAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_UNICAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_UNICAST_ADDRESS));
				c.InternalAdd (new Win32UnicastIPAddressInformation (a));
			}
			return c;
		}

		public override IPAddressCollection WinsServersAddresses {
			get {
				try {
					return Win32IPAddressCollection.FromWinsServer (addr.FirstWinsServerAddress);
				} catch (IndexOutOfRangeException) {
					return Win32IPAddressCollection.Empty;
				}
			}
		}
	}
}
#endif
