//
// System.Collections.Generic.Dictionary
//
// Authors:
//	Sureshkumar T (tsureshkumar@novell.com)
//	Marek Safar (marek.safar@seznam.cz) (stubs)
//	Ankit Jain (radical@corewars.org)
//	David Waite (mass@akuma.org)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Collections.Generic {

	[Serializable]
	[CLSCompliant(true)]
	public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>,
		IDictionary,
		ICollection,
		ICollection<KeyValuePair<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>>,
		ISerializable,
		IDeserializationCallback
	{
		const int INITIAL_SIZE = 10;
		const float DEFAULT_LOAD_FACTOR = (90f / 100);

		private class Slot {
			public KeyValuePair<TKey, TValue> Data;
			public Slot Next;
			
			public Slot (KeyValuePair<TKey,TValue> Data, Slot Next)
			{
				this.Data = Data;
				this.Next = Next;
			}
		}
	
		Slot [] _table;
	
		int _usedSlots;
	
		IEqualityComparer<TKey> _hcp;
		SerializationInfo _serializationInfo;
		
		private int _threshold;
	
		public int Count {
			get { return _usedSlots; }
		}

		public TValue this [TKey key] {
			get {
				int index = GetSlot (key);
				if (index < 0)
					throw new KeyNotFoundException ();
				return _table [index].Data.Value;
			}
			
			set {
				int index = GetSlot (key);
				if (index < 0)
					DoAdd (index, key, value);
				else
					_table [index].Data.Value = value;
			}
		}
	
		public Dictionary ()
		{
			Init (INITIAL_SIZE, null);
		}
	
		public Dictionary (IEqualityComparer<TKey> comparer)
		{
			Init (INITIAL_SIZE, comparer);
		}
	
		public Dictionary (IDictionary<TKey, TValue> dictionary)
			: this (dictionary, null)
		{
		}
	
		public Dictionary (int capacity)
		{
			Init (capacity, null);
		}
	
		public Dictionary (IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			int capacity = dictionary.Count;
			Init (capacity, comparer);
			foreach (KeyValuePair<TKey, TValue> entry in dictionary) {
				this.Add (entry.Key, entry.Value);
			}
		}
	
		public Dictionary (int capacity, IEqualityComparer<TKey> comparer)
		{
			Init (capacity, comparer);
		}
	
		protected Dictionary (SerializationInfo info, StreamingContext context)
		{
			_serializationInfo = info;
		}

		void SetThreshold ()
		{
			_threshold = (int)(_table.Length * DEFAULT_LOAD_FACTOR);
			if (_threshold == 0 && _table.Length > 0)
				_threshold = 1;
		}
		
		private void Init (int capacity, IEqualityComparer<TKey> hcp)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			this._hcp = (hcp != null) ? hcp : EqualityComparer<TKey>.Default;
			_table = new Slot [capacity];
			SetThreshold ();
		}

		void CopyTo (KeyValuePair<TKey, TValue> [] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index >= array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < _usedSlots)
				throw new ArgumentException ("Destination array cannot hold the requested elements!");
			
			for (int i = 0; i < _table.Length; ++i) {
				for (Slot slot = _table [i]; slot != null; slot = slot.Next)
					array [index++] = slot.Data;
			}
		}
	
		private void Resize ()
		{
			// From the SDK docs:
			//	 Hashtable is automatically increased
			//	 to the smallest prime number that is larger
			//	 than twice the current number of Hashtable buckets
			uint newSize = (uint) Hashtable.ToPrime ((_table.Length << 1) | 1);
		
			Slot nextslot = null;
			Slot [] oldTable = _table;
			
			_table = new Slot [newSize];
			SetThreshold ();

			int index;
			for (int i = 0; i < oldTable.Length; i++) {
				for (Slot slot = oldTable [i]; slot != null; slot = nextslot) {
					nextslot = slot.Next;

					index = DoHash (slot.Data.Key);
					slot.Next = _table [index];
					_table [index] = slot;
				}
			}
		}

		public void Add (TKey key, TValue value)
		{
			int index = GetSlot (key);
			if (index >= 0)
				throw new ArgumentException ("An element with the same key already exists in the dictionary.");

			DoAdd (index, key, value);
		}

		void DoAdd (int negated_index, TKey key, TValue value)
		{
			int index = -negated_index - 1;

			if (_usedSlots >= _threshold) {
				Resize ();
				index = DoHash (key);
			}

			_table [index] = new Slot (
				new KeyValuePair<TKey,TValue>(key, value), 
				_table [index]);
			++_usedSlots;
		}
	
		private int DoHash (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "null key");
	
			int size = this._table.Length;
			int h = _hcp.GetHashCode (key) & Int32.MaxValue;
			int spot = (int) ((uint) h % size);
			return spot;
		}

		public IEqualityComparer<TKey> Comparer {
			get {
				return _hcp;
			}
		}
		
		public void Clear ()
		{
			for (int i = 0; i < _table.Length; i++)
				_table [i] = null;
			_usedSlots = 0;
		}
	
		public bool ContainsKey (TKey key)
		{
			return GetSlot (key) >= 0;
		}
	
		public bool ContainsValue (TValue value)
		{
			IEqualityComparer<TValue> cmp = EqualityComparer<TValue>.Default;

			for (int i = 0; i < _table.Length; ++i) {
				for (Slot slot = _table [i]; slot != null; slot = slot.Next) {
					if (cmp.Equals (value, slot.Data.Value))
						return true;
				}
			}
			return false;
		}
	
		[SecurityPermission (SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("hcp", _hcp);
			KeyValuePair<TKey, TValue>[] data = null;
			if (_usedSlots > 0) {
				data = new KeyValuePair<TKey,TValue> [_usedSlots];
				CopyTo (data, 0);
			}
			info.AddValue ("data", data);
			info.AddValue ("buckets_hint", _table.Length);
		}
	
		public virtual void OnDeserialization (object sender)
		{
			if (_serializationInfo == null)
				return;

			_hcp = (IEqualityComparer<TKey>) _serializationInfo.GetValue ("hcp", typeof (IEqualityComparer<TKey>));
			KeyValuePair<TKey, TValue> [] data =
				(KeyValuePair<TKey, TValue> []) 
				_serializationInfo.GetValue ("data", typeof (KeyValuePair<TKey, TValue> []));

			int buckets = _serializationInfo.GetInt32 ("buckets_hint");
			if (buckets < INITIAL_SIZE)
				buckets = INITIAL_SIZE;

			_table = new Slot [buckets];
			SetThreshold ();
			_usedSlots = 0;

			if (data != null) {
				for (int i = 0; i < data.Length; ++i)
					Add (data [i].Key, data [i].Value);
			}
			_serializationInfo = null;
		}
	
		public bool Remove (TKey key)
		{
			int index = GetSlot (key);

			if (index < 0)
				return false;

			// If GetSlot returns a valid index, the given key is at the head of the chain.
			_table [index] = _table [index].Next;
			--_usedSlots;
			return true;
		}

		//
		// Returns the index of the chain containing key.	 Also ensures that the found key is the first element of the chain.
		// If the key is not found, returns -h-1, where 'h' is the index of the chain that would've contained the key.
		// 
		private int GetSlot (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			int index = DoHash (key);
			Slot slot = _table [index];
			Slot prev = null;

			while (slot != null && !slot.Data.Key.Equals (key)) {
				prev = slot;
				slot = slot.Next;
			}
	
			if (slot == null)
				return - index - 1;
	
			if (prev != null) {
				// Move to the head of the list
				prev.Next = slot.Next;
				slot.Next = _table [index];
				_table [index] = slot;
			}

			return index;
		}
	
		public bool TryGetValue (TKey key, out TValue value)
		{
			int index = GetSlot (key);
			bool found = index >= 0;
			value = found ? _table [index].Data.Value : default (TValue);
			return found;
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

		public KeyCollection Keys {
			get {
				return new KeyCollection (this);
			}
		}

		public ValueCollection Values {
			get {
				return new ValueCollection (this);
			}
		}
		ICollection IDictionary.Keys {
			get {
				return Keys;
			}
		}
		ICollection IDictionary.Values {
			get {
				return Values;
			}
		}

		bool IDictionary.IsFixedSize {
			get { return false; }
		}
		
		bool IDictionary.IsReadOnly {
			get { return false; }
		}
		
		object IDictionary.this [object key] {
			get {
				if (!(key is TKey))
					throw new ArgumentException ("key is of not '" + typeof (TKey).ToString () + "'!");
				return this [(TKey) key];
			}
			set { this [(TKey) key] = (TValue) value; }
		}
	
		void IDictionary.Add (object key, object value)
		{
			if (!(key is TKey))
				throw new ArgumentException ("key is of not '" + typeof (TKey).ToString () + "'!");
			if (!(value is TValue))
				throw new ArgumentException ("value is of not '" + typeof (TValue).ToString () + "'!");
			this.Add ((TKey) key, (TValue) value);
		}
	
		bool IDictionary.Contains (object key)
		{
			return ContainsKey ((TKey) key);
		}
	
		void IDictionary.Remove (object key)
		{
			Remove ((TKey) key);
		}
	
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		object ICollection.SyncRoot {
			get { return this; }
		}
	
		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}
	
		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> keyValuePair)
		{
			Add (keyValuePair.Key, keyValuePair.Value);
		}
	
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> keyValuePair)
		{
			return this.ContainsKey (keyValuePair.Key);
		}
	
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue> [] array, int index)
		{
			this.CopyTo (array, index);
		}
	
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> keyValuePair)
		{
			return Remove (keyValuePair.Key);
		}
	
	
		void ICollection.CopyTo (Array array, int index)
		{
			// TODO: Verify this can be a KeyValuePair, and doesn't need to be
			// a DictionaryEntry type
			CopyTo ((KeyValuePair<TKey, TValue> []) array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}
	
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this);
		}
	
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}
	
		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>,
			IDisposable, IDictionaryEnumerator, IEnumerator
		{
			Slot [] _dictionaryTable;

			Slot _current;
			int _nextIndex;
	
			internal Enumerator (Dictionary<TKey, TValue> dictionary)
			{
				_dictionaryTable = dictionary._table;

				// The following stanza is identical to IEnumerator.Reset (),
				// but because of the
				// definite assignment rule, we cannot call it here.
				_nextIndex = 0;
				_current = null;
			}

			public bool MoveNext ()
			{
				if (_dictionaryTable == null)
					throw new ObjectDisposedException(null);

				// Pre-condition: _current == null => this is the first call
				// to MoveNext ()
				if (_current != null)
					_current = _current.Next;

				while (_current == null && _nextIndex < _dictionaryTable.Length)
					_current = _dictionaryTable [_nextIndex++];

				// Post-condition: _current == null => this is the last call
				// to MoveNext()
				return _current != null;
			}

			public KeyValuePair<TKey, TValue> Current {
				get {
					VerifyState();
					return _current.Data;
				}
			}
	
			object IEnumerator.Current {
				get {
					VerifyState();
					return new DictionaryEntry(_current.Data.Key, _current.Data.Value);
				}
			}
	
			DictionaryEntry IDictionaryEnumerator.Entry {
				get {
					VerifyState();
					return new DictionaryEntry (_current.Data.Key, _current.Data.Value);
				}
			}
	
			void IEnumerator.Reset ()
			{
				_nextIndex = 0;
				_current = null;
			}
	
			object IDictionaryEnumerator.Key
			{
				get
				{
					VerifyState();
					return _current.Data.Key;
				}
			}
			object IDictionaryEnumerator.Value
			{
				get
				{
					VerifyState();
					return _current.Data.Value;
				}
			}

			void VerifyState()
			{
				if (_dictionaryTable == null)
					throw new ObjectDisposedException(null);
				if (_current == null)
					throw new InvalidOperationException();
			}	
			public void Dispose ()
			{
				_current = null;
				_dictionaryTable = null;
			}
		}
	
		// This collection is a read only collection
		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable {
			Dictionary<TKey, TValue> _dictionary;
	
			public KeyCollection (Dictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				_dictionary = dictionary;
			}
	
			public void CopyTo (TKey [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < _dictionary._usedSlots)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				IEnumerable<TKey> enumerateThis = (IEnumerable<TKey>) this;
				foreach (TKey k in enumerateThis)
					array [index++] = k;
			}
	
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dictionary.GetEnumerator ());
			}

			void ICollection<TKey>.Add (TKey item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
	
			void ICollection<TKey>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
	
			bool ICollection<TKey>.Contains (TKey item)
			{
				return _dictionary.ContainsKey (item);
			}
	
			bool ICollection<TKey>.Remove (TKey item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
	
			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((TKey []) array, index);
			}
	
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			public int Count {
				get { return _dictionary.Count; }
			}
	
			bool ICollection<TKey>.IsReadOnly {
				get { return true; }
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return ((ICollection) _dictionary).SyncRoot; }
			}
	
			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator {
				Dictionary<TKey, TValue>.Enumerator _hostEnumerator;

				internal Enumerator (Dictionary<TKey, TValue>.Enumerator hostEnumerator)
				{
					_hostEnumerator = hostEnumerator;
				}
				
				public void Dispose ()
				{
					_hostEnumerator.Dispose();
				}
				
				public bool MoveNext ()
				{
					return _hostEnumerator.MoveNext ();
				}
				
				public TKey Current {
					get {
						return _hostEnumerator.Current.Key;
					}
				}
				
				object IEnumerator.Current {
					get {
						return _hostEnumerator.Current.Key;
					}
				}
				
				void IEnumerator.Reset ()
				{
					((IEnumerator)_hostEnumerator).Reset ();
				}
			}
		}

		// This collection is a read only collection
		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable {
			Dictionary<TKey, TValue> _dictionary;
	
			public ValueCollection (Dictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				_dictionary = dictionary;
			}
	
			public void CopyTo (TValue [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < _dictionary._usedSlots)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				IEnumerable<TValue> enumerateThis = (IEnumerable<TValue>) this;
				foreach (TValue k in enumerateThis)
					array [index++] = k;
			}
	
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dictionary.GetEnumerator ());
			}

			void ICollection<TValue>.Add (TValue item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
	
			void ICollection<TValue>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}
	
			bool ICollection<TValue>.Contains (TValue item)
			{
				return _dictionary.ContainsValue (item);
			}
	
			bool ICollection<TValue>.Remove (TValue item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}
	
			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((TValue []) array, index);
			}
	
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			public int Count {
				get { return _dictionary.Count; }
			}
	
			bool ICollection<TValue>.IsReadOnly {
				get { return true; }
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return ((ICollection) _dictionary).SyncRoot; }
			}
	
			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator {
				Dictionary<TKey, TValue>.Enumerator _hostEnumerator;

				internal Enumerator (Dictionary<TKey,TValue>.Enumerator hostEnumerator)
				{
					_hostEnumerator = hostEnumerator;
				}
				
				public void Dispose ()
				{
					_hostEnumerator.Dispose();
				}
				
				public bool MoveNext ()
				{
					return _hostEnumerator.MoveNext ();
				}
				
				public TValue Current {
					get {
						return _hostEnumerator.Current.Value;
					}
				}
				
				object IEnumerator.Current {
					get {
						return _hostEnumerator.Current.Value;
					}
				}
				
				void IEnumerator.Reset ()
				{
					((IEnumerator)_hostEnumerator).Reset ();
				}
			}
		}
	}
}
#endif
