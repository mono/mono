//
// System.Collections.Generic.Dictionary
//
// Authors:
//	Sureshkumar T (tsureshkumar@novell.com)
//	Marek Safar (marek.safar@seznam.cz) (stubs)
//	Ankit Jain (radical@corewars.org)
//
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace System.Collections.Generic {

	[Serializable]
	[CLSCompliant(true)]
	public class Dictionary<K, V> : IDictionary<K, V>,
		//ICollection<KeyValuePair<K, V>>,
		IEnumerable<KeyValuePair<K, V>>,
		IDictionary,
		ICollection,
		IEnumerable,
		ISerializable,
		IDeserializationCallback
	{
		const int INITIAL_SIZE = 10;
		const float DEFAULT_LOAD_FACTOR = (90f / 100);

		[Serializable]
		internal class Slot {
			public K Key;
			public V Value;
			public Slot next;
			public Slot (K Key, V Value, Slot next)
			{
				this.Key = Key;
				this.Value = Value;
				this.next = next;
			}
		}
	
		Slot [] _table;
	
		int _usedSlots;
		float _loadFactor = DEFAULT_LOAD_FACTOR;
	
		IComparer<K> _hcp;
	
		uint _threshold;
	
		public int Count {
			get { return _usedSlots; }
		}

		public V this [K key] {
			get {
				int index = GetSlot (key);
				if (index < 0)
					throw new KeyNotFoundException ();
				return _table [index].Value;
			}
			
			set {
				int index = GetSlot (key);
				if (index < 0)
					DoAdd (index, key, value);
				else
					_table [index].Value = value;
			}
		}
	
		public Dictionary ()
		{
			Init ();
		}
	
		public Dictionary (IComparer<K> comparer)
		{
			Init (INITIAL_SIZE, comparer, DEFAULT_LOAD_FACTOR);
		}
	
		public Dictionary (IDictionary<K, V> dictionary)
			: this (dictionary, null)
		{
		}
	
		public Dictionary (int capacity)
		{
			Init (capacity);
		}
	
		public Dictionary (float loadFactor)
		{
			Init (loadFactor);
		}
	
		public Dictionary (IDictionary<K, V> dictionary, IComparer<K> comparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			int capacity = dictionary.Count;
			Init (capacity, comparer, DEFAULT_LOAD_FACTOR);
			foreach (KeyValuePair<K, V> entry in dictionary) {
				this.Add (entry.Key, entry.Value);
			}
		}
	
		public Dictionary (int capacity, IComparer<K> comparer)
		{
			Init (capacity, comparer, DEFAULT_LOAD_FACTOR);
		}
	
		protected Dictionary (SerializationInfo info, StreamingContext context)
		{
			Init ();
		}
	
		void Init ()
		{
			Init (INITIAL_SIZE, null, DEFAULT_LOAD_FACTOR);
		}
	
		void Init (int capacity)
		{
			Init (capacity, null, DEFAULT_LOAD_FACTOR);
		}
	
		void Init (float loadFactor)
		{
			Init (INITIAL_SIZE, null, loadFactor);
		}
		
		protected void Init (int capacity, IComparer<K> hcp, float loadFactor)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			this._hcp = hcp;
			_table = new Slot [capacity];
			_loadFactor = loadFactor;
			_threshold = (uint) (capacity * _loadFactor);
			if (_threshold == 0 && capacity > 0)
				_threshold = 1;
		}
	
		ICollection<V> GetValues ()
		{
			return ((IDictionary<K, V>) this).Values;
		}
	
		ICollection<K> GetKeys ()
		{
			return ((IDictionary<K, V>) this).Keys;
		}
	
	
		void CopyTo (KeyValuePair<K, V> [] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index >= array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < _usedSlots)
				throw new ArgumentException ("Destination array cannot hold the requested elements!");
			
			foreach (KeyValuePair<K, V> kv in this)
				array [index++] = kv;
		}
	
		protected void Resize ()
		{
			// From the SDK docs:
			//	 Hashtable is automatically increased
			//	 to the smallest prime number that is larger
			//	 than twice the current number of Hashtable buckets
			uint newSize = (uint) ToPrime ((_table.Length << 1) | 1);

			_threshold = (uint) (newSize * _loadFactor);
			if (_threshold == 0 && newSize > 0)
				_threshold = 1;
		
			Slot nextslot = null;
			Slot [] oldTable = _table;
			
			_table = new Slot [newSize];

			int index;
			for (int i = 0; i < oldTable.Length; i++) {
				for (Slot slot = oldTable [i]; slot != null; slot = nextslot) {
					nextslot = slot.next;

					index = DoHash (slot.Key);
					slot.next = _table [index];
					_table [index] = slot;
				}
			}
		}
	
		protected virtual int GetHash (K key)
		{
			IComparer<K> hcp = this._hcp;
			
			return key.GetHashCode ();
			/*
			return (hcp != null)
			? hcp.GetHashCode (key)
			: key.GetHashCode ();
			*/
		}
	
		public void Add (K key, V value)
		{
			int index = GetSlot (key);
			if (index >= 0)
				throw new ArgumentException ("An element with the same key already exists in the dictionary.");

			DoAdd (index, key, value);
		}

		void DoAdd (int negated_index, K key, V value)
		{
			int index = -negated_index - 1;

			if (_usedSlots >= _threshold) {
				Resize ();
				index = DoHash (key);
			}

			_table [index] = new Slot (key, value, _table [index]);
			++_usedSlots;
		}
	
		protected int DoHash (K key)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "null key");
	
			int size = this._table.Length;
			int h = this.GetHash (key) & Int32.MaxValue;
			//Console.WriteLine ("Hashvalue for key {0} is {1}", key.ToString (), h);
			int spot = (int) ((uint) h % size);
			return spot;
		}
	
		public void Clear ()
		{
			for (int i = 0; i < _table.Length; i++)
				_table [i] = null;
			_usedSlots = 0;
		}
	
		public bool ContainsKey (K key)
		{
			return GetSlot (key) >= 0;
		}
	
		public bool ContainsValue (V value)
		{
			foreach (V v in ((IDictionary) this).Values) {
				if (v.Equals (value))
					return true;
			}
			return false;
		}
	
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	
		public virtual void OnDeserialization (object sender)
		{
			throw new NotImplementedException ();
		}
	
		public bool Remove (K key)
		{
			int index = GetSlot (key);

			if (index < 0)
				return false;

			// If GetSlot returns a valid index, the given key is at the head of the chain.
			_table [index] = _table [index].next;
			--_usedSlots;
			return true;
		}

		//
		// Returns the index of the chain containing key.  Also ensures that the found key is the first element of the chain.
		// If the key is not found, returns -h-1, where 'h' is the index of the chain that would've contained the key.
		// 
		internal int GetSlot (K key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			int index = DoHash (key);
			Slot slot = _table [index];
			Slot prev = null;

			while (slot != null && !slot.Key.Equals (key)) {
				prev = slot;
				slot = slot.next;
			}
	
			if (slot == null)
				return - index - 1;
	
			if (prev != null) {
				// Move to the head of the list
				prev.next = slot.next;
				slot.next = _table [index];
				_table [index] = slot;
			}

			return index;
		}
	
		public bool TryGetValue (K key, out V value)
		{
			int index = GetSlot (key);
			bool found = index >= 0;
			value = found ? _table [index].Value : default (V);
			return found;
		}
	
		ICollection<K> IDictionary<K, V>.Keys {
			get { return new HashKeyCollection (this); }
		}
	
		ICollection<V> IDictionary<K, V>.Values {
			get { return new HashValueCollection (this); }
		}
		
		bool IDictionary.IsFixedSize {
			get { return false; }
		}
		
		bool IDictionary.IsReadOnly {
			get { return false; }
		}
		
		object IDictionary.this [object key] {
			get {
				if (!(key is K))
					throw new ArgumentException ("key is of not '" + typeof (K).ToString () + "'!");
				return this [(K) key];
			}
			set { this [(K) key] = (V) value; }
		}
		ICollection IDictionary.Keys
		{
			get { return ((IDictionary<K, V>) this).Keys as ICollection; }
		}
		ICollection IDictionary.Values
		{
			get { return ((IDictionary<K, V>) this).Values as ICollection; }
		}
	
		void IDictionary.Add (object key, object value)
		{
			if (!(key is K))
				throw new ArgumentException ("key is of not '" + typeof (K).ToString () + "'!");
			if (!(value is V))
				throw new ArgumentException ("value is of not '" + typeof (V).ToString () + "'!");
			this.Add ((K) key, (V) value);
		}
	
		bool IDictionary.Contains (object key)
		{
			return ContainsKey ((K) key);
		}
	
		void IDictionary.Remove (object key)
		{
			Remove ((K) key);
		}
	
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		object ICollection.SyncRoot {
			get { return this; }
		}
	
		bool ICollection<KeyValuePair<K, V>>.IsReadOnly {
			get { return false; }
		}
	
		void ICollection<KeyValuePair<K, V>>.Add (KeyValuePair<K, V> keyValuePair)
		{
			Add (keyValuePair.Key, keyValuePair.Value);
		}
	
		bool ICollection<KeyValuePair<K, V>>.Contains (KeyValuePair<K, V> keyValuePair)
		{
			return this.ContainsKey (keyValuePair.Key);
		}
	
		void ICollection<KeyValuePair<K, V>>.CopyTo (KeyValuePair<K, V> [] array, int index)
		{
			CopyTo (array, index);
		}
	
		bool ICollection<KeyValuePair<K, V>>.Remove (KeyValuePair<K, V> keyValuePair)
		{
			return Remove (keyValuePair.Key);
		}
	
	
		void ICollection.CopyTo (Array array, int index)
		{
			CopyTo ((KeyValuePair<K, V> []) array, index);
		}
	
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this, EnumerationMode.DictionaryEntry);
		}
	
		IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}
	
		/**
		 * This is to make the gmcs compiler errror silent
		 */
	//			   IEnumerator<K> IEnumerable<K>.GetEnumerator ()
	//			   {
	//					   throw new NotImplementedException ();
	//			   }
	
	
		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this, EnumerationMode.DictionaryEntry);
		}
	
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this, EnumerationMode.KeyValuePair);
		}
	
		public enum EnumerationMode { Key, Value, DictionaryEntry, KeyValuePair };
	
		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<K,V>>,
			IDisposable, IDictionaryEnumerator, IEnumerator
		{
			Dictionary<K, V> _dictionary;
			Slot _current;
			int _index;
			int _validNodeVisited;
			bool _isValid;
			EnumerationMode _navigationMode;
	
	
	
			public Enumerator (Dictionary<K, V> dictionary) : this (dictionary, EnumerationMode.KeyValuePair)
			{
			}
	
			public Enumerator (Dictionary<K, V> dictionary, EnumerationMode mode)
			{
				_index = 0;
				_current = null;
				_validNodeVisited = 0;
				_dictionary = dictionary;
				_isValid = false;
				_navigationMode = mode;
			}
	
			public bool MoveNext ()
			{
				if (_validNodeVisited == _dictionary.Count)
					return (_isValid = false);
	
				while (_index < _dictionary._table.Length) {
					if (_current == null)
						_current = _dictionary._table [_index++];
					else
						_current = _current.next;
	
					if (_current != null) {
						++_validNodeVisited;
						return (_isValid = true);
					}
				}
	
				return (_isValid = false);
			}
	
			public KeyValuePair<K, V> Current {
				get {
					if (!_isValid) throw new InvalidOperationException ();
					KeyValuePair<K, V> kv = new KeyValuePair<K, V> (_current.Key, _current.Value);
					return kv;
				}
			}
	
			object IEnumerator.Current {
				get {
					if (!_isValid) throw new InvalidOperationException ();
					switch (_navigationMode) {
					case EnumerationMode.Key:
						return _current.Key as object;
					case EnumerationMode.Value:
						return _current.Value as object;
					case EnumerationMode.DictionaryEntry:
						DictionaryEntry de = new DictionaryEntry (_current.Key, _current.Value);
						return de as object;
					case EnumerationMode.KeyValuePair:
					default:
						KeyValuePair<K, V> kv = new KeyValuePair<K, V> (_current.Key, _current.Value);
						return kv as object;
					}
				}
			}
	
			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					if (!_isValid) throw new InvalidOperationException ();
					DictionaryEntry entry = new DictionaryEntry (_current.Key, _current.Value);
					return entry;
				}
			}
	
			void IEnumerator.Reset ()
			{
				_index = 0;
				_current = null;
				_isValid = false;
				_validNodeVisited = 0;
			}
	
			object IDictionaryEnumerator.Key
			{
				get
				{
					if (!_isValid) throw new InvalidOperationException ();
					return _current.Key;
				}
			}
			object IDictionaryEnumerator.Value
			{
				get
				{
					if (!_isValid) throw new InvalidOperationException ();
					return _current.Value;
				}
			}
	
			public void Dispose ()
			{
	
			}
		}
	
		// This collection is a read only collection
		internal class HashKeyCollection : ICollection<K>, IEnumerable<K>, ICollection {
			Dictionary<K, V> _dictionary;
	
			public HashKeyCollection (Dictionary<K, V> dictionary)
			{
				_dictionary = dictionary;
			}
	
			void ICollection<K>.Add (K item)
			{
				throw new InvalidOperationException ();
			}
	
			void ICollection<K>.Clear ()
			{
				throw new InvalidOperationException ();
			}
	
			bool ICollection<K>.Contains (K item)
			{
				return _dictionary.ContainsKey (item);
			}
	
			bool ICollection<K>.Remove (K item)
			{
				throw new InvalidOperationException ();
			}
	
			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((K []) array, index);
			}
	
			public void CopyTo (K [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < _dictionary._usedSlots)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				IEnumerable<K> enumerateThis = (IEnumerable<K>) this;
				foreach (K k in enumerateThis) {
					array [index++] = k;
				}
			}
	
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dictionary);
			}
	
			IEnumerator<K> IEnumerable<K>.GetEnumerator ()
			{
				return new KeyEnumerator (_dictionary);
			}
	
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new Enumerator (_dictionary, EnumerationMode.Key);
			}
	
	
			bool ICollection<K>.IsReadOnly { get { return ((IDictionary) _dictionary).IsReadOnly; } }
			public int Count { get { return _dictionary.Count; } }
			bool ICollection.IsSynchronized { get { return ((IDictionary) _dictionary).IsSynchronized; } }
			object ICollection.SyncRoot { get { return ((IDictionary) _dictionary).SyncRoot; } }
	
			public struct KeyEnumerator : IEnumerator<K>, IDisposable, IEnumerator {
				IEnumerator _hostEnumerator;
				internal KeyEnumerator (Dictionary<K, V> dictionary)
				{
					_hostEnumerator = new Enumerator (dictionary, EnumerationMode.Key);
				}
				
				public void Dispose ()
				{
				}
				
				public bool MoveNext ()
				{
					return _hostEnumerator.MoveNext ();
				}
				
				public K Current {
					get {
						return (K) _hostEnumerator.Current;
					}
				}
				
				object IEnumerator.Current {
					get {
						return _hostEnumerator.Current;
					}
				}
				
				void IEnumerator.Reset ()
				{
					_hostEnumerator.Reset ();
				}
			}
		}
	
		// This collection is a read only collection
		internal class HashValueCollection : ICollection<V>, IEnumerable<V>, ICollection {
			Dictionary<K, V> _dictionary;
	
			public HashValueCollection (Dictionary<K, V> dictionary)
			{
				_dictionary = dictionary;
			}
	
			void ICollection<V>.Add (V item)
			{
				throw new InvalidOperationException ();
			}
	
			void ICollection<V>.Clear ()
			{
				throw new InvalidOperationException ();
			}
	
			bool ICollection<V>.Contains (V item)
			{
				return _dictionary.ContainsValue (item);
			}
	
			bool ICollection<V>.Remove (V item)
			{
				throw new InvalidOperationException ();
			}
	
			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((V []) array, index);
			}
	
			public void CopyTo (V [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < _dictionary._usedSlots)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				IEnumerable<V> enumerateThis = (IEnumerable<V>) this;
				foreach (V v in enumerateThis) {
					array [index++] = v;
				}
			}
	
			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dictionary);
			}
	
			IEnumerator<V> IEnumerable<V>.GetEnumerator ()
			{
				return new ValueEnumerator (_dictionary);
			}
	
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new Enumerator (_dictionary, EnumerationMode.Value);
			}
	
	
			bool ICollection<V>.IsReadOnly { get { return ((IDictionary) _dictionary).IsReadOnly; } }
			public int Count { get { return _dictionary.Count; } }
			bool ICollection.IsSynchronized { get { return ((IDictionary) _dictionary).IsSynchronized; } }
			object ICollection.SyncRoot { get { return ((IDictionary) _dictionary).SyncRoot; } }
	
			public struct ValueEnumerator : IEnumerator<V>, IDisposable, IEnumerator
			{
				IEnumerator _hostEnumerator;
				internal ValueEnumerator (Dictionary<K, V> dictionary)
				{
					_hostEnumerator = new Enumerator (dictionary, EnumerationMode.Value);
				}
				
				public void Dispose ()
				{
				}
				
				public bool MoveNext ()
				{
					return _hostEnumerator.MoveNext ();
				}
				
				public V Current {
					get {
						return (V) _hostEnumerator.Current;
					}
				}
				
				object IEnumerator.Current {
					get {
						return _hostEnumerator.Current;
					}
				}
				
				void IEnumerator.Reset ()
				{
					_hostEnumerator.Reset ();
				}
	
			}
		}
	
	
		static bool TestPrime (int x)
		{
			if ((x & 1) != 0) {
				for (int n = 3; n < (int) Math.Sqrt (x); n += 2) {
					if ((x % n) == 0)
						return false;
				}
				return true;
			}
			// There is only one even prime - 2.
			return (x == 2);
		}
	
		static int CalcPrime (int x)
		{
			for (int i = (x & (~1)) - 1; i < Int32.MaxValue; i += 2) {
				if (TestPrime (i)) return i;
			}
			return x;
		}
	
		static int ToPrime (int x)
		{
			for (int i = 0; i < primeTbl.Length; i++) {
				if (x <= primeTbl [i])
					return primeTbl [i];
			}
			return CalcPrime (x);
		}
	
		static readonly int [] primeTbl = {
			11,
			19,
			37,
			73,
			109,
			163,
			251,
			367,
			557,
			823,
			1237,
			1861,
			2777,
			4177,
			6247,
			9371,
			14057,
			21089,
			31627,
			47431,
			71143,
			106721,
			160073,
			240101,
			360163,
			540217,
			810343,
			1215497,
			1823231,
			2734867,
			4102283,
			6153409,
			9230113,
			13845163
		};
	}
}
#endif

