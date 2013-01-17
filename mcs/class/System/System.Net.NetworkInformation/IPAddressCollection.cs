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
	public class IPAddressCollection : ICollection<IPAddress>, IEnumerable<IPAddress>, IEnumerable {
		IList <IPAddress> list = new List<IPAddress> ();

		protected internal IPAddressCollection ()
		{
		}

		internal void SetReadOnly ()
		{
			if (!IsReadOnly)
				list = ((List<IPAddress>) list).AsReadOnly ();
		}

		public virtual void Add (IPAddress address)
		{
			if (IsReadOnly)
				throw new NotSupportedException ("The collection is read-only.");
			list.Add (address);
		}

		public virtual void Clear ()
		{
			if (IsReadOnly)
				throw new NotSupportedException ("The collection is read-only.");
			list.Clear ();
		}

		public virtual bool Contains (IPAddress address)
		{
			return list.Contains (address);
		}

		public virtual void CopyTo (IPAddress [] array, int offset)
		{
			list.CopyTo (array, offset);
		}

		public virtual IEnumerator<IPAddress> GetEnumerator ()
		{
			return ((IEnumerable<IPAddress>)list).GetEnumerator ();
		}

		public virtual bool Remove (IPAddress address)
		{
			if (IsReadOnly)
				throw new NotSupportedException ("The collection is read-only.");
			return list.Remove (address);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public virtual int Count {
			get { return list.Count; }
		}

		public virtual bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public virtual IPAddress this [int index] {
			get { return list [index]; }
		}
	}

	class Win32IPAddressCollection : IPAddressCollection
	{
		public static readonly Win32IPAddressCollection Empty = new Win32IPAddressCollection (IntPtr.Zero);

		bool is_readonly;

		// for static methods
		Win32IPAddressCollection ()
		{
		}

		public Win32IPAddressCollection (params IntPtr [] heads)
		{
			foreach (IntPtr head in heads)
				AddSubsequentlyString (head);
			is_readonly = true;
		}

		public Win32IPAddressCollection (params Win32_IP_ADDR_STRING [] al)
		{
			foreach (Win32_IP_ADDR_STRING a in al) {
				if (String.IsNullOrEmpty (a.IpAddress))
					continue;
				Add (IPAddress.Parse (a.IpAddress));
				AddSubsequentlyString (a.Next);
			}
			is_readonly = true;
		}

		public static Win32IPAddressCollection FromAnycast (IntPtr ptr)
		{
			Win32IPAddressCollection c = new Win32IPAddressCollection ();
			Win32_IP_ADAPTER_ANYCAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_ANYCAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_ANYCAST_ADDRESS));
				c.Add (a.Address.GetIPAddress ());
			}
			c.is_readonly = true;
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
				c.Add (a.Address.GetIPAddress ());
			}
			c.is_readonly = true;
			return c;
		}

		void AddSubsequentlyString (IntPtr head)
		{
			Win32_IP_ADDR_STRING a;
			for (IntPtr p = head; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADDR_STRING) Marshal.PtrToStructure (p, typeof (Win32_IP_ADDR_STRING));
				Add (IPAddress.Parse (a.IpAddress));
			}
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
		}
	}
}


