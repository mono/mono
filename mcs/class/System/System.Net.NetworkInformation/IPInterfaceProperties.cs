//
// System.Net.NetworkInformation.IPInterfaceProperties
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
	public abstract class IPInterfaceProperties {
		protected IPInterfaceProperties ()
		{
		}

		public abstract IPv4InterfaceProperties GetIPv4Properties ();
		public abstract IPv6InterfaceProperties GetIPv6Properties ();

		public abstract IPAddressInformationCollection AnycastAddresses { get; }
		public abstract IPAddressCollection DhcpServerAddresses { get; }
		public abstract IPAddressCollection DnsAddresses { get; }
		public abstract string DnsSuffix { get; }
		public abstract GatewayIPAddressInformationCollection GatewayAddresses { get; }
		public abstract bool IsDnsEnabled { get; }
		public abstract bool IsDynamicDnsEnabled { get; }
		public abstract MulticastIPAddressInformationCollection MulticastAddresses { get; }
		public abstract UnicastIPAddressInformationCollection UnicastAddresses { get; }
		public abstract IPAddressCollection WinsServersAddresses { get; }
	}
}
#endif

