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
using System;
using System.Runtime.Serialization;

namespace System.Collections.Specialized
{
	[Serializable]
	public class ListDictionary : IDictionary, ICollection, IEnumerable {
		private int count;
		private int version;
		private DictionaryNode head;
		private IComparer comparer;

		public ListDictionary ()
		{
			count = 0;
			version = 0;
			comparer = null;
			head = null;
		}

		public ListDictionary (IComparer comparer) : this ()
		{
			this.comparer = comparer;
		}

		private DictionaryNode FindEntry (object key)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "Attempted lookup for a null key.");

			DictionaryNode entry = head;
			if (comparer == null) {
				while (entry != null) {
					if (key.Equals (entry.key))
						break;
					entry = entry.next;
				}
			} else {
				while (entry != null) {
					if (comparer.Compare (key, entry.key) == 0)
						break;
					entry = entry.next;
				}
			}
			return entry;
		}
		private DictionaryNode FindEntry (object key, out DictionaryNode prev)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "Attempted lookup for a null key.");

			DictionaryNode entry = head;
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

		private void AddImpl (object key, object value, DictionaryNode prev)
		{
			//
			// Code in the MCS compiler (doc.cs) appears to depend on the new entry being
			// added at the end, even though we don't promise any such thing.
			// Preferably, this code would just have been:
			//
			//   head = new DictionaryNode (key, value, head);
			//
			if (prev == null)
				head = new DictionaryNode (key, value, head);
			else
				prev.next = new DictionaryNode (key, value, prev.next);
			++count;
			++version;
		}

		// IEnumerable Interface
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DictionaryNodeEnumerator (this);
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
				DictionaryNode entry = FindEntry (key);
				return entry == null ? null : entry.value;
			}

			set {
				DictionaryNode prev;
				DictionaryNode entry = FindEntry (key, out prev);
				if (entry != null)
					entry.value = value;
				else
					AddImpl (key, value, prev);
			}
		}

		public ICollection Keys {
			get { return new DictionaryNodeCollection (this, true); }
		}

		public ICollection Values {
			get { return new DictionaryNodeCollection (this, false); }
		}

		public void Add (object key, object value)
		{
			DictionaryNode prev;
			DictionaryNode entry = FindEntry (key, out prev);
			if (entry != null)
				throw new ArgumentException ("key", "Duplicate key in add.");

			AddImpl (key, value, prev);
		}

		public void Clear ()
		{
			head = null;
			count = 0;
			version++;
		}

		public bool Contains (object key)
		{
			return FindEntry (key) != null;
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return new DictionaryNodeEnumerator (this);
		}

		public void Remove (object key)
		{
			DictionaryNode prev;
			DictionaryNode entry = FindEntry (key, out prev);
			if (entry == null)
				return;
			if (prev == null)
				head = entry.next;
			else
				prev.next = entry.next;
			entry.value = null;
			count--;
			version++;
		}


		[Serializable]
		private class DictionaryNode {
			public object key;
			public object value;
			public DictionaryNode next;
			public DictionaryNode (object key, object value, DictionaryNode next)
			{
				this.key = key;
				this.value = value;
				this.next = next;
			}
		}

		private class DictionaryNodeEnumerator : IEnumerator, IDictionaryEnumerator {
			private ListDictionary dict;
			private bool isAtStart;
			private DictionaryNode current;
			private int version;

			public DictionaryNodeEnumerator (ListDictionary dict)
			{
				this.dict = dict;
				version = dict.version;
				Reset();
			}

			private void FailFast()
			{
				if (version != dict.version) {
					throw new InvalidOperationException (
						"The ListDictionary's contents changed after this enumerator was instantiated.");
				}
			}

			public bool MoveNext ()
			{
				FailFast ();
				if (current == null && !isAtStart)
					return false;
				current = isAtStart ? dict.head : current.next;
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

			private DictionaryNode DictionaryNode {
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
					object key = DictionaryNode.key;
					return new DictionaryEntry (key, current.value);
				}
			}

			public object Key {
				get { return DictionaryNode.key; }
			}

			public object Value {
				get { return DictionaryNode.value; }
			}
		}

		private class DictionaryNodeCollection : ICollection {
			private ListDictionary dict;
			private bool isKeyList;

			public DictionaryNodeCollection (ListDictionary dict, bool isKeyList)
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
				return new DictionaryNodeCollectionEnumerator (dict.GetEnumerator (), isKeyList);
			}

			private class DictionaryNodeCollectionEnumerator : IEnumerator {
				private IDictionaryEnumerator inner;
				private bool isKeyList;

				public DictionaryNodeCollectionEnumerator (IDictionaryEnumerator inner, bool isKeyList)
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
