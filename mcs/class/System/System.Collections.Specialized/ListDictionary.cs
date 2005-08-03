//
// System.Collections.Specialized.ListDictionary.cs
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
namespace System.Collections.Specialized
{
	[Serializable]
	public class ListDictionary : IDictionary, ICollection, IEnumerable {
		private int count;
		private int modCount;
		private ListEntry root;
		private IComparer comparer;

		public ListDictionary ()
		{
			count = 0;
			modCount = 0;
			comparer = null;
			root = null;
		}

		public ListDictionary (IComparer comparer) : this ()
		{
			this.comparer = comparer;
		}

		private ListEntry FindEntry (object key, out ListEntry prev)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "Attempted lookup for a null key.");

			ListEntry entry = root;
			prev = null;
			if (comparer == null) {
				while (entry != null) {
					if (key.Equals (entry.key))
						break;
					prev = entry;
					entry = entry.next;
				}
			} else {
				while (entry != null) {
					if (comparer.Compare (key, entry.key) == 0)
						break;
					prev = entry;
					entry = entry.next;
				}
			}
			return entry;
		}

		private void AddImpl (object key, object value, ListEntry prev)
		{
			//
			// Code in the MCS compiler (doc.cs) appears to depend on the new entry being
			// added at the end, even though we don't promise any such thing.
			// Preferably, this code would just have been:
			//
			//   root = new ListEntry (key, value, root);
			//
			if (prev == null)
				root = new ListEntry (key, value, root);
			else
				prev.next = new ListEntry (key, value, prev.next);
			++count;
			++modCount;
		}

		// IEnumerable Interface
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new ListEntryEnumerator (this);
		}

		// ICollection Interface
		public int Count {
			get { return count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array", "Array cannot be null.");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "index is less than 0");
			if (index > array.Length)
				throw new IndexOutOfRangeException ("index is too large");
			if (Count > array.Length - index)
				throw new ArgumentException ("Not enough room in the array");

			foreach (DictionaryEntry entry in this)
				array.SetValue (entry, index++);
		}

		// IDictionary Interface
		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		// Indexer
		public object this [object key] {
			get {
				ListEntry prev;
				ListEntry entry = FindEntry (key, out prev);
				return entry == null ? null : entry.value;
			}

			set {
				ListEntry prev;
				ListEntry entry = FindEntry (key, out prev);
				if (entry != null)
					entry.value = value;
				else
					AddImpl (key, value, prev);
			}
		}

		public ICollection Keys {
			get { return new ListEntryCollection (this, true); }
		}

		public ICollection Values {
			get { return new ListEntryCollection (this, false); }
		}

		public void Add (object key, object value)
		{
			ListEntry prev;
			ListEntry entry = FindEntry (key, out prev);
			if (entry != null)
				throw new ArgumentException ("key", "Duplicate key in add.");

			AddImpl (key, value, prev);
		}

		public void Clear ()
		{
			root = null;
			count = 0;
			modCount++;
		}

		public bool Contains (object key)
		{
			ListEntry prev;
			return FindEntry (key, out prev) != null;
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return new ListEntryEnumerator (this);
		}

		public void Remove (object key)
		{
			ListEntry prev;
			ListEntry entry = FindEntry (key, out prev);
			if (entry == null)
				return;
			if (prev == null)
				root = entry.next;
			else
				prev.next = entry.next;
			entry.value = null;
			count--;
			modCount++;
		}


		[Serializable]
		private class ListEntry {
			public object key;
			public object value;
			public ListEntry next;
			public ListEntry (object key, object value, ListEntry next)
			{
				this.key = key;
				this.value = value;
				this.next = next;
			}
		}

		private class ListEntryEnumerator : IEnumerator, IDictionaryEnumerator {
			private ListDictionary dict;
			private bool isAtStart;
			private ListEntry current;
			private int version;

			public ListEntryEnumerator (ListDictionary dict)
			{
				this.dict = dict;
				version = dict.modCount;
				Reset();
			}

			private void FailFast()
			{
				if (version != dict.modCount) {
					throw new InvalidOperationException (
						"The ListDictionary's contents changed after this enumerator was instantiated.");
				}
			}

			public bool MoveNext ()
			{
				FailFast ();
				if (current == null && !isAtStart)
					return false;
				current = isAtStart ? dict.root : current.next;
				isAtStart = false;
				return current != null;
			}

			public void Reset ()
			{
				FailFast ();
				isAtStart = true;
				current = null;
			}

			public object Current {
				get { return Entry; }
			}

			private ListEntry ListEntry {
				get {
					FailFast ();
					if (current == null)
						throw new InvalidOperationException (
							"Enumerator is positioned before the collection's first element or after the last element.");
					return current;
				}
			}

			// IDictionaryEnumerator
			public DictionaryEntry Entry {
				get {
					object key = ListEntry.key;
					return new DictionaryEntry (key, current.value);
				}
			}

			public object Key {
				get { return ListEntry.key; }
			}

			public object Value {
				get { return ListEntry.value; }
			}
		}

		private class ListEntryCollection : ICollection {
			private ListDictionary dict;
			private bool isKeyList;

			public ListEntryCollection (ListDictionary dict, bool isKeyList)
			{
				this.dict = dict;
				this.isKeyList = isKeyList;
			}

			// ICollection Interface
			public int Count {
				get { return dict.Count; }
			}

			public bool IsSynchronized {
				get { return false; }
			}

			public object SyncRoot {
				get { return dict.SyncRoot; }
			}

			public void CopyTo (Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array", "Array cannot be null.");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index", "index is less than 0");
				if (index > array.Length)
					throw new IndexOutOfRangeException ("index is too large");
				if (Count > array.Length - index)
					throw new ArgumentException ("Not enough room in the array");

				foreach (object obj in this)
					array.SetValue (obj, index++);
			}

			// IEnumerable Interface
			public IEnumerator GetEnumerator()
			{
				return new ListEntryCollectionEnumerator (dict.GetEnumerator (), isKeyList);
			}

			private class ListEntryCollectionEnumerator : IEnumerator {
				private IDictionaryEnumerator inner;
				private bool isKeyList;

				public ListEntryCollectionEnumerator (IDictionaryEnumerator inner, bool isKeyList)
				{
					this.inner = inner;
					this.isKeyList = isKeyList;
				}

				public object Current {
					get { return isKeyList ? inner.Key : inner.Value; }
				}

				public bool MoveNext ()
				{
					return inner.MoveNext ();
				}

				public void Reset ()
				{
					inner.Reset ();
				}
			}
		}
	}
}
