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

#if !MOONLIGHT // global remove of async methods

		private delegate IPHostEntry GetHostByNameCallback (string hostName);
		private delegate IPHostEntry ResolveCallback (string hostName);
#if NET_2_0
		private delegate IPHostEntry GetHostEntryNameCallback (string hostName);
		private delegate IPHostEntry GetHostEntryIPCallback (IPAddress hostAddress);
		private delegate IPAddress [] GetHostAddressesCallback (string hostName);
#endif

#if NET_2_0
		[Obsolete ("Use BeginGetHostEntry instead")]
#endif
		public static IAsyncResult BeginGetHostByName (string hostName,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");

			GetHostByNameCallback c = new GetHostByNameCallback (GetHostByName);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
		}

#if NET_2_0
		[Obsolete ("Use BeginGetHostEntry instead")]
#endif
		public static IAsyncResult BeginResolve (string hostName,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");

			ResolveCallback c = new ResolveCallback (Resolve);
			return c.BeginInvoke (hostName, requestCallback, stateObject);
		}

#if NET_2_0
		public static IAsyncResult BeginGetHostAddresses (string hostNameOrAddress,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostNameOrAddress == null)
				throw new ArgumentNullException ("hostName");
			if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
				throw new ArgumentException ("Addresses 0.0.0.0 (IPv4) " +
					"and ::0 (IPv6) are unspecified addresses. You " +
					"cannot use them as target address.",
					"hostNameOrAddress");

			GetHostAddressesCallback c = new GetHostAddressesCallback (GetHostAddresses);
			return c.BeginInvoke (hostNameOrAddress, requestCallback, stateObject);
		}

		public static IAsyncResult BeginGetHostEntry (string hostNameOrAddress,
			AsyncCallback requestCallback, object stateObject)
		{
			if (hostNameOrAddress == null)
				throw new ArgumentNullException ("hostName");
			if (hostNameOrAddress == "0.0.0.0" || hostNameOrAddress == "::0")
				throw new ArgumentException ("Addresses 0.0.0.0 (IPv4) " +
					"and ::0 (IPv6) are unspecified addresses. You " +
					"cannot use them as target address.",
					"hostNameOrAddress");

			GetHostEntryNameCallback c = new GetHostEntryNameCallback (GetHostEntry);
			return c.BeginInvoke (hostNameOrAddress, requestCallback, stateObject);
		}

		public static IAsyncResult BeginGetHostEntry (IPAddress address,
			AsyncCallback requestCallback, object stateObject)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			GetHostEntryIPCallback c = new GetHostEntryIPCallback (GetHostEntry);
			return c.BeginInvoke (address, requestCallback, stateObject);
		}
#endif

#if NET_2_0
		[Obsolete ("Use EndGetHostEntry instead")]
