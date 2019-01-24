//
// ConditionalWeakTable.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//   Tautvydas Žilys <zilys@unity3d.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
// Copyright (C) 2016 Unity Technologies (https://unity3d.com)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Runtime.CompilerServices
{
	internal struct Ephemeron
	{
		internal object key;
		internal object value;
	}

	/*
	TODO:
		The runtime need to inform the table about how many entries were expired.   
		Compact the table when there are too many tombstones.
		Rehash to a smaller size when there are too few entries.
		Change rehash condition check to use non-fp code.
		Look into using quatratic probing/double hashing to reduce clustering problems.
		Make reads and non-expanding writes (add/remove) lock free.
	*/
	public sealed class ConditionalWeakTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
		where TKey : class
		where TValue : class
	{
		const int INITIAL_SIZE = 13;
		const float LOAD_FACTOR = 0.7f;
		const float COMPACT_FACTOR = 0.5f;
		const float EXPAND_FACTOR = 1.1f;

		Ephemeron[] data;
		object _lock = new object ();
		int size;

		public delegate TValue CreateValueCallback (TKey key);

		public ConditionalWeakTable ()
		{
			data = new Ephemeron [INITIAL_SIZE];
			GC.register_ephemeron_array (data);
		}

		~ConditionalWeakTable ()
		{
		}

		private void RehashWithoutResize ()
		{
			int len = data.Length;

			for (int i = 0; i < len; i++) {
				if (data [i].key == GC.EPHEMERON_TOMBSTONE)
					data [i].key = null;
			}

			for (int i = 0; i < len; i++) {
				object key = data [i].key;
				if (key != null) {
					int idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;

					while (true) {
						if (data [idx].key == null) {
							// The object was not stored in its normal slot. Rehash
							data [idx].key = key;
							data [idx].value = data [i].value;
							// At this point we have this Ephemeron entry duplicated in the array. Shouldn't
							// be a problem.
							data [i].key = null;
							data [i].value = null;
							break;
						} else if (data [idx].key == key) {
							/* We already have the key in the first available position, finished */
							break;
						}

						if (++idx == len) //Wrap around
							idx = 0;
					}
				}
			}
		}

		private void RecomputeSize ()
		{
			size = 0;
			for (int i = 0; i < data.Length; i++) {
				if (data [i].key != null)
					size++;
			}
		}

		/*LOCKING: _lock must be held*/
		private void Rehash ()
		{
			// Size doesn't track elements that die without being removed. Before attempting
			// to rehash we traverse the array to see how many entries are left alive. We
			// rehash the array into a new one which has a capacity relative to the number of
			// live entries.
			RecomputeSize ();

			uint newLength = (uint)HashHelpers.GetPrime (((int)(size / LOAD_FACTOR) << 1) | 1);

			if (newLength > data.Length * COMPACT_FACTOR && newLength < data.Length * EXPAND_FACTOR) {
				/* Avoid unnecessary LOS allocations */
				RehashWithoutResize ();
				return;
			}
			//Console.WriteLine ("--- resizing from {0} to {1}", data.Length, newLength);

			Ephemeron[] tmp = new Ephemeron [newLength];
			GC.register_ephemeron_array (tmp);
			size = 0;

			for (int i = 0; i < data.Length; ++i) {
				object key = data[i].key;
				object value = data[i].value;
				if (key == null || key == GC.EPHEMERON_TOMBSTONE)
					continue;

				int len = tmp.Length;
				int idx, initial_idx;
				int free_slot = -1;
	
				idx = initial_idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;
	
				do {
					object k = tmp [idx].key;
	
					//keys might be GC'd during Rehash
					if (k == null || k == GC.EPHEMERON_TOMBSTONE) {
						free_slot = idx;
						break;
					}
	
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
	
				tmp [free_slot].key = key;
				tmp [free_slot].value = value;
				++size;
			}
			data = tmp;
		}

		// the whole method is just a copy of `public void Add (TKey key, TValue value)`
		// the only difference it doesn't throw exceptions if the given key exists
		// both methods will be merged once a wierd issue (broken acceptence test dev10_535767.cs) is resolved
		public void AddOrUpdate (TKey key, TValue value)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				if (size >= data.Length * LOAD_FACTOR)
					Rehash ();

				int len = data.Length;
				int idx,initial_idx;
				int free_slot = -1;

				idx = initial_idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;
				do {
					object k = data [idx].key;

					if (k == null) {
						if (free_slot == -1)
							free_slot = idx;
						break;
					} else if (k == GC.EPHEMERON_TOMBSTONE && free_slot == -1) { //Add requires us to check for dupes :(
						free_slot = idx;
					} else if (k == key) {
						free_slot = idx; 
					}

					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);

				data [free_slot].key = key;
				data [free_slot].value = value;
				++size;
			}
		}

		public void Add (TKey key, TValue value)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				if (size >= data.Length * LOAD_FACTOR)
					Rehash ();

				int len = data.Length;
				int idx,initial_idx;
				int free_slot = -1;

				idx = initial_idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;
				do {
					object k = data [idx].key;

					if (k == null) {
						if (free_slot == -1)
							free_slot = idx;
						break;
					} else if (k == GC.EPHEMERON_TOMBSTONE && free_slot == -1) { //Add requires us to check for dupes :(
						free_slot = idx;
					} else if (k == key) {
						throw new ArgumentException ("Key already in the list", "key");
					}

					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);

				data [free_slot].key = key;
				data [free_slot].value = value;
				++size;
			}
		}

		public bool Remove (TKey key)
		{
			if (key == default (TKey))
				throw new ArgumentNullException ("Null key", "key");

			lock (_lock) {
				int len = data.Length;
				int idx, initial_idx;
				idx = initial_idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;
				do {
					object k = data[idx].key;
					if (k == key) {
						data [idx].key = GC.EPHEMERON_TOMBSTONE;
						data [idx].value = null;
						return true;
					}
					if (k == null)
						break;
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
			}
			return false;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("Null key", "key");

			value = default (TValue);
			lock (_lock) {
				int len = data.Length;
				int idx, initial_idx;
				idx = initial_idx = (RuntimeHelpers.GetHashCode (key) & int.MaxValue) % len;
				
				do {
					object k = data [idx].key;
					if (k == key) {
						value = (TValue)data [idx].value;
						return true;
					}
					if (k == null)
						break;
					if (++idx == len) //Wrap around
						idx = 0;
				} while (idx != initial_idx);
			}
			return false;
		}

		public TValue GetOrCreateValue (TKey key)
		{
			return GetValue (key, k => Activator.CreateInstance<TValue> ());
		}

		public TValue GetValue (TKey key, CreateValueCallback createValueCallback)
		{
			if (createValueCallback == null)
				throw new ArgumentNullException ("Null create delegate", "createValueCallback");

			TValue res;

			lock (_lock) {
				if (TryGetValue (key, out res))
					return res;
	
				res = createValueCallback (key);
				Add (key, res);
			}

			return res;
		}

		//--------------------------------------------------------------------------------------------
		// Find a key that equals (value equality) with the given key - don't use in perf critical path
		// Note that it calls out to Object.Equals which may calls the override version of Equals
		// and that may take locks and leads to deadlock
		// Currently it is only used by WinRT event code and you should only use this function
		// if you know for sure that either you won't run into dead locks or you need to live with the
		// possiblity
		//--------------------------------------------------------------------------------------------
#if !MONO
		[System.Security.SecuritySafeCritical]
		[FriendAccessAllowed]
#endif
		internal TKey FindEquivalentKeyUnsafe(TKey key, out TValue value)
		{
			lock (_lock)
			{
				for (int i = 0; i < data.Length; ++i)
				{
					var item = data[i];
					if (Object.Equals(item.key, key))
					{
						value = (TValue)item.value;
						return (TKey)item.key;
					}
				}
			}

			value = default(TValue);
			return null;
		}

		//--------------------------------------------------------------------------------------------
		// Clear all the key/value pairs
		//--------------------------------------------------------------------------------------------
		[System.Security.SecuritySafeCritical]
		public void Clear()
		{
			lock (_lock)
			{
				for (int i = 0; i < data.Length; i++)
				{
					data[i].key = null;
					data[i].value = null;
				}

				size = 0;
			}
		}

		// extracted from ../../../../external/referencesource/mscorlib/system/runtime/compilerservices/
		internal ICollection<TKey> Keys
		{
			[System.Security.SecuritySafeCritical]
			get
			{
				var tombstone = GC.EPHEMERON_TOMBSTONE;
				List<TKey> list = new List<TKey>(data.Length);
				lock (_lock)
				{
					for (int i = 0; i < data.Length; ++i)
					{
						TKey key = (TKey) data [i].key;
						if (key != null && key != tombstone)
							list.Add (key);
					}
				}
				return list;
			}
		}

		internal ICollection<TValue> Values
		{
			[System.Security.SecuritySafeCritical]
			get
			{
				var tombstone = GC.EPHEMERON_TOMBSTONE;
				List<TValue> list = new List<TValue>(data.Length);
				lock (_lock)
				{
					for (int i = 0; i < data.Length; ++i)
					{
						var item = data[i];
						if (item.key != null && item.key != tombstone)
							list.Add((TValue)item.value);
					}
				}

				return list;
			}
		}

		// IEnumerable implementation was copied from CoreCLR
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator ()
		{
			lock (_lock)
			{
				return size == 0 ?
					((IEnumerable<KeyValuePair<TKey, TValue>>)Array.Empty<KeyValuePair<TKey, TValue>>()).GetEnumerator() :
					new Enumerator(this);
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator ();
		
		/// <summary>Provides an enumerator for the table.</summary>
		private sealed class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			// The enumerator would ideally hold a reference to the Container and the end index within that
			// container.  However, the safety of the CWT depends on the only reference to the Container being
			// from the CWT itself; the Container then employs a two-phase finalization scheme, where the first
			// phase nulls out that parent CWT's reference, guaranteeing that the second time it's finalized there
			// can be no other existing references to it in use that would allow for concurrent usage of the
			// native handles with finalization.  We would break that if we allowed this Enumerator to hold a
			// reference to the Container.  Instead, the Enumerator holds a reference to the CWT rather than to
			// the Container, and it maintains the CWT._activeEnumeratorRefCount field to track whether there
			// are outstanding enumerators that have yet to be disposed/finalized.  If there aren't any, the CWT
			// behaves as it normally does.  If there are, certain operations are affected, in particular resizes.
			// Normally when the CWT is resized, it enumerates the contents of the table looking for indices that
			// contain entries which have been collected or removed, and it frees those up, effectively moving
			// down all subsequent entries in the container (not in the existing container, but in a replacement).
			// This, however, would cause the enumerator's understanding of indices to break.  So, as long as
			// there is any outstanding enumerator, no compaction is performed.

			private ConditionalWeakTable<TKey, TValue> _table; // parent table, set to null when disposed
			private int _currentIndex = -1;                    // the current index into the container
			private KeyValuePair<TKey, TValue> _current;       // the current entry set by MoveNext and returned from Current

			public Enumerator(ConditionalWeakTable<TKey, TValue> table)
			{
				Debug.Assert(table != null, "Must provide a valid table");
				Debug.Assert(Monitor.IsEntered(table._lock), "Must hold the _lock lock to construct the enumerator");

				// Store a reference to the parent table and increase its active enumerator count.
				_table = table;
				_currentIndex = -1;
			}

			~Enumerator() { Dispose(); }

			public void Dispose()
			{
				// Use an interlocked operation to ensure that only one thread can get access to
				// the _table for disposal and thus only decrement the ref count once.
				ConditionalWeakTable<TKey, TValue> table = Interlocked.Exchange(ref _table, null);
				if (table != null)
				{
					// Ensure we don't keep the last current alive unnecessarily
					_current = default;

					// Finalization is purely to decrement the ref count.  We can suppress it now.
					GC.SuppressFinalize(this);
				}
			}

			public bool MoveNext()
			{
				// Start by getting the current table.  If it's already been disposed, it will be null.
				ConditionalWeakTable<TKey, TValue> table = _table;
				if (table != null)
				{
					// Once have the table, we need to lock to synchronize with other operations on
					// the table, like adding.
					lock (table._lock)
					{
						var tombstone = GC.EPHEMERON_TOMBSTONE;
						while (_currentIndex < table.data.Length - 1)
						{
							_currentIndex++;
							var currentDataItem = table.data[_currentIndex];
							if (currentDataItem.key != null && currentDataItem.key != tombstone)
							{
								_current = new KeyValuePair<TKey, TValue>((TKey)currentDataItem.key, (TValue)currentDataItem.value);
								return true;
							}
						}
					}
				}

				// Nothing more to enumerate.
				return false;
			}

			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					if (_currentIndex < 0)
					{
						ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen();
					}
					return _current;
				}
			}

			object IEnumerator.Current => Current;

			public void Reset() { }
		}
	}
}
