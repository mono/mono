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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation {
	abstract class CommonUnixIPGlobalProperties : IPGlobalProperties
	{
		[DllImport ("libc")]
		static extern int gethostname ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] name, int len);

#if !ORBIS
		[DllImport ("libc")]
		static extern int getdomainname ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] name, int len);
#else
		static int getdomainname ([MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 1)] byte [] name, int len)
		{
			throw new PlatformNotSupportedException ();
		}
#endif

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

	class UnixIPGlobalProperties : CommonUnixIPGlobalProperties
	{
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
			throw new NotImplementedException ();
		}

		public override IcmpV6Statistics GetIcmpV6Statistics ()
		{
			throw new NotImplementedException ();
		}

		public override IPGlobalStatistics GetIPv4GlobalStatistics ()
		{
			throw new NotImplementedException ();
		}

		public override IPGlobalStatistics GetIPv6GlobalStatistics ()
		{
			throw new NotImplementedException ();
		}

		public override TcpStatistics GetTcpIPv4Statistics ()
		{
			throw new NotImplementedException ();
		}

		public override TcpStatistics GetTcpIPv6Statistics ()
		{
			throw new NotImplementedException ();
		}

		public override UdpStatistics GetUdpIPv4Statistics ()
		{
			throw new NotImplementedException ();
		}

		public override UdpStatistics GetUdpIPv6Statistics ()
		{
			throw new NotImplementedException ();
		}
	}

#if MONODROID
	sealed class AndroidIPGlobalProperties : UnixIPGlobalProperties
	{
		public override string DomainName {
			get {
				return String.Empty;
			}
		}
	}
#endif

	// It expects /proc/net/snmp (or /usr/compat/linux/proc/net/snmp),
	// formatted like:
	// http://www.linuxdevcenter.com/linux/2000/11/16/example5.html
	// http://www.linuxdevcenter.com/linux/2000/11/16/example2.html
	class MibIPGlobalProperties : UnixIPGlobalProperties
	{
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

		static TcpState UnixTcpStateToTcpState (int unixState)
		{
			//The values of these states in Linux are listed here:
			//https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/tree/include/net/tcp_states.h?id=HEAD
			switch (unixState) {
			case 1:
				return TcpState.Established;
			case 2:
				return TcpState.SynSent;
			case 3:
				return TcpState.SynReceived;
			case 4:
				return TcpState.FinWait1;
			case 5:
				return TcpState.FinWait2;
			case 6:
				return TcpState.TimeWait;
			case 7:
				return TcpState.Closed;
			case 8:
				return TcpState.CloseWait;
			case 9:
				return TcpState.LastAck;
			case 10:
				return TcpState.Listen;
			case 11:
				return TcpState.Closing;
			default:
				return TcpState.Unknown;
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
				TcpState state = UnixTcpStateToTcpState (int.Parse (list [i] [3], NumberStyles.HexNumber));
				ret [i] = new SystemTcpConnectionInformation (local, remote, state);
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
	}

	internal static class UnixIPGlobalPropertiesFactoryPal {
		public static IPGlobalProperties Create ()
		{
#if MONODROID
			return new AndroidIPGlobalProperties ();
#elif MONOTOUCH || XAMMAC
			return new UnixIPGlobalProperties ();
#elif MONO
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
				return new UnixIPGlobalProperties ();
			default:
#if !WIN_PLATFORM
				return new UnixIPGlobalProperties ();
#endif
				return null;
		}
#else
			(new NetworkInformationPermission (NetworkInformationAccess.Read)).Demand ();
			return new SystemIPGlobalProperties ();
#endif
		}
	}
}
