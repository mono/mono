//
// System.Collections.Specialized.HybridDictionary.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Raja R Harinath <rharinath@novell.com>
//
// Copyright (C) 2004, 2005 Novell (http://www.novell.com)
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

namespace System.Collections.Specialized {

	[Serializable]
	public class HybridDictionary : IDictionary, ICollection, IEnumerable {

		private const int switchAfter = 10;

		private IDictionary inner {
			get { return list == null ? (IDictionary) hashtable : (IDictionary) list; }
		}

		private bool caseInsensitive;
		private Hashtable hashtable;
		private ListDictionary list;

		// Constructors

		public HybridDictionary() : this (0, false) { }

		public HybridDictionary (bool caseInsensitive) : this (0, caseInsensitive) { }

		public HybridDictionary (int initialSize) : this (initialSize, false) { }

		public HybridDictionary (int initialSize, bool caseInsensitive)
		{
			this.caseInsensitive = caseInsensitive;

			IComparer comparer = caseInsensitive ? CaseInsensitiveComparer.DefaultInvariant : null;
			IHashCodeProvider hcp = caseInsensitive ? CaseInsensitiveHashCodeProvider.DefaultInvariant : null;

			if (initialSize <= switchAfter)
				list = new ListDictionary (comparer);
			else
				hashtable = new Hashtable (initialSize, hcp, comparer);
		}

		// Properties

		public int Count {
			get { return inner.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object this [object key] {
			get { return inner [key]; }

			set {
				inner [key] = value;
				if (list != null && Count > switchAfter)
					Switch ();
			}
		}

		public ICollection Keys {
			get { return inner.Keys; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public ICollection Values {
			get { return inner.Values; }
		}


		// Methods

		public void Add (object key, object value)
		{
			inner.Add (key, value);
			if (list != null && Count > switchAfter)
				Switch ();
		}

		public void Clear ()
		{
			// According to MSDN, this doesn't switch a Hashtable back to a ListDictionary
			inner.Clear ();
		}

		public bool Contains (object key)
		{
			return inner.Contains (key);
		}

		public void CopyTo (Array array, int index)
		{
			inner.CopyTo (array, index);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return inner.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public void Remove (object key)
		{
			// According to MSDN, this does not switch a Hashtable back to a ListDictionary
			// even if Count falls below switchAfter
			inner.Remove (key);
		}

		private void Switch ()
		{
			IComparer comparer = caseInsensitive ? CaseInsensitiveComparer.DefaultInvariant : null;
			IHashCodeProvider hcp = caseInsensitive ? CaseInsensitiveHashCodeProvider.DefaultInvariant : null;

			hashtable = new Hashtable (list, hcp, comparer);
			list.Clear ();
			list = null;
		}
	}
}
