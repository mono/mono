//
// OidCollection.cs - System.Security.Cryptography.OidCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

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

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class OidCollection : ICollection, IEnumerable {

		private ArrayList _list;

		// constructors

		public OidCollection ()
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

		public Oid this [int index] {
			get { return (Oid) _list [index]; }
		}

		public Oid this [string oid] {
			get { 
				foreach (Oid o in _list) {
					if (o.Value == oid)
						return o;
				}
				return null; 
			}
		}

		public object SyncRoot {
			get { return _list.SyncRoot; }
		}

		// methods

		public int Add (Oid oid)
		{
			return _list.Add (oid);
		}

		public void CopyTo (Oid[] array, int index)
		{
			_list.CopyTo ((Array)array, index);
		}

		// to satisfy ICollection - private
		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public OidEnumerator GetEnumerator () 
		{
			return new OidEnumerator (this);
		}

		// to satisfy IEnumerator - private
		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new OidEnumerator (this);
		}
	}
}

#endif