//
// System.Collections.Generic.SortedDictionary
//
// Authors:
//    Kazuki Oikawa (kazuki@panicode.com)
//    Atsushi Enomoto (atsushi@ximian.com)
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

namespace System.Collections.Generic
{
	[Serializable]
	public class SortedDictionary<TKey,TValue> : IDictionary<TKey,TValue>, ICollection<KeyValuePair<TKey,TValue>>, IEnumerable<KeyValuePair<TKey,TValue>>, IDictionary, ICollection, IEnumerable
	{
		TKey [] _keys;
		TValue [] _values;
		IComparer<TKey> _comparer;
		int _size;
		int _version = 0;
		const int DefaultCapacitySize = 4;

		KeyCollection _keyList;
		ValueCollection _valueList;

		#region Constructor
		public SortedDictionary () : this (0, null)
		{
		}

		public SortedDictionary (IComparer<TKey> comparer) : this (0, comparer)
		{
		}

		public SortedDictionary (IDictionary<TKey,TValue> dic) : this (dic, null)
		{
		}

		// it disappeared in 2.0 RTM
		SortedDictionary (int capacity) : this (capacity, null)
		{

		}

		public SortedDictionary (IDictionary<TKey,TValue> dic, IComparer<TKey> comparer) : this (dic == null ? 0 : dic.Count, comparer)
		{
			if (dic == null)
				throw new ArgumentNullException ();

			dic.Keys.CopyTo (_keys, 0);
			dic.Values.CopyTo (_values, 0);
			Array.Sort<TKey,TValue> (_keys, _values);
			_size = dic.Count;
		}

		// it disappeared in 2.0 RTM
		SortedDictionary (int capacity, IComparer<TKey> comparer)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ();

			_keys = new TKey [capacity];
			_values = new TValue [capacity];
			_size = 0;

			if (comparer == null)
				_comparer = Comparer<TKey>.Default;
			else
				_comparer = comparer;
		}
		#endregion

		#region PublicProperty

		public IComparer<TKey> Comparer {
			get { return _comparer; }
		}

		// It disappeared in 2.0 RTM.
		int Capacity {
			get { return _keys.Length; }
			set {
				if (value < _size)
					throw new ArgumentOutOfRangeException ();
				
				Array.Resize<TKey> (ref _keys, value);
				Array.Resize<TValue> (ref _values, value);
			}
		}

		public int Count {
			get { return _size; }
		}

		public TValue this [TKey key] {
			get {
				int index = IndexOfKey (key);
				if (index >= 0)
					return _values [index];
				
				throw new KeyNotFoundException ();
			}
			set {
				if (key == null)
					throw new ArgumentNullException ();
				
				int index = IndexOfKey (key);
				if (index < 0)
					Add (key, value);
				else
					_values [index] = value;
			}
		}

		public KeyCollection Keys {
			get { return GetKeyCollection (); }
		}

		public ValueCollection Values {
			get { return GetValueCollection (); }
		}
		#endregion

		#region PublicMethod

		public void Add (TKey key, TValue value)
		{
			if (key == null) 
				throw new ArgumentNullException ();
			
			int index = Array.BinarySearch<TKey> (_keys, 0, _size, key, _comparer);
			if (index >= 0)
				throw new ArgumentException ();

			index = ~index;

			if (_size == _keys.Length)
				Capacity += Capacity > 0 ? Capacity : DefaultCapacitySize;

			if (index < _size) {
				Array.Copy (_keys, index, _keys, index + 1, _size - index);
				Array.Copy (_values, index, _values, index + 1, _size - index);
			}

			_keys [index] = key;
			_values [index] = value;
			_size++;
			_version++;
		}

		public void Clear ()
		{
			Array.Clear (_keys, 0, _size);
			Array.Clear (_values, 0, _size);
			_size = 0;
			_version++;
		}

		public bool ContainsKey (TKey key)
		{
			return IndexOfKey (key) >= 0;
		}

