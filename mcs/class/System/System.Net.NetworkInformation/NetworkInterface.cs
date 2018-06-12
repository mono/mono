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
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Globalization;

namespace System.Net.NetworkInformation {
	static class SystemNetworkInterface {

		static readonly NetworkInterfaceFactory nif = NetworkInterfaceFactory.Create ();

		public static NetworkInterface [] GetNetworkInterfaces ()
		{
			try {
				return nif.GetAllNetworkInterfaces ();
			} catch {
				return new NetworkInterface [0];
			}
		}

		public static bool InternalGetIsNetworkAvailable ()
		{
			// TODO:
			return true;
		}

		public static int InternalLoopbackInterfaceIndex {
			get {
				return nif.GetLoopbackInterfaceIndex ();
			}
		}

		public static int InternalIPv6LoopbackInterfaceIndex {
			get {
				throw new NotImplementedException ();
			}
		}

		public static IPAddress GetNetMask (IPAddress address)
		{
			return nif.GetNetMask (address);
		}
	}

	abstract class NetworkInterfaceFactory
	{
		internal abstract class UnixNetworkInterfaceAPI : NetworkInterfaceFactory
		{
#if ORBIS
			public static int if_nametoindex(string ifname)
			{
				throw new PlatformNotSupportedException ();
			}

			protected static int getifaddrs (out IntPtr ifap)
			{
				throw new PlatformNotSupportedException ();
			}

			protected static void freeifaddrs (IntPtr ifap)
			{
				throw new PlatformNotSupportedException ();
			}
#else
			[DllImport("libc")]
			public static extern int if_nametoindex(string ifname);

			[DllImport ("libc")]
			protected static extern int getifaddrs (out IntPtr ifap);

			[DllImport ("libc")]
			protected static extern void freeifaddrs (IntPtr ifap);
#endif
		}

		class MacOsNetworkInterfaceAPI : UnixNetworkInterfaceAPI
		{
			const int AF_INET  = 2;
			const int AF_INET6 = 30;
			const int AF_LINK  = 18;

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

		class LinuxNetworkInterfaceAPI : UnixNetworkInterfaceAPI
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

#if WIN_PLATFORM
		class Win32NetworkInterfaceAPI : NetworkInterfaceFactory
		{
			private const string IPHLPAPI = "iphlpapi.dll";

			[DllImport (IPHLPAPI, SetLastError = true)]
			static extern int GetAdaptersAddresses (uint family, uint flags, IntPtr reserved, IntPtr info, ref int size);

			[DllImport (IPHLPAPI)]
			static extern uint GetBestInterfaceEx (byte[] ipAddress, out int index);

			static Win32_IP_ADAPTER_ADDRESSES [] GetAdaptersAddresses ()
			{
				IntPtr ptr = IntPtr.Zero;
				int len = 0;
				uint flags = Win32_IP_ADAPTER_ADDRESSES.GAA_FLAG_INCLUDE_WINS_INFO | Win32_IP_ADAPTER_ADDRESSES.GAA_FLAG_INCLUDE_GATEWAYS;
				GetAdaptersAddresses (0, flags, IntPtr.Zero, ptr, ref len);
				if (Marshal.SizeOf (typeof (Win32_IP_ADAPTER_ADDRESSES)) > len)
					throw new NetworkInformationException ();

				ptr = Marshal.AllocHGlobal(len);
				int ret = GetAdaptersAddresses (0, flags, IntPtr.Zero, ptr, ref len);
				if (ret != 0)
					throw new NetworkInformationException (ret);

				List<Win32_IP_ADAPTER_ADDRESSES> l = new List<Win32_IP_ADAPTER_ADDRESSES> ();
				Win32_IP_ADAPTER_ADDRESSES info;
				for (IntPtr p = ptr; p != IntPtr.Zero; p = info.Next) {
					info = Marshal.PtrToStructure<Win32_IP_ADAPTER_ADDRESSES> (p);
					l.Add (info);
				}

				return l.ToArray ();
			}

