//
// System.Security.Cryptography.X509Certificates.X509ExtensionCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)
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

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509ExtensionCollection : ICollection, IEnumerable {

		private ArrayList _list;

		internal X509ExtensionCollection ()
		{
			_list = new ArrayList ();
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		public X509Extension this [int index] {
			get { return (X509Extension) _list [index]; }
		}

		public X509Extension this [string oid] {
			get { 
				foreach (X509Extension extension in this) {
					if (extension.Oid.Value.Equals (oid))
						return extension;
				}
				return null;
			}
		}

		// methods

		public int Add (X509Extension extension) 
		{
			return _list.Add (extension);
		}

		[MonoTODO]
		public void CopyTo (X509Extension[] array, int index) 
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentException ("negative index");
			if (index > array.Length)
				throw new ArgumentOutOfRangeException ("index > array.Length");
		}

		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public X509ExtensionEnumerator GetEnumerator () 
		{
			return new X509ExtensionEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new X509ExtensionEnumerator (this);
		}
	}
}

#endif
