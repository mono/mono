//
// System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//      Eric Butler (eric@extremeboredom.net)
//      Marek Habersack (mhabersack@novell.com)
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2006-2008 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	internal class MacOsNetworkInterfaceAPI : UnixNetworkInterfaceAPI
	{
		const int AF_INET = 2;
		const int AF_LINK = 18;
		protected readonly int AF_INET6;

		public MacOsNetworkInterfaceAPI ()
		{
			AF_INET6 = 30;
		}

		protected MacOsNetworkInterfaceAPI (int AF_INET6)
		{
			this.AF_INET6 = AF_INET6;
		}

		public override NetworkInterface [] GetAllNetworkInterfaces ()
		{
			var interfaces = new Dictionary <string, MacOsNetworkInterface> ();
			IntPtr ifap;
			if (getifaddrs (out ifap) != 0)
				throw new SystemException ("getifaddrs() failed");

			try {
				IntPtr next = ifap;
				while (next != IntPtr.Zero) {
					MacOsStructs.ifaddrs addr = (MacOsStructs.ifaddrs) Marshal.PtrToStructure (next, typeof (MacOsStructs.ifaddrs));
					IPAddress address = IPAddress.None;
					string    name = addr.ifa_name;
					int       index = -1;
					byte[]    macAddress = null;
					NetworkInterfaceType type = NetworkInterfaceType.Unknown;

					if (addr.ifa_addr != IntPtr.Zero) {
						// optain IPAddress
						MacOsStructs.sockaddr sockaddr = (MacOsStructs.sockaddr) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr));

						if (sockaddr.sa_family == AF_INET6) {
							MacOsStructs.sockaddr_in6 sockaddr6 = (MacOsStructs.sockaddr_in6) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in6));
							address = new IPAddress (sockaddr6.sin6_addr.u6_addr8, sockaddr6.sin6_scope_id);
						} else if (sockaddr.sa_family == AF_INET) {
							MacOsStructs.sockaddr_in sockaddrin = (MacOsStructs.sockaddr_in) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in));
							address = new IPAddress (sockaddrin.sin_addr);
						} else if (sockaddr.sa_family == AF_LINK) {
							MacOsStructs.sockaddr_dl sockaddrdl = new MacOsStructs.sockaddr_dl ();
							sockaddrdl.Read (addr.ifa_addr);

							macAddress = new byte [(int) sockaddrdl.sdl_alen];
							// copy mac address from sdl_data field starting at last index pos of interface name into array macaddress, starting
							// at index 0
							Array.Copy (sockaddrdl.sdl_data, sockaddrdl.sdl_nlen, macAddress, 0, Math.Min (macAddress.Length, sockaddrdl.sdl_data.Length - sockaddrdl.sdl_nlen));

							index = sockaddrdl.sdl_index;

							int hwtype = (int) sockaddrdl.sdl_type;
							if (Enum.IsDefined (typeof (MacOsArpHardware), hwtype)) {
								switch ((MacOsArpHardware) hwtype) {
									case MacOsArpHardware.ETHER:
										type = NetworkInterfaceType.Ethernet;
										break;

									case MacOsArpHardware.ATM:
										type = NetworkInterfaceType.Atm;
										break;

									case MacOsArpHardware.SLIP:
										type = NetworkInterfaceType.Slip;
										break;

									case MacOsArpHardware.PPP:
										type = NetworkInterfaceType.Ppp;
										break;

									case MacOsArpHardware.LOOPBACK:
										type = NetworkInterfaceType.Loopback;
										macAddress = null;
										break;

									case MacOsArpHardware.FDDI:
										type = NetworkInterfaceType.Fddi;
										break;
								}
							}
						}
					}

					MacOsNetworkInterface iface = null;

					// create interface if not already present
					if (!interfaces.TryGetValue (name, out iface)) {
						iface = new MacOsNetworkInterface (name, addr.ifa_flags);
						interfaces.Add (name, iface);
					}

					// if a new address has been found, add it
					if (!address.Equals (IPAddress.None))
						iface.AddAddress (address);

					// set link layer info, if iface has macaddress or is loopback device
					if (macAddress != null || type == NetworkInterfaceType.Loopback)
						iface.SetLinkLayerInfo (index, macAddress, type);

					next = addr.ifa_next;
				}
			} finally {
				freeifaddrs (ifap);
			}

			NetworkInterface [] result = new NetworkInterface [interfaces.Count];
			int x = 0;
			foreach (NetworkInterface thisInterface in interfaces.Values) {
				result [x] = thisInterface;
				x++;
			}
			return result;
		}

		public override int GetLoopbackInterfaceIndex ()
		{
			return if_nametoindex ("lo0");
		}

		public override IPAddress GetNetMask (IPAddress address)
		{
			IntPtr ifap;
			if (getifaddrs (out ifap) != 0)
				throw new SystemException ("getifaddrs() failed");

			try {
				IntPtr next = ifap;
				while (next != IntPtr.Zero) {
					MacOsStructs.ifaddrs addr = (MacOsStructs.ifaddrs) Marshal.PtrToStructure (next, typeof (MacOsStructs.ifaddrs));

					if (addr.ifa_addr != IntPtr.Zero) {
						// optain IPAddress
						MacOsStructs.sockaddr sockaddr = (MacOsStructs.sockaddr) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr));

						if (sockaddr.sa_family == AF_INET) {
							MacOsStructs.sockaddr_in sockaddrin = (MacOsStructs.sockaddr_in) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in));
							var saddress = new IPAddress (sockaddrin.sin_addr);
							if (address.Equals (saddress))
								return new IPAddress(((sockaddr_in)Marshal.PtrToStructure(addr.ifa_netmask, typeof(sockaddr_in))).sin_addr);
						}
					}
					next = addr.ifa_next;
				}
			} finally {
				freeifaddrs (ifap);
			}

			return null;
		}
	}

	sealed class MacOsNetworkInterface : UnixNetworkInterface
	{
		private uint _ifa_flags;

		internal MacOsNetworkInterface (string name, uint ifa_flags)
			: base (name)
		{
			_ifa_flags = ifa_flags;
		}

		public override IPInterfaceProperties GetIPProperties ()
		{
			if (ipproperties == null)
				ipproperties = new MacOsIPInterfaceProperties (this, addresses);
			return ipproperties;
		}

		public override IPv4InterfaceStatistics GetIPv4Statistics ()
		{
			if (ipv4stats == null)
				ipv4stats = new MacOsIPv4InterfaceStatistics (this);
			return ipv4stats;
		}

		public override OperationalStatus OperationalStatus {
			get {
				if(((MacOsInterfaceFlags)_ifa_flags & MacOsInterfaceFlags.IFF_UP) == MacOsInterfaceFlags.IFF_UP){
					return OperationalStatus.Up;
				}
				return OperationalStatus.Unknown;
			}
		}

		public override bool SupportsMulticast {
			get {
				return ((MacOsInterfaceFlags)_ifa_flags & MacOsInterfaceFlags.IFF_MULTICAST) == MacOsInterfaceFlags.IFF_MULTICAST;
			}
		}
	}
}