			public override NetworkInterface [] GetAllNetworkInterfaces ()
			{
	//			Win32_IP_ADAPTER_INFO [] ai = GetAdaptersInfo ();
				Win32_IP_ADAPTER_ADDRESSES [] aa = GetAdaptersAddresses ();
				NetworkInterface [] ret = new NetworkInterface [aa.Length];
				for (int i = 0; i < ret.Length; i++)
					ret [i] = new Win32NetworkInterface2 (aa [i]);
				return ret;
			}

			private static int GetBestInterfaceForAddress (IPAddress addr) {
				int index;
				SocketAddress address = new SocketAddress (addr);
				int error = (int) GetBestInterfaceEx (address.m_Buffer, out index);
				if (error != 0) {
					throw new NetworkInformationException (error);
				}

				return index;
			}

			public override int GetLoopbackInterfaceIndex ()
			{
				return GetBestInterfaceForAddress (IPAddress.Loopback);
			}

			public override IPAddress GetNetMask (IPAddress address)
			{
				throw new NotImplementedException ();
			}
		}
#endif

		public abstract NetworkInterface [] GetAllNetworkInterfaces ();
		public abstract int GetLoopbackInterfaceIndex ();
		public abstract IPAddress GetNetMask (IPAddress address);

		public static NetworkInterfaceFactory Create ()
		{
#if MONOTOUCH || XAMMAC
			return new MacOsNetworkInterfaceAPI ();
#else
			bool runningOnUnix = (Environment.OSVersion.Platform == PlatformID.Unix);

			if (runningOnUnix) {
				if (Platform.IsMacOS || Platform.IsFreeBSD)
					return new MacOsNetworkInterfaceAPI ();
					
				return new LinuxNetworkInterfaceAPI ();
			}

#if WIN_PLATFORM
			Version windowsVer51 = new Version (5, 1);
			if (Environment.OSVersion.Version >= windowsVer51)
				return new Win32NetworkInterfaceAPI ();
#endif

			throw new NotImplementedException ();
#endif
		}
	}

	abstract class UnixNetworkInterface : NetworkInterface
	{

		protected IPv4InterfaceStatistics ipv4stats;
		protected IPInterfaceProperties ipproperties;
		
		string               name;
		//int                  index;
		protected List <IPAddress> addresses;
		byte[]               macAddress;
		NetworkInterfaceType type;
		
		internal UnixNetworkInterface (string name)
		{
			this.name = name;
			addresses = new List<IPAddress> ();
		}

		internal void AddAddress (IPAddress address)
		{
			addresses.Add (address);
		}

		internal void SetLinkLayerInfo (int index, byte[] macAddress, NetworkInterfaceType type)
		{
			//this.index = index;
			this.macAddress = macAddress;
			this.type = type;
		}

		public override PhysicalAddress GetPhysicalAddress ()
		{
			if (macAddress != null)
				return new PhysicalAddress (macAddress);
			else
				return PhysicalAddress.None;
		}

		public override bool Supports (NetworkInterfaceComponent networkInterfaceComponent)
		{
			bool wantIPv4 = networkInterfaceComponent == NetworkInterfaceComponent.IPv4;
			bool wantIPv6 = wantIPv4 ? false : networkInterfaceComponent == NetworkInterfaceComponent.IPv6;
				
			foreach (IPAddress address in addresses) {
				if (wantIPv4 && address.AddressFamily == AddressFamily.InterNetwork)
					return true;
				else if (wantIPv6 && address.AddressFamily == AddressFamily.InterNetworkV6)
					return true;
			}
			
			return false;
		}

		public override string Description {
			get { return name; }
		}

		public override string Id {
			get { return name; }
		}

		public override bool IsReceiveOnly {
			get { return false; }
		}

		public override string Name {
			get { return name; }
		}
		
