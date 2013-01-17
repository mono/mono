//
// System.Net.NetworkInformation.IPAddressInformationCollection
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
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public class IPAddressInformationCollection : ICollection<IPAddressInformation>, IEnumerable<IPAddressInformation>, IEnumerable {
		List <IPAddressInformation> list = new List <IPAddressInformation> ();

		internal IPAddressInformationCollection ()
		{
		}

		public virtual void Add (IPAddressInformation address)
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

		public virtual bool Contains (IPAddressInformation address)
		{
			return list.Contains (address);
		}

		public virtual void CopyTo (IPAddressInformation [] array, int offset)
		{
			list.CopyTo (array, offset);
		}

		public virtual IEnumerator<IPAddressInformation> GetEnumerator ()
		{
			return ((IEnumerable<IPAddressInformation>)list).GetEnumerator ();
		}

		public virtual bool Remove (IPAddressInformation address)
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
			get { return true; }
		}

		public virtual IPAddressInformation this [int index] {
			get { return list [index]; }
		}
	}

	class IPAddressInformationImplCollection : IPAddressInformationCollection
	{
		public static readonly IPAddressInformationImplCollection Empty = new IPAddressInformationImplCollection (true);

		bool is_readonly;

		// for static methods
		IPAddressInformationImplCollection (bool isReadOnly)
		{
			is_readonly = isReadOnly;
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
		}

		public static IPAddressInformationCollection Win32FromAnycast (IntPtr ptr)
		{
			IPAddressInformationImplCollection c = new IPAddressInformationImplCollection (false);
			Win32_IP_ADAPTER_ANYCAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_ANYCAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_ANYCAST_ADDRESS));
				c.Add (new IPAddressInformationImpl (
				       a.Address.GetIPAddress (),
				       a.LengthFlags.IsDnsEligible,
				       a.LengthFlags.IsTransient));
			}
			c.is_readonly = true;
			return c;
		}

		public static IPAddressInformationImplCollection LinuxFromAnycast (IList<IPAddress> addresses)
		{
			IPAddressInformationImplCollection c = new IPAddressInformationImplCollection (false);
			foreach (IPAddress address in addresses) {
				c.Add (new IPAddressInformationImpl (address, false, false));
			}
			c.is_readonly = true;
			return c;
		}
	}
}

