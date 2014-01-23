//
// ReadOnlyDictionary.cs
//
// Authors:
//       Martin Baulig <martin.baulig@xamarin.com>
//       Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_5
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.ObjectModel {

	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary,
		IReadOnlyDictionary<TKey, TValue>
	{
		readonly IDictionary<TKey, TValue> inner;

		public ReadOnlyDictionary (IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");

			this.inner = dictionary;
		}

		protected IDictionary<TKey, TValue> Dictionary {
			get {
				return inner;
			}
		}

		public bool ContainsKey (TKey key)
		{
			return inner.ContainsKey (key);
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			return inner.TryGetValue (key, out value);
		}

		public TValue this [TKey key] {
			get {
				return inner [key];
			}
		}

		public KeyCollection Keys {
			get {
				return new KeyCollection (inner.Keys);
			}
		}

		public ValueCollection Values {
			get {
				return new ValueCollection (inner.Values);
			}
		}

		#region IEnumerable<KeyValuePair<TKey, TValue>> implementation
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			return inner.GetEnumerator ();
		}
		#endregion
		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return inner.GetEnumerator ();
		}
		#endregion

		#region IDictionary<TKey, TValue> implementation

		void IDictionary<TKey, TValue>.Add (TKey key, TValue value)
		{
			throw new NotSupportedException ();
		}

		bool IDictionary<TKey, TValue>.Remove (TKey key)
		{
			throw new NotSupportedException ();
		}

		TValue IDictionary<TKey, TValue>.this [TKey key] {
			get {
				return inner [key];
			}
			set {
				throw new NotSupportedException ();
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

		#endregion

		#region IDictionary implementation

		void IDictionary.Add (object key, object value)
		{
			throw new NotSupportedException ();
		}

		void IDictionary.Clear ()
		{
			throw new NotSupportedException ();
		}

		bool IDictionary.Contains (object key)
		{
			return ((IDictionary)inner).Contains (key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return ((IDictionary)inner).GetEnumerator ();
		}

		void IDictionary.Remove (object key)
		{
			throw new NotSupportedException ();
		}

		bool IDictionary.IsFixedSize {
			get {
				return true;
			}
		}

		bool IDictionary.IsReadOnly {
			get {
				return true;
			}
		}

		object IDictionary.this [object key] {
			get {
				return ((IDictionary)inner)[key];
			}
			set {
				throw new NotSupportedException ();
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

		#endregion

		#region ICollection implementation

		void ICollection.CopyTo (Array array, int index)
		{
			((ICollection)inner).CopyTo (array, index);
		}

		#endregion

		#region ICollection<KeyValuePair<TKey, TValue>> implementation

		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException ();
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear ()
		{
			throw new NotSupportedException ();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)inner).Contains (item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)inner).CopyTo (array, arrayIndex);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException ();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get {
				return true;
			}
		}

		#endregion

		#region ICollection implementation

		public int Count {
			get {
				return inner.Count;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		object ICollection.SyncRoot {
			get {
				throw new NotSupportedException ();
			}
		}

		#endregion

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
			get {
				return Keys;
			}
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
			get {
				return Values;
			}
		}

		[Serializable]
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]		
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
		{
			readonly ICollection<TKey> collection;

			internal KeyCollection (ICollection<TKey> collection)
			{
				this.collection = collection;
			}

			public void CopyTo (TKey [] array, int arrayIndex)
			{
				collection.CopyTo (array, arrayIndex);
			}

			public IEnumerator<TKey> GetEnumerator ()
			{
				return collection.GetEnumerator ();
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
				return collection.Contains (item);
			}

			bool ICollection<TKey>.Remove (TKey item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			void ICollection.CopyTo (Array array, int index)
			{
				var target = array as TKey [];
				if (target != null) {
					CopyTo (target, index);
					return;
				}

				throw new NotImplementedException ();
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return collection.GetEnumerator ();
			}

			public int Count {
				get {
					return collection.Count;
				}
			}

			bool ICollection<TKey>.IsReadOnly {
				get {
					return true;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return false;
				}
			}

			object ICollection.SyncRoot {
				get {
					throw new NotImplementedException ();
				}
			}
		}

		[Serializable]
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView<,>))]		
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
		{
			readonly ICollection<TValue> collection;

			internal ValueCollection (ICollection<TValue> collection)
			{
				this.collection = collection;
			}

			public void CopyTo (TValue [] array, int arrayIndex)
			{
				collection.CopyTo (array, arrayIndex);
			}

			public IEnumerator<TValue> GetEnumerator ()
			{
				return collection.GetEnumerator ();
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
				return collection.Contains (item);
			}

			bool ICollection<TValue>.Remove (TValue item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			void ICollection.CopyTo (Array array, int index)
			{
				var target = array as TValue [];
				if (target != null) {
					CopyTo (target, index);
					return;
				}

				throw new NotImplementedException ();
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return collection.GetEnumerator ();
			}

			public int Count {
				get {
					return collection.Count;
				}
			}

			bool ICollection<TValue>.IsReadOnly {
				get {
					return true;
				}
			}

			bool ICollection.IsSynchronized {
				get {
					return false;
				}
			}

			object ICollection.SyncRoot {
				get {
					throw new NotImplementedException ();
				}
			}
		}
	}
}
#endif

