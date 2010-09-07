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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation {
	public abstract class IPGlobalProperties {
		protected IPGlobalProperties ()
		{
		}

		public static IPGlobalProperties GetIPGlobalProperties ()
		{
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
				MibIPGlobalProperties impl = null;
				if (Directory.Exists (MibIPGlobalProperties.ProcDir)) {
					impl = new MibIPGlobalProperties (MibIPGlobalProperties.ProcDir);
					if (File.Exists (impl.StatisticsFile))
						return impl;
				}
				if (Directory.Exists (MibIPGlobalProperties.CompatProcDir)) {
					impl = new MibIPGlobalProperties (MibIPGlobalProperties.CompatProcDir);
					if (File.Exists (impl.StatisticsFile))
						return impl;
				}
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

	// It expects /proc/net/snmp (or /usr/compat/linux/proc/net/snmp),
	// formatted like:
	// http://www.linuxdevcenter.com/linux/2000/11/16/example5.html
	// http://www.linuxdevcenter.com/linux/2000/11/16/example2.html
	class MibIPGlobalProperties : IPGlobalProperties
	{
		[DllImport ("libc")]
		static extern int gethostname ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] name, int len);

		[DllImport ("libc")]
		static extern int getdomainname ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] name, int len);

		public const string ProcDir = "/proc";
		public const string CompatProcDir = "/usr/compat/linux/proc";

		public readonly string StatisticsFile, StatisticsFileIPv6, TcpFile, Tcp6File, UdpFile, Udp6File;

		public MibIPGlobalProperties (string procDir)
		{
			StatisticsFile = Path.Combine (procDir, "net/snmp");
			StatisticsFileIPv6 = Path.Combine (procDir, "net/snmp6");
			TcpFile = Path.Combine (procDir,"net/tcp");
			Tcp6File = Path.Combine (procDir,"net/tcp6");
			UdpFile = Path.Combine (procDir,"net/udp");
			Udp6File = Path.Combine (procDir,"net/udp6");
		}

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
		IPEndPoint [] GetLocalAddresses (List<string []> list)
		{
			IPEndPoint [] ret = new IPEndPoint [list.Count];
			for (int i = 0; i < ret.Length; i++)
				ret [i] = ToEndpoint (list [i] [1]);
			return ret;
		}

		IPEndPoint ToEndpoint (string s)
		{
			int idx = s.IndexOf (':');
			int port = int.Parse (s.Substring (idx + 1), NumberStyles.HexNumber);
			if (s.Length == 13)
				return new IPEndPoint (long.Parse (s.Substring (0, idx), NumberStyles.HexNumber), port);
			else {
				byte [] bytes = new byte [16];
				for (int i = 0; (i << 1) < idx; i++)
					bytes [i] = byte.Parse (s.Substring (i << 1, 2), NumberStyles.HexNumber);
				return new IPEndPoint (new IPAddress (bytes), port);
			}
		}

		void GetRows (string file, List<string []> list)
		{
			if (!File.Exists (file))
				return;
			using (StreamReader sr = new StreamReader (file, Encoding.ASCII)) {
				sr.ReadLine (); // skip first line
				while (!sr.EndOfStream) {
					string [] item = sr.ReadLine ().Split (wsChars, StringSplitOptions.RemoveEmptyEntries);
					if (item.Length < 4)
						throw CreateException (file, null);
					list.Add (item);
				}
			}
		}

		public override TcpConnectionInformation [] GetActiveTcpConnections ()
		{
			List<string []> list = new List<string []> ();
			GetRows (TcpFile, list);
			GetRows (Tcp6File, list);

			TcpConnectionInformation [] ret = new TcpConnectionInformation [list.Count];
			for (int i = 0; i < ret.Length; i++) {
				// sl  local_address rem_address   st tx_queue rx_queue tr tm->when retrnsmt   uid  timeout inode
				IPEndPoint local = ToEndpoint (list [i] [1]);
				IPEndPoint remote = ToEndpoint (list [i] [2]);
				TcpState state = (TcpState) int.Parse (list [i] [3], NumberStyles.HexNumber);
				ret [i] = new TcpConnectionInformationImpl (local, remote, state);
			}
			return ret;
		}

		public override IPEndPoint [] GetActiveTcpListeners ()
		{
			List<string []> list = new List<string []> ();
			GetRows (TcpFile, list);
			GetRows (Tcp6File, list);
			return GetLocalAddresses (list);
		}

		public override IPEndPoint [] GetActiveUdpListeners ()
		{
			List<string []> list = new List<string []> ();
			GetRows (UdpFile, list);
			GetRows (Udp6File, list);
			return GetLocalAddresses (list);
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
			get { return String.Empty; }
		}

		public override string DomainName {
			get {
				byte [] bytes = new byte [256];
				if (getdomainname (bytes, 256) != 0)
					throw new NetworkInformationException ();
				int len = Array.IndexOf<byte> (bytes, 0);
				return Encoding.ASCII.GetString (bytes, 0, len < 0 ? 256 : len);
			}
		}

		public override string HostName {
			get {
				byte [] bytes = new byte [256];
				if (gethostname (bytes, 256) != 0)
					throw new NetworkInformationException ();
				int len = Array.IndexOf<byte> (bytes, 0);
				return Encoding.ASCII.GetString (bytes, 0, len < 0 ? 256 : len);
			}
		}

		public override bool IsWinsProxy {
			get { return false; } // no WINS
		}

		public override NetBiosNodeType NodeType {
			get { return NetBiosNodeType.Unknown; } // no NetBios
		}
	}

	class Win32IPGlobalProperties : IPGlobalProperties
	{
		public const int AF_INET = 2;
		public const int AF_INET6 = 23;

		// FIXME: it might be getting wrong table. I'm getting
		// different results from .NET 2.0.
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
			get { return Win32_FIXED_INFO.Instance.ScopeId; }
		}

		public override string DomainName {
			get { return Win32_FIXED_INFO.Instance.DomainName; }
		}

		public override string HostName {
			get { return Win32_FIXED_INFO.Instance.HostName; }
		}

		public override bool IsWinsProxy {
			get { return Win32_FIXED_INFO.Instance.EnableProxy != 0; }
		}

		public override NetBiosNodeType NodeType {
			get { return Win32_FIXED_INFO.Instance.NodeType; }
		}

		// PInvokes

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcpTable (byte [] pTcpTable, ref int pdwSize, bool bOrder);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcp6Table (byte [] TcpTable, ref int SizePointer, bool Order);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdpTable (byte [] pUdpTable, ref int pdwSize, bool bOrder);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdp6Table (byte [] Udp6Table, ref int SizePointer, bool Order);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetTcpStatisticsEx (out Win32_MIB_TCPSTATS pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetUdpStatisticsEx (out Win32_MIB_UDPSTATS pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIcmpStatistics (out Win32_MIBICMPINFO pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIcmpStatisticsEx (out Win32_MIB_ICMP_EX pStats, int dwFamily);

		[DllImport ("Iphlpapi.dll")]
		static extern int GetIpStatisticsEx (out Win32_MIB_IPSTATS pStats, int dwFamily);

		// Win32 structures

		[StructLayout (LayoutKind.Explicit)]
		struct Win32_IN6_ADDR
		{
			[FieldOffset (0)]
			[MarshalAs ((short) UnmanagedType.U1, SizeConst = 16)]
			public byte [] Bytes;
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_TCPROW
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
		class Win32_MIB_TCP6ROW
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
		class Win32_MIB_UDPROW
		{
			public uint LocalAddr;
			public int LocalPort;

			public IPEndPoint LocalEndPoint {
				get { return new IPEndPoint (LocalAddr, LocalPort); }
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		class Win32_MIB_UDP6ROW
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