		public override NetworkInterfaceType NetworkInterfaceType {
			get { return type; }
		}
		
		[MonoTODO ("Parse dmesg?")]
		public override long Speed {
			get {
				// Bits/s
				return 1000000;
			}
		}

		internal int NameIndex {
			get {
				return NetworkInterfaceFactory.UnixNetworkInterfaceAPI.if_nametoindex (Name);
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

#if WIN_PLATFORM
	class Win32NetworkInterface2 : NetworkInterface
	{
		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetAdaptersInfo (IntPtr info, ref int size);

		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetIfEntry (ref Win32_MIB_IFROW row);

		static Win32_IP_ADAPTER_INFO [] GetAdaptersInfo ()
		{
			int len = 0;
			IntPtr ptr = IntPtr.Zero;
			GetAdaptersInfo (ptr, ref len);
			ptr = Marshal.AllocHGlobal(len);
			int ret = GetAdaptersInfo (ptr, ref len);

			if (ret != 0)
				throw new NetworkInformationException (ret);

			List<Win32_IP_ADAPTER_INFO> l = new List<Win32_IP_ADAPTER_INFO> ();
			Win32_IP_ADAPTER_INFO info;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = info.Next) {
				info = Marshal.PtrToStructure<Win32_IP_ADAPTER_INFO> (p);
				l.Add (info);
			}
			return l.ToArray ();
		}

		Win32_IP_ADAPTER_ADDRESSES addr;
		Win32_MIB_IFROW mib4, mib6;
		Win32IPv4InterfaceStatistics ip4stats;
		IPInterfaceProperties ip_if_props;

		internal Win32NetworkInterface2 (Win32_IP_ADAPTER_ADDRESSES addr)
		{
			this.addr = addr;
			mib4 = default (Win32_MIB_IFROW);
			mib4.Index = addr.Alignment.IfIndex;
			if (GetIfEntry (ref mib4) != 0)
				mib4.Index = -1; // unavailable;
			mib6 = default (Win32_MIB_IFROW);
			mib6.Index = addr.Ipv6IfIndex;
			if (GetIfEntry (ref mib6) != 0)
				mib6.Index = -1; // unavailable;
			ip4stats = new Win32IPv4InterfaceStatistics (mib4);
			ip_if_props = new Win32IPInterfaceProperties2 (addr, mib4, mib6);
		}

		public override IPInterfaceProperties GetIPProperties ()
		{
			return ip_if_props;
		}

		public override IPv4InterfaceStatistics GetIPv4Statistics ()
		{
			return ip4stats;
		}

		public override PhysicalAddress GetPhysicalAddress ()
		{
			byte [] bytes = new byte [addr.PhysicalAddressLength];
			Array.Copy (addr.PhysicalAddress, 0, bytes, 0, bytes.Length);
			return new PhysicalAddress (bytes);
		}

		public override bool Supports (NetworkInterfaceComponent networkInterfaceComponent)
		{
			switch (networkInterfaceComponent) {
			case NetworkInterfaceComponent.IPv4:
				return mib4.Index >= 0;
			case NetworkInterfaceComponent.IPv6:
				return mib6.Index >= 0;
			}
			return false;
		}

		public override string Description {
			get { return addr.Description; }
		}
		public override string Id {
			get { return addr.AdapterName; }
		}
		public override bool IsReceiveOnly {
			get { return addr.IsReceiveOnly; }
		}
		public override string Name {
			get { return addr.FriendlyName; }
		}
		public override NetworkInterfaceType NetworkInterfaceType {
			get { return addr.IfType; }
		}
		public override OperationalStatus OperationalStatus {
			get { return addr.OperStatus; }
		}
		public override long Speed {
			get { return mib6.Index >= 0 ? mib6.Speed : mib4.Speed; }
		}
		public override bool SupportsMulticast {
			get { return !addr.NoMulticast; }
		}
	}
#endif
}

