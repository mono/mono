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
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace System.Net {
#if NET_2_0
	public static class Dns {
#else
	public sealed class Dns {

		private Dns () {}
#endif

		static Dns ()
		{
			System.Net.Sockets.Socket.CheckProtocolSupport();
		}

		private delegate IPHostEntry GetHostByNameCallback (string hostName);
		private delegate IPHostEntry ResolveCallback (string hostName);

		public static IAsyncResult BeginGetHostByName (string hostName,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");

			GetHostByNameCallback c = new GetHostByNameCallback (GetHostByName);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
		}

		public static IAsyncResult BeginResolve (string hostName,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");

			ResolveCallback c = new ResolveCallback (Resolve);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
		}

		public static IPHostEntry EndGetHostByName (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult async = (AsyncResult) asyncResult;
			GetHostByNameCallback cb = (GetHostByNameCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}

		public static IPHostEntry EndResolve (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			ResolveCallback cb = (ResolveCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostByAddr_internal(string addr, out string h_name, out string[] h_aliases, out string[] h_addr_list);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostName_internal(out string h_name);
		
		private static IPHostEntry hostent_to_IPHostEntry(string h_name, string[] h_aliases, string[] h_addrlist) 
		{
			IPHostEntry he = new IPHostEntry();
			ArrayList addrlist = new ArrayList();

			he.HostName = h_name;
			he.Aliases = h_aliases;
			for(int i=0; i<h_addrlist.Length; i++) {
				IPAddress newAddress = IPAddress.Parse(h_addrlist[i]);

				if( (Socket.SupportsIPv6 && newAddress.AddressFamily == AddressFamily.InterNetworkV6) ||
					(Socket.SupportsIPv4 && newAddress.AddressFamily == AddressFamily.InterNetwork) )
					addrlist.Add(newAddress);
			}

			if(addrlist.Count == 0)
				throw new SocketException(11001);

			he.AddressList = addrlist.ToArray(typeof(IPAddress)) as IPAddress[];
			return he;
		}

		public static IPHostEntry GetHostByAddress(IPAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return GetHostByAddressFromString (address.ToString (), false);
		}

		public static IPHostEntry GetHostByAddress(string address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return GetHostByAddressFromString (address, true);
		}

		static IPHostEntry GetHostByAddressFromString (string address, bool parse)
		{
			// Undocumented MS behavior: when called with IF_ANY,
			// this should return the local host
			if (address.Equals ("0.0.0.0")) {
				address = "127.0.0.1";
				parse = false;
			}

			// Must check the IP format, might send an exception if invalid string.
			if (parse)
				IPAddress.Parse(address);

			string h_name;
			string[] h_aliases, h_addrlist;

			bool ret = GetHostByAddr_internal(address, out h_name, out h_aliases, out h_addrlist);
			if (!ret)
				throw new SocketException(11001);

			return (hostent_to_IPHostEntry (h_name, h_aliases, h_addrlist));
		}

		public static IPHostEntry GetHostByName(string hostName) 
		{
			if (hostName == null)
				throw new ArgumentNullException();

			string h_name;
			string[] h_aliases, h_addrlist;

			bool ret = GetHostByName_internal(hostName, out h_name,
				out h_aliases,
				out h_addrlist);
			if (ret == false)
				throw new SocketException(11001);

			return(hostent_to_IPHostEntry(h_name, h_aliases,
				h_addrlist));
		}

		public static string GetHostName() 
		{
			string hostName;

			bool ret = GetHostName_internal(out hostName);

			if (ret == false)
				throw new SocketException(11001);

			return hostName;
		}

		public static IPHostEntry Resolve(string hostName) 
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");

			IPHostEntry ret = null;

			try {
				ret =  GetHostByAddress(hostName);
			}
			catch{}

			if(ret == null)
				ret =  GetHostByName(hostName);

			return ret;
		}
	}
}

