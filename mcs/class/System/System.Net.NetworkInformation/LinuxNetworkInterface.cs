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
using System.IO;
using System.Globalization;

namespace System.Net.NetworkInformation {

	internal class LinuxNetworkInterfaceAPI : UnixNetworkInterfaceAPI
	{
		const int AF_INET = 2;
		const int AF_INET6 = 10;
		const int AF_PACKET = 17;

		static void FreeInterfaceAddresses (IntPtr ifap)
		{
#if MONODROID
			AndroidPlatform.FreeInterfaceAddresses (ifap);
#else
			freeifaddrs (ifap);
#endif
		}

		static int GetInterfaceAddresses (out IntPtr ifap)
		{
#if MONODROID
			return AndroidPlatform.GetInterfaceAddresses (out ifap);
#else
			return getifaddrs (out ifap);
#endif
		}

		public override NetworkInterface [] GetAllNetworkInterfaces ()
		{

			var interfaces = new Dictionary <string, LinuxNetworkInterface> ();
			IntPtr ifap;
			if (GetInterfaceAddresses (out ifap) != 0)
				throw new SystemException ("getifaddrs() failed");

			try {
				IntPtr next = ifap;
				while (next != IntPtr.Zero) {
					ifaddrs   addr = (ifaddrs) Marshal.PtrToStructure (next, typeof (ifaddrs));
					IPAddress address = IPAddress.None;
					string    name = addr.ifa_name;
					int       index = -1;
					byte[]    macAddress = null;
					NetworkInterfaceType type = NetworkInterfaceType.Unknown;
					int       nullNameCount = 0;

					if (addr.ifa_addr != IntPtr.Zero) {
						sockaddr_in sockaddr = (sockaddr_in) Marshal.PtrToStructure (addr.ifa_addr, typeof (sockaddr_in));

						if (sockaddr.sin_family == AF_INET6) {
							sockaddr_in6 sockaddr6 = (sockaddr_in6) Marshal.PtrToStructure (addr.ifa_addr, typeof (sockaddr_in6));
							address = new IPAddress (sockaddr6.sin6_addr.u6_addr8, sockaddr6.sin6_scope_id);
						} else if (sockaddr.sin_family == AF_INET) {
							address = new IPAddress (sockaddr.sin_addr);
						} else if (sockaddr.sin_family == AF_PACKET) {
							sockaddr_ll sockaddrll = (sockaddr_ll) Marshal.PtrToStructure (addr.ifa_addr, typeof (sockaddr_ll));
							if (((int)sockaddrll.sll_halen) > sockaddrll.sll_addr.Length){
								next = addr.ifa_next;
								continue;
							}

							macAddress = new byte [(int) sockaddrll.sll_halen];
							Array.Copy (sockaddrll.sll_addr, 0, macAddress, 0, macAddress.Length);
							index = sockaddrll.sll_ifindex;

							int hwtype = (int)sockaddrll.sll_hatype;
							if (Enum.IsDefined (typeof (LinuxArpHardware), hwtype)) {
								switch ((LinuxArpHardware)hwtype) {
									case LinuxArpHardware.EETHER:
										goto case LinuxArpHardware.ETHER;

									case LinuxArpHardware.ETHER:
										type = NetworkInterfaceType.Ethernet;
										break;

									case LinuxArpHardware.PRONET:
										type = NetworkInterfaceType.TokenRing;
										break;

									case LinuxArpHardware.ATM:
										type = NetworkInterfaceType.Atm;
										break;

									case LinuxArpHardware.SLIP:
									case LinuxArpHardware.CSLIP:
									case LinuxArpHardware.SLIP6:
									case LinuxArpHardware.CSLIP6:
										type = NetworkInterfaceType.Slip;
										break;

									case LinuxArpHardware.PPP:
										type = NetworkInterfaceType.Ppp;
										break;

									case LinuxArpHardware.LOOPBACK:
										type = NetworkInterfaceType.Loopback;
										macAddress = null;
										break;

									case LinuxArpHardware.FDDI:
										type = NetworkInterfaceType.Fddi;
										break;

									case LinuxArpHardware.SIT:
									case LinuxArpHardware.IPDDP:
									case LinuxArpHardware.IPGRE:
									case LinuxArpHardware.IP6GRE:
									case LinuxArpHardware.TUNNEL6:
									case LinuxArpHardware.TUNNEL:
										type = NetworkInterfaceType.Tunnel;
										break;
								}
							}
						}
					}

					LinuxNetworkInterface iface = null;

					if (String.IsNullOrEmpty (name))
						name = "\0" + (++nullNameCount).ToString ();

					if (!interfaces.TryGetValue (name, out iface)) {
						iface = new LinuxNetworkInterface (name);
						interfaces.Add (name, iface);
					}

					if (!address.Equals (IPAddress.None))
						iface.AddAddress (address);

					if (macAddress != null || type == NetworkInterfaceType.Loopback) {
						if (type == NetworkInterfaceType.Ethernet) {
							if (Directory.Exists(iface.IfacePath + "wireless")) {
								type = NetworkInterfaceType.Wireless80211;
							}
						}
						iface.SetLinkLayerInfo (index, macAddress, type);
					}

					next = addr.ifa_next;
				}
			} finally {
				FreeInterfaceAddresses (ifap);
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
			return if_nametoindex ("lo");
		}

		public override IPAddress GetNetMask (IPAddress address)
		{
			foreach (ifaddrs networkInteface in GetNetworkInterfaces()) {
				if (networkInteface.ifa_addr == IntPtr.Zero)
					continue;

				var sockaddr = (sockaddr_in)Marshal.PtrToStructure(networkInteface.ifa_addr, typeof(sockaddr_in));

				if (sockaddr.sin_family != AF_INET)
					continue;

				if (!address.Equals(new IPAddress(sockaddr.sin_addr)))
					continue;

				var netmask = (sockaddr_in)Marshal.PtrToStructure(networkInteface.ifa_netmask, typeof(sockaddr_in));
				return new IPAddress(netmask.sin_addr);
			}

			return null;
		}

		private static IEnumerable<ifaddrs> GetNetworkInterfaces()
		{
			IntPtr ifap = IntPtr.Zero;

			try {
				if (GetInterfaceAddresses(out ifap) != 0)
					yield break;

				var next = ifap;
				while (next != IntPtr.Zero) {
					var addr = (ifaddrs)Marshal.PtrToStructure(next, typeof(ifaddrs));
					yield return addr;
					next = addr.ifa_next;
				}
			} finally {
				if (ifap != IntPtr.Zero)
					FreeInterfaceAddresses(ifap);
			}
		}
	}

