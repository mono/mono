//
// System.Net.NetworkInformation.IPGlobalProperties
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Marek Safar (marek.safar@gmail.com)
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
#if WIN_PLATFORM
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	class Win32IPGlobalProperties : IPGlobalProperties
	{
		public const int AF_INET = 2;
		public const int AF_INET6 = 23;

		unsafe void FillTcpTable (out List<Win32_MIB_TCPROW> tab4, out List<Win32_MIB_TCP6ROW> tab6)
		{
			tab4 = new List<Win32_MIB_TCPROW> ();
			int size4 = 0;
			GetTcpTable (null, ref size4, true); // get size
			byte [] bytes4 = new byte [size4];
			GetTcpTable (bytes4, ref size4, true); // get list

			int structSize4 = Marshal.SizeOf (typeof (Win32_MIB_TCPROW));

			fixed (byte* ptr = bytes4) {
				int count = Marshal.ReadInt32 ((IntPtr) ptr);
				for (int i = 0; i < count; i++) {
					Win32_MIB_TCPROW row = new Win32_MIB_TCPROW ();
					Marshal.PtrToStructure ((IntPtr) (ptr + i * structSize4 + 4), row);
					tab4.Add (row);
				}
			}

			tab6 = new List<Win32_MIB_TCP6ROW> ();
			if (Environment.OSVersion.Version.Major >= 6) { // Vista
				int size6 = 0;
				GetTcp6Table (null, ref size6, true); // get size
				byte [] bytes6 = new byte [size6];
				GetTcp6Table (bytes6, ref size6, true); // get list

				int structSize6 = Marshal.SizeOf (typeof (Win32_MIB_TCP6ROW));

				fixed (byte* ptr = bytes6) {
					int count = Marshal.ReadInt32 ((IntPtr) ptr);
					for (int i = 0; i < count; i++) {
						Win32_MIB_TCP6ROW row = new Win32_MIB_TCP6ROW ();
						Marshal.PtrToStructure ((IntPtr) (ptr + i * structSize6 + 4), row);
						tab6.Add (row);
					}
				}
			}
		}

		bool IsListenerState (TcpState state)
		{
			switch (state) {
			case TcpState.SynSent:
			case TcpState.Listen:
			case TcpState.FinWait1:
			case TcpState.FinWait2:
			case TcpState.CloseWait:
				return true;
			}
			return false;
		}

		public override TcpConnectionInformation [] GetActiveTcpConnections ()
		{
			List<Win32_MIB_TCPROW> tab4 = null;
			List<Win32_MIB_TCP6ROW> tab6 = null;
			FillTcpTable (out tab4, out tab6);
			int size4 = tab4.Count;

			TcpConnectionInformation [] ret = new TcpConnectionInformation [size4 + tab6.Count];
			for (int i = 0; i < size4; i++)
				ret [i] = tab4 [i].TcpInfo;
			for (int i = 0; i < tab6.Count; i++)
				ret [size4 + i] = tab6 [i].TcpInfo;
			return ret;
		}

		public override IPEndPoint [] GetActiveTcpListeners ()
		{
			List<Win32_MIB_TCPROW> tab4 = null;
			List<Win32_MIB_TCP6ROW> tab6 = null;
			FillTcpTable (out tab4, out tab6);

			List<IPEndPoint> ret = new List<IPEndPoint> ();
			for (int i = 0, count = tab4.Count; i < count; i++)
				if (IsListenerState (tab4 [i].State))
					ret.Add (tab4 [i].LocalEndPoint);
			for (int i = 0, count = tab6.Count; i < count; i++)
				if (IsListenerState (tab6 [i].State))
					ret.Add (tab6 [i].LocalEndPoint);
			return ret.ToArray ();
		}

		public unsafe override IPEndPoint [] GetActiveUdpListeners ()
		{
			List<IPEndPoint> list = new List<IPEndPoint> ();

			byte [] bytes4 = null;
			int size4 = 0;
			GetUdpTable (null, ref size4, true); // get size
			bytes4 = new byte [size4];
			GetUdpTable (bytes4, ref size4, true); // get list

			int structSize4 = Marshal.SizeOf (typeof (Win32_MIB_UDPROW));

			fixed (byte* ptr = bytes4) {
				int count = Marshal.ReadInt32 ((IntPtr) ptr);
				for (int i = 0; i < count; i++) {
					Win32_MIB_UDPROW row = new Win32_MIB_UDPROW ();
					Marshal.PtrToStructure ((IntPtr) (ptr + i * structSize4 + 4), row);
					list.Add (row.LocalEndPoint);
				}
			}

			if (Environment.OSVersion.Version.Major >= 6) { // Vista
				byte [] bytes6 = null;
				int size6 = 0;
				GetUdp6Table (null, ref size6, true); // get size
				bytes6 = new byte [size6];
				GetUdp6Table (bytes6, ref size6, true); // get list

				int structSize6 = Marshal.SizeOf (typeof (Win32_MIB_UDP6ROW));

				fixed (byte* ptr = bytes6) {
					int count = Marshal.ReadInt32 ((IntPtr) ptr);
					for (int i = 0; i < count; i++) {
						Win32_MIB_UDP6ROW row = new Win32_MIB_UDP6ROW ();
						Marshal.PtrToStructure ((IntPtr) (ptr + i * structSize6 + 4), row);
						list.Add (row.LocalEndPoint);
					}
				}
			}

			return list.ToArray ();
		}

		public override IcmpV4Statistics GetIcmpV4Statistics ()
		{
			if (!Socket.OSSupportsIPv4)
				throw new NetworkInformationException ();
			Win32_MIBICMPINFO stats;
			GetIcmpStatistics (out stats, AF_INET);
			return new Win32IcmpV4Statistics (stats);
		}

		public override IcmpV6Statistics GetIcmpV6Statistics ()
		{
			if (!Socket.OSSupportsIPv6)
				throw new NetworkInformationException ();
			Win32_MIB_ICMP_EX stats;
			GetIcmpStatisticsEx (out stats, AF_INET6);
			return new Win32IcmpV6Statistics (stats);
		}

		public override IPGlobalStatistics GetIPv4GlobalStatistics ()
		{
			if (!Socket.OSSupportsIPv4)
				throw new NetworkInformationException ();
			Win32_MIB_IPSTATS stats;
			GetIpStatisticsEx (out stats, AF_INET);
			return new Win32IPGlobalStatistics (stats);
		}

		public override IPGlobalStatistics GetIPv6GlobalStatistics ()
		{
			if (!Socket.OSSupportsIPv6)
				throw new NetworkInformationException ();
			Win32_MIB_IPSTATS stats;
			GetIpStatisticsEx (out stats, AF_INET6);
			return new Win32IPGlobalStatistics (stats);
		}

		public override TcpStatistics GetTcpIPv4Statistics ()
		{
			if (!Socket.OSSupportsIPv4)
				throw new NetworkInformationException ();
			Win32_MIB_TCPSTATS stats;
			GetTcpStatisticsEx (out stats, AF_INET);
			return new Win32TcpStatistics (stats);
		}

		public override TcpStatistics GetTcpIPv6Statistics ()
		{
			if (!Socket.OSSupportsIPv6)
				throw new NetworkInformationException ();
			Win32_MIB_TCPSTATS stats;
			GetTcpStatisticsEx (out stats, AF_INET6);
			return new Win32TcpStatistics (stats);
		}

		public override UdpStatistics GetUdpIPv4Statistics ()
		{
			if (!Socket.OSSupportsIPv4)
				throw new NetworkInformationException ();
			Win32_MIB_UDPSTATS stats;
			GetUdpStatisticsEx (out stats, AF_INET);
			return new Win32UdpStatistics (stats);
		}

		public override UdpStatistics GetUdpIPv6Statistics ()
		{
			if (!Socket.OSSupportsIPv6)
				throw new NetworkInformationException ();
			Win32_MIB_UDPSTATS stats;
			GetUdpStatisticsEx (out stats, AF_INET6);
			return new Win32UdpStatistics (stats);
		}

		public override string DhcpScopeName {
			get { return Win32NetworkInterface.FixedInfo.ScopeId; }
		}

		public override string DomainName {
			get { return Win32NetworkInterface.FixedInfo.DomainName; }
		}

		public override string HostName {
			get { return Win32NetworkInterface.FixedInfo.HostName; }
		}

		public override bool IsWinsProxy {
			get { return Win32NetworkInterface.FixedInfo.EnableProxy != 0; }
		}

		public override NetBiosNodeType NodeType {
			get { return Win32NetworkInterface.FixedInfo.NodeType; }
		}

		// PInvokes

		[DllImport ("iphlpapi.dll")]
		static extern int GetTcpTable (byte [] pTcpTable, ref int pdwSize, bool bOrder);

		[DllImport ("iphlpapi.dll")]
		static extern int GetTcp6Table (byte [] TcpTable, ref int SizePointer, bool Order);

		[DllImport ("iphlpapi.dll")]
		static extern int GetUdpTable (byte [] pUdpTable, ref int pdwSize, bool bOrder);

		[DllImport ("iphlpapi.dll")]
		static extern int GetUdp6Table (byte [] Udp6Table, ref int SizePointer, bool Order);

		[DllImport ("iphlpapi.dll")]
		static extern int GetTcpStatisticsEx (out Win32_MIB_TCPSTATS pStats, int dwFamily);

		[DllImport ("iphlpapi.dll")]
		static extern int GetUdpStatisticsEx (out Win32_MIB_UDPSTATS pStats, int dwFamily);

		[DllImport ("iphlpapi.dll")]
		static extern int GetIcmpStatistics (out Win32_MIBICMPINFO pStats, int dwFamily);

		[DllImport ("iphlpapi.dll")]
		static extern int GetIcmpStatisticsEx (out Win32_MIB_ICMP_EX pStats, int dwFamily);

		[DllImport ("iphlpapi.dll")]
		static extern int GetIpStatisticsEx (out Win32_MIB_IPSTATS pStats, int dwFamily);

		[DllImport ("Ws2_32.dll")]
		static extern ushort ntohs (ushort netshort);

		// Win32 structures

		[StructLayout (LayoutKind.Explicit)]
		struct Win32_IN6_ADDR
		{
			[FieldOffset (0)]
			[MarshalAs ( UnmanagedType.ByValArray, SizeConst = 16)]
			public byte [] Bytes;
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_TCPROW
		{
			public TcpState State;
			public uint LocalAddr;
			public uint LocalPort;
			public uint RemoteAddr;
			public uint RemotePort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (LocalAddr, ntohs((ushort)LocalPort)); }
			}

			public IPEndPoint RemoteEndPoint {
				get { return new IPEndPoint (RemoteAddr, ntohs((ushort)RemotePort)); }
			}

			public TcpConnectionInformation TcpInfo {
				get { return new SystemTcpConnectionInformation (LocalEndPoint, RemoteEndPoint, State); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_TCP6ROW
		{
			public TcpState State;
			public Win32_IN6_ADDR LocalAddr;
			public uint LocalScopeId;
			public uint LocalPort;
			public Win32_IN6_ADDR RemoteAddr;
			public uint RemoteScopeId;
			public uint RemotePort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (new IPAddress (LocalAddr.Bytes, LocalScopeId), ntohs((ushort)LocalPort)); }
			}

			public IPEndPoint RemoteEndPoint {
				get { return new IPEndPoint (new IPAddress (RemoteAddr.Bytes, RemoteScopeId), ntohs((ushort)RemotePort)); }
			}

			public TcpConnectionInformation TcpInfo {
				get { return new SystemTcpConnectionInformation (LocalEndPoint, RemoteEndPoint, State); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_UDPROW
		{
			public uint LocalAddr;
			public uint LocalPort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (LocalAddr, ntohs((ushort)LocalPort)); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_UDP6ROW
		{
			public Win32_IN6_ADDR LocalAddr;
			public uint LocalScopeId;
			public uint LocalPort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (new IPAddress (LocalAddr.Bytes, LocalScopeId), ntohs((ushort)LocalPort)); }
			}
		}
	}

	internal static class Win32IPGlobalPropertiesFactoryPal {
		public static IPGlobalProperties Create ()
		{
#if MONO
#if WIN_PLATFORM
			return new Win32IPGlobalProperties ();
#else
			return null;
#endif
#else
			(new NetworkInformationPermission (NetworkInformationAccess.Read)).Demand ();
			return new SystemIPGlobalProperties ();
#endif
		}
	}
}
#endif
