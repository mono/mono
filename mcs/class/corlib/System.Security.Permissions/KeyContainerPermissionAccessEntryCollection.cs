//
// System.Security.Permissions.KeyContainerPermissionAccessEntryCollection class
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Collections;
using System.Globalization;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class KeyContainerPermissionAccessEntryCollection : ICollection, IEnumerable {

		private ArrayList _list;

		internal KeyContainerPermissionAccessEntryCollection ()
		{
			_list = new ArrayList ();
		}

		internal KeyContainerPermissionAccessEntryCollection (KeyContainerPermissionAccessEntry[] entries)
			: base ()
		{
			if (entries != null) {
				foreach (KeyContainerPermissionAccessEntry kcpae in entries) {
					Add (kcpae);
				}
			}
		}


		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }	// as documented
		}

		public KeyContainerPermissionAccessEntry this [int index] {
			get { return (KeyContainerPermissionAccessEntry) _list [index]; }
		}

		public object SyncRoot {
			get { return this; }	// as documented
		}


		public int Add (KeyContainerPermissionAccessEntry accessEntry)
		{
			return _list.Add (accessEntry);
		}

		public void Clear ()
		{
			_list.Clear ();
		}

		public void CopyTo (KeyContainerPermissionAccessEntry[] array, int index)
		{
			_list.CopyTo (array, index);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			_list.CopyTo (array, index);
		}

		public KeyContainerPermissionAccessEntryEnumerator GetEnumerator ()
		{
			return new KeyContainerPermissionAccessEntryEnumerator (_list);
		}

		IEnumerator IEnumerable.GetEnumerator () 
		{
			return new KeyContainerPermissionAccessEntryEnumerator (_list);
		}

		public int IndexOf (KeyContainerPermissionAccessEntry accessEntry) 
		{
			if (accessEntry == null)
				throw new ArgumentNullException ("accessEntry");

			for (int i=0; i < _list.Count; i++) {
				if (accessEntry.Equals (_list [i]))
					return i;
			}
			return -1;
		}

		public void Remove (KeyContainerPermissionAccessEntry accessEntry) 
		{
			if (accessEntry == null)
				throw new ArgumentNullException ("accessEntry");

			for (int i=0; i < _list.Count; i++) {
				if (accessEntry.Equals (_list [i]))
					_list.RemoveAt (i);
			}
		}
	}
}

#endif
