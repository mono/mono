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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	abstract class UnixIPInterfaceProperties : IPInterfaceProperties
	{
		protected IPv4InterfaceProperties ipv4iface_properties;
		protected UnixNetworkInterface iface;
		List <IPAddress> addresses;
		IPAddressCollection dns_servers;
		
		public UnixIPInterfaceProperties (UnixNetworkInterface iface, List <IPAddress> addresses)
		{
			this.iface = iface;
			this.addresses = addresses;
		}

		public override IPv6InterfaceProperties GetIPv6Properties ()
		{
			throw new NotImplementedException ();
		}
#if MONODROID
		[DllImport ("__Internal")]
		static extern int _monodroid_get_dns_servers (out IntPtr dns_servers_array);

		void GetDNSServersFromOS ()
		{
			IntPtr dsa;
			int len = _monodroid_get_dns_servers (out dsa);
			if (len <= 0)
				return;

			var servers = new IntPtr [len];
			Marshal.Copy (dsa, servers, 0, len);

			dns_servers = new IPAddressCollection ();
			foreach (IntPtr s in servers) {
				string server_ip = Marshal.PtrToStringAnsi (s);
				Marshal.FreeHGlobal (s);

				IPAddress addr;
				if (!IPAddress.TryParse (server_ip, out addr))
					continue;
				dns_servers.InternalAdd (addr);
			}
			Marshal.FreeHGlobal (dsa);
		}
#else
		static Regex ns = new Regex (@"\s*nameserver\s+(?<address>.*)");
		static Regex search = new Regex (@"\s*search\s+(?<domain>.*)");

		string dns_suffix;
		DateTime last_parse;

		void ParseResolvConf ()
		{
			try {
				DateTime wt = File.GetLastWriteTime ("/etc/resolv.conf");
				if (wt <= last_parse)
					return;

				last_parse = wt;
				dns_suffix = "";
				dns_servers = new IPAddressCollection ();
				using (StreamReader reader = new StreamReader ("/etc/resolv.conf")) {
					string str;
					string line;
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						if (line.Length == 0 || line [0] == '#')
							continue;
						Match match = ns.Match (line);
						if (match.Success) {
							try {
								str = match.Groups ["address"].Value;
								str = str.Trim ();
								dns_servers.InternalAdd (IPAddress.Parse (str));
							} catch {
							}
						} else {
							match = search.Match (line);
							if (match.Success) {
								str = match.Groups ["domain"].Value;
								string [] parts = str.Split (',');
								dns_suffix = parts [0].Trim ();
							}
						}
					}
				}
			} catch {
			}
		}
