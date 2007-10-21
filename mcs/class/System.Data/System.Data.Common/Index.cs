//
// System.Data.Common.Key.cs
//
// Author:
//   Boris Kirzner  <borisk@mainsoft.com>
//   Konstantin Triger (kostat@mainsoft.com)
//

/*
  * Copyright (c) 2002-2004 Mainsoft Corporation.
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  *
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  *
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */

using System;
using System.Collections;

using System.Text;

namespace System.Data.Common
{
	enum IndexDuplicatesState
	{
		Unknown,
		True,
		False
	}

	/// <summary>
	/// Summary description for Index.
	/// </summary>
	internal class Index
	{
		#region Fields

		int [] _array;
		int _size;
		Key _key;
		int _refCount;
		IndexDuplicatesState _hasDuplicates;

		#endregion // Fields

		#region Constructors

		internal Index (Key key)
		{
			_key = key;
			Reset();
		}

		#endregion // Constructors

		#region Properties

		internal Key Key {
			get { return _key; }
		}

		internal int Size {
			get {
				EnsureArray();
				return _size;
			}
		}

		internal int RefCount {
			get { return _refCount; }
		}

		internal int IndexToRecord (int index)
		{
			return index < 0 ? index : Array[index];
		}

		private int [] Array {
			get {
				EnsureArray ();
				return _array;
			}
		}

		internal bool HasDuplicates {
			get {
				if (_array == null || _hasDuplicates == IndexDuplicatesState.Unknown) {
					EnsureArray ();
					if (_hasDuplicates == IndexDuplicatesState.Unknown) {
						// check for duplicates
						_hasDuplicates = IndexDuplicatesState.False;
						for (int i = 0; i < Size - 1; i++) {
							if (Key.CompareRecords (Array [i], Array [i + 1]) == 0) {
								_hasDuplicates = IndexDuplicatesState.True;
								break;
							}
						}
					}
				}
				return (_hasDuplicates == IndexDuplicatesState.True);
			}
		}

		#endregion // Properties

		#region Methods

		internal int [] Duplicates {
			get {
				if (!HasDuplicates)
					return null;

				ArrayList dups = new ArrayList();

				bool inRange = false;
				for (int i = 0; i < Size - 1; i++) {
					if (Key.CompareRecords (Array [i], Array [i + 1]) == 0) {
						if (!inRange) {
							dups.Add(Array[i]);
							inRange = true;
						}

						dups.Add (Array [i + 1]);
					}
					else
						inRange = false;
				}

				return (int []) dups.ToArray (typeof (int));
			}
		}

		private void EnsureArray ()
		{
			if (_array == null)
				RebuildIndex ();
		}

		internal int [] GetAll ()
		{
			return Array;
		}

		internal DataRow [] GetAllRows ()
		{
			DataRow [] list = new DataRow [Size];
			for (int i = 0; i < Size; ++i)
				list [i] = Key.Table.RecordCache [Array [i]];
			return list;
		}

		internal DataRow [] GetDistinctRows () 
		{
			ArrayList list = new ArrayList ();
			list.Add (Key.Table.RecordCache [Array [0]]);
			int currRecord = Array [0];
			for (int i = 1; i <  Size; ++i) {
				if (Key.CompareRecords (currRecord, Array [i]) == 0)
					continue;
				list.Add (Key.Table.RecordCache [Array [i]]);
				currRecord = Array [i];
			}
			return (DataRow []) list.ToArray (typeof (DataRow));
		}

		internal void Reset()
		{
			_array = null;
			RebuildIndex ();
		}

		private void RebuildIndex()
		{
			// consider better capacity approximation
			_array = new int [Key.Table.RecordCache.CurrentCapacity];
			_size = 0;
			foreach (DataRow row in Key.Table.Rows) {
				int record = Key.GetRecord (row);
				if (record != -1)
					_array [_size++] = record;
			}
			_hasDuplicates = IndexDuplicatesState.False;
			// Note : MergeSort may update hasDuplicates to True
			Sort ();
		}

		private void Sort ()
		{
			//QuickSort(Array,0,Size-1);
			MergeSort (Array, Size);
		}
		
