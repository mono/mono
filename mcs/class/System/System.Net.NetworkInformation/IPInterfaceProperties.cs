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
#if NET_2_0
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace System.Net.NetworkInformation {
	public abstract class IPInterfaceProperties {
		protected IPInterfaceProperties ()
		{
		}

		public abstract IPv4InterfaceProperties GetIPv4Properties ();
		public abstract IPv6InterfaceProperties GetIPv6Properties ();

		public abstract IPAddressInformationCollection AnycastAddresses { get; }
		public abstract IPAddressCollection DhcpServerAddresses { get; }
		public abstract IPAddressCollection DnsAddresses { get; }
		public abstract string DnsSuffix { get; }
		public abstract GatewayIPAddressInformationCollection GatewayAddresses { get; }
		public abstract bool IsDnsEnabled { get; }
		public abstract bool IsDynamicDnsEnabled { get; }
		public abstract MulticastIPAddressInformationCollection MulticastAddresses { get; }
		public abstract UnicastIPAddressInformationCollection UnicastAddresses { get; }
		public abstract IPAddressCollection WinsServersAddresses { get; }
	}

	abstract class UnixIPInterfaceProperties : IPInterfaceProperties
	{
		protected IPv4InterfaceProperties ipv4iface_properties;
		protected UnixNetworkInterface iface;
		List <IPAddress> addresses;
		IPAddressCollection dns_servers;
		IPAddressCollection gateways;
		string dns_suffix;
		DateTime last_parse;
		
		public UnixIPInterfaceProperties (UnixNetworkInterface iface, List <IPAddress> addresses)
		{
			this.iface = iface;
			this.addresses = addresses;
		}

		public override IPv6InterfaceProperties GetIPv6Properties ()
		{
			throw new NotImplementedException ();
		}

		void ParseRouteInfo (string iface)
		{
			try {
				gateways = new IPAddressCollection ();
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
							if (!ip.Equals (IPAddress.Any))
								gateways.Add (ip);
						}
					}
				}
			} catch {
			}
		}

		static Regex ns = new Regex (@"\s*nameserver\s+(?<address>.*)");
		static Regex search = new Regex (@"\s*search\s+(?<domain>.*)");
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
								dns_servers.Add (IPAddress.Parse (str));
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
			} finally {
				dns_servers.SetReadOnly ();
			}
		}

		public override IPAddressInformationCollection AnycastAddresses {
			get {
				List<IPAddress> anycastAddresses = new List<IPAddress> ();
				/* XXX:
				foreach (IPAddress address in addresses) {
					if (is_anycast_address (address)) {
						anycastAddresses.Add (address);
					}
				}
				*/
				return IPAddressInformationImplCollection.LinuxFromAnycast (anycastAddresses);
			}
		}

		[MonoTODO ("Always returns an empty collection.")]
		public override IPAddressCollection DhcpServerAddresses {
			get {
				// There are lots of different DHCP clients
				// that all store their configuration differently.
				// I'm not sure what to do here.
				IPAddressCollection coll = new IPAddressCollection ();
				coll.SetReadOnly ();
				return coll;
			}
		}

		public override IPAddressCollection DnsAddresses {
			get {
				ParseResolvConf ();
				return dns_servers;
			}
		}

		public override string DnsSuffix {
			get {
				ParseResolvConf ();
				return dns_suffix;
			}
		}
     
		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				ParseRouteInfo (this.iface.Name.ToString());
				if (gateways.Count > 0)
					return new LinuxGatewayIPAddressInformationCollection (gateways);
				else
					return LinuxGatewayIPAddressInformationCollection.Empty;
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
				List<IPAddress> multicastAddresses = new List<IPAddress> ();
				foreach (IPAddress address in addresses) {
					byte[] addressBytes = address.GetAddressBytes ();
					if (addressBytes[0] >= 224 && addressBytes[0] <= 239) {
						multicastAddresses.Add (address);
					}
				}
				return MulticastIPAddressInformationImplCollection.LinuxFromList (multicastAddresses);
			}
		}

		public override UnicastIPAddressInformationCollection UnicastAddresses {
			get {
				List<IPAddress> unicastAddresses = new List<IPAddress> ();
				foreach (IPAddress address in addresses) {
					switch (address.AddressFamily) {
						case AddressFamily.InterNetwork:
							byte top = address.GetAddressBytes () [0];
							if (top >= 224 && top <= 239)
								continue;
							unicastAddresses.Add (address);
							break;

						case AddressFamily.InterNetworkV6:
							if (address.IsIPv6Multicast)
								continue;
							unicastAddresses.Add (address);
							break;
					}
				}
				return UnicastIPAddressInformationImplCollection.LinuxFromList (unicastAddresses);
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
	}

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
			Win32_IP_ADAPTER_INFO v4info = Win32NetworkInterface2.GetAdapterInfoByIndex (mib4.Index);
			return v4info != null ? new Win32IPv4InterfaceProperties (v4info, mib4) : null;
		}

		public override IPv6InterfaceProperties GetIPv6Properties ()
		{
			Win32_IP_ADAPTER_INFO v6info = Win32NetworkInterface2.GetAdapterInfoByIndex (mib6.Index);
			return v6info != null ? new Win32IPv6InterfaceProperties (mib6) : null;
		}

		public override IPAddressInformationCollection AnycastAddresses {
			get { return IPAddressInformationImplCollection.Win32FromAnycast (addr.FirstAnycastAddress); }
		}

		public override IPAddressCollection DhcpServerAddresses {
			get {
				Win32_IP_ADAPTER_INFO v4info = Win32NetworkInterface2.GetAdapterInfoByIndex (mib4.Index);
				// FIXME: should ipv6 DhcpServer be considered?
				return v4info != null ? new Win32IPAddressCollection (v4info.DhcpServer) : Win32IPAddressCollection.Empty;
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
				Win32_IP_ADAPTER_INFO v4info = Win32NetworkInterface2.GetAdapterInfoByIndex (mib4.Index);
				// FIXME: should ipv6 DhcpServer be considered?
				return v4info != null ? new Win32GatewayIPAddressInformationCollection (v4info.GatewayList) : Win32GatewayIPAddressInformationCollection.Empty;
			}
		}

		public override bool IsDnsEnabled {
			get { return Win32_FIXED_INFO.Instance.EnableDns != 0; }
		}

		public override bool IsDynamicDnsEnabled {
			get { return addr.DdnsEnabled; }
		}

		public override MulticastIPAddressInformationCollection MulticastAddresses {
			get { return MulticastIPAddressInformationImplCollection.Win32FromMulticast (addr.FirstMulticastAddress); }
		}

		public override UnicastIPAddressInformationCollection UnicastAddresses {
			get {
				Win32_IP_ADAPTER_INFO ai = Win32NetworkInterface2.GetAdapterInfoByIndex (mib4.Index);
				// FIXME: should ipv6 DhcpServer be considered?
				return ai != null ? UnicastIPAddressInformationImplCollection.Win32FromUnicast ((int) ai.Index, addr.FirstUnicastAddress) : UnicastIPAddressInformationImplCollection.Empty;
			}
		}

		public override IPAddressCollection WinsServersAddresses {
			get {
				Win32_IP_ADAPTER_INFO v4info = Win32NetworkInterface2.GetAdapterInfoByIndex (mib4.Index);
				// FIXME: should ipv6 DhcpServer be considered?
				return v4info != null ? new Win32IPAddressCollection (v4info.PrimaryWinsServer, v4info.SecondaryWinsServer) : Win32IPAddressCollection.Empty;
			}
		}

	}
}
#endif

