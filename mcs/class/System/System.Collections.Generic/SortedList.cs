// 
// System.Collections.Generic.SortedList.cs
// 
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//   Duncan Mak (duncan@ximian.com)
//   Herve Poussineau (hpoussineau@fr.st
//   Zoltan Varga (vargaz@gmail.com)
// 

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

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections.Generic
{
	/// <summary>
	///  Represents a collection of associated keys and values
	///  that are sorted by the keys and are accessible by key
	///  and by index.
	/// </summary>
	[Serializable]
	[ComVisible(false)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
	public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, 
		IDictionary,
		ICollection,
		ICollection<KeyValuePair<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>>,
		IEnumerable {

		private readonly static int INITIAL_SIZE = 16;

		private enum EnumeratorMode : int { KEY_MODE = 0, VALUE_MODE, ENTRY_MODE }

		private int inUse;
		private int modificationCount;
		private KeyValuePair<TKey, TValue>[] table;
		private IComparer<TKey> comparer;
		private int defaultCapacity;

		//
		// Constructors
		//
		public SortedList () 
			: this (INITIAL_SIZE, null)
		{
		}

		public SortedList (int capacity)
			: this (capacity, null)
		{
		}

		public SortedList (int capacity, IComparer<TKey> comparer)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("initialCapacity");

			if (capacity == 0)
				defaultCapacity = 0;
			else
				defaultCapacity = INITIAL_SIZE;
			Init (comparer, capacity, true);
		}

		public SortedList (IComparer<TKey> comparer) : this (INITIAL_SIZE, comparer)
		{
		}

		public SortedList (IDictionary<TKey, TValue> dictionary) : this (dictionary, null)
		{
		}

		public SortedList (IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");

			Init (comparer, dictionary.Count, true);

			foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
				Add (kvp.Key, kvp.Value);
		}

		//
		// Properties
		//

		// ICollection

		public int Count {
			get {
				return inUse;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		Object ICollection.SyncRoot {
			get {
				return this;
			}
		}

		// IDictionary

		bool IDictionary.IsFixedSize {
			get {
				return false;
			}
		}

		bool IDictionary.IsReadOnly {
			get {
				return false;
			}
		}

		public TValue this [TKey key] {
			get {
				if (key == null)
					throw new ArgumentNullException("key");

				int i = Find (key);

				if (i >= 0)
					return table [i].Value;
				else
					throw new KeyNotFoundException ();
			}
			set {
				if (key == null)
					throw new ArgumentNullException("key");

				PutImpl (key, value, true);
			}
		}

		object IDictionary.this [object key] {
			get {
				if (!(key is TKey))
					return null;
				else
					return this [(TKey)key];
			}

			set {
				this [ToKey (key)] = ToValue (value);
			}
		}

		public int Capacity {
			get {
				return table.Length;
			}

			set {
				int current = this.table.Length;

				if (inUse > value) {
					throw new ArgumentOutOfRangeException("capacity too small");
				}
				else if (value == 0) {
					// return to default size
                                        KeyValuePair<TKey, TValue> [] newTable = new KeyValuePair<TKey, TValue> [defaultCapacity];
                                        Array.Copy (table, newTable, inUse);
                                        this.table = newTable;
				}
#if NET_1_0
				else if (current > defaultCapacity && value < current) {
                                        KeyValuePair<TKey, TValue> [] newTable = new KeyValuePair<TKey, TValue> [defaultCapacity];
                                        Array.Copy (table, newTable, inUse);
                                        this.table = newTable;
                                }
#endif
				else if (value > inUse) {
                                        KeyValuePair<TKey, TValue> [] newTable = new KeyValuePair<TKey, TValue> [value];
                                        Array.Copy (table, newTable, inUse);
                                        this.table = newTable;
                                }
				else if (value > current) {
					KeyValuePair<TKey, TValue> [] newTable = new KeyValuePair<TKey, TValue> [value];
					Array.Copy (table, newTable, current);
					this.table = newTable;
				}
			}
		}

		public IList<TKey> Keys {
			get { 
				return new ListKeys (this);
			}
		}

		public IList<TValue> Values {
			get {
				return new ListValues (this);
			}
		}

		ICollection IDictionary.Keys {
			get {
				return new ListKeys (this);
			}
		}

		ICollection IDictionary.Values {
			get {
				return new ListValues (this);
			}
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys {
			get { 
				return Keys;
			}
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values {
			get {
				return Values;
			}
		}

		public IComparer<TKey> Comparer {
			get {
				return comparer;
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get {
				return false;
			}
		}

		//
		// Public instance methods.
		//

		public void Add (TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			PutImpl (key, value, false);
		}

		public bool ContainsKey (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			return (Find (key) >= 0);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			for (int i = 0; i < inUse; i ++) {
				KeyValuePair<TKey, TValue> current = this.table [i];

				yield return new KeyValuePair<TKey, TValue> (current.Key, current.Value);
			}
		}

		public bool Remove (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			int i = IndexOfKey (key);
			if (i >= 0) {
				RemoveAt (i);
				return true;
			}
			else
				return false;
		}

		// ICollection<KeyValuePair<TKey, TValue>>

		void ICollection<KeyValuePair<TKey, TValue>>.Clear () 
		{
			defaultCapacity = INITIAL_SIZE;
			this.table = new KeyValuePair<TKey, TValue> [defaultCapacity];
			inUse = 0;
			modificationCount++;
		}

		public void Clear () 
		{
			defaultCapacity = INITIAL_SIZE;
			this.table = new KeyValuePair<TKey, TValue> [defaultCapacity];
			inUse = 0;
			modificationCount++;
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (Count == 0)
				return;
			
			if (null == array)
				throw new ArgumentNullException();

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException();
			
			if (arrayIndex >= array.Length)
				throw new ArgumentNullException("arrayIndex is greater than or equal to array.Length");
			if (Count > (array.Length - arrayIndex))
				throw new ArgumentNullException("Not enough space in array from arrayIndex to end of array");

			int i = arrayIndex;
			foreach (KeyValuePair<TKey, TValue> pair in this)
				array [i++] = pair;
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> keyValuePair) {
			Add (keyValuePair.Key, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> keyValuePair) {
			int i = Find (keyValuePair.Key);

			if (i >= 0)
				return Comparer<KeyValuePair<TKey, TValue>>.Default.Compare (table [i], keyValuePair) == 0;
			else
				return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> keyValuePair) {
			int i = Find (keyValuePair.Key);

			if (i >= 0 && (Comparer<KeyValuePair<TKey, TValue>>.Default.Compare (table [i], keyValuePair) == 0)) {
				RemoveAt (i);
				return true;
			}
			else
				return false;
		}

		// IEnumerable<KeyValuePair<TKey, TValue>>

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator ()
		{
			for (int i = 0; i < inUse; i ++) {
				KeyValuePair<TKey, TValue> current = this.table [i];

				yield return new KeyValuePair<TKey, TValue> (current.Key, current.Value);
			}
		}

		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		// IDictionary

		void IDictionary.Add (object key, object value)
		{
			PutImpl (ToKey (key), ToValue (value), false);
		}

		bool IDictionary.Contains (object key)
		{
			if (null == key)
				throw new ArgumentNullException();
			if (!(key is TKey))
				return false;

			return (Find ((TKey)key) >= 0);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.ENTRY_MODE);
		}

		void IDictionary.Remove (object key)
		{
			if (null == key)
				throw new ArgumentNullException ("key");
			if (!(key is TKey))
				return;
			int i = IndexOfKey ((TKey)key);
			if (i >= 0) RemoveAt (i);
		}

		// ICollection

		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			if (Count == 0)
				return;
			
			if (null == array)
				throw new ArgumentNullException();

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException();
			
			if (array.Rank > 1)
				throw new ArgumentException("array is multi-dimensional");
			if (arrayIndex >= array.Length)
				throw new ArgumentNullException("arrayIndex is greater than or equal to array.Length");
			if (Count > (array.Length - arrayIndex))
				throw new ArgumentNullException("Not enough space in array from arrayIndex to end of array");

			IEnumerator<KeyValuePair<TKey,TValue>> it = GetEnumerator ();
			int i = arrayIndex;

			while (it.MoveNext ()) {
				array.SetValue (it.Current, i++);
			}
		}

		//
		// SortedList<TKey, TValue>
		//

		public void RemoveAt (int index)
		{
			KeyValuePair<TKey, TValue> [] table = this.table;
			int cnt = Count;
			if (index >= 0 && index < cnt) {
				if (index != cnt - 1) {
					Array.Copy (table, index+1, table, index, cnt-1-index);
				} else {
					table [index] = default (KeyValuePair <TKey, TValue>);
				}
				--inUse;
				++modificationCount;
			} else {
				throw new ArgumentOutOfRangeException("index out of range");
			}
		}

		public int IndexOfKey (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			int indx = 0;
			try {
				indx = Find (key);
			} catch (Exception) {
				throw new InvalidOperationException();
			}

			return (indx | (indx >> 31));
		}

		public int IndexOfValue (TValue value)
		{
			if (inUse == 0)
				return -1;

			for (int i = 0; i < inUse; i ++) {
				KeyValuePair<TKey, TValue> current = this.table [i];

				if (Equals (value, current.Value))
					return i;
			}

			return -1;
		}

		public bool ContainsValue (TValue value)
		{
			return IndexOfValue (value) >= 0;
		}

		public void TrimExcess ()
		{
			if (inUse < table.Length * 0.9)
				Capacity = inUse;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			int i = Find (key);

			if (i >= 0) {
				value = table [i].Value;
				return true;
			}
			else {
				value = default (TValue);
				return false;
			}
		}

		//
		// Private methods
		//

		private void EnsureCapacity (int n, int free)
		{
			KeyValuePair<TKey, TValue> [] table = this.table;
			KeyValuePair<TKey, TValue> [] newTable = null;
			int cap = Capacity;
			bool gap = (free >=0 && free < Count);

			if (n > cap) {
				newTable = new KeyValuePair<TKey, TValue> [n << 1];
			}

			if (newTable != null) {
				if (gap) {
					int copyLen = free;
					if (copyLen > 0) {
						Array.Copy (table, 0, newTable, 0, copyLen);
					}
					copyLen = Count - free;
					if (copyLen > 0) {
						Array.Copy (table, free, newTable, free+1, copyLen);
					}
				} else {
					// Just a resizing, copy the entire table.
					Array.Copy (table, newTable, Count);
				}
				this.table = newTable;
			} else if (gap) {
				Array.Copy (table, free, table, free+1, Count - free);
			}
		}

		private void PutImpl (TKey key, TValue value, bool overwrite)
		{
			if (key == null)
				throw new ArgumentNullException ("null key");

			KeyValuePair<TKey, TValue> [] table = this.table;

			int freeIndx = -1;

			try {
				freeIndx = Find (key);
			} catch (Exception) {
				throw new InvalidOperationException();
			}

			if (freeIndx >= 0) {
				if (!overwrite)
					throw new ArgumentException("element already exists");

				table [freeIndx] = new KeyValuePair <TKey, TValue> (key, value);
				++modificationCount;
				return;
			}

			freeIndx = ~freeIndx;

			if (freeIndx > Capacity + 1)
				throw new Exception ("SortedList::internal error ("+key+", "+value+") at ["+freeIndx+"]");


			EnsureCapacity (Count+1, freeIndx);

			table = this.table;
			table [freeIndx] = new KeyValuePair <TKey, TValue> (key, value);

			++inUse;
			++modificationCount;

		}

		private void Init (IComparer<TKey> comparer, int capacity, bool forceSize) 
		{
			if (comparer == null)
				comparer = Comparer<TKey>.Default;
			this.comparer = comparer;
			if (!forceSize && (capacity < defaultCapacity))
				capacity = defaultCapacity;
			this.table = new KeyValuePair<TKey, TValue> [capacity];
			this.inUse = 0;
			this.modificationCount = 0;
		}

		private void  CopyToArray (Array arr, int i, 
					   EnumeratorMode mode)
		{
			if (arr == null)
				throw new ArgumentNullException ("arr");

			if (i < 0 || i + this.Count > arr.Length)
				throw new ArgumentOutOfRangeException ("i");
			
			IEnumerator it = new Enumerator (this, mode);

			while (it.MoveNext ()) {
				arr.SetValue (it.Current, i++);
			}
		}

		private int Find (TKey key)
		{
			KeyValuePair<TKey, TValue> [] table = this.table;
			int len = Count;

			if (len == 0) return ~0;

			int left = 0;
			int right = len-1;

			while (left <= right) {
				int guess = (left + right) >> 1;

				int cmp = comparer.Compare (table[guess].Key, key);
				if (cmp == 0) return guess;

				if (cmp <  0) left = guess+1;
				else right = guess-1;
			}

			return ~left;
		}

		private TKey ToKey (object key) {
			if (key == null)
				throw new ArgumentNullException ("key");
			if (!(key is TKey))
				throw new ArgumentException ("The value \"" + key + "\" isn't of type \"" + typeof (TKey) + "\" and can't be used in this generic collection.", "key");
			return (TKey)key;
		}

		private TValue ToValue (object value) {
			if (!(value is TValue))
				throw new ArgumentException ("The value \"" + value + "\" isn't of type \"" + typeof (TValue) + "\" and can't be used in this generic collection.", "value");
			return (TValue)value;
		}

		internal TKey KeyAt (int index) {
			if (index >= 0 && index < Count)
				return table [index].Key;
			else
				throw new ArgumentOutOfRangeException("Index out of range");
		}

		internal TValue ValueAt (int index) {
			if (index >= 0 && index < Count)
				return table [index].Value;
			else
				throw new ArgumentOutOfRangeException("Index out of range");
		}

		//
		// Inner classes
		//


		private sealed class Enumerator : ICloneable, IDictionaryEnumerator, IEnumerator {

			private SortedList<TKey, TValue>host;
			private int stamp;
			private int pos;
			private int size;
			private EnumeratorMode mode;

			private object currentKey;
			private object currentValue;

			bool invalid = false;

			private readonly static string xstr = "SortedList.Enumerator: snapshot out of sync.";

			public Enumerator (SortedList<TKey, TValue>host, EnumeratorMode mode)
			{
				this.host = host;
				stamp = host.modificationCount;
				size = host.Count;
				this.mode = mode;
				Reset ();
			}

			public Enumerator (SortedList<TKey, TValue>host)
			: this (host, EnumeratorMode.ENTRY_MODE)
			{
			}

			public void Reset ()
			{
				if (host.modificationCount != stamp || invalid)
					throw new InvalidOperationException (xstr);

				pos = -1;
				currentKey = null;
				currentValue = null;
			}

			public bool MoveNext ()
			{
				if (host.modificationCount != stamp || invalid)
					throw new InvalidOperationException (xstr);

				KeyValuePair<TKey, TValue> [] table = host.table;

				if (++pos < size) {
					KeyValuePair<TKey, TValue> entry = table [pos];

					currentKey = entry.Key;
					currentValue = entry.Value;
					return true;
				}

				currentKey = null;
				currentValue = null;
				return false;
			}

			public DictionaryEntry Entry
			{
				get {
					if (invalid || pos >= size || pos == -1)
						throw new InvalidOperationException (xstr);
					
					return new DictionaryEntry (currentKey,
					                            currentValue);
				}
			}

			public Object Key {
				get {
					if (invalid || pos >= size || pos == -1)
						throw new InvalidOperationException (xstr);
					return currentKey;
				}
			}

			public Object Value {
				get {
					if (invalid || pos >= size || pos == -1)
						throw new InvalidOperationException (xstr);
					return currentValue;
				}
			}

			public Object Current {
				get {
					if (invalid || pos >= size || pos == -1)
						throw new InvalidOperationException (xstr);

					switch (mode) {
                                        case EnumeratorMode.KEY_MODE:
                                                return currentKey;
                                        case EnumeratorMode.VALUE_MODE:
                                                return currentValue;
                                        case EnumeratorMode.ENTRY_MODE:
                                                return this.Entry;

                                        default:
                                                throw new NotSupportedException (mode + " is not a supported mode.");
                                        }
				}
			}

			// ICloneable

			public object Clone ()
			{
				Enumerator e = new Enumerator (host, mode);
				e.stamp = stamp;
				e.pos = pos;
				e.size = size;
				e.currentKey = currentKey;
				e.currentValue = currentValue;
				e.invalid = invalid;
				return e;
			}
		}

		[Serializable]
		struct KeyEnumerator : IEnumerator <TKey>, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			SortedList <TKey, TValue> l;
			int idx;
			int ver;
			
			internal KeyEnumerator (SortedList<TKey, TValue> l)
			{
				this.l = l;
				idx = NOT_STARTED;
				ver = l.modificationCount;
			}
			
			public void Dispose ()
			{
				idx = NOT_STARTED;
			}
			
			public bool MoveNext ()
			{
				if (ver != l.modificationCount)
					throw new InvalidOperationException ("Collection was modified after the enumerator was instantiated.");
				
				if (idx == NOT_STARTED)
					idx = l.Count;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public TKey Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return l.KeyAt (l.Count - 1 - idx);
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != l.modificationCount)
					throw new InvalidOperationException ("Collection was modified after the enumerator was instantiated.");
				
				idx = NOT_STARTED;
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}
		}

		[Serializable]
		struct ValueEnumerator : IEnumerator <TValue>, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			SortedList <TKey, TValue> l;
			int idx;
			int ver;
			
			internal ValueEnumerator (SortedList<TKey, TValue> l)
			{
				this.l = l;
				idx = NOT_STARTED;
				ver = l.modificationCount;
			}
			
			public void Dispose ()
			{
				idx = NOT_STARTED;
			}
			
			public bool MoveNext ()
			{
				if (ver != l.modificationCount)
					throw new InvalidOperationException ("Collection was modified after the enumerator was instantiated.");
				
				if (idx == NOT_STARTED)
					idx = l.Count;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public TValue Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return l.ValueAt (l.Count - 1 - idx);
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != l.modificationCount)
					throw new InvalidOperationException ("Collection was modified after the enumerator was instantiated.");
				
				idx = NOT_STARTED;
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}
		}

		private class ListKeys : IList<TKey>, ICollection, IEnumerable {

			private SortedList<TKey, TValue> host;

			public ListKeys (SortedList<TKey, TValue> host)
			{
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			// ICollection<TKey>

			public virtual void Add (TKey item) {
				throw new NotSupportedException();
			}

			public virtual bool Remove (TKey key) {
				throw new NotSupportedException ();
			}

			public virtual void Clear () {
				throw new NotSupportedException();
			}

			public virtual void CopyTo (TKey[] array, int arrayIndex) {
				if (host.Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ("array");
				if (arrayIndex < 0)
					throw new ArgumentOutOfRangeException();
				if (arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException ("arrayIndex is greater than or equal to array.Length");
				if (Count > (array.Length - arrayIndex))
					throw new ArgumentOutOfRangeException("Not enough space in array from arrayIndex to end of array");

				int j = arrayIndex;
				for (int i = 0; i < Count; ++i)
					array [j ++] = host.KeyAt (i);
			}

			public virtual bool Contains (TKey item) {
				return host.IndexOfKey (item) > -1;
			}

			//
			// IList<TKey>
			//
			public virtual int IndexOf (TKey item) {
				return host.IndexOfKey (item);
			}

			public virtual void Insert (int index, TKey item) {
				throw new NotSupportedException ();
			}

			public virtual void RemoveAt (int index) {
				throw new NotSupportedException ();
			}

			public virtual TKey this [int index] {
				get {
					return host.KeyAt (index);
				}
				set {
					throw new NotSupportedException("attempt to modify a key");
				}
			}

			//
			// IEnumerable<TKey>
			//

			public virtual IEnumerator<TKey> GetEnumerator ()
			{
				/* We couldn't use yield as it does not support Reset () */
				return new KeyEnumerator (host);
			}

			//
			// ICollection
			//

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return ((ICollection)host).IsSynchronized;
				}
			}

			public virtual bool IsReadOnly {
				get {
					return true;
				}
			}

			public virtual Object SyncRoot {
				get {
					return ((ICollection)host).SyncRoot;
				}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				host.CopyToArray (array, arrayIndex, EnumeratorMode.KEY_MODE);
			}

			//
			// IEnumerable
			//

			IEnumerator IEnumerable.GetEnumerator ()
			{
				for (int i = 0; i < host.Count; ++i)
					yield return host.KeyAt (i);
			}
		}			

		private class ListValues : IList<TValue>, ICollection, IEnumerable {

			private SortedList<TKey, TValue>host;

			public ListValues (SortedList<TKey, TValue>host)
			{
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			// ICollection<TValue>

			public virtual void Add (TValue item) {
				throw new NotSupportedException();
			}

			public virtual bool Remove (TValue value) {
				throw new NotSupportedException ();
			}

			public virtual void Clear () {
				throw new NotSupportedException();
			}

			public virtual void CopyTo (TValue[] array, int arrayIndex) {
				if (host.Count == 0)
					return;
				if (array == null)
					throw new ArgumentNullException ("array");
				if (arrayIndex < 0)
					throw new ArgumentOutOfRangeException();
				if (arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException ("arrayIndex is greater than or equal to array.Length");
				if (Count > (array.Length - arrayIndex))
					throw new ArgumentOutOfRangeException("Not enough space in array from arrayIndex to end of array");

				int j = arrayIndex;
				for (int i = 0; i < Count; ++i)
					array [j ++] = host.ValueAt (i);
			}

			public virtual bool Contains (TValue item) {
				return host.IndexOfValue (item) > -1;
			}

			//
			// IList<TValue>
			//
			public virtual int IndexOf (TValue item) {
				return host.IndexOfValue (item);
			}

			public virtual void Insert (int index, TValue item) {
				throw new NotSupportedException ();
			}

			public virtual void RemoveAt (int index) {
				throw new NotSupportedException ();
			}

			public virtual TValue this [int index] {
				get {
					return host.ValueAt (index);
				}
				set {
					throw new NotSupportedException("attempt to modify a key");
				}
			}

			//
			// IEnumerable<TValue>
			//

			public virtual IEnumerator<TValue> GetEnumerator ()
			{
				/* We couldn't use yield as it does not support Reset () */
				return new ValueEnumerator (host);
			}

			//
			// ICollection
			//

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return ((ICollection)host).IsSynchronized;
				}
			}

			public virtual bool IsReadOnly {
				get {
					return true;
				}
			}

			public virtual Object SyncRoot {
				get {
					return ((ICollection)host).SyncRoot;
				}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				host.CopyToArray (array, arrayIndex, EnumeratorMode.VALUE_MODE);
			}

			//
			// IEnumerable
			//

			IEnumerator IEnumerable.GetEnumerator ()
			{
				for (int i = 0; i < host.Count; ++i)
					yield return host.ValueAt (i);
			}
		}

	} // SortedList

} // System.Collections.Generic
