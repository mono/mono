//
// System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Miguel de Icaza (miguel@novell.com
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
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Globalization;

namespace System.Net.NetworkInformation {
	public abstract class NetworkInterface {
		protected NetworkInterface ()
		{
		}

		[MonoTODO("Only works on Linux and Windows")]
		public static NetworkInterface [] GetAllNetworkInterfaces ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
				if (Directory.Exists ("/sys/class/net")){
					return LinuxNetworkInterface.ImplGetAllNetworkInterfaces ();
				}
				return new NetworkInterface [0];
				
			default:
				if (Environment.OSVersion.Version >= new Version (5, 1))
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

		[MonoTODO("Only works on Linux")]
		public static int LoopbackInterfaceIndex {
			get {
				try {
					return Int32.Parse (ReadLine ("/sys/class/net/lo/ifindex"));
				} catch {
					return 0;
				}
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

	//
	// This class needs support from the libsupport.so library to fetch the
	// data using arch-specific ioctls.
	//
	// For this to work, we have to create this on the factory above.
	//
	class LinuxNetworkInterface : NetworkInterface
	{
		internal string iface;
		internal string iface_path;
		IPv4InterfaceStatistics ipv4stats;
		
		public static NetworkInterface [] ImplGetAllNetworkInterfaces ()
		{
			string [] dirs = Directory.GetFileSystemEntries ("/sys/class/net");
			ArrayList a = null;
			
			foreach (string d in dirs){
				if (a == null)
					a = new ArrayList ();

				a.Add (new LinuxNetworkInterface (Path.GetFileName (d)));
			}
			if (a == null)
				return new NetworkInterface [0];

			return (NetworkInterface []) a.ToArray (typeof (LinuxNetworkInterface));
		}

		LinuxNetworkInterface (string dir)
		{
			iface = dir;
			iface_path = "/sys/class/net/" + iface + "/";
			ipv4stats = new LinuxIPv4InterfaceStatistics (this);
		}
		
		public override IPInterfaceProperties GetIPProperties ()
		{
			throw new NotImplementedException ();
		}

		public override IPv4InterfaceStatistics GetIPv4Statistics ()
		{
			return ipv4stats;
		}

		public override PhysicalAddress GetPhysicalAddress ()
		{
			return PhysicalAddress.ParseEthernet (ReadLine (iface_path + "address"));
			
		}

		public override bool Supports (NetworkInterfaceComponent networkInterfaceComponent)
		{
			switch (networkInterfaceComponent) {
			case NetworkInterfaceComponent.IPv4:
				return (Directory.Exists ("/proc/sys/net/ipv4/conf/" + iface));
				
			case NetworkInterfaceComponent.IPv6:
				return (Directory.Exists ("/proc/sys/net/ipv6/conf/" + iface));
			}
			return false;
		}

		public override string Description {
			get { return iface_path; }
		}

		public override string Id {
			get { return iface; }
		}

		public override bool IsReceiveOnly {
			get { return false; }
		}

		public override string Name {
			get { return iface; }
		}
		
		public override NetworkInterfaceType NetworkInterfaceType {
			get {
				try {
					// The constants come from the ARP hardware identifiers, this is what Linux uses
					
					switch (Int32.Parse (ReadLine (iface_path + "type"))){
					case 1:
						if (Directory.Exists (iface_path + "wireless"))
							return NetworkInterfaceType.Wireless80211;
						
						return NetworkInterfaceType.Ethernet;

					case 19:
						return NetworkInterfaceType.Atm;
						
					case 512:
						return NetworkInterfaceType.Ppp;

					case 772:
						return NetworkInterfaceType.Loopback;

					case 774:
						return NetworkInterfaceType.Fddi;

					case 800:
						return NetworkInterfaceType.TokenRing;

					case 801:
						return NetworkInterfaceType.Wireless80211;
						
					case 256: // Slip
					case 257: // CSlip
					case 258: // Slip6
					case 259: // CSlip6
						return NetworkInterfaceType.Slip;
						
					// .NET exposes these, but we do not currently have a mapping:
					// BasicIsdn
					// PrimaryIsdn
					// Ethernet3Megabit
					// GenericModem
					// FastEthernetT
					// FastEthernetFx
					//
					// AsymmetricDsl
					// RateAdaptDsl = 95,
					// SymmetricDsl = 96,
					// VeryHighSpeedDsl = 97,
						
					}
					return NetworkInterfaceType.Unknown;
				} catch {
					return NetworkInterfaceType.Unknown;
				}
			}
		}
		
		public override OperationalStatus OperationalStatus {
			get {
				try {
					string s = ReadLine (iface_path + "operstate");

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

		public override long Speed {
			get {
				// What are the units?
				// In Linux there is no information about the device speed, maybe only for modems?
				throw new NotImplementedException ();
			}
		}
		
		public override bool SupportsMulticast {
			get {
				try {
					string s = ReadLine (iface_path + "flags");
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