#endif
		public override IPAddressInformationCollection AnycastAddresses {
			get {
				var c = new IPAddressInformationCollection ();
				foreach (IPAddress address in addresses) {
					c.InternalAdd (new SystemIPAddressInformation (address, false, false));
				}
				return c;
			}
		}

		[MonoTODO ("Always returns an empty collection.")]
		public override IPAddressCollection DhcpServerAddresses {
			get {
				// There are lots of different DHCP clients
				// that all store their configuration differently.
				// I'm not sure what to do here.
				IPAddressCollection coll = new IPAddressCollection ();
				return coll;
			}
		}

		public override IPAddressCollection DnsAddresses {
			get {
#if MONODROID
				GetDNSServersFromOS ();
#else
				ParseResolvConf ();
#endif
				return dns_servers;
			}
		}

		public override string DnsSuffix {
			get {
#if MONODROID
				return String.Empty;
#else
				ParseResolvConf ();
				return dns_suffix;
#endif
			}
		}

		[MonoTODO ("Always returns true")]
		public override bool IsDnsEnabled {
			get {
				return true;
			}
		}

		[MonoTODO ("Always returns false")]
		public override bool IsDynamicDnsEnabled {
			get {
				return false;
			}
		}

		public override MulticastIPAddressInformationCollection MulticastAddresses {
			get {
				var multicastAddresses = new MulticastIPAddressInformationCollection ();
				foreach (IPAddress address in addresses) {
					byte[] addressBytes = address.GetAddressBytes ();
					if (addressBytes[0] >= 224 && addressBytes[0] <= 239) {
						multicastAddresses.InternalAdd (new SystemMulticastIPAddressInformation (new SystemIPAddressInformation (address, true, false)));
					}
				}
				return multicastAddresses;
			}
		}

		public override UnicastIPAddressInformationCollection UnicastAddresses {
			get {
				var unicastAddresses = new UnicastIPAddressInformationCollection ();
				foreach (IPAddress address in addresses) {
					switch (address.AddressFamily) {
						case AddressFamily.InterNetwork:
							byte top = address.GetAddressBytes () [0];
							if (top >= 224 && top <= 239)
								continue;
							unicastAddresses.InternalAdd (new LinuxUnicastIPAddressInformation (address));
							break;

						case AddressFamily.InterNetworkV6:
							if (address.IsIPv6Multicast)
								continue;
							unicastAddresses.InternalAdd (new LinuxUnicastIPAddressInformation (address));
							break;
					}
				}
				return unicastAddresses;
			}
		}

		[MonoTODO ("Always returns an empty collection.")]
		public override IPAddressCollection WinsServersAddresses {
			get {
				// I do SUPPOSE we could scrape /etc/samba/smb.conf, but.. yeesh.
				return new IPAddressCollection ();
			}
		}
	}

	class LinuxIPInterfaceProperties : UnixIPInterfaceProperties
	{
		public LinuxIPInterfaceProperties (LinuxNetworkInterface iface, List <IPAddress> addresses)
			: base (iface, addresses)
		{
		}

		public override IPv4InterfaceProperties GetIPv4Properties ()
		{
			if (ipv4iface_properties == null)
				ipv4iface_properties = new LinuxIPv4InterfaceProperties (iface as LinuxNetworkInterface);
			
			return ipv4iface_properties;
		}

		IPAddressCollection ParseRouteInfo (string iface)
		{
			var col = new IPAddressCollection ();
			try {
				using (StreamReader reader = new StreamReader ("/proc/net/route")) {
					string line;
					reader.ReadLine (); // Ignore first line
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						if (line.Length == 0)
							continue;

						string [] parts = line.Split ('\t');
						if (parts.Length < 3)
							continue;
						string gw_address = parts [2].Trim ();
						byte [] ipbytes = new byte [4];
						if (gw_address.Length == 8 && iface.Equals (parts [0], StringComparison.OrdinalIgnoreCase)) {
							for (int i = 0; i < 4; i++) {
								if (!Byte.TryParse (gw_address.Substring (i * 2, 2), NumberStyles.HexNumber, null, out ipbytes [3 - i]))
									continue;
							}
							IPAddress ip = new IPAddress (ipbytes);
							if (!ip.Equals (IPAddress.Any) && !col.Contains (ip))
								col.InternalAdd (ip);
						}
					}
				}
			} catch {
			}

			return col;
		}

		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				return SystemGatewayIPAddressInformation.ToGatewayIpAddressInformationCollection (ParseRouteInfo (this.iface.Name.ToString()));
			}
		}
	}

	class MacOsIPInterfaceProperties : UnixIPInterfaceProperties
	{
		public MacOsIPInterfaceProperties (MacOsNetworkInterface iface, List <IPAddress> addresses)
			: base (iface, addresses)
		{
		}

		public override IPv4InterfaceProperties GetIPv4Properties ()
		{
			if (ipv4iface_properties == null)
				ipv4iface_properties = new MacOsIPv4InterfaceProperties (iface as MacOsNetworkInterface);
			
			return ipv4iface_properties;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool ParseRouteInfo_internal(string iface, out string[] gw_addr_list);

		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				var gateways = new IPAddressCollection ();
				string[] gw_addrlist;
				if (!ParseRouteInfo_internal (this.iface.Name.ToString(), out gw_addrlist))
					return new GatewayIPAddressInformationCollection ();

				for(int i=0; i<gw_addrlist.Length; i++) {
					try {
						IPAddress ip = IPAddress.Parse(gw_addrlist[i]);
						if (!ip.Equals (IPAddress.Any) && !gateways.Contains (ip))
							gateways.InternalAdd (ip);
					} catch (ArgumentNullException) {
						/* Ignore this, as the
						 * internal call might have
						 * left some blank entries at
						 * the end of the array
						 */
					}
				}

				return SystemGatewayIPAddressInformation.ToGatewayIpAddressInformationCollection (gateways);
			}
		}
	}

#if WIN_PLATFORM
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
#endif

}