		/*
		 * Returns record number of the record equal to the key values supplied 
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int Find (object [] keys)
		{
			int index = FindIndex(keys);
			return IndexToRecord (index);
		}

		/*
		 * Returns record index (location) of the record equal to the key values supplied 
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int FindIndex (object [] keys)
		{
			if (keys == null || keys.Length != Key.Columns.Length)
				throw new ArgumentException("Expecting " + Key.Columns.Length + " value(s) for the key being indexed, " +
					"but received " + ((keys == null) ? 0 : keys.Length) + " value(s).");

			int tmp = Key.Table.RecordCache.NewRecord ();
			try {
				// init key values for temporal record
				for (int i = 0; i < Key.Columns.Length; i++)
					Key.Columns [i].DataContainer [tmp] = keys [i];
				return FindIndex (tmp);
			} finally {
				Key.Table.RecordCache.DisposeRecord (tmp);
			}
		}

		/*
		 * Returns record number of the record equal to the record supplied 
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int Find (int record)
		{
			int index = FindIndex (record);
			return IndexToRecord (index);
		}

		/*
		 * Returns array of record numbers of the records equal equal to the key values supplied 
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int[] FindAll (object [] keys)
		{
			int [] indexes = FindAllIndexes (keys);
			IndexesToRecords (indexes);
			return indexes;
		}

		/*
		 * Returns array of indexes of the records inside the index equal equal to the key values supplied 
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int [] FindAllIndexes (object [] keys)
		{
			if (keys == null || keys.Length != Key.Columns.Length)
				throw new ArgumentException("Expecting " + Key.Columns.Length + " value(s) for the key being indexed," +
					"but received " + ((keys == null) ? 0 : keys.Length) + " value(s).");

			int tmp = Key.Table.RecordCache.NewRecord ();
			try {
				// init key values for temporal record
				for (int i = 0; i < Key.Columns.Length; i++)
					Key.Columns [i].DataContainer [tmp] = keys [i];
				return FindAllIndexes (tmp);
			} catch(FormatException) {
				return new int [0];
			} catch(InvalidCastException) {
				return new int [0];
			} finally {
				Key.Table.RecordCache.DisposeRecord (tmp);
			}
		}

		/*
		 * Returns array of record numbers of the records equal to the record supplied 
		 * in the meaning of index key, or empty list if no equal records found.
		 */
		internal int [] FindAll (int record)
		{
			int [] indexes = FindAllIndexes (record);
			IndexesToRecords (indexes);
			return indexes;
		}

		/*
		 * Returns array of indexes of the records inside the index that equal to the record supplied 
		 * in the meaning of index key, or empty list if no equal records found.
		 */
		internal int [] FindAllIndexes (int record)
		{
			int index = FindIndex(record);
			if (index == -1)
				return new int[0];

			int startIndex = index++;
			int endIndex = index;

			for (; startIndex >= 0 && Key.CompareRecords (Array [startIndex], record) == 0; startIndex--) {
			}
			for (; endIndex < Size && Key.CompareRecords (Array [endIndex], record) == 0; endIndex++) {
			}
			
			int length = endIndex - startIndex - 1;
			int [] indexes = new int [length];
			
			for (int i = 0; i < length; i++)
				indexes [i] = ++startIndex;
		
			return indexes;
		}

		/*
		 * Returns index inside the array where record number of the record equal to the record supplied 
		 * in the meaning of index key is sored, or -1 if no equal record found.
		 */
		private int FindIndex (int record)
		{
			if (Size == 0)
				return -1;
			return BinarySearch (Array, 0, Size - 1, record);
		}

		/*
		 * Finds exact location of the record specified
		 */ 
		private int FindIndexExact (int record)
		{
			for (int i = 0, size = Size; i < size; i++)
				if (Array [i] == record)
					return i;
			return -1;
		}

		/*
		 * Returns array of records from the indexes (locations) inside the index
		 */
		private void IndexesToRecords (int [] indexes)
		{
			for (int i = 0; i < indexes.Length; i++)
				indexes [i] = Array [indexes [i]];
		}

		internal void Delete (DataRow row)
		{
			int oldRecord = Key.GetRecord (row);
			Delete (oldRecord);
		}