		public bool ContainsValue (TValue value)
		{
			return IndexOfValue (value) >= 0;
		}

		public void CopyTo (KeyValuePair<TKey,TValue>[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ();
			if (arrayIndex < 0 || array.Length <= arrayIndex)
				throw new ArgumentOutOfRangeException ();
			if (array.Length - arrayIndex < _size)
				throw new ArgumentException ();

			for (int i = 0; i < _size; i ++) {
				array [arrayIndex + i] = new KeyValuePair<TKey,TValue> (_keys [i], _values [i]);
			}
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		int IndexOfKey (TKey key)
		{
			if (key == null)
				throw new ArgumentNullException ();

			return Array.BinarySearch<TKey> (_keys, 0, _size, key, _comparer);
		}

		int IndexOfValue (TValue value)
		{
			return Array.IndexOf<TValue> (_values, value, 0, _size);
		}

		public bool Remove (TKey key)
		{
			int index = IndexOfKey (key);
			if (index >= 0) {
				RemoveAt (index);
				return true;
			}
			return false;
		}

		void RemoveAt (int index)
		{
			if (index < 0 || _size <= index)
				throw new ArgumentOutOfRangeException ();

			_size--;
			if (index < _size) {
				Array.Copy (_keys, index + 1, _keys, index, _size - index);
				Array.Copy (_values, index + 1, _values, index, _size - index);
			}

			_keys[_size] = default (TKey) ;
			_values[_size] = default (TValue) ;
			_version++;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			int index = IndexOfKey (key);
			if (index >= 0) {
				value = _values [index];
				return true;
			}

			value = default (TValue) ;
			return false;			
		}

		#endregion

		#region PrivateMethod
		private KeyCollection GetKeyCollection ()
		{
			if (_keyList == null)
				_keyList = new KeyCollection (this);
			return _keyList;
		}
		private ValueCollection GetValueCollection ()
		{
			if (_valueList == null)
				_valueList = new ValueCollection (this);
			return _valueList;
		}

		TKey ToKey (object key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (!(key is TKey))
				throw new ArgumentException (String.Format ("Key \"{0}\" cannot be converted to the key type {1}.", key, typeof (TKey)));
			return (TKey) key;
		}

		TValue ToValue (object value)
		{
			if (!(value is TValue) && (value != null || typeof (TValue).IsValueType))
				throw new ArgumentException (String.Format ("Value \"{0}\" cannot be converted to the value type {1}.", value, typeof (TValue)));
			return (TValue) value;
		}
		#endregion

		#region IDictionary<TKey,TValue> Member

		ICollection<TKey> IDictionary<TKey,TValue>.Keys {
			get { return GetKeyCollection (); }
		}

		ICollection<TValue> IDictionary<TKey,TValue>.Values {
			get { return GetValueCollection (); }
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Member

		void ICollection<KeyValuePair<TKey,TValue>>.Add (KeyValuePair<TKey,TValue> item)
		{
			Add (item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.Contains (KeyValuePair<TKey,TValue> item)
		{
			int index = IndexOfKey (item.Key);
			return index >= 0 && Comparer<TValue>.Default.Compare (_values [index], item.Value) == 0;
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.IsReadOnly {
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey,TValue>>.Remove (KeyValuePair<TKey,TValue> item)
		{
			int index = IndexOfKey (item.Key);
			if (index >= 0 && Comparer<TValue>.Default.Compare (_values [index], item.Value) == 0) {
				RemoveAt (index);
				return true;
			}
			return false;
		}

		#endregion

		#region IDictionary Member

		void IDictionary.Add (object key, object value)
		{
			Add (ToKey (key), ToValue (value));
		}

		bool IDictionary.Contains (object key)
		{
			return ContainsKey (ToKey (key));
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		bool IDictionary.IsFixedSize {
			get { return false; }
		}

		bool IDictionary.IsReadOnly {
			get { return false; }
		}

		ICollection IDictionary.Keys  {
			get { return GetKeyCollection (); }
		}

		void IDictionary.Remove (object key)
		{
			Remove (ToKey (key));
		}
		ICollection IDictionary.Values {
			get { return GetValueCollection (); }
		}

		object IDictionary.this [object key] {
			get {
				return this [ToKey (key)];
			}
			set {
				if (!(value is TValue))
					throw new ArgumentException ();

				this [ToKey (key)] = ToValue (value);
			}
		}

		#endregion

		#region ICollection Member

		void ICollection.CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ();
			if (index < 0 || array.Length <= index)
				throw new ArgumentOutOfRangeException ();
			if (array.Length - index < _size)
				throw new ArgumentException ();

			for (int i = 0; i < _size; i ++) {
				array.SetValue (new KeyValuePair<TKey,TValue> (_keys [i], _values [i]), i);
			}
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		// TODO:Is this correct? If this is wrong,please fix.
		object ICollection.SyncRoot {
			get { return this; }
		}

		#endregion

		#region IEnumerable Member

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		#endregion

		#region IEnumerable<TKey> Member

		IEnumerator<KeyValuePair<TKey,TValue>> IEnumerable<KeyValuePair<TKey,TValue>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		#endregion

		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>,
			IEnumerable<TValue>, ICollection, IEnumerable
		{
			SortedDictionary<TKey,TValue> _dic;

			public ValueCollection (SortedDictionary<TKey,TValue> dic)
			{
				_dic = dic;
			}

			void ICollection<TValue>.Add (TValue item)
			{
				throw new NotSupportedException ();
			}

			void ICollection<TValue>.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool ICollection<TValue>.Contains (TValue item)
			{
				return _dic.ContainsValue (item);
			}

			public void CopyTo (TValue [] array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException ();
				if (arrayIndex < 0 || array.Length <= arrayIndex)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - arrayIndex < _dic._size)
					throw new ArgumentException ();
				Array.Copy (_dic._values, 0, array, arrayIndex, _dic._size);
			}

			public int Count {
				get { return _dic._size; }
			}

			bool ICollection<TValue>.IsReadOnly {
				get { return true; }
			}

			bool ICollection<TValue>.Remove (TValue item)
			{
				throw new NotSupportedException ();
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dic);
			}
		
			void ICollection.CopyTo(Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || array.Length <= index)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - index < _dic._size)
					throw new ArgumentException ();
				Array.Copy (_dic._values, 0, array, index, _dic._size);
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return _dic; }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
 				return new Enumerator (_dic);
			}

			public struct Enumerator : IEnumerator<TValue>,IEnumerator, IDisposable
			{
				SortedDictionary<TKey,TValue> _dic;
				int _version;
				int _index;
				int _count;

				internal Enumerator (SortedDictionary<TKey,TValue> dic)
				{
					_dic = dic;
					_version = dic._version;
					_index = -1;
					_count = dic._size;
				}

				public TValue Current {
					get {
						if (_count <= _index)
							throw new InvalidOperationException ();
						return _dic._values [_index];
					}
				}

				public bool MoveNext ()
				{
					if (_version != _dic._version)
						throw new InvalidOperationException ();

					if (_index + 1 < _count) {
						_index ++;
						return true;
					}

					return false;
				}

				public void Dispose ()
				{
					_dic = null;
				}

				object IEnumerator.Current {
					get { return Current; }
				}

				void IEnumerator.Reset ()
				{
					if (_version != _dic._version)
						throw new InvalidOperationException ();
					_index = -1;
				}
			}
		}

		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>,
			IEnumerable<TKey>, ICollection, IEnumerable
		{
			SortedDictionary<TKey,TValue> _dic;

			public KeyCollection (SortedDictionary<TKey,TValue> dic)
			{
				_dic = dic;
			}

			void ICollection<TKey>.Add (TKey item)
			{
				throw new NotSupportedException ();
			}

			void ICollection<TKey>.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool ICollection<TKey>.Contains (TKey item)
			{
				return _dic.ContainsKey (item);
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public void CopyTo (TKey [] array, int arrayIndex)
			{
				Array.Copy (_dic._keys, 0, array, arrayIndex, _dic._size);
			}

			public int Count {
				get { return _dic._size; }
			}

			bool ICollection<TKey>.IsReadOnly {
				get { return true; }
			}

			bool ICollection<TKey>.Remove (TKey item)
			{
				throw new NotSupportedException ();
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (_dic);
			}

			void ICollection.CopyTo (Array array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ();
				if (index < 0 || array.Length <= index)
					throw new ArgumentOutOfRangeException ();
				if (array.Length - index < _dic._size)
					throw new ArgumentException ();
				Array.Copy (_dic._keys, 0, array, index, _dic._size);
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return _dic; }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
 				return new Enumerator (_dic);
			}

			public struct Enumerator : IEnumerator<TKey>, IEnumerator, IDisposable
			{
				SortedDictionary<TKey,TValue> _dic;
				int _version;
				int _index;
				int _count;

				internal Enumerator (SortedDictionary<TKey,TValue> dic)
				{
					_dic = dic;
					_version = dic._version;
					_index = -1;
					_count = dic._size;
				}

				public TKey Current {
					get {
						if (_count <= _index)
							throw new InvalidOperationException ();
						return _dic._keys [_index];
					}
				}

				public bool MoveNext ()
				{
					if (_version != _dic._version)
						throw new InvalidOperationException ();

					if (_index + 1 < _count) {
						_index ++;
						return true;
					}

					return false;
				}

				public void Dispose ()
				{
					_dic = null;
				}

				object IEnumerator.Current {
					get { return Current; }
				}

				void IEnumerator.Reset ()
				{
					if (_version != _dic._version)
						throw new InvalidOperationException ();
					_index = -1;
				}
			}

		}

		public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>, IDisposable , IDictionaryEnumerator, IEnumerator
		{
			SortedDictionary<TKey,TValue> _dic;
			int _version;
			int _index;
			int _count;

			internal Enumerator (SortedDictionary<TKey,TValue> dic)
			{
				_dic = dic;
				_version = dic._version;
				_index = -1;
				_count = dic._size;
			}

			public KeyValuePair<TKey,TValue> Current {
				get {
					if (_count <= _index)
						throw new InvalidOperationException ();
					return new KeyValuePair<TKey,TValue> (_dic._keys [_index], _dic._values [_index]);
				}
			}

			public bool MoveNext ()
			{
				if (_version != _dic._version)
					throw new InvalidOperationException ();

				if (_index + 1 < _count) {
					_index ++;
					return true;
				}

				return false;
			}

			public void Dispose ()
			{
				_dic = null;
			}

			DictionaryEntry IDictionaryEnumerator.Entry {
				get {
					if (_count <= _index)
						throw new InvalidOperationException ();
					return new DictionaryEntry (_dic._keys [_index], _dic._values [_index]);
				}
			}

			object IDictionaryEnumerator.Key {
				get {
					if (_count <= _index)
						throw new InvalidOperationException ();
					return _dic._keys [_index];
				}
			}

			object IDictionaryEnumerator.Value {
				get {
					if (_count <= _index)
						throw new InvalidOperationException ();
					return _dic._values [_index];
				}
			}

			object IEnumerator.Current {
				get {
					if (_count <= _index)
						throw new InvalidOperationException ();
					return new DictionaryEntry (_dic._keys [_index], _dic._values [_index]);
				}
			}

			void IEnumerator.Reset ()
			{
				if (_version != _dic._version)
					throw new InvalidOperationException ();

				_index = -1;
			}
		}
	}
}
#endif
