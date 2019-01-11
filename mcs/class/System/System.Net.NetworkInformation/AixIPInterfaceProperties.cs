//
// System.Net.NetworkInformation.IPInterfaceProperties
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
using System.Runtime.CompilerServices;

namespace System.Net.NetworkInformation {
	class AixIPInterfaceProperties : UnixIPInterfaceProperties
	{
		private int _mtu;

		public AixIPInterfaceProperties (AixNetworkInterface iface, List <IPAddress> addresses)
			: this (iface, addresses, 0)
		{
		}

		public AixIPInterfaceProperties (AixNetworkInterface iface, List <IPAddress> addresses, int mtu)
			: base (iface, addresses)
		{
			_mtu = mtu;
		}

		public override IPv4InterfaceProperties GetIPv4Properties ()
		{
			if (ipv4iface_properties == null)
				ipv4iface_properties = new AixIPv4InterfaceProperties (iface as AixNetworkInterface, _mtu);

			return ipv4iface_properties;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool ParseRouteInfo_internal(string iface, out string[] gw_addr_list);

		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				var gateways = new IPAddressCollection ();
				string[] gw_addrlist;
				if (!ParseRouteInfo_internal (this.iface.Name.ToString(), out gw_addrlist))
					return new GatewayIPAddressInformationCollection ();

				for(int i=0; i<gw_addrlist.Length; i++) {
					try {
						IPAddress ip = IPAddress.Parse(gw_addrlist[i]);
						if (!ip.Equals (IPAddress.Any) && !gateways.Contains (ip))
							gateways.InternalAdd (ip);
					} catch (ArgumentNullException) {
						/* Ignore this, as the
						 * internal call might have
						 * left some blank entries at
						 * the end of the array
						 */
					}
				}

				return SystemGatewayIPAddressInformation.ToGatewayIpAddressInformationCollection (gateways);
			}
		}
	}
}

