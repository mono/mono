//
// System.Security.Cryptography.AsnEncodedDataCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP

using System.Collections;

namespace System.Security.Cryptography {

	public sealed class AsnEncodedDataCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public AsnEncodedDataCollection ()
		{
			_list = new ArrayList ();
		}

		public AsnEncodedDataCollection (AsnEncodedData asnEncodedData)
		{
			_list = new ArrayList ();
			_list.Add (asnEncodedData);
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public AsnEncodedData this [int index] {
			get { return (AsnEncodedData) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (AsnEncodedData asnEncodedData)
		{
			return _list.Add (asnEncodedData);
		}

		public void CopyTo (AsnEncodedData[] array, int index)
		{
			_list.CopyTo ((Array)array, index);
		}

		// to satisfy ICollection - private
		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public AsnEncodedDataEnumerator GetEnumerator () 
		{
			return new AsnEncodedDataEnumerator (this);
		}

		// to satisfy IEnumerator - private
		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new AsnEncodedDataEnumerator (this);
		}

		public void Remove (AsnEncodedData asnEncodedData)
		{
			_list.Remove (asnEncodedData);
		}
	}
}

#endif