	//
	// This class needs support from the libsupport.so library to fetch the
	// data using arch-specific ioctls.
	//
	// For this to work, we have to create this on the factory above.
	//
	sealed class LinuxNetworkInterface : UnixNetworkInterface
	{
		//NetworkInterfaceType type;
		string               iface_path;
		string               iface_operstate_path;
		string               iface_flags_path;

#if MONODROID
		[DllImport ("__Internal")]
		static extern int _monodroid_get_android_api_level ();

		[DllImport ("__Internal")]
		static extern bool _monodroid_get_network_interface_up_state (string ifname, ref bool is_up);

		[DllImport ("__Internal")]
		static extern bool _monodroid_get_network_interface_supports_multicast (string ifname, ref bool supports_multicast);

		bool android_use_java_api;
#endif

		internal string IfacePath {
			get { return iface_path; }
		}

		internal LinuxNetworkInterface (string name)
			: base (name)
		{
			iface_path = "/sys/class/net/" + name + "/";
			iface_operstate_path = iface_path + "operstate";
			iface_flags_path = iface_path + "flags";
#if MONODROID
			android_use_java_api = _monodroid_get_android_api_level () >= 24;
#endif
		}

		public override IPInterfaceProperties GetIPProperties ()
		{
			if (ipproperties == null)
				ipproperties = new LinuxIPInterfaceProperties (this, addresses);
			return ipproperties;
		}

		public override IPv4InterfaceStatistics GetIPv4Statistics ()
		{
			if (ipv4stats == null)
				ipv4stats = new LinuxIPv4InterfaceStatistics (this);
			return ipv4stats;
		}

		public override OperationalStatus OperationalStatus {
			get {
#if MONODROID
				if (android_use_java_api) {
					// Starting from API 24 (Android 7 "Nougat") Android restricts access to many
					// files in the /sys filesystem (see https://code.google.com/p/android/issues/detail?id=205565
					// for more information) and therefore we are forced to call into Java API in
					// order to get the information. Alas, what we can obtain in this way is quite
					// limited. In the case of OperationalStatus we can only determine whether the
					// interface is up or down. There is a way to get more detailed information but
					// it requires an instance of the Android Context class which is not available
					// to us here.
					bool is_up = false;
					if (_monodroid_get_network_interface_up_state (Name, ref is_up))
						return is_up ? OperationalStatus.Up : OperationalStatus.Down;
					else
						return OperationalStatus.Unknown;
				}
#endif
				if (!Directory.Exists (iface_path))
					return OperationalStatus.Unknown;

				try {
					string s = ReadLine (iface_operstate_path);

					switch (s){
						case "unknown":
							return OperationalStatus.Unknown;

						case "notpresent":
							return OperationalStatus.NotPresent;

						case "down":
							return OperationalStatus.Down;

						case "lowerlayerdown":
							return OperationalStatus.LowerLayerDown;

						case "testing":
							return OperationalStatus.Testing;

						case "dormant":
							return OperationalStatus.Dormant;

						case "up":
							return OperationalStatus.Up;
					}
				} catch {
				}
				return OperationalStatus.Unknown;
			}
		}

		public override bool SupportsMulticast {
			get {
#if MONODROID
				if (android_use_java_api) {
					// Starting from API 24 (Android 7 "Nougat") Android restricts access to many
					// files in the /sys filesystem (see https://code.google.com/p/android/issues/detail?id=205565
					// for more information) and therefore we are forced to call into Java API in
					// order to get the information.
					bool supports_multicast = false;
					_monodroid_get_network_interface_supports_multicast (Name, ref supports_multicast);
					return supports_multicast;
				}
#endif
				if (!Directory.Exists (iface_path))
					return false;

				try {
					string s = ReadLine (iface_flags_path);
					if (s.Length > 2 && s [0] == '0' && s [1] == 'x')
						s = s.Substring (2);

					ulong f = UInt64.Parse (s, NumberStyles.HexNumber);

					// Hardcoded, only useful for Linux.
					return ((f & 0x1000) == 0x1000);
				} catch {
					return false;
				}
			}
		}

		internal static string ReadLine (string path)
		{
			using (FileStream fs = File.OpenRead (path)){
				using (StreamReader sr = new StreamReader (fs)){
					return sr.ReadLine ();
				}
			}
		}
	}
}
