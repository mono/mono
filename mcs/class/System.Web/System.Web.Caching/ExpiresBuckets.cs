// 
// System.Web.Caching
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Daniel Cazzulino [DHC] (dcazzulino@users.sf.net)
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
using System.Collections;
using System.Threading;

namespace System.Web.Caching {
	/// <summary>
	/// Responsible for holding a cache entry in the linked list bucket.
	/// </summary>
	internal struct ExpiresEntry {
		internal CacheEntry Entry;
		internal long TicksExpires;
		internal int _intNext;
	}

	/// <summary>
	/// Holds cache entries that has a expiration in a bucket list.
	/// </summary>
	internal class ExpiresBucket {
		private static int MIN_ENTRIES = 4;
		
		private byte _byteID;
		private int	_intSize;
		private int	_intCount;
		private	int	_intNext;

		private Cache _objManager;

		private ExpiresEntry [] _arrEntries;

		private System.Threading.ReaderWriterLock _lock = new System.Threading.ReaderWriterLock();

		/// <summary>
		/// Keeps a list of indexes in the list which are available to place new items. [DHC]
		/// </summary>
		private Int32Collection _freeidx = new Int32Collection();

		/// <summary>
		/// Constructs a new bucket.
		/// </summary>
		/// <param name="bucket">Current bucket ID.</param>
		/// <param name="objManager">Cache manager reponsible for the item(s) in the expires bucket.</param>
		internal ExpiresBucket (byte bucket, Cache objManager) {
			_objManager = objManager;
			Initialize(bucket);
		}

		/// <summary>
		/// Initializes the expires bucket, creates a linked list of MIN_ENTRIES.
		/// </summary>
		/// <param name="bucket">Bucket ID.</param>
		private void Initialize (byte bucket) {
			_byteID = bucket;
			_intNext = 0;
			_intCount = 0;

			_arrEntries = new ExpiresEntry [MIN_ENTRIES];
			_intSize = MIN_ENTRIES;

			int intPos = 0;
			do {
				_arrEntries[intPos]._intNext = intPos + 1;
				_arrEntries[intPos].TicksExpires = Cache.NoAbsoluteExpiration.Ticks;
				
				intPos++;
			} while (intPos < _intSize);

			_arrEntries[_intSize - 1]._intNext = -1;
		}

		/// <summary>
		/// Expands the bucket linked array list.
		/// </summary>
		private void Expand () {
			_lock.AcquireWriterLock(-1);
			try {
				int oldsize = _intSize;
				_intSize *= 2;

				// Copy items to the new list.
				ExpiresEntry[] newlist = new ExpiresEntry[_intSize];
				_arrEntries.CopyTo(newlist, 0);

				// Set last element to point to the next new empty element
				_intNext = oldsize;
				newlist[oldsize - 1]._intNext = oldsize;

				// Initialize positions for the rest of new elements.
				for (int i = oldsize; i < _intSize; i++) {
					newlist[i]._intNext = i + 1;
					newlist[i].TicksExpires = Cache.NoAbsoluteExpiration.Ticks;
				}

				// Last item signals the expansion of the list.
				newlist[_intSize - 1]._intNext = -1;

				// Replace the existing list.
				_arrEntries = newlist;
			}
			finally {
				_lock.ReleaseWriterLock();
			}

		}

