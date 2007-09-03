//
// System.Net.NetworkInformation.NetworkInterface
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
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation {
	public abstract class NetworkInterface {
		protected NetworkInterface ()
		{
		}

		[MonoTODO ("unimplemented on Non-Windows")]
		public static NetworkInterface [] GetAllNetworkInterfaces ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
				throw new NotSupportedException ("This platform is not supported");
			default:
				if (Environment.OSVersion.Version >= new Version (5, 1))
					return Win32NetworkInterface2.GetAllNetworkInterfaces ();
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static bool GetIsNetworkAvailable ()
		{
			return true;
		}

		[MonoTODO]
		public static int LoopbackInterfaceIndex {
			get { return 0; }
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

	class Win32NetworkInterface2 : NetworkInterface
	{
		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetAdaptersInfo (byte [] info, ref int size);

		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetAdaptersAddresses (uint family, uint flags, IntPtr reserved, byte [] info, ref int size);

		[DllImport ("iphlpapi.dll", SetLastError = true)]
		static extern int GetIfEntry (ref Win32_MIB_IFROW row);

		public static NetworkInterface [] GetAllNetworkInterfaces ()
		{
			Win32_IP_ADAPTER_INFO [] ai = GetAdaptersInfo ();
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

