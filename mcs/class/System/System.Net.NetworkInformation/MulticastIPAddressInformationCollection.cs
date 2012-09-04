//
// System.Net.NetworkInformation.MulticastIPAddressInformationCollection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Eric Butler (eric@extremeboredom.net)
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public class MulticastIPAddressInformationCollection : ICollection<MulticastIPAddressInformation>, IEnumerable<MulticastIPAddressInformation>, IEnumerable {
		List<MulticastIPAddressInformation> list = new List<MulticastIPAddressInformation> ();
		
		protected internal MulticastIPAddressInformationCollection ()
		{
		}

		public virtual void Add (MulticastIPAddressInformation address)
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

		public virtual bool Contains (MulticastIPAddressInformation address)
		{
			return list.Contains (address);
		}

		public virtual void CopyTo (MulticastIPAddressInformation [] array, int offset)
		{
			list.CopyTo (array, offset);
		}

		public virtual IEnumerator<MulticastIPAddressInformation> GetEnumerator ()
		{
			return ((IEnumerable<MulticastIPAddressInformation>)list).GetEnumerator ();
		}

		public virtual bool Remove (MulticastIPAddressInformation address)
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

		public virtual MulticastIPAddressInformation this [int index] {
			get { return list [index]; }
		}
	}

	class MulticastIPAddressInformationImplCollection : MulticastIPAddressInformationCollection
	{
		public static readonly MulticastIPAddressInformationImplCollection Empty = new MulticastIPAddressInformationImplCollection (true);

		bool is_readonly;

		// for static methods
		MulticastIPAddressInformationImplCollection (bool isReadOnly)
		{
			is_readonly = isReadOnly;
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
		}

		public static MulticastIPAddressInformationCollection Win32FromMulticast (IntPtr ptr)
		{
			MulticastIPAddressInformationImplCollection c = new MulticastIPAddressInformationImplCollection (false);
			Win32_IP_ADAPTER_MULTICAST_ADDRESS a;
			for (IntPtr p = ptr; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADAPTER_MULTICAST_ADDRESS) Marshal.PtrToStructure (p, typeof (Win32_IP_ADAPTER_MULTICAST_ADDRESS));
				c.Add (new MulticastIPAddressInformationImpl (
				       a.Address.GetIPAddress (),
				       a.LengthFlags.IsDnsEligible,
				       a.LengthFlags.IsTransient));
			}
			c.is_readonly = true;
			return c;
		}

		public static MulticastIPAddressInformationImplCollection LinuxFromList (List<IPAddress> addresses)
		{
			MulticastIPAddressInformationImplCollection c = new MulticastIPAddressInformationImplCollection (false);
			foreach (IPAddress address in addresses) {
				c.Add (new MulticastIPAddressInformationImpl (
					address,
					true,
					false));
			}
			c.is_readonly = true;
			return c;
		}
	}
}
