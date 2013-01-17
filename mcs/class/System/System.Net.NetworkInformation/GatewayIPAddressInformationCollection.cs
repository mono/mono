//
// System.Net.NetworkInformation.GatewayIPAddressInformationCollection
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public class GatewayIPAddressInformationCollection : ICollection<GatewayIPAddressInformation>, IEnumerable<GatewayIPAddressInformation>, IEnumerable {
		List<GatewayIPAddressInformation> list = new List<GatewayIPAddressInformation> ();
		
		protected GatewayIPAddressInformationCollection ()
		{
		}

		public virtual void Add (GatewayIPAddressInformation address)
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

		public virtual bool Contains (GatewayIPAddressInformation address)
		{
			return list.Contains (address);
		}

		public virtual void CopyTo (GatewayIPAddressInformation [] array, int offset)
		{
			list.CopyTo (array, offset);
		}

		public virtual IEnumerator<GatewayIPAddressInformation> GetEnumerator ()
		{
			return ((IEnumerable<GatewayIPAddressInformation>)list).GetEnumerator ();
		}

		public virtual bool Remove (GatewayIPAddressInformation address)
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

		public virtual GatewayIPAddressInformation this [int index] {
			get { return list [index]; }
		}
	}

	class Win32GatewayIPAddressInformationCollection : GatewayIPAddressInformationCollection
	{
		public static readonly Win32GatewayIPAddressInformationCollection Empty = new Win32GatewayIPAddressInformationCollection (true);

		bool is_readonly;

		private Win32GatewayIPAddressInformationCollection (bool isReadOnly)
		{
			this.is_readonly = isReadOnly;
		}

		public Win32GatewayIPAddressInformationCollection (params Win32_IP_ADDR_STRING [] al)
		{
			foreach (Win32_IP_ADDR_STRING a in al) {
				if (String.IsNullOrEmpty (a.IpAddress))
					continue;
				Add (new GatewayIPAddressInformationImpl (IPAddress.Parse (a.IpAddress)));
				AddSubsequently (a.Next);
			}
			is_readonly = true;
		}

		void AddSubsequently (IntPtr head)
		{
			Win32_IP_ADDR_STRING a;
			for (IntPtr p = head; p != IntPtr.Zero; p = a.Next) {
				a = (Win32_IP_ADDR_STRING) Marshal.PtrToStructure (p, typeof (Win32_IP_ADDR_STRING));
				Add (new GatewayIPAddressInformationImpl (IPAddress.Parse (a.IpAddress)));
			}
		}

		public override bool IsReadOnly {
			get { return is_readonly; }
		}
	}

	class LinuxGatewayIPAddressInformationCollection : GatewayIPAddressInformationCollection
	{
		public static readonly LinuxGatewayIPAddressInformationCollection Empty = new LinuxGatewayIPAddressInformationCollection (true);

		bool is_readonly;

		private LinuxGatewayIPAddressInformationCollection (bool isReadOnly)
		{
			this.is_readonly = isReadOnly;
		}

		public LinuxGatewayIPAddressInformationCollection (IPAddressCollection col)
		{
			foreach (IPAddress a in col)
				Add (new GatewayIPAddressInformationImpl (a));
			this.is_readonly = true;
		}
		
		public override bool IsReadOnly {
			get { return is_readonly; }
		}
	}
}

