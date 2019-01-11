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
using System.Globalization;
using System.IO;

namespace System.Net.NetworkInformation {
	class LinuxIPInterfaceProperties : UnixIPInterfaceProperties
	{
		public LinuxIPInterfaceProperties (LinuxNetworkInterface iface, List <IPAddress> addresses)
			: base (iface, addresses)
		{
		}

		public override IPv4InterfaceProperties GetIPv4Properties ()
		{
			if (ipv4iface_properties == null)
				ipv4iface_properties = new LinuxIPv4InterfaceProperties (iface as LinuxNetworkInterface);

			return ipv4iface_properties;
		}

		IPAddressCollection ParseRouteInfo (string iface)
		{
			var col = new IPAddressCollection ();
			try {
				using (StreamReader reader = new StreamReader ("/proc/net/route")) {
					string line;
					reader.ReadLine (); // Ignore first line
					while ((line = reader.ReadLine ()) != null) {
						line = line.Trim ();
						if (line.Length == 0)
							continue;

						string [] parts = line.Split ('\t');
						if (parts.Length < 3)
							continue;
						string gw_address = parts [2].Trim ();
						byte [] ipbytes = new byte [4];
						if (gw_address.Length == 8 && iface.Equals (parts [0], StringComparison.OrdinalIgnoreCase)) {
							for (int i = 0; i < 4; i++) {
								if (!Byte.TryParse (gw_address.Substring (i * 2, 2), NumberStyles.HexNumber, null, out ipbytes [3 - i]))
									continue;
							}
							IPAddress ip = new IPAddress (ipbytes);
							if (!ip.Equals (IPAddress.Any) && !col.Contains (ip))
								col.InternalAdd (ip);
						}
					}
				}
			} catch {
			}

			return col;
		}

		public override GatewayIPAddressInformationCollection GatewayAddresses {
			get {
				return SystemGatewayIPAddressInformation.ToGatewayIpAddressInformationCollection (ParseRouteInfo (this.iface.Name.ToString()));
			}
		}
	}
}