		internal void Delete (int oldRecord)
		{
			if (oldRecord == -1)
				return;

			int index = FindIndexExact (oldRecord);
			if (index != -1) {
				if (_hasDuplicates == IndexDuplicatesState.True) {
					int c1 = 1;
					int c2 = 1;

					if (index > 0)
						c1 = Key.CompareRecords (Array [index - 1], oldRecord);
					if (index < Size - 1)
						c2 = Key.CompareRecords (Array [index + 1], oldRecord);

					if (c1 == 0 ^ c2 == 0)
						_hasDuplicates = IndexDuplicatesState.Unknown;
				}
				Remove(index);
			}
		}

		private void Remove (int index)
		{
			if (Size > 1)
				System.Array.Copy (Array, index + 1, Array, index,Size - index - 1);
			_size--;
		}

		internal void Update (DataRow row, int oldRecord, DataRowVersion oldVersion, DataRowState oldState)
		{
			bool contains = Key.ContainsVersion (oldState, oldVersion);
			int newRecord = Key.GetRecord (row);
			// the record did not appeared in the index before update
			if (oldRecord == -1 || Size == 0 || !contains) {
				if (newRecord >= 0)
					if (FindIndexExact (newRecord) < 0)
						Add (row,newRecord);
				return;
			}
			
			// the record will not appeare in the index after update
			if (newRecord < 0 || !Key.CanContain (newRecord)) {
				Delete (oldRecord);
				return;
			}

			int oldIdx = FindIndexExact (oldRecord);
			if (oldIdx == -1) {
				Add (row, newRecord);
				return;
			}

			int newIdx = -1;
			int compare = Key.CompareRecords (Array [oldIdx], newRecord);
			int start, end;

			int c1 = 1;
			int c2 = 1;

			if (compare == 0) {
				if (Array [oldIdx] == newRecord) {
					// we deal with the same record that didn't change
					// in the context of current index.
					// so , do nothing.
					return;
				}
			} else {
				if (_hasDuplicates == IndexDuplicatesState.True) {
					if (oldIdx > 0)
						c1 = Key.CompareRecords (Array [oldIdx - 1], newRecord);
					if (oldIdx < Size - 1)
						c2 = Key.CompareRecords (Array [oldIdx + 1], newRecord);

					if ((c1 == 0 ^ c2 == 0) && compare != 0)
						_hasDuplicates = IndexDuplicatesState.Unknown;
				}
			}
			
			if ((oldIdx == 0 && compare > 0) || (oldIdx == (Size - 1) && compare < 0) || (compare == 0)) {
				// no need to switch cells
				newIdx = oldIdx;
			} else {
				if (compare < 0) {
					// search after the old place
					start = oldIdx + 1;
					end = Size - 1;
				} else {
					// search before the old palce
					start = 0;
					end = oldIdx - 1;
				}

				newIdx = LazyBinarySearch (Array, start, end, newRecord);

				if (oldIdx < newIdx) {
					System.Array.Copy (Array, oldIdx + 1, Array, oldIdx, newIdx - oldIdx);
					if (Key.CompareRecords (Array [newIdx], newRecord) > 0)
						--newIdx;
				} else if (oldIdx > newIdx){
					System.Array.Copy (Array, newIdx, Array, newIdx + 1, oldIdx - newIdx);
					if (Key.CompareRecords (Array [newIdx], newRecord) < 0)
						++newIdx;
				}
			}
			Array[newIdx] = newRecord;

			if (compare != 0) {
				if (!(_hasDuplicates == IndexDuplicatesState.True)) {
					if (newIdx > 0)
						c1 = Key.CompareRecords (Array [newIdx - 1], newRecord);
					if (newIdx < Size - 1)
						c2 = Key.CompareRecords (Array [newIdx + 1], newRecord);

					if (c1 == 0 || c2 == 0)
						_hasDuplicates = IndexDuplicatesState.True;
				}
			}
		}

		internal void Add (DataRow row)
		{
			Add(row, Key.GetRecord (row));
		}

