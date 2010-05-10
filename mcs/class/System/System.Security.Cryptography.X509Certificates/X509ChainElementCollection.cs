//
// X509ChainElementCollection.cs - System.Security.Cryptography.X509Certificates.X509ChainElementCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2006 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP || MOONLIGHT

using System.Collections;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509ChainElementCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		// only accessible from X509Chain
		internal X509ChainElementCollection () 
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

		public X509ChainElement this [int index] {
			get { return (X509ChainElement) _list [index]; }
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public void CopyTo (X509ChainElement[] array, int index) 
		{
			_list.CopyTo ((Array)array, index);
		}

		void ICollection.CopyTo (Array array, int index) 
		{
			_list.CopyTo (array, index);
		}

		public X509ChainElementEnumerator GetEnumerator ()
		{
			return new X509ChainElementEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new X509ChainElementEnumerator (_list);
		}

		// private stuff

		internal void Add (X509Certificate2 certificate)
		{
			_list.Add (new X509ChainElement (certificate));
		}

		internal void Clear ()
		{
			_list.Clear ();
		}

		internal bool Contains (X509Certificate2 certificate)
		{
			for (int i=0; i < _list.Count; i++) {
				if (certificate.Equals (( _list [i] as X509ChainElement).Certificate))
					return true;
			}
			return false;
		}
	}
}

#endif
