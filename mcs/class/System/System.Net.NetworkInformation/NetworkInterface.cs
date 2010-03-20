//
// System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//      Eric Butler (eric@extremeboredom.net)
//      Marek Habersack (mhabersack@novell.com)
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
#if NET_2_0
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
	public abstract class NetworkInterface {
		[DllImport ("libc")]
		static extern int uname (IntPtr buf);

		static Version windowsVer51 = new Version (5, 1);
		static internal readonly bool runningOnUnix = (Environment.OSVersion.Platform == PlatformID.Unix);
		
		protected NetworkInterface ()
		{
		}

		[MonoTODO("Only works on Linux and Windows")]
		public static NetworkInterface [] GetAllNetworkInterfaces ()
		{
			if (runningOnUnix) {
				bool darwin = false;
				IntPtr buf = Marshal.AllocHGlobal (8192);
				if (uname (buf) == 0) {
					string os = Marshal.PtrToStringAnsi (buf);
					if (os == "Darwin")
						darwin = true;
				}
				Marshal.FreeHGlobal (buf);

				try {
					if (darwin)
						return MacOsNetworkInterface.ImplGetAllNetworkInterfaces ();
					else
						return LinuxNetworkInterface.ImplGetAllNetworkInterfaces ();
				} catch (SystemException ex) {
					throw ex;
				} catch {
					return new NetworkInterface [0];
				}
			} else {
				if (Environment.OSVersion.Version >= windowsVer51)
					return Win32NetworkInterface2.ImplGetAllNetworkInterfaces ();
				return new NetworkInterface [0];
			}
		}

		[MonoTODO("Always returns true")]
		public static bool GetIsNetworkAvailable ()
		{
			return true;
		}

		internal static string ReadLine (string path)
		{
			using (FileStream fs = File.OpenRead (path)){
				using (StreamReader sr = new StreamReader (fs)){
					return sr.ReadLine ();
				}
			}
		}
		
		[MonoTODO("Only works on Linux. Returns 0 on other systems.")]
		public static int LoopbackInterfaceIndex {
			get {
				if (runningOnUnix) {
					try {
						return UnixNetworkInterface.IfNameToIndex ("lo");
					} catch  {
						return 0;
					}
				} else
					return 0;
			}
		}

		public abstract IPInterfaceProperties GetIPProperties ();
		public abstract IPv4InterfaceStatistics GetIPv4Statistics ();
		public abstract PhysicalAddress GetPhysicalAddress ();
		public abstract bool Supports (NetworkInterfaceComponent networkInterfaceComponent);

		public abstract string Description { get; }
		public abstract string Id { get; }
		public abstract bool IsReceiveOnly { get; }
		public abstract string Name { get; }
		public abstract NetworkInterfaceType NetworkInterfaceType { get; }
		public abstract OperationalStatus OperationalStatus { get; }
		public abstract long Speed { get; }
		public abstract bool SupportsMulticast { get; }
	}

	abstract class UnixNetworkInterface : NetworkInterface
	{
		[DllImport("libc")]
		static extern int if_nametoindex(string ifname);

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

		public static int IfNameToIndex (string ifname)
		{
			return if_nametoindex(ifname);
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
	}

	//
	// This class needs support from the libsupport.so library to fetch the
	// data using arch-specific ioctls.
	//
	// For this to work, we have to create this on the factory above.
	//
	class LinuxNetworkInterface : UnixNetworkInterface
	{
		[DllImport ("libc")]
		static extern int getifaddrs (out IntPtr ifap);

		[DllImport ("libc")]
		static extern void freeifaddrs (IntPtr ifap);

		const int AF_INET   = 2;
		const int AF_INET6  = 10;
		const int AF_PACKET = 17;
		
		//NetworkInterfaceType type;
		string               iface_path;
		string               iface_operstate_path;
		string               iface_flags_path;		

		internal string IfacePath {
			get { return iface_path; }
		}
		
		public static NetworkInterface [] ImplGetAllNetworkInterfaces ()
		{
			var interfaces = new Dictionary <string, LinuxNetworkInterface> ();
			IntPtr ifap;
			if (getifaddrs (out ifap) != 0)
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
								Console.Error.WriteLine ("Got a bad hardware address length for an AF_PACKET {0} {1}",
											 sockaddrll.sll_halen, sockaddrll.sll_addr.Length);
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

									case LinuxArpHardware.TUNNEL6:
										goto case LinuxArpHardware.TUNNEL;
										
									case LinuxArpHardware.TUNNEL:
										type = NetworkInterfaceType.Tunnel;
										break;
								}
							}
						}
					}

					LinuxNetworkInterface iface = null;

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
		
		LinuxNetworkInterface (string name)
			: base (name)
		{
			iface_path = "/sys/class/net/" + name + "/";
			iface_operstate_path = iface_path + "operstate";
			iface_flags_path = iface_path + "flags";
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
	}

	class MacOsNetworkInterface : UnixNetworkInterface
	{
		[DllImport ("libc")]
		static extern int getifaddrs (out IntPtr ifap);

		[DllImport ("libc")]
		static extern void freeifaddrs (IntPtr ifap);

		const int AF_INET  = 2;
		const int AF_INET6 = 30;
		const int AF_LINK  = 18;
		
		public static NetworkInterface [] ImplGetAllNetworkInterfaces ()
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
						MacOsStructs.sockaddr sockaddr = (MacOsStructs.sockaddr) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr));

						if (sockaddr.sa_family == AF_INET6) {
							MacOsStructs.sockaddr_in6 sockaddr6 = (MacOsStructs.sockaddr_in6) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in6));
							address = new IPAddress (sockaddr6.sin6_addr.u6_addr8, sockaddr6.sin6_scope_id);
						} else if (sockaddr.sa_family == AF_INET) {
							MacOsStructs.sockaddr_in sockaddrin = (MacOsStructs.sockaddr_in) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_in));
							address = new IPAddress (sockaddrin.sin_addr);
						} else if (sockaddr.sa_family == AF_LINK) {
							MacOsStructs.sockaddr_dl sockaddrdl = (MacOsStructs.sockaddr_dl) Marshal.PtrToStructure (addr.ifa_addr, typeof (MacOsStructs.sockaddr_dl));

							macAddress = new byte [(int) sockaddrdl.sdl_alen];
							Array.Copy (sockaddrdl.sdl_data, sockaddrdl.sdl_nlen, macAddress, 0, macAddress.Length);
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

					if (!interfaces.TryGetValue (name, out iface)) {
						iface = new MacOsNetworkInterface (name);
						interfaces.Add (name, iface);
					}

					if (!address.Equals (IPAddress.None))
						iface.AddAddress (address);

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
		
		MacOsNetworkInterface (string name)
			: base (name)
		{
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
				return OperationalStatus.Unknown;
			}
		}

		public override bool SupportsMulticast {
			get {
				return false;
			}
		}
	}

	class Win32NetworkInterface2 : NetworkInterface
	{
		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetAdaptersInfo (byte [] info, ref int size);

		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetAdaptersAddresses (uint family, uint flags, IntPtr reserved, byte [] info, ref int size);

		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetIfEntry (ref Win32_MIB_IFROW row);

		public static NetworkInterface [] ImplGetAllNetworkInterfaces ()
		{
//			Win32_IP_ADAPTER_INFO [] ai = GetAdaptersInfo ();
			Win32_IP_ADAPTER_ADDRESSES [] aa = GetAdaptersAddresses ();
			NetworkInterface [] ret = new NetworkInterface [aa.Length];
			for (int i = 0; i < ret.Length; i++)
				ret [i] = new Win32NetworkInterface2 (aa [i]);
			return ret;
		}

		public static Win32_IP_ADAPTER_INFO GetAdapterInfoByIndex (int index)
		{
			foreach (Win32_IP_ADAPTER_INFO info in GetAdaptersInfo ())
				if (info.Index == index)
					return info;
			return null;
		}

		unsafe static Win32_IP_ADAPTER_INFO [] GetAdaptersInfo ()
		{
			byte [] bytes = null;
			int len = 0;
			GetAdaptersInfo (bytes, ref len);
			bytes = new byte [len];
			int ret = GetAdaptersInfo (bytes, ref len);

			if (ret != 0)
				throw new NetworkInformationException (ret);

			List<Win32_IP_ADAPTER_INFO> l = new List<Win32_IP_ADAPTER_INFO> ();
			fixed (byte* ptr = bytes) {
				Win32_IP_ADAPTER_INFO info;
				for (IntPtr p = (IntPtr) ptr; p != IntPtr.Zero; p = info.Next) {
					info = new Win32_IP_ADAPTER_INFO ();
					Marshal.PtrToStructure (p, info);
					l.Add (info);
				}
			}
			return l.ToArray ();
		}

		unsafe static Win32_IP_ADAPTER_ADDRESSES [] GetAdaptersAddresses ()
		{
			byte [] bytes = null;
			int len = 0;
			GetAdaptersAddresses (0, 0, IntPtr.Zero, bytes, ref len);
			bytes = new byte [len];
			int ret = GetAdaptersAddresses (0, 0, IntPtr.Zero, bytes, ref len);
			if (ret != 0)
				throw new NetworkInformationException (ret);

			List<Win32_IP_ADAPTER_ADDRESSES> l = new List<Win32_IP_ADAPTER_ADDRESSES> ();
			fixed (byte* ptr = bytes) {
				Win32_IP_ADAPTER_ADDRESSES info;
				for (IntPtr p = (IntPtr) ptr; p != IntPtr.Zero; p = info.Next) {
					info = new Win32_IP_ADAPTER_ADDRESSES ();
					Marshal.PtrToStructure (p, info);
					l.Add (info);
				}
			}
			return l.ToArray ();
		}

		Win32_IP_ADAPTER_ADDRESSES addr;
		Win32_MIB_IFROW mib4, mib6;
		Win32IPv4InterfaceStatistics ip4stats;
		IPInterfaceProperties ip_if_props;

		Win32NetworkInterface2 (Win32_IP_ADAPTER_ADDRESSES addr)
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
}
#endif

