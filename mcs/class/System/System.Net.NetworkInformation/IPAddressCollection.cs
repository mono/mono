//
// System.Net.NetworkInformation.IPAddressCollection
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {

#if WIN_PLATFORM
	class Win32IPAddressCollection : IPAddressCollection
	{
		public static readonly Win32IPAddressCollection Empty = new Win32IPAddressCollection (IntPtr.Zero);

		// for static methods
		Win32IPAddressCollection ()
		{
		}

		public Win32IPAddressCollection (params IntPtr [] heads)
		{
			foreach (IntPtr head in heads)
				AddSubsequentlyString (head);
		}

		public Win32IPAddressCollection (params Win32_IP_ADDR_STRING [] al)
		{
			foreach (Win32_IP_ADDR_STRING a in al) {
				if (String.IsNullOrEmpty (a.IpAddress))
					continue;
				InternalAdd (IPAddress.Parse (a.IpAddress));
				AddSubsequentlyString (a.Next);
			}
		}

		public static Win32IPAddressCollection FromAnycast (IntPtr ptr)
		{
			Win32IPAddressCollection c = new Win32IPAddressCollection ();
			Win32_IP_ADAPTER_ANYCAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_ANYCAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_ANYCAST_ADDRESS));
				c.InternalAdd (a.Address.GetIPAddress ());
			}
			return c;
		}

		public static Win32IPAddressCollection FromDnsServer (IntPtr ptr)
		{
			Win32IPAddressCollection c = new Win32IPAddressCollection ();
			Win32_IP_ADAPTER_DNS_SERVER_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_DNS_SERVER_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_DNS_SERVER_ADDRESS));
// FIXME: It somehow fails here. Looks like there is something wrong.
//if (a.Address.Sockaddr == IntPtr.Zero) throw new Exception ("pointer " + p + " a.length " + a.Address.SockaddrLength);
				c.InternalAdd (a.Address.GetIPAddress ());
			}
			return c;
		}

		public static Win32IPAddressCollection FromSocketAddress (Win32_SOCKET_ADDRESS addr)
		{
			Win32IPAddressCollection c = new Win32IPAddressCollection ();
			if (addr.Sockaddr != IntPtr.Zero)
				c.InternalAdd (addr.GetIPAddress ());
			return c;
		}

		public static Win32IPAddressCollection FromWinsServer (IntPtr ptr)
		{
			Win32IPAddressCollection c = new Win32IPAddressCollection ();
			Win32_IP_ADAPTER_WINS_SERVER_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_WINS_SERVER_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_WINS_SERVER_ADDRESS));
				c.InternalAdd (a.Address.GetIPAddress ());
			}
			return c;
		}

		void AddSubsequentlyString (IntPtr head)
		{
			Win32_IP_ADDR_STRING a;
			for (IntPtr p = head; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADDR_STRING) Marshal.PtrToStructure (p, typeof (Win32_IP_ADDR_STRING));
				InternalAdd (IPAddress.Parse (a.IpAddress));
			}
		}
	}
#endif
}


