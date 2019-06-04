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
#if WIN_PLATFORM
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	internal class Win32NetworkInterfaceAPI : NetworkInterfaceFactory
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

	sealed class Win32NetworkInterface2 : NetworkInterface
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
}
#endif
