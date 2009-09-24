// System.Net.Dns.cs
//
// Author: Mads Pultz (mpultz@diku.dk)
// Author: Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Mads Pultz, 2001

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

using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace System.Net {

	internal static class Dns {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list);

		internal static IPAddress [] GetHostAddresses (string hostNameOrAddress)
		{
			if (hostNameOrAddress == null)
				throw new ArgumentNullException ("hostNameOrAddress");

			if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
				throw new ArgumentException ("Addresses 0.0.0.0 (IPv4) " +
					"and ::0 (IPv6) are unspecified addresses. You " +
					"cannot use them as target address.",
					"hostNameOrAddress");

			IPAddress addr;
			if (hostNameOrAddress.Length > 0 && IPAddress.TryParse (hostNameOrAddress, out addr))
				return new IPAddress[1] { addr };

			string h_name;
			string[] h_aliases, h_addrlist;

			bool ret = GetHostByName_internal (hostNameOrAddress, out h_name, out h_aliases, out h_addrlist);
			if (ret == false)
				throw new SocketException(11001);

			IPHostEntry entry = hostent_to_IPHostEntry (h_name, h_aliases, h_addrlist);
			return entry.AddressList;
		}

		private static IPHostEntry hostent_to_IPHostEntry(string h_name, string[] h_aliases, string[] h_addrlist) 
		{
			IPHostEntry he = new IPHostEntry();
			ArrayList addrlist = new ArrayList();

			he.HostName = h_name;
			he.Aliases = h_aliases;
			for(int i=0; i<h_addrlist.Length; i++) {
				try {
					IPAddress newAddress = IPAddress.Parse(h_addrlist[i]);

					if( (Socket.SupportsIPv6 && newAddress.AddressFamily == AddressFamily.InterNetworkV6) ||
					    (Socket.SupportsIPv4 && newAddress.AddressFamily == AddressFamily.InterNetwork) )
						addrlist.Add(newAddress);
				} catch (ArgumentNullException) {
					/* Ignore this, as the
					 * internal call might have
					 * left some blank entries at
					 * the end of the array
					 */
				}
			}

			if(addrlist.Count == 0)
				throw new SocketException(11001);

			he.AddressList = addrlist.ToArray(typeof(IPAddress)) as IPAddress[];
			return he;
		}
	}
}