#endif
		public static IPHostEntry EndGetHostByName (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult async = (AsyncResult) asyncResult;
			GetHostByNameCallback cb = (GetHostByNameCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}

#if NET_2_0
		[Obsolete ("Use EndGetHostEntry instead")]
#endif
		public static IPHostEntry EndResolve (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
			ResolveCallback cb = (ResolveCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}

#if NET_2_0

		public static IPAddress [] EndGetHostAddresses (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult async = (AsyncResult) asyncResult;
			GetHostAddressesCallback cb = (GetHostAddressesCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}

		public static IPHostEntry EndGetHostEntry (IAsyncResult asyncResult) 
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			AsyncResult async = (AsyncResult) asyncResult;
#if NET_2_0
			if (async.AsyncDelegate is GetHostEntryIPCallback)
				return ((GetHostEntryIPCallback) async.AsyncDelegate).EndInvoke (asyncResult);
#endif
			GetHostEntryNameCallback cb = (GetHostEntryNameCallback) async.AsyncDelegate;
			return cb.EndInvoke(asyncResult);
		}
#endif
		
#endif // !MOONLIGHT: global remove of async methods

#if !TARGET_JVM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostByName_internal(string host, out string h_name, out string[] h_aliases, out string[] h_addr_list);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostByAddr_internal(string addr, out string h_name, out string[] h_aliases, out string[] h_addr_list);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool GetHostName_internal(out string h_name);
#endif	

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

#if NET_2_0
		[Obsolete ("Use GetHostEntry instead")]
#endif
		public static IPHostEntry GetHostByAddress(IPAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return GetHostByAddressFromString (address.ToString (), false);
		}

#if NET_2_0
		[Obsolete ("Use GetHostEntry instead")]
#endif
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
				IPAddress.Parse (address);

			string h_name;
			string[] h_aliases, h_addrlist;
#if TARGET_JVM
			h_name = null;
			h_aliases = null;
			h_addrlist = null;
            		try {
                		java.net.InetAddress[] iaArr = 
					java.net.InetAddress.getAllByName(address);
		                if (iaArr != null && iaArr.Length > 0)
                		    h_name = iaArr[0].getHostName();
		                if (iaArr != null && iaArr.Length > 0)
                		{
		                    h_addrlist = new String[iaArr.Length];
                		    for (int i = 0; i < h_addrlist.Length; i++)
		                        h_addrlist[i] = iaArr[i].getHostAddress();
                		}
            		} catch (java.net.UnknownHostException jUHE) {
		                throw new SocketException((int)SocketError.HostNotFound, jUHE.Message);
            		}
#else
			bool ret = GetHostByAddr_internal(address, out h_name, out h_aliases, out h_addrlist);
			if (!ret)
				throw new SocketException(11001);
#endif
			return (hostent_to_IPHostEntry (h_name, h_aliases, h_addrlist));
			
		}

#if NET_2_0
		public
#else
		internal
#endif
		static IPHostEntry GetHostEntry (string hostNameOrAddress)
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
				return GetHostEntry (addr);

			return GetHostByName (hostNameOrAddress);
		}

#if NET_2_0
		public
#else
		internal
#endif
		static IPHostEntry GetHostEntry (IPAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return GetHostByAddressFromString (address.ToString (), false);
		}

#if NET_2_0
		public
#else
		internal
#endif
		static IPAddress [] GetHostAddresses (string hostNameOrAddress)
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

			return GetHostEntry (hostNameOrAddress).AddressList;
		}

#if NET_2_0
		[Obsolete ("Use GetHostEntry instead")]
#endif
		public static IPHostEntry GetHostByName (string hostName)
		{
			if (hostName == null)
				throw new ArgumentNullException ("hostName");
#if TARGET_JVM
			if (hostName.Length == 0)
				hostName = "localhost";
		        try {
				java.net.InetAddress[] iaArr = java.net.InetAddress.getAllByName(hostName);
				IPHostEntry host = new IPHostEntry();
				if (iaArr != null && iaArr.Length > 0)
                		{
					host.HostName = iaArr[0].getHostName();
                		    	IPAddress[] ipArr = new IPAddress[iaArr.Length];
		                    	for (int i = 0; i < iaArr.Length; i++)
                		        	ipArr[i] = IPAddress.Parse(iaArr[i].getHostAddress());

					host.AddressList = ipArr;
                		}
		                return host;
			} catch (java.net.UnknownHostException jUHE) {
				throw new SocketException((int)SocketError.HostNotFound, jUHE.Message);
			}
#else
			string h_name;
			string[] h_aliases, h_addrlist;

			bool ret = GetHostByName_internal(hostName, out h_name, out h_aliases, out h_addrlist);
			if (ret == false)
				throw new SocketException(11001);

			return(hostent_to_IPHostEntry(h_name, h_aliases, h_addrlist));
#endif
		}

		public static string GetHostName ()
		{
#if TARGET_JVM
			return java.net.InetAddress.getLocalHost ().getHostName ();
#else
			string hostName;

			bool ret = GetHostName_internal(out hostName);

			if (ret == false)
				throw new SocketException(11001);

			return hostName;
#endif
		}

#if NET_2_0
		[Obsolete ("Use GetHostEntry instead")]
#endif
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

