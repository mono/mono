//
// System.Net.NetworkInformation.IPGlobalProperties
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
namespace System.Net.NetworkInformation {
	public abstract class IPGlobalProperties {
		protected IPGlobalProperties ()
		{
		}

		[MonoTODO]
		public static IPGlobalProperties GetIPGlobalProperties ()
		{
			throw new NotImplementedException ();
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
}
#endif