		/// <summary>
		/// Adds a cache entry into the expires bucket.
		/// </summary>
		/// <param name="objEntry">Cache Entry object to be added.</param>
		internal void Add (CacheEntry objEntry) {
			_lock.AcquireWriterLock(-1);
			try {
				if (_intNext == -1) {
					if (_freeidx.Count == 0)
						Expand();
					else {
						_intNext = _freeidx[0];
						_freeidx.Remove(_intNext);
					}
				}
			
				_arrEntries[_intNext].TicksExpires = objEntry.Expires;
				_arrEntries[_intNext].Entry = objEntry;

				objEntry.ExpiresBucket = _byteID;
				objEntry.ExpiresIndex = _intNext;

				// If there are free indexes in the list, reuse them for the _next value.
				if (_freeidx.Count != 0) {
					_intNext = _freeidx [0];
					_freeidx.Remove(_intNext);
				} else
					_intNext = _arrEntries[_intNext]._intNext;

				_intCount++;
			}
			finally {
				_lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Removes a cache entry from the expires bucket.
		/// </summary>
		/// <param name="objEntry">Cache entry to be removed.</param>
		internal void Remove(CacheEntry objEntry) {
			// Check if this is our bucket
			if (objEntry.ExpiresBucket != _byteID) return;
			if (objEntry.ExpiresIndex == CacheEntry.NoIndexInBucket) return;

			_lock.AcquireWriterLock(-1);
			try {
				if (_arrEntries.Length < objEntry.ExpiresIndex) return;
				_intCount--;

				// Push the index as a free one.
				_freeidx.Add(objEntry.ExpiresIndex);

				_arrEntries[objEntry.ExpiresIndex].Entry = null;
				// Clear bucket-related values from the item.
				objEntry.ExpiresBucket = CacheEntry.NoBucketHash;
				objEntry.ExpiresIndex = CacheEntry.NoIndexInBucket;
			}
			finally {
				//Releases both reader & writer locks
				_lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Updates a cache entry in the expires bucket, this is called during a hit of an item if the 
		/// cache item has a sliding expiration. The function is responsible for updating the cache
		/// entry.
		/// </summary>
		/// <param name="objEntry">Cache entry to update.</param>
		/// <param name="ticksExpires">New expiration value for the cache entry.</param>
		internal void Update(CacheEntry objEntry, long ticksExpires) {
			// Check if this is our bucket
			if (objEntry.ExpiresBucket != _byteID) return;
			if (objEntry.ExpiresIndex == CacheEntry.NoIndexInBucket) return;

			_lock.AcquireWriterLock(-1);
			try {
				if (_arrEntries.Length < objEntry.ExpiresIndex) return;

				// Proceed to update.
				_arrEntries[objEntry.ExpiresIndex].TicksExpires = ticksExpires;
				_arrEntries[objEntry.ExpiresIndex].Entry.Expires = ticksExpires;
			}
			finally {
				//Releases both read & write locks
				_lock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Flushes all cache entries that has expired and removes them from the cache manager.
		/// </summary>
		internal void FlushExpiredItems() {
			ExpiresEntry objEntry;
			ArrayList removeList = null;
			ArrayList flushList = null;
			int	intPos;
			long ticksNow;
	
			ticksNow = DateTime.UtcNow.Ticks;

			intPos = 0;
			// Lookup all items that needs to be removed, this is done in a two part
			// operation to minimize the locking time.
			_lock.AcquireReaderLock (-1);
			try {
				do {
					objEntry = _arrEntries [intPos];
					if (null != objEntry.Entry && 
						((objEntry.TicksExpires < ticksNow) || objEntry.Entry.ExpiresBucket != _byteID))
					{
						if (null == removeList)
							removeList = new ArrayList ();
						
						removeList.Add (objEntry);
					}
						
					intPos++;
				} while (intPos < _intSize);
			}
			finally {
				_lock.ReleaseReaderLock ();
			}			

			if (null != removeList) {
				flushList = new ArrayList (removeList.Count);

				_lock.AcquireWriterLock (-1);	
				try {
					foreach (ExpiresEntry entry in removeList) { 
						ExpiresEntry e = entry;
						int id = entry.Entry.ExpiresIndex;

						//push the index for reuse
						_freeidx.Add (id);

						if (entry.Entry.ExpiresBucket == _byteID) {
							// add to our flush list
							flushList.Add (e.Entry);

							// Remove from bucket
							e.Entry.ExpiresBucket = CacheEntry.NoBucketHash;
							e.Entry.ExpiresIndex = CacheEntry.NoIndexInBucket;
						} 
						
						e.Entry = null;
						
						// Entries is structs, put it back
						_arrEntries [id] = e;
					}
				}
				finally {
					_lock.ReleaseWriterLock ();
				}

				// We can call this without locks, it can takes time due to callbacks to user code
				foreach (CacheEntry entry in flushList)
					_objManager.Remove (entry.Key, CacheItemRemovedReason.Expired);

				flushList = null;
				removeList = null;
			}
		}

		/// <summary>
		/// Returns the current size of the expires bucket.
		/// </summary>
		internal int Size {
			get { 
				return _intSize;
			}
		}

		/// <summary>
		/// Returns number of items in the bucket.
		/// </summary>
		internal int Count {
			get {
				return  _intCount;
			}
		}

		#region Private Int32Collection
		/* This file has been automatically generated by TextBox -- DO NOT EDIT! */
		/*
		Int32Collection
		Int32Collection.Enumerator

		These C# classes implement a strongly-typed collection of 
		Int32 objects.

		The internal representation is an array of Int32, so the performance 
		characteristics will be more like a vector than a list, to use STL terminology.

		The implementation is optimized for value-types, as it goes to great length to 
		avoid the overhead of boxing and unboxing.  But it should also work well for 
		reference types.

		Mad props to Nick Wienholt <sheyenne@bigpond.com> for assisting me in 
		this research, and the testing, the benchmarking, and of course, the 
		implementation!

		Last but not least, a quick shout out to Kit George, for his generous 
		contribution to the dotnet mailing list -- a code-generator for 
		CollectionBase-derived classes:
			http://discuss.develop.com/archives/wa.exe?A2=ind0107C&L=DOTNET&P=R35911
		This was the original inspiration for the fine code you are now enjoying.

		- Shawn Van Ness

		Other folks who've contributed:
			Ethan Smith <ethan.smith@pobox.com> (minor perf. improvements)
			Joel Mueller <jmueller@swiftk.com> (major perf. improvements)
			Chris Sells <csells@sellsbrothers.com> (generative programming guru)
			Patrice Lafond <plafond@hemisphere.bm> (a bug fix -- yikes!)
		*/

		/// <summary>
		/// An optimized collection for holding <see cref="Int32"/> values.
		/// </summary>
		[System.Serializable]
			private class Int32Collection : System.Collections.ICollection, System.Collections.IList, System.Collections.IEnumerable {
			#region Private vars & ctors
			private const int DefaultMinimumCapacity = 16;

			private System.Int32[] m_array = new System.Int32[DefaultMinimumCapacity];
			private int m_count = 0;
			private int m_version = 0;

			/// <summary />
			public Int32Collection() {
			}

			/// <summary />
			public Int32Collection(Int32Collection collection) {
				AddRange(collection); }

			/// <summary />
			public Int32Collection(System.Int32[] array) {
				AddRange(array); }
			#endregion

			#region Public members

			/// <summary />
			public int Count {
				get {
					return m_count; }
			}

			/// <summary />
			public void CopyTo(System.Int32[] array) {
				this.CopyTo(array, 0);
			}

			/// <summary />
			public void CopyTo(System.Int32[] array, int start) {
				if (m_count > array.GetUpperBound(0)+1-start)
					throw new System.ArgumentException("Destination array was not long enough.");

				// for (int i=0; i < m_count; ++i) array[start+i] = m_array[i];
				System.Array.Copy(m_array, 0, array, start, m_count); 
			}

			/// <summary />
			public System.Int32 this[int index] {
				get {
					ValidateIndex(index); // throws
					return m_array[index]; 
				}
				set {
					ValidateIndex(index); // throws
				
					++m_version; 
					m_array[index] = value; 
				}
			}

			/// <summary />
			public int Add(System.Int32 item) {			
				if (NeedsGrowth())
					Grow();

				++m_version;
				m_array[m_count] = item;
			
				return m_count++;
			}
		
			/// <summary />
			public void Clear() {
				++m_version;
				m_array = new System.Int32[DefaultMinimumCapacity];
				m_count = 0;
			}

			/// <summary />
			public bool Contains(System.Int32 item) {
				return ((IndexOf(item) == -1)?false:true);
			}

			/// <summary />
			public int IndexOf(System.Int32 item) {
				for (int i=0; i < m_count; ++i)
					if (m_array[i].Equals(item))
						return i;
				return -1;
			}

			/// <summary />
			public void Insert(int position, System.Int32 item) {
				ValidateIndex(position,true); // throws
			
				if (NeedsGrowth())
					Grow();

				++m_version;
				System.Array.Copy(m_array, position, m_array, position+1, m_count-position);

				m_array[position] = item;
				m_count++;
			}

			/// <summary />
			public void Remove(System.Int32 item) {			
				int index = IndexOf(item);
				if (index < 0)
					throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
				RemoveAt(index);
			}

			/// <summary />
			public void RemoveAt(int index) {
				ValidateIndex(index); // throws
			
				++m_version;
				m_count--;
				System.Array.Copy(m_array, index+1, m_array, index, m_count-index);

				if (NeedsTrimming())
					Trim();
			}

			// Public helpers (just to mimic some nice features of ArrayList)

			/// <summary />
			public int Capacity {
				get {
					return m_array.Length; }
				set {
					if (value < m_count) value = m_count;
					if (value < DefaultMinimumCapacity) value = DefaultMinimumCapacity;

					if (m_array.Length == value) return;

					++m_version;

					System.Int32[] temp = new System.Int32[value];
					System.Array.Copy(m_array, 0, temp, 0, m_count); 
					m_array = temp;
				}
			}

			/// <summary />
			public void AddRange(Int32Collection collection) {
				++m_version;

				Capacity += collection.Count;
				System.Array.Copy(collection.m_array, 0, this.m_array, m_count, collection.m_count);
				m_count += collection.Count;
			}

			/// <summary />
			public void AddRange(System.Int32[] array) {
				++m_version;

				Capacity += array.Length;
				System.Array.Copy(array, 0, this.m_array, m_count, array.Length);
				m_count += array.Length;
			}
			#endregion

			#region Private helper methods
			private void ValidateIndex(int index) {
				ValidateIndex(index,false);
			}

			private void ValidateIndex(int index, bool allowEqualEnd) {
				int max = (allowEqualEnd)?(m_count):(m_count-1);
				if (index < 0 || index > max)
					throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)index, "Specified argument was out of the range of valid values.");
			}

			private bool NeedsGrowth() {
				return (m_count >= Capacity);
			}

			private void Grow() {
				if (NeedsGrowth())
					Capacity = m_count*2;
			}

			private bool NeedsTrimming() {
				return (m_count <= Capacity/2);
			}

			private void Trim() {
				if (NeedsTrimming())
					Capacity = m_count;
			}
			#endregion

			#region System.Collections.ICollection implementation
			bool System.Collections.ICollection.IsSynchronized {
				get {
					return m_array.IsSynchronized; }
			}

			object System.Collections.ICollection.SyncRoot {
				get {
					return m_array.SyncRoot; }
			}

			void System.Collections.ICollection.CopyTo(System.Array array, int start) {
				this.CopyTo((System.Int32[])array, start);
			}
			#endregion

			#region System.Collections.IList implementation
			bool System.Collections.IList.IsFixedSize {
				get {
					return false; }
			}

			bool System.Collections.IList.IsReadOnly {
				get {
					return false; }
			}

			object System.Collections.IList.this[int index] {
				get { return (object)this[index]; }
				set { this[index] = (System.Int32)value; }
			}

			int System.Collections.IList.Add(object item) {
				return this.Add((System.Int32)item);
			}

			bool System.Collections.IList.Contains(object item) {
				return this.Contains((System.Int32)item);
			}

			int System.Collections.IList.IndexOf(object item) {
				return this.IndexOf((System.Int32)item);
			}

			void System.Collections.IList.Insert(int position, object item) {
				this.Insert(position, (System.Int32)item);
			}

			void System.Collections.IList.Remove(object item) {
				this.Remove((System.Int32)item);
			}
			#endregion

			#region System.Collections.IEnumerable and enumerator implementation
			/// <summary />
			public Enumerator GetEnumerator() {
				return new Enumerator(this);
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			// Nested enumerator class
			/// <summary />
			public class Enumerator : System.Collections.IEnumerator {
				private Int32Collection m_collection;
				private int m_index;
				private int m_version;

				// Construction
	
				/// <summary />
				public Enumerator(Int32Collection tc) {
					m_collection = tc;
					m_index = -1;
					m_version = tc.m_version;
				}
	
				/// <summary />
				public System.Int32 Current {
					get {
						return m_collection[m_index]; }
				}
	
				/// <summary />
				public bool MoveNext() {
					if (m_version != m_collection.m_version)
						throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

					++m_index;
					return (m_index < m_collection.Count)?true:false;
				}
	
				/// <summary />
				public void Reset() {
					if (m_version != m_collection.m_version)
						throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

					m_index = -1;
				}

				object System.Collections.IEnumerator.Current {
					get {
						return (object)(this.Current); }
				}
			}
			#endregion
		}
		#endregion
	}
}
	