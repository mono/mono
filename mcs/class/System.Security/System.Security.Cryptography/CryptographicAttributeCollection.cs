//
// System.Security.Cryptography.CryptographicAttributeCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Cryptography {

	public sealed class CryptographicAttributeCollection : ICollection, IEnumerable {

		private ArrayList _list;

		public CryptographicAttributeCollection () 
		{
			_list = new ArrayList ();
		}

		public CryptographicAttributeCollection (CryptographicAttribute attribute)
			: this ()
		{
			_list.Add (attribute);
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public CryptographicAttribute this [int index] {
			get { return (CryptographicAttribute) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (AsnEncodedData asnEncodedData)
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

			return _list.Add (asnEncodedData);
		}

		public int Add (CryptographicAttribute attribute)
		{
			if (attribute == null)
				throw new ArgumentNullException ("attribute");

			return _list.Add (attribute);
		}

		public void CopyTo (CryptographicAttribute[] array, int index)
		{
			_list.CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public CryptographicAttributeEnumerator GetEnumerator () 
		{
			return new CryptographicAttributeEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new CryptographicAttributeEnumerator (_list);
		}

		public void Remove (CryptographicAttribute attribute) 
		{
			_list.Remove (attribute);
		}
	}
}

#endif