		private void Add (DataRow row,int newRecord)
		{
			int newIdx;

			if (newRecord < 0 || !Key.CanContain (newRecord))
				return;

			if (Size == 0) {
				newIdx = 0;
			} else {
				newIdx = LazyBinarySearch (Array, 0, Size - 1, newRecord);
				// if newl value is greater - insert afer old value
				// else - insert before old value
				if (Key.CompareRecords (Array [newIdx], newRecord) < 0)
					newIdx++;
			}

			Insert (newIdx, newRecord);

			int c1 = 1;
			int c2 = 1;
			if (!(_hasDuplicates == IndexDuplicatesState.True)) {
				if (newIdx > 0)
					c1 = Key.CompareRecords (Array [newIdx - 1], newRecord);
				if (newIdx < Size - 1)
					c2 = Key.CompareRecords (Array [newIdx + 1], newRecord);

				if (c1 == 0 || c2 == 0)
					_hasDuplicates = IndexDuplicatesState.True;
			}
		}

		private void Insert (int index,int r)
		{
			if (Array.Length == Size) {
				int [] tmp = (Size == 0) ? new int[16] : new int[Size << 1];
				System.Array.Copy (Array, 0, tmp, 0, index);
				tmp [index] = r;
				System.Array.Copy (Array, index, tmp, index + 1, Size - index);
				_array = tmp;
			} else {
				System.Array.Copy (Array, index, Array, index + 1, Size - index);
				Array [index] = r;
			}
			_size++;
		}

		private void MergeSort (int [] to, int length)
		{
			int [] from = new int [length];
			System.Array.Copy (to, 0, from, 0, from.Length);
			MergeSort (from, to, 0, from.Length);
		}

		private void MergeSort(int[] from, int[] to,int p, int r)
		{
			int q = (p + r) >> 1;
			if (q == p)
				return;

			MergeSort (to, from, p, q);
			MergeSort (to, from, q, r);

			// merge
			for (int middle = q, current = p;;) {
				int res = Key.CompareRecords (from[p], from[q]);
				if (res > 0) {
					to [current++] = from [q++];

					if (q == r) {
						while (p < middle)
							to[current++] = from[p++];
						break;
					}
				} else {
					if (res == 0)
						_hasDuplicates = IndexDuplicatesState.True;

					to [current++] = from [p++];

					if (p == middle) {
						while (q < r)
							to[current++] = from[q++];
						break;
					}
				}
			}
		}

		private void QuickSort (int [] a,int p,int r)
		{
			if (p < r) {
				int q = Partition (a, p, r);
				QuickSort (a, p, q);
				QuickSort (a, q + 1, r);
			}
		}

		private int Partition (int [] a,int p,int r)
		{
			int x = a [p];
			int i = p - 1;
			int j = r + 1;

			while (true) {
				// decrement upper limit while values are greater then border value
				do {
					j--;
				} while (Key.CompareRecords (a [j], x) > 0);

				do {
					i++;
				} while (Key.CompareRecords (a [i], x) < 0);
				
				if (i < j) {
					int tmp = a [j];
					a [j] = a [i];
					a [i] = tmp;
				} else {
					return j;
				}
			}
		}

		private int BinarySearch (int [] a, int p, int r,int b)
		{
			int i = LazyBinarySearch (a, p, r, b);
			return (Key.CompareRecords (a [i], b) == 0) ? i : -1;
		}

		// Lazy binary search only returns the cell number the search finished in,
		// but does not checks that the correct value was actually found
		private int LazyBinarySearch (int [] a, int p, int r, int b)
		{
			if (p == r)
				return p;

			int q = (p + r) >> 1;

			int compare = Key.CompareRecords (a [q], b);
			if (compare < 0)
				return LazyBinarySearch (a, q + 1, r, b);
			else if (compare > 0)
				return LazyBinarySearch (a, p, q, b);
			else
				return q;
		}

		internal void AddRef ()
		{
			_refCount++;
		}

		internal void RemoveRef ()
		{
			_refCount--;
		}

		/*
		// Prints indexes. For debugging.
		internal void Print ()
		{
			for (int i=0; i < Size; i++) {
				Console.Write ("Index {0} record {1}: ", i, Array [i]);
				for (int j=0; j < Key.Table.Columns.Count; j++) {
					DataColumn col = Key.Table.Columns [j];
					if (Array [i] >= 0)
						Console.Write ("{0,15} ", col [Array [i]]);
				}
				Console.WriteLine ();
			}
		}
		*/
		
		#endregion // Methods
	}
}
