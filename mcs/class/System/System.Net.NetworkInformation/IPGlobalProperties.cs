//
// System.Net.NetworkInformation.IPGlobalProperties
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
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation {
	public abstract class IPGlobalProperties {
		protected IPGlobalProperties ()
		{
		}

		[MonoTODO ("Unimplemented on non-Windows. A marshalling issue on Windows")]
		public static IPGlobalProperties GetIPGlobalProperties ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
				if (File.Exists (MibIPGlobalProperties.StatisticsFile))
					return new MibIPGlobalProperties ();
				throw new NotSupportedException ("This platform is not supported");
			default:
				return new Win32IPGlobalProperties ();
			}
		}

		public abstract TcpConnectionInformation [] GetActiveTcpConnections ();
		public abstract IPEndPoint [] GetActiveTcpListeners ();
		public abstract IPEndPoint [] GetActiveUdpListeners ();
		public abstract IcmpV4Statistics GetIcmpV4Statistics ();
		public abstract IcmpV6Statistics GetIcmpV6Statistics ();
		public abstract IPGlobalStatistics GetIPv4GlobalStatistics ();
		public abstract IPGlobalStatistics GetIPv6GlobalStatistics ();
		public abstract TcpStatistics GetTcpIPv4Statistics ();
		public abstract TcpStatistics GetTcpIPv6Statistics ();
		public abstract UdpStatistics GetUdpIPv4Statistics ();
		public abstract UdpStatistics GetUdpIPv6Statistics ();

		public abstract string DhcpScopeName { get; }
		public abstract string DomainName { get; }
		public abstract string HostName { get; }
		public abstract bool IsWinsProxy { get; }
		public abstract NetBiosNodeType NodeType { get; }
	}

	// It expects /proc/net/snmp file formattes as:
	// http://www.linuxdevcenter.com/linux/2000/11/16/example5.html
	class MibIPGlobalProperties : IPGlobalProperties
	{
		public const string StatisticsFile = "/proc/net/snmp";
		public const string StatisticsFileIPv6 = "/proc/net/snmp6";

		StringDictionary GetProperties4 (string item)
		{
			string file = StatisticsFile;

			string head = item + ": ";
			using (StreamReader sr = new StreamReader (file, Encoding.ASCII)) {
				string [] keys = null;
				string [] values = null;
				string s = String.Empty;
				do {
					s = sr.ReadLine ();
					if (String.IsNullOrEmpty (s))
						continue;
					if (s.Length <= head.Length || String.CompareOrdinal (s, 0, head, 0, head.Length) != 0)
						continue;
					if (keys == null)
						keys = s.Substring (head.Length).Split (' ');
					else if (values != null)
						// hmm, there may be better error type...
						throw CreateException (file, String.Format ("Found duplicate line for values for the same item '{0}'", item));
					else {
						values = s.Substring (head.Length).Split (' ');
						break;
					}
				} while (!sr.EndOfStream);

				if (values == null)
					throw CreateException (file, String.Format ("No corresponding line was not found for '{0}'", item));
				if (keys.Length != values.Length)
					throw CreateException (file, String.Format ("The counts in the header line and the value line do not match for '{0}'", item));
				StringDictionary dic = new StringDictionary ();
				for (int i = 0; i < keys.Length; i++)
					dic [keys [i]] = values [i];
				return dic;
			}
		}

		StringDictionary GetProperties6 (string item)
		{
			if (!File.Exists (StatisticsFileIPv6))
				throw new NetworkInformationException ();

			string file = StatisticsFileIPv6;

			string head = item;
			using (StreamReader sr = new StreamReader (file, Encoding.ASCII)) {
				StringDictionary dic = new StringDictionary ();
				string s = String.Empty;
				do {
					s = sr.ReadLine ();
					if (String.IsNullOrEmpty (s))
						continue;
					if (s.Length <= head.Length || String.CompareOrdinal (s, 0, head, 0, head.Length) != 0)
						continue;
					int idx = s.IndexOfAny (wsChars, head.Length);
					if (idx < 0)
						throw CreateException (file, null);
					dic [s.Substring (head.Length, idx - head.Length)] = s.Substring (idx + 1).Trim (wsChars);
				} while (!sr.EndOfStream);

				return dic;
			}
		}

		static readonly char [] wsChars = new char [] {' ', '\t'};

		Exception CreateException (string file, string msg)
		{
			return new InvalidOperationException (String.Format ("Unsupported (unexpected) '{0}' file format. ", file) + msg);
		}

		public override TcpConnectionInformation [] GetActiveTcpConnections ()
		{
			throw new NotImplementedException ();
		}

		public override IPEndPoint [] GetActiveTcpListeners ()
		{
			throw new NotImplementedException ();
		}

		public override IPEndPoint [] GetActiveUdpListeners ()
		{
			throw new NotImplementedException ();
		}

		public override IcmpV4Statistics GetIcmpV4Statistics ()
		{
			return new MibIcmpV4Statistics (GetProperties4 ("Icmp"));
		}

		public override IcmpV6Statistics GetIcmpV6Statistics ()
		{
			return new MibIcmpV6Statistics (GetProperties6 ("Icmp6"));
		}

		public override IPGlobalStatistics GetIPv4GlobalStatistics ()
		{
			return new MibIPGlobalStatistics (GetProperties4 ("Ip"));
		}

		public override IPGlobalStatistics GetIPv6GlobalStatistics ()
		{
			return new MibIPGlobalStatistics (GetProperties6 ("Ip6"));
		}

		public override TcpStatistics GetTcpIPv4Statistics ()
		{
			return new MibTcpStatistics (GetProperties4 ("Tcp"));
		}

		public override TcpStatistics GetTcpIPv6Statistics ()
		{
			// There is no TCP info in /proc/net/snmp,
			// so it is shared with IPv4 info.
			return new MibTcpStatistics (GetProperties4 ("Tcp"));
		}

		public override UdpStatistics GetUdpIPv4Statistics ()
		{
			return new MibUdpStatistics (GetProperties4 ("Udp"));
		}

		public override UdpStatistics GetUdpIPv6Statistics ()
		{
			return new MibUdpStatistics (GetProperties6 ("Udp6"));
		}

		public override string DhcpScopeName {
			get { throw new NotImplementedException (); }
		}

		public override string DomainName {
			get { throw new NotImplementedException (); }
		}

		public override string HostName {
			get { throw new NotImplementedException (); }
		}

		public override bool IsWinsProxy {
			get { throw new NotImplementedException (); }
		}

		public override NetBiosNodeType NodeType {
			get { throw new NotImplementedException (); }
		}
	}

	class Win32IPGlobalProperties : IPGlobalProperties
	{
		public const int AF_INET = 2;
		public const int AF_INET6 = 23;

		// FIXME: fails at some marshaling stage
		void FillTcpTable (out Win32_MIB_TCPTABLE tab4, out Win32_MIB_TCP6TABLE tab6)
		{
			tab4 = null;
			int size4 = 0;
			GetTcpTable (ref tab4, ref size4, false); // get size
			tab4 = new Win32_MIB_TCPTABLE (size4);
			GetTcpTable (ref tab4, ref size4, false); // get list
			tab6 = null;
			int size6 = 0;
			GetTcp6Table (ref tab6, ref size6, false); // get size
			tab6 = new Win32_MIB_TCP6TABLE (size6);
			GetTcp6Table (ref tab6, ref size6, false); // get list

		}

		public override TcpConnectionInformation [] GetActiveTcpConnections ()
		{
			Win32_MIB_TCPTABLE tab4 = null;
			Win32_MIB_TCP6TABLE tab6 = null;
			FillTcpTable (out tab4, out tab6);
			int size4 = tab4.Table.Length;

			TcpConnectionInformation [] ret = new TcpConnectionInformation [size4 + tab6.Table.Length];
			for (int i = 0; i < size4; i++)
				ret [i] = tab4.Table [i].TcpInfo;
			for (int i = 0; i < tab6.Table.Length; i++)
				ret [size4 + i] = tab6.Table [i].TcpInfo;
			return ret;
		}

		public override IPEndPoint [] GetActiveTcpListeners ()
		{
			Win32_MIB_TCPTABLE tab4 = null;
			Win32_MIB_TCP6TABLE tab6 = null;
			FillTcpTable (out tab4, out tab6);
			int size4 = tab4.Table.Length;

			IPEndPoint [] ret = new IPEndPoint [size4 + tab6.Table.Length];
			for (int i = 0; i < size4; i++)
				ret [i] = tab4.Table [i].LocalEndPoint;
			for (int i = 0; i < tab6.Table.Length; i++)
				ret [size4 + i] = tab6.Table [i].LocalEndPoint;
			return ret;
		}

		// FIXME: fails at some marshaling stage
		public override IPEndPoint [] GetActiveUdpListeners ()
		{
			Win32_MIB_UDPTABLE tab4 = null;
			int size4 = 0;
			GetUdpTable (ref tab4, ref size4, false); // get size
			tab4 = new Win32_MIB_UDPTABLE (size4);
			GetUdpTable (ref tab4, ref size4, false); // get list
			Win32_MIB_UDP6TABLE tab6 = null;
			int size6 = 0;
			GetUdp6Table (ref tab6, ref size6, false); // get size
			tab6 = new Win32_MIB_UDP6TABLE (size6);
			GetUdp6Table (ref tab6, ref size6, false); // get list

			IPEndPoint [] ret = new IPEndPoint [size4 + size6];
			for (int i = 0; i < size4; i++)
				ret [i] = tab4.Table [i].LocalEndPoint;
			for (int i = 0; i < size6; i++)
				ret [size4 + i] = tab6.Table [i].LocalEndPoint;
			return ret;
		}

		public override IcmpV4Statistics GetIcmpV4Statistics ()
		{
			if (!Socket.SupportsIPv4)
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
			if (!Socket.SupportsIPv4)
				throw new NetworkInformationException ();
			Win32_MIB_IPSTATS stats;
			GetIPStatisticsEx (out stats, AF_INET);
			return new Win32IPGlobalStatistics (stats);
		}

		public override IPGlobalStatistics GetIPv6GlobalStatistics ()
		{
			if (!Socket.OSSupportsIPv6)
				throw new NetworkInformationException ();
			Win32_MIB_IPSTATS stats;
			GetIPStatisticsEx (out stats, AF_INET6);
			return new Win32IPGlobalStatistics (stats);
		}

		public override TcpStatistics GetTcpIPv4Statistics ()
		{
			if (!Socket.SupportsIPv4)
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
			if (!Socket.SupportsIPv4)
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
			get { throw new NotImplementedException (); }
		}

		public override string DomainName {
			get { throw new NotImplementedException (); }
		}

		public override string HostName {
			get { throw new NotImplementedException (); }
		}

		public override bool IsWinsProxy {
			get { throw new NotImplementedException (); }
		}

		public override NetBiosNodeType NodeType {
			get { throw new NotImplementedException (); }
		}

		// PInvokes

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcpTable (ref Win32_MIB_TCPTABLE pTcpTable, ref int pdwSize, bool bOrder);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcp6Table (ref Win32_MIB_TCP6TABLE TcpTable, ref int SizePointer, bool Order);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdpTable (ref Win32_MIB_UDPTABLE pUdpTable, ref int pdwSize, bool bOrder);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdp6Table (ref Win32_MIB_UDP6TABLE Udp6Table, ref int SizePointer, bool Order);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcpStatisticsEx (out Win32_MIB_TCPSTATS pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdpStatisticsEx (out Win32_MIB_UDPSTATS pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIcmpStatistics (out Win32_MIBICMPINFO pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIcmpStatisticsEx (out Win32_MIB_ICMP_EX pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIPStatisticsEx (out Win32_MIB_IPSTATS pStats, int dwFamily);

		// Win32 structures

		[StructLayout (LayoutKind.Explicit)]
		struct Win32_IN6_ADDR
		{
			[FieldOffset (0)]
			[MarshalAs ((short) UnmanagedType.U1, SizeConst = 16)]
			public byte [] Bytes;
			[FieldOffset (0)]
			[MarshalAs ((short) UnmanagedType.U2, SizeConst = 8)]
			public byte [] UInt16Array;
			[FieldOffset (0)]
			[MarshalAs ((short) UnmanagedType.U4, SizeConst = 4)]
			public byte [] UInt32Array;
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_TCPTABLE
		{
			public int NumEntries;
			// FIXME: looks like it is wrong
			[MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_USERDEFINED, SafeArrayUserDefinedSubType = typeof (Win32_MIB_TCPROW))]
			public Win32_MIB_TCPROW [] Table;

			public Win32_MIB_TCPTABLE (int size)
			{
				NumEntries = size;
				Table = new Win32_MIB_TCPROW [size];
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		struct Win32_MIB_TCPROW
		{
			public TcpState State;
			public uint LocalAddr;
			public int LocalPort;
			public uint RemoteAddr;
			public int RemotePort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (LocalAddr, LocalPort); }
			}

			public IPEndPoint RemoteEndPoint {
				get { return new IPEndPoint (RemoteAddr, RemotePort); }
			}

			public TcpConnectionInformation TcpInfo {
				get { return new TcpConnectionInformationImpl (LocalEndPoint, RemoteEndPoint, State); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_TCP6TABLE
		{
			public int NumEntries;
			// FIXME: looks like it is wrong
			[MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_USERDEFINED, SafeArrayUserDefinedSubType = typeof (Win32_MIB_TCP6ROW))]
			public Win32_MIB_TCP6ROW [] Table;

			public Win32_MIB_TCP6TABLE (int size)
			{
				NumEntries = size;
				Table = new Win32_MIB_TCP6ROW [size];
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		struct Win32_MIB_TCP6ROW
		{
			public TcpState State;
			public Win32_IN6_ADDR LocalAddr;
			public uint LocalScopeId;
			public int LocalPort;
			public Win32_IN6_ADDR RemoteAddr;
			public uint RemoteScopeId;
			public int RemotePort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (new IPAddress (LocalAddr.Bytes, LocalScopeId), LocalPort); }
			}

			public IPEndPoint RemoteEndPoint {
				get { return new IPEndPoint (new IPAddress (RemoteAddr.Bytes, RemoteScopeId), RemotePort); }
			}

			public TcpConnectionInformation TcpInfo {
				get { return new TcpConnectionInformationImpl (LocalEndPoint, RemoteEndPoint, State); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_UDPTABLE
		{
			public int NumEntries;
			// FIXME: looks like it is wrong
			[MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_USERDEFINED, SafeArrayUserDefinedSubType = typeof (Win32_MIB_UDPROW))]
			public Win32_MIB_UDPROW [] Table;

			public Win32_MIB_UDPTABLE (int size)
			{
				NumEntries = size;
				Table = new Win32_MIB_UDPROW [size];
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		struct Win32_MIB_UDPROW
		{
			public uint LocalAddr;
			public int LocalPort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (LocalAddr, LocalPort); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_UDP6TABLE
		{
			public int NumEntries;
			// FIXME: looks like it is wrong
			[MarshalAs (UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_USERDEFINED, SafeArrayUserDefinedSubType = typeof (Win32_MIB_UDP6ROW))]
			public Win32_MIB_UDP6ROW [] Table;

			public Win32_MIB_UDP6TABLE (int size)
			{
				NumEntries = size;
				Table = new Win32_MIB_UDP6ROW [size];
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		struct Win32_MIB_UDP6ROW
		{
			public Win32_IN6_ADDR LocalAddr;
			public uint LocalScopeId;
			public int LocalPort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (new IPAddress (LocalAddr.Bytes, LocalScopeId), LocalPort); }
			}
		}
	}
}
#endif

