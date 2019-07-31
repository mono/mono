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
using System.Net.Sockets;
using System.IO;
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
}
