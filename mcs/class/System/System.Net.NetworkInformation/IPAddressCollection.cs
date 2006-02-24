//
// System.Net.NetworkInformation.IPAddressCollection
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Net.NetworkInformation {
	public class IPAddressCollection : ICollection<IPAddress>, IEnumerable<IPAddress>, IEnumerable {
		List<IPAddress> list;

		protected internal IPAddressCollection ()
		{
		}

		public virtual void Add (IPAddress address)
		{
			throw new NotSupportedException ("The collection is read-only.");
		}

		public virtual void Clear ()
		{
			throw new NotSupportedException ("The collection is read-only.");
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
			throw new NotSupportedException ("The collection is read-only.");
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

		public virtual IPAddress this [int index] {
			get { return list [index]; }
		}
	}
}
#endif

