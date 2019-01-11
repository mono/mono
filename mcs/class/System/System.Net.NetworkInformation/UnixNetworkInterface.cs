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
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
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
				return UnixNetworkInterfaceAPI.if_nametoindex (Name);
			}
		}
	}
}
